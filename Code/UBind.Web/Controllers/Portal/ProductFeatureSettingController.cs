// <copyright file="ProductFeatureSettingController.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using UBind.Application.Commands.User;
    using UBind.Application.ExtensionMethods;
    using UBind.Domain;
    using UBind.Domain.Enums;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Permissions;
    using UBind.Domain.Product;
    using UBind.Domain.Services;
    using UBind.Web.Filters;
    using UBind.Web.ResourceModels;

    /// <summary>ManageTransactionType
    /// Controller for product feature.
    /// </summary>
    [Produces("application/json")]
    [Route("/api/v1/product-feature-setting")]
    [MustBeLoggedIn]
    public class ProductFeatureSettingController : PortalBaseController
    {
        private readonly IProductFeatureSettingService productFeatureService;
        private readonly ICqrsMediator mediator;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductFeatureSettingController"/> class.
        /// </summary>
        /// <param name="productFeatureService">The product feature service.</param>
        /// <param name="mediator">The mediator.</param>
        /// <param name="cachingResolver">The caching resolver.</param>
        public ProductFeatureSettingController(
            IProductFeatureSettingService productFeatureService,
            ICqrsMediator mediator,
            ICachingResolver cachingResolver)
            : base(cachingResolver)
        {
            this.mediator = mediator;
            this.productFeatureService = productFeatureService;
        }

        /// <summary>
        /// Gets product features for a given tenant with confirmed releases on given environment.
        /// </summary>
        /// <param name="tenant">The tenant Id or Alias.</param>
        /// <param name="environment">The deployment environment.</param>
        /// <returns>The list of product features.</returns>
        [HttpGet]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [ProducesResponseType(typeof(IEnumerable<ProductFeatureSettingResourceModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetProductFeaturesByDeployedEnvironment(
            [FromQuery] string tenant, [FromQuery] DeploymentEnvironment environment)
        {
            var tenantModel = await this.CachingResolver.GetTenantOrThrow(new GuidOrAlias(tenant));
            List<ProductFeatureSetting> filteredProductFeature =
                this.productFeatureService.GetEnabledDeployedProductFeatureSettings(tenantModel.Id, environment);
            var productFeaturesModel = filteredProductFeature
                .Select(u => new ProductFeatureSettingResourceModel(u));
            return this.Ok(productFeaturesModel);
        }

        /// <summary>
        /// Gets the product features for a given tenant and product.
        /// </summary>
        /// <param name="product">The product Id or Alias.</param>
        /// <param name="tenant">The tenant Id or Alias.</param>
        /// <returns>The list of product features.</returns>
        [HttpGet]
        [Route("{product}")]
        [Route("/api/v1/product/{product}/feature-setting")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [ProducesResponseType(typeof(ProductFeatureSettingResourceModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetProductFeatureSetting(string product, [FromQuery] string tenant = null)
        {
            tenant = tenant ?? this.User.GetTenantIdOrNull()?.ToString();
            var tenantModel = await this.CachingResolver.GetTenantOrThrow(new GuidOrAlias(tenant));
            var productModel = await this.CachingResolver.GetProductOrThrow(tenantModel, new GuidOrAlias(product));
            this.VerifyProductFeaturePermission(tenantModel.Id, this.User.GetTenantId(), "Get");
            ProductFeatureSetting productFeature = this.productFeatureService.GetProductFeature(tenantModel.Id, productModel.Id);
            var productFeaturesModel = new ProductFeatureSettingResourceModel(productFeature);
            return this.Ok(productFeaturesModel);
        }

        /// <summary>
        /// Enable product feature.
        /// </summary>
        /// <param name="product">The product Id or Alias.</param>
        /// <param name="productFeatureSettingItem">The product feature type.</param>
        /// <param name="tenant">The tenant Id or Alias.</param>
        /// <returns>The list of product features.</returns>
        [HttpPatch]
        [Route("{product}/feature/{productFeatureSettingItem}/enable")]
        [Route("/api/v1/product/{product}/feature/{productFeatureSettingItem}/enable")]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 30)]
        [MustHavePermission(Permission.ManageProducts)]
        [ProducesResponseType(typeof(ProductFeatureSettingResourceModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> EnableProductFeature(
            string product,
            ProductFeatureSettingItem productFeatureSettingItem,
            [FromQuery] string tenant = null)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(
                 tenant,
                 "update product feature settings",
                 "your user account does not have access to the master tenancy");
            var tenantModel = await this.CachingResolver.GetTenantOrNull(tenantId);
            this.VerifyProductFeaturePermission(tenantModel?.Id ?? Guid.Empty, this.User.GetTenantId(), "Enable");
            var productModel = await this.CachingResolver.GetProductOrThrow(tenantModel, new GuidOrAlias(product));
            var productFeature
                = this.productFeatureService.EnableProductFeature(tenantModel.Id, productModel.Id, productFeatureSettingItem);
            ProductFeatureSettingResourceModel productFeatureModel
                = new ProductFeatureSettingResourceModel(productFeature);
            return this.Ok(productFeatureModel);
        }

        /// <summary>
        /// Disable product feature.
        /// </summary>
        /// <param name="product">The product Id or Alias.</param>
        /// <param name="productFeatureSettingItem">The product feature type.</param>
        /// <param name="tenant">The tenant Id or Alias.</param>
        /// <returns>The list of product features.</returns>
        [HttpPatch]
        [Route("{product}/feature/{productFeatureSettingItem}/disable")]
        [Route("/api/v1/product/{product}/feature/{productFeatureSettingItem}/disable")]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 30)]
        [MustHavePermission(Permission.ManageProducts)]
        [ProducesResponseType(typeof(ProductFeatureSettingResourceModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> DisableProductFeature(
            string product,
            ProductFeatureSettingItem productFeatureSettingItem,
            [FromQuery] string tenant = null)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(
                 tenant,
                 "update product feature settings",
                 "your user account does not have access to the master tenancy");
            var tenantModel = await this.CachingResolver.GetTenantOrNull(tenantId);
            this.VerifyProductFeaturePermission(tenantModel?.Id ?? Guid.Empty, this.User.GetTenantId(), "Disable");
            var productModel = await this.CachingResolver.GetProductOrThrow(tenantModel, new GuidOrAlias(product));
            var productFeature
                = this.productFeatureService.DisableProductFeature(tenantModel.Id, productModel.Id, productFeatureSettingItem);
            ProductFeatureSettingResourceModel productFeatureModel
                = new ProductFeatureSettingResourceModel(productFeature);
            return this.Ok(productFeatureModel);
        }

        /// <summary>
        /// Update product feature additional setting.
        /// </summary>
        /// <param name="tenant">The tenant Id or Alias.</param>
        /// <param name="product">The product Id or Alias.</param>
        /// <param name="productFeatureAddionalSettingModel">The product feature additional setting model.</param>
        /// <returns>The list of product features.</returns>
        [HttpPatch]
        [Route("/api/v1/product-feature-setting/{product}/feature/renewal/additional-settings")]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 30)]
        [MustHavePermission(Permission.ManageProducts)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateProductFeatureAdditionalSetting([FromQuery] string tenant, string product, [FromBody] ProductFeatureRenewalSettingModel productFeatureAddionalSettingModel)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(
                 tenant,
                 "update product feature settings",
                 "your user account does not have access to the master tenancy");
            var tenantModel = await this.CachingResolver.GetTenantOrNull(tenantId);
            this.VerifyProductFeaturePermission(tenantModel?.Id ?? Guid.Empty, this.User.GetTenantId(), "Update product feature additional setting");
            var productModel = await this.CachingResolver.GetProductOrThrow(tenantModel, new GuidOrAlias(product));
            var command = new UpdateProductFeatureRenewalSettingCommand(
                tenantModel.Id,
                productModel.Id,
                productFeatureAddionalSettingModel.AllowRenewalAfterExpiry,
                productFeatureAddionalSettingModel.ExpiredPolicyRenewalPeriodSeconds);
            await this.mediator.Send(command);

            return this.Ok();
        }

        /// <summary>
        /// Update cancellation setting.
        /// </summary>
        /// <param name="tenant">The tenant Id or Alias.</param>
        /// <param name="product">The product Id or Alias.</param>
        /// <param name="refundPolicyModel">The cancellation setting model.</param>
        /// <returns>Ok.</returns>
        [HttpPatch]
        [Route("/api/v1/product-feature-setting/{product}/refund-settings")]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 30)]
        [MustHavePermission(Permission.ManageProducts)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateCancellationSetting([FromQuery] string tenant, string product, [FromBody] RefundSettingsModel refundPolicyModel)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(
                 tenant,
                 "update product feature settings",
                 "your user account does not have access to the master tenancy");
            var tenantModel = await this.CachingResolver.GetTenantOrNull(tenantId);
            this.VerifyProductFeaturePermission(tenantModel.Id, this.User.GetTenantId(), "Update product cancellation setting");
            var productModel = await this.CachingResolver.GetProductOrThrow(tenantModel, new GuidOrAlias(product));
            var command = new UpdateRefundSettingsCommand(
                tenantModel.Id,
                productModel.Id,
                refundPolicyModel.RefundPolicy,
                refundPolicyModel.PeriodWhichNoClaimsMade,
                refundPolicyModel.LastNumberOfYearsWhichNoClaimsMade);
            await this.mediator.Send(command);

            return this.Ok();
        }

        private void VerifyProductFeaturePermission(
            Guid tenantId,
            Guid userTenantId,
            string action)
        {
            if (userTenantId != Tenant.MasterTenantId && userTenantId != tenantId)
            {
                throw new ErrorException(Errors.General.NotAuthorized(action, "ProductFeature"));
            }
        }
    }
}
