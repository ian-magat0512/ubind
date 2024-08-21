// <copyright file="SystemAlertController.cs" company="uBind">
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
    using UBind.Application.Authorisation;
    using UBind.Domain;
    using UBind.Domain.Enums;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Permissions;
    using UBind.Domain.Services;
    using UBind.Web.Filters;
    using UBind.Web.ResourceModels;

    /// <summary>
    /// Controller for tenant requests.
    /// </summary>
    [Produces("application/json")]
    [Route("api/v1/system-alert/")]
    [MustBeLoggedIn(Policy = UserTypePolicies.ClientOrMaster)]
    public class SystemAlertController : Controller
    {
        private readonly ISystemAlertService systemAlertService;
        private readonly IAuthorisationService authorisationService;
        private readonly ICachingResolver cachingResolver;
        private readonly ICqrsMediator mediator;

        /// <summary>
        /// Initializes a new instance of the <see cref="SystemAlertController"/> class.
        /// </summary>
        /// <param name="systemAlertService">The System Alert Service.</param>
        /// <param name="authorisationService">The authorisation Service.</param>
        /// <param name="cachingResolver">The caching resolver.</param>
        public SystemAlertController(
            ISystemAlertService systemAlertService,
            IAuthorisationService authorisationService,
            ICachingResolver cachingResolver,
            ICqrsMediator mediator)
        {
            this.systemAlertService = systemAlertService;
            this.authorisationService = authorisationService;
            this.cachingResolver = cachingResolver;
            this.mediator = mediator;
        }

        /// <summary>
        /// Gets all systemAlerts filter by tenant.
        /// </summary>
        /// <param name="tenant">The tenant Id or Alias.</param>
        /// <returns>All tenant systemAlerts.</returns>
        [HttpGet]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [MustHavePermission(Permission.ViewTenants)]
        [ProducesResponseType(typeof(IEnumerable<SystemAlertModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSystemAlertsByTenant([FromQuery] string tenant)
        {
            var tenantModel = await this.cachingResolver.GetTenantOrThrow(new GuidOrAlias(tenant));
            await this.authorisationService.ThrowIfUserCannotAccessTenant(tenantModel.Id, this.User, "view", "system alert", tenant);
            var systemAlerts = await this.systemAlertService
                .GetSystemAlertsByTenantId(tenantModel.Id);
            return this.Ok(systemAlerts.Select(u => new SystemAlertModel(u)));
        }

        /// <summary>
        /// Gets all systemAlerts filter by product.
        /// </summary>
        /// <param name="tenant">The tenant Id or Alias.</param>
        /// <param name="product">The product Id or Alias.</param>
        /// <returns>System Alerts Filtered by product Id.</returns>
        [HttpGet]
        [Route("product/{product}")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [MustHavePermission(Permission.ViewProducts)]
        [ProducesResponseType(typeof(IEnumerable<SystemAlertModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSystemAlertsByProduct([FromQuery] string tenant, string product)
        {
            var productModel = await this.cachingResolver.GetProductOrThrow(new GuidOrAlias(tenant), new GuidOrAlias(product));
            await this.authorisationService.ThrowIfUserCannotAccessTenant(productModel.TenantId, this.User, "view", "system alert");
            var systemAlerts = await this.systemAlertService
                .GetSystemAlertsByTenantIdAndProductId(productModel.TenantId, productModel.Id);
            return this.Ok(systemAlerts.Select(u => new SystemAlertModel(u)));
        }

        /// <summary>
        /// Gets all systemAlerts filter by system alert id.
        /// </summary>
        /// <param name="tenant">The tenant.</param>
        /// <param name="systemAlertId">The system alert id.</param>
        /// <returns>All tenant systemAlerts.</returns>
        [HttpGet]
        [Route("{systemAlertId}")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [MustHaveOneOfPermissions(Permission.ViewProducts, Permission.ViewTenants)]
        [ProducesResponseType(typeof(SystemAlertModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSystemAlertById([FromQuery] string tenant, Guid systemAlertId)
        {
            var tenantModel = await this.cachingResolver.GetTenantOrThrow(new GuidOrAlias(tenant));
            await this.authorisationService.ThrowIfUserCannotAccessTenant(tenantModel.Id, this.User, null, "system alert", systemAlertId);
            var systemAlert = this.systemAlertService.GetSystemAlertById(tenantModel.Id, systemAlertId);
            return this.Ok(new SystemAlertModel(systemAlert));
        }

        /// <summary>
        /// Update an existing System Alert.
        /// </summary>
        /// <param name="tenant">The tenant.</param>
        /// <param name="systemAlertId">The ID of the System Alert to update.</param>
        /// <param name="model">New System Alert details.</param>
        /// <returns>The updated setting.</returns>
        [HttpPut]
        [Route("{systemAlertId}")]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 15)]
        [ValidateModel]
        [MustHaveOneOfPermissions(Permission.ManageProducts, Permission.ManageTenants)]
        [ProducesResponseType(typeof(SystemAlertModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateSystemAlert([FromQuery] string tenant, Guid systemAlertId, [FromBody] SystemAlertModel model)
        {
            var tenantModel = await this.cachingResolver.GetTenantOrThrow(new GuidOrAlias(tenant));
            await this.authorisationService.ThrowIfUserCannotAccessTenant(tenantModel.Id, this.User, null, "system alert", systemAlertId);
            var systemAlert = await this.systemAlertService.UpdateSystemAlert(tenantModel.Id, systemAlertId, model.WarningThreshold, model.CriticalThreshold);
            return this.Ok(new SystemAlertModel(systemAlert));
        }

        /// <summary>
        /// Disable system alert.
        /// </summary>
        /// <param name="tenant">The tenant.</param>
        /// <param name="systemAlertId">The System AlertID.</param>
        /// <returns>The updated setting.</returns>
        [HttpPut]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 15)]
        [ValidateModel]
        [Route("{systemAlertId}/disable")]
        [MustHaveOneOfPermissions(Permission.ManageProducts, Permission.ManageTenants)]
        [ProducesResponseType(typeof(SystemAlertModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> DisableSystemAlert([FromQuery] string tenant, Guid systemAlertId)
        {
            var tenantModel = await this.cachingResolver.GetTenantOrThrow(new GuidOrAlias(tenant));
            await this.authorisationService.ThrowIfUserCannotAccessTenant(tenantModel.Id, this.User, null, "system alert", systemAlertId);
            var systemAlert = await this.systemAlertService.DisableSystemAlert(tenantModel.Id, systemAlertId);
            return this.Ok(new SystemAlertModel(systemAlert));
        }

        /// <summary>
        /// Enable system alert.
        /// </summary>
        /// <param name="tenant">The tenant.</param>
        /// <param name="systemAlertId">The ID of the System Alert to update.</param>
        /// <returns>The updated setting.</returns>
        [HttpPut]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 15)]
        [ValidateModel]
        [Route("{systemAlertId}/enable")]
        [MustHaveOneOfPermissions(Permission.ManageProducts, Permission.ManageTenants)]
        [ProducesResponseType(typeof(SystemAlertModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> EnableSystemAlert([FromQuery] string tenant, Guid systemAlertId)
        {
            var tenantModel = await this.cachingResolver.GetTenantOrThrow(new GuidOrAlias(tenant));
            await this.authorisationService.ThrowIfUserCannotAccessTenant(tenantModel.Id, this.User, null, "system alert", systemAlertId);
            var enabledSystemAlert = await this.systemAlertService.EnableSystemAlert(tenantModel.Id, systemAlertId);
            return this.Ok(new SystemAlertModel(enabledSystemAlert));
        }
    }
}
