// <copyright file="ApplicationUpdateController.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Controllers
{
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using UBind.Application.Commands.Quote;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Claim;
    using UBind.Domain.Enums;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Product;
    using UBind.Domain.Services;
    using UBind.Web.Filters;
    using UBind.Web.Helpers;
    using UBind.Web.ResourceModels.Claim;
    using UBind.Web.ResourceModels.Quote;

    /// <summary>
    /// Controller for form data update requests.
    /// </summary>
    [Produces("application/json")]
    [Route("/api/v1/{tenant}/{environment}/{product}")]
    public class ApplicationUpdateController : Controller
    {
        private readonly ICachingResolver cachingResolver;
        private readonly IClaimService claimService;
        private readonly IClaimAggregateRepository claimAggregateRepository;
        private readonly IQuoteAggregateResolverService quoteAggregateResolver;
        private readonly IProductConfigurationProvider productConfigurationProvider;
        private readonly ICqrsMediator mediator;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationUpdateController"/> class.
        /// </summary>
        /// <param name="claimAggregateRepository">The claim aggregate repository.</param>
        /// <param name="quoteAggregateResolver">The quote aggregate resolver.</param>
        /// <param name="claimService">The claim service.</param>
        /// <param name="cachingResolver">The tenant, product and portal resolver.</param>
        /// <param name="productConfigurationProvider">The product configuration.</param>
        /// <param name="mediator">The mediator for sending queries and commands.</param>
        public ApplicationUpdateController(
            IClaimAggregateRepository claimAggregateRepository,
            IQuoteAggregateResolverService quoteAggregateResolver,
            IClaimService claimService,
            ICachingResolver cachingResolver,
            IProductConfigurationProvider productConfigurationProvider,
            ICqrsMediator mediator)
        {
            this.cachingResolver = cachingResolver;
            this.claimService = claimService;
            this.claimAggregateRepository = claimAggregateRepository;
            this.quoteAggregateResolver = quoteAggregateResolver;
            this.productConfigurationProvider = productConfigurationProvider;
            this.mediator = mediator;
        }

        /// <summary>
        /// Update an application's form data.
        /// </summary>
        /// <param name="tenant">The ID or Alias for the tenant the quote is for.</param>
        /// <param name="product">The ID or Alias of the product the quote is for.</param>
        /// <param name="environment">The environment to use.</param>
        /// <param name="model">Resource model with new form data, and quote ID (if already exists).</param>
        /// <returns>Ok.</returns>
        [HttpPost]
        [Route("quote/formupdate")]
        [Route("quote/save")]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 15)]
        [ValidateModel]
        [ProducesResponseType(typeof(QuoteFormUpdateResultModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateQuote(
            string tenant,
            string product,
            DeploymentEnvironment environment,
            [FromBody] QuoteFormDataUpdateModel model)
        {
            Product productModel = await this.cachingResolver.GetProductOrThrow(new GuidOrAlias(tenant), new GuidOrAlias(product));
            await this.mediator.Send(new UpdateQuoteFormDataCommand(
                productModel.TenantId, model.QuoteId, new Domain.Aggregates.Quote.FormData(model.FormDataJson)));

            var path = this.Request.Path.Value.Split('/').LastOrDefault();
            if (path == "save")
            {
                // creates an aggregate and system event that can be used to trigger the sending of an
                // email with a continuation link, etc.
                await this.mediator.Send(new RecordQuoteSavedCommand(productModel.TenantId, model.QuoteId));
            }

            return this.NoContent();
        }

        /// <summary>
        /// Updates a claims form data.
        /// </summary>
        /// <param name="tenant">The ID or Alias of the tenant the claim is for.</param>
        /// <param name="product">The ID or Alias of the product the claim is for.</param>
        /// <param name="environment">The environment to use.</param>
        /// <param name="model">Resource model with new form data, and claim ID (if already exists).</param>
        /// <returns>Ok.</returns>
        [HttpPost]
        [Route("claim/formupdate")]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 15)]
        [ValidateModel]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> UpdateClaim(string tenant, string product, DeploymentEnvironment environment, [FromBody] ClaimFormDataUpdateModel model)
        {
            Product productModel = await this.cachingResolver.GetProductOrThrow(new GuidOrAlias(tenant), new GuidOrAlias(product));
            var formDataJson = JsonConvert.SerializeObject(model.FormDataJson);
            var claim = this.claimAggregateRepository.GetById(productModel.TenantId, model.ClaimId);
            var productContext = new ProductContext(
                productModel.TenantId, productModel.Id, environment);
            WebFormValidator.ValidateClaimRequest(model.ClaimId, claim, productContext);
            await this.claimService.UpdateFormData(productModel.TenantId, model.ClaimId, formDataJson);

            return this.NoContent();
        }
    }
}
