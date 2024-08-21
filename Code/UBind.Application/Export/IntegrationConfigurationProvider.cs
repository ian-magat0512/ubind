// <copyright file="IntegrationConfigurationProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Export
{
    using System.Threading.Tasks;
    using UBind.Application.Releases;
    using UBind.Domain.Configuration;
    using UBind.Domain.Product;

    /// <inheritdoc/>
    public class IntegrationConfigurationProvider : IIntegrationConfigurationProvider
    {
        private readonly IReleaseQueryService releaseQueryService;
        private readonly IExporterDependencyProvider dependencyProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="IntegrationConfigurationProvider"/> class.
        /// </summary>
        /// <param name="releaseQueryService">Service for providing the current product release.</param>
        /// <param name="dependencyProvider">Container providing dependencies required in exporter building.</param>
        public IntegrationConfigurationProvider(
            IReleaseQueryService releaseQueryService,
            IExporterDependencyProvider dependencyProvider)
        {
            this.releaseQueryService = releaseQueryService;
            this.dependencyProvider = dependencyProvider;
        }

        /// <inheritdoc/>
        public Task<IntegrationConfiguration> GetIntegrationConfigurationAsync(
            ReleaseContext releaseContext,
            IProductConfiguration productConfig = null)
        {
            var currentRelease = this.releaseQueryService.GetRelease(releaseContext);
            var config = currentRelease.IntegrationsConfigurationModel.Build(this.dependencyProvider, productConfig);
            return Task.FromResult(config);
        }
    }
}
