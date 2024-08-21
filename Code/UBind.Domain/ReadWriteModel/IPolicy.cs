// <copyright file="IPolicy.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadWriteModel
{
    using NodaTime;

    /// <summary>
    /// Common interface for read and write policy models.
    /// </summary>
    public interface IPolicy
    {
        /// <summary>
        /// Gets a value indicating whether the policy has been adjusted.
        /// </summary>
        bool IsAdjusted { get; }

        /// <summary>
        /// Gets a value indicating whether the policy is date-based.
        /// A date-based policy has an inception date and expiry date.
        /// </summary>
        bool IsTermBased { get; }

        /// <summary>
        /// Gets a value indicating whether timestamp values should be used instead
        /// of date time values for official policy dates.
        /// When set to false, the time zone and daylight savings are taken into account
        /// when calculating the real policy expiry dates and other dates.
        /// </summary>
        bool AreTimestampsAuthoritative { get; }

        /// <summary>
        /// Gets the date the policy first started.
        /// </summary>
        LocalDateTime InceptionDateTime { get; }

        /// <summary>
        /// Gets the timestamp for when the first policy period starts,
        /// calculated in advance.
        /// </summary>
        Instant InceptionTimestamp { get; }

        /// <summary>
        /// Gets the date the policy ends.
        /// </summary>
        LocalDateTime? ExpiryDateTime { get; }

        /// <summary>
        /// Gets the timestamp when the policy expires,
        /// as it was calculated it advance.
        /// </summary>
        Instant? ExpiryTimestamp { get; }

        /// <summary>
        /// Gets the cancellation date.
        /// </summary>
        LocalDateTime? CancellationEffectiveDateTime { get; }

        /// <summary>
        /// Gets the timestamp when the policy cancellation becomes effective,
        /// as it was calculated in advance.
        /// </summary>
        Instant? CancellationEffectiveTimestamp { get; }

        /// <summary>
        /// Gets the last adjustment effective date time.
        /// </summary>
        LocalDateTime? AdjustmentEffectiveDateTime { get; }

        /// <summary>
        /// Gets the timestamp when the policy's last adjustment becomes effective,
        /// as calculated in advance.
        /// </summary>
        Instant? AdjustmentEffectiveTimestamp { get; }

        /// <summary>
        /// Gets the date the latest policy period begins.
        /// </summary>
        LocalDateTime LatestPolicyPeriodStartDateTime { get; }

        /// <summary>
        /// Gets the timestamp when the latest policy period begins,
        /// as calculated in advance.
        /// </summary>
        Instant LatestPolicyPeriodStartTimestamp { get; }

        /// <summary>
        /// Gets the time zone which applies to local dates.
        /// </summary>
        DateTimeZone TimeZone { get; }
    }
}
