// <copyright file="EnquiryController.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Web.Controllers
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json.Linq;
    using UBind.Application.Authorisation;
    using UBind.Application.Commands.Claim;
    using UBind.Application.Commands.Quote;
    using UBind.Application.ExtensionMethods;
    using UBind.Application.Services;
    using UBind.Domain;
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
    /// Controller for handling application enquiries.
    /// </summary>
    [Produces("application/json")]
    [Route("/api/v1/{tenant}/{environment}/{product}")]
    public class EnquiryController : Controller
    {
        private readonly ICachingResolver cachingResolver;
        private readonly ICqrsMediator mediator;
        private readonly IQuoteAggregateResolverService quoteAggregateResolver;
        private readonly IAuthorisationService authorisationService;
        private readonly IQuoteService quoteService;

        /// <summary>
        /// Initializes a new instance of the <see cref="EnquiryController "/> class.
        /// </summary>
        /// <param name="quoteAggregateResolver">The aggregate resolver service.</param>
        /// <param name="cachingResolver">The product and tenant resolver.</param>
        /// <param name="authorisationService">The authorization service.</param>
        /// <param name="quoteService">The quote service.</param>
        public EnquiryController(
            ICqrsMediator mediator,
            IQuoteAggregateResolverService quoteAggregateResolver,
            ICachingResolver cachingResolver,
            IAuthorisationService authorisationService,
            IQuoteService quoteService)
        {
            this.cachingResolver = cachingResolver;
            this.mediator = mediator;
            this.quoteAggregateResolver = quoteAggregateResolver;
            this.authorisationService = authorisationService;
            this.quoteService = quoteService;
        }

        /// <summary>
        /// Submits an enquiry on the application.
        /// </summary>
        /// <param name="tenant">The ID or Alias of the tenant to quote on.</param>
        /// <param name="product">The ID or Alias of the product to quote on.</param>
        /// <param name="environment">The environment to use.</param>
        /// <param name="model">Resource model with new form data.</param>
        /// <returns>Ok.</returns>
        [HttpPost]
        [RequestRateLimit(Period = 20, Type = RateLimitPeriodType.Seconds, Limit = 10)]
        [ValidateModel]
        [Route("quote/enquiry")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> QuoteEnqiry(string tenant, string product, DeploymentEnvironment environment, [FromBody] QuoteEnquiryModel model)
        {
            Product productModel = await this.cachingResolver.GetProductOrThrow(new GuidOrAlias(tenant), new GuidOrAlias(product));
            var quote = this.quoteAggregateResolver.GetQuoteAggregateForQuote(productModel.TenantId, model.QuoteId);
            WebFormValidator.ValidateQuoteRequest(model.QuoteId, quote, new ProductContext(productModel.TenantId, productModel.Id, environment));
            this.ThrowIfUserCannotViewQuote(productModel.TenantId, model.QuoteId);

            var command = new EnquireQuoteCommand(
                productModel.TenantId,
                productModel.Id,
                model.QuoteId,
                new Domain.Aggregates.Quote.FormData(model.FormDataJson));
            await this.mediator.Send(command);
            return this.NoContent();
        }

        /// <summary>
        /// Submits an enquiry on the application.
        /// </summary>
        /// <param name="tenant">The ID or Alias of the tenant to quote on.</param>
        /// <param name="product">The ID or Alias of the product to quote on.</param>
        /// <param name="environment">The environment to use.</param>
        /// <param name="model">Resource model with new form data.</param>
        /// <returns>Ok.</returns>
        [HttpPost]
        [RequestRateLimit(Period = 20, Type = RateLimitPeriodType.Seconds, Limit = 10)]
        [ValidateModel]
        [Route("claim/enquiry")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> ClaimEnquiry(string tenant, string product, DeploymentEnvironment environment, [FromBody] ClaimEnquiryModel model)
        {
            Product productModel = await this.cachingResolver.GetProductOrThrow(new GuidOrAlias(tenant), new GuidOrAlias(product));
            await this.authorisationService.ThrowIfUserIsAuthenticatedAndCannotViewClaim(this.User, model.ClaimId);

            var formDataJson = JObject.FromObject(model.FormDataJson).ToString();
            var command = new EnquireClaimCommand(
                productModel.TenantId,
                productModel.Id,
                model.ClaimId,
                formDataJson);
            await this.mediator.Send(command);
            return this.NoContent();
        }

        private void ThrowIfUserCannotViewQuote(Guid tenantId, Guid quoteId)
        {
            if (this.User.IsAuthenticated())
            {
                var quoteDetail = this.quoteService.GetQuoteDetails(tenantId, quoteId);
                this.authorisationService.ThrowIfUserCannotViewQuote(this.User, quoteDetail);
            }
        }
    }
}
