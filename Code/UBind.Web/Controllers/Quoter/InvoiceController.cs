// <copyright file="InvoiceController.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Controllers
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using NodaTime;
    using UBind.Application.Commands.Quote;
    using UBind.Domain;
    using UBind.Domain.Entities;
    using UBind.Domain.Enums;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Product;
    using UBind.Domain.Services;
    using UBind.Web.Filters;
    using UBind.Web.ResourceModels;
    using UBind.Web.ResourceModels.Quote;

    /// <summary>
    /// Controller for handling invoice issuance.
    /// </summary>
    [Produces("application/json")]
    [Route("api/v1/{tenant}/{environment}/{product}/invoice")]
    public class InvoiceController : Controller
    {
        private readonly ICqrsMediator cqrsMediator;
        private readonly ICachingResolver cachingResolver;
        private readonly IQuoteAggregateResolverService quoteAggregateResolver;
        private readonly IFileAttachmentRepository<QuoteFileAttachment> fileAttachmentRepository;
        private readonly IClock clock;

        /// <summary>
        /// Initializes a new instance of the <see cref="InvoiceController"/> class.
        /// </summary>
        /// <param name="mediator">The cqrs mediator.</param>
        /// <param name="quoteAggregateResolver">The quote aggregate resolver service.</param>
        /// <param name="cachingResolver">The product and tenant resolver.</param>
        /// <param name="fileAttachmentRepository">The file attachment repository.</param>
        /// <param name="clock">The clock.</param>
        public InvoiceController(
            ICqrsMediator mediator,
            IQuoteAggregateResolverService quoteAggregateResolver,
            ICachingResolver cachingResolver,
            IFileAttachmentRepository<QuoteFileAttachment> fileAttachmentRepository,
            IClock clock)
        {
            this.cachingResolver = cachingResolver;
            this.quoteAggregateResolver = quoteAggregateResolver;
            this.fileAttachmentRepository = fileAttachmentRepository;
            this.cqrsMediator = mediator;
            this.clock = clock;
        }

        /// <summary>
        /// Handle invoice requests.
        /// </summary>
        /// <param name="tenant">The ID or Alias of the tenant to invoice on.</param>
        /// <param name="product">The ID or Alias of the product to invoice on.</param>
        /// <param name="environment">The environment to use.</param>
        /// <param name="model">Model specifying the ID of the quote to issue the invoice for.</param>
        /// <returns>Updated application state.</returns>
        [HttpPost]
        [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 5)]
        [ValidateModel]
        [ProducesResponseType(typeof(QuoteApplicationModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> IssueInvoice(string tenant, string product, DeploymentEnvironment environment, [FromBody] InvoiceIssueModel model)
        {
            Product productModel = await this.cachingResolver.GetProductOrThrow(new GuidOrAlias(tenant), new GuidOrAlias(product));
            var quote = await this.cqrsMediator.Send(
                new IssueInvoiceCommand(
                    productModel.TenantId,
                    model.QuoteId));
            var outputModel = new QuoteApplicationModel(quote);
            return this.Ok(outputModel);
        }
    }
}
