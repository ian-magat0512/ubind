// <copyright file="UserActivationInvitationService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.User
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Threading.Tasks;
    using NodaTime;
    using UBind.Application.Queries.Portal;
    using UBind.Application.Services.SystemEmail;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.User;
    using UBind.Domain.Authentication;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Services;

    /// <inheritdoc />
    public class UserActivationInvitationService : IUserActivationInvitationService
    {
        private readonly IPersonAggregateRepository personAggregateRepository;
        private readonly IUserAggregateRepository userAggregateRepository;
        private readonly IUserReadModelRepository userReadModelRepository;
        private readonly ICustomerReadModelRepository customerReadModelRepository;
        private readonly IPersonReadModelRepository personReadModelRepository;
        private readonly IPasswordHashingService passwordHashingService;
        private readonly IPasswordComplexityValidator passwordComplexityValidator;
        private readonly IOrganisationReadModelRepository organisationReadModelRepository;
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
        private readonly ICachingResolver cachingResolver;
        private readonly IClock clock;
        private readonly ICqrsMediator mediator;
        private readonly ISystemEmailService systemEmailService;
        private readonly IPortalService portalService;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserActivationInvitationService"/> class.
        /// </summary>
        /// <param name="personAggregateRepository">The person aggregate repository.</param>
        /// <param name="userAggregateRepository">The user aggregate repository.</param>
        /// <param name="userReadModelRepository">The user read model repository.</param>
        /// <param name="passwordHashingService">Service for hashing passwords.</param>
        /// <param name="passwordComplexityValidtor">Service for validating password complexity.</param>
        /// <param name="systemEmailService">The System Email Service.</param>
        /// <param name="organisationReadModelRepository">The organisation read model repository.</param>
        /// <param name="httpContextPropertiesResolver">The performing user resolver.</param>
        /// <param name="customerReadModelRepository">The customer read model repository.</param>
        /// <param name="personReadModelRepository">The person read model repository.</param>
        /// <param name="clock">Clock for obtaining current time.</param>
        public UserActivationInvitationService(
            IPersonAggregateRepository personAggregateRepository,
            IUserAggregateRepository userAggregateRepository,
            IUserReadModelRepository userReadModelRepository,
            IPasswordHashingService passwordHashingService,
            IPasswordComplexityValidator passwordComplexityValidtor,
            ISystemEmailService systemEmailService,
            IOrganisationReadModelRepository organisationReadModelRepository,
            IHttpContextPropertiesResolver httpContextPropertiesResolver,
            ICqrsMediator mediator,
            ICachingResolver cachingResolver,
            ICustomerReadModelRepository customerReadModelRepository,
            IPersonReadModelRepository personReadModelRepository,
            IPortalService portalService,
            IClock clock)
        {
            Contract.Assert(personAggregateRepository != null);
            Contract.Assert(userAggregateRepository != null);
            Contract.Assert(userReadModelRepository != null);
            Contract.Assert(passwordHashingService != null);
            Contract.Assert(passwordComplexityValidtor != null);
            Contract.Assert(systemEmailService != null);
            Contract.Assert(organisationReadModelRepository != null);
            Contract.Assert(httpContextPropertiesResolver != null);
            Contract.Assert(clock != null);

            this.personAggregateRepository = personAggregateRepository;
            this.userAggregateRepository = userAggregateRepository;
            this.userReadModelRepository = userReadModelRepository;
            this.passwordHashingService = passwordHashingService;
            this.passwordComplexityValidator = passwordComplexityValidtor;
            this.systemEmailService = systemEmailService;
            this.organisationReadModelRepository = organisationReadModelRepository;
            this.httpContextPropertiesResolver = httpContextPropertiesResolver;
            this.cachingResolver = cachingResolver;
            this.clock = clock;
            this.mediator = mediator;
            this.customerReadModelRepository = customerReadModelRepository;
            this.personReadModelRepository = personReadModelRepository;
            this.portalService = portalService;
        }

        /// <inheritdoc />
        public async Task<UserModel> QueueActivationEmail(
            Guid invitationId,
            UserAggregate userAggregate,
            PersonAggregate personAggregate,
            Domain.Tenant tenant,
            OrganisationReadModel organisation,
            DeploymentEnvironment environment,
            Guid? portalId = null)
        {
            portalId = portalId
                ?? userAggregate.PortalId
                ?? await this.portalService.GetDefaultPortalIdByUserType(
                    userAggregate.TenantId, organisation.Id, userAggregate.PortalUserType);
            var path = $"activate/{userAggregate.Id}?invitationId={invitationId}";
            string link = await this.mediator.Send(
                new GetPortalUrlQuery(userAggregate.TenantId, organisation.Id, portalId, environment, path));
            var userDrop = new UserDrop(
                personAggregate.TenantId,
                Convert.ToString(userAggregate.Environment ?? environment),
                userAggregate.Id,
                personAggregate.Email,
                personAggregate.AlternativeEmail,
                personAggregate.PreferredName,
                personAggregate.FullName,
                personAggregate.NamePrefix,
                personAggregate.FirstName,
                personAggregate.MiddleNames,
                personAggregate.LastName,
                personAggregate.NameSuffix,
                personAggregate.GreetingName,
                personAggregate.Company,
                personAggregate.Title,
                personAggregate.MobilePhone,
                personAggregate.WorkPhone,
                personAggregate.HomePhone,
                userAggregate.Blocked,
                userAggregate.CreatedTimestamp);
            var tenantDrop = new TenantDrop(tenant.Id, tenant.Details.Name, tenant.Details.Alias);
            var userActivationDrop = new EmailInvitationDrop(link, invitationId.ToString());
            var organisationDrop = new OrganisationDrop(organisation.Id, organisation.Alias, organisation.Name);

            var emailDrop = EmailDrop.CreateUserActivationInvitation(
                    userAggregate.TenantId,
                    portalId,
                    userDrop,
                    tenantDrop,
                    organisationDrop,
                    userActivationDrop);

            this.systemEmailService.SendAndPersistAccountActivationInvitationEmail(
                emailDrop, userAggregate);

            return new UserModel(userAggregate, personAggregate);
        }

        /// <inheritdoc />
        public async Task<UserModel> QueueAccountAlreadyActivatedEmail(
            UserAggregate userAggregate,
            PersonAggregate personAggregate,
            Domain.Tenant tenant,
            OrganisationReadModel organisation,
            DeploymentEnvironment environment,
            Guid? portalId = null)
        {
            portalId = portalId
                ?? userAggregate.PortalId
                ?? await this.portalService.GetDefaultPortalIdByUserType(
                    userAggregate.TenantId, organisation.Id, userAggregate.PortalUserType);
            var userDrop = new UserDrop(
                personAggregate.TenantId,
                Convert.ToString(userAggregate.Environment),
                userAggregate.Id,
                personAggregate.Email,
                personAggregate.AlternativeEmail,
                personAggregate.PreferredName,
                personAggregate.FullName,
                personAggregate.NamePrefix,
                personAggregate.FirstName,
                personAggregate.MiddleNames,
                personAggregate.LastName,
                personAggregate.NameSuffix,
                personAggregate.GreetingName,
                personAggregate.Company,
                personAggregate.Title,
                personAggregate.MobilePhone,
                personAggregate.WorkPhone,
                personAggregate.HomePhone,
                userAggregate.Blocked,
                userAggregate.CreatedTimestamp);
            var tenantDrop = new TenantDrop(tenant.Id, tenant.Details.Name, tenant.Details.Alias);
            var organisationDrop = new OrganisationDrop(organisation.Id, organisation.Alias, organisation.Name);

            var emailDrop = EmailDrop.CreateAccountAlreadyActivated(
                userAggregate.TenantId,
                portalId,
                userDrop,
                tenantDrop,
                organisationDrop);

            this.systemEmailService.SendAndPersistAccountAlreadyActivatedEmail(
                emailDrop, userAggregate);

            return new UserModel(userAggregate, personAggregate);
        }

        /// <inheritdoc />
        public async Task<Guid> CreateActivationInvitationAndSendEmail(
            Domain.Tenant tenant,
            Guid organisationId,
            Guid userId,
            DeploymentEnvironment environment,
            Guid? portalId = null)
        {
            var userAggregate = this.userAggregateRepository.GetById(tenant.Id, userId);
            EntityHelper.ThrowIfNotFound(userAggregate, userId);
            portalId = portalId ?? userAggregate.PortalId;

            async Task<Guid> CreateActivationInvitationAsync()
            {
                var newInvitationId = userAggregate.CreateActivationInvitation(
                    this.httpContextPropertiesResolver.PerformingUserId, this.clock.GetCurrentInstant());
                await this.userAggregateRepository.Save(userAggregate);
                return newInvitationId;
            }

            Guid invitationId = await ConcurrencyPolicy.ExecuteWithRetriesAsync(
                CreateActivationInvitationAsync,
                () => userAggregate = this.userAggregateRepository.GetById(tenant.Id, userAggregate.Id));

            var organisation
                = this.organisationReadModelRepository.Get(userAggregate.TenantId, userAggregate.OrganisationId);
            if (organisation == null)
            {
                throw new ErrorException(Errors.Organisation.NotFound(organisationId));
            }

            var personAggregate = this.personAggregateRepository.GetById(tenant.Id, userAggregate.PersonId);
            EntityHelper.ThrowIfNotFound(personAggregate, userAggregate.PersonId, "Person");

            await this.QueueActivationEmail(
                invitationId,
                userAggregate,
                personAggregate,
                tenant,
                organisation,
                environment,
                portalId);

            return invitationId;
        }

        public async Task CreateActivationInvitationAndQueueEmail(
            Guid tenantId, Guid userId, DeploymentEnvironment environment, Guid? portalId = null)
        {
            var userAggregate =
                this.userAggregateRepository.GetById(tenantId, userId) ?? throw new ErrorException(Errors.User.NotFound(userId));
            var invitationId = userAggregate.CreateActivationInvitation(
                this.httpContextPropertiesResolver.PerformingUserId, this.clock.Now());
            await this.userAggregateRepository.Save(userAggregate);

            var personAggregate =
                this.personAggregateRepository.GetById(userAggregate.TenantId, userAggregate.PersonId);
            var tenant = await this.cachingResolver.GetTenantOrThrow(tenantId);
            var organisation = await this.cachingResolver.GetOrganisationOrThrow(tenantId, personAggregate.OrganisationId);

            await this.QueueActivationEmail(
                invitationId,
                userAggregate,
                personAggregate,
                tenant,
                organisation,
                environment,
                portalId);
        }

        public async Task SendAccountAlreadyActivatedEmail(
            Guid tenantId, Guid userId, DeploymentEnvironment environment, Guid? portalId = null)
        {
            var userAggregate =
                this.userAggregateRepository.GetById(tenantId, userId) ?? throw new ErrorException(Errors.User.NotFound(userId));
            await this.userAggregateRepository.Save(userAggregate);

            var personAggregate = this.personAggregateRepository.GetById(userAggregate.TenantId, userAggregate.PersonId);
            var tenant = await this.cachingResolver.GetTenantOrThrow(tenantId);
            var organisation = await this.cachingResolver.GetOrganisationOrThrow(tenantId, personAggregate.OrganisationId);

            await this.QueueAccountAlreadyActivatedEmail(
                userAggregate,
                personAggregate,
                tenant,
                organisation,
                environment,
                portalId);
        }

        /// <inheritdoc/>
        public async Task SetPasswordFromActivation(
            Guid tenantId,
            Guid userId,
            Guid activationInvitationId,
            string cleartextPassword)
        {
            async Task ActivateAndSave()
            {
                var userAggregate = this.userAggregateRepository.GetById(tenantId, userId);
                if (userAggregate == null || userAggregate.TenantId != tenantId)
                {
                    throw new ErrorException(Errors.User.Activation.UserNotFound(userId));
                }

                var complexityValidationResult = this.passwordComplexityValidator.Validate(cleartextPassword);
                if (complexityValidationResult.IsFailure)
                {
                    throw new ErrorException(Errors.User.ChangePassword.TooSimple(complexityValidationResult.Error));
                }

                var saltedHashedPassword = this.passwordHashingService.SaltAndHash(cleartextPassword);
                userAggregate.Activate(
                    activationInvitationId,
                    saltedHashedPassword,
                    this.httpContextPropertiesResolver.PerformingUserId,
                    this.clock.Now());
                await this.userAggregateRepository.Save(userAggregate);
            }

            await ConcurrencyPolicy.ExecuteWithRetriesAsync(ActivateAndSave);
        }

        /// <inheritdoc />
        public void CheckUserActivationInvitationStatus(
            Guid tenantId,
            Guid userId,
            Guid invitationId)
        {
            this.HandleInvalidUserIdentity(userId, tenantId, invitationId);
            var user = this.userAggregateRepository.GetById(tenantId, userId);
            user.VerifyActivationInvitation(invitationId, this.clock.GetCurrentInstant());
        }

        private void HandleInvalidUserIdentity(Guid userId, Guid tenantId, Guid invitationId)
        {
            // User data check
            var userReadModel = this.userReadModelRepository.GetUser(tenantId, userId);
            if (userReadModel == null)
            {
                // Customer data check
                var customerReadModelResult = this.customerReadModelRepository.GetCustomerById(tenantId, userId);
                if (customerReadModelResult != null)
                {
                    // Throw if id is found on CustomerReadModels
                    throw new ErrorException(Errors.User.Activation.CustomerIdProvided(userId, invitationId));
                }

                // Person data check
                var personReadModelResult = this.personReadModelRepository.GetPersonsByCustomerId(tenantId, userId);
                if (customerReadModelResult != null)
                {
                    // Throw if id is found on PersonReadModels
                    throw new ErrorException(Errors.User.Activation.PersonIdProvided(userId, invitationId));
                }

                throw new ErrorException(Errors.User.Activation.UserNotFound(userId));
            }
        }
    }
}
