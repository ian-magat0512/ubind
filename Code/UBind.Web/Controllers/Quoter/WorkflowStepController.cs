// <copyright file="WorkflowStepController.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Controllers.Quoter
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using UBind.Application.Commands.Claim;
    using UBind.Application.Commands.Quote;
    using UBind.Domain;
    using UBind.Domain.Enums;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Product;
    using UBind.Web.Filters;
    using UBind.Web.ResourceModels.Claim;
    using UBind.Web.ResourceModels.Quote;

    /// <summary>
    /// Controller for handling workflow-step related requests.
    /// </summary>
    [Produces("application/json")]
    [Route("/api/v1/{tenant}/{environment}/{product}")]
    public class WorkflowStepController : Controller
    {
        private readonly ICachingResolver cachingResolver;
        private readonly ICqrsMediator mediator;

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowStepController"/> class.
        /// </summary>
        /// <param name="cachingResolver">The tenant, product and portal resolver.</param>
        public WorkflowStepController(
            ICachingResolver cachingResolver,
            ICqrsMediator mediator)
        {
            this.cachingResolver = cachingResolver;
            this.mediator = mediator;
        }

        /// <summary>
        /// Records the change from one workflow step to another for a quote.
        /// </summary>
        /// <param name="tenant">The ID or Alias of the tenant.</param>
        /// <param name="product">The ID or Alias of the product to quote on.</param>
        /// <param name="environment">The environment to use.</param>
        /// <param name="model">A model with the quoteId and workflow step.</param>
        /// <returns>Ok.</returns>
        [HttpPost]
        [RequestRateLimit(Period = 20, Type = RateLimitPeriodType.Seconds, Limit = 30)]
        [ValidateModel]
        [Route("quote/workflowStep")]
        [ProducesResponseType(typeof(QuoteApplicationModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> RecordQuoteWorkflowStepChanged(
            string tenant,
            string product,
            DeploymentEnvironment environment,
            [FromBody] QuoteWorkflowStepChangedModel model)
        {
            Product productModel = await this.cachingResolver.GetProductOrThrow(new GuidOrAlias(tenant), new GuidOrAlias(product));
            var quote = await this.mediator.Send(new RecordQuoteWorkflowStepCommand(
                productModel.TenantId,
                model.QuoteId,
                model.WorkflowStep));
            var outputModel = new QuoteApplicationModel(quote);
            return this.Ok(outputModel);
        }

        /// <summary>
        /// Records the change from one workflow step to another for a claim.
        /// </summary>
        /// <param name="tenant">The tenant ID or Alias of the product to quote on.</param>
        /// <param name="product">The ID or Alias of the product to quote on.</param>
        /// <param name="environment">The environment to use.</param>
        /// <param name="model">A model with the claimId and workflow step.</param>
        /// <returns>Ok.</returns>
        [HttpPost]
        [RequestRateLimit(Period = 20, Type = RateLimitPeriodType.Seconds, Limit = 30)]
        [ValidateModel]
        [Route("claim/workflowStep")]
        [ProducesResponseType(typeof(ClaimApplicationModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> RecordClaimWorkflowStepChanged(
            string tenant,
            string product,
            DeploymentEnvironment environment,
            [FromBody] ClaimWorkflowStepChangedModel model)
        {
            Product productModel = await this.cachingResolver.GetProductOrThrow(new GuidOrAlias(tenant), new GuidOrAlias(product));
            var claim = await this.mediator.Send(new RecordClaimWorkflowStepCommand(
                productModel.TenantId,
                model.ClaimId,
                model.WorkflowStep));
            var outputModel = new ClaimApplicationModel(claim);
            return this.Ok(outputModel);
        }
    }
}
