// <copyright file="PasswordResetTrackingService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Authentication
{
    using System;
    using System.Linq;
    using NodaTime;
    using UBind.Domain.Entities;
    using UBind.Domain.Extensions;

    /// <inheritdoc/>
    public class PasswordResetTrackingService : IPasswordResetTrackingService
    {
        private readonly IEmailRequestRecordRepository<PasswordResetRecord> resetPasswordRecordRepository;
        private readonly IClock clock;

        /// <summary>
        /// Initializes a new instance of the <see cref="PasswordResetTrackingService"/> class.
        /// </summary>
        /// <param name="resetPasswordRecordRepository">Repository for reset attempt results.</param>
        /// <param name="clock">the clock.</param>
        public PasswordResetTrackingService(
            IEmailRequestRecordRepository<PasswordResetRecord> resetPasswordRecordRepository,
            IClock clock)
        {
            this.clock = clock;
            this.resetPasswordRecordRepository = resetPasswordRecordRepository;
        }

        /// <inheritdoc/>
        public void Record(Guid tenantId, Guid organisationId, string email, string clientIpAddress)
        {
            var loginSucceededEvent = new PasswordResetRecord(
                tenantId, organisationId, email, clientIpAddress, this.clock.Now());
            this.resetPasswordRecordRepository.Insert(loginSucceededEvent);
            this.resetPasswordRecordRepository.SaveChanges();
        }

        /// <inheritdoc/>
        public bool ShouldBlockRequest(
            Guid tenantId,
            Guid organisationId,
            string email,
            int requestPerPeriodBlockingThreshold,
            int periodSizeInMinutes)
        {
            var latestNResetsInOrderOfRecency = this.resetPasswordRecordRepository.GetLatestRecords(
                tenantId, organisationId, email, requestPerPeriodBlockingThreshold);

            if (latestNResetsInOrderOfRecency.Count() < requestPerPeriodBlockingThreshold)
            {
                return false;
            }

            var latestReset = latestNResetsInOrderOfRecency.First();
            var nthPreviousReset = latestNResetsInOrderOfRecency.Last();
            return this.BlockTriggeredByLastRequest(latestReset, nthPreviousReset, periodSizeInMinutes)
                && this.BlockWouldBeOngoing(latestReset, periodSizeInMinutes);
        }

        private bool BlockTriggeredByLastRequest(
            PasswordResetRecord latestReset,
            PasswordResetRecord nthPreviousReset,
            int periodSizeInMinutes)
        {
            var timespanBetweenLastNRequests = latestReset.CreatedTimestamp - nthPreviousReset.CreatedTimestamp;
            return timespanBetweenLastNRequests < Duration.FromMinutes(periodSizeInMinutes);
        }

        private bool BlockWouldBeOngoing(
            PasswordResetRecord latestReset,
            int periodSizeInMinutes)
        {
            var blockExpiryTime = latestReset.CreatedTimestamp.Plus(Duration.FromMinutes(periodSizeInMinutes));
            return this.clock.Now() < blockExpiryTime;
        }
    }
}
