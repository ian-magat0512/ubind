// <copyright file="AutomationConfigurationModelProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation
{
    using System;
    using UBind.Application.Releases;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Product;

    /// <inheritdoc/>
    public class AutomationConfigurationModelProvider : IAutomationConfigurationModelProvider
    {
        private readonly IReleaseQueryService releaseQueryService;
        private readonly ICachingResolver cachingResolver;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutomationConfigurationModelProvider"/> class.
        /// </summary>
        /// <param name="releaseQueryService">The deployment repository.</param>
        public AutomationConfigurationModelProvider(
            IReleaseQueryService releaseQueryService,
            ICachingResolver cachingResolver)
        {
            this.releaseQueryService = releaseQueryService;
            this.cachingResolver = cachingResolver;
        }

        /// <inheritdoc/>
        public AutomationsConfigurationModel? GetAutomationConfigurationOrNull(
            Guid tenantId,
            Guid productId,
            DeploymentEnvironment environment,
            Guid? productReleaseId)
        {
            productReleaseId = productReleaseId ?? this.releaseQueryService.GetDefaultProductReleaseIdOrNull(
                tenantId,
                productId,
                environment);
            if (productReleaseId == null)
            {
                return null;
            }

            var releaseContext = new ReleaseContext(tenantId, productId, environment, productReleaseId.Value);
            return this.releaseQueryService.GetRelease(releaseContext)?.AutomationsConfigurationModel;
        }

        public AutomationsConfigurationModel GetAutomationConfigurationOrThrow(
            Guid tenantId,
            Guid productId,
            DeploymentEnvironment environment,
            Guid? productReleaseId)
        {
            var releaseContext = this.releaseQueryService.GetReleaseContextForReleaseOrDefaultRelease(
                tenantId,
                productId,
                environment,
                productReleaseId);
            var release = this.releaseQueryService.GetRelease(releaseContext);
            if (release.AutomationsConfigurationModel == null)
            {
                var tenantAlias = this.cachingResolver.GetTenantAliasOrThrow(tenantId);
                var productAlias = this.cachingResolver.GetProductAliasOrThrow(tenantId, productId);
                throw new ErrorException(Errors.Automation.AutomationConfigurationNotFound(
                    tenantAlias, productAlias, environment, productReleaseId.Value));
            }

            return release.AutomationsConfigurationModel;
        }
    }
}
