// <copyright file="LoginAttemptTrackingService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Authentication
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NodaTime;
    using UBind.Domain.Aggregates;
    using UBind.Domain.Aggregates.Organisation;
    using UBind.Domain.Entities;
    using UBind.Domain.Events;
    using UBind.Domain.Repositories;

    /// <inheritdoc/>
    public class LoginAttemptTrackingService : ILoginAttemptTrackingService
    {
        private readonly IEmailRequestRecordRepository<LoginAttemptResult> loginAttemptRepository;
        private readonly IEmailAddressBlockingEventRepository emailAddressBlockingEventRepository;
        private readonly IOrganisationAggregateRepository organisationAggregateRepository;
        private readonly IUserLoginEmailRepository userLoginEmailRepository;
        private readonly IUserSystemEventEmitter userSystemEventEmitter;
        private readonly IClock clock;
        private Dictionary<Tuple<Guid, string>, EmailAddressBlockingStatus> blockingStatusByAccountInfo
            = new Dictionary<Tuple<Guid, string>, EmailAddressBlockingStatus>();

        /// <summary>
        /// Initializes a new instance of the <see cref="LoginAttemptTrackingService"/> class.
        /// </summary>
        /// <param name="loginAttemptResultRepository">Repository for login attempt results.</param>
        /// <param name="emailBlockingEventRepository">Repository for email blocking events.</param>
        /// <param name="clock">the clock.</param>
        public LoginAttemptTrackingService(
            IEmailRequestRecordRepository<LoginAttemptResult> loginAttemptResultRepository,
            IEmailAddressBlockingEventRepository emailBlockingEventRepository,
            IOrganisationAggregateRepository organisationAggregateRepository,
            IUserSystemEventEmitter userSystemEventEmitter,
            IUserLoginEmailRepository userLoginEmailRepository,
            IClock clock)
        {
            this.clock = clock;
            this.loginAttemptRepository = loginAttemptResultRepository;
            this.emailAddressBlockingEventRepository = emailBlockingEventRepository;
            this.organisationAggregateRepository = organisationAggregateRepository;
            this.userSystemEventEmitter = userSystemEventEmitter;
            this.userLoginEmailRepository = userLoginEmailRepository;
        }

        /// <inheritdoc/>
        public bool IsLoginAttemptEmailBlocked(Guid tenantId, Guid organisationId, string email)
        {
            return this.GetEmailAddressBlockingStatus(tenantId, organisationId, email).IsEmailAddressBlocked;
        }

        /// <inheritdoc/>
        public void RecordLoginSuccess(
             Guid tenantId, Guid organisationId, string email, string clientIpAddress, Instant timestamp)
        {
            var loginSucceededEvent = LoginAttemptResult.CreateSuccess(
                tenantId, organisationId, email, clientIpAddress, timestamp);
            var lastSixLoginAttemptResults = this.loginAttemptRepository.GetLatestRecords(tenantId, organisationId, email, 6);
            this.loginAttemptRepository.Insert(loginSucceededEvent);

            if (!lastSixLoginAttemptResults.Any(r => r.Succeeded))
            {
                var organisation = this.organisationAggregateRepository.GetById(tenantId, organisationId);

                // Record an unblock event for the Email Address if on the seventh attempt (after timeout), the user has logged in successfully
                var unblockEvent = EmailAddressBlockingEvent.EmailAddressUnblocked(
                    organisation.TenantId,
                    organisationId,
                    email,
                    EmailAddressUnblockedReason.EmailAddressUnblockedDueToSuccessfulAuthenticationAfterTimeout,
                    timestamp);
                this.emailAddressBlockingEventRepository.Insert(unblockEvent);
            }

            this.loginAttemptRepository.SaveChanges();
        }

        /// <inheritdoc/>
        public async Task RecordLoginFailureAndBlockEmailIfNecessary(
            Guid tenantId,
            Guid organisationId,
            string email,
            string clientIpAddress,
            string reason,
            Instant timestamp)
        {
            var blockingRequired = this.GetEmailAddressBlockingStatus(tenantId, organisationId, email)
                .ShouldEmailAddressBeBlockedOnNextFailedLogin;
            if (blockingRequired)
            {
                var organisation = this.organisationAggregateRepository.GetById(tenantId, organisationId);
                var blockingEvent = EmailAddressBlockingEvent.EmailAddressBlocked(
                    organisation.TenantId,
                    organisationId,
                    email,
                    EmailAddressUnblockedReason.EmailAddressBlockedDueToMaximumUnsuccessfulAuthenticationAttempts,
                    timestamp);
                this.emailAddressBlockingEventRepository.Insert(blockingEvent);
            }

            var loginFailedEvent = LoginAttemptResult.CreateFailure(
                tenantId, organisationId, email, clientIpAddress, timestamp, reason);
            this.loginAttemptRepository.Insert(loginFailedEvent);
            this.loginAttemptRepository.SaveChanges();
            var userLogin = this.userLoginEmailRepository.GetUserLoginByEmail(tenantId, organisationId, email);
            if (userLogin == null)
            {
                return;
            }

            var eventTypes = new List<SystemEventType> { SystemEventType.UserLoginAttemptFailed };
            if (blockingRequired)
            {
                eventTypes.Add(SystemEventType.UserEmailAddressBlocked);
            }

            await this.userSystemEventEmitter.CreateAndEmitLoginSystemEvents(tenantId, userLogin.Id, eventTypes, this.clock.GetCurrentInstant());
        }

        private EmailAddressBlockingStatus GetEmailAddressBlockingStatus(Guid tenantId, Guid organisationId, string email)
        {
            var accountInfo = Tuple.Create(organisationId, email);

            if (!this.blockingStatusByAccountInfo.TryGetValue(accountInfo, out EmailAddressBlockingStatus blockingStatus))
            {
                var organisation = this.organisationAggregateRepository.GetById(tenantId, organisationId);
                var lastFiveLoginAttemptResults = this.loginAttemptRepository.GetLatestRecords(organisation.TenantId, organisationId, email, 5);
                var latestBlockingEvent = this.emailAddressBlockingEventRepository.GetLatestBlockingEvent(
                    organisation.TenantId, organisationId, email);
                blockingStatus = new EmailAddressBlockingStatus(
                    lastFiveLoginAttemptResults, latestBlockingEvent, this.clock);
                this.blockingStatusByAccountInfo.Add(accountInfo, blockingStatus);
            }

            return blockingStatus;
        }
    }
}
