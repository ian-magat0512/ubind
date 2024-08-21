// <copyright file="ProductFeatureRenewalSettingModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    /// <summary>
    /// Resource model for product feature.
    /// </summary>
    public class ProductFeatureRenewalSettingModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProductFeatureRenewalSettingModel"/> class.
        /// </summary>
        /// <param name="allowRenewalAfterExpiry">The indicator renewal is allowed after expiry.</param>
        /// <param name="expiredPolicyRenewalPeriodSeconds">The expired policy renewal period in seconds.</param>
        public ProductFeatureRenewalSettingModel(bool allowRenewalAfterExpiry, int expiredPolicyRenewalPeriodSeconds)
        {
            this.AllowRenewalAfterExpiry = allowRenewalAfterExpiry;
            this.ExpiredPolicyRenewalPeriodSeconds = expiredPolicyRenewalPeriodSeconds;
        }

        /// <summary>
        /// Gets the allowable expired policy renewal duration in seconds.
        /// </summary>
        public int ExpiredPolicyRenewalPeriodSeconds { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the renewal is allowed after expiry.
        /// </summary>
        public bool AllowRenewalAfterExpiry { get; private set; }
    }
}
