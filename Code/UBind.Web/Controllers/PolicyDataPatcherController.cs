// <copyright file="PolicyDataPatcherController.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Web.Controllers.Portal
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using UBind.Application.Authorisation;
    using UBind.Application.ExtensionMethods;
    using UBind.Application.Services.PolicyDataPatcher;
    using UBind.Domain;
    using UBind.Domain.Enums;
    using UBind.Domain.Helpers;
    using UBind.Web.Extensions;
    using UBind.Web.Filters;
    using UBind.Web.ResourceModels;

    /// <summary>
    /// Controller for updating the quote or policy's form data.
    /// </summary>
    [Produces("application/json")]
    public class PolicyDataPatcherController : Controller
    {
        private readonly ICachingResolver cachingResolver;
        private readonly IPatchService patchService;
        private readonly IAuthorisationService authorisationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="PolicyDataPatcherController"/> class.
        /// </summary>
        /// <param name="patchService">The patch service to be used.</param>
        /// <param name="cachingResolver">The tenant and product resolver.</param>
        /// <param name="authorisationService">The authorization service.</param>
        public PolicyDataPatcherController(
            IPatchService patchService,
            ICachingResolver cachingResolver,
            IAuthorisationService authorisationService)
        {
            this.cachingResolver = cachingResolver;
            this.patchService = patchService;
            this.authorisationService = authorisationService;
        }

        /// <summary>
        /// Correct policy additions API.
        /// </summary>
        /// <param name="environment">The deployment environment to use.</param>
        /// <param name="tenant">The tenant ID or Alias to patch in.</param>
        /// <param name="organisationId">The organisation ID to patch in.</param>
        /// <param name="product">The product ID or Alias to patch in.</param>
        /// <param name="model">The product policy data patch command model class.</param>
        /// <returns>Returns a contract that represents the result of an action method.</returns>
        [HttpPost]
        [Route("/api/v1/tenant/{tenant}/organisation/{organisationId}/product/{product}/patchpolicydata")]
        [RequestRateLimit(Period = 20, Type = RateLimitPeriodType.Seconds, Limit = 10)]
        [ServiceFilter(typeof(ClientIPAddressFilterAttribute))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> PatchProductFormData(
            DeploymentEnvironment environment,
            string tenant,
            Guid organisationId,
            string product,
            [FromBody] IncomingPolicyDataPatchCommandModel model)
        {
            await this.authorisationService.ThrowIfUserIsNotFromMasterTenant(this.User, "patch product form data.");
            var productModel = await this.cachingResolver.GetProductOrThrow(new GuidOrAlias(tenant), new GuidOrAlias(product));
            this.patchService.QueuePatchProductFormData(productModel.TenantId, environment, organisationId, productModel.Id, model);
            return this.NoContent();
        }

        /// <summary>
        /// Patch the policy data.
        /// </summary>
        /// <param name="policyId">The policy Id to patch in.</param>
        /// <param name="tenant">The tenant alias or Id of the policy.</param>
        /// <param name="model">The product policy data patch command model in string.</param>
        /// <returns>Returns a contract that represents the result of an action method.</returns>
        [HttpPatch]
        [Route("/api/v1/policydata/{policyId}/data")]
        [RequestRateLimit(Period = 20, Type = RateLimitPeriodType.Seconds, Limit = 10)]
        [ServiceFilter(typeof(ClientIPAddressFilterAttribute))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> PatchPolicyFormData(Guid policyId, [FromQuery] string tenant, [FromBody] IncomingPolicyDataPatchCommandModel model)
        {
            var tenantIdOrAlias = !string.IsNullOrEmpty(tenant) ? tenant : this.User.GetTenantId().ToString();
            var tenantModel = await this.cachingResolver.GetTenantOrThrow(new GuidOrAlias(tenantIdOrAlias));

            await this.authorisationService.ThrowIfUserIsNotFromMasterTenant(this.User, "patch policy form data.");
            var result = await this.patchService.PatchPolicyDataAsync(tenantModel.Id, policyId, model.ToCommand());
            if (result.IsSuccess)
            {
                return this.Ok();
            }

            return Errors.General.BadRequest(result.Error).ToProblemJsonResult();
        }
    }
}
