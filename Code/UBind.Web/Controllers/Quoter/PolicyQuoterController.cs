// <copyright file="PolicyQuoterController.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Controllers.Quoter
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json.Linq;
    using UBind.Domain;
    using UBind.Domain.Commands.Policy;
    using UBind.Domain.Enums;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Product;
    using UBind.Web.Extensions;
    using UBind.Web.Filters;
    using UBind.Web.ResourceModels;
    using UBind.Web.ResourceModels.Quote;

    /// <summary>
    /// Controller for handling application submissions.
    /// </summary>
    [Produces("application/json")]
    [Route("/api/v1/{tenant}/{environment}/{product}")]
    public class PolicyQuoterController : Controller
    {
        private readonly ICachingResolver cachingResolver;
        private readonly ICqrsMediator cqrsMediator;

        /// <summary>
        /// Initializes a new instance of the <see cref="PolicyQuoterController"/> class.
        /// </summary>
        /// <param name="cachingResolver">The product and tenant resolver.</param>
        /// <param name="mediator">The cqrs mediator.</param>
        public PolicyQuoterController(
            ICachingResolver cachingResolver,
            ICqrsMediator mediator)
        {
            this.cachingResolver = cachingResolver;
            this.cqrsMediator = mediator;
        }

        /// <summary>
        /// Handles policy creation requests.
        /// </summary>
        /// <param name="tenant">The ID or Alias of the tenant to quote on.</param>
        /// <param name="product">The ID or Alias of the product to quote on.</param>
        /// <param name="environment">The environment to use.</param>
        /// <param name="model">Resource model with new form data.</param>
        /// <returns>Ok.</returns>
        [HttpPost]
        [RequestRateLimit(Period = 20, Type = RateLimitPeriodType.Seconds, Limit = 10)]
        [ValidateModel]
        [Route("policy")]
        [ProducesResponseType(typeof(QuoteApplicationModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> CompleteTransaction(string tenant, string product, DeploymentEnvironment environment, [FromBody] BindRequestModel model)
        {
            Product productModel = await this.cachingResolver.GetProductOrThrow(new GuidOrAlias(tenant), new GuidOrAlias(product));

            try
            {
                var completePolicyTransactionCommand = new CompletePolicyTransactionCommand(
                        productModel.TenantId,
                        model.QuoteId,
                        model.CalculationResultId,
                        new Domain.Aggregates.Quote.FormData(new JObject(model.FormDataJson)));
                var quote = await this.cqrsMediator.Send(completePolicyTransactionCommand);
                var outputModel = new QuoteApplicationModel(quote);
                return this.Ok(outputModel);
            }
            catch (Exception ex)
            {
                if (ex is InvalidCalculationTriggerException || ex is InvalidOperationException)
                {
                    return Errors.General.BadRequest(ex.Message).ToProblemJsonResult();
                }

                throw;
            }
        }
    }
}
