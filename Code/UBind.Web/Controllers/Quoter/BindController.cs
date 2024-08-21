// <copyright file="BindController.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Controllers.Quoter
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using NodaTime;
    using UBind.Application.Queries.ProductRelease;
    using UBind.Application.Queries.Quote;
    using UBind.Application.Services.Encryption;
    using UBind.Domain;
    using UBind.Domain.CustomPipelines.BindPolicy;
    using UBind.Domain.Entities;
    using UBind.Domain.Enums;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Policy;
    using UBind.Domain.Services;
    using UBind.Web.Filters;
    using UBind.Web.Helpers;
    using UBind.Web.ResourceModels;
    using UBind.Web.ResourceModels.Policy;
    using UBind.Web.ResourceModels.Quote;

    /// <summary>
    /// Controller for handling quote bind requests.
    /// </summary>
    [Produces("application/json")]
    [Route("/api/v1/{tenant}/{environment}/{product}")]
    public class BindController : Controller
    {
        private readonly ICachingResolver cachingResolver;
        private readonly IQuoteAggregateResolverService quoteAggregateResolver;
        private readonly IAsymmetricEncryptionService encryptionService;
        private readonly IFileAttachmentRepository<QuoteFileAttachment> fileAttachmentRepository;
        private readonly IClock clock;
        private readonly ICqrsMediator mediator;

        /// <summary>
        /// Initializes a new instance of the <see cref="BindController"/> class.
        /// </summary>
        /// <param name="quoteAggregateResolver">The quote aggregate resolver service.</param>
        /// <param name="cachingResolver">The product and tenant resolver.</param>
        /// <param name="encryptionService">The encryption service.</param>
        /// <param name="fileAttachmentRepository">The file attachment repository.</param>
        /// <param name="clock">The clock.</param>
        /// <param name="mediator">The CQRS mediator.</param>
        public BindController(
            IQuoteAggregateResolverService quoteAggregateResolver,
            ICachingResolver cachingResolver,
            IAsymmetricEncryptionService encryptionService,
            IFileAttachmentRepository<QuoteFileAttachment> fileAttachmentRepository,
            IClock clock,
            ICqrsMediator mediator)
        {
            this.cachingResolver = cachingResolver;
            this.quoteAggregateResolver = quoteAggregateResolver;
            this.encryptionService = encryptionService;
            this.fileAttachmentRepository = fileAttachmentRepository;
            this.clock = clock;
            this.mediator = mediator;
        }

        /// <summary>
        /// Handles quote bind requests.
        /// </summary>
        /// <param name="tenant">The ID or Alias of the tenant the quote is for.</param>
        /// <param name="product">The ID or Alias of the product the quote is for.</param>
        /// <param name="environment">The environment to use.</param>
        /// <param name="model">Resource model with new form data.</param>
        /// <returns>Ok.</returns>
        [HttpPost]
        [ValidateModel]
        [Route("bind")]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 15)]
        [ProducesResponseType(typeof(PolicyIssuanceModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(QuoteApplicationModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> BindQuote(
            string tenant, string product, DeploymentEnvironment environment, [FromBody] BindModel model)
        {
            Tenant tenantModel = await this.cachingResolver.GetTenantOrThrow(new GuidOrAlias(tenant));
            Product productModel = await this.cachingResolver.GetProductOrThrow(new GuidOrAlias(tenant), new GuidOrAlias(product));
            var paymentDetails = model.CreditCardDetails != null || model.BankAccountDetails != null ?
                model.GetPaymentMethodDetails(this.encryptionService) :
                null;
            var quote = await this.mediator.Send(new GetQuoteByIdQuery(tenantModel.Id, model.QuoteId));
            WebFormValidator.ValidateQuoteRequest(model.QuoteId, quote, new ProductContext(productModel.TenantId, productModel.Id, environment));
            var releaseContext = await this.mediator.Send(new GetReleaseContextForReleaseOrDefaultReleaseQuery(
                productModel.TenantId,
                productModel.Id,
                environment,
                quote.ProductReleaseId));
            var bindCommand = BindPolicyCommand.CreateForBindingWithQuote(
                releaseContext,
                model.QuoteId,
                model.ToDto(),
                model.PremiumFundingProposalId,
                paymentDetails,
                model.SavedPaymentMethodId,
                null,
                model.TokenId);

            (NewQuoteReadModel, PolicyReadModel) result;
            try
            {
                result = await this.mediator.Send(bindCommand);
            }
            catch (ConcurrencyException)
            {
                var paidOrFunded = bindCommand.AcceptedFundingProposal != null || bindCommand.PaymentResult != null;
                throw new ErrorException(Errors.Quote.Bind.BindCannotBeProcessedDueToConcurrency(bindCommand.QuoteId.Value, paidOrFunded));
            }

            var updatedQuote = result.Item1;
            var policy = result.Item2;

            // reload the aggregate
            if (policy != null)
            {
                return this.Ok(new PolicyIssuanceModel(updatedQuote, policy));
            }

            var calculationResultModelPaid = new QuoteCalculationResultModel(updatedQuote);
            return this.Ok(new QuoteApplicationModel(updatedQuote, calculationResultModelPaid: calculationResultModelPaid));
        }
    }
}
