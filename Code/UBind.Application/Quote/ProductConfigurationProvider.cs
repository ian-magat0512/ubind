// <copyright file="ProductConfigurationProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Quote
{
    using System.Threading.Tasks;
    using StackExchange.Profiling;
    using UBind.Application.Releases;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Claim.Workflow;
    using UBind.Domain.Aggregates.Quote.Workflow;
    using UBind.Domain.Configuration;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Product;

    /// <summary>
    /// Provides the configuration for a given product under a specific tenant.
    /// </summary>
    /// <remarks>Configuration to be obtained from a product.json file.</remarks>
    public class ProductConfigurationProvider : IProductConfigurationProvider, IQuoteWorkflowProvider, IClaimWorkflowProvider
    {
        private readonly IReleaseQueryService releaseQueryService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductConfigurationProvider"/> class.
        /// </summary>
        /// <param name="releaseQueryService">Repository used to obtain the current release.</param>
        public ProductConfigurationProvider(
            IReleaseQueryService releaseQueryService)
        {
            this.releaseQueryService = releaseQueryService;
        }

        /// <inheritdoc />
        public async Task<IClaimWorkflow> GetConfigurableClaimWorkflow(ReleaseContext releaseContext)
        {
            using (MiniProfiler.Current.Step(nameof(ProductConfigurationProvider) + "." + nameof(this.GetConfigurableClaimWorkflow)))
            {
                IProductConfiguration config = await this.GetProductConfiguration(releaseContext, WebFormAppType.Claim);
                if (config.ClaimWorkflow == null)
                {
                    throw new ErrorException(Errors.Claim.WorkflowConfigurationNotFound(releaseContext));
                }

                return config.ClaimWorkflow;
            }
        }

        /// <inheritdoc />
        public async Task<IQuoteWorkflow> GetConfigurableQuoteWorkflow(ReleaseContext releaseContext)
        {
            using (MiniProfiler.Current.Step(nameof(ProductConfigurationProvider) + "." + nameof(this.GetConfigurableQuoteWorkflow)))
            {
                IProductConfiguration config = await this.GetProductConfiguration(releaseContext, WebFormAppType.Quote);
                if (config.QuoteWorkflow == null)
                {
                    throw new ErrorException(Errors.Quote.WorkflowConfigurationNotFound(releaseContext));
                }

                return config.QuoteWorkflow;
            }
        }

        /// <inheritdoc />
        public async Task<IProductConfiguration> GetProductConfiguration(ReleaseContext releaseContext, WebFormAppType webFormAppType)
        {
            using (MiniProfiler.Current.Step(nameof(ProductConfigurationProvider) + "." + nameof(this.GetProductConfiguration)))
            {
                var release = this.releaseQueryService.GetRelease(releaseContext);
                ProductComponentConfiguration componentConfig = release.GetProductComponentConfigurationOrThrow(webFormAppType);
                return await Task.FromResult(componentConfig.ProductConfiguration);
            }
        }

        public FormDataSchema GetFormDataSchema(ReleaseContext releaseContext, WebFormAppType webFormAppType)
        {
            var release = this.releaseQueryService.GetRelease(releaseContext);
            if (release[webFormAppType] != null)
            {
                ProductComponentConfiguration componentConfig = release.GetProductComponentConfigurationOrThrow(webFormAppType);
                return componentConfig.FormDataSchema.GetValueOrDefault(null);
            }

            return null;
        }

        public FormDataSchema GetFormDataSchema(ProductContext productContext, WebFormAppType webFormAppType)
        {
            var releaseContext = this.releaseQueryService.GetDefaultReleaseContextOrThrow(
                productContext.TenantId,
                productContext.ProductId,
                productContext.Environment);
            return this.GetFormDataSchema(releaseContext, webFormAppType);
        }
    }
}
