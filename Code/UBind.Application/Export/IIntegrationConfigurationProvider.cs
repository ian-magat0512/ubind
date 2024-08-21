// <copyright file="IIntegrationConfigurationProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Export
{
    using System.Threading.Tasks;
    using UBind.Domain.Configuration;
    using UBind.Domain.Product;

    /// <summary>
    /// Provides the integration configuration for a given application.
    /// </summary>
    public interface IIntegrationConfigurationProvider
    {
        /// <summary>
        /// Gets the integration configuration for a given application.
        /// </summary>
        /// <param name="productConfiguration">The product configuration to be used to build the integration configuration.</param>
        /// <returns>The integration configuration for the given application.</returns>
        Task<IntegrationConfiguration> GetIntegrationConfigurationAsync(
            ReleaseContext releaseContext,
            IProductConfiguration productConfiguration = null);
    }
}
