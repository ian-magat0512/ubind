// <copyright file="UserService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.User
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Threading.Tasks;
    using CSharpFunctionalExtensions;
    using Humanizer;
    using Microsoft.AspNetCore.Http;
    using MoreLinq;
    using NodaTime;
    using UBind.Application.Commands.Customer;
    using UBind.Application.Commands.Person;
    using UBind.Application.ExtensionMethods;
    using UBind.Application.Helpers;
    using UBind.Application.Queries.Customer;
    using UBind.Application.Queries.User;
    using UBind.Application.Services;
    using UBind.Domain;
    using UBind.Domain.Aggregates;
    using UBind.Domain.Aggregates.AdditionalPropertyValue;
    using UBind.Domain.Aggregates.Customer;
    using UBind.Domain.Aggregates.Organisation;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Aggregates.User;
    using UBind.Domain.Authentication;
    using UBind.Domain.Entities;
    using UBind.Domain.Events;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Permissions;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Organisation;
    using UBind.Domain.ReadModel.User;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services;
    using UBind.Domain.Services.AdditionalPropertyValue;

    /// <inheritdoc />
    public class UserService : IUserService
    {
        private readonly ICustomerAggregateRepository customerAggregateRepository;
        private readonly ICustomerService customerService;
        private readonly IUserAggregateRepository userAggregateRepository;
        private readonly IPersonAggregateRepository personRepository;
        private readonly IUserReadModelRepository userReadModelRepository;
        private readonly IPasswordHashingService passwordHashingService;
        private readonly IClock clock;
        private readonly IRoleRepository roleRepository;
        private readonly IQuoteAggregateResolverService quoteAggregateResolverService;
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
        private readonly IOrganisationReadModelRepository organisationReadModelRepository;
        private readonly IUserProfilePictureRepository userProfilePictureRepository;
        private readonly IUserActivationInvitationService userActivationInvitationService;
        private readonly ICqrsMediator mediator;
        private readonly IAdditionalPropertyValueService additionalPropertyValueService;
        private readonly IPasswordComplexityValidator passwordComplexityValidator;
        private readonly IUserLoginEmailRepository userLoginEmailRepository;
        private readonly IUBindDbContext dbContext;
        private readonly ICachingResolver cachingResolver;
        private readonly IAuthenticationMethodReadModelRepository authenticationMethodReadModelRepository;
        private readonly IUserSessionDeletionService userSessionDeletionService;
        private readonly IUserSystemEventEmitter userSystemEventEmitter;

        public UserService(
            IUserAggregateRepository userAggregateRepository,
            ICustomerAggregateRepository customerRepository,
            IPersonAggregateRepository personRepository,
            IUserReadModelRepository userReadModelRepository,
            IRoleRepository roleRepository,
            IUserProfilePictureRepository userProfilePictureRepository,
            IOrganisationReadModelRepository organisationReadModelRepository,
            IUserLoginEmailRepository userLoginEmailRepository,
            IPasswordHashingService passwordHashingService,
            ICustomerService customerService,
            IHttpContextPropertiesResolver httpContextPropertiesResolver,
            IQuoteAggregateResolverService quoteAggregateResolverService,
            IUserActivationInvitationService userActivationInvitationService,
            ICqrsMediator mediator,
            IAdditionalPropertyValueService additionalPropertyValueService,
            IPasswordComplexityValidator passwordComplexityValidator,
            IClock clock,
            IUBindDbContext dbContext,
            ICachingResolver cachingResolver,
            IAuthenticationMethodReadModelRepository authenticationMethodReadModelRepository,
            IUserSessionDeletionService userSessionDeletionService,
            IUserSystemEventEmitter userSystemEventEmitter)
        {
            Contract.Assert(userAggregateRepository != null);
            Contract.Assert(customerRepository != null);
            Contract.Assert(personRepository != null);
            Contract.Assert(userReadModelRepository != null);
            Contract.Assert(passwordHashingService != null);
            Contract.Assert(roleRepository != null);
            Contract.Assert(clock != null);
            Contract.Assert(customerService != null);
            Contract.Assert(httpContextPropertiesResolver != null);
            Contract.Assert(quoteAggregateResolverService != null);
            Contract.Assert(userProfilePictureRepository != null);
            Contract.Assert(organisationReadModelRepository != null);
            Contract.Assert(userActivationInvitationService != null);
            Contract.Assert(passwordComplexityValidator != null);

            this.customerService = customerService;
            this.userAggregateRepository = userAggregateRepository;
            this.customerAggregateRepository = customerRepository;
            this.personRepository = personRepository;
            this.userReadModelRepository = userReadModelRepository;
            this.passwordHashingService = passwordHashingService;
            this.clock = clock;
            this.roleRepository = roleRepository;
            this.httpContextPropertiesResolver = httpContextPropertiesResolver;
            this.quoteAggregateResolverService = quoteAggregateResolverService;
            this.organisationReadModelRepository = organisationReadModelRepository;
            this.userProfilePictureRepository = userProfilePictureRepository;
            this.userActivationInvitationService = userActivationInvitationService;
            this.mediator = mediator;
            this.additionalPropertyValueService = additionalPropertyValueService;
            this.passwordComplexityValidator = passwordComplexityValidator;
            this.userLoginEmailRepository = userLoginEmailRepository;
            this.dbContext = dbContext;
            this.cachingResolver = cachingResolver;
            this.authenticationMethodReadModelRepository = authenticationMethodReadModelRepository;
            this.userSessionDeletionService = userSessionDeletionService;
            this.userSystemEventEmitter = userSystemEventEmitter;
        }

        /// <inheritdoc />
        public IUserReadModelSummary GetUser(Guid tenantId, Guid userId)
        {
            return this.userReadModelRepository.GetUser(tenantId, userId);
        }

        /// <inheritdoc />
        public UserReadModelDetail GetUserDetail(Guid tenantId, Guid userId)
        {
            return this.userReadModelRepository.GetUserDetail(tenantId, userId);
        }

        /// <inheritdoc />
        public async Task<UserAggregate> CreateUser(
            UserSignupModel userSignupModel,
            Guid? authenticationMethodId = null,
            string? externalUserId = null)
        {
            if (userSignupModel.SendActivationInvitation == false
                && userSignupModel.Password != null)
            {
                var complexityValidationResult = this.passwordComplexityValidator.Validate(userSignupModel.Password);
                if (complexityValidationResult.IsFailure)
                {
                    throw new ErrorException(Errors.User.InitialPassword.TooSimple(complexityValidationResult.Error));
                }
            }

            await this.ThrowIfPortalUserTypeMismatch(userSignupModel.TenantId, userSignupModel.PortalId, userSignupModel.UserType);
            if (externalUserId == null)
            {
                this.ThrowIfClientUserEmailAddressIsTaken(userSignupModel.TenantId, userSignupModel.Email, null);
            }

            var userAggregate = await this.CreateUserAndSendActivation(
                userSignupModel, null, authenticationMethodId, externalUserId);
            await this.userAggregateRepository.ApplyChangesToDbContext(userAggregate);

            if (userSignupModel.SendActivationInvitation == false)
            {
                await this.ActivateUser(userAggregate, userSignupModel.Password);
            }

            return userAggregate;
        }

        /// <inheritdoc />
        public async Task<UserModel> CreateUserForQuoteCustomer(
            UserSignupModel user,
            Guid quoteId,
            Guid? productId = null)
        {
            var quoteAggregate = this.quoteAggregateResolverService.GetQuoteAggregateForQuote(user.TenantId, quoteId);
            PersonAggregate? person = null;
            CustomerAggregate? customer = null;

            if (quoteAggregate.HasCustomer
                && quoteAggregate.CustomerId != Guid.Empty
                && quoteAggregate.CustomerId != null)
            {
                customer = this.customerService.GetCustomerAggregateById(quoteAggregate.TenantId, quoteAggregate.CustomerId.Value);
                customer = EntityHelper.ThrowIfNotFound(customer, quoteAggregate.CustomerId.Value);
                async Task UpdatePerson()
                {
                    person = this.personRepository.GetById(customer.TenantId, customer.PrimaryPersonId);
                    person = EntityHelper.ThrowIfNotFound(person, customer.PrimaryPersonId);
                    var userPersonDetails = new PersonalDetails(user);
                    var performingUserId = this.httpContextPropertiesResolver.PerformingUserId;
                    var timestamp = this.clock.GetCurrentInstant();
                    person.Update(userPersonDetails, performingUserId, timestamp);
                    person.AssociateWithCustomer(customer.Id, performingUserId, timestamp);
                    await this.personRepository.Save(person);
                }

                await ConcurrencyPolicy.ExecuteWithRetriesAsync(UpdatePerson);

                person = EntityHelper.ThrowIfNotFound(person, customer.PrimaryPersonId);
                var userDto = await this
                    .EnsureCustomerHasUser(quoteAggregate, customer, person, user);
                return userDto;
            }

            throw new ErrorException(Errors.User.CannotCreateUserForNonExistentCustomer());
        }

        /// <inheritdoc />
        public async Task<UserAggregate> CreateCustomerUserForPersonAndSendActivationInvitation(
            Guid tenantId,
            Guid personId,
            DeploymentEnvironment environment)
        {
            var personAggregate = this.personRepository.GetById(tenantId, personId);
            personAggregate = EntityHelper.ThrowIfNotFound(personAggregate, "PersonAggregate");
            if (personAggregate.CustomerId.IsNullOrEmpty())
            {
                throw new ErrorException(Errors.Person.CustomerNotFound(personId));
            }

            var customerId = personAggregate.CustomerId.ThrowIfNullOrEmpty("CustomerIg");

            var customerAggregate = this.customerAggregateRepository.GetById(personAggregate.TenantId, customerId);
            customerAggregate = EntityHelper.ThrowIfNotFound(customerAggregate, customerId);
            this.ThrowIfCustomerUserEmailAddressIsTaken(
                personAggregate.TenantId, personAggregate.Email, personAggregate.OrganisationId);
            return await this.CreateCustomerUserForPersonAggregate(
                personAggregate, customerAggregate.PortalId, customerAggregate.Environment);
        }

        /// <inheritdoc />
        public async Task<UserAggregate> CreateUserForPerson(
            PersonAggregate personAggregate, CustomerAggregate? customerAggregate = null)
        {
            var performingUserId = this.httpContextPropertiesResolver.PerformingUserId;
            var userId = Guid.NewGuid();

            UserAggregate userAggregate;
            if (personAggregate.UserId.HasValue)
            {
                throw new ErrorException(
                    Errors.Person.CannotCreateAUserAccountForAPersonWithExistingUser(
                        personAggregate.UserId.Value));
            }
            else
            {
                var userLogin = this.userLoginEmailRepository.GetUserLoginByEmail(
                    personAggregate.TenantId, personAggregate.OrganisationId, personAggregate.Email);
                if (userLogin != null)
                {
                    throw new DuplicateUserEmailException(
                        Errors.Customer.EmailAddressInUseByAnotherUser(personAggregate.Email));
                }

                // This if-else statement is needed until we make a refactor on the user where it doesn't subclass
                // person common properties anymore.
                if (personAggregate.CustomerId.HasValue)
                {
                    customerAggregate ??= this.customerAggregateRepository.GetById(personAggregate.TenantId, personAggregate.CustomerId.Value);
                    customerAggregate = EntityHelper.ThrowIfNotFound(customerAggregate, personAggregate.CustomerId.Value);
                    var customerRole = this.roleRepository.GetCustomerRoleForTenant(personAggregate.TenantId);
                    userAggregate = UserAggregate.CreateCustomerUser(
                        personAggregate.TenantId,
                        userId,
                        personAggregate,
                        personAggregate.CustomerId.Value,
                        customerRole,
                        performingUserId,
                        customerAggregate.PortalId,
                        customerAggregate.Environment,
                        this.clock.Now());
                }
                else
                {
                    userAggregate = UserAggregate.CreateUser(
                        personAggregate.TenantId,
                        userId,
                        UserType.Client,
                        personAggregate,
                        performingUserId,
                        null,
                        this.clock.Now());
                }

                personAggregate.RecordUserAccountCreatedForPerson(userId, performingUserId, this.clock.Now());

                // No concurrency exceptions will be thrown when saving a new aggregate.
                await this.userAggregateRepository.Save(userAggregate);
                await this.personRepository.Save(personAggregate);
            }

            return userAggregate;
        }

        public async Task ActivateUser(UserAggregate userAggregate, string? clearTextPassword = null)
        {
            string? saltedHashedPassword = null;
            if (clearTextPassword != null) // we won't have a password if they're signing in via SAML
            {
                var complexityValidationResult = this.passwordComplexityValidator.Validate(clearTextPassword);
                if (complexityValidationResult.IsFailure)
                {
                    throw new ErrorException(Errors.User.ChangePassword.TooSimple(complexityValidationResult.Error));
                }

                saltedHashedPassword = this.passwordHashingService.SaltAndHash(clearTextPassword);
            }

            userAggregate.Activate(
                saltedHashedPassword,
                this.httpContextPropertiesResolver.PerformingUserId,
                this.clock.Now());
            await this.userAggregateRepository.ApplyChangesToDbContext(userAggregate);
        }

        /// <inheritdoc />
        public async Task CreateDefaultUsersForOrganisationAsync(Domain.Tenant tenant, Organisation organisation)
        {
            await this.CreateDefaultUserForOrganisation(
                tenant, organisation, UserType.Client, $"{tenant.Details.Alias}.client.admin@ubind.com.au");
        }

        public async Task<List<Permission>> GetEffectivePermissions(
            UserReadModel user,
            OrganisationReadModel organisation)
        {
            var tenant = await this.cachingResolver.GetTenantOrThrow(user.TenantId);
            List<Permission> excludedPermissions;
            if (tenant.Details.DefaultOrganisationId != user.OrganisationId)
            {
                excludedPermissions = organisation.ExcludedPermissions;

                // NOTE: Hardcoding steadfast-irs in condition. Implemented for UB-7366.
                // Please remove once change for organisation configuration pertaining to permissions is in place.
                if (tenant.Details.Alias.Equals("steadfast-irs"))
                {
                    excludedPermissions = organisation.ExcludedPermissions
                        .Except(new List<Permission>() { Permission.EndorseQuotes }).ToList<Permission>();
                }

                // this is a temporary hardcode condition for ride-protect sub-organisation,
                // to be removed after the implementation of UB-8372
                if (organisation.Alias != "ride-protect")
                {
                    var defaultOrganisationOnlyPermission = Array.FindAll(
                        (Permission[])Enum.GetValues(typeof(Permission)),
                        p =>
                        {
                            var field = typeof(Permission).GetField(p.ToString());
                            return field != null && field.GetCustomAttributes(typeof(PermissionAvailableToDefaultOrganisationUsersOnlyAttribute), false).Any();
                        });
                    excludedPermissions = defaultOrganisationOnlyPermission.Union(excludedPermissions).ToList();
                }
            }
            else
            {
                excludedPermissions = new List<Permission>();
            }

            return user.Roles
                .SelectMany(r => r.Permissions)
                .Except(excludedPermissions)
                .Distinct()
                .ToList();
        }

        /// <inheritdoc />
        public async Task<UserAggregate> Update(
            Guid tenantId,
            Guid userId,
            UserUpdateModel model,
            List<AdditionalPropertyValueUpsertModel>? properties = null)
        {
            var personalDetails = new PersonalDetails(model);
            personalDetails.Validate();
            var performingUserId = this.httpContextPropertiesResolver.PerformingUserId;
            var userAggregate = this.userAggregateRepository.GetById(tenantId, userId);
            userAggregate = EntityHelper.ThrowIfNotFound(userAggregate, userId);
            await this.ThrowIfPortalUserTypeMismatch(tenantId, model.PortalId, userAggregate.UserType.ToEnumOrThrow<UserType>());
            if (userAggregate == null)
            {
                throw new ErrorException(Errors.General.NotFound("User", userId));
            }
            else if (userAggregate.TenantId != tenantId || userAggregate.Id != userId)
            {
                throw new ErrorException(
                    Errors.General.Forbidden(
                        "update a user in another tenancy", "each tenancy must manage their own users"));
            }

            var now = this.clock.GetCurrentInstant();
            if (model.Blocked)
            {
                userAggregate.Block(performingUserId, now);
            }
            else
            {
                userAggregate.Unblock(performingUserId, now);
            }

            if (userAggregate.LoginEmail != model.Email)
            {
                userAggregate.SetLoginEmail(model.Email, performingUserId, now);
            }

            if (properties != null)
            {
                await this.additionalPropertyValueService.UpsertValuesForUser(
                    properties, this.httpContextPropertiesResolver.PerformingUserId, now, userAggregate);
            }

            if (model.PortalId != userAggregate.PortalId)
            {
                userAggregate.ChangePortal(
                    model.PortalId, this.httpContextPropertiesResolver.PerformingUserId, this.clock.Now());
            }

            if (model.AuthenticationMethodId != null && model.ExternalUserId != null)
            {
                // link the identity if it's not already linked
                if (!userAggregate.LinkedIdentities.Any(i => i.AuthenticationMethodId == model.AuthenticationMethodId
                    && i.UniqueId == model.ExternalUserId))
                {
                    userAggregate.LinkIdentity(
                        model.AuthenticationMethodId.Value,
                        model.ExternalUserId,
                        this.httpContextPropertiesResolver.PerformingUserId,
                        this.clock.Now());
                }
            }

            if (model.RoleIds != null)
            {
                this.UpdateUserRoles(userAggregate, model.RoleIds);
            }

            await this.userAggregateRepository.ApplyChangesToDbContext(userAggregate);

            PersonAggregate? personAggregate = null;
            personAggregate = this.personRepository.GetById(userAggregate.TenantId, userAggregate.PersonId);
            personAggregate = EntityHelper.ThrowIfNotFound(personAggregate, userAggregate.PersonId);
            personAggregate.Update(personalDetails, performingUserId, now);
            await this.personRepository.ApplyChangesToDbContext(personAggregate);

            await this.userSystemEventEmitter.CreateAndEmitSystemEvents(
                tenantId,
                userId,
                new List<SystemEventType> { SystemEventType.UserEdited, SystemEventType.UserModified },
                performingUserId);

            return userAggregate;
        }

        /// <inheritdoc />
        public async Task Delete(Guid tenantId, Guid userId)
        {
            var userAggregate = this.userAggregateRepository.GetById(tenantId, userId);
            if (userAggregate != null)
            {
                userAggregate.SoftDelete(this.httpContextPropertiesResolver.PerformingUserId, this.clock.Now());
                await this.userAggregateRepository.ApplyChangesToDbContext(userAggregate);
            }
        }

        /// <inheritdoc />
        public async Task AddRoleToUser(Guid tenantId, Guid userId, Guid roleId)
        {
            this.ThrowIfRemoteIdentityProviderManagesRolesExclusively(tenantId, userId);
            var user = this.userReadModelRepository.GetUser(tenantId, userId);
            var role = this.roleRepository.GetRoleById(tenantId, roleId);

            if (role == null)
            {
                throw new NotFoundException(Errors.General.NotFound("role", roleId));
            }

            if (this.httpContextPropertiesResolver.PerformingUserId == null)
            {
                throw new NotFoundException(Errors.General.Unexpected($"When assigning role to user \"{userId}\", the performing user id was null."));
            }

            // check that this role is one of the assignable roles
            bool assignable = await this.mediator.Send(new CanAssignRoleToUserQuery(
                this.httpContextPropertiesResolver.PerformingUser.GetTenantId(),
                (Guid)this.httpContextPropertiesResolver.PerformingUserId,
                role,
                user));

            if (!assignable)
            {
                throw new ErrorException(Errors.Role.CannotAssignRoleToUser(role.Name, user.FullName));
            }

            async Task AssignRole()
            {
                var userAggregate = this.userAggregateRepository.GetById(tenantId, userId);
                if (userAggregate == null || userAggregate.TenantId != tenantId)
                {
                    throw new UserNotFoundException(Errors.General.NotFound("user", userId));
                }

                userAggregate.AssignRole(role, this.httpContextPropertiesResolver.PerformingUserId, this.clock.Now());
                await this.userAggregateRepository.Save(userAggregate);

                // forces the user to logout and login to refresh permissions in the frontend.
                await this.userSessionDeletionService.DeleteForUser(tenantId, userId);
                this.dbContext.SaveChanges();
            }

            await ConcurrencyPolicy.ExecuteWithRetriesAsync(AssignRole);
        }

        /// <inheritdoc />
        public async Task RemoveUserRole(Guid tenantId, Guid userId, Guid roleId)
        {
            this.ThrowIfRemoteIdentityProviderManagesRolesExclusively(tenantId, userId);
            async Task RetractRole()
            {
                var userAggregate = this.userAggregateRepository.GetById(tenantId, userId);
                if (userAggregate == null || userAggregate.TenantId != tenantId)
                {
                    throw new UserNotFoundException(Errors.General.NotFound("user", userId));
                }

                var role = this.roleRepository.GetRoleById(tenantId, roleId);
                if (role == null)
                {
                    throw new NotFoundException(Errors.General.NotFound("role", roleId));
                }

                this.VerifyAdminRoleCanBeRemoved(tenantId, userId, role);
                userAggregate.RetractRole(role, this.httpContextPropertiesResolver.PerformingUserId, this.clock.Now());
                await this.userAggregateRepository.Save(userAggregate);

                // forces the user to logout and login to refresh permissions in the frontend.
                await this.userSessionDeletionService.DeleteForUser(tenantId, userId);
                this.dbContext.SaveChanges();
            }

            await ConcurrencyPolicy.ExecuteWithRetriesAsync(RetractRole);
        }

        /// <inheritdoc />
        public List<Role> GetUserRoles(Guid tenantId, Guid userId)
        {
            var user = this.GetUser(tenantId, userId);
            return user.Roles.ToList();
        }

        /// <inheritdoc />
        public List<Role> GetAvailableUserRoles(Guid tenantId, Guid userId)
        {
            var userRoles = this.GetUserRoles(tenantId, userId);
            var roles = this.roleRepository.GetRoles(tenantId, new RoleReadModelFilters())
                .Where(role => role.Type != RoleType.Customer);
            var availableRoles = roles.Where(role => !userRoles.Contains(role)).ToList();
            return availableRoles;
        }

        /// <inheritdoc/>
        public async Task<Guid> SaveProfilePictureForUser(
            Guid tenantId, IFormFile formFile, IUserReadModelSummary user)
        {
            var userAggregate = this.userAggregateRepository.GetById(tenantId, user.Id);

            if (userAggregate == null)
            {
                throw new ErrorException(Errors.General.NotFound("User", user.Id));
            }
            else if (userAggregate.TenantId != tenantId || userAggregate.Id != user.Id)
            {
                throw new ErrorException(
                    Errors.General.Forbidden(
                        "update a user in another tenancy", "each tenancy must manage their own users"));
            }

            byte[] pictureData = await FormFileReader.ReadAllFormFileBytes(formFile);

            var newProfilePicture = await this.CreateNewUserProfilePictureAsync(pictureData);

            var now = this.clock.GetCurrentInstant();

            async Task UpdateUserAggregateProfilePictureId()
            {
                if (user.ProfilePictureId.HasValue)
                {
                    var existingProfilePicture = this.userProfilePictureRepository.GetById(user.ProfilePictureId.Value);
                    if (existingProfilePicture != null)
                    {
                        this.userProfilePictureRepository.Delete(existingProfilePicture);
                    }
                }

                userAggregate
                    .SetProfilePictureId(newProfilePicture.Id, this.httpContextPropertiesResolver.PerformingUserId, now);

                await this.userAggregateRepository.Save(userAggregate);
            }

            await ConcurrencyPolicy.ExecuteWithRetriesAsync(
                    UpdateUserAggregateProfilePictureId,
                    () => userAggregate = this.userAggregateRepository.GetById(userAggregate.TenantId, user.Id));

            return newProfilePicture.Id;
        }

        /// <inheritdoc />
        public void ThrowIfClientUserEmailAddressIsTaken(Guid tenantId, string email, Guid? customerId)
        {
            var userReadModel = this.userReadModelRepository.GetInvitedOrActivatedUserByEmailAndTenantId(tenantId, email);
            if (userReadModel != null)
            {
                if (customerId != null)
                {
                    throw new DuplicateUserEmailException(Errors.User.CustomerEmailAddressInUseByAnotherUser(email));
                }
                else
                {
                    throw new DuplicateUserEmailException(Errors.User.UserEmailAddressInUseByAnotherUser(email));
                }
            }
        }

        public async Task ThrowIfPortalUserTypeMismatch(Guid tenantId, Guid? portalId, UserType userType)
        {
            if (portalId.HasValue)
            {
                var portal = await this.cachingResolver.GetPortalOrThrow(tenantId, portalId.Value);
                var portalUserType = userType.ToPortalUserType();
                if (portalUserType != portal.UserType)
                {
                    throw new ErrorException(Errors.User.PortalUserTypeMismatch(
                        portal.Name, portalUserType.Humanize(), portal.UserType.Humanize()));
                }
            }
        }

        /// <inheritdoc/>
        public void ThrowIfCustomerUserEmailAddressIsTaken(Guid tenantId, string email, Guid organisationId)
        {
            var anyUserByEmailAndOrganisationId = this.userReadModelRepository
                .GetInvitedUserByEmailTenantIdAndOrganisationId(tenantId, email, organisationId);
            if (anyUserByEmailAndOrganisationId != null)
            {
                throw new ErrorException(Errors.User.CustomerEmailAddressInUseByAnotherUser(email));
            }

            var anyClientUserByEmailAndTenantId
                = this.userReadModelRepository.GetInvitedClientUserByEmailAndTenantId(tenantId, email);
            if (anyClientUserByEmailAndTenantId != null)
            {
                throw new ErrorException(Errors.User.CustomerEmailAddressInUseByAnotherUser(email));
            }
        }

        private void UpdateUserRoles(UserAggregate userAggregate, IEnumerable<Guid> roleIds)
        {
            var performingUserId = this.httpContextPropertiesResolver.PerformingUserId;
            var now = this.clock.Now();
            var roleIdsToAssign = roleIds.Where(roleId => !userAggregate.RoleIds.Contains(roleId));
            var roleIdsToRetract = userAggregate.RoleIds.Where(roleId => !roleIds.Contains(roleId));
            foreach (var roleId in roleIdsToAssign)
            {
                var role = this.roleRepository.GetRoleById(userAggregate.TenantId, roleId);
                userAggregate.AssignRole(role, performingUserId, now);
            }

            foreach (var roleId in roleIdsToRetract)
            {
                var role = this.roleRepository.GetRoleById(userAggregate.TenantId, roleId);
                userAggregate.RetractRole(role, performingUserId, now);
            }
        }

        private async Task<UserProfilePicture> CreateNewUserProfilePictureAsync(byte[] pictureData)
        {
            return await Task.Run(() =>
            {
                var newProfilePicture = new UserProfilePicture(pictureData);
                this.userProfilePictureRepository.Add(newProfilePicture);
                this.userProfilePictureRepository.SaveChanges();

                return newProfilePicture;
            });
        }

        private void VerifyAdminRoleCanBeRemoved(Guid tenantId, Guid userId, Role role)
        {
            var user = this.GetUser(tenantId, userId);
            var users = role.Users.Where(u => u.TenantId == tenantId);
            if (user.UserStatus == UserStatus.Active
                && (role.IsTenantAdmin || role.IsOrganisationAdmin)
                && role.Users.Count(u => u.TenantId == tenantId
                    && u.OrganisationId == user.OrganisationId
                    && u.UserStatus == UserStatus.Active) <= 1)
            {
                throw new ErrorException(Errors.Role.CannotUnassignRole());
            }
        }

        private async Task<UserModel> CreateDefaultUserForOrganisation(
            Domain.Tenant tenant, Organisation organisation, UserType userType, string email)
        {
            var userId = Guid.NewGuid();
            var userName = $"{organisation.Name} ${userType}";
            var now = this.clock.GetCurrentInstant();
            var personCommonProperties = new PersonCommonProperties
            {
                FullName = userName,
                FirstName = organisation.Name,
                LastName = userType.Humanize(),
                Email = email,
                TenantId = tenant.Id,
                OrganisationId = organisation.Id,
            };
            var performingUserId = this.httpContextPropertiesResolver.PerformingUserId;
            var personDetails = new PersonalDetails(tenant.Id, personCommonProperties);
            var personAggregate = PersonAggregate
                .CreatePersonFromPersonalDetails(tenant.Id, organisation.Id, personDetails, performingUserId, now);
            personAggregate.Update(personDetails, performingUserId, now);
            personAggregate.RecordUserAccountCreatedForPerson(userId, performingUserId, now);

            // No concurrency exception will be thrown when saving new aggregate.
            await this.personRepository.ApplyChangesToDbContext(personAggregate);

            var userAggregate = UserAggregate.CreateUser(personAggregate.TenantId, userId, userType, personAggregate, performingUserId, null, now);

            if (userType == UserType.Client && tenant.Id != Tenant.MasterTenantId)
            {
                var clientAdminRole = this.roleRepository.GetAdminRoleForTenant(tenant.Id);
                userAggregate.AssignRole(clientAdminRole, performingUserId, this.clock.Now());
            }

            var invitationId = userAggregate.CreateActivationInvitation(performingUserId, now);
            var saltedAndHashedPassword = this.passwordHashingService.SaltAndHash("ubindTest123*");
            userAggregate.Activate(invitationId, saltedAndHashedPassword, performingUserId, now);

            // No concurrency exception will be thrown when saving new aggregate.
            await this.userAggregateRepository.ApplyChangesToDbContext(userAggregate);

            return new UserModel(userAggregate, personAggregate);
        }

        /// <summary>
        /// Create user and send activation.
        /// </summary>
        /// <param name="user">User signup model.</param>
        /// <param name="customerId">the associated customer.</param>
        /// <param name="portalId">the portal id.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task<UserAggregate> CreateUserAndSendActivation(
            UserSignupModel user,
            Guid? customerId,
            Guid? authenticationMethodId = null,
            string? externalUserId = null)
        {
            var now = this.clock.GetCurrentInstant();
            var userId = Guid.NewGuid();
            PersonAggregate? person = null;
            CustomerAggregate? customer = null;

            if (customerId.HasValue && customerId != Guid.Empty)
            {
                customer = this.customerService.GetCustomerAggregateById(user.TenantId, customerId.Value);
            }

            var tenant = await this.cachingResolver.GetTenantOrThrow(user.TenantId);
            var performingUserId = this.httpContextPropertiesResolver.PerformingUserId;
            UserAggregate? userAggregate = null;
            if (customer != null)
            {
                // Get the person from the customer
                person = this.personRepository.GetById(customer.TenantId, customer.PrimaryPersonId);
                person = EntityHelper.ThrowIfNotFound(person, customer.PrimaryPersonId);
                var command = new CreateCustomerPersonUserAccountCommand(
                    user.TenantId,
                    user.PortalId,
                    user.Environment,
                    person,
                    person.OrganisationId);
                var personId = await this.mediator.Send(command);

                var userCommand = new GetUserByPersonIdQuery(customer.TenantId, personId);
                var userPerson = await this.mediator.Send(userCommand);

                userAggregate = this.userAggregateRepository.GetById(user.TenantId, userPerson.Id);
                userAggregate = EntityHelper.ThrowIfNotFound(userAggregate, userPerson.Id);
                return userAggregate;
            }
            else
            {
                person = this.CreatePerson(user, tenant.Id, now);
                if (externalUserId == null)
                {
                    this.ThrowIfClientUserEmailAddressIsTaken(tenant.Id, user.Email, customerId);
                }

                // No concurrency exception will be thrown when saving new aggregate.
                await this.personRepository.Save(person);

                var roleIds = new List<Guid>();
                if (user.InitialRoles != null)
                {
                    foreach (var roleId in user.InitialRoles)
                    {
                        var initialRole = this.roleRepository.GetRoleById(tenant.Id, roleId);
                        if (initialRole != null)
                        {
                            roleIds.Add(roleId);
                        }
                    }
                }

                userAggregate = UserAggregate.CreateUser(
                    person.TenantId,
                    userId,
                    user.UserType,
                    person,
                    performingUserId,
                    user.PortalId,
                    now,
                    roleIds.Count > 0 ? roleIds.ToArray() : null);
            }

            if (externalUserId == null && user.SendActivationInvitation.GetValueOrDefault())
            {
                var invitationId = userAggregate.CreateActivationInvitation(performingUserId, now);
                OrganisationReadModel? organisation = this.organisationReadModelRepository.Get(tenant.Id, userAggregate.OrganisationId);
                organisation = EntityHelper.ThrowIfNotFound(organisation, userAggregate.OrganisationId);

                await this.userActivationInvitationService.QueueActivationEmail(
                    invitationId,
                    userAggregate,
                    person,
                    tenant,
                    organisation,
                    user.Environment,
                    user.PortalId);
            }

            if (authenticationMethodId != null && externalUserId != null)
            {
                userAggregate.LinkIdentity(authenticationMethodId.Value, externalUserId, performingUserId, now);
            }

            // ensure person record is updated with user id as well. Retrieve first to ensure that initial events were applied.
            person = this.personRepository.GetById(userAggregate.TenantId, userAggregate.PersonId);
            person = EntityHelper.ThrowIfNotFound(person, userAggregate.PersonId);
            person.RecordUserAccountCreatedForPerson(userId, this.httpContextPropertiesResolver.PerformingUserId, this.clock.Now());
            await this.personRepository.Save(person);

            if (user.Properties != null)
            {
                await this.additionalPropertyValueService.UpsertValuesForUser(
                    user.Properties, this.httpContextPropertiesResolver.PerformingUserId, now, userAggregate);
            }

            // No concurrency exception will be thrown when saving new aggregate.
            await this.userAggregateRepository.Save(userAggregate);

            if (customer != null)
            {
                var associateWithUserCommand
                    = new AssociatePersonWithUserCommand(user.TenantId, customer.Id, userAggregate.Id);
                await this.mediator.Send(associateWithUserCommand);
            }

            return userAggregate;
        }

        // Dev note: This will only work if the person was created from a customer entity (should contain customer Id)
        private async Task<UserAggregate> CreateCustomerUserForPersonAggregate(
            PersonAggregate personAggregate, Guid? portalId, DeploymentEnvironment environment)
        {
            var userId = Guid.NewGuid();
            var performingUserId = this.httpContextPropertiesResolver.PerformingUserId;
            var customerRole = this.roleRepository.GetCustomerRoleForTenant(personAggregate.TenantId);

            // Declare the variable without assignment; Assigned on a try-catch but will be used outside the statement
            UserAggregate? userAggregate = null;
            Guid invitationId;

            if (personAggregate.CustomerId != null)
            {
                var customerHasUserAccountQuery = new CustomerHasUserAccountQuery(personAggregate.TenantId, personAggregate.CustomerId.Value);
                var hasUserAccount = await this.mediator.Send(customerHasUserAccountQuery);
                if (hasUserAccount)
                {
                    // Get the primary person's user aggregate first
                    userAggregate = personAggregate.UserId != null
                        ? this.userAggregateRepository.GetById(personAggregate.TenantId, personAggregate.UserId.Value)
                        : null;
                    if (userAggregate == null)
                    {
                        var nextAvailableUserForCustomer
                            = new GetNextAvailableUserForCustomerQuery(personAggregate.TenantId, personAggregate.CustomerId.Value);
                        var customerUserReadModel = await this.mediator.Send(nextAvailableUserForCustomer);
                        userAggregate = this.userAggregateRepository.GetById(customerUserReadModel.TenantId, customerUserReadModel.Id);
                    }
                }
            }

            if (userAggregate == null && personAggregate.CustomerId != null)
            {
                userAggregate = UserAggregate.CreateCustomerUser(
                    personAggregate.TenantId,
                    userId,
                    personAggregate,
                    personAggregate.CustomerId.Value,
                    customerRole,
                    performingUserId,
                    portalId,
                    environment,
                    this.clock.Now());
                await this.userAggregateRepository.Save(userAggregate);
            }

            // We should have a user aggregate by now, it it's still null then we throw a noty found error
            userAggregate = EntityHelper.ThrowIfNotFound(userAggregate, userId);

            // Assign user id to the newly created person read model record
            personAggregate.RecordUserAccountCreatedForPerson(userId, performingUserId, this.clock.Now());

            userAggregate = this.userAggregateRepository.GetById(userAggregate.TenantId, userAggregate.Id);
            userAggregate = EntityHelper.ThrowIfNotFound(userAggregate, userId);

            invitationId = userAggregate.CreateActivationInvitation(performingUserId, this.clock.Now());

            // No concurrency exceptions can occur when saving a new aggregate.
            await this.userAggregateRepository.Save(userAggregate);
            await this.personRepository.Save(personAggregate);

            var tenant = await this.cachingResolver.GetTenantOrThrow(personAggregate.TenantId);
            var organisation = this.organisationReadModelRepository.Get(tenant.Id, personAggregate.OrganisationId);
            organisation = EntityHelper.ThrowIfNotFound(organisation, personAggregate.OrganisationId);
            await this.userActivationInvitationService.QueueActivationEmail(
                invitationId,
                userAggregate,
                personAggregate,
                tenant,
                organisation,
                environment,
                portalId);

            return userAggregate;
        }

        private PersonAggregate CreatePerson(UserSignupModel user, Guid tenantId, Instant now)
        {
            var personDetails = new PersonalDetails(user);
            personDetails.Validate();
            personDetails.TenantId = tenantId;  // TODO: A better implementation is the injection of tenant newID as part of claims.
            var performingUserId = this.httpContextPropertiesResolver.PerformingUserId;
            var person = PersonAggregate.CreatePersonFromPersonalDetails(
                tenantId, user.OrganisationId, personDetails, performingUserId, now);
            return person;
        }

        private async Task<UserModel> EnsureCustomerHasUser(
            QuoteAggregate quote,
            CustomerAggregate customer,
            PersonAggregate person,
            UserSignupModel user)
        {
            var userLogin = this.userLoginEmailRepository
                .GetUserLoginByEmail(quote.TenantId, quote.OrganisationId, user.Email);
            if (userLogin == null)
            {
                if (string.IsNullOrEmpty(user.Email))
                {
                    throw new ErrorException(Errors.User.CannotCreateIncompleteDetails("email address"));
                }
                else if (string.IsNullOrEmpty(user.FullName))
                {
                    throw new ErrorException(Errors.User.CannotCreateIncompleteDetails("full name"));
                }

                var userAggregate = await this.CreateUserAndSendActivation(user, customer.Id);
                return new UserModel(userAggregate, person);
            }
            else
            {
                var userAggregate = this.userAggregateRepository.GetById(userLogin.TenantId, userLogin.Id);
                if (userAggregate == null)
                {
                    throw new InvalidOperationException("User is not found for the given email.");
                }

                return new UserModel(userAggregate, person);
            }
        }

        /// <summary>
        /// Throws an exception if the remote identity provider manages roles exclusively.
        /// This is a configuration setting on AuthenticationMethods called
        /// "AreRolesManagedExclusivelyByThisIdentityProvider". If it's set to true then roles cannot be manually
        /// assigned or unassigned to the user within the uBind portal. Instead they must be done at the
        /// remote identity provider.
        private void ThrowIfRemoteIdentityProviderManagesRolesExclusively(Guid tenantId, Guid userId)
        {
            var authenticationMethods = this.authenticationMethodReadModelRepository.GetUserLinked(tenantId, userId);
            var authenticationMethod = authenticationMethods?
                .FirstOrDefault(x => x is SamlAuthenticationMethodReadModel samlMethod
                    && samlMethod.AreRolesManagedExclusivelyByThisIdentityProvider);
            if (authenticationMethod != null)
            {
                var user = this.userReadModelRepository.GetUser(tenantId, userId);
                throw new ErrorException(Errors.User.CannotManageUserRolesLocallyDueToExclusiveManagement(
                    authenticationMethod.Name, user.DisplayName));
            }
        }
    }
}
