// <copyright file="IAutomationConfigurationProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation
{
    using System;
    using System.Threading.Tasks;
    using UBind.Domain;

    /// <summary>
    /// Provides the automations configuration for a given tenancy and product.
    /// </summary>
    public interface IAutomationConfigurationProvider
    {
        /// <summary>
        /// Gets the automation configuration for a given tenancy and product.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant.</param>
        /// <param name="productId">The ID of the product.</param>
        /// <param name="environment">The environment the configuration is for.</param>
        /// <returns>The automation configuration.</returns>
        Task<AutomationsConfiguration?> GetAutomationConfigurationOrNull(
            Guid tenantId,
            Guid productId,
            DeploymentEnvironment environment,
            Guid? productReleaseId);

        /// <summary>
        /// Gets the automation configuration for a given tenancy and product.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant.</param>
        /// <param name="productId">The ID of the product.</param>
        /// <param name="environment">The environment the configuration is for.</param>
        /// <returns>The automation configuration.</returns>
        Task<AutomationsConfiguration> GetAutomationConfigurationOrThrow(
            Guid tenantId,
            Guid productId,
            DeploymentEnvironment environment,
            Guid? productReleaseId);

        AutomationsConfiguration GetAutomationConfiguration(AutomationsConfigurationModel model);
    }
}
