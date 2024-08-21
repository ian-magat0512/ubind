// <copyright file="IProductFeatureSettingService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Services
{
    using System;
    using System.Collections.Generic;
    using UBind.Domain;
    using UBind.Domain.Product;

    /// <summary>
    /// Service for handling product feature setting.
    /// </summary>
    public interface IProductFeatureSettingService
    {
        /// <summary>
        /// Enable product feature.
        /// </summary>
        /// <param name="tenantId">the tenantId.</param>
        /// <param name="productId">The product feature id.</param>
        /// <param name="productFeatureType">The product feature type.</param>
        /// <returns>The updated product feature.</returns>
        ProductFeatureSetting EnableProductFeature(
            Guid tenantId,
            Guid productId,
            ProductFeatureSettingItem productFeatureType);

        /// <summary>
        /// Throw if the product feature setting is not enabled.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="productId">The product ID.</param>
        /// <param name="productFeatureType">The product feature type.</param>
        /// <param name="operationName">The operation name.</param>
        Task ThrowIfFeatureIsNotEnabled(
            Guid tenantId,
            Guid productId,
            ProductFeatureSettingItem productFeatureType,
            string operationName);

        /// <summary>
        /// Determines whether a specific product feature setting is enabled for a given tenant and product.
        /// </summary>
        /// <param name="tenantId">The unique identifier of the tenant.</param>
        /// <param name="productId">The unique identifier of the product.</param>
        /// <param name="productFeatureType">The type of the product feature setting to check.</param>
        /// <returns>
        /// A boolean value indicating whether the specified product feature setting is enabled.
        /// </returns>
        bool IsProductFeatureSettingEnabled(
            Guid tenantId,
            Guid productId,
            ProductFeatureSettingItem productFeatureType);

        /// <summary>
        /// Disable product feature setting.
        /// </summary>
        /// <param name="tenantId">the tenantId.</param>
        /// <param name="productId">The product Id.</param>
        /// <param name="productFeatureType">The product feature type.</param>
        /// <returns>The updated product feature setting.</returns>
        ProductFeatureSetting DisableProductFeature(Guid tenantId, Guid productId, ProductFeatureSettingItem productFeatureType);

        /// <summary>
        /// Get list of product feature setting.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="productId">The product Id.</param>
        /// <returns>The list product features.</returns>
        ProductFeatureSetting GetProductFeature(Guid tenantId, Guid productId);

        /// <summary>
        /// Create product feature setting.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="productId">The product Id.</param>
        void CreateProductFeatures(Guid tenantId, Guid productId);

        /// <summary>
        /// Filter Product Feature setting with deployments.
        /// </summary>
        /// <param name="tenantId">The tenantId.</param>
        /// <param name="environment">The deployment environment.</param>
        /// <returns>An instance of product.</returns>
        List<ProductFeatureSetting> GetEnabledDeployedProductFeatureSettings(
            Guid tenantId,
            DeploymentEnvironment environment);
    }
}
