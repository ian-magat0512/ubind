// <copyright file="CustomerReadModelRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.ReadModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Dapper;
    using LinqKit;
    using MoreLinq;
    using UBind.Domain;
    using UBind.Domain.Entities;
    using UBind.Domain.Enums;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Claim;
    using UBind.Domain.ReadModel.Customer;
    using UBind.Domain.ReadModel.Person.Fields;
    using UBind.Domain.ReadModel.Policy;
    using UBind.Domain.ReadWriteModel;
    using UBind.Domain.Repositories;
    using UBind.Persistence.Extensions;

    /// <inheritdoc />
    public class CustomerReadModelRepository : ICustomerReadModelRepository
    {
        private readonly IUBindDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerReadModelRepository"/> class.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        public CustomerReadModelRepository(
            IUBindDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        /// <inheritdoc />
        public CustomerReadModelDetail GetCustomerById(Guid tenantId, Guid id, bool includeTestData = true)
        {
            return this.Filter(this.QueryReadModelDetail(tenantId, id), new EntityListFilters() { IncludeTestData = includeTestData }).FirstOrDefault();
        }

        /// <inheritdoc />
        public CustomerReadModel GetCustomerReadModelById(Guid tenantId, Guid id)
        {
            return this.QueryReadModel(tenantId, id).FirstOrDefault();
        }

        /// <inheritdoc />
        public IEnumerable<CustomerReadModel> GetCustomersMatchingFilter(Guid tenantId, EntityListFilters filters)
        {
            var customerQuery = this.Filter(this.QueryReadModel(tenantId, null), filters).AsEnumerable();

            if (!string.IsNullOrEmpty(filters.SortBy))
            {
                customerQuery = customerQuery.AsQueryable().Order(filters.SortBy, filters.SortOrder);
            }

            return customerQuery.AsQueryable().Paginate(filters);
        }

        /// <inheritdoc />
        public ICustomerReadModelSummary GetCustomerSummary(Guid tenantId, Guid id)
        {
            var customerQuery = this.QueryReadModel(tenantId, id);
            return customerQuery
                .Where(c => c.TenantId == tenantId && c.Id == id)
                .Select(c => new CustomerReadModelDetail
                {
                    Id = c.Id,
                    PrimaryPersonId = c.PrimaryPersonId,
                    People = c.People,
                    Environment = c.Environment,
                    IsTestData = c.IsTestData,
                    OwnerUserId = c.OwnerUserId,
                    OwnerPersonId = c.OwnerPersonId,
                    OwnerFullName = c.OwnerFullName,
                    IsDeleted = c.IsDeleted,
                    CreatedTicksSinceEpoch = c.CreatedTicksSinceEpoch,
                    LastModifiedTicksSinceEpoch = c.LastModifiedTicksSinceEpoch,
                    TenantId = c.TenantId,
                    OrganisationId = c.OrganisationId,
                    PortalId = c.PortalId,
                }).FirstOrDefault();
        }

        /// <inheritdoc/>
        public IEnumerable<ICustomerReadModelSummary> GetCustomersSummaryMatchingFilters(
            Guid tenantId, EntityListFilters filters)
        {
            var customerQuery = this.QueryReadModelDetail(tenantId, null);
            var filteredCustomers = this.Filter(customerQuery, filters).ToList();

            var result = filteredCustomers.AsQueryable();

            if (!string.IsNullOrEmpty(filters.SortBy))
            {
                result = result.Order(filters.SortBy, filters.SortOrder);
            }

            return result.Paginate(filters).AsEnumerable();
        }

        /// <inheritdoc/>
        public IEnumerable<ICustomerReadModelSummary> GetActiveCustomerUsersSummaryMatchingFilters(Guid tenantId, EntityListFilters filters)
        {
            return this.GetCustomersSummaryMatchingFilters(tenantId, filters)
                .Where(customer => customer.UserId != null && customer.UserHasBeenActivated)
                .AsEnumerable();
        }

        /// <inheritdoc/>
        public IEnumerable<ICustomerReadModelWithRelatedEntities> GetCustomersWithRelatedEntities(
            Guid tenantId, EntityListFilters filters, IEnumerable<string> relatedEntities)
        {
            var customers = this.Query(tenantId, filters)
                .Paginate(filters);

            var query = this.CreateQueryForCustomerDetailsWithRelatedEntities(customers, relatedEntities);
            return query.ToList();
        }

        /// <inheritdoc/>
        public IDictionary<Guid, Guid> GetAllExistingCustomersPersonIds(Guid tenantId)
        {
            var customersPersonIds = this.dbContext.CustomerReadModels
                .Where(c => c.TenantId == tenantId
                    && !this.dbContext.PersonReadModels.Any(p => c.PrimaryPersonId == p.Id))
                .ToList()
                .ToDictionary(d => d.PrimaryPersonId, d => d.Id);
            return customersPersonIds;
        }

        /// <inheritdoc/>
        public ICustomerReadModelWithRelatedEntities GetCustomerWithRelatedEntities(
            Guid tenantId, DeploymentEnvironment? environment, Guid id, IEnumerable<string> relatedEntities)
        {
            var query = this.CreateQueryForCustomerDetailsWithRelatedEntities(tenantId, environment, relatedEntities);
            return query.FirstOrDefault(q => q.Customer.Id == id);
        }

        /// <inheritdoc/>
        public ICustomerReadModelWithRelatedEntities GetCustomerWithRelatedEntities(
            Guid tenantId,
            DeploymentEnvironment? environment,
            Guid organisationId,
            string email,
            IEnumerable<string> relatedEntities)
        {
            var customerQuery = this.CreateQueryForCustomerDetailsWithRelatedEntities(
                tenantId, environment, relatedEntities);
            return customerQuery
                .Where(q => q.Customer.PrimaryPerson.Email == email)
                .Where(q => q.Customer.OrganisationId == organisationId)
                .FirstOrDefault();
        }

        /// <inheritdoc/>
        public List<Guid> GetIdsForAllActiveCustomersByEntityFilters(EntityFilters entityFilters)
        {
            var entityListFilters = new EntityListFilters()
            {
                OrganisationIds = entityFilters.OrganisationId.HasValue
                    ? new Guid[] { entityFilters.OrganisationId.Value }
                    : null,
                SortBy = nameof(CustomerReadModelDetail.CreatedTicksSinceEpoch),
                SortOrder = SortDirection.Descending,
            };
            var filteredQuery = this.Filter(this.QueryReadModelDetail(entityFilters.TenantId, null), entityListFilters);

            if (entityFilters.Skip.HasValue && entityFilters.PageSize.HasValue)
            {
                filteredQuery = filteredQuery
                    .Skip(entityFilters.Skip.Value)
                    .Take(entityFilters.PageSize.Value);
            }

            return filteredQuery.Select(c => c.Id).ToList();
        }

        public List<CustomerReadModel> GetDeletedCustomersWithUser()
        {
            var customerWithUser = from customer in this.dbContext.CustomerReadModels
                                   join person in this.dbContext.PersonReadModels on customer.PrimaryPersonId equals person.Id
                                   join userLogin in this.dbContext.UserLoginEmails on person.UserId equals userLogin.Id
                                   where customer.IsDeleted && person.UserId != null
                                   select customer;
            return customerWithUser.ToList();
        }

        public Guid? GetCustomerOwnerId(Guid tenantId, Guid customerId)
        {
            string sql = $@"SELECT OwnerUserId 
                            FROM CustomerReadModels (nolock)
                            WHERE Id = @CustomerId AND TenantId = @TenantId";
            var parameters = new
            {
                CustomerId = customerId,
                TenantId = tenantId,
            };
            return this.dbContext.Database.Connection.QueryFirstOrDefault<Guid?>(sql, parameters);
        }

        /// <inheritdoc/>
        public IQueryable<CustomerReadModelWithRelatedEntities> CreateQueryForCustomerDetailsWithRelatedEntities(
            Guid tenantId, DeploymentEnvironment? environment, IEnumerable<string> relatedEntities)
        {
            var customerDatasource = this.dbContext.CustomerReadModels
                .Where(x => x.TenantId == tenantId);
            if (environment.HasValue)
            {
                customerDatasource = customerDatasource.Where(x => x.Environment == environment);
            }

            return this.CreateQueryForCustomerDetailsWithRelatedEntities(
                customerDatasource, relatedEntities);
        }

        public IQueryable<CustomerReadModel> GetCustomersForOwnerUserIdTenantIdAndOrganisationId(
            Guid tenantId, Guid organisationId, Guid ownerUserId)
        {
            return this.GetCustomersForTenantIdAndOrganisationId(tenantId, organisationId)
                .Where(c => c.OwnerUserId == ownerUserId);
        }

        public IQueryable<CustomerReadModel> GetCustomersForTenantIdAndOrganisationId(
            Guid tenantId, Guid organisationId)
        {
            return this.dbContext.CustomerReadModels
                .Where(c => c.TenantId == tenantId)
                .Where(c => c.OrganisationId == organisationId)
                .Where(c => !c.IsDeleted);
        }

        private IQueryable<CustomerReadModel> Query(
            Guid tenantId, EntityListFilters filters)
        {
            IQueryable<CustomerReadModel> query = this.dbContext.CustomerReadModels
                .Where(c => c.TenantId == tenantId)
                .Where(c => !c.IsDeleted);

            return this.ApplyFilters(query, filters);
        }

        private IQueryable<CustomerReadModel> ApplyFilters(IQueryable<CustomerReadModel> query, EntityListFilters filters)
        {
            if (filters.Environment != null)
            {
                query = query.Where(c => c.Environment == filters.Environment);
            }

            bool hasOrganisationFilter = filters.OrganisationIds != null && filters.OrganisationIds.Any();
            if (hasOrganisationFilter)
            {
                query = query.Where(c => filters.OrganisationIds.Contains(c.OrganisationId));
            }

            if (filters.OwnerUserId != null)
            {
                query = query.Where(customer => customer.OwnerUserId == filters.OwnerUserId);
            }

            if (filters.SearchTerms.Any())
            {
                var searchExpression = PredicateBuilder.New<CustomerReadModel>(false);
                foreach (var customerName in filters.SearchTerms)
                {
                    searchExpression.Or(p => p.PrimaryPerson.FullName.Contains(customerName)
                        || p.PrimaryPerson.PreferredName.Contains(customerName));
                }

                query = query.Where(searchExpression);
            }

            if (filters.DateIsAfterTicks != null && filters.DateFilteringPropertyName != null)
            {
                query = query.Where(ExpressionHelper.GreaterThanExpression<CustomerReadModel>(filters.DateFilteringPropertyName, filters.DateIsAfterTicks));
            }

            if (filters.DateIsBeforeTicks != null && filters.DateFilteringPropertyName != null)
            {
                query = query.Where(ExpressionHelper.LessThanExpression<CustomerReadModel>(filters.DateFilteringPropertyName, filters.DateIsBeforeTicks));
            }

            if (filters.Statuses.Any())
            {
                var statusPredicate = PredicateBuilder.New<CustomerReadModel>(false);
                foreach (var status in filters.Statuses)
                {
                    var userStatus =
                        status.EqualsIgnoreCase("disabled") ? UserStatus.Deactivated : status.ToEnumOrThrow<UserStatus>();
                    statusPredicate = statusPredicate.Or(this.GetStatusExpression(userStatus));
                }

                query = query.Where(statusPredicate);
            }

            if (!filters.IncludeTestData)
            {
                query = query.Where(c => c.IsTestData == false);
            }

            if (!string.IsNullOrEmpty(filters.SortBy))
            {
                query = query.Order(filters.SortBy, filters.SortOrder);
            }

            return query;
        }

        private IQueryable<CustomerReadModelWithRelatedEntities> CreateQueryForCustomerDetailsWithRelatedEntities(
            IQueryable<CustomerReadModel> dataSource, IEnumerable<string> relatedEntities)
        {
            var query = from customer in dataSource
                        select new CustomerReadModelWithRelatedEntities
                        {
                            Customer = customer,
                            Tenant = default,
                            TenantDetails = new TenantDetails[] { },
                            Organisation = default,
                            Owner = default,
                            QuoteDocuments = new QuoteDocumentReadModel[] { },
                            ClaimDocuments = new ClaimAttachmentReadModel[] { },
                            Emails = new Domain.ReadWriteModel.Email.Email[] { },
                            Quotes = new NewQuoteReadModel[] { },
                            Claims = new ClaimReadModel[] { },
                            PolicyTransactions = new PolicyTransaction[] { },
                            Policies = new PolicyReadModel[] { },
                            PrimaryPerson = default,
                            PersonStreetAddresses = new StreetAddressReadModel[] { },
                            PersonEmailAddresses = new EmailAddressReadModel[] { },
                            PersonPhoneNumbers = new PhoneNumberReadModel[] { },
                            PersonMessengerIds = new MessengerIdReadModel[] { },
                            PersonSocialMediaIds = new SocialMediaIdReadModel[] { },
                            PersonWebsiteAddreses = new WebsiteAddressReadModel[] { },
                            People = new PersonReadModel[] { },
                            Sms = new Sms[] { },
                            FromRelationships = new Relationship[] { },
                            ToRelationships = new Relationship[] { },
                            Portal = default,
                            TextAdditionalPropertiesValues = new TextAdditionalPropertyValueReadModel[] { },
                            StructuredDataAdditionalPropertyValues = new StructuredDataAdditionalPropertyValueReadModel[] { },
                            SavedPaymentMethods = new SavedPaymentMethod[] { },
                        };

            if (relatedEntities.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.Customer.Policies))))
            {
                query = query.GroupJoin(
                    this.dbContext.Policies,
                    c => c.Customer.Id,
                    p => p.CustomerId,
                    (c, policies) => new CustomerReadModelWithRelatedEntities
                    {
                        Customer = c.Customer,
                        Tenant = c.Tenant,
                        TenantDetails = c.TenantDetails,
                        Organisation = c.Organisation,
                        Owner = c.Owner,
                        QuoteDocuments = c.QuoteDocuments,
                        ClaimDocuments = c.ClaimDocuments,
                        Emails = c.Emails,
                        Quotes = c.Quotes,
                        Claims = c.Claims,
                        PolicyTransactions = c.PolicyTransactions,
                        Policies = policies,
                        PrimaryPerson = c.PrimaryPerson,
                        PersonStreetAddresses = c.PersonStreetAddresses,
                        PersonEmailAddresses = c.PersonEmailAddresses,
                        PersonPhoneNumbers = c.PersonPhoneNumbers,
                        PersonMessengerIds = c.PersonMessengerIds,
                        PersonSocialMediaIds = c.PersonSocialMediaIds,
                        PersonWebsiteAddreses = c.PersonWebsiteAddreses,
                        People = c.People,
                        Sms = c.Sms,
                        FromRelationships = c.FromRelationships,
                        ToRelationships = c.ToRelationships,
                        Portal = c.Portal,
                        TextAdditionalPropertiesValues = c.TextAdditionalPropertiesValues,
                        StructuredDataAdditionalPropertyValues = c.StructuredDataAdditionalPropertyValues,
                        SavedPaymentMethods = c.SavedPaymentMethods,
                    });
            }

            if (relatedEntities.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.Customer.Tenant))))
            {
                query = query.Join(
                    this.dbContext.Tenants,
                    c => c.Customer.TenantId,
                    t => t.Id,
                    (c, tenant) => new CustomerReadModelWithRelatedEntities
                    {
                        Customer = c.Customer,
                        Tenant = tenant,
                        TenantDetails = tenant.DetailsCollection,
                        Organisation = c.Organisation,
                        Owner = c.Owner,
                        QuoteDocuments = c.QuoteDocuments,
                        ClaimDocuments = c.ClaimDocuments,
                        Emails = c.Emails,
                        Quotes = c.Quotes,
                        Claims = c.Claims,
                        PolicyTransactions = c.PolicyTransactions,
                        Policies = c.Policies,
                        PrimaryPerson = c.PrimaryPerson,
                        PersonStreetAddresses = c.PersonStreetAddresses,
                        PersonEmailAddresses = c.PersonEmailAddresses,
                        PersonPhoneNumbers = c.PersonPhoneNumbers,
                        PersonMessengerIds = c.PersonMessengerIds,
                        PersonSocialMediaIds = c.PersonSocialMediaIds,
                        PersonWebsiteAddreses = c.PersonWebsiteAddreses,
                        People = c.People,
                        Sms = c.Sms,
                        FromRelationships = c.FromRelationships,
                        ToRelationships = c.ToRelationships,
                        Portal = c.Portal,
                        TextAdditionalPropertiesValues = c.TextAdditionalPropertiesValues,
                        StructuredDataAdditionalPropertyValues = c.StructuredDataAdditionalPropertyValues,
                        SavedPaymentMethods = c.SavedPaymentMethods,
                    });
            }

            if (relatedEntities.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.Customer.Organisation))))
            {
                query = query.Join(
                    this.dbContext.OrganisationReadModel,
                    c => c.Customer.OrganisationId,
                    t => t.Id,
                    (c, organisation) => new CustomerReadModelWithRelatedEntities
                    {
                        Customer = c.Customer,
                        Tenant = c.Tenant,
                        TenantDetails = c.TenantDetails,
                        Organisation = organisation,
                        Owner = c.Owner,
                        QuoteDocuments = c.QuoteDocuments,
                        ClaimDocuments = c.ClaimDocuments,
                        Emails = c.Emails,
                        Quotes = c.Quotes,
                        Claims = c.Claims,
                        PolicyTransactions = c.PolicyTransactions,
                        Policies = c.Policies,
                        PrimaryPerson = c.PrimaryPerson,
                        PersonStreetAddresses = c.PersonStreetAddresses,
                        PersonEmailAddresses = c.PersonEmailAddresses,
                        PersonPhoneNumbers = c.PersonPhoneNumbers,
                        PersonMessengerIds = c.PersonMessengerIds,
                        PersonSocialMediaIds = c.PersonSocialMediaIds,
                        PersonWebsiteAddreses = c.PersonWebsiteAddreses,
                        People = c.People,
                        Sms = c.Sms,
                        FromRelationships = c.FromRelationships,
                        ToRelationships = c.ToRelationships,
                        Portal = c.Portal,
                        TextAdditionalPropertiesValues = c.TextAdditionalPropertiesValues,
                        StructuredDataAdditionalPropertyValues = c.StructuredDataAdditionalPropertyValues,
                        SavedPaymentMethods = c.SavedPaymentMethods,
                    });
            }

            if (relatedEntities.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.Customer.Owner))))
            {
                // Left Join
                query = query.GroupJoin(
                    this.dbContext.Users,
                    c => c.Customer.OwnerUserId,
                    c => c.Id,
                    (c, user) => new CustomerReadModelWithRelatedEntities
                    {
                        Customer = c.Customer,
                        Tenant = c.Tenant,
                        TenantDetails = c.TenantDetails,
                        Organisation = c.Organisation,
                        Owner = user.FirstOrDefault(),
                        QuoteDocuments = c.QuoteDocuments,
                        ClaimDocuments = c.ClaimDocuments,
                        Emails = c.Emails,
                        Quotes = c.Quotes,
                        Claims = c.Claims,
                        PolicyTransactions = c.PolicyTransactions,
                        Policies = c.Policies,
                        PrimaryPerson = c.PrimaryPerson,
                        PersonStreetAddresses = c.PersonStreetAddresses,
                        PersonEmailAddresses = c.PersonEmailAddresses,
                        PersonPhoneNumbers = c.PersonPhoneNumbers,
                        PersonMessengerIds = c.PersonMessengerIds,
                        PersonSocialMediaIds = c.PersonSocialMediaIds,
                        PersonWebsiteAddreses = c.PersonWebsiteAddreses,
                        People = c.People,
                        Sms = c.Sms,
                        FromRelationships = c.FromRelationships,
                        ToRelationships = c.ToRelationships,
                        Portal = c.Portal,
                        TextAdditionalPropertiesValues = c.TextAdditionalPropertiesValues,
                        StructuredDataAdditionalPropertyValues = c.StructuredDataAdditionalPropertyValues,
                        SavedPaymentMethods = c.SavedPaymentMethods,
                    });
            }

            if (relatedEntities.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.Policy.Documents))))
            {
                query = query.GroupJoin(
                   this.dbContext.QuoteDocuments,
                   p => p.Customer.Id,
                   c => c.CustomerId,
                   (c, documents) => new CustomerReadModelWithRelatedEntities
                   {
                       Customer = c.Customer,
                       Tenant = c.Tenant,
                       TenantDetails = c.TenantDetails,
                       Organisation = c.Organisation,
                       Owner = c.Owner,
                       QuoteDocuments = documents,
                       ClaimDocuments = c.ClaimDocuments,
                       Emails = c.Emails,
                       Quotes = c.Quotes,
                       Claims = c.Claims,
                       PolicyTransactions = c.PolicyTransactions,
                       Policies = c.Policies,
                       PrimaryPerson = c.PrimaryPerson,
                       PersonStreetAddresses = c.PersonStreetAddresses,
                       PersonEmailAddresses = c.PersonEmailAddresses,
                       PersonPhoneNumbers = c.PersonPhoneNumbers,
                       PersonMessengerIds = c.PersonMessengerIds,
                       PersonSocialMediaIds = c.PersonSocialMediaIds,
                       PersonWebsiteAddreses = c.PersonWebsiteAddreses,
                       People = c.People,
                       Sms = c.Sms,
                       FromRelationships = c.FromRelationships,
                       ToRelationships = c.ToRelationships,
                       Portal = c.Portal,
                       TextAdditionalPropertiesValues = c.TextAdditionalPropertiesValues,
                       StructuredDataAdditionalPropertyValues = c.StructuredDataAdditionalPropertyValues,
                       SavedPaymentMethods = c.SavedPaymentMethods,
                   });

                query = query.GroupJoin(
                    this.dbContext.ClaimAttachment,
                    p => p.Customer.Id,
                    c => c.CustomerId,
                    (c, documents) => new CustomerReadModelWithRelatedEntities
                    {
                        Customer = c.Customer,
                        Tenant = c.Tenant,
                        TenantDetails = c.TenantDetails,
                        Organisation = c.Organisation,
                        Owner = c.Owner,
                        QuoteDocuments = c.QuoteDocuments,
                        ClaimDocuments = documents,
                        Emails = c.Emails,
                        Quotes = c.Quotes,
                        Claims = c.Claims,
                        PolicyTransactions = c.PolicyTransactions,
                        Policies = c.Policies,
                        PrimaryPerson = c.PrimaryPerson,
                        PersonStreetAddresses = c.PersonStreetAddresses,
                        PersonEmailAddresses = c.PersonEmailAddresses,
                        PersonPhoneNumbers = c.PersonPhoneNumbers,
                        PersonMessengerIds = c.PersonMessengerIds,
                        PersonSocialMediaIds = c.PersonSocialMediaIds,
                        PersonWebsiteAddreses = c.PersonWebsiteAddreses,
                        People = c.People,
                        Sms = c.Sms,
                        FromRelationships = c.FromRelationships,
                        ToRelationships = c.ToRelationships,
                        Portal = c.Portal,
                        TextAdditionalPropertiesValues = c.TextAdditionalPropertiesValues,
                        StructuredDataAdditionalPropertyValues = c.StructuredDataAdditionalPropertyValues,
                        SavedPaymentMethods = c.SavedPaymentMethods,
                    });
            }

            if (relatedEntities.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.Customer.Messages))))
            {
                var emailQuery = from email in this.dbContext.Emails
                                 join relationship in this.dbContext.Relationships on new
                                 {
                                     EmailId = email.Id,
                                     Type = RelationshipType.CustomerMessage,
                                     FromEntityType = EntityType.Customer,
                                 }
                                 equals new
                                 {
                                     EmailId = relationship.ToEntityId,
                                     Type = relationship.Type,
                                     FromEntityType = relationship.FromEntityType,
                                 }
                                 select new
                                 {
                                     Email = email,
                                     RelationShip = relationship,
                                 };

                query = query.GroupJoin(
                    emailQuery,
                    p => p.Customer.Id,
                    c => c.RelationShip.FromEntityId,
                    (c, emails) => new CustomerReadModelWithRelatedEntities
                    {
                        Customer = c.Customer,
                        Tenant = c.Tenant,
                        TenantDetails = c.TenantDetails,
                        Organisation = c.Organisation,
                        Owner = c.Owner,
                        QuoteDocuments = c.QuoteDocuments,
                        ClaimDocuments = c.ClaimDocuments,
                        Emails = emails.Select(e => e.Email),
                        Quotes = c.Quotes,
                        Claims = c.Claims,
                        PolicyTransactions = c.PolicyTransactions,
                        Policies = c.Policies,
                        PrimaryPerson = c.PrimaryPerson,
                        PersonStreetAddresses = c.PersonStreetAddresses,
                        PersonEmailAddresses = c.PersonEmailAddresses,
                        PersonPhoneNumbers = c.PersonPhoneNumbers,
                        PersonMessengerIds = c.PersonMessengerIds,
                        PersonSocialMediaIds = c.PersonSocialMediaIds,
                        PersonWebsiteAddreses = c.PersonWebsiteAddreses,
                        People = c.People,
                        Sms = c.Sms,
                        FromRelationships = c.FromRelationships,
                        ToRelationships = c.ToRelationships,
                        Portal = c.Portal,
                        TextAdditionalPropertiesValues = c.TextAdditionalPropertiesValues,
                        StructuredDataAdditionalPropertyValues = c.StructuredDataAdditionalPropertyValues,
                        SavedPaymentMethods = c.SavedPaymentMethods,
                    });

                var smsQuery = from sms in this.dbContext.Sms
                               join relationship in this.dbContext.Relationships on new { SmsId = sms.Id, Type = RelationshipType.CustomerMessage, FromEntityType = EntityType.Customer } equals new { SmsId = relationship.ToEntityId, Type = relationship.Type, FromEntityType = relationship.FromEntityType }
                               select new
                               {
                                   Sms = sms,
                                   RelationShip = relationship,
                               };

                query = query.GroupJoin(
                    smsQuery,
                    p => p.Customer.Id,
                    c => c.RelationShip.FromEntityId,
                    (c, sms) => new CustomerReadModelWithRelatedEntities
                    {
                        Customer = c.Customer,
                        Tenant = c.Tenant,
                        TenantDetails = c.TenantDetails,
                        Organisation = c.Organisation,
                        Owner = c.Owner,
                        QuoteDocuments = c.QuoteDocuments,
                        ClaimDocuments = c.ClaimDocuments,
                        Emails = c.Emails,
                        Quotes = c.Quotes,
                        Claims = c.Claims,
                        PolicyTransactions = c.PolicyTransactions,
                        Policies = c.Policies,
                        PrimaryPerson = c.PrimaryPerson,
                        PersonStreetAddresses = c.PersonStreetAddresses,
                        PersonEmailAddresses = c.PersonEmailAddresses,
                        PersonPhoneNumbers = c.PersonPhoneNumbers,
                        PersonMessengerIds = c.PersonMessengerIds,
                        PersonSocialMediaIds = c.PersonSocialMediaIds,
                        PersonWebsiteAddreses = c.PersonWebsiteAddreses,
                        People = c.People,
                        Sms = sms.Select(e => e.Sms),
                        FromRelationships = c.FromRelationships,
                        ToRelationships = c.ToRelationships,
                        Portal = c.Portal,
                        TextAdditionalPropertiesValues = c.TextAdditionalPropertiesValues,
                        StructuredDataAdditionalPropertyValues = c.StructuredDataAdditionalPropertyValues,
                        SavedPaymentMethods = c.SavedPaymentMethods,
                    });
            }

            if (relatedEntities.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.Customer.Quotes))))
            {
                query = query.GroupJoin(
                    this.dbContext.QuoteReadModels,
                    p => p.Customer.Id,
                    c => c.CustomerId,
                    (c, quotes) => new CustomerReadModelWithRelatedEntities
                    {
                        Customer = c.Customer,
                        Tenant = c.Tenant,
                        TenantDetails = c.TenantDetails,
                        Organisation = c.Organisation,
                        Owner = c.Owner,
                        QuoteDocuments = c.QuoteDocuments,
                        ClaimDocuments = c.ClaimDocuments,
                        Emails = c.Emails,
                        Quotes = quotes,
                        Claims = c.Claims,
                        PolicyTransactions = c.PolicyTransactions,
                        Policies = c.Policies,
                        PrimaryPerson = c.PrimaryPerson,
                        PersonStreetAddresses = c.PersonStreetAddresses,
                        PersonEmailAddresses = c.PersonEmailAddresses,
                        PersonPhoneNumbers = c.PersonPhoneNumbers,
                        PersonMessengerIds = c.PersonMessengerIds,
                        PersonSocialMediaIds = c.PersonSocialMediaIds,
                        PersonWebsiteAddreses = c.PersonWebsiteAddreses,
                        People = c.People,
                        Sms = c.Sms,
                        FromRelationships = c.FromRelationships,
                        ToRelationships = c.ToRelationships,
                        Portal = c.Portal,
                        TextAdditionalPropertiesValues = c.TextAdditionalPropertiesValues,
                        StructuredDataAdditionalPropertyValues = c.StructuredDataAdditionalPropertyValues,
                        SavedPaymentMethods = c.SavedPaymentMethods,
                    });
            }

            if (relatedEntities.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.Customer.Claims))))
            {
                query = query.GroupJoin(
                    this.dbContext.ClaimReadModels,
                    p => p.Customer.Id,
                    c => c.CustomerId,
                    (c, claims) => new CustomerReadModelWithRelatedEntities
                    {
                        Customer = c.Customer,
                        Tenant = c.Tenant,
                        TenantDetails = c.TenantDetails,
                        Organisation = c.Organisation,
                        Owner = c.Owner,
                        QuoteDocuments = c.QuoteDocuments,
                        ClaimDocuments = c.ClaimDocuments,
                        Emails = c.Emails,
                        Quotes = c.Quotes,
                        Claims = claims.Where(s => s.Status != ClaimState.Nascent),
                        PolicyTransactions = c.PolicyTransactions,
                        Policies = c.Policies,
                        PrimaryPerson = c.PrimaryPerson,
                        PersonStreetAddresses = c.PersonStreetAddresses,
                        PersonEmailAddresses = c.PersonEmailAddresses,
                        PersonPhoneNumbers = c.PersonPhoneNumbers,
                        PersonMessengerIds = c.PersonMessengerIds,
                        PersonSocialMediaIds = c.PersonSocialMediaIds,
                        PersonWebsiteAddreses = c.PersonWebsiteAddreses,
                        People = c.People,
                        Sms = c.Sms,
                        FromRelationships = c.FromRelationships,
                        ToRelationships = c.ToRelationships,
                        Portal = c.Portal,
                        TextAdditionalPropertiesValues = c.TextAdditionalPropertiesValues,
                        StructuredDataAdditionalPropertyValues = c.StructuredDataAdditionalPropertyValues,
                        SavedPaymentMethods = c.SavedPaymentMethods,
                    });
            }

            if (relatedEntities.Any(
                    a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.Customer.PolicyTransactions))))
            {
                query = query.GroupJoin(
                    this.dbContext.PolicyTransactions,
                    p => p.Customer.Id,
                    c => c.CustomerId,
                    (c, policyTransactions) => new CustomerReadModelWithRelatedEntities
                    {
                        Customer = c.Customer,
                        Tenant = c.Tenant,
                        TenantDetails = c.TenantDetails,
                        Organisation = c.Organisation,
                        Owner = c.Owner,
                        QuoteDocuments = c.QuoteDocuments,
                        ClaimDocuments = c.ClaimDocuments,
                        Emails = c.Emails,
                        Quotes = c.Quotes,
                        Claims = c.Claims,
                        PolicyTransactions = policyTransactions,
                        Policies = c.Policies,
                        PrimaryPerson = c.PrimaryPerson,
                        PersonStreetAddresses = c.PersonStreetAddresses,
                        PersonEmailAddresses = c.PersonEmailAddresses,
                        PersonPhoneNumbers = c.PersonPhoneNumbers,
                        PersonMessengerIds = c.PersonMessengerIds,
                        PersonSocialMediaIds = c.PersonSocialMediaIds,
                        PersonWebsiteAddreses = c.PersonWebsiteAddreses,
                        People = c.People,
                        Sms = c.Sms,
                        FromRelationships = c.FromRelationships,
                        ToRelationships = c.ToRelationships,
                        Portal = c.Portal,
                        TextAdditionalPropertiesValues = c.TextAdditionalPropertiesValues,
                        StructuredDataAdditionalPropertyValues = c.StructuredDataAdditionalPropertyValues,
                        SavedPaymentMethods = c.SavedPaymentMethods,
                    });
            }

            if (relatedEntities.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.Customer.People))))
            {
                query = query.GroupJoin(
                    this.dbContext.PersonReadModels,
                    p => p.Customer.Id,
                    c => c.CustomerId,
                    (c, p) => new CustomerReadModelWithRelatedEntities()
                    {
                        Customer = c.Customer,
                        Tenant = c.Tenant,
                        TenantDetails = c.TenantDetails,
                        Organisation = c.Organisation,
                        Owner = c.Owner,
                        QuoteDocuments = c.QuoteDocuments,
                        ClaimDocuments = c.ClaimDocuments,
                        Emails = c.Emails,
                        Quotes = c.Quotes,
                        Claims = c.Claims,
                        PolicyTransactions = c.PolicyTransactions,
                        Policies = c.Policies,
                        PrimaryPerson = c.PrimaryPerson,
                        PersonStreetAddresses = p.SelectMany(x => x.StreetAddresses),
                        PersonEmailAddresses = p.SelectMany(x => x.EmailAddresses),
                        PersonPhoneNumbers = p.SelectMany(x => x.PhoneNumbers),
                        PersonMessengerIds = p.SelectMany(x => x.MessengerIds),
                        PersonSocialMediaIds = p.SelectMany(x => x.SocialMediaIds),
                        PersonWebsiteAddreses = p.SelectMany(x => x.WebsiteAddresses),
                        People = p.Where(person => !person.IsDeleted),
                        Sms = c.Sms,
                        FromRelationships = c.FromRelationships,
                        ToRelationships = c.ToRelationships,
                        Portal = c.Portal,
                        TextAdditionalPropertiesValues = c.TextAdditionalPropertiesValues,
                        StructuredDataAdditionalPropertyValues = c.StructuredDataAdditionalPropertyValues,
                        SavedPaymentMethods = c.SavedPaymentMethods,
                    });
            }

            if (relatedEntities.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.Customer.PrimaryPerson))))
            {
                query = query.Join(
                   this.dbContext.PersonReadModels,
                   c => c.Customer.PrimaryPersonId,
                   p => p.Id,
                   (c, p) => new CustomerReadModelWithRelatedEntities()
                   {
                       Customer = c.Customer,
                       Tenant = c.Tenant,
                       TenantDetails = c.TenantDetails,
                       Organisation = c.Organisation,
                       Owner = c.Owner,
                       QuoteDocuments = c.QuoteDocuments,
                       ClaimDocuments = c.ClaimDocuments,
                       Emails = c.Emails,
                       Quotes = c.Quotes,
                       Claims = c.Claims,
                       PolicyTransactions = c.PolicyTransactions,
                       Policies = c.Policies,
                       PrimaryPerson = p,
                       PersonStreetAddresses = c.PersonStreetAddresses.Concat(p.StreetAddresses),
                       PersonEmailAddresses = c.PersonEmailAddresses.Concat(p.EmailAddresses),
                       PersonPhoneNumbers = c.PersonPhoneNumbers.Concat(p.PhoneNumbers),
                       PersonMessengerIds = c.PersonMessengerIds.Concat(p.MessengerIds),
                       PersonSocialMediaIds = c.PersonSocialMediaIds.Concat(p.SocialMediaIds),
                       PersonWebsiteAddreses = c.PersonWebsiteAddreses.Concat(p.WebsiteAddresses),
                       People = c.People,
                       Sms = c.Sms,
                       FromRelationships = c.FromRelationships,
                       ToRelationships = c.ToRelationships,
                       Portal = c.Portal,
                       TextAdditionalPropertiesValues = c.TextAdditionalPropertiesValues,
                       StructuredDataAdditionalPropertyValues = c.StructuredDataAdditionalPropertyValues,
                       SavedPaymentMethods = c.SavedPaymentMethods,
                   });
            }

            if (relatedEntities.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.Customer.Portal))))
            {
                // Left Join
                query = query.GroupJoin(
                    this.dbContext.PortalReadModels,
                    c => c.Customer.PortalId,
                    p => p.Id,
                    (c, portal) => new CustomerReadModelWithRelatedEntities()
                    {
                        Customer = c.Customer,
                        Tenant = c.Tenant,
                        TenantDetails = c.TenantDetails,
                        Organisation = c.Organisation,
                        Owner = c.Owner,
                        QuoteDocuments = c.QuoteDocuments,
                        ClaimDocuments = c.ClaimDocuments,
                        Emails = c.Emails,
                        Quotes = c.Quotes,
                        Claims = c.Claims,
                        PolicyTransactions = c.PolicyTransactions,
                        Policies = c.Policies,
                        PrimaryPerson = c.PrimaryPerson,
                        PersonStreetAddresses = c.PersonStreetAddresses,
                        PersonEmailAddresses = c.PersonEmailAddresses,
                        PersonPhoneNumbers = c.PersonPhoneNumbers,
                        PersonMessengerIds = c.PersonMessengerIds,
                        PersonSocialMediaIds = c.PersonSocialMediaIds,
                        PersonWebsiteAddreses = c.PersonWebsiteAddreses,
                        People = c.People,
                        Sms = c.Sms,
                        FromRelationships = c.FromRelationships,
                        ToRelationships = c.ToRelationships,
                        Portal = portal.FirstOrDefault(),
                        TextAdditionalPropertiesValues = c.TextAdditionalPropertiesValues,
                        StructuredDataAdditionalPropertyValues = c.StructuredDataAdditionalPropertyValues,
                        SavedPaymentMethods = c.SavedPaymentMethods,
                    });
            }

            if (relatedEntities.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.Customer.AdditionalProperties))))
            {
                query = query.GroupJoin(
                this.dbContext.TextAdditionalPropertValues,
                c => c.Customer.Id,
                p => p.EntityId,
                (c, apv) => new CustomerReadModelWithRelatedEntities()
                {
                    Customer = c.Customer,
                    Tenant = c.Tenant,
                    TenantDetails = c.TenantDetails,
                    Organisation = c.Organisation,
                    Owner = c.Owner,
                    QuoteDocuments = c.QuoteDocuments,
                    ClaimDocuments = c.ClaimDocuments,
                    Emails = c.Emails,
                    Quotes = c.Quotes,
                    Claims = c.Claims,
                    PolicyTransactions = c.PolicyTransactions,
                    Policies = c.Policies,
                    PrimaryPerson = c.PrimaryPerson,
                    PersonStreetAddresses = c.PersonStreetAddresses,
                    PersonEmailAddresses = c.PersonEmailAddresses,
                    PersonPhoneNumbers = c.PersonPhoneNumbers,
                    PersonMessengerIds = c.PersonMessengerIds,
                    PersonSocialMediaIds = c.PersonSocialMediaIds,
                    PersonWebsiteAddreses = c.PersonWebsiteAddreses,
                    People = c.People,
                    Sms = c.Sms,
                    FromRelationships = c.FromRelationships,
                    ToRelationships = c.ToRelationships,
                    Portal = c.Portal,
                    TextAdditionalPropertiesValues = apv.Where(df => !df.AdditionalPropertyDefinition.IsDeleted),
                    StructuredDataAdditionalPropertyValues = c.StructuredDataAdditionalPropertyValues,
                    SavedPaymentMethods = c.SavedPaymentMethods,
                })
                .GroupJoin(
                this.dbContext.StructuredDataAdditionalPropertyValues,
                c => c.Customer.Id,
                p => p.EntityId,
                (c, apv) => new CustomerReadModelWithRelatedEntities()
                {
                    Customer = c.Customer,
                    Tenant = c.Tenant,
                    TenantDetails = c.TenantDetails,
                    Organisation = c.Organisation,
                    Owner = c.Owner,
                    QuoteDocuments = c.QuoteDocuments,
                    ClaimDocuments = c.ClaimDocuments,
                    Emails = c.Emails,
                    Quotes = c.Quotes,
                    Claims = c.Claims,
                    PolicyTransactions = c.PolicyTransactions,
                    Policies = c.Policies,
                    PrimaryPerson = c.PrimaryPerson,
                    PersonStreetAddresses = c.PersonStreetAddresses,
                    PersonEmailAddresses = c.PersonEmailAddresses,
                    PersonPhoneNumbers = c.PersonPhoneNumbers,
                    PersonMessengerIds = c.PersonMessengerIds,
                    PersonSocialMediaIds = c.PersonSocialMediaIds,
                    PersonWebsiteAddreses = c.PersonWebsiteAddreses,
                    People = c.People,
                    Sms = c.Sms,
                    FromRelationships = c.FromRelationships,
                    ToRelationships = c.ToRelationships,
                    Portal = c.Portal,
                    TextAdditionalPropertiesValues = c.TextAdditionalPropertiesValues,
                    StructuredDataAdditionalPropertyValues = apv.Where(df => !df.AdditionalPropertyDefinition.IsDeleted),
                    SavedPaymentMethods = c.SavedPaymentMethods,
                });
            }

            if (relatedEntities.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.Customer.SavedPaymentMethods))))
            {
                query = query.GroupJoin(
                    this.dbContext.SavedPaymentMethods,
                    c => c.Customer.Id,
                    p => p.CustomerId,
                    (c, som) => new CustomerReadModelWithRelatedEntities
                    {
                        Customer = c.Customer,
                        Tenant = c.Tenant,
                        TenantDetails = c.TenantDetails,
                        Organisation = c.Organisation,
                        Owner = c.Owner,
                        QuoteDocuments = c.QuoteDocuments,
                        ClaimDocuments = c.ClaimDocuments,
                        Emails = c.Emails,
                        Quotes = c.Quotes,
                        Claims = c.Claims,
                        PolicyTransactions = c.PolicyTransactions,
                        Policies = c.Policies,
                        PrimaryPerson = c.PrimaryPerson,
                        PersonStreetAddresses = c.PersonStreetAddresses,
                        PersonEmailAddresses = c.PersonEmailAddresses,
                        PersonPhoneNumbers = c.PersonPhoneNumbers,
                        PersonMessengerIds = c.PersonMessengerIds,
                        PersonSocialMediaIds = c.PersonSocialMediaIds,
                        PersonWebsiteAddreses = c.PersonWebsiteAddreses,
                        People = c.People,
                        Sms = c.Sms,
                        FromRelationships = c.FromRelationships,
                        ToRelationships = c.ToRelationships,
                        Portal = c.Portal,
                        TextAdditionalPropertiesValues = c.TextAdditionalPropertiesValues,
                        StructuredDataAdditionalPropertyValues = c.StructuredDataAdditionalPropertyValues,
                        SavedPaymentMethods = som.ToList(),
                    });
            }

            return query;
        }

        private IQueryable<CustomerReadModel> Filter(IQueryable<CustomerReadModel> query, EntityListFilters filters)
        {
            if (filters.Environment != null)
            {
                query = query.Where(c => c.Environment == filters.Environment);
            }

            bool hasOrganisationFilter = filters.OrganisationIds != null && filters.OrganisationIds.Any();
            if (hasOrganisationFilter)
            {
                query = query.Where(c => filters.OrganisationIds.Contains(c.OrganisationId));
            }

            if (filters.OwnerUserId != null)
            {
                query = query.Where(customer => customer.OwnerUserId == filters.OwnerUserId);
            }

            if (filters.SearchTerms.Any())
            {
                var searchExpression = PredicateBuilder.New<CustomerReadModel>(false);
                foreach (var customerName in filters.SearchTerms)
                {
                    searchExpression.Or(p => p.PrimaryPerson.FullName.Contains(customerName)
                        || p.PrimaryPerson.PreferredName.Contains(customerName));
                }

                query = query.Where(searchExpression);
            }

            if (filters.DateIsAfterTicks != null && filters.DateFilteringPropertyName != null)
            {
                query = query.Where(ExpressionHelper.GreaterThanExpression<CustomerReadModel>(filters.DateFilteringPropertyName, filters.DateIsAfterTicks));
            }

            if (filters.DateIsBeforeTicks != null && filters.DateFilteringPropertyName != null)
            {
                query = query.Where(ExpressionHelper.LessThanExpression<CustomerReadModel>(filters.DateFilteringPropertyName, filters.DateIsBeforeTicks));
            }

            if (filters.Statuses.Any())
            {
                var statusPredicate = PredicateBuilder.New<CustomerReadModel>(false);
                foreach (var status in filters.Statuses.Select(s => s.ToEnumOrThrow<UserStatus>()))
                {
                    statusPredicate = statusPredicate.Or(this.GetStatusExpression(status));
                }

                query = query.Where(statusPredicate);
            }

            if (!filters.IncludeTestData)
            {
                query = query.Where(c => c.IsTestData == false);
            }

            return query;
        }

        private IQueryable<CustomerReadModelDetail> Filter(IQueryable<CustomerReadModelDetail> query, EntityListFilters filters)
        {
            if (filters.Environment != null)
            {
                query = query.Where(c => c.Environment == filters.Environment);
            }

            bool hasOrganisationFilter = filters.OrganisationIds != null && filters.OrganisationIds.Any();
            if (hasOrganisationFilter)
            {
                query = query.Where(c => filters.OrganisationIds.Contains(c.OrganisationId));
            }

            if (filters.OwnerUserId != null)
            {
                query = query.Where(customer => customer.OwnerUserId == filters.OwnerUserId);
            }

            if (filters.SearchTerms.Any())
            {
                var searchExpression = PredicateBuilder.New<CustomerReadModelDetail>(false);
                foreach (var customerName in filters.SearchTerms)
                {
                    searchExpression.Or(p => p.FullName.Contains(customerName) || p.PreferredName.Contains(customerName));
                }

                query = query.Where(searchExpression);
            }

            if (filters.DateIsAfterTicks != null && filters.DateFilteringPropertyName != null)
            {
                query = query.Where(ExpressionHelper.GreaterThanExpression<CustomerReadModelDetail>(filters.DateFilteringPropertyName, filters.DateIsAfterTicks));
            }

            if (filters.DateIsBeforeTicks != null && filters.DateFilteringPropertyName != null)
            {
                query = query.Where(ExpressionHelper.LessThanExpression<CustomerReadModelDetail>(filters.DateFilteringPropertyName, filters.DateIsBeforeTicks));
            }

            if (filters.Statuses.Any())
            {
                var statusPredicate = PredicateBuilder.New<CustomerReadModelDetail>(false);
                foreach (var status in filters.Statuses.Select(s => s.ToEnumOrThrow<UserStatus>()))
                {
                    statusPredicate = statusPredicate.Or(this.GetDetailStatusExpression(status));
                }

                query = query.Where(statusPredicate);
            }

            if (!filters.IncludeTestData)
            {
                query = query.Where(c => c.IsTestData == false);
            }

            return query;
        }

        private IQueryable<CustomerReadModel> QueryReadModel(
            Guid tenantId, Guid? customerId)
        {
            var customerQuery = this.dbContext.CustomerReadModels
                .IncludeAllProperties()
                .Where(c => c.TenantId == tenantId && !c.IsDeleted);

            if (customerId.HasValue)
            {
                customerQuery = customerQuery.Where(c => c.Id == customerId.Value);
            }

            return customerQuery;
        }

        private IQueryable<CustomerReadModelDetail> QueryReadModelDetail(
            Guid tenantId, Guid? customerId)
        {
            var customerQuery = this.QueryReadModel(tenantId, customerId);
            var query = customerQuery
                .Select(c => new CustomerReadModelDetail
                {
                    Id = c.Id,
                    PrimaryPersonId = c.PrimaryPersonId,
                    People = c.People,
                    Environment = c.Environment,
                    UserId = default,
                    IsTestData = c.IsTestData,
                    OwnerUserId = c.OwnerUserId,
                    OwnerPersonId = c.OwnerPersonId,
                    OwnerFullName = c.OwnerFullName,
                    UserIsBlocked = default,
                    UserHasBeenInvitedToActivate = default,
                    UserHasBeenActivated = default,
                    IsDeleted = c.IsDeleted,
                    CreatedTicksSinceEpoch = c.CreatedTicksSinceEpoch,
                    LastModifiedTicksSinceEpoch = c.LastModifiedTicksSinceEpoch,
                    FullName = default,
                    NamePrefix = default,
                    FirstName = default,
                    MiddleNames = default,
                    LastName = default,
                    NameSuffix = default,
                    Company = default,
                    Title = default,
                    PreferredName = default,
                    Email = default,
                    AlternativeEmail = default,
                    MobilePhoneNumber = default,
                    HomePhoneNumber = default,
                    WorkPhoneNumber = default,
                    TenantId = c.TenantId,
                    OrganisationId = c.OrganisationId,
                    OrganisationName = default,
                    PortalId = c.PortalId,
                });

            // Get the primary person
            query = query.Join(
                this.dbContext.PersonReadModels,
                c => c.PrimaryPersonId,
                p => p.Id,
                (c, p) => new CustomerReadModelDetail
                {
                    Id = c.Id,
                    PrimaryPersonId = c.PrimaryPersonId,
                    People = c.People,
                    Environment = c.Environment,
                    UserId = p.UserId,
                    IsTestData = c.IsTestData,
                    OwnerUserId = c.OwnerUserId,
                    OwnerPersonId = c.OwnerPersonId,
                    OwnerFullName = c.OwnerFullName,
                    UserIsBlocked = p.UserIsBlocked,
                    UserHasBeenInvitedToActivate = p.UserHasBeenInvitedToActivate,
                    UserHasBeenActivated = p.UserHasBeenActivated,
                    IsDeleted = c.IsDeleted,
                    CreatedTicksSinceEpoch = c.CreatedTicksSinceEpoch,
                    LastModifiedTicksSinceEpoch = c.LastModifiedTicksSinceEpoch,
                    FullName = p.FullName,
                    NamePrefix = p.NamePrefix,
                    FirstName = p.FirstName,
                    MiddleNames = p.MiddleNames,
                    LastName = p.LastName,
                    NameSuffix = p.NameSuffix,
                    Company = p.Company,
                    Title = p.Title,
                    PreferredName = p.PreferredName,
                    Email = p.Email,
                    AlternativeEmail = p.AlternativeEmail,
                    MobilePhoneNumber = p.MobilePhoneNumber,
                    HomePhoneNumber = p.HomePhoneNumber,
                    WorkPhoneNumber = p.WorkPhoneNumber,
                    TenantId = c.TenantId,
                    OrganisationId = c.OrganisationId,
                    OrganisationName = default,
                    PortalId = c.PortalId,
                });

            // Get the organisation
            query = query.Join(
                this.dbContext.OrganisationReadModel,
                c => c.OrganisationId,
                o => o.Id,
                (c, o) => new CustomerReadModelDetail
                {
                    Id = c.Id,
                    PrimaryPersonId = c.PrimaryPersonId,
                    People = c.People,
                    Environment = c.Environment,
                    UserId = c.UserId,
                    IsTestData = c.IsTestData,
                    OwnerUserId = c.OwnerUserId,
                    OwnerPersonId = c.OwnerPersonId,
                    OwnerFullName = c.OwnerFullName,
                    UserIsBlocked = c.UserIsBlocked,
                    UserHasBeenInvitedToActivate = c.UserHasBeenInvitedToActivate,
                    UserHasBeenActivated = c.UserHasBeenActivated,
                    IsDeleted = c.IsDeleted,
                    CreatedTicksSinceEpoch = c.CreatedTicksSinceEpoch,
                    LastModifiedTicksSinceEpoch = c.LastModifiedTicksSinceEpoch,
                    FullName = c.FullName,
                    NamePrefix = c.NamePrefix,
                    FirstName = c.FirstName,
                    MiddleNames = c.MiddleNames,
                    LastName = c.LastName,
                    NameSuffix = c.NameSuffix,
                    Company = c.Company,
                    Title = c.Title,
                    PreferredName = c.PreferredName,
                    Email = c.Email,
                    AlternativeEmail = c.AlternativeEmail,
                    MobilePhoneNumber = c.MobilePhoneNumber,
                    HomePhoneNumber = c.HomePhoneNumber,
                    WorkPhoneNumber = c.WorkPhoneNumber,
                    TenantId = c.TenantId,
                    OrganisationId = c.OrganisationId,
                    OrganisationName = o.Name,
                    PortalId = c.PortalId,
                });

            return query;
        }

        private Expression<Func<CustomerReadModelDetail, bool>> GetDetailStatusExpression(UserStatus status)
        {
            if (status == UserStatus.New)
            {
                return c => !c.UserIsBlocked && !c.UserHasBeenInvitedToActivate;
            }

            if (status == UserStatus.Invited)
            {
                return c => !c.UserIsBlocked && c.UserHasBeenInvitedToActivate && !c.UserHasBeenActivated;
            }

            if (status == UserStatus.Active)
            {
                return c => !c.UserIsBlocked && c.UserHasBeenActivated;
            }

            if (status == UserStatus.Deactivated)
            {
                return c => c.UserIsBlocked;
            }

            throw new InvalidOperationException("Unknown user status " + status.ToString());
        }

        private Expression<Func<CustomerReadModel, bool>> GetStatusExpression(UserStatus status)
        {
            if (status == UserStatus.New)
            {
                return customer =>
                    !customer.PrimaryPerson.UserIsBlocked &&
                    !customer.PrimaryPerson.UserHasBeenInvitedToActivate;
            }

            if (status == UserStatus.Invited)
            {
                return customer =>
                    !customer.PrimaryPerson.UserIsBlocked &&
                    customer.PrimaryPerson.UserHasBeenInvitedToActivate &&
                    !customer.PrimaryPerson.UserHasBeenActivated;
            }

            if (status == UserStatus.Active)
            {
                return customer =>
                    !customer.PrimaryPerson.UserIsBlocked &&
                    customer.PrimaryPerson.UserHasBeenActivated;
            }

            if (status == UserStatus.Deactivated)
            {
                return customer => customer.PrimaryPerson.UserIsBlocked;
            }

            throw new InvalidOperationException("Unknown user status " + status.ToString());
        }
    }
}
