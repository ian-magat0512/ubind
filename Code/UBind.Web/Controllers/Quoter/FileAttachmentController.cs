// <copyright file="FileAttachmentController.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Web.Controllers
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using UBind.Application.Commands.Quote;
    using UBind.Application.Services;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Claim;
    using UBind.Domain.Enums;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Product;
    using UBind.Domain.Services;
    using UBind.Web.Filters;
    using UBind.Web.Helpers;
    using UBind.Web.ResourceModels;
    using UBind.Web.ResourceModels.Claim;
    using UBind.Web.ResourceModels.Quote;

    /// <summary>
    /// Controller for handling file attachments.
    /// </summary>
    [Produces("application/json")]
    public class FileAttachmentController : Controller
    {
        private readonly ICachingResolver cachingResolver;
        private readonly IApplicationClaimFileAttachmentService claimApplicationFileAttachmentService;
        private readonly IQuoteAggregateResolverService quoteAggregateResolver;
        private readonly IClaimAggregateRepository claimAggregateRepository;
        private readonly ICqrsMediator mediator;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileAttachmentController"/> class.
        /// </summary>
        /// <param name="claimApplicationFileAttachmentService">A service for attaching file/s claim application.</param>
        /// <param name="claimAggregateRepository">The claim aggregate repository.</param>
        /// <param name="quoteAggregateResolver">The quote aggregate resolver service.</param>
        /// <param name="cachingResolver">The product and tenant resolver.</param>
        public FileAttachmentController(
            IApplicationClaimFileAttachmentService claimApplicationFileAttachmentService,
            IClaimAggregateRepository claimAggregateRepository,
            IQuoteAggregateResolverService quoteAggregateResolver,
            ICqrsMediator mediator,
            ICachingResolver cachingResolver)
        {
            this.cachingResolver = cachingResolver;
            this.claimApplicationFileAttachmentService = claimApplicationFileAttachmentService;
            this.claimAggregateRepository = claimAggregateRepository;
            this.quoteAggregateResolver = quoteAggregateResolver;
            this.mediator = mediator;
        }

        /// <summary>
        /// Attach a file for quotes.
        /// </summary>
        /// <param name="tenant">The ID or Alias of the tenant the quote is for.</param>
        /// <param name="product">The ID or Alias of the product to invoice on.</param>
        /// <param name="environment">The environment to use.</param>
        /// <param name="model">Resource model with new form data.</param>
        /// <returns>File attachment result.</returns>
        [HttpPost]
        [RequestRateLimit(Period = 20, Type = RateLimitPeriodType.Seconds, Limit = 20)]
        [ValidateModel]
        [Route("api/v1/{tenant}/{environment}/{product}/quote/attachment")]
        [ProducesResponseType(typeof(FileAttachmentResultModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> AttachFileToQuote(
            string tenant, string product, DeploymentEnvironment environment, [FromBody] QuoteFileAttachmentModel model)
        {
            Product productModel = await this.cachingResolver.GetProductOrThrow(new GuidOrAlias(tenant), new GuidOrAlias(product));
            var application = this.quoteAggregateResolver.GetQuoteAggregateForQuote(productModel.TenantId, model.QuoteId);
            WebFormValidator.ValidateQuoteRequest(model.QuoteId, application, new ProductContext(productModel.TenantId, productModel.Id, environment));

            var attachFileToQuoteCommand = new AttachFileToQuoteCommand(
                    productModel.TenantId,
                    model.QuoteId,
                    model.AttachmentId,
                    model.FileName,
                    model.MimeType,
                    model.FileData);
            var quoteFileAttachment = await this.mediator.Send(attachFileToQuoteCommand);
            var attachedFileResult = FileAttachmentResultModel.CreateQuoteFileAttachmentResult(
                quoteFileAttachment.Id,
                quoteFileAttachment.QuoteId);
            return this.Ok(attachedFileResult);
        }

        /// <summary>
        /// Attach a file for claims.
        /// </summary>
        /// <param name="tenant">The ID or Alias of the tenant the quote is for.</param>
        /// <param name="product">The ID or Alias of the product to invoice on.</param>
        /// <param name="environment">The environment to use.</param>
        /// <param name="model">Resource model with new form data.</param>
        /// <returns>File attachment result.</returns>
        [HttpPost]
        [RequestRateLimit(Period = 20, Type = RateLimitPeriodType.Seconds, Limit = 20)]
        [ValidateModel]
        [Route("api/v1/{tenant}/{environment}/{product}/claim/attachment")]
        [ProducesResponseType(typeof(FileAttachmentResultModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> AttachFileToClaim(
            string tenant, string product, DeploymentEnvironment environment, [FromBody] ClaimFileAttachmentModel model)
        {
            Product productModel = await this.cachingResolver.GetProductOrThrow(new GuidOrAlias(tenant), new GuidOrAlias(product));
            var claim = this.claimAggregateRepository.GetById(productModel.TenantId, model.ClaimId);
            claim = EntityHelper.ThrowIfNotFound(claim, model.ClaimId, "Claim");
            WebFormValidator.ValidateClaimRequest(model.ClaimId, claim, new ProductContext(productModel.TenantId, productModel.Id, environment));
            var claimAttachment = await this.claimApplicationFileAttachmentService
                .AttachFile(productModel.TenantId, model.ClaimId, model.FileName, model.MimeType, model.FileData);

            var attachedFileResult = FileAttachmentResultModel.CreateClaimFileAttachmentResult(
                    claimAttachment.FileContentId, claimAttachment.ClaimId);
            return this.Ok(attachedFileResult);
        }
    }
}
