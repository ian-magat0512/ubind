// <copyright file="SmsRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using LinqKit;
    using UBind.Domain;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Email;
    using UBind.Domain.ReadModel.Sms;
    using UBind.Domain.ReadWriteModel;
    using UBind.Domain.Repositories;
    using UBind.Persistence.ReadModels;
    using SmsEntitySchema = UBind.Domain.SerialisedEntitySchemaObject.SmsMessage;

    public class SmsRepository : ISmsRepository
    {
        private readonly IUBindDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="SmsRepository"/> class.
        /// </summary>
        /// <param name="dbContext">The db context.</param>
        public SmsRepository(
            IUBindDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        /// <summary>
        /// Gets an expression for instantiating detail projection from QuoteReadModels for use in EF projections.
        /// </summary>
        private Expression<Func<SmsDetailReadModel, ISmsDetails>> DetailsSelector =>
            model => new SmsDetails
            {
                To = model.Sms.To,
                From = model.Sms.From,
                Message = model.Sms.Message,
                User = model.User != null
                    ? new UserData
                    {
                        Id = model.User.Id,
                        UserType = model.User.UserType,
                        FullName = model.User.FullName,
                        PreferredName = model.User.PreferredName,
                        Email = model.User.Email,
                        AlternativeEmail = model.User.AlternativeEmail,
                        MobilePhoneNumber = model.User.MobilePhoneNumber,
                        WorkPhoneNumber = model.User.WorkPhoneNumber,
                        HomePhoneNumber = model.User.HomePhoneNumber,
                        OrganisationId = model.User.OrganisationId,
                        CustomerId = model.User.CustomerId,
                    }
                    : null,
                CreatedTicksSinceEpoch = model.Sms.CreatedTicksSinceEpoch,
                Id = model.Sms.Id,
                Policy = model.Quote != null && model.Quote.PolicyIssued && model.Policy != null
                    ? new PolicyData
                    {
                        Id = model.Policy.Id,
                        PolicyNumber = model.Policy.PolicyNumber,
                        OwnerUserId = model.Policy.OwnerUserId,
                        OrganisationId = model.Policy.OrganisationId,
                        CustomerId = model.Policy.CustomerId,
                    }
                    : null,
                Quote = model.Quote != null
                    ? new QuoteData
                    {
                        Id = model.Quote.Id,
                        QuoteNumber = model.Quote.QuoteNumber,
                        Type = model.Quote.Type,
                        OwnerUserId = model.Quote.OwnerUserId,
                        OrganisationId = model.Quote.OrganisationId,
                        CustomerId = model.Quote.CustomerId,
                    }
                    : null,
                Claim = model.Claim != null
                    ? new ClaimData
                    {
                        Id = model.Claim.Id.ToString(),
                        ClaimNumber = model.Claim.ClaimNumber ?? model.Claim.ClaimReference,
                        OwnerUserId = model.Claim.OwnerUserId,
                        OrganisationId = model.Claim.OrganisationId,
                        CustomerId = model.Claim.CustomerId,
                    }
                    : null,
                PolicyTransaction = model.PolicyTransaction != null
                    ? new Domain.ReadModel.Email.PolicyTransactionData { TransactionData = model.PolicyTransaction } : null,
                Customer = model.Customer != null
                    ? new CustomerData()
                    {
                        Id = model.Customer.Id,
                        FullName = model.CustomerPrimaryPerson.FullName,
                        PreferredName = model.CustomerPrimaryPerson.PreferredName,
                        Email = model.CustomerPrimaryPerson.Email,
                        AlternativeEmail = model.CustomerPrimaryPerson.AlternativeEmail,
                        MobilePhoneNumber = model.CustomerPrimaryPerson.MobilePhoneNumber,
                        WorkPhoneNumber = model.CustomerPrimaryPerson.WorkPhoneNumber,
                        HomePhoneNumber = model.CustomerPrimaryPerson.HomePhoneNumber,
                        OwnerUserId = model.Customer.OwnerUserId,
                        OrganisationId = model.Customer.OrganisationId,
                    }
                    : model.Policy != null && model.Policy.CustomerId.HasValue
                    ? new CustomerData()
                    {
                        Id = model.Policy.CustomerId.Value,
                        FullName = model.Policy.CustomerFullName,
                        PreferredName = model.Policy.CustomerPreferredName,
                        Email = model.Policy.CustomerEmail,
                        AlternativeEmail = model.Policy.CustomerAlternativeEmail,
                        MobilePhoneNumber = model.Policy.CustomerMobilePhone,
                        WorkPhoneNumber = model.Policy.CustomerWorkPhone,
                        HomePhoneNumber = model.Policy.CustomerHomePhone,
                        OwnerUserId = model.Policy.OwnerUserId,
                        OrganisationId = model.Policy.OrganisationId,
                    }
                    : null,
                Organisation = model.Organisation != null
                    ? new OrganisationData
                    {
                        Id = model.Organisation.Id,
                        Name = model.Organisation.Name,
                    }
                    : null,
                TenantId = model.Sms.TenantId,
                OrganisationId = model.Sms.OrganisationId,
                ProductId = model.Sms.ProductId,
            };

        /// <inheritdoc/>
        public IEnumerable<Sms> GetAll(Guid tenantId, EntityListFilters filters)
        {
            var query = this.Query(tenantId, filters);
            query = this.Filter(query, filters);
            return query.Select(s => s.Sms);
        }

        /// <inheritdoc/>
        public void Insert(Sms sms)
        {
            this.dbContext.Sms.Add(sms);
            this.dbContext.SaveChanges();
        }

        /// <inheritdoc/>
        public ISmsDetails GetById(Guid tenantId, Guid smsId)
        {
            var joinQuery = from sms in this.dbContext.Sms
                            join fromRelatioship in this.dbContext.Relationships on sms.Id equals fromRelatioship.FromEntityId into groupFromRelationship
                            join toRelatioship in this.dbContext.Relationships on sms.Id equals toRelatioship.ToEntityId into groupToRelationship
                            where sms.TenantId == tenantId && sms.Id == smsId
                            select new
                            {
                                Sms = sms,
                                Relationships = groupFromRelationship.Concat(groupToRelationship),
                                CustomerRelationships = groupToRelationship.FirstOrDefault(x => x.FromEntityType == EntityType.Customer),
                                UserRelationships = groupToRelationship.FirstOrDefault(x => x.FromEntityType == EntityType.User),
                                QuoteRelationships = groupToRelationship.FirstOrDefault(x => x.FromEntityType == EntityType.Quote),
                                ClaimRelationships = groupToRelationship.FirstOrDefault(x => x.FromEntityType == EntityType.Claim),
                                PolicyRelationships = groupToRelationship.FirstOrDefault(x => x.FromEntityType == EntityType.Policy),
                                PolicyTransactionRelationships = groupToRelationship.FirstOrDefault(x => x.FromEntityType == EntityType.PolicyTransaction),
                                SmsOrganisation = this.dbContext.OrganisationReadModel.FirstOrDefault(x => x.Id == sms.OrganisationId),
                            };

            var smsDetailQuery = from query in joinQuery
                                 join customerReadModel in this.dbContext.CustomerReadModels.IncludeAllProperties() on query.CustomerRelationships.FromEntityId equals customerReadModel.Id into customer
                                 join quoteReadModel in this.dbContext.QuoteReadModels on query.QuoteRelationships.FromEntityId equals quoteReadModel.Id into quote
                                 join claimReadModel in this.dbContext.ClaimReadModels on query.ClaimRelationships.FromEntityId equals claimReadModel.Id into claim
                                 join policyReadModel in this.dbContext.Policies on query.PolicyRelationships.FromEntityId equals policyReadModel.Id into policy
                                 join userReadModel in this.dbContext.Users on query.PolicyRelationships.FromEntityId equals userReadModel.Id into user
                                 join policyTransactionReadModel in this.dbContext.PolicyTransactions on query.PolicyTransactionRelationships.FromEntityId equals policyTransactionReadModel.Id into policyTransaction
                                 select new SmsDetailReadModel
                                 {
                                     Sms = query.Sms,
                                     Customer = customer.FirstOrDefault(),
                                     User = user.FirstOrDefault(),
                                     Quote = quote.FirstOrDefault(),
                                     Claim = claim.FirstOrDefault(),
                                     Policy = policy.FirstOrDefault(),
                                     PolicyTransaction = policyTransaction.FirstOrDefault(),
                                     Organisation = query.SmsOrganisation,
                                     CustomerPrimaryPerson = customer.FirstOrDefault().People.FirstOrDefault(p => p.Id == customer.FirstOrDefault().PrimaryPersonId),
                                 };

            var emailDetails = smsDetailQuery.Select(this.DetailsSelector).SingleOrDefault();

            return emailDetails;
        }

        /// <inheritdoc />
        public void InsertSmsTag(Tag tag)
        {
            this.dbContext.Tags.Add(tag);
            this.dbContext.SaveChanges();
        }

        /// <inheritdoc />
        public IEnumerable<Relationship> GetSmsRelationships(Guid tenantId, Guid smsId, EntityType? entityType = null)
        {
            var relationshipQuery = this.dbContext.Sms
                .Where(s => s.Id == smsId && s.TenantId == tenantId)
                .GroupJoin(
                     this.dbContext.Relationships,
                     sms => sms.Id,
                     relationship => relationship.FromEntityId,
                     (sms, relationships) => new { Sms = sms, Relationships = relationships })
                .GroupJoin(
                     this.dbContext.Relationships,
                     model => model.Sms.Id,
                     relationship => relationship.ToEntityId,
                     (model, relationships) => new SmsTagRelationshipModel
                     {
                         Sms = model.Sms,
                         Relationships = model.Relationships.Concat(relationships),
                     });

            if (entityType != null)
            {
                relationshipQuery = relationshipQuery
                    .Where(x => x.Relationships.Any(y => y.FromEntityType == entityType || y.ToEntityType == entityType));
            }

            return relationshipQuery.SelectMany(x => x.Relationships).ToList();
        }

        /// <inheritdoc />
        public void InsertSmsRelationship(Relationship relationship)
        {
            this.dbContext.Relationships.Add(relationship);
            this.dbContext.SaveChanges();
        }

        /// <inheritdoc />
        public void RemoveSmsRelationship(Relationship relationship)
        {
            this.dbContext.Relationships.Remove(relationship);
            this.dbContext.SaveChanges();
        }

        /// <inheritdoc />
        public ISmsReadModelWithRelatedEntities GetSmsWithRelatedEntities(
            Guid tenantId, Guid smsId, List<string> includeProperties)
        {
            var query = this.CreateQueryForSmsDetailsWithRelatedEntities(tenantId, includeProperties);
            return query.FirstOrDefault(s => s.Sms.Id == smsId);
        }

        /// <inheritdoc />
        public bool DoesSmsExists(Guid tenantId, Guid smsId)
        {
            return this.dbContext.Sms.Where(s => s.TenantId == tenantId && s.Id == smsId).Any();
        }

        /// <inheritdoc />
        public IQueryable<ISmsReadModelWithRelatedEntities> CreateQueryForSmsDetailsWithRelatedEntities(
            Guid tenantId, IEnumerable<string> includeProperties)
        {
            var smsList = this.GetAll(tenantId, new EntityListFilters()).AsQueryable();
            return this.CreateQueryForSmsDetailsWithRelatedEntities(smsList, includeProperties);
        }

        public IEnumerable<ISmsReadModelWithRelatedEntities> GetSmsListWithRelatedEntities(
            Guid tenantId, EntityListFilters filters, IEnumerable<string> relatedEntities)
        {
            var smsList = this.GetAll(tenantId, filters).AsQueryable();
            return this.CreateQueryForSmsDetailsWithRelatedEntities(smsList, relatedEntities);
        }

        public IQueryable<ISmsReadModelWithRelatedEntities> CreateQueryForSmsDetailsWithRelatedEntities(
            IQueryable<Sms> dataSource, IEnumerable<string> includeProperties)
        {
            var query = from sms in dataSource
                        select new SmsReadModelWithRelatedEntities()
                        {
                            Sms = sms,
                            Tenant = default,
                            TenantDetails = new TenantDetails[] { },
                            Organisation = default,
                            Tags = new Tag[] { },
                            FromRelationships = new Relationship[] { },
                            ToRelationships = new Relationship[] { },
                        };

            if (includeProperties.Any(a => a.EqualsIgnoreCase(nameof(SmsEntitySchema.Tenant))))
            {
                query = query.Join(this.dbContext.Tenants.IncludeAllProperties(), s => s.Sms.TenantId, t => t.Id, (s, tenant) => new SmsReadModelWithRelatedEntities
                {
                    Sms = s.Sms,
                    Tenant = tenant,
                    TenantDetails = tenant.DetailsCollection,
                    Organisation = s.Organisation,
                    Tags = s.Tags,
                    FromRelationships = s.FromRelationships,
                    ToRelationships = s.ToRelationships,
                });
            }

            if (includeProperties.Any(a => a.EqualsIgnoreCase(nameof(SmsEntitySchema.Organisation))))
            {
                query = query.Join(this.dbContext.OrganisationReadModel, s => s.Sms.OrganisationId, t => t.Id, (s, organisation) => new SmsReadModelWithRelatedEntities
                {
                    Sms = s.Sms,
                    Tenant = s.Tenant,
                    TenantDetails = s.TenantDetails,
                    Organisation = organisation,
                    Tags = s.Tags,
                    FromRelationships = s.FromRelationships,
                    ToRelationships = s.ToRelationships,
                });
            }

            if (includeProperties.Any(a => a.EqualsIgnoreCase(nameof(SmsEntitySchema.Tags))))
            {
                query = query.GroupJoin(this.dbContext.Tags, s => s.Sms.Id, t => t.EntityId, (s, tags) => new SmsReadModelWithRelatedEntities
                {
                    Sms = s.Sms,
                    Tenant = s.Tenant,
                    TenantDetails = s.TenantDetails,
                    Organisation = s.Organisation,
                    Tags = tags,
                    FromRelationships = s.FromRelationships,
                    ToRelationships = s.ToRelationships,
                });
            }

            if (includeProperties.Any(a => a.EqualsIgnoreCase(nameof(SmsEntitySchema.Relationships))))
            {
                query = query.GroupJoin(this.dbContext.Relationships, s => s.Sms.Id, r => r.FromEntityId, (s, relationships) => new SmsReadModelWithRelatedEntities
                {
                    Sms = s.Sms,
                    Tenant = s.Tenant,
                    TenantDetails = s.TenantDetails,
                    Organisation = s.Organisation,
                    Tags = s.Tags,
                    FromRelationships = relationships,
                    ToRelationships = s.ToRelationships,
                });

                query = query.GroupJoin(this.dbContext.Relationships, s => s.Sms.Id, r => r.ToEntityId, (s, relationships) => new SmsReadModelWithRelatedEntities
                {
                    Sms = s.Sms,
                    Tenant = s.Tenant,
                    TenantDetails = s.TenantDetails,
                    Organisation = s.Organisation,
                    Tags = s.Tags,
                    FromRelationships = s.FromRelationships,
                    ToRelationships = relationships,
                });
            }

            return query;
        }

        private IQueryable<SmsTagRelationshipModel> Query(Guid tenantId, EntityListFilters filters)
        {
            var joinQuery = from sms in this.dbContext.Sms
                            where sms.TenantId == tenantId
                            join tag in this.dbContext.Tags on sms.Id equals tag.EntityId into groupTag
                            join fromRelatioship in this.dbContext.Relationships on sms.Id equals fromRelatioship.FromEntityId into groupFromRelatioship
                            join toRelatioship in this.dbContext.Relationships on sms.Id equals toRelatioship.ToEntityId into groupToRelatioship
                            select new SmsTagRelationshipModel { Sms = sms, Tags = groupTag, Relationships = groupFromRelatioship.Concat(groupToRelatioship) };

            bool hasOrganisationFilter = filters.OrganisationIds != null && filters.OrganisationIds.Any();
            if (hasOrganisationFilter)
            {
                joinQuery = joinQuery.Where(s => filters.OrganisationIds.Contains(s.Sms.OrganisationId));
            }

            if (filters.Environment != null)
            {
                var predicate = PredicateBuilder.New<SmsTagRelationshipModel>(false);
                if (filters.IncludeNonEnvironmentSpecificData)
                {
                    predicate = predicate.Or(s => !s.Tags.Any(t => t.TagType == TagType.Environment));
                }

                predicate = predicate.Or(s => s.Tags.Any(t => t.TagType == TagType.Environment && t.Value == filters.Environment.ToString()));
                joinQuery = joinQuery.Where(predicate);
            }

            if (filters.OwnerUserId.HasValue)
            {
                var ownedCustomerPersonIdList = this.dbContext.CustomerReadModels
                    .Where(x => x.OwnerUserId == filters.OwnerUserId).Select(x => x.PrimaryPersonId);

                joinQuery = joinQuery.Where(x => x.Relationships.Any(
                    y => (y.FromEntityType == EntityType.Message && ownedCustomerPersonIdList.Any(a => a == y.ToEntityId))));
            }

            if (filters.EntityType.HasValue && filters.EntityId != null)
            {
                joinQuery = joinQuery.Where(y => y.Relationships.Any(x =>
                     (x.FromEntityType == filters.EntityType && x.FromEntityId == filters.EntityId) ||
                     (x.ToEntityType == filters.EntityType && x.ToEntityId == filters.EntityId)));
            }

            if (filters.CustomerId.HasValue && filters.CustomerId.GetValueOrDefault() != default)
            {
                joinQuery = joinQuery
                    .Where(y => y.Relationships.Any(x => x.FromEntityType == EntityType.Customer && x.FromEntityId == filters.CustomerId)
                        || y.Relationships.Any(x => x.ToEntityType == EntityType.Customer && x.ToEntityId == filters.CustomerId));
            }

            return joinQuery;
        }

        /// <summary>
        /// Filter by readmodel filter.
        /// </summary>
        /// <param name="joinQuery">The query to filter.</param>
        /// <param name="filters">The filter.</param>
        /// <returns>The IQueriable.</returns>
        private IQueryable<SmsTagRelationshipModel> Filter(
            IQueryable<SmsTagRelationshipModel> joinQuery, EntityListFilters filters)
        {
            if (filters.Statuses.Any())
            {
                var statusPredicate = PredicateBuilder.New<SmsTagRelationshipModel>(false);

                foreach (var status in filters.Statuses)
                {
                    if (!string.IsNullOrEmpty(status))
                    {
                        if (string.Equals(status, "Customer", StringComparison.OrdinalIgnoreCase))
                        {
                            statusPredicate = statusPredicate
                                .Or(str => str.Relationships
                                .Any(r => r.Type == RelationshipType.CustomerMessage
                                            && (r.FromEntityType == EntityType.Customer || r.ToEntityType == EntityType.Customer)));
                        }
                    }
                }

                joinQuery = joinQuery.Where(statusPredicate);
            }

            if (filters.DateIsAfterTicks.HasValue && filters.DateFilteringPropertyName != null)
            {
                joinQuery = joinQuery.Where(str => str.Sms.CreatedTicksSinceEpoch > filters.DateIsAfterTicks);
            }

            if (filters.DateIsBeforeTicks.HasValue && filters.DateFilteringPropertyName != null)
            {
                joinQuery = joinQuery.Where(str => str.Sms.CreatedTicksSinceEpoch < filters.DateIsBeforeTicks);
            }

            if (filters.SearchTerms.Any())
            {
                var searchExpression = PredicateBuilder.New<SmsTagRelationshipModel>(false);
                foreach (var searchTerm in filters.SearchTerms)
                {
                    var unescapedSearchTerm = Uri.UnescapeDataString(searchTerm);
                    searchExpression.Or(str => str.Sms.To.IndexOf(unescapedSearchTerm) >= 0
                                          || str.Sms.Message.IndexOf(unescapedSearchTerm) >= 0);
                }

                joinQuery = joinQuery.Where(searchExpression);
            }

            if (!string.IsNullOrEmpty(filters.SortBy))
            {
                var sortByVal = filters.SortBy.Contains("Sms.") ? filters.SortBy : "Sms." + filters.SortBy;
                joinQuery = joinQuery.Order(sortByVal, filters.SortOrder);
            }

            return joinQuery.Paginate(filters);
        }

        private class SmsTagRelationshipModel
        {
            public Sms Sms { get; set; }

            public IEnumerable<Relationship> Relationships { get; set; }

            public IEnumerable<Tag> Tags { get; set; }
        }
    }
}
