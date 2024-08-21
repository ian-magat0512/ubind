// <copyright file="DefaultPolicyTransactionTimeOfDayScheme.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Services
{
    using NodaTime;
    using UBind.Domain;

    /// <summary>
    /// Default policy specifying 4pm AEST as inception, expiry, and effectivity time for policies.
    /// </summary>
    public class DefaultPolicyTransactionTimeOfDayScheme : IPolicyTransactionTimeOfDayScheme
    {
        /// <summary>
        /// Gets a value indicating whether the policy start time will be set to the transaction created time
        /// if the start date of the policy is the current date.
        /// </summary>
        public bool DoesAllowImmediateCoverage { get; } = true;

        /// <summary>
        /// Gets a value indicating whether the policy start time will be set to the default inception time
        /// even if that is a time in the past.
        /// If the start date of the policy is the current date, but the current time of day is past the Inception
        /// time, setting this to true will cause the policy start time to be the default inception time from
        /// this scheme. If it's false, the inception time will be the time of the policy transaction, so that
        /// customers cannot create a policy starting in the past.
        /// </summary>
        public bool DoesAllowInceptionTimeInThePast { get; }

        /// <summary>
        /// Gets a value indicating whether to use pre-calculated timestamps for future transaction dates,
        /// rather than relying on a LocalDateTime, which could change by an hour or so due to daylight savings
        /// changes.
        /// This setting is used for policy status but does not have an effect on bulk reporting, where
        /// timestamps are typically used regardless, for performance reasons.
        /// </summary>
        public bool AreTimestampsAuthoritative { get; }

        public LocalTime GetInceptionTime()
        {
            return new LocalTime(16, 0);
        }

        public LocalTime GetEndTime()
        {
            return new LocalTime(16, 0);
        }

        public LocalTime GetAdjustmentEffectiveTime()
        {
            return new LocalTime(16, 0);
        }

        public LocalTime GetCancellationEffectiveTime()
        {
            return new LocalTime(16, 0);
        }

        /// <summary>
        /// Calculate the inception time as 4pm AET.
        /// </summary>
        /// <param name="inceptionDate">The expiry date.</param>
        /// <returns>The exact expiry time.</returns>
        public Instant GetInceptionTimestamp(LocalDate inceptionDate) => this.GetInstantAt4pmAetOn(inceptionDate);

        /// <summary>
        /// Calculate the cancellation time as 4pm AET.
        /// </summary>
        /// <param name="cancellationDate">The cancellation date.</param>
        /// <returns>The exact cancellation time.</returns>
        public Instant GetCancellationEffectiveTimestamp(LocalDate cancellationDate) => this.GetInstantAt4pmAetOn(cancellationDate);

        /// <summary>
        /// Calculate the expiry time as 4pm AET.
        /// </summary>
        /// <param name="expiryDate">The expiry date.</param>
        /// <returns>The exact cancellation time.</returns>
        public Instant GetExpiryTimestamp(LocalDate expiryDate) => this.GetInstantAt4pmAetOn(expiryDate);

        /// <summary>
        /// Calculates the effective time as 4pm AET.
        /// </summary>
        /// <param name="effectiveDate">The effective date.</param>
        /// <returns>The exact effective time.</returns>
        public Instant GetEffectiveTimestamp(LocalDate effectiveDate) => this.GetInstantAt4pmAetOn(effectiveDate);

        private Instant GetInstantAt4pmAetOn(LocalDate localDate)
        {
            return new LocalDateTime(localDate.Year, localDate.Month, localDate.Day, 16, 0)
                .InZoneLeniently(Timezones.AET)
                .ToInstant();
        }
    }
}
