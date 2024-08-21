// <copyright file="NumberPoolController.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
namespace UBind.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using UBind.Application.Authorisation;
    using UBind.Application.Queries.Number;
    using UBind.Domain;
    using UBind.Domain.Enums;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Permissions;
    using UBind.Domain.Services;
    using UBind.Web.Extensions;
    using UBind.Web.Filters;
    using UBind.Web.ResourceModels;

    /// <summary>
    /// Controller for handling invoice numbers.
    /// </summary>
    [Produces("application/json")]
    [Route("/api/v1/tenant/{tenant}/product/{product}/number-pool/{numberPoolId}")]
    public class NumberPoolController : Controller
    {
        private readonly INumberPoolService numberPoolService;
        private readonly ICachingResolver cacheResolver;
        private readonly IAuthorisationService authorisationService;
        private readonly ICqrsMediator mediator;

        /// <summary>
        /// Initializes a new instance of the <see cref="NumberPoolController"/> class.
        /// </summary>
        /// <param name="numberPoolService">The number pool service.</param>
        /// <param name="cacheResolver">The caching resolver.</param>
        /// <param name="authorisationService">The authorization service.</param>
        public NumberPoolController(
            INumberPoolService numberPoolService,
            ICachingResolver cacheResolver,
            IAuthorisationService authorisationService,
            ICqrsMediator mediator)
        {
            this.numberPoolService = numberPoolService;
            this.cacheResolver = cacheResolver;
            this.authorisationService = authorisationService;
            this.mediator = mediator;
        }

        /// <summary>
        /// Handles the submission of new numbers in the pool of numbers of that type for a given product id and environment.
        /// </summary>
        /// <param name="tenant">The Id or Alias of the tenant the numbers are for.</param>
        /// <param name="product">The Id or Alias of the product the numbers are for.</param>
        /// <param name="numberPoolId">The ID of the number pool.</param>
        /// <param name="numbers">The new numbers to be added.</param>
        /// <param name="environment">The environment for wich the numbers are for. Defaults to "production".</param>
        /// <returns>ActionResult OK.</returns>
        [HttpPost]
        [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 5)]
        [ValidateModel]
        [RequestFormLimits(ValueCountLimit = 350000)]
        [MustHavePermission(Permission.ManageProducts)]
        [ProducesResponseType(typeof(NumberPoolAddResultDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> AddNumbers(
            string tenant,
            string product,
            string numberPoolId,
            [FromBody] IEnumerable<string> numbers,
            [FromQuery] string environment = "production")
        {
            var parseEnvironment = Enum.TryParse<DeploymentEnvironment>(
                environment, true, out DeploymentEnvironment env);
            if (!parseEnvironment)
            {
                return Errors.General.NotFound("environment", environment).ToProblemJsonResult();
            }

            var productModel = await this.cacheResolver.GetProductOrThrow(new GuidOrAlias(tenant), new GuidOrAlias(product));
            await this.authorisationService.ThrowIfUserCannotAccessTenant(productModel.TenantId, this.User, "add", numberPoolId);

            var loadPolicyNumbersResult = this.numberPoolService.Add(
                productModel.TenantId,
                productModel.Id,
                numberPoolId,
                env,
                numbers);
            var resouceNumbersLoadResultModel = new NumberPoolAddResultDto(
                loadPolicyNumbersResult.AddedNumbers,
                loadPolicyNumbersResult.DuplicateNumbers,
                env);
            return this.Ok(resouceNumbersLoadResultModel);
        }

        /// <summary>
        /// Returns a collection of available invoice numbers for a given product and environment.
        /// </summary>
        /// <param name="tenant">The Id or Alias of the tenant the invoice numbers are for.</param>
        /// <param name="product">The Id or Alias of the product the invoice numbers are for.</param>
        /// <param name="numberPoolId">The ID of the number pool.</param>
        /// <param name="environment">The environment for wich the numbers are for. Defaults to "production".</param>
        /// <returns>Ok.</returns>
        [HttpGet]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [ValidateModel]
        [MustHavePermission(Permission.ManageProducts)]
        [ProducesResponseType(typeof(NumberPoolGetResultDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetNumbers(
            string tenant,
            string product,
            string numberPoolId,
            [FromQuery] DeploymentEnvironment environment = DeploymentEnvironment.Production)
        {
            var getAllNumbersFromPoolQuery = new GetAllNumbersFromPoolQuery(this.User, tenant, product, numberPoolId, environment);
            var result = await this.mediator.Send(getAllNumbersFromPoolQuery);
            var model = new NumberPoolGetResultDto(result.ProductId, environment, result.Numbers);
            return this.Ok(model);
        }

        /// <summary>
        /// Returns a collection of available invoice numbers for a given product and environment.
        /// </summary>
        /// <param name="tenant">The Id or Alias of the tenant the invoice numbers are for.</param>
        /// <param name="product">The Id or Alias of the product the invoice numbers are for.</param>
        /// <param name="numberPoolId">The ID of the number pool.</param>
        /// <param name="environment">The environment for wich the numbers are for. Defaults to "production".</param>
        /// <returns>Ok.</returns>
        [HttpGet]
        [Route("available")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [ValidateModel]
        [MustHavePermission(Permission.ManageProducts)]
        [ProducesResponseType(typeof(NumberPoolGetResultDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAvailableNumbers(
            string tenant,
            string product,
            string numberPoolId,
            [FromQuery] DeploymentEnvironment environment = DeploymentEnvironment.Production)
        {
            var getAvailableNumbersFromPoolQuery = new GetAvailableNumbersFromPoolQuery(this.User, tenant, product, numberPoolId, environment);
            var result = await this.mediator.Send(getAvailableNumbersFromPoolQuery);
            var model = new NumberPoolGetResultDto(result.ProductId, environment, result.Numbers);
            return this.Ok(model);
        }

        /// <summary>
        /// Handles the submission of invoice numbers to delete for a given product and environment.
        /// </summary>
        /// <param name="tenant">The Id or Alias of the tenant the invoice numbers are for.</param>
        /// <param name="product">The Id or Alias of the product the invoice numbers are from.</param>
        /// <param name="numberPoolId">The ID of the number pool.</param>
        /// <param name="numbers">The numbers to delete.</param>
        /// <param name="environment">The environment for wich the numbers are for. Defaults to "production".</param>
        /// <returns>Ok.</returns>
        [HttpDelete]
        [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 5)]
        [ValidateModel]
        [MustHavePermission(Permission.ManageProducts)]
        [ProducesResponseType(typeof(NumberPoolDeleteResultDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteNumbers(
            string tenant,
            string product,
            string numberPoolId,
            [FromBody] IEnumerable<string> numbers,
            [FromQuery] DeploymentEnvironment environment = DeploymentEnvironment.Production)
        {
            var productModel = await this.cacheResolver.GetProductOrThrow(new GuidOrAlias(tenant), new GuidOrAlias(product));
            await this.authorisationService.ThrowIfUserCannotAccessTenant(productModel.TenantId, this.User, "delete", numberPoolId);
            var deletedInvoiceNumbers = this.numberPoolService.Remove(
                productModel.TenantId,
                productModel.Id,
                numberPoolId,
                environment,
                numbers);
            return this.Ok(new NumberPoolDeleteResultDto(deletedInvoiceNumbers.ToList(), environment));
        }

        /// <summary>
        /// Returns a collection of available numbers for a given product and environment.
        /// </summary>
        /// <param name="tenant">The Id or Alias of the tenant the invoice numbers are for.</param>
        /// <param name="product">The Id or Alias of the product the invoice numbers are for.</param>
        /// <param name="numberPoolId">The ID of the number pool.</param>
        /// <param name="environment">The environment for wich the numbers are for. Defaults to "production".</param>
        /// <returns>Ok.</returns>
        [HttpGet]
        [Route("has-available-numbers")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [ValidateModel]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<IActionResult> HasAvailableNumbers(
            string tenant,
            string product,
            string numberPoolId,
            [FromQuery] DeploymentEnvironment environment = DeploymentEnvironment.Production)
        {
            var getAvailableNumbersFromPoolQuery = new GetAvailableNumbersFromPoolQuery(this.User, tenant, product, numberPoolId, environment);
            var result = await this.mediator.Send(getAvailableNumbersFromPoolQuery);
            return this.Ok(result.Numbers.Any());
        }
    }
}
