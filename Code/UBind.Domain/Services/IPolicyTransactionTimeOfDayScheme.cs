// <copyright file="IPolicyTransactionTimeOfDayScheme.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Services
{
    using NodaTime;

    /// <summary>
    /// Specifies the exact times a policy begins and expires, for given inception, expiry and effectivity dates.
    /// </summary>
    public interface IPolicyTransactionTimeOfDayScheme
    {
        /// <summary>
        /// Gets a value indicating whether the policy start time will be set to the transaction created time
        /// if the start date of the policy is the current date.
        /// </summary>
        bool DoesAllowImmediateCoverage { get; }

        /// <summary>
        /// Gets a value indicating whether the policy start time will be set to the default inception time
        /// even if that is a time in the past.
        /// If the start date of the policy is the current date, but the current time of day is past the Inception
        /// time, setting this to true will cause the policy start time to be the default inception time from
        /// this scheme. If it's false, the inception time will be the time of the policy transaction, so that
        /// customers cannot create a policy starting in the past.
        /// </summary>
        bool DoesAllowInceptionTimeInThePast { get; }

        /// <summary>
        /// Gets a value indicating whether to use pre-calculated timestamps for future transaction dates,
        /// rather than relying on a LocalDateTime, which could change by an hour or so due to daylight savings
        /// changes.
        /// This setting is used for policy status but does not have an effect on bulk reporting, where
        /// timestamps are typically used regardless, for performance reasons.
        /// </summary>
        bool AreTimestampsAuthoritative { get; }

        /// <summary>
        /// Gets the time of day that policies start.
        /// Note that if if it's the same day, the inception time cannot be before the
        /// transaction time. You cannot backdate the start date of a policy.
        /// </summary>
        /// <returns>The local time of day that policies start.</returns>
        LocalTime GetInceptionTime();

        /// <summary>
        /// Gets the time of day that policies expire or renewals become effective.
        /// </summary>
        /// <returns>The local time of day that policies expire.</returns>
        LocalTime GetEndTime();

        /// <summary>
        /// Gets the time of day that policy cancellations become effective.
        /// </summary>
        /// <returns>The local time of day that policies cancel.</returns>
        LocalTime GetAdjustmentEffectiveTime();

        /// <summary>
        /// Gets the time of day that policy cancellations become effective.
        /// </summary>
        /// <returns>The local time of day that policies cancel.</returns>
        LocalTime GetCancellationEffectiveTime();

        /// <summary>
        /// Get the precise inception time for a given inception date.
        /// </summary>
        /// <param name="inceptionDate">The inception date.</param>
        /// <returns>The inception time.</returns>
        Instant GetInceptionTimestamp(LocalDate inceptionDate);

        /// <summary>
        /// Gets the precise expiry time for a given expiry date.
        /// </summary>
        /// <param name="expiryDate">The expiry date.</param>
        /// <returns>The expiry time.</returns>
        Instant GetExpiryTimestamp(LocalDate expiryDate);

        /// <summary>
        /// Calculates the effective start or end time as 4pm AET.
        /// </summary>
        /// <param name="effectiveDate">The effective start/end date.</param>
        /// <returns>The exact effective start/end time.</returns>
        Instant GetEffectiveTimestamp(LocalDate effectiveDate);

        /// <summary>
        /// Calculates the cancellation time start or end time as 4pm AET.
        /// </summary>
        /// <param name="cancellationEffectiveDate">The effective start/end date.</param>
        /// <returns>The exact effective start/end time.</returns>
        Instant GetCancellationEffectiveTimestamp(LocalDate cancellationEffectiveDate);
    }
}
