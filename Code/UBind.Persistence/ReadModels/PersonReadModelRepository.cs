// <copyright file="PersonReadModelRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Persistence.ReadModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Dapper;
    using LinqKit;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Person.Fields;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Repositories;
    using UBind.Persistence.Extensions;

    /// <inheritdoc />
    public class PersonReadModelRepository : IPersonReadModelRepository
    {
        private readonly IUBindDbContext dbContext;
        private readonly IPolicyReadModelRepository policyReadModelRepository;
        private readonly IClock clock;

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonReadModelRepository"/> class.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="policyReadModelRepository">The policy read model repository.</param>
        /// <param name="clock">A clock for obtaining time.</param>
        public PersonReadModelRepository(
            IUBindDbContext dbContext,
            IPolicyReadModelRepository policyReadModelRepository,
            IClock clock)
        {
            this.dbContext = dbContext;
            this.policyReadModelRepository = policyReadModelRepository;
            this.clock = clock;
        }

        /// <inheritdoc />
        public PersonReadModel? GetPersonAssociatedWithCustomerByEmail(Guid tenantId, Guid organisationId, string email)
        {
            return this.dbContext.PersonReadModels
                .IncludeAllProperties()
                .Where(p => p.CustomerId.HasValue)
                .Where(p => p.TenantId == tenantId)
                .Where(p => p.OrganisationId == organisationId)
                .Where(p => p.Email == email)
                .Where(p => !p.IsDeleted)
                .OrderByDescending(p => p.CreatedTicksSinceEpoch)
                .FirstOrDefault();
        }

        /// <inheritdoc />
        public IEnumerable<PersonReadModel> GetAllPersonAssociatedWithCustomerByEmail(Guid tenantId, string email, DeploymentEnvironment environment)
        {
            var query = from person in this.dbContext.PersonReadModels
                        join customer in this.dbContext.CustomerReadModels on person.CustomerId equals customer.Id
                        where person.TenantId == tenantId
                            && (person.Email == email || person.EmailAddresses.Any(x => x.EmailAddress == email))
                            && !person.IsDeleted && customer.Environment == environment
                        select person;
            query.IncludeAllProperties().OrderByDescending(p => p.CreatedTicksSinceEpoch);

            return query;
        }

        /// <inheritdoc />
        public IEnumerable<PersonReadModel> GetAllPersonsAssociatedWithCustomerByEmailAndOrganisationId(Guid tenantId, Guid organisation, string email, DeploymentEnvironment environment)
        {
            var query = from person in this.dbContext.PersonReadModels
                        join customer in this.dbContext.CustomerReadModels on person.CustomerId equals customer.Id
                        where person.TenantId == tenantId
                            && (person.Email == email || person.EmailAddresses.Any(x => x.EmailAddress == email))
                            && !person.IsDeleted && customer.Environment == environment
                            && person.OrganisationId == organisation
                        select person;
            query.IncludeAllProperties().OrderByDescending(p => p.CreatedTicksSinceEpoch);

            return query;
        }

        public PersonReadModel? GetPersonAssociatedWithUserByEmailAndOrganisationId(Guid tenantId, Guid organisation, string email, DeploymentEnvironment environment)
        {
            var query = from person in this.dbContext.PersonReadModels
                        join customer in this.dbContext.CustomerReadModels on person.CustomerId equals customer.Id
                        where person.TenantId == tenantId
                            && (person.Email == email || person.EmailAddresses.Any(x => x.EmailAddress == email))
                            && !person.IsDeleted && customer.Environment == environment
                            && person.OrganisationId == organisation
                            && person.UserId != null
                        select person;
            return query.IncludeAllProperties().SingleOrDefault();
        }

        public IEnumerable<PersonReadModel> GetPersonsWhichHaveAUserAccountInOrganisationByCustomerId(
            Guid tenantId, Guid organisationId, Guid customerId, DeploymentEnvironment environment)
        {
            return this.dbContext.PersonReadModels
                .Join(
                    this.dbContext.CustomerReadModels,
                    person => person.CustomerId,
                    customer => customer.Id,
                    (person, customer) => new { person, customer })
                .Join(
                    this.dbContext.Users.WhereNotDeleted(),
                    personCustomer => personCustomer.person.Id,
                    user => user.PersonId,
                    (personCustomer, user) => new { personCustomer, user })
                .Where(pc => pc.personCustomer.person.TenantId == tenantId
                    && pc.personCustomer.customer.OrganisationId == organisationId
                    && pc.personCustomer.customer.Id == customerId
                    && pc.personCustomer.customer.Environment == environment
                    && !pc.personCustomer.person.IsDeleted)
                .Select(pc => pc.personCustomer.person);
        }

        public PersonReadModel? GetPersonWhoHasAUserAccountInOrganisationByEmail(
            Guid tenantId, Guid organisationId, string email, DeploymentEnvironment environment)
        {
            return this.dbContext.PersonReadModels
                .Join(
                    this.dbContext.Users.WhereNotDeleted(),
                    person => person.Id,
                    user => user.PersonId,
                    (person, user) => new { person, user })
                .Where(pc => pc.person.TenantId == tenantId
                    && pc.person.OrganisationId == organisationId
                    && pc.person.Email == email
                    && pc.user.Environment == environment
                    && !pc.person.IsDeleted)
                .Select(pc => pc.person)
                .FirstOrDefault();
        }

        /// <inheritdoc />
        public PersonReadModel? GetPersonById(Guid tenantId, Guid personId)
        {
            return this.dbContext.PersonReadModels.IncludeAllProperties()
                .FirstOrDefault(p => p.TenantId == tenantId && p.Id == personId && !p.IsDeleted);
        }

        /// <inheritdoc />
        public PersonReadModel? GetPersonByIdAndOrganisationId(Guid tenantId, Guid organisationId, Guid personId)
        {
            return this.dbContext.PersonReadModels.IncludeAllProperties()
                .FirstOrDefault(p => p.TenantId == tenantId && p.Id == personId && p.OrganisationId == organisationId && !p.IsDeleted);
        }

        /// <inheritdoc />
        public IPersonReadModelSummary? GetPersonSummaryById(Guid tenantId, Guid id)
        {
            var personReadModel = this.dbContext.PersonReadModels
                .IncludeAllProperties()
                .FirstOrDefault(p => p.TenantId == tenantId && p.Id == id && !p.IsDeleted);
            return this.GetSummary(personReadModel);
        }

        /// <inheritdoc />
        public PersonReadModel? GetByIdWithoutTenantIdForMigrations(Guid id)
        {
            return this.dbContext.PersonReadModels.IncludeAllProperties().FirstOrDefault(p => p.Id == id && !p.IsDeleted);
        }

        /// <inheritdoc />
        public IEnumerable<IPersonReadModelSummary?> GetPersonsMatchingFilters(Guid tenantId, EntityListFilters filters)
        {
            var personReadModels = this.Query(tenantId, filters).ToList();
            var personResult = personReadModels.AsQueryable();

            if (!string.IsNullOrEmpty(filters.SortBy))
            {
                personResult = personResult.Order(filters.SortBy, filters.SortOrder);
            }

            return personResult.Paginate(filters).Select(p => this.GetSummary(p)).AsEnumerable();
        }

        /// <inheritdoc />
        public IEnumerable<IPersonReadModelSummary?> GetPersonsAssociatedWithUsersByCustomerId(Guid tenantId, Guid customerId)
        {
            var personReadModels = this.dbContext.PersonReadModels
                .IncludeAllProperties()
                .Where(x => x.TenantId == tenantId && x.CustomerId == customerId)
                .Where(x => x.UserId.HasValue)
                .Where(x => !x.UserIsBlocked)
                .Where(x => !x.IsDeleted)
                .AsEnumerable();
            return personReadModels.Select(p => this.GetSummary(p));
        }

        /// <inheritdoc/>
        public IEnumerable<IPersonReadModelSummary?> GetPersonsByCustomerId(Guid tenantId, Guid customerId, bool includeAllProperties = false)
        {
            var personReadModels = this.dbContext.PersonReadModels
                .Where(x => x.TenantId == tenantId && x.CustomerId == customerId)
                .Where(x => !x.IsDeleted);
            if (includeAllProperties)
            {
                personReadModels = personReadModels.IncludeAllProperties();
            }

            return personReadModels.AsEnumerable().Select(p => this.GetSummary(p));
        }

        /// <inheritdoc/>
        public IEnumerable<PersonReadModel> GetPersonsByUserId(Guid tenantId, Guid userId)
        {
            var personReadModels = this.dbContext.PersonReadModels
                .Where(x => x.TenantId == tenantId && x.UserId == userId)
                .Where(x => !x.IsDeleted)
                .AsEnumerable();
            return personReadModels;
        }

        /// <inheritdoc/>
        public IPersonReadModelWithRelatedEntities? GetPersonWithRelatedEntities(
            Guid tenantId, Guid id, IEnumerable<string> relatedEntities)
        {
            var query = this.CreateQueryForPersonDetailsWithRelatedEntities(relatedEntities);
            return query.FirstOrDefault(q => q.Person.Id == id && q.Person.TenantId == tenantId);
        }

        /// <inheritdoc/>
        public PersonReadModel? GetPersonDetailForOwnerAssignmentByUserId(Guid tenantId, Guid userId)
        {
            var sql = $@"SELECT p.UserId, p.Id, p.FullName
                        FROM UserReadModels u (nolock)
                        INNER JOIN PersonReadModels p (nolock) ON u.PersonId = p.Id 
                        WHERE u.Id = @UserId AND u.TenantId = @TenantId";
            var parameters = new
            {
                UserId = userId,
                TenantId = tenantId,
            };
            return this.dbContext.Database.Connection.QueryFirstOrDefault<PersonReadModel>(sql, parameters);
        }

        /// <inheritdoc/>
        public PersonReadModel? GetPersonAssociatedWithPrimaryPersonByCustmerId(Guid tenantId, Guid customerId)
        {
            string sql = $@"SELECT p.* 
                        FROM CustomerReadModels c (nolock)
                        INNER JOIN PersonReadModels p (nolock) ON c.PrimaryPersonId = p.Id 
                        WHERE c.Id = @CustomerId AND c.TenantId = @TenantId";
            var parameters = new
            {
                CustomerId = customerId,
                TenantId = tenantId,
            };
            return this.dbContext.Database.Connection.QueryFirstOrDefault<PersonReadModel>(sql, parameters);
        }

        private IQueryable<PersonReadModel> Query(Guid tenantId, EntityListFilters filters)
        {
            IQueryable<PersonReadModel> query = this.dbContext.PersonReadModels
                .Where(p => p.TenantId == tenantId)
                .Where(p => !p.IsDeleted);

            if (filters.CustomerId.HasValue)
            {
                query = query.Where(p => p.CustomerId == filters.CustomerId.Value);
            }

            if (filters.OrganisationIds != null && filters.OrganisationIds.Any())
            {
                query = query.Where(p => filters.OrganisationIds.Contains(p.OrganisationId));
            }

            if (filters.SearchTerms.Any())
            {
                var searchExpression = PredicateBuilder.New<PersonReadModel>(false);
                foreach (var personName in filters.SearchTerms)
                {
                    searchExpression.Or(p => p.FullName.Contains(personName) || p.PreferredName.Contains(personName));
                }

                query = query.Where(searchExpression);
            }

            if (filters.DateIsAfterTicks.HasValue && filters.DateFilteringPropertyName != null)
            {
                query = query.Where(ExpressionHelper.GreaterThanExpression<PersonReadModel>(filters.DateFilteringPropertyName, filters.DateIsAfterTicks));
            }

            if (filters.DateIsBeforeTicks.HasValue && filters.DateFilteringPropertyName != null)
            {
                query = query.Where(ExpressionHelper.LessThanExpression<PersonReadModel>(filters.DateFilteringPropertyName, filters.DateIsBeforeTicks));
            }

            return query;
        }

        private IPersonReadModelSummary? GetSummary(PersonReadModel? person)
        {
            if (person == null)
            {
                return null;
            }

            var personReadModel = new PersonReadModelSummary
            {
                TenantId = person.TenantId,
                OrganisationId = person.OrganisationId,
                Id = person.Id,
                CustomerId = person.CustomerId,
                UserId = person.UserId,
                FullName = person.FullName,
                FirstName = person.FirstName,
                LastName = person.LastName,
                NameSuffix = person.NameSuffix,
                NamePrefix = person.NamePrefix,
                MiddleNames = person.MiddleNames,
                PreferredName = person.PreferredName,
                Company = person.Company,
                Title = person.Title,
                Email = person.Email,
                AlternativeEmail = person.AlternativeEmail,
                HomePhoneNumber = person.HomePhoneNumber,
                WorkPhoneNumber = person.WorkPhoneNumber,
                MobilePhoneNumber = person.MobilePhoneNumber,
                Status = this.GetStatus(person),
                CreatedTimestamp = person.CreatedTimestamp,
                LastModifiedTimestamp = person.LastModifiedTimestamp,
                EmailAddresses = person.EmailAddresses
                    .Select(e => new EmailAddressField(e))
                    .OrderBy(o => o.SequenceNo)
                    .ToList(),
                PhoneNumbers = person.PhoneNumbers
                    .Select(e => new PhoneNumberField(e))
                    .OrderBy(o => o.SequenceNo)
                    .ToList(),
                StreetAddresses = person.StreetAddresses
                    .Select(e => new StreetAddressField(e))
                    .ToList(),
                WebsiteAddresses = person.WebsiteAddresses
                    .Select(e => new WebsiteAddressField(e))
                    .OrderBy(o => o.SequenceNo)
                    .ToList(),
                MessengerIds = person.MessengerIds
                    .Select(e => new MessengerIdField(e))
                    .OrderBy(o => o.SequenceNo)
                    .ToList(),
                SocialMediaIds = person.SocialMediaIds
                    .Select(e => new SocialMediaIdField(e))
                    .OrderBy(o => o.SequenceNo)
                    .ToList(),
                UserHasBeenActivated = person.UserHasBeenActivated,
                UserHasBeenInvitedToActivate = person.UserHasBeenInvitedToActivate,
            };

            return this.SetCustomerProperty(personReadModel);
        }

        private IPersonReadModelSummary? SetCustomerProperty(IPersonReadModelSummary person)
        {
            var customer = this.dbContext.CustomerReadModels.FirstOrDefault(p => p.Id == person.CustomerId);

            if (customer != null)
            {
                // owner
                person.OwnerId = customer.OwnerUserId;
                person.OwnerFullName = customer.OwnerFullName;

                // policy
                PolicyReadModelFilters filters = new PolicyReadModelFilters();
                filters.CustomerId = customer.Id;
                filters.TenantId = customer.TenantId;
                filters.OrganisationIds = new Guid[] { customer.OrganisationId };
                filters.Environment = customer.Environment;
                IEnumerable<IPolicyReadModelSummary> policies
                    = this.policyReadModelRepository.ListPolicies(customer.TenantId, filters);
                person.HasActivePolicies = policies.Any(
                    p => PolicyStatus.IssuedOrActive.HasFlag(p.GetPolicyStatus(this.clock.Now())));
            }

            return person;
        }

        private string GetStatus(PersonReadModel person)
        {
            string status = UserStatus.Deactivated.ToString();
            if (!person.UserIsBlocked && !person.UserHasBeenInvitedToActivate)
            {
                status = UserStatus.New.ToString();
            }
            else if (!person.UserIsBlocked && person.UserHasBeenInvitedToActivate && !person.UserHasBeenActivated)
            {
                status = UserStatus.Invited.ToString();
            }
            else if (!person.UserIsBlocked && person.UserHasBeenActivated)
            {
                status = UserStatus.Active.ToString();
            }

            return status;
        }

        private IQueryable<PersonReadModelWithRelatedEntities> CreateQueryForPersonDetailsWithRelatedEntities(
            IEnumerable<string> relatedEntities)
        {
            // Tenant, Organisation, Customer, User is required to be assigned with a default value for LINQ joins to work properly
            // Please see ticket UB-12533
            var query = from person in this.dbContext.PersonReadModels
                        .Where(p => !p.IsDeleted)
                        .IncludeAllProperties()
                        select new PersonReadModelWithRelatedEntities
                        {
                            Person = person,
                            Tenant = default!,
                            TenantDetails = new TenantDetails[] { },
                            Organisation = default!,
                            EmailAddresses = person.EmailAddresses,
                            PhoneNumbers = person.PhoneNumbers,
                            StreetAddresses = person.StreetAddresses,
                            WebsiteAddresses = person.WebsiteAddresses,
                            MessengerIds = person.MessengerIds,
                            SocialMediaIds = person.SocialMediaIds,
                            Customer = default!,
                            User = default!,
                        };

            if (relatedEntities.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.Person.Tenant))))
            {
                query = query.Join(
                    this.dbContext.Tenants,
                    c => c.Person.TenantId,
                    t => t.Id,
                    (c, tenant) => new PersonReadModelWithRelatedEntities
                    {
                        Person = c.Person,
                        Tenant = tenant,
                        TenantDetails = c.TenantDetails,
                        Organisation = c.Organisation,
                        EmailAddresses = c.EmailAddresses,
                        PhoneNumbers = c.PhoneNumbers,
                        StreetAddresses = c.StreetAddresses,
                        WebsiteAddresses = c.WebsiteAddresses,
                        MessengerIds = c.MessengerIds,
                        SocialMediaIds = c.SocialMediaIds,
                        Customer = c.Customer,
                        User = c.User,
                    });
            }

            if (relatedEntities.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.Person.Organisation))))
            {
                query = query.Join(
                    this.dbContext.OrganisationReadModel,
                    c => c.Person.OrganisationId,
                    t => t.Id,
                    (c, organisation) => new PersonReadModelWithRelatedEntities
                    {
                        Person = c.Person,
                        Tenant = c.Tenant,
                        TenantDetails = c.TenantDetails,
                        Organisation = organisation,
                        EmailAddresses = c.EmailAddresses,
                        PhoneNumbers = c.PhoneNumbers,
                        StreetAddresses = c.StreetAddresses,
                        WebsiteAddresses = c.WebsiteAddresses,
                        MessengerIds = c.MessengerIds,
                        SocialMediaIds = c.SocialMediaIds,
                        Customer = c.Customer,
                        User = c.User,
                    });
            }

            if (relatedEntities.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.Person.Customer))))
            {
                query = query.GroupJoin(
                    this.dbContext.CustomerReadModels,
                    p => p.Person.CustomerId,
                    c => c.Id,
                    (c, customer) => new PersonReadModelWithRelatedEntities
                    {
                        Person = c.Person,
                        Tenant = c.Tenant,
                        TenantDetails = c.TenantDetails,
                        Organisation = c.Organisation,
                        EmailAddresses = c.EmailAddresses,
                        PhoneNumbers = c.PhoneNumbers,
                        StreetAddresses = c.StreetAddresses,
                        WebsiteAddresses = c.WebsiteAddresses,
                        MessengerIds = c.MessengerIds,
                        SocialMediaIds = c.SocialMediaIds,
                        Customer = customer.First(),
                        User = c.User,
                    });
            }

            if (relatedEntities.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.Person.User))))
            {
                query = query.GroupJoin(
                    this.dbContext.Users,
                    p => p.Person.Id,
                    c => c.PersonId,
                    (c, user) => new PersonReadModelWithRelatedEntities
                    {
                        Person = c.Person,
                        Tenant = c.Tenant,
                        TenantDetails = c.TenantDetails,
                        Organisation = c.Organisation,
                        EmailAddresses = c.EmailAddresses,
                        PhoneNumbers = c.PhoneNumbers,
                        StreetAddresses = c.StreetAddresses,
                        WebsiteAddresses = c.WebsiteAddresses,
                        MessengerIds = c.MessengerIds,
                        SocialMediaIds = c.SocialMediaIds,
                        Customer = c.Customer,
                        User = user.First(),
                    });
            }

            return query;
        }
    }
}
