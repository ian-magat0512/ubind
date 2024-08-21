// <copyright file="IProductFeatureSettingRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Product
{
    using System;
    using System.Collections.Generic;
    using NodaTime;

    /// <summary>
    /// Repository for product feature setting.
    /// </summary>
    public interface IProductFeatureSettingRepository
    {
        /// <summary>
        /// Add a product feature setting.
        /// </summary>
        /// <param name="productFeatureSetting">The product feature.</param>
        /// <returns>The created product feature.</returns>
        ProductFeatureSetting AddProductFeatureSetting(ProductFeatureSetting productFeatureSetting);

        /// <summary>
        /// Get product feature for a given tenantId and productId.
        /// </summary>
        /// <returns>The product feature.</returns>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="productId">The product ID.</param>
        ProductFeatureSetting? GetProductFeatureSetting(Guid tenantId, Guid productId);

        /// <summary>
        /// Enable product feature setting.
        /// </summary>
        /// <returns>The updated product feature.</returns>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="productId">The product ID.</param>
        /// <param name="productFeatureType">The product feature type.</param>
        ProductFeatureSetting EnableProductFeature(Guid tenantId, Guid productId, ProductFeatureSettingItem productFeatureType);

        /// <summary>
        /// Disable product feature setting.
        /// </summary>
        /// <returns>The updated product feature.</returns>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="productId">The product ID.</param>
        /// <param name="productFeatureType">The product feature type.</param>
        ProductFeatureSetting DisableProductFeature(Guid tenantId, Guid productId, ProductFeatureSettingItem productFeatureType);

        /// <summary>
        /// Get product feature setting by tenant ID.
        /// </summary>
        /// <returns>The updated product feature.</returns>
        /// <param name="tenantId">The tenant Id.</param>
        IEnumerable<ProductFeatureSetting> GetProductFeatureByTenantId(Guid tenantId);

        /// <summary>
        /// Get product features for products deployed to the given environment, for a given tenant,
        /// where the products are enabled (not disabled or deleted).
        /// </summary>
        /// <returns>The product feature setting.</returns>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="environment">The deployment environment.</param>
        List<ProductFeatureSetting> GetDeployedProductFeatureSettings(
            Guid tenantId,
            DeploymentEnvironment environment);

        /// <summary>
        /// Update product feature setting.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="productId">The product ID.</param>
        /// <param name="allowRenewalAfterExpiry">The Indicator whether the renewal is allowed after expiry.</param>
        /// <param name="expiredPolicyRenewalDurationInSeconds">The expired policy renewal duration in seconds.</param>
        void UpdateProductFeatureRenewalSetting(Guid tenantId, Guid productId, bool allowRenewalAfterExpiry, Duration expiredPolicyRenewalDurationInSeconds);

        /// <summary>
        /// Update cancellation setting.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="productId">The product ID.</param>
        /// <param name="refundRule">The refund rule.</param>
        /// <param name="periodWhichNoClaimsMade">The period which no claims made.</param>
        /// <param name="lastNumberOfYearsWhichNoClaimsMade">The last number of years which no claims were made.</param>
        void UpdateRefundSettings(
        Guid tenantId,
        Guid productId,
        RefundRule refundRule,
        PolicyPeriodCategory? periodWhichNoClaimsMade,
        int? lastNumberOfYearsWhichNoClaimsMade);
    }
}
