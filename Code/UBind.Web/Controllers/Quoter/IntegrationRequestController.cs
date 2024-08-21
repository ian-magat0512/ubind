// <copyright file="IntegrationRequestController.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Controllers.Quoter
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using UBind.Application.Export;
    using UBind.Application.Services;
    using UBind.Domain;
    using UBind.Domain.Enums;
    using UBind.Domain.Helpers;
    using UBind.Web.Extensions;
    using UBind.Web.Filters;
    using UBind.Web.ResourceModels;

    /// <summary>
    /// Controller for webhook integration requests.
    /// </summary>
    [Produces("application/json")]
    [Route("/api/v1/{tenant}/{environment}/{product}")]
    public class IntegrationRequestController : Controller
    {
        private readonly IApplicationIntegrationRequestService requestService;
        private readonly ICachingResolver cachingResolver;

        /// <summary>
        /// Initializes a new instance of the <see cref="IntegrationRequestController"/> class.
        /// </summary>
        /// <param name="requestService">The integration request service.</param>
        /// <param name="cachingResolver">The tenant, product and portal resolver.</param>
        public IntegrationRequestController(
            IApplicationIntegrationRequestService requestService,
            ICachingResolver cachingResolver)
        {
            this.requestService = requestService;
            this.cachingResolver = cachingResolver;
        }

        /// <summary>
        /// Handles third-party webhook request.
        /// </summary>
        /// <param name="tenant">The ID or Alias of the tenant.</param>
        /// <param name="product">The ID or Alias of the product under the specific tenant.</param>
        /// <param name="environment">The environment to use.</param>
        /// <param name="requestModel">The request model.</param>
        /// <returns>A model.</returns>
        [HttpPost]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 15)]
        [ValidateModel]
        [Route("webhook-request")]
        [ProducesResponseType(typeof(WebServiceIntegrationResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> Execute(string tenant, string product, string environment, [FromBody] IntegrationRequestModel requestModel)
        {
            var isSuccess = Enum.TryParse<DeploymentEnvironment>(environment, true, out DeploymentEnvironment env);
            if (!isSuccess)
            {
                return Errors.General.NotFound("environment", environment).ToProblemJsonResult();
            }

            var productModel = await this.cachingResolver.GetProductOrThrow(new GuidOrAlias(tenant), new GuidOrAlias(product));
            var payloadJson = JsonConvert.SerializeObject(requestModel.FormData);
            var response = await this.requestService.ExecuteRequest(
                productModel.TenantId,
                requestModel.WebIntegrationId,
                (Guid)requestModel.QuoteId,
                productModel.Id,
                env,
                payloadJson);

            return this.StatusCode(
                (int)response.Code,
                (response.ResponseJson as string).IsValidJson() ? JsonConvert.DeserializeObject(response.ResponseJson) : JsonConvert.SerializeObject(response.ResponseJson));
        }
    }
}
