// <copyright file="PremiumFundingController.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Controllers
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using UBind.Application.Commands.Quote;
    using UBind.Application.Queries.ProductRelease;
    using UBind.Application.Queries.Quote;
    using UBind.Application.Services.Encryption;
    using UBind.Domain;
    using UBind.Domain.Enums;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Product;
    using UBind.Domain.Services;
    using UBind.Web.Filters;
    using UBind.Web.Helpers;
    using UBind.Web.ResourceModels;

    /// <summary>
    /// Controller for handling application submissions.
    /// </summary>
    [Produces("application/json")]
    [Route("/api/v1/{tenant}/{environment}/{product}/premiumFunding")]
    public class PremiumFundingController : Controller
    {
        private readonly ICachingResolver cachingResolver;
        private readonly IQuoteAggregateResolverService quoteAggregateResolver;
        private readonly IAsymmetricEncryptionService encryptionService;
        private readonly ICqrsMediator mediator;

        /// <summary>
        /// Initializes a new instance of the <see cref="PremiumFundingController"/> class.
        /// </summary>
        /// <param name="quoteAggregateResolver">The quote aggregate resolver service.</param>
        /// <param name="cachingResolver">The product and tenant resolver.</param>
        /// <param name="encryptionService">The encryption/decryption service.</param>
        /// <param name="cqrsMediator">CQRS mediator.</param>
        public PremiumFundingController(
            IQuoteAggregateResolverService quoteAggregateResolver,
            ICachingResolver cachingResolver,
            IAsymmetricEncryptionService encryptionService,
            ICqrsMediator cqrsMediator)
        {
            this.cachingResolver = cachingResolver;
            this.quoteAggregateResolver = quoteAggregateResolver;
            this.encryptionService = encryptionService;
            this.mediator = cqrsMediator;
        }

        /// <summary>
        /// Handle premium funding proposal acceptance requests.
        /// </summary>
        /// <param name="tenant">The ID or Alias of the tenant to quote on.</param>
        /// <param name="product">The ID or Alias of the product to quote on.</param>
        /// <param name="environment">The environment to work on.</param>
        /// <param name="model">Resource model with proposal and payment details.</param>
        /// <returns>Ok.</returns>
        [HttpPost]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 15)]
        [ValidateModel]
        [ProducesResponseType(typeof(ResultModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> AcceptProposalWithPayment(
            string tenant,
            string product,
            DeploymentEnvironment environment,
            [FromBody] PremiumFundingPaymentModel model)
        {
            Product productModel = await this.cachingResolver.GetProductOrThrow(new GuidOrAlias(tenant), new GuidOrAlias(product));
            var quote = await this.mediator.Send(new GetQuoteByIdQuery(productModel.TenantId, model.QuoteId));
            WebFormValidator.ValidateQuoteRequest(model.QuoteId, quote, new ProductContext(productModel.TenantId, productModel.Id, environment));
            var releaseContext = await this.mediator.Send(new GetReleaseContextForReleaseOrDefaultReleaseQuery(
                productModel.TenantId,
                productModel.Id,
                environment,
                quote.ProductReleaseId));
            var command = new AcceptFundingProposalCommand(
                releaseContext, model.QuoteId, model.PremiumFundingProposalId, model.GetPaymentMethodDetails(this.encryptionService));
            await this.mediator.Send(command);
            return this.Ok(ResultModel.CreateSuccess());
        }
    }
}
