// <copyright file="FeatureSettingController.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using NodaTime;
    using UBind.Application;
    using UBind.Application.ExtensionMethods;
    using UBind.Application.Queries.Feature;
    using UBind.Domain;
    using UBind.Domain.Enums;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Permissions;
    using UBind.Domain.Repositories;
    using UBind.Web.Extensions;
    using UBind.Web.Filters;
    using UBind.Web.ResourceModels;

    /// <summary>
    /// Controller for feature setting requests.
    /// </summary>
    [Produces("application/json")]
    public class FeatureSettingController : Controller
    {
        private readonly ITenantRepository tenantRepository;
        private readonly ICachingResolver cachingResolver;
        private readonly IFeatureSettingService settingService;
        private readonly IClock clock;
        private readonly ICqrsMediator mediator;

        /// <summary>
        /// Initializes a new instance of the <see cref="FeatureSettingController"/> class.
        /// </summary>
        /// <param name="settingService">The setting service.</param>
        /// <param name="clock">A clock.</param>
        /// <param name="tenantRepository">The tenant repository.</param>
        /// <param name="cachingResolver">The tenant, product and portal resolver.</param>
        /// <param name="mediator">The Mediator.</param>
        public FeatureSettingController(
            IFeatureSettingService settingService,
            IClock clock,
            ITenantRepository tenantRepository,
            ICachingResolver cachingResolver,
            ICqrsMediator mediator)
        {
            this.tenantRepository = tenantRepository;
            this.cachingResolver = cachingResolver;
            this.settingService = settingService;
            this.clock = clock;
            this.mediator = mediator;
        }

        /// <summary>
        /// Gets all settings of a tenant.
        /// </summary>
        /// <remarks>This is deliberatly open and public, and doesn't require authentication.</remarks>
        /// <param name="tenant">The tenant ID or alise.</param>
        /// <returns>All tenant settings.</returns>
        [HttpGet]
        [Route("api/v1/tenant/{tenant}/feature-setting")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [ProducesResponseType(typeof(IEnumerable<SettingModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTenantSettings(string tenant)
        {
            Tenant tenantModel = await this.cachingResolver.GetTenantOrThrow(new GuidOrAlias(tenant));
            var command = new GetFeatureSettingsQuery(tenantModel.Id);
            var settings = await this.mediator.Send(command);
            var settingsModel = settings.Select(s => new SettingModel(s));
            return this.Ok(settingsModel);
        }

        /// <summary>
        /// Gets all settings of a portal.
        /// </summary>
        /// <remarks>This is deliberatly open and public, and doesn't require authentication.</remarks>
        /// <param name="tenant">The tenant ID or alias.</param>
        /// <param name="portal">The portal ID or alias.</param>
        /// <returns>All tenant settings.</returns>
        [HttpGet]
        [Route("api/v1/tenant/{tenant}/portal/{portal}/feature-setting")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [ProducesResponseType(typeof(IEnumerable<SettingModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPortalSettings(string tenant, string portal)
        {
            Tenant tenantModel = await this.cachingResolver.GetTenantOrThrow(new GuidOrAlias(tenant));
            var portalModel = await this.cachingResolver.GetPortalOrThrow(tenantModel.Id, new GuidOrAlias(portal));

            // TODO: Get portal specific settings
            var command = new GetFeatureSettingsQuery(tenantModel.Id);
            var settings = await this.mediator.Send(command);
            var settingsModel = settings.Select(s => new SettingModel(s, portalModel.Id));

            return this.Ok(settingsModel);
        }

        /// <summary>
        /// Gets all settings of a portal of a agent tenancy.
        /// </summary>
        /// <remarks>This is deliberatly open and public, and doesn't require authentication.</remarks>
        /// <param name="portal">The portal ID or alias.</param>
        /// <param name="tenant">The tenant ID or alias.</param>
        /// <returns>All tenant settings.</returns>
        [HttpGet]
        [Route("api/v1/portal/{portal}/feature-setting")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [ProducesResponseType(typeof(IEnumerable<SettingModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAgentPortalSettings(string portal, [FromQuery] string tenant)
        {
            var tenantIdOrAlias = tenant ?? this.User.GetTenantId().ToString();
            var tenantModel = await this.cachingResolver.GetTenantOrThrow(new GuidOrAlias(tenantIdOrAlias));
            var portalModel = await this.cachingResolver.GetPortalOrThrow(tenantModel.Id, new GuidOrAlias(portal));

            // TODO: Get portal specific settings
            var command = new GetFeatureSettingsQuery(tenantModel.Id);
            var settings = await this.mediator.Send(command);
            var settingsModel = settings.Select(s => new SettingModel(s, portalModel.Id));
            return this.Ok(settingsModel);
        }

        /// <summary>
        /// Update an existing Feature setting for a tenant.
        /// </summary>
        /// <param name="tenant">The tenant ID.</param>
        /// <param name="settingId">The ID of the setting to update.</param>
        /// <param name="model">New setting details.</param>
        /// <returns>The updated setting.</returns>
        [HttpPut]
        [Route("api/v1/tenant/{tenant}/feature-setting/{settingId}")]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 15)]
        [ValidateModel]
        [MustHavePermission(Permission.ManageTenants)]
        [ProducesResponseType(typeof(SettingModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateTenantSetting(string tenant, string settingId, [FromBody] SettingModel model)
        {
            var userTenantId = this.User.GetTenantId();
            var tenantModel = await this.cachingResolver.GetTenantOrThrow(new GuidOrAlias(tenant));
            if (userTenantId != Tenant.MasterTenantId && userTenantId != tenantModel.Id)
            {
                return Errors.General.Forbidden("update the feature settings for a tenant", "your user account doesn't have access to the master tenancy").ToProblemJsonResult();
            }

            // We need to fetch a copy of the tenant from the db so it's in the dbContext, ready for our write operation
            // So that EF can know about the foreign key. If we don't fetch it, then EF will thing we're trying to insert
            // a new copy, and then will complain about a duplicate key.
            var dbContextTenant = this.tenantRepository.GetTenantById(tenantModel.Id);
            var settingDetails = new SettingDetails(model.Disabled, dbContextTenant, this.clock.GetCurrentInstant());
            var serviceResult = await this.settingService.UpdateSetting(settingId, settingDetails);
            var result = new SettingModel(serviceResult);
            return this.Ok(result);
        }

        /// <summary>
        /// Update an existing feature setting for a portal.
        /// </summary>
        /// <param name="tenant">The tenant ID or alias.</param>
        /// <param name="portal">The portal ID or alias.</param>
        /// <param name="settingId">The ID of the setting to update.</param>
        /// <param name="model">New setting details.</param>
        /// <returns>The updated setting.</returns>
        [HttpPut]
        [Route("api/v1/tenant/{tenantId}/portal/{portal}/feature-setting/{settingId}")]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 15)]
        [ValidateModel]
        [MustHavePermission(Permission.ManagePortals)]
        [ProducesResponseType(typeof(SettingModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateSetting(string tenant, string portal, string settingId, [FromBody] SettingModel model)
        {
            var userTenantId = this.User.GetTenantId();
            var tenantModel = await this.cachingResolver.GetTenantOrThrow(new GuidOrAlias(tenant));
            if (userTenantId != Tenant.MasterTenantId && userTenantId != tenantModel.Id)
            {
                return Errors.General.Forbidden("update the feature settings for a tenant", "your user account doesn't have access to the master tenancy").ToProblemJsonResult();
            }

            // We need to fetch a copy of the tenant from the db so it's in the dbContext, ready for our write operation
            // So that EF can know about the foreign key. If we don't fetch it, then EF will thing we're trying to insert
            // a new copy, and then will complain about a duplicate key.
            var dbContextTenant = this.tenantRepository.GetTenantById(tenantModel.Id);
            var settingDetails = new SettingDetails(model.Disabled, dbContextTenant, this.clock.GetCurrentInstant());
            var serviceResult = await this.settingService.UpdateSetting(settingId, settingDetails);
            var result = new SettingModel(serviceResult);
            return this.Ok(result);
        }
    }
}
