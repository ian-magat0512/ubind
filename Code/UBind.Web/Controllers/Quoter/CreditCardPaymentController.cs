// <copyright file="CreditCardPaymentController.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Controllers
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
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
    using FormData = UBind.Domain.Aggregates.Quote.FormData;

    /// <summary>
    /// Controller for handling application submissions.
    /// </summary>
    [Produces("application/json")]
    [Route("/api/v1/{tenant}/{environment}/{product}/")]
    public class CreditCardPaymentController : Controller
    {
        private readonly IPaymentService paymentService;
        private readonly ICachingResolver cachingResolver;
        private readonly IQuoteAggregateResolverService quoteAggregateResolverService;
        private readonly IAsymmetricEncryptionService encryptionService;
        private readonly ICqrsMediator cqrsMediator;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreditCardPaymentController"/> class.
        /// </summary>
        /// <param name="paymentService">The application service.</param>
        /// <param name="quoteAggregateResolverService">The quote aggregate resolver service.</param>
        /// <param name="cachingResolver">The product and tenant resolver.</param>
        /// <param name="encryptionService">The encryption service.</param>
        /// <param name="mediator">The CQRS mediator.</param>
        public CreditCardPaymentController(
            IPaymentService paymentService,
            IQuoteAggregateResolverService quoteAggregateResolverService,
            ICachingResolver cachingResolver,
            IAsymmetricEncryptionService encryptionService,
            ICqrsMediator mediator)
        {
            this.cachingResolver = cachingResolver;
            this.quoteAggregateResolverService = quoteAggregateResolverService;
            this.paymentService = paymentService;
            this.encryptionService = encryptionService;
            this.cqrsMediator = mediator;
        }

        /// <summary>
        /// Handle credit card payment requests.
        /// </summary>
        /// <param name="tenant">The ID or Alias of the tenant to quote on.</param>
        /// <param name="product">The ID or Alias of the product to quote on.</param>
        /// <param name="environment">The environment to use.</param>
        /// <param name="model">Resource model with new form data.</param>
        /// <returns>Ok.</returns>
        [HttpPost]
        [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 5)]
        [ValidateModel]
        [Route("creditCardPayment")]
        [ProducesResponseType(typeof(CreditCardPaymentResultModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> PostCreditCard(string tenant, string product, DeploymentEnvironment environment, [FromBody] CreditCardPaymentModel model)
        {
            var tenantModel = await this.cachingResolver.GetTenantOrThrow(new GuidOrAlias(tenant));
            var productModel = await this.cachingResolver.GetProductOrThrow(new GuidOrAlias(tenant), new GuidOrAlias(product));
            var creditCardDetails = model.CreditCardDetails != null
                ? model.CreditCardDetails.Map(this.encryptionService)
                : null;
            var quote = await this.cqrsMediator.Send(new GetQuoteByIdQuery(productModel.TenantId, model.QuoteId));
            WebFormValidator.ValidateQuoteRequest(model.QuoteId, quote, new ProductContext(productModel.TenantId, productModel.Id, environment));
            var releaseContext = await this.cqrsMediator.Send(new GetReleaseContextForReleaseOrDefaultReleaseQuery(
                productModel.TenantId,
                productModel.Id,
                environment,
                quote.ProductReleaseId));
            var cardPaymentCommand = new CardPaymentCommand(
                releaseContext,
                model.QuoteId,
                model.CalculationResultId,
                new FormData(model.FormDataJson),
                null,
                creditCardDetails,
                model.SavedPaymentMethodId);
            await this.cqrsMediator.Send(cardPaymentCommand);

            var updatedQuote = this.quoteAggregateResolverService.GetQuoteAggregateForQuote(tenantModel.Id, model.QuoteId).GetQuoteOrThrow(model.QuoteId);
            var response = new CreditCardPaymentResultModel(updatedQuote);
            return response.Succeeded
                ? (IActionResult)this.Ok(response)
                : this.BadRequest(response);
        }

        /// <summary>
        /// Handle tokenised payment requests.
        /// </summary>
        /// <param name="tenant">The ID or Alias of the tenant to quote on.</param>
        /// <param name="product">The ID or Alias of the product to quote on.</param>
        /// <param name="environment">The environment to use.</param>
        /// <param name="model">Resource model token (and new form data).</param>
        /// <returns>Ok.</returns>
        [HttpPost]
        [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 5)]
        [ValidateModel]
        [Route("stripePayment")]
        [ProducesResponseType(typeof(CreditCardPaymentResultModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> PostToken(string tenant, string product, DeploymentEnvironment environment, [FromBody] TokenPaymentModel model)
        {
            var productModel = await this.cachingResolver.GetProductOrThrow(new GuidOrAlias(tenant), new GuidOrAlias(product));
            var formDataJson = JsonConvert.SerializeObject(model.FormDataJson);
            var quote = await this.cqrsMediator.Send(new GetQuoteByIdQuery(productModel.TenantId, model.QuoteId));
            WebFormValidator.ValidateQuoteRequest(model.QuoteId, quote, new ProductContext(productModel.TenantId, productModel.Id, environment));
            var releaseContext = await this.cqrsMediator.Send(new GetReleaseContextForReleaseOrDefaultReleaseQuery(
                productModel.TenantId,
                productModel.Id,
                environment,
                quote.ProductReleaseId));
            var stripePaymentCommand = new CardPaymentCommand(
                releaseContext, model.QuoteId, model.CalculationResultId, new FormData(formDataJson), model.TokenId);
            await this.cqrsMediator.Send(stripePaymentCommand);

            var updatedQuote = this.quoteAggregateResolverService.GetQuoteAggregateForQuote(
                productModel.TenantId, model.QuoteId)?
                .GetQuoteOrThrow(model.QuoteId);
            var response = new CreditCardPaymentResultModel(updatedQuote);
            return response.Succeeded
                ? (IActionResult)this.Ok(response)
                : this.BadRequest(response);
        }
    }
}
