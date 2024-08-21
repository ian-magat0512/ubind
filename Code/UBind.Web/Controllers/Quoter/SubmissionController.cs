// <copyright file="SubmissionController.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Web.Controllers
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using UBind.Application.Commands.Quote;
    using UBind.Domain;
    using UBind.Domain.Enums;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Product;
    using UBind.Web.Filters;
    using UBind.Web.ResourceModels.Quote;

    /// <summary>
    /// Controller for handling application submissions.
    /// </summary>
    [Produces("application/json")]
    [Route("/api/v1/{tenant}/{environment}/{product}")]
    public class SubmissionController : Controller
    {
        private readonly ICachingResolver cachingResolver;
        private readonly ICqrsMediator mediator;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubmissionController"/> class.
        /// </summary>
        /// <param name="cachingResolver">The tenant, product and portal resolver.</param>
        public SubmissionController(
            ICachingResolver cachingResolver,
            ICqrsMediator mediator)
        {
            this.cachingResolver = cachingResolver;
            this.mediator = mediator;
        }

        /// <summary>
        /// Submits an application.
        /// </summary>
        /// <param name="tenant">The ID or Alias of the tenant the quote is for.</param>
        /// <param name="product">The ID or Alias of the product to quote on.</param>
        /// <param name="environment">The environment to use.</param>
        /// <param name="model">Resource model with new form data.</param>
        /// <returns>Ok.</returns>
        [HttpPost]
        [RequestRateLimit(Period = 20, Type = RateLimitPeriodType.Seconds, Limit = 10)]
        [Route("submission")]
        [ValidateModel]
        [ProducesResponseType(typeof(QuoteApplicationModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateSubmission(
            string tenant, string product, DeploymentEnvironment environment, [FromBody] QuoteFormDataUpdateModel model)
        {
            Product productModel = await this.cachingResolver.GetProductOrThrow(new GuidOrAlias(tenant), new GuidOrAlias(product));
            var quote = await this.mediator.Send(new SubmitQuoteCommand(
                productModel.TenantId,
                model.QuoteId,
                model.FormDataJson != null ? new Domain.Aggregates.Quote.FormData(model.FormDataJson) : null));
            var outputModel = new QuoteApplicationModel(quote);
            return this.Ok(outputModel);
        }
    }
}
