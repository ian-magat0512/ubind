// <copyright file="PolicyExtensions.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Extensions
{
    using System;
    using System.Collections.Generic;
    using NodaTime;
    using UBind.Domain.ReadWriteModel;

    /// <summary>
    /// Extension methods on the common policy interface (for sharing functionality between read and write models).
    /// </summary>
    public static class PolicyExtensions
    {
        /// <summary>
        /// Calculate's a policy's status at a given time.
        /// </summary>
        /// <param name="policy">The policy.</param>
        /// <param name="time">The time to calculate the policy's status for.</param>
        /// <returns>The policy status.</returns>
        public static PolicyStatus GetPolicyStatus(this IPolicy policy, Instant time)
        {
            if ((policy.CancellationEffectiveTimestamp.HasValue && policy.CancellationEffectiveTimestamp != default) &&
                ((policy.CancellationEffectiveTimestamp < time) ||
                    (policy.CancellationEffectiveTimestamp == policy.InceptionTimestamp)))
            {
                return PolicyStatus.Cancelled;
            }

            if (policy.IsAdjusted)
            {
                return PolicyStatus.Adjusted;
            }

            if (policy.InceptionTimestamp > time)
            {
                return PolicyStatus.Issued;
            }

            if (policy.ExpiryTimestamp > time)
            {
                return PolicyStatus.Active;
            }

            return PolicyStatus.Expired;
        }

        /// <summary>
        /// Calculate's a policy's state at a given time.
        /// </summary>
        /// <param name="policy">The policy.</param>
        /// <param name="timestamp">The time to calculate the policy's status for.</param>
        /// <returns>The policy status.</returns>
        public static PolicyStatus GetPolicyState(this IPolicy policy, Instant timestamp)
        {
            if (policy.CancellationEffectiveTimestamp.HasValue &&
                ((policy.CancellationEffectiveTimestamp < timestamp) ||
                    (policy.CancellationEffectiveTimestamp == policy.InceptionTimestamp)))
            {
                return PolicyStatus.Cancelled;
            }

            if (policy.InceptionTimestamp > timestamp)
            {
                return PolicyStatus.Issued;
            }

            if (policy.ExpiryTimestamp > timestamp)
            {
                return PolicyStatus.Active;
            }

            return PolicyStatus.Expired;
        }

        /// <summary>
        /// Get the number of days to expire.
        /// </summary>
        /// <param name="policy">The policy.</param>
        /// <param name="date">The time to calculate the policy's status for.</param>
        /// <returns>The policy status.</returns>
        public static int GetDaysToExpire(this IPolicy policy, LocalDate date)
        {
            if (policy.IsTermBased)
            {
                return Period.Between(date, policy.ExpiryDateTime.Value.Date, PeriodUnits.Days).Days;
            }

            throw new InvalidOperationException("An attempt was made to get the number of days to expire for a policy "
                + "which is not term-based. Only term-based policies expire on a certain date.");
        }

        /// <summary>
        /// Check if a policy is eligible for Renewal at a given time.
        /// </summary>
        /// <param name="policy">The policy.</param>
        /// <param name="productFeatureSetting">The product feature setting.</param>
        /// <param name="time">The time.</param>
        /// <returns>A value indicating whether the policy is eligible for renewal.</returns>
        public static bool CheckPolicyRenewalEligibilityAtTime(this IPolicy policy, ProductFeatureSetting productFeatureSetting, Instant time)
        {
            int preExpiryRenewalLimitInDays = 60;
            int postExpiryRenewalLimitInDays = LocalDateExtensions.SecondsToDays(productFeatureSetting.ExpiredPolicyRenewalDurationInSeconds);
            var status = policy.GetPolicyStatus(time);
            var date = time.ToLocalDateInAet();
            HashSet<PolicyStatus> validPreExpiryRenewalStatuses = new HashSet<PolicyStatus>
            {
                PolicyStatus.Issued,
                PolicyStatus.Active,
            };

            var daysToGoBeforeExpiring = Period.Between(date, policy.ExpiryDateTime.Value.Date, PeriodUnits.Days).Days;
            if (validPreExpiryRenewalStatuses.Contains(status) && daysToGoBeforeExpiring < preExpiryRenewalLimitInDays)
            {
                return true;
            }

            var daysSinceExpiry = Period.Between(policy.ExpiryDateTime.Value.Date, date, PeriodUnits.Days).Days;
            if (productFeatureSetting.IsRenewalAllowedAfterExpiry && status == PolicyStatus.Expired && daysSinceExpiry <= postExpiryRenewalLimitInDays)
            {
                return true;
            }

            return false;
        }
    }
}
