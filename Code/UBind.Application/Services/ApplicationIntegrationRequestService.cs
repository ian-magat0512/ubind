// <copyright file="ApplicationIntegrationRequestService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Services
{
    using System;
    using System.Threading.Tasks;
    using UBind.Application.Export;
    using UBind.Application.Releases;
    using UBind.Domain;
    using UBind.Domain.Product;
    using UBind.Domain.Services;

    /// <inheritdoc/>
    public class ApplicationIntegrationRequestService : IApplicationIntegrationRequestService
    {
        private readonly ICachingResolver cachingResolver;
        private readonly IProductConfigurationProvider productConfigurationProvider;
        private readonly IIntegrationConfigurationProvider integrationConfigurationProvider;
        private readonly IQuoteAggregateResolverService quoteAggregateResolverService;
        private readonly IReleaseQueryService releaseQueryService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationIntegrationRequestService"/> class.
        /// </summary>
        /// <param name="cachingResolver">The tenant, product and portal resolver.</param>
        /// <param name="productConfigProvider">The product configuration provider.</param>
        /// <param name="integrationConfigProvider">The integration configuration provider.</param>
        /// <param name="quoteAggregateResolverService">The service to resolve the quote aggregate for a given quote ID.</param>
        public ApplicationIntegrationRequestService(
            ICachingResolver cachingResolver,
            IProductConfigurationProvider productConfigProvider,
            IIntegrationConfigurationProvider integrationConfigProvider,
            IQuoteAggregateResolverService quoteAggregateResolverService,
            IReleaseQueryService releaseQueryService)
        {
            this.cachingResolver = cachingResolver;
            this.productConfigurationProvider = productConfigProvider;
            this.integrationConfigurationProvider = integrationConfigProvider;
            this.quoteAggregateResolverService = quoteAggregateResolverService;
            this.releaseQueryService = releaseQueryService;
        }

        /// <inheritdoc/>
        public async Task<WebServiceIntegrationResponse> ExecuteRequest(
            Guid tenantId,
            string webIntegrationId,
            Guid quoteId,
            Guid productId,
            DeploymentEnvironment environment,
            string payloadJson)
        {
            if (string.IsNullOrWhiteSpace(webIntegrationId))
            {
                throw new InvalidOperationException($"web integration id is required");
            }

            if (quoteId == Guid.Empty)
            {
                throw new InvalidOperationException($"quote id is required");
            }

            var product = await this.cachingResolver.GetProductOrThrow(tenantId, productId);

            var quoteAggregate = this.quoteAggregateResolverService.GetQuoteAggregateForQuote(tenantId, quoteId);
            if (quoteAggregate == null)
            {
                throw new InvalidOperationException($"Cannot execute integraton for unknown quote {quoteId}");
            }

            var quote = quoteAggregate.GetQuoteOrThrow(quoteId);
            var releaseContext = this.releaseQueryService.GetReleaseContextForReleaseOrDefaultRelease(
                tenantId,
                productId,
                environment,
                quote.ProductReleaseId);
            var productConfig = await this.productConfigurationProvider.GetProductConfiguration(
                releaseContext, WebFormAppType.Quote);
            var config =
                await this.integrationConfigurationProvider.GetIntegrationConfigurationAsync(
                    releaseContext,
                    productConfig);
            var response = await config.ExecuteIntegration(webIntegrationId, payloadJson, quoteAggregate, productConfig, quoteId);
            return response;
        }
    }
}
