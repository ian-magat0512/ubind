// <copyright file="IAutomationConfigurationModelProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation
{
    using System;
    using UBind.Domain;

    /// <summary>
    /// Provides the automation configuration model builder for the given product.
    /// </summary>
    public interface IAutomationConfigurationModelProvider
    {
        /// <summary>
        /// Gets the automation configuration for a given product.
        /// </summary>
        /// <param name="tenantId">The ID of the product's tenant.</param>
        /// <param name="productId">The ID of the product.</param>
        /// <param name="environment">The environment to get the configuration for.</param>
        /// <returns>The automation configuration.</returns>
        /// <param name="productReleaseId">The product release ID, or null to use the default release for the environment.</param>
        AutomationsConfigurationModel? GetAutomationConfigurationOrNull(
            Guid tenantId,
            Guid productId,
            DeploymentEnvironment environment,
            Guid? productReleaseId);

        /// <summary>
        /// Gets the automation configuration for a given product.
        /// </summary>
        /// <param name="tenantId">The ID of the product's tenant.</param>
        /// <param name="productId">The ID of the product.</param>
        /// <param name="environment">The environment to get the configuration for.</param>
        /// <returns>The automation configuration.</returns>
        /// <param name="productReleaseId">The product release ID, or null to use the default release for the environment.</param>
        AutomationsConfigurationModel GetAutomationConfigurationOrThrow(
            Guid tenantId,
            Guid productId,
            DeploymentEnvironment environment,
            Guid? productReleaseId);
    }
}
