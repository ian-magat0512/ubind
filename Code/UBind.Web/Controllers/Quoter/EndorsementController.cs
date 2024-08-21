// <copyright file="EndorsementController.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Controllers.Quoter
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using NodaTime;
    using UBind.Application.Commands.Quote;
    using UBind.Application.Services;
    using UBind.Domain;
    using UBind.Domain.Entities;
    using UBind.Domain.Enums;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Permissions;
    using UBind.Domain.Product;
    using UBind.Domain.Services;
    using UBind.Domain.Services.QuoteExpiry;
    using UBind.Web.Filters;
    using UBind.Web.ResourceModels.Quote;

    /// <summary>
    /// Controller for endorsement-related requests.
    /// </summary>
    [Produces("application/json")]
    [Route("/api/v1/{tenant}/{environment}/{product}")]
    public class EndorsementController : Controller
    {
        private readonly ICachingResolver cachingResolver;
        private readonly IQuoteEndorsementService applicationQuoteService;
        private readonly IQuoteAggregateResolverService quoteAggregateResolver;
        private readonly IQuoteExpirySettingsProvider quoteExpirySettingsProvider;
        private readonly IFileAttachmentRepository<QuoteFileAttachment> fileAttachmentRepository;
        private readonly IClock clock;
        private readonly ICqrsMediator mediator;

        /// <summary>
        /// Initializes a new instance of the <see cref="EndorsementController"/> class.
        /// </summary>
        /// <param name="endorsementService">The endorsement service.</param>
        /// <param name="quoteAggregateResolver">The aggregate resolver service.</param>
        /// <param name="quoteExpirySettingsProvider">The quote expiry settings provider.</param>
        /// <param name="cachingResolver">The product and tenant resolver.</param>
        /// <param name="fileAttachmentRepository">The file attachment repository.</param>
        /// <param name="clock">The clock.</param>
        public EndorsementController(
            IQuoteEndorsementService endorsementService,
            IQuoteAggregateResolverService quoteAggregateResolver,
            IQuoteExpirySettingsProvider quoteExpirySettingsProvider,
            ICachingResolver cachingResolver,
            IFileAttachmentRepository<QuoteFileAttachment> fileAttachmentRepository,
            IClock clock,
            ICqrsMediator mediator)
        {
            this.cachingResolver = cachingResolver;
            this.applicationQuoteService = endorsementService;
            this.quoteAggregateResolver = quoteAggregateResolver;
            this.quoteExpirySettingsProvider = quoteExpirySettingsProvider;
            this.fileAttachmentRepository = fileAttachmentRepository;
            this.clock = clock;
            this.mediator = mediator;
        }

        /// <summary>
        /// Handles approval requests for a quote.
        /// </summary>
        /// <param name="tenant">The ID or Alias of the tenant the quote is for.</param>
        /// <param name="product">The ID or Alias of the product the quote is for.</param>
        /// <param name="environment">The environment to work on.</param>
        /// <param name="model">Resource model with new form data.</param>
        /// <returns>Ok.</returns>
        [HttpPost]
        [RequestRateLimit(Period = 20, Type = RateLimitPeriodType.Seconds, Limit = 10)]
        [ValidateModel]
        [MustHavePermission(Permission.ReviewQuotes)]
        [Route("reviewApproval")]
        [ProducesResponseType(typeof(QuoteApplicationModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> ApproveQuote(
            string tenant, string product, DeploymentEnvironment environment, [FromBody] QuoteFormDataUpdateModel model)
        {
            Product productModel = await this.cachingResolver.GetProductOrThrow(new GuidOrAlias(tenant), new GuidOrAlias(product));
            var quote = await this.mediator.Send(new ApproveReviewedQuoteCommand(
                productModel.TenantId,
                model.QuoteId,
                model.FormDataJson != null ? new Domain.Aggregates.Quote.FormData(model.FormDataJson) : null));
            var outputModel = new QuoteApplicationModel(quote);
            return this.Ok(outputModel);
        }

        /// <summary>
        /// Handles approval requests for a quote.
        /// </summary>
        /// <param name="tenant">The ID or Alias of the tenant the quote is for.</param>
        /// <param name="product">The ID or Alias of the product the quote is for.</param>
        /// <param name="environment">The environment to work on.</param>
        /// <param name="model">Resource model with new form data.</param>
        /// <returns>Ok.</returns>
        [HttpPost]
        [RequestRateLimit(Period = 20, Type = RateLimitPeriodType.Seconds, Limit = 10)]
        [ValidateModel]
        [Route("autoApproval")]
        [ProducesResponseType(typeof(QuoteApplicationModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> AutoApproveQuote(
            string tenant, string product, DeploymentEnvironment environment, [FromBody] QuoteFormDataUpdateModel model)
        {
            Product productModel = await this.cachingResolver.GetProductOrThrow(new GuidOrAlias(tenant), new GuidOrAlias(product));
            var quote = await this.mediator.Send(new AutoApproveQuoteCommand(
                productModel.TenantId,
                model.QuoteId,
                model.FormDataJson != null ? new Domain.Aggregates.Quote.FormData(model.FormDataJson) : null));
            var outputModel = new QuoteApplicationModel(quote);
            return this.Ok(outputModel);
        }

        /// <summary>
        /// Handles decline requests for a quote.
        /// </summary>
        /// <param name="tenant">The ID or Alias of the tenant the quote is for.</param>
        /// <param name="product">The ID or Alias of the product the quote is for.</param>
        /// <param name="environment">The environment to work on.</param>
        /// <param name="model">Resource model with new form data.</param>
        /// <returns>Ok.</returns>
        [HttpPost]
        [RequestRateLimit(Period = 20, Type = RateLimitPeriodType.Seconds, Limit = 10)]
        [ValidateModel]
        [Route("decline")]
        [ProducesResponseType(typeof(QuoteApplicationModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> DeclineQuote(
            string tenant, string product, DeploymentEnvironment environment, [FromBody] QuoteFormDataUpdateModel model)
        {
            Product productModel = await this.cachingResolver.GetProductOrThrow(new GuidOrAlias(tenant), new GuidOrAlias(product));
            var quote = await this.mediator.Send(new DeclineQuoteCommand(
                productModel.TenantId,
                model.QuoteId,
                model.FormDataJson != null ? new Domain.Aggregates.Quote.FormData(model.FormDataJson) : null));
            var outputModel = new QuoteApplicationModel(quote);
            return this.Ok(outputModel);
        }

        /// <summary>
        /// Handles referral requests for a quote.
        /// </summary>
        /// <param name="tenant">The ID or Alias of the tenant the quote is for.</param>
        /// <param name="product">The ID or Alias of the product the quote is for.</param>
        /// <param name="environment">The environment to work on.</param>
        /// <param name="model">Resource model with new form data.</param>
        /// <returns>Ok.</returns>
        [HttpPost]
        [RequestRateLimit(Period = 20, Type = RateLimitPeriodType.Seconds, Limit = 10)]
        [ValidateModel]
        [Route("endorsementReferral")]
        [ProducesResponseType(typeof(QuoteApplicationModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> ReferQuote(
            string tenant, string product, DeploymentEnvironment environment, [FromBody] QuoteFormDataUpdateModel model)
        {
            Product productModel = await this.cachingResolver.GetProductOrThrow(new GuidOrAlias(tenant), new GuidOrAlias(product));
            var quote = await this.mediator.Send(new ReferQuoteForEndorsementCommand(
                productModel.TenantId,
                model.QuoteId,
                model.FormDataJson != null ? new Domain.Aggregates.Quote.FormData(model.FormDataJson) : null));
            var outputModel = new QuoteApplicationModel(quote);
            return this.Ok(outputModel);
        }

        /// <summary>
        /// Handles releasing requests for a quote.
        /// </summary>
        /// <param name="tenant">The ID or Alias of the tenant the quote is for.</param>
        /// <param name="product">The ID or Alias of the product the quote is for.</param>
        /// <param name="environment">The environment to work on.</param>
        /// <param name="model">Resource model with new form data.</param>
        /// <returns>Ok.</returns>
        [HttpPost]
        [RequestRateLimit(Period = 20, Type = RateLimitPeriodType.Seconds, Limit = 10)]
        [ValidateModel]
        [MustHavePermission(Permission.EndorseQuotes)]
        [Route("endorsementApproval")]
        [ProducesResponseType(typeof(QuoteApplicationModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> ReleaseQuote(
            string tenant, string product, DeploymentEnvironment environment, [FromBody] QuoteFormDataUpdateModel model)
        {
            Product productModel = await this.cachingResolver.GetProductOrThrow(new GuidOrAlias(tenant), new GuidOrAlias(product));
            var quote = await this.mediator.Send(new ApproveEndorsedQuoteCommand(
                productModel.TenantId,
                model.QuoteId,
                model.FormDataJson != null ? new Domain.Aggregates.Quote.FormData(model.FormDataJson) : null));
            var outputModel = new QuoteApplicationModel(quote);
            return this.Ok(outputModel);
        }

        /// <summary>
        /// Handles returning to previous state requests for a quote.
        /// </summary>
        /// <param name="tenant">The ID or Alias of the tenant the quote is for.</param>
        /// <param name="product">The ID or Alias of the product the quote is for.</param>
        /// <param name="environment">The environment to work on.</param>
        /// <param name="model">Resource model with new form data.</param>
        /// <returns>Ok.</returns>
        [HttpPost]
        [RequestRateLimit(Period = 20, Type = RateLimitPeriodType.Seconds, Limit = 10)]
        [ValidateModel]
        [Route("return")]
        [ProducesResponseType(typeof(QuoteApplicationModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> ReturnQuote(
            string tenant, string product, DeploymentEnvironment environment, [FromBody] QuoteFormDataUpdateModel model)
        {
            Product productModel = await this.cachingResolver.GetProductOrThrow(new GuidOrAlias(tenant), new GuidOrAlias(product));
            var quote = await this.mediator.Send(new ReturnQuoteCommand(
                productModel.TenantId,
                model.QuoteId,
                model.FormDataJson != null ? new Domain.Aggregates.Quote.FormData(model.FormDataJson) : null));
            var outputModel = new QuoteApplicationModel(quote);
            return this.Ok(outputModel);
        }

        /// <summary>
        /// Handles review requests for a quote.
        /// </summary>
        /// <param name="tenant">The ID or Alias of the tenant the quote is for.</param>
        /// <param name="product">The ID or Alias of the product the quote is for.</param>
        /// <param name="environment">The environment to work on.</param>
        /// <param name="model">Resource model with new form data.</param>
        /// <returns>Ok.</returns>
        [HttpPost]
        [RequestRateLimit(Period = 20, Type = RateLimitPeriodType.Seconds, Limit = 10)]
        [ValidateModel]
        [Route("reviewReferral")]
        [ProducesResponseType(typeof(QuoteApplicationModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> ReviewQuote(
            string tenant, string product, DeploymentEnvironment environment, [FromBody] QuoteFormDataUpdateModel model)
        {
            Product productModel = await this.cachingResolver.GetProductOrThrow(new GuidOrAlias(tenant), new GuidOrAlias(product));
            var quote = await this.mediator.Send(new ReferQuoteForReviewCommand(
                productModel.TenantId,
                model.QuoteId,
                model.FormDataJson != null ? new Domain.Aggregates.Quote.FormData(model.FormDataJson) : null));
            var outputModel = new QuoteApplicationModel(quote);
            return this.Ok(outputModel);
        }
    }
}
