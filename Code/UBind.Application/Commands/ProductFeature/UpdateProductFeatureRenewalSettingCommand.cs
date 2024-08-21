// <copyright file="UpdateProductFeatureRenewalSettingCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.User
{
    using System;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// Command for updating product feature renewal setting.
    /// </summary>
    public class UpdateProductFeatureRenewalSettingCommand : ICommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateProductFeatureRenewalSettingCommand"/> class.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="productId">The product Id.</param>
        /// <param name="allowRenewalAfterExpiry">The allow renewal after expiry.</param>
        /// <param name="expiredPolicyRenewalPeriodSeconds">The expired policy renewal period in seconds.</param>
        public UpdateProductFeatureRenewalSettingCommand(
            Guid tenantId,
            Guid productId,
            bool allowRenewalAfterExpiry,
            int expiredPolicyRenewalPeriodSeconds)
        {
            this.TenantId = tenantId;
            this.ProductId = productId;
            this.AllowRenewalAfterExpiry = allowRenewalAfterExpiry;
            this.ExpiredPolicyRenewalPeriodSeconds = expiredPolicyRenewalPeriodSeconds;
        }

        /// <summary>
        /// Gets the tenantId.
        /// </summary>
        public Guid TenantId { get; }

        /// <summary>
        /// Gets the product Id.
        /// </summary>
        public Guid ProductId { get; }

        /// <summary>
        /// Gets a value indicating whether product is allowed to renew after expiry.
        /// </summary>
        public bool AllowRenewalAfterExpiry { get; }

        /// <summary>
        /// Gets the expired policy renewal period in seconds.
        /// </summary>
        public int ExpiredPolicyRenewalPeriodSeconds { get; }
    }
}
