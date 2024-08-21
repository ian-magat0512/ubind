// <copyright file="ProductDeploymentSettingController.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Controllers
{
    using System.ComponentModel.DataAnnotations;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using UBind.Application;
    using UBind.Domain;
    using UBind.Domain.Enums;
    using UBind.Domain.Helpers;
    using UBind.Domain.Permissions;
    using UBind.Domain.Product;
    using UBind.Web.Filters;

    /// <summary>
    /// Controller for product requests.
    /// </summary>
    [Produces("application/json")]
    [Route("/api/v1/{environment}/{tenant}/{product}/productdeploymentsettings")]
    [Route("/api/v1/tenant/{tenant}/organisation/{organisationId}/product/{product}/productdeploymentsettings")]
    [Route("/api/v1/tenant/{tenant}/product/{product}/productdeploymentsettings")]
    public class ProductDeploymentSettingController : PortalBaseController
    {
        private readonly IProductService productService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductDeploymentSettingController"/> class.
        /// </summary>
        /// <param name="productService">The product application service.</param>
        /// <param name="cachingResolver">The tenant, product and portal resolver.</param>
        public ProductDeploymentSettingController(
            IProductService productService,
            ICachingResolver cachingResolver)
            : base(cachingResolver)
        {
            this.productService = productService;
        }

        /// <summary>
        /// retrieve an existing product deployment.
        /// </summary>
        /// <param name="tenant">The ID or Alias of the tenant.</param>
        /// <param name="product">The ID or Alias of the product.</param>
        /// <returns>the existing product deployment.</returns>
        [HttpGet]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [MustHavePermission(Permission.ViewProducts)]
        [ValidateModel]
        [ProducesResponseType(typeof(ProductDeploymentSetting), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDeployments(
            [Required] string tenant, [Required] string product)
        {
            var tenantModel = await this.CachingResolver.GetTenantOrThrow(new GuidOrAlias(tenant));
            var productModel = await this.CachingResolver.GetProductOrThrow(tenantModel, new GuidOrAlias(product));
            var deploymentSettings = this.productService.GetDeploymentSettings(tenantModel.Id, productModel.Id);
            return this.Ok(deploymentSettings);
        }

        /// <summary>
        /// Update an existing product deployment.
        /// </summary>
        /// <param name="tenant">The ID or Alias of the tenant.</param>
        /// <param name="product">The ID or Alias of the product to update.</param>
        /// <param name="model">New deployment details.</param>
        /// <returns>The updated product.</returns>
        [HttpPut]
        [RequestRateLimit(Period = 20, Type = RateLimitPeriodType.Seconds, Limit = 10)]
        [ValidateModel]
        [MustHavePermission(Permission.ManageProducts)]
        [ProducesResponseType(typeof(ProductDeploymentSetting), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateDeployments(string tenant, string product, [FromBody] ProductDeploymentSetting model)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(
                 tenant,
                 "update the deployment targets for a product",
                 "your user account does not have access to the master tenancy");
            var tenantModel = await this.CachingResolver.GetTenantOrThrow(tenantId);
            var productModel = await this.CachingResolver.GetProductOrThrow(tenantModel, new GuidOrAlias(product));
            var productResult = this.productService.UpdateDeploymentSettings(tenantModel.Id, productModel.Id, model);
            return this.Ok(productResult.Details.DeploymentSetting);
        }
    }
}
