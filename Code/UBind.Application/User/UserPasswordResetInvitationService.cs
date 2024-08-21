// <copyright file="UserPasswordResetInvitationService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.User
{
    using System;
    using System.ComponentModel;
    using System.Threading.Tasks;
    using CSharpFunctionalExtensions;
    using Microsoft.Extensions.Logging;
    using NodaTime;
    using UBind.Application.Queries.Portal;
    using UBind.Application.Services;
    using UBind.Application.Services.SystemEmail;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.User;
    using UBind.Domain.Authentication;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Repositories;

    /// <inheritdoc />
    public class UserPasswordResetInvitationService : IUserPasswordResetInvitationService
    {
        private readonly IUserLoginEmailRepository userLoginEmailRepository;
        private readonly IUserAggregateRepository userAggregateRepository;
        private readonly IUserReadModelRepository userReadModelRepository;
        private readonly IPersonAggregateRepository personRepository;
        private readonly IPasswordHashingService passwordHashingService;
        private readonly IPasswordComplexityValidator passwordComplexityValidator;
        private readonly IPasswordReuseValidator passwordReuseValidator;
        private readonly ISystemEmailService systemEmailService;
        private readonly IClock clock;
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
        private readonly ICqrsMediator mediator;
        private readonly IUserSessionDeletionService userSessionDeletionService;
        private readonly ICachingResolver cachingResolver;
        private readonly ILogger<UserPasswordResetInvitationService> logger;
        private readonly IUserActivationInvitationService userActivationInvitationService;

        public UserPasswordResetInvitationService(
            ISystemEmailService systemEmailService,
            IUserLoginEmailRepository userLoginEmailRepository,
            IUserAggregateRepository userAggregateRepository,
            IUserReadModelRepository userReadModelRepository,
            IPersonAggregateRepository personRepository,
            IPasswordHashingService passwordHashingService,
            IPasswordComplexityValidator passwordComplexityValidator,
            IPasswordReuseValidator passwordReuseValidator,
            IHttpContextPropertiesResolver httpContextPropertiesResolver,
            ICqrsMediator mediator,
            IUserSessionDeletionService userSessionDeletionService,
            IClock clock,
            ICachingResolver cachingResolver,
            ILogger<UserPasswordResetInvitationService> logger,
            IUserActivationInvitationService userActivationInvitationService)
        {
            this.userLoginEmailRepository = userLoginEmailRepository;
            this.systemEmailService = systemEmailService;
            this.userAggregateRepository = userAggregateRepository;
            this.userReadModelRepository = userReadModelRepository;
            this.personRepository = personRepository;
            this.passwordHashingService = passwordHashingService;
            this.passwordComplexityValidator = passwordComplexityValidator;
            this.passwordReuseValidator = passwordReuseValidator;
            this.clock = clock;
            this.httpContextPropertiesResolver = httpContextPropertiesResolver;
            this.mediator = mediator;
            this.userSessionDeletionService = userSessionDeletionService;
            this.cachingResolver = cachingResolver;
            this.logger = logger;
            this.userActivationInvitationService = userActivationInvitationService;
        }

        /// <inheritdoc />
        public int PasswordResetTrackingPeriodInMinutes { get; set; } = 30;

        /// <inheritdoc />
        public int PasswordResetRequestBlockingThreshold { get; set; } = 6;

        /// <inheritdoc />
        public void CheckPasswordResetInvitationStatus(Guid tenantId, Guid userId, Guid invitationId)
        {
            var user = this.userAggregateRepository.GetById(tenantId, userId);
            EntityHelper.ThrowIfNotFound(user, userId, "user");
            user.VerifyPasswordResetInvitation(invitationId, this.clock.Now());
        }

        /// <inheritdoc />
        [DisplayName("Create and send password reset invitation | {2}")]
        public async Task<Result<Guid, Error>> CreateAndSendPasswordResetInvitation(
            Guid tenantId,
            Guid organisationId,
            string emailAddress,
            DeploymentEnvironment environment,
            bool isPasswordExpired,
            Guid? portalId = null,
            Guid? productId = null)
        {
            var tenant = await this.cachingResolver.GetTenantOrThrow(tenantId);
            Product product = null;
            if (productId.HasValue)
            {
                product = await this.cachingResolver.GetProductOrThrow(tenant.Id, productId.Value);
            }

            // Get the UserReadModels that match the email address
            var userReadModels = this.userReadModelRepository.GetByEmailAddress(tenant.Id, emailAddress);

            // Get the UserReadModel that matches the requested organisation, if any
            var matchingUser = userReadModels
                .Where(u => u.OrganisationId == organisationId).FirstOrDefault();
            if (matchingUser == null)
            {
                // let's grab the next match then, that doesn't match the requested organisation
                matchingUser = userReadModels.FirstOrDefault(u => !u.IsDisabled);
            }

            if (matchingUser == null)
            {
                var disabledUser = userReadModels.FirstOrDefault(u => u.IsDisabled);
                if (disabledUser != null)
                {
                    this.logger.LogError(
                        "When trying to reset the password for a user with the email address \"{0}\" for tenant \"{1}\", "
                        + "only a disabled user with that email address was found. No password reset invitation will be sent.",
                        emailAddress,
                        tenant.Details.Name);
                    return Result.Failure<Guid, Error>(Errors.User.RequestResetPassword.UserDisabled(
                        emailAddress,
                        tenant.Details.Alias));
                }
                else
                {
                    this.logger.LogError(
                        "When trying to reset the password for a user with the email address \"{0}\" for tenant \"{1}\", "
                        + "no user with that email address was found. No password reset invitation will be sent.",
                        emailAddress,
                        tenant.Details.Name);
                    return Result.Failure<Guid, Error>(Errors.User.RequestResetPassword.UserNotFound(
                        emailAddress,
                        tenant.Details.Alias));
                }
            }

            if (!matchingUser.HasBeenActivated)
            {
                // let's send them an activation invitation email instead
                this.logger.LogWarning(
                    "When trying to reset the password for a user with the email address \"{0}\" for tenant \"{1}\", "
                    + "the user \"{2}\" with ID \"{3}\" was found, but has not yet been activated. We'll send an activation "
                    + "invitation instead.",
                    emailAddress,
                    tenant.Details.Name,
                    matchingUser.DisplayName,
                    matchingUser.Id);
                var activationInvitationId = await this.userActivationInvitationService.CreateActivationInvitationAndSendEmail(
                    tenant,
                    organisationId,
                    matchingUser.Id,
                    environment,
                    portalId);
                return Result.Success<Guid, Error>(activationInvitationId);
            }

            var organisation = await this.cachingResolver.GetOrganisationOrThrow(tenant.Id, matchingUser.OrganisationId);
            var userLogin = this.userLoginEmailRepository.GetUserLoginByEmail(tenantId, organisationId, emailAddress);
            var userAggregate = this.userAggregateRepository.GetById(tenantId, userLogin.Id);
            EntityHelper.ThrowIfNotFound(userAggregate, userLogin.Id, "UserAggregate");
            var person = this.personRepository.GetById(tenantId, userAggregate.PersonId);
            var invitationId = userAggregate.CreatePasswordResetInvitation(
                this.httpContextPropertiesResolver.PerformingUserId, this.clock.GetCurrentInstant());
            await this.userAggregateRepository.Save(userAggregate);
            string path = string.Format($"login/reset-password/{userAggregate.Id}?invitationId={invitationId}");
            portalId = portalId
                ?? userAggregate.PortalId
                ?? await this.mediator.Send(new GetDefaultPortalIdQuery(
                    tenant.Id, userAggregate.OrganisationId, userAggregate.PortalUserType));
            string link = await this.mediator.Send(
                new GetPortalUrlQuery(userAggregate.TenantId, organisation.Id, userAggregate.PortalId, environment, path));
            var userDrop = new UserDrop(
                person.TenantId,
                Convert.ToString(userAggregate.Environment),
                userAggregate.Id,
                person.Email,
                person.AlternativeEmail,
                person.PreferredName,
                person.FullName,
                person.NamePrefix,
                person.FirstName,
                person.MiddleNames,
                person.LastName,
                person.NameSuffix,
                person.GreetingName,
                person.Company,
                person.Title,
                person.MobilePhone,
                person.WorkPhone,
                person.HomePhone,
                userAggregate.Blocked,
                userAggregate.CreatedTimestamp);
            var tenantDrop = new TenantDrop(tenant.Id, tenant.Details.Name, tenant.Details.Alias);
            var passwordResetDrop = new EmailInvitationDrop(link, invitationId.ToString());
            var organisationDrop = new OrganisationDrop(organisation.Id, organisation.Alias, organisation.Name);
            var emailDrop = this.GetEmailDropForPasswordInvitation(
                isPasswordExpired,
                product?.Id,
                userAggregate,
                userDrop,
                tenantDrop,
                organisationDrop,
                passwordResetDrop,
                portalId);
            this.systemEmailService.SendAndPersistPasswordResetInvitationEmail(
                emailDrop, userAggregate);
            return Result.Success<Guid, Error>(invitationId);
        }

        /// <inheritdoc />
        public async Task SetPasswordFromPasswordReset(
            Guid tenantId, Guid userId, Guid invitationId, string cleartextPassword)
        {
            var userAggregate = this.userAggregateRepository.GetById(tenantId, userId);
            EntityHelper.ThrowIfNotFound(userAggregate, userId, "user");
            var complexityValidationResult = this.passwordComplexityValidator.Validate(cleartextPassword);
            if (complexityValidationResult.IsFailure)
            {
                throw new ErrorException(Errors.User.ChangePassword.TooSimple(complexityValidationResult.Error));
            }

            userAggregate.ChangePassword(
                invitationId,
                cleartextPassword,
                this.clock.Now(),
                this.passwordHashingService,
                this.passwordReuseValidator,
                this.httpContextPropertiesResolver.PerformingUserId);
            await this.userAggregateRepository.Save(userAggregate);

            // remove all other sessions for that user.
            await this.userSessionDeletionService.DeleteForUser(
                userAggregate.TenantId,
                userAggregate.Id);
        }

        private EmailDrop GetEmailDropForPasswordInvitation(
            bool isPasswordExpired,
            Guid? productId,
            UserAggregate userAggregate,
            UserDrop userDrop,
            TenantDrop tenantDrop,
            OrganisationDrop organisationDrop,
            EmailInvitationDrop emailInvitationDrop,
            Guid? portalId)
        {
            if (isPasswordExpired)
            {
                return EmailDrop.CreatePasswordExpiredResetInvition(
                    userAggregate.TenantId,
                    productId,
                    portalId,
                    userDrop,
                    tenantDrop,
                    organisationDrop,
                    emailInvitationDrop);
            }
            else
            {
                return EmailDrop.CreatePasswordResetInvition(
                     userAggregate.TenantId,
                     portalId,
                     userDrop,
                     tenantDrop,
                     organisationDrop,
                     emailInvitationDrop);
            }
        }
    }
}
