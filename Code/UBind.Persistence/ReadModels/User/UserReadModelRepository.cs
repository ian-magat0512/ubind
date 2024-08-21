// <copyright file="UserReadModelRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.ReadModels
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Linq.Expressions;
    using Humanizer;
    using LinqKit;
    using UBind.Domain;
    using UBind.Domain.Entities;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Permissions;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Person.Fields;
    using UBind.Domain.ReadModel.User;
    using UBind.Domain.Repositories;
    using UBind.Persistence.Extensions;

    /// <summary>
    /// Repository for querying users.
    /// </summary>
    public class UserReadModelRepository : IUserReadModelRepository
    {
        private readonly IUBindDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserReadModelRepository"/> class.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        public UserReadModelRepository(IUBindDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        /// <summary>
        /// Gets an expression for instantiating summaries from QuoteReadModels for use in EF projections.
        /// </summary>
        private Expression<Func<UserAndOrganisation, UserReadModelDetail>> DetailSelector =>
            (q) => new UserReadModelDetail
            {
                OrganisationName = q.Organisation.Name,
                OrganisationAlias = q.Organisation.Alias,
                FullName = q.User.FullName,
                NamePrefix = q.User.NamePrefix,
                FirstName = q.User.FirstName,
                MiddleNames = q.User.MiddleNames,
                LastName = q.User.LastName,
                NameSuffix = q.User.NameSuffix,
                Company = q.User.Company,
                Title = q.User.Title,
                PreferredName = q.User.PreferredName,
                Email = q.User.Email,
                AlternativeEmail = q.User.AlternativeEmail,
                MobilePhoneNumber = q.User.MobilePhoneNumber,
                HomePhoneNumber = q.User.HomePhoneNumber,
                WorkPhoneNumber = q.User.WorkPhoneNumber,
                TenantId = q.User.TenantId,
                OrganisationId = q.User.OrganisationId,
                Id = q.User.Id,
                Environment = q.User.Environment,
                PersonId = q.User.PersonId,
                LoginEmail = q.User.LoginEmail,
                CustomerId = q.User.CustomerId,
                UserType = q.User.UserType,
                IsDisabled = q.User.IsDisabled,
                HasBeenInvitedToActivate = q.User.HasBeenInvitedToActivate,
                HasBeenActivated = q.User.HasBeenActivated,
                CreatedTicksSinceEpoch = q.User.CreatedTicksSinceEpoch,
                ProfilePictureId = q.User.ProfilePictureId,
                Roles = q.User.Roles,
                LinkedIdentities = q.User.LinkedIdentities,
                LastModifiedTicksSinceEpoch = q.User.LastModifiedTicksSinceEpoch,
                PortalId = q.User.PortalId,
            };

        /// <inheritdoc/>
        public IEnumerable<UserReadModel> GetUsersMatchingEmailAddressIncludingPlusAddressingForAllTenants(string emailAddress)
        {
            return this.FilterByEmailIncludingPlusAddressing(this.dbContext.Users.WhereNotDeleted(), emailAddress);
        }

        /// <inheritdoc/>
        public IEnumerable<UserReadModel> GetUsersMatchingEmailAddressIncludingPlusAddressing(Guid tenantId, string emailAddress)
        {
            return this.FilterByEmailIncludingPlusAddressing(
                this.dbContext.Users.WhereNotDeleted(u => u.TenantId == tenantId),
                emailAddress);
        }

        public List<UserReadModel> GetByEmailAddress(Guid tenantId, string emailAddress)
        {
            return this.dbContext.Users.WhereNotDeleted(u => u.TenantId == tenantId && u.Email == emailAddress).ToList();
        }

        /// <inheritdoc/>
        public UserReadModelDetail GetUserDetail(Guid tenantId, Guid userId)
        {
            var userDetail = this.dbContext.Users
                .WhereNotDeleted(u => u.TenantId == tenantId && u.Id == userId)
                .IncludeRoles()
                .IncludeLinkedIdentities()
                .Join(
                    this.dbContext.OrganisationReadModel,
                    user => user.OrganisationId,
                    organisation => organisation.Id,
                    (c, o) => new UserAndOrganisation
                    {
                        User = c,
                        Organisation = o,
                    })
                .Select(this.DetailSelector)
                .SingleOrDefault();
            return userDetail;
        }

        /// <inheritdoc/>
        public UserReadModel GetUser(Guid tenantId, Guid userId)
        {
            var user = this.dbContext.Users
               .WhereNotDeleted(u => u.TenantId == tenantId && u.Id == userId)
               .SingleOrDefault();
            return user;
        }

        /// <inheritdoc/>
        public UserReadModel GetUserWithRoles(Guid tenantId, Guid userId)
        {
            var user = this.dbContext.Users
               .WhereNotDeleted(u => u.TenantId == tenantId && u.Id == userId)
               .IncludeRoles()
               .SingleOrDefault();
            return user;
        }

        /// <inheritdoc/>
        public IDictionary<Guid, Guid> GetAllExistingUsersPersonIds(Guid tenantId)
        {
            var userPersonIds = this.dbContext.Users
                .WhereNotDeleted(c => c.TenantId == tenantId && c.PersonId != default)
                .AsEnumerable()
                .Select(p => new KeyValuePair<Guid, Guid>(p.PersonId, p.Id)).ToDictionary(d => d.Key, d => d.Value);

            return userPersonIds;
        }

        /// <inheritdoc/>
        public IQueryable<UserReadModel> GetAllUsersAsQueryable(Guid tenantId)
        {
            return this.dbContext.Users.WhereNotDeleted(u => u.TenantId == tenantId);
        }

        /// <inheritdoc/>
        public IEnumerable<UserReadModel> GetAllUsersByRoleId(Guid tenantId, Guid roleId)
        {
            return this.dbContext.Users.WhereNotDeleted()
                .IncludeRoles()
                .Where(u => u.TenantId == tenantId && u.Roles.Any(role => role.Id == roleId))
                .ToList();
        }

        /// <summary>
        /// Gets users with at least one of the given roles.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="organisationId">The organisation ID.</param>
        /// <param name="roleNames">The role names.</param>
        /// <returns>A list of users.</returns>
        public IEnumerable<UserReadModel> GetUsersWithRoles(
            Guid tenantId,
            Guid organisationId,
            string[] roleNames)
        {
            UserReadModelFilters filters = new UserReadModelFilters
            {
                RoleNames = roleNames,
            };

            var query = this.dbContext.Users.WhereNotDeleted()
                .IncludeRoles();

            if (filters.RoleNames != null && filters.RoleNames.Any())
            {
                var roleNamesPredicate = PredicateBuilder.New<UserReadModel>(false);
                foreach (var roleNameStr in filters.RoleNames)
                {
                    roleNamesPredicate = roleNamesPredicate.Or(user =>
                        user.Roles.Where(r => r.Name == roleNameStr).Any());
                }

                query = query.Where(roleNamesPredicate);
            }

            return query.Where(u => u.TenantId == tenantId && u.OrganisationId == organisationId).ToList();
        }

        public IEnumerable<UserReadModel> GetUserWithAnyOfThePermissions(
            Guid tenantId,
            Guid organisationId,
            Permission[] permissions)
        {
            var query = this.dbContext.Users
                .WhereNotDeleted(u => u.TenantId == tenantId && u.OrganisationId == organisationId);

            var permissionPredicate = PredicateBuilder.New<UserReadModel>(false);
            foreach (var permission in permissions)
            {
                permissionPredicate = permissionPredicate.Or(user =>
                    user.HasPermission(permission));
            }

            return query.ToList().Where(permissionPredicate).ToList();
        }

        /// <inheritdoc/>
        public List<UserReadModel> GetUsers(Guid tenantId, UserReadModelFilters filters)
        {
            return this.QueryUsers(tenantId, filters).ToList();
        }

        /// <inheritdoc/>
        public IUserReadModelWithRelatedEntities GetUserWithRelatedEntities(
            Guid tenantId, Guid userId, IEnumerable<string> relatedEntities)
        {
            var query = this.CreateQueryForUserDetailsWithRelatedEntities(tenantId, relatedEntities);
            return query.FirstOrDefault(q => q.User.Id == userId && q.User.TenantId == tenantId);
        }

        /// <inheritdoc/>
        public IUserReadModelWithRelatedEntities GetUserWithRelatedEntities(Guid tenantId, string email, IEnumerable<string> relatedEntities)
        {
            var query = this.CreateQueryForUserDetailsWithRelatedEntities(tenantId, relatedEntities);
            return query.FirstOrDefault(q => q.User.Email == email && q.User.TenantId == tenantId);
        }

        /// <inheritdoc/>
        public IQueryable<UserReadModelWithRelatedEntities> CreateQueryForUserDetailsWithRelatedEntities(
            Guid tenantId, IEnumerable<string> relatedEntities)
        {
            return this.CreateQueryForUserDetailsWithRelatedEntities(
                tenantId, this.dbContext.Users.WhereNotDeleted().IncludeRoles(), relatedEntities);
        }

        /// <inheritdoc/>
        public UserReadModel GetInvitedOrActivatedUserByEmailAndTenantId(Guid tenantId, string email)
        {
            return this.dbContext.Users.WhereNotDeleted()
                .Where(user => user.TenantId == tenantId)
                .Where(user => user.LoginEmail == email)
                .Where(user => user.HasBeenInvitedToActivate || user.HasBeenActivated)
                .Where(user => !user.IsDisabled)
                .IncludeRoles()
                .FirstOrDefault();
        }

        /// <inheritdoc/>
        public UserReadModel GetInvitedClientUserByEmailAndTenantId(Guid tenantId, string email)
        {
            var customerUserType = UserType.Customer.Humanize();
            return this.dbContext.Users.WhereNotDeleted()
                .Where(user => user.TenantId == tenantId)
                .Where(user => user.LoginEmail == email)
                .Where(user => user.HasBeenInvitedToActivate)
                .Where(user => user.UserType != customerUserType)
                .IncludeRoles()
                .FirstOrDefault();
        }

        /// <inheritdoc/>
        public UserReadModel GetInvitedUserByEmailTenantIdAndOrganisationId(
            Guid tenantId, string email, Guid organisationId)
        {
            return this.dbContext.Users.WhereNotDeleted()
                .Where(user => user.TenantId == tenantId)
                .Where(user => user.OrganisationId == organisationId)
                .Where(user => user.LoginEmail == email)
                .Where(user => user.HasBeenInvitedToActivate)
                .IncludeRoles()
                .FirstOrDefault();
        }

        /// <inheritdoc/>
        public UserReadModel GetUserByPersonId(Guid tenantId, Guid personId)
        {
            return this.dbContext.Users.WhereNotDeleted()
                .IncludeRoles()
                .SingleOrDefault(u => u.TenantId == tenantId && u.PersonId == personId);
        }

        /// <inheritdoc/>
        public List<Guid> GetAllUserIdsBy(Guid tenantId, int skip, int pageSize, Guid? organisationId)
        {
            var queryable = this.dbContext.Users
                .WhereNotDeleted(u => u.TenantId == tenantId);

            if (organisationId.HasValue)
            {
                queryable = queryable.Where(u => u.OrganisationId == organisationId.Value);
            }

            queryable = queryable.OrderByDescending(u => u.CreatedTicksSinceEpoch).Skip(skip).Take(pageSize);

            return queryable.Select(u => u.Id).ToList();
        }

        public IEnumerable<UserReadModelWithRelatedEntities> GetUsersWithRelatedEntities(
            Guid tenantId, UserReadModelFilters filters, IEnumerable<string> relatedEntities)
        {
            var users = this.QueryUsers(tenantId, filters);
            return this.CreateQueryForUserDetailsWithRelatedEntities(tenantId, users, relatedEntities);
        }

        public UserReadModel? GetLinkedUser(Guid tenantId, Guid authenticationMethodId, string userExternalId)
        {
            var user = this.dbContext.Users
                .Join(this.dbContext.UserLinkedIdentities,
                    u => u.Id,
                    uli => uli.UserId,
                    (u, uli) => new { User = u, UserLinkedIdentity = uli })
                .Join(this.dbContext.OrganisationReadModel,
                    t => t.User.OrganisationId,
                    org => org.Id,
                    (t, org) => new { t.User, t.UserLinkedIdentity, Organisation = org })
                .Where(t => t.UserLinkedIdentity.TenantId == tenantId
                    && t.User.TenantId == tenantId
                    && t.UserLinkedIdentity.AuthenticationMethodId == authenticationMethodId
                    && t.UserLinkedIdentity.UniqueId == userExternalId
                    && !t.Organisation.IsDeleted) // Ensuring organisation is not deleted
                .Select(t => t.User)
                .SingleOrDefault();

            return user;
        }

        /// <inheritdoc/>
        public IQueryable<UserReadModel> GetAllUsersByOrganisation(Guid tenantId, Guid organisationId)
        {
            return this.dbContext.Users
                .WhereNotDeleted(u => u.TenantId == tenantId && u.OrganisationId == organisationId);
        }

        /// <inheritdoc/>
        public IQueryable<UserReadModel> GetAllUsersByPortal(Guid tenantId, Guid portalId)
        {
            return this.dbContext.Users
                .Where(u => u.TenantId == tenantId && u.PortalId == portalId);
        }

        /// <inheritdoc/>
        public async Task<List<Guid>> GetAllUserIdsByPortal(Guid tenantId, Guid portalId)
        {
            return await this.dbContext.Users
                .Where(user => user.TenantId == tenantId && user.PortalId == portalId)
                .Select(user => user.Id)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public void SaveChanges()
        {
            this.dbContext.SaveChanges();
        }

        private IQueryable<UserReadModel> QueryUsers(Guid tenantId, UserReadModelFilters filters)
        {
            IQueryable<UserReadModel> query = this.dbContext.Users.WhereNotDeleted();
            var tenant = this.dbContext.Tenants
                .IncludeAllProperties()
                .FirstOrDefault(t => t.Id == tenantId);
            if (tenant == null)
            {
                throw new ErrorException(Errors.Tenant.NotFound());
            }
            else
            {
                query = query.Where(u => u.TenantId == tenantId);
            }

            bool hasOrganisationFilters = filters.OrganisationIds != null && filters.OrganisationIds.Any();
            if (hasOrganisationFilters)
            {
                query = query.Where(u => filters.OrganisationIds.Contains(u.OrganisationId));
            }

            if (filters.RoleNames != null && filters.RoleNames.Any())
            {
                query = query.IncludeRoles();
                var roleNamesPredicate = PredicateBuilder.New<UserReadModel>(false);
                foreach (var roleNameStr in filters.RoleNames)
                {
                    roleNamesPredicate = roleNamesPredicate.Or(user =>
                        user.Roles.Where(r => r.Name == roleNameStr).Any());
                }

                query = query.Where(roleNamesPredicate);
            }

            if (filters.SearchTerms.Any())
            {
                var searchExpression = PredicateBuilder.New<UserReadModel>(false);
                foreach (var searchTerm in filters.SearchTerms)
                {
                    searchExpression.Or(u => u.FullName.Contains(searchTerm) ||
                        u.PreferredName.Contains(searchTerm) ||
                        u.UserType.Contains(searchTerm));
                }

                query = query.Where(searchExpression);
            }

            if (filters.DateIsAfterTicks.HasValue && filters.DateFilteringPropertyName != null)
            {
                query = query.Where(ExpressionHelper.GreaterThanExpression<UserReadModel>(filters.DateFilteringPropertyName, filters.DateIsAfterTicks));
            }

            if (filters.DateIsBeforeTicks.HasValue && filters.DateFilteringPropertyName != null)
            {
                query = query.Where(ExpressionHelper.LessThanExpression<UserReadModel>(filters.DateFilteringPropertyName, filters.DateIsBeforeTicks));
            }

            if (filters.Statuses.Any())
            {
                var statusPredicate = PredicateBuilder.New<UserReadModel>(false);
                foreach (var status in filters.Statuses.Where(x => !string.IsNullOrEmpty(x) && x != "null" && x != null).Select(s => s.ToEnumOrThrow<UserStatus>()))
                {
                    statusPredicate = statusPredicate.Or(this.GetStatusExpression(status));
                }

                query = query.Where(statusPredicate);
            }

            if (filters.Environment != null)
            {
                query = query.Where(user => user.Environment == filters.Environment);
            }

            if (filters.UserTypes != null && filters.UserTypes.Any())
            {
                var userTypePredicate = PredicateBuilder.New<UserReadModel>(false);
                foreach (var userType in filters.UserTypes)
                {
                    userTypePredicate = userTypePredicate.Or(user => user.UserType == userType);
                }

                query = query.Where(userTypePredicate);
            }

            if (!string.IsNullOrEmpty(filters.SortBy))
            {
                query = query.Order(filters.SortBy, filters.SortOrder);
            }

            return query.Paginate(filters);
        }

        private IQueryable<UserReadModelWithRelatedEntities> CreateQueryForUserDetailsWithRelatedEntities(
            Guid tenantId, IQueryable<UserReadModel> dataSource, IEnumerable<string> relatedEntities)
        {
            var query = from user in dataSource
                        where user.TenantId == tenantId
                        select new UserReadModelWithRelatedEntities
                        {
                            User = user,
                            Tenant = default,
                            TenantDetails = new TenantDetails[] { },
                            Organisation = default,
                            Roles = new Role[] { },
                            Person = default,
                            StreetAddresses = new StreetAddressReadModel[] { },
                            PhoneNumbers = new PhoneNumberReadModel[] { },
                            EmailAddresses = new EmailAddressReadModel[] { },
                            SocialMediaIds = new SocialMediaIdReadModel[] { },
                            MessengerIds = new MessengerIdReadModel[] { },
                            WebsiteAddresses = new WebsiteAddressReadModel[] { },
                            Portal = default,
                            TextAdditionalPropertiesValues = new TextAdditionalPropertyValueReadModel[] { },
                            StructuredDataAdditionalPropertyValues = new StructuredDataAdditionalPropertyValueReadModel[] { },
                        };

            if (relatedEntities.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.User.Roles))))
            {
                query = from user in dataSource
                        select new UserReadModelWithRelatedEntities
                        {
                            User = user,
                            Tenant = default,
                            TenantDetails = new TenantDetails[] { },
                            Organisation = default,
                            Roles = user.Roles,
                            Person = default,
                            StreetAddresses = new StreetAddressReadModel[] { },
                            PhoneNumbers = new PhoneNumberReadModel[] { },
                            EmailAddresses = new EmailAddressReadModel[] { },
                            SocialMediaIds = new SocialMediaIdReadModel[] { },
                            MessengerIds = new MessengerIdReadModel[] { },
                            WebsiteAddresses = new WebsiteAddressReadModel[] { },
                            Portal = default,
                            TextAdditionalPropertiesValues = new TextAdditionalPropertyValueReadModel[] { },
                            StructuredDataAdditionalPropertyValues = new StructuredDataAdditionalPropertyValueReadModel[] { },
                        };
            }

            if (relatedEntities.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.User.Tenant))))
            {
                query = query.Join(this.dbContext.Tenants, u => u.User.TenantId, t => t.Id, (u, tenant) => new UserReadModelWithRelatedEntities
                {
                    User = u.User,
                    Tenant = tenant,
                    TenantDetails = tenant.DetailsCollection,
                    Organisation = u.Organisation,
                    Roles = u.Roles,
                    Person = u.Person,
                    StreetAddresses = u.StreetAddresses,
                    PhoneNumbers = u.PhoneNumbers,
                    EmailAddresses = u.EmailAddresses,
                    SocialMediaIds = u.SocialMediaIds,
                    MessengerIds = u.MessengerIds,
                    WebsiteAddresses = u.WebsiteAddresses,
                    Portal = u.Portal,
                    TextAdditionalPropertiesValues = u.TextAdditionalPropertiesValues,
                    StructuredDataAdditionalPropertyValues = u.StructuredDataAdditionalPropertyValues,
                });
            }

            if (relatedEntities.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.User.Organisation))))
            {
                query = query.Join(this.dbContext.OrganisationReadModel, u => u.User.OrganisationId, t => t.Id, (u, organisation) => new UserReadModelWithRelatedEntities
                {
                    User = u.User,
                    Tenant = u.Tenant,
                    TenantDetails = u.TenantDetails,
                    Organisation = organisation,
                    Roles = u.Roles,
                    Person = u.Person,
                    StreetAddresses = u.StreetAddresses,
                    PhoneNumbers = u.PhoneNumbers,
                    EmailAddresses = u.EmailAddresses,
                    SocialMediaIds = u.SocialMediaIds,
                    MessengerIds = u.MessengerIds,
                    WebsiteAddresses = u.WebsiteAddresses,
                    Portal = u.Portal,
                    TextAdditionalPropertiesValues = u.TextAdditionalPropertiesValues,
                    StructuredDataAdditionalPropertyValues = u.StructuredDataAdditionalPropertyValues,
                });
            }

            if (relatedEntities.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.User.Person))))
            {
                query = query.Join(this.dbContext.PersonReadModels.IncludeAllProperties(), u => u.User.PersonId, p => p.Id, (u, person) => new UserReadModelWithRelatedEntities
                {
                    User = u.User,
                    Tenant = u.Tenant,
                    TenantDetails = u.TenantDetails,
                    Organisation = u.Organisation,
                    Roles = u.Roles,
                    Person = person,
                    StreetAddresses = person.StreetAddresses,
                    PhoneNumbers = person.PhoneNumbers,
                    EmailAddresses = person.EmailAddresses,
                    SocialMediaIds = person.SocialMediaIds,
                    MessengerIds = person.MessengerIds,
                    WebsiteAddresses = person.WebsiteAddresses,
                    Portal = u.Portal,
                    TextAdditionalPropertiesValues = u.TextAdditionalPropertiesValues,
                    StructuredDataAdditionalPropertyValues = u.StructuredDataAdditionalPropertyValues,
                });
            }

            if (relatedEntities.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.User.Portal))))
            {
                query = query.GroupJoin(this.dbContext.PortalReadModels, u => u.User.PortalId, p => p.Id, (u, portal) => new UserReadModelWithRelatedEntities
                {
                    User = u.User,
                    Tenant = u.Tenant,
                    TenantDetails = u.TenantDetails,
                    Organisation = u.Organisation,
                    Roles = u.Roles,
                    Person = u.Person,
                    StreetAddresses = u.StreetAddresses,
                    PhoneNumbers = u.PhoneNumbers,
                    EmailAddresses = u.EmailAddresses,
                    SocialMediaIds = u.SocialMediaIds,
                    MessengerIds = u.MessengerIds,
                    WebsiteAddresses = u.WebsiteAddresses,
                    Portal = portal.FirstOrDefault(),
                    TextAdditionalPropertiesValues = u.TextAdditionalPropertiesValues,
                    StructuredDataAdditionalPropertyValues = u.StructuredDataAdditionalPropertyValues,
                });
            }

            if (relatedEntities.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.User.AdditionalProperties))))
            {
                query = query.GroupJoin(this.dbContext.TextAdditionalPropertValues, u => u.User.Id, p => p.EntityId, (u, apv) => new UserReadModelWithRelatedEntities
                {
                    User = u.User,
                    Tenant = u.Tenant,
                    TenantDetails = u.TenantDetails,
                    Organisation = u.Organisation,
                    Roles = u.Roles,
                    Person = u.Person,
                    StreetAddresses = u.StreetAddresses,
                    PhoneNumbers = u.PhoneNumbers,
                    EmailAddresses = u.EmailAddresses,
                    SocialMediaIds = u.SocialMediaIds,
                    MessengerIds = u.MessengerIds,
                    WebsiteAddresses = u.WebsiteAddresses,
                    Portal = u.Portal,
                    TextAdditionalPropertiesValues = apv.Where(df => !df.AdditionalPropertyDefinition.IsDeleted),
                    StructuredDataAdditionalPropertyValues = u.StructuredDataAdditionalPropertyValues,
                })
                .GroupJoin(this.dbContext.StructuredDataAdditionalPropertyValues, u => u.User.Id, p => p.EntityId, (u, apv) => new UserReadModelWithRelatedEntities
                {
                    User = u.User,
                    Tenant = u.Tenant,
                    TenantDetails = u.TenantDetails,
                    Organisation = u.Organisation,
                    Roles = u.Roles,
                    Person = u.Person,
                    StreetAddresses = u.StreetAddresses,
                    PhoneNumbers = u.PhoneNumbers,
                    EmailAddresses = u.EmailAddresses,
                    SocialMediaIds = u.SocialMediaIds,
                    MessengerIds = u.MessengerIds,
                    WebsiteAddresses = u.WebsiteAddresses,
                    Portal = u.Portal,
                    TextAdditionalPropertiesValues = u.TextAdditionalPropertiesValues,
                    StructuredDataAdditionalPropertyValues = apv.Where(df => !df.AdditionalPropertyDefinition.IsDeleted),
                });
            }

            return query;
        }

        private IEnumerable<UserReadModel> FilterByEmailIncludingPlusAddressing(IQueryable<UserReadModel> users, string email)
        {
            var userName = email.Substring(0, email.IndexOf("@"));
            var domain = email.Substring(email.IndexOf("@"), email.Length - email.IndexOf("@"));

            var filteredUsers = users.Where(u => (u.LoginEmail == email) || (u.LoginEmail.StartsWith(userName + "+") && u.LoginEmail.EndsWith(domain))).Take(1000);
            return filteredUsers;
        }

        private Expression<Func<UserReadModel, bool>> GetStatusExpression(UserStatus status)
        {
            if (status == UserStatus.New)
            {
                return user =>
                    !user.IsDisabled &&
                    !user.HasBeenInvitedToActivate &&
                    !user.HasBeenActivated;
            }

            if (status == UserStatus.Invited)
            {
                return user =>
                    !user.IsDisabled &&
                    user.HasBeenInvitedToActivate &&
                    !user.HasBeenActivated;
            }

            if (status == UserStatus.Active)
            {
                return user =>
                    !user.IsDisabled &&
                    user.HasBeenActivated;
            }

            if (status == UserStatus.Deactivated)
            {
                return user => user.IsDisabled;
            }

            throw new InvalidOperationException("Unknown user status " + status.ToString());
        }

        private class UserAndOrganisation
        {
            public UserReadModel User { get; set; }

            public OrganisationReadModel Organisation { get; set; }
        }
    }
}
