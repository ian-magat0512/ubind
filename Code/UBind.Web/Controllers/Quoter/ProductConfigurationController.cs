// <copyright file="ProductConfigurationController.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Controllers
{
    using System.Threading.Tasks;
    using Humanizer;
    using Microsoft.AspNetCore.Mvc;
    using UBind.Application;
    using UBind.Application.Queries.ProductConfiguration;
    using UBind.Application.Queries.ProductRelease;
    using UBind.Application.Queries.Quote;
    using UBind.Domain;
    using UBind.Domain.Enums;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Product;
    using UBind.Web.Extensions;
    using UBind.Web.Filters;
    using UBind.Web.Mapping;

    /// <summary>
    /// Controller for configuration requests.
    /// </summary>
    [Produces("application/json")]
    [Route("/api/v1/{tenant}/{environment}/{product}/{formType}")]
    public class ProductConfigurationController : Controller
    {
        private readonly ICachingResolver cachingResolver;
        private readonly ICqrsMediator mediator;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductConfigurationController"/> class.
        /// </summary>
        /// <param name="cachingResolver">The tenant, product and portal resolver.</param>
        /// <param name="mediator">The Mediator.</param>
        public ProductConfigurationController(
            ICachingResolver cachingResolver,
            ICqrsMediator mediator)
        {
            this.cachingResolver = cachingResolver;
            this.mediator = mediator;
        }

        /// <summary>
        /// Action for handling configuration requests.
        /// </summary>
        /// <param name="tenant">The ID or Alias of the tenant to obtain configuration for.</param>
        /// <param name="product">The ID or Alias of the product to obtain the configuration for.</param>
        /// <param name="environment">The environment to obtain the configuration for.</param>
        /// <param name="formType">The type of form configuration to load.</param>
        /// <param name="isTestData">The value whether the request is for test data.</param>
        /// <returns>The product configuration for the requested environment.</returns>
        [HttpGet]
        [Route("configuration")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 90)]
        [ProducesResponseType(typeof(ContentResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetConfiguration(
            string tenant,
            string product,
            DeploymentEnvironment environment,
            FormType formType,
            [FromQuery] Guid? quoteId = null,
            [FromQuery] Guid? policyId = null,
            [FromQuery] string? productRelease = null,
            [FromQuery] QuoteType quoteType = QuoteType.NewBusiness,
            bool isTestData = false)
        {
            // TODO: Get configuration based on tenant after product configuration has been updated to include so.
            if (environment != DeploymentEnvironment.Production && isTestData == true)
            {
                return Errors.Forms.TestModeEnvironmentMismatch(environment).ToProblemJsonResult();
            }
            productRelease = productRelease == "null" ? null : productRelease;

            var webFormType = formType.ToWebFormAppType();
            Product productModel = await this.cachingResolver.GetProductOrThrow(new GuidOrAlias(tenant), new GuidOrAlias(product));
            Guid? productReleaseId = await this.GetProductReleaseIdForQuote(
                environment,
                quoteId,
                policyId,
                productRelease,
                quoteType,
                webFormType,
                productModel);

            var releaseContext = await this.mediator.Send(new GetReleaseContextForReleaseOrDefaultReleaseQuery(
                productModel.TenantId,
                productModel.Id,
                environment,
                productReleaseId));
            var configurationQuery = new GetProductComponentConfigurationQuery(releaseContext, webFormType);
            ReleaseProductConfiguration config = await this.mediator.Send(configurationQuery);
            this.HttpContext.Response.Headers.Add("X-Product-Release-Id", config.ProductReleaseId.ToString());
            var result = this.Content(config.ConfigurationJson, ContentTypes.Json);
            return result;
        }

        private async Task<Guid?> GetProductReleaseIdForQuote(
            DeploymentEnvironment environment,
            Guid? quoteId,
            Guid? policyId,
            string? productRelease,
            QuoteType quoteType,
            WebFormAppType webFormType,
            Product productModel)
        {
            Guid? productReleaseId = null;
            if (webFormType == WebFormAppType.Quote)
            {
                if (quoteId != null && productRelease == null && policyId == null)
                {
                    var quote = await this.mediator.Send(new GetQuoteByIdQuery(productModel.TenantId, quoteId));
                    EntityHelper.ThrowIfNotFound(quote, quoteId.Value, "quote");
                    productReleaseId = quote.ProductReleaseId;
                }
                else if (productRelease != null)
                {
                    if (environment == DeploymentEnvironment.Production)
                    {
                        throw new ErrorException(Errors.Release.ProductReleaseCannotBeSpecified(quoteType.Humanize()));
                    }

                    productReleaseId = await this.cachingResolver.GetProductReleaseIdOrThrow(
                        productModel.TenantId,
                        productModel.Id,
                        new GuidOrAlias(productRelease));
                }
                else
                {
                    productReleaseId = await this.mediator.Send(new GetProductReleaseIdQuery(
                        productModel.TenantId,
                        productModel.Id,
                        environment,
                        quoteType,
                        policyId));
                }
            }

            return productReleaseId;
        }
    }
}
