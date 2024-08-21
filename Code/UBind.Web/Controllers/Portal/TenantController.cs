// <copyright file="TenantController.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Web.Controllers
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.CodeAnalysis.CSharp;
    using UBind.Application.Commands.Tenant;
    using UBind.Application.ExtensionMethods;
    using UBind.Application.Queries.Tenant;
    using UBind.Domain;
    using UBind.Domain.Enums;
    using UBind.Domain.Extensions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Permissions;
    using UBind.Domain.Services.AdditionalPropertyValue;
    using UBind.Web.Extensions;
    using UBind.Web.Filters;
    using UBind.Web.ResourceModels;
    using UBind.Web.ResourceModels.Portal;

    /// <summary>
    /// Controller for tenant requests.
    /// </summary>
    [Produces("application/json")]
    [Route("api/v1/tenant")]
    public class TenantController : PortalBaseController
    {
        private readonly ICachingResolver cachingResolver;
        private readonly ICqrsMediator mediator;
        private readonly IAdditionalPropertyValueService additionalPropertyValueService;
        private readonly IHttpContextAccessor contextAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="TenantController"/> class.
        /// </summary>
        /// <param name="mediator">The mediator.</param>
        /// <param name="cachingResolver">The tenant, product and portal resolver.</param>
        /// <param name="contextAccessor">The Http context accessor.</param>
        /// <param name="additionalPropertyValueService">Additional property value service.</param>
        public TenantController(
            ICqrsMediator mediator,
            ICachingResolver cachingResolver,
            IHttpContextAccessor contextAccessor,
            IAdditionalPropertyValueService additionalPropertyValueService)
            : base(cachingResolver)
        {
            this.cachingResolver = cachingResolver;
            this.mediator = mediator;
            this.additionalPropertyValueService = additionalPropertyValueService;
            this.contextAccessor = contextAccessor;
        }

        /// <summary>
        /// Gets all tenants based on the options.
        /// </summary>
        /// <param name="options">The filter options.</param>
        /// <returns>All tenants.</returns>
        [HttpGet]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [MustHavePermission(Permission.ViewTenants)]
        [ProducesResponseType(typeof(TenantModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllTenants([FromQuery] TenantQueryOptionsModel options)
        {
            var userTenantId = this.User.GetTenantId();
            var filters = await options.ToFilters(
                userTenantId,
                this.cachingResolver,
                $"{nameof(Tenant.Details)}.{nameof(Tenant.Details.Name)}");

            var query = new GetTenantsQuery(filters, false, false);
            var tenants = await this.mediator.Send(query);
            var resources = tenants.Select(t => new TenantModel(t));
            return this.Ok(resources);
        }

        /// <summary>
        /// Redirect to custom domain.
        /// </summary>
        /// <returns>returns Ok if there is no matches custom domain
        /// otherwise it will redirect to custom domain.</returns>
        [HttpGet]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [Route("/")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> RedirectToCustomDomainAsync()
        {
            var command = new GetTenantByCustomDomainQuery(this.contextAccessor.GetDomain());
            var tenant = await this.mediator.Send(command);

            if (tenant != null)
            {
                var redirectUrl = $"{this.contextAccessor.GetBaseUrl()}portal/{tenant.Details.Alias}";
                return this.RedirectPermanent(redirectUrl);
            }

            return this.Ok();
        }

        /// <summary>
        /// Gets a tenant record with the given ID or alias.
        /// </summary>
        /// <param name="tenant">The Id or Alias of the tenant.</param>
        /// <returns>A tenant.</returns>
        [HttpGet]
        [Route("{tenant}")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [ProducesResponseType(typeof(TenantModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTenant(string tenant)
        {
            var tenantModel = await this.GetContextTenantOrThrow(tenant, "access tenants from a different tenancy");
            return this.Ok(new TenantModel(tenantModel));
        }

        /// <summary>
        /// Create a new tenant.
        /// </summary>
        /// <param name="model">View model with tenant details.</param>
        /// <returns>The newly created tenant.</returns>
        [HttpPost]
        [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 5)]
        [ValidateModel]
        [MustHavePermission(Permission.ManageTenants)]
        [ProducesResponseType(typeof(TenantModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateTenant([FromBody] TenantModel model)
        {
            model.Name = model.Name.NormalizeWhitespace();
            var tenantId = await this.mediator.Send(new CreateTenantCommand(
                model.Name,
                model.Alias,
                model.CustomDomain));
            return this.Ok(await this.GetTenantResult(tenantId));
        }

        /// <summary>
        /// Update an existing tenant.
        /// </summary>
        /// <param name="tenant">The Id or Alias of the tenant to update.</param>
        /// <param name="model">New tenant details.</param>
        /// <returns>The updated tenant.</returns>
        [HttpPut]
        [Route("{tenant}")]
        [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 5)]
        [ValidateModel]
        [MustHavePermission(Permission.ManageTenants)]
        [ProducesResponseType(typeof(TenantModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateTenant(string tenant, [FromBody] TenantModel model)
        {
            var tenantModel = await this.GetContextTenantOrThrow(tenant, "update tenants from a different tenancy");
            await this.mediator.Send(new UpdateTenantCommand(
                tenantModel.Id,
                model.Name.NormalizeWhitespace(),
                model.Alias,
                model.CustomDomain));

            var domainProperties = model.Properties.ToDomainAdditionalProperties();
            await this.additionalPropertyValueService.UpsertSetAdditionalPropertyValuesForNonAggregateEntity(
                tenantModel.Id,
                tenantModel.Id,
                domainProperties);
            return this.Ok(await this.GetTenantResult(tenantModel.Id));
        }

        /// <summary>
        /// Disables a tenant.
        /// </summary>
        /// <param name="tenant">The tenant ID or alias.</param>
        /// <returns>The updated tenant.</returns>
        [HttpPatch("{tenant}/disable")]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 15)]
        [MustHavePermission(Permission.ManageTenants)]
        [ProducesResponseType(typeof(PortalModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> DisableTenant(string tenant)
        {
            var tenantModel = await this.GetContextTenantOrThrow(tenant, "disable tenants from a different tenancy");
            await this.mediator.Send(new DisableTenantCommand(tenantModel.Id));
            return this.Ok(await this.GetTenantResult(tenantModel.Id));
        }

        /// <summary>
        /// Enable a tenant.
        /// </summary>
        /// <param name="tenant">The tenant ID or alias.</param>
        /// <returns>The updated tenant.</returns>
        [HttpPatch("{tenant}/enable")]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 15)]
        [MustHavePermission(Permission.ManageTenants)]
        [ProducesResponseType(typeof(PortalModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> EnableTenant(string tenant)
        {
            var tenantModel = await this.GetContextTenantOrThrow(tenant, "enable tenants from a different tenancy");
            await this.mediator.Send(new EnableTenantCommand(tenantModel.Id));
            return this.Ok(await this.GetTenantResult(tenantModel.Id));
        }

        /// <summary>
        /// Deletes a tenant.
        /// </summary>
        /// <param name="tenant">The tenand ID or alias.</param>
        /// <returns>A 200 OK status code.</returns>
        [HttpDelete]
        [Route("{tenant}")]
        [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 5)]
        [MustHavePermission(Permission.ManageTenants)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteTenant(string tenant)
        {
            var tenantModel = await this.GetContextTenantOrThrow(tenant, "delete tenants from a different tenancy");
            await this.mediator.Send(new DeleteTenantCommand(tenantModel.Id));
            return this.Ok();
        }

        /// <summary>
        /// Gets a tenant record with the given ID.
        /// </summary>
        /// <param name="tenant">The Id or Alias of the tenant.</param>
        /// <returns>A tenant.</returns>
        [HttpGet]
        [Route("{tenant}/session-settings")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [ProducesResponseType(typeof(TenantSessionSettingUpdateModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTenantSessionSettings(string tenant)
        {
            var tenantModel = await this.GetContextTenantOrThrow(tenant, "view session expiry settings from a different tenancy");
            return this.Ok(new TenantSessionSettingUpdateModel(tenantModel));
        }

        /// <summary>
        /// Update an existing tenant.
        /// </summary>
        /// <param name="tenant">The Id or Alias of the tenant to update.</param>
        /// <param name="model">New tenant details.</param>
        /// <returns>The updated tenant.</returns>
        [HttpPut]
        [Route("{tenant}/session-settings")]
        [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 5)]
        [ValidateModel]
        [MustHavePermission(Permission.ManageTenants)]
        [ProducesResponseType(typeof(TenantSessionSettingUpdateModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateTenantSessionSettings(string tenant, [FromBody] TenantSessionSettingUpdateModel model)
        {
            var tenantModel = await this.GetContextTenantOrThrow(tenant, "update session expiry settings from a different tenancy");
            var command = new UpdateSessionSettingsCommand(
                tenantModel.Id,
                model.SessionExpiryMode.ToEnumOrThrow<SessionExpiryMode>(),
                model.IdleTimeoutPeriodType,
                model.FixLengthTimeoutInPeriodType,
                model.IdleTimeout,
                model.FixLengthTimeout);
            var tenantResult = await this.mediator.Send(command);
            return this.Ok(new TenantSessionSettingUpdateModel(tenantResult));
        }

        /// <summary>
        /// Gets a tenant record with the given ID.
        /// </summary>
        /// <param name="tenant">The ID or Alias of the tenant.</param>
        /// <returns>A tenant.</returns>
        [HttpGet]
        [Route("{tenant}/password-expiry-settings")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [ProducesResponseType(typeof(TenantPasswordExpirySettingModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTenantPasswordExpirySettings(string tenant)
        {
            var tenantModel = await this.GetContextTenantOrThrow(tenant, "view password expiry settings from a different tenancy");
            return this.Ok(new TenantPasswordExpirySettingModel(tenantModel));
        }

        /// <summary>
        /// Update an existing tenant for password expiry settings.
        /// </summary>
        /// <param name="tenant">The ID or Alias of the tenant to update.</param>
        /// <param name="model">New tenant details.</param>
        /// <returns>The updated tenant.</returns>
        [HttpPut]
        [Route("{tenant}/password-expiry-settings")]
        [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 5)]
        [ValidateModel]
        [MustHavePermission(Permission.ManageTenants)]
        [ProducesResponseType(typeof(TenantPasswordExpirySettingModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateTenantPasswordExpirySettings(string tenant, [FromBody] TenantPasswordExpirySettingModel model)
        {
            var tenantModel = await this.GetContextTenantOrThrow(tenant, "update passowrd expiry settings from a different tenancy");
            var passwordExpiry = await this.mediator.Send(new UpdatePasswordExpirySettingsCommand(
                tenantModel.Id,
                model.PasswordExpiryEnabled,
                model.MaxPasswordAgeDays));
            return this.Ok(new TenantPasswordExpirySettingModel(passwordExpiry));
        }

        /// <summary>
        /// Get the tenant of the logged in user.
        /// </summary>
        /// <returns>The tenant of the logged in user.</returns>
        [MustBeLoggedIn]
        [HttpGet]
        [Route("for-logged-in-user")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [ProducesResponseType(typeof(TenantDetails), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTenantForLoggedInUser()
        {
            Guid? userTenantId = this.User.GetTenantId();
            if (userTenantId == null || userTenantId == default)
            {
                return Errors.User.SessionNotFound().ToProblemJsonResult();
            }

            var tenant = await this.cachingResolver.GetTenantOrThrow(userTenantId);
            return this.Ok(tenant.Details);
        }

        /// <summary>
        /// Get tenant name.
        /// </summary>
        /// <param name="tenant">The alias of tenant.</param>
        /// <returns>Name of tenant.</returns>
        [HttpGet]
        [Route("{tenant}/name")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [Microsoft.AspNetCore.Authorization.AllowAnonymous]
        [ProducesResponseType(typeof(TenantNameModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTenantName(string tenant)
        {
            var tenantModel = await this.GetContextTenantOrThrow(tenant, "get tenant details from a different tenancy");
            return this.Ok(new TenantNameModel(tenantModel.Id, tenantModel.Details.Name));
        }

        /// <summary>
        /// Get tenant id.
        /// </summary>
        /// <param name="tenant">The alias of tenant.</param>
        /// <returns>Id of tenant.</returns>
        [HttpGet]
        [Route("{tenant}/id")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [Microsoft.AspNetCore.Authorization.AllowAnonymous]
        [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTenantIdByAlias(string tenant)
        {
            var tenantModel = await this.GetContextTenantOrThrow(tenant, "get tenant details from a different tenancy");
            return this.Ok(tenantModel.Id);
        }

        /// <summary>
        /// Get tenant alias by tenantId.
        /// </summary>
        /// <param name="tenant">The Id or Alias of tenant.</param>
        /// <returns>Alias of tenant.</returns>
        [HttpGet]
        [Route("{tenant}/alias")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [Microsoft.AspNetCore.Authorization.AllowAnonymous]
        [ProducesResponseType(typeof(TenantAliasModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTenantAlias(string tenant)
        {
            var tenantModel = await this.GetContextTenantOrThrow(tenant, "get tenant details from a different tenancy");
            return this.Ok(new TenantAliasModel(tenantModel.Id, tenantModel.Details.Alias));
        }

        private async Task<TenantModel> GetTenantResult(Guid tenantId)
        {
            var additionalPropertyValues
                = await this.additionalPropertyValueService.GetAdditionalPropertyValuesByEntityTypeAndEntityId(
                tenantId,
                AdditionalPropertyEntityType.Tenant,
                tenantId);
            var updatedTenant = await this.mediator.Send(new GetTenantByIdQuery(tenantId));
            return new TenantModel(updatedTenant, additionalPropertyValues);
        }
    }
}
