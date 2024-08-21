// <copyright file="AuthenticateUserCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Commands.Authentication
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Aggregates;
    using UBind.Domain.Aggregates.User;
    using UBind.Domain.Authentication;
    using UBind.Domain.Events;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.User;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services;
    using Errors = Domain.Errors;

    /// <summary>
    /// Command handler for authenticating users.
    /// </summary>
    public class AuthenticateUserCommandHandler : IRequestHandler<AuthenticateUserCommand, UserReadModel>
    {
        private readonly IPasswordHashingService passwordHashingService;
        private readonly IUserLoginEmailRepository userLoginEmailRepository;
        private readonly IUserAggregateRepository userAggregateRepository;
        private readonly IUserReadModelRepository userReadModelRepository;
        private readonly ILoginAttemptTrackingService loginAttemptTrackingService;
        private readonly IOrganisationService organisationService;
        private readonly IUserSystemEventEmitter userSystemEventEmitter;
        private readonly ICachingResolver cachingResolver;
        private readonly IClock clock;
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticateUserCommandHandler"/> class.
        /// </summary>
        /// <param name="passwordHashingService">A service for salting and hashing passwords.</param>
        /// <param name="userLoginEmailRepository">The user login email repository.</param>
        /// <param name="userReadModelRepository">The repository for user read models.</param>
        /// <param name="loginAttemptTrackingService">Service for tracking login attempts and managing blocking.</param>
        /// <param name="organisationService">Service for organisation-related commands and queries.</param>
        /// <param name="clock">A clock.</param>
        public AuthenticateUserCommandHandler(
            IPasswordHashingService passwordHashingService,
            IUserLoginEmailRepository userLoginEmailRepository,
            IUserAggregateRepository userAggregateRepository,
            IUserReadModelRepository userReadModelRepository,
            ILoginAttemptTrackingService loginAttemptTrackingService,
            IOrganisationService organisationService,
            IUserSystemEventEmitter userSystemEventEmitter,
            ICachingResolver cachingResolver,
            IClock clock,
            IHttpContextPropertiesResolver httpContextPropertiesResolver)
        {
            this.passwordHashingService = passwordHashingService;
            this.userLoginEmailRepository = userLoginEmailRepository;
            this.userAggregateRepository = userAggregateRepository;
            this.userReadModelRepository = userReadModelRepository;
            this.loginAttemptTrackingService = loginAttemptTrackingService;
            this.organisationService = organisationService;
            this.userSystemEventEmitter = userSystemEventEmitter;
            this.clock = clock;
            this.cachingResolver = cachingResolver;
            this.httpContextPropertiesResolver = httpContextPropertiesResolver;
        }

        public async Task<UserReadModel> Handle(AuthenticateUserCommand command, CancellationToken cancellationToken)
        {
            await this.organisationService
                .ValidateOrganisationBelongsToTenantAndIsActive(command.TenantId, command.OrganisationId);

            // Get the IP address of the request
            var ipAddress = this.httpContextPropertiesResolver.ClientIpAddress.ToString();

            // Return early if the email is blocked as this does not reveal anything about account existance
            // and will reduce impact of spammed login attempts.
            if (this.loginAttemptTrackingService.IsLoginAttemptEmailBlocked(command.TenantId, command.OrganisationId, command.Email))
            {
                await this.loginAttemptTrackingService.RecordLoginFailureAndBlockEmailIfNecessary(
                    command.TenantId, command.OrganisationId, command.Email, ipAddress, "Email blocked", this.clock.Now());
                throw new ErrorException(Errors.User.Login.AccountLocked());
            }

            // Get the UserLoginEmails that match the email address
            var userLoginEmails = this.userLoginEmailRepository.GetUserLoginsByEmail(command.TenantId, command.Email);
            var didNotMatchEmailAddress = userLoginEmails.None();

            // let's ensure there's at least one password to check.
            // Always test verify something, to prevent latency-based user enumeration exploits.
            // A latency-based enumeration exploit is one where a hacker measures the response time
            // of a login request. If we skip the password verification step because there is no account
            // then a hacker can know when there is and isn't an account, based upon how long it takes for the
            // server to respond. Once they know an account does exist, they can then target it.
            if (didNotMatchEmailAddress)
            {
                userLoginEmails.Add(this.GenerateDummyUserLoginEmail(command.TenantId, command.OrganisationId));
            }

            var tasks = new List<Task<UserLoginEmail?>>();
            foreach (var ule in userLoginEmails)
            {
                tasks.Add(this.ReturnUserLoginEmailIfPasswordMatches(command.PlaintextPassword, ule));
            }

            // We wait for all password checks to complete in parallel so that we don't reveal whether there's
            // one account, many accounts or none based upon the time it takes.
            await Task.WhenAll(tasks);

            // Get the UserLoginEmail that matches the requested organisation, if any
            var matchingUserLoginEmail = tasks.Select(t => t.Result)
                .FirstOrDefault(ule => ule != null && ule.OrganisationId == command.OrganisationId);
            if (matchingUserLoginEmail == null)
            {
                // let's grab the next match then, that doesn't match the requested organisation
                matchingUserLoginEmail = tasks.Select(t => t.Result).FirstOrDefault(ule => ule != null);
            }

            if (matchingUserLoginEmail == null)
            {
                // no matching user login email was found, so record a login failure
                string incorrectEmailOrPasswordDescription = "Incorrect username or password";
                string emailNotExistsDescription = "Account with specified email does not exist.";
                var recordedFailureReason = didNotMatchEmailAddress
                    ? emailNotExistsDescription
                    : incorrectEmailOrPasswordDescription;
                await this.loginAttemptTrackingService.RecordLoginFailureAndBlockEmailIfNecessary(
                    command.TenantId, command.OrganisationId, command.Email, ipAddress, recordedFailureReason, this.clock.Now());
                throw new ErrorException(Errors.User.Login.IncorrectCredentials());
            }

            // if we got here, then we have a successful password match, so we can now do some additional checks
            // without worrying about the time it takes to do them.
            this.loginAttemptTrackingService.RecordLoginSuccess(
                command.TenantId, command.OrganisationId, command.Email, ipAddress, this.clock.Now());
            var user = this.userReadModelRepository.GetUserWithRoles(command.TenantId, matchingUserLoginEmail.Id);
            await this.ValidateUserEnabled(user);
            await this.ValidatePasswordIsNotExpired(user);
            await this.ValidateUserOrganisation(user);
            await this.ValidateUserOrganisationPortal(user);
            return user;
        }

        private async Task ValidateUserOrganisation(UserReadModel user)
        {
            var userOrganisation = await this.cachingResolver.GetOrganisationOrThrow(user.TenantId, user.OrganisationId);
            this.organisationService.ValidateOrganisationIsActive(userOrganisation, user.OrganisationId);
        }

        private async Task ValidateUserOrganisationPortal(UserReadModel user)
        {
            if (user.PortalId != null)
            {
                Guid nonNullablePortalId = user.PortalId.GetValueOrDefault();
                var userOrganisationPortal = await this.cachingResolver.GetPortalOrThrow(user.TenantId, nonNullablePortalId);
                if (userOrganisationPortal.Disabled)
                {
                    throw new ErrorException(Errors.Organisation.Login.OrganisationPortalDisabled(userOrganisationPortal.Name));
                }
            }
        }

        private async Task ValidateUserEnabled(UserReadModel user)
        {
            if (user.IsDisabled)
            {
                await this.userSystemEventEmitter.CreateAndEmitLoginSystemEvents(
                    user,
                    new List<SystemEventType> { SystemEventType.UserLoginAttemptFailed });
                throw new ErrorException(Errors.User.Login.AccountDisabled());
            }
        }

        private async Task ValidatePasswordIsNotExpired(UserReadModel user)
        {
            var isPasswordExpired = false;
            Tenant tenant = await this.cachingResolver.GetTenantOrThrow(user.TenantId);
            if (tenant.Details.PasswordExpiryEnabled)
            {
                var passwordExpiryTimeSpan = TimeSpan.FromDays((double)tenant.Details.MaxPasswordAgeDays);
                var expiryDuration = this.clock.Now() - user.PasswordLastChangedTimestamp;
                isPasswordExpired = expiryDuration.ToTimeSpan() > passwordExpiryTimeSpan;
            }

            if (isPasswordExpired)
            {
                await this.userSystemEventEmitter.CreateAndEmitLoginSystemEvents(
                    user,
                    new List<SystemEventType> { SystemEventType.UserLoginAttemptFailed });
                throw new ErrorException(Errors.User.PasswordExpiry.UserPasswordExpired(user.Id, user.LoginEmail));
            }
        }

        private async Task<UserLoginEmail?> ReturnUserLoginEmailIfPasswordMatches(string plainTextPassword, UserLoginEmail userLoginEmail)
        {
            var saltedHashedPassword = userLoginEmail.SaltedHashedPassword;
            if (saltedHashedPassword == null)
            {
                // try to get it from the aggregate. This is just temporary and this code can be removed after the migration completes.
                var userAggregate = this.userAggregateRepository.GetById(userLoginEmail.TenantId, userLoginEmail.Id);
                saltedHashedPassword = userAggregate?.CurrentSaltedHashedPassword;
            }
            if (saltedHashedPassword == null)
            {
                return null;
            }
            var passwordMatches = await Task.Run(() =>
            {
                return this.passwordHashingService.Verify(plainTextPassword, saltedHashedPassword);
            });
            return passwordMatches
                ? userLoginEmail
                : null;
        }

        private UserLoginEmail GenerateDummyUserLoginEmail(Guid tenantId, Guid organisationId)
        {
            var placeholderHashedPassword = this.passwordHashingService.SaltAndHash(Guid.NewGuid().ToString());
            var dummyUserLoginEmail = new UserLoginEmail(
                tenantId,
            Guid.NewGuid(),
                this.clock.Now(),
                organisationId,
                string.Empty);
            dummyUserLoginEmail.SaltedHashedPassword = placeholderHashedPassword;
            return dummyUserLoginEmail;
        }
    }
}
