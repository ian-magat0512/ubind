// <copyright file="GetProductComponentConfigurationQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.ProductConfiguration
{
    using StackExchange.Profiling;
    using UBind.Application.Releases;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Product;
    using UBind.Domain.Repositories;

    /// <summary>
    /// Query Handler for getting the product configuration.
    /// </summary>
    public class GetProductComponentConfigurationQueryHandler : IQueryHandler<GetProductComponentConfigurationQuery, ReleaseProductConfiguration>
    {
        private readonly IReleaseQueryService releaseQueryService;
        private readonly IConfigurationService configurationService;
        private readonly ITenantRepository tenantRepository;
        private readonly IProductRepository productRepository;
        private readonly ICachingResolver cachingResolver;

        public GetProductComponentConfigurationQueryHandler(
            IReleaseQueryService releaseQueryService,
            IConfigurationService configurationService,
            ITenantRepository tenantRepository,
            IProductRepository productRepository,
            ICachingResolver cachingResolver)
        {
            this.releaseQueryService = releaseQueryService;
            this.configurationService = configurationService;
            this.tenantRepository = tenantRepository;
            this.productRepository = productRepository;
            this.cachingResolver = cachingResolver;
        }

        /// <inheritdoc/>
        public async Task<ReleaseProductConfiguration> Handle(GetProductComponentConfigurationQuery request, CancellationToken cancellationToken)
        {
            await this.CheckTenantAndProduct(request.ReleaseContext);
            using (MiniProfiler.Current.Step(nameof(ConfigurationService) + "." + nameof(this.Handle)))
            {
                var release = this.releaseQueryService.GetRelease(request.ReleaseContext);
                ProductComponentConfiguration componentConfig =
                    release.GetProductComponentConfigurationOrThrow(request.WebFormAppType);
                var config = componentConfig.WebFormAppConfigurationJson;

                return await Task.FromResult(new ReleaseProductConfiguration
                {
                    ConfigurationJson = config,
                    ProductReleaseId = release.ReleaseId,
                });
            }
        }

        private async Task CheckTenantAndProduct(ReleaseContext releaseContext)
        {
            Tenant tenant = await this.cachingResolver.GetTenantOrThrow(releaseContext.TenantId);
            if (tenant.Details.Disabled)
            {
                throw new ErrorException(Errors.Tenant.Disabled(tenant.Details.Alias));
            }

            Product product = await this.cachingResolver.GetProductOrThrow(releaseContext.TenantId, releaseContext.ProductId);
            if (product.Details.Disabled)
            {
                throw new ErrorException(Errors.Product.Disabled(product.Details.Alias));
            }
        }
    }
}
