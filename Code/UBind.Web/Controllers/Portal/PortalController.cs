// <copyright file="PortalController.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using UBind.Application;
    using UBind.Application.Authorisation;
    using UBind.Application.Commands.Organisation;
    using UBind.Application.Commands.Portal;
    using UBind.Application.ExtensionMethods;
    using UBind.Application.Queries.Portal;
    using UBind.Domain;
    using UBind.Domain.Enums;
    using UBind.Domain.Extensions;
    using UBind.Domain.Filters;
    using UBind.Domain.Helpers;
    using UBind.Domain.Models;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Permissions;
    using UBind.Domain.ReadModel.Portal;
    using UBind.Domain.Services.AdditionalPropertyValue;
    using UBind.Domain.ValueTypes;
    using UBind.Web.Extensions;
    using UBind.Web.Filters;
    using UBind.Web.Mappers;
    using UBind.Web.ResourceModels;
    using UBind.Web.ResourceModels.Organisation;
    using UBind.Web.ResourceModels.Portal;

    /// <summary>
    /// Provides an API controller that respond to HTTP requests to serve portal-related module.
    /// </summary>
    [Produces("application/json")]
    [Route("api/v1/portal")]
    public class PortalController : PortalBaseController
    {
        private readonly ICachingResolver cachingResolver;
        private readonly IPortalSettingsService portalService;
        private readonly IAdditionalPropertyValueService additionalPropertyValueService;
        private readonly IFeatureSettingService settingService;
        private readonly IAuthorisationService authorisationService;
        private readonly ICqrsMediator mediator;
        private readonly PortalSignInMethodMapper portalSignInMethodMapper = new PortalSignInMethodMapper();

        /// <summary>
        /// Initializes a new instance of the <see cref="PortalController"/> class.
        /// </summary>
        /// <param name="portalService">The portal service.</param>
        /// <param name="authorisationService">The authorisation service.</param>
        /// <param name="mediator">The mediator.</param>
        /// <param name="settingService">The setting service.</param>
        /// <param name="cachingResolver">The tenant, product and portal resolver.</param>
        /// <param name="additionalPropertyValueService">Additional property value service.</param>
        public PortalController(
            IPortalSettingsService portalService,
            IFeatureSettingService settingService,
            ICachingResolver cachingResolver,
            IAuthorisationService authorisationService,
            ICqrsMediator mediator,
            IAdditionalPropertyValueService additionalPropertyValueService)
            : base(cachingResolver)
        {
            this.cachingResolver = cachingResolver;
            this.portalService = portalService;
            this.settingService = settingService;
            this.additionalPropertyValueService = additionalPropertyValueService;
            this.authorisationService = authorisationService;
            this.mediator = mediator;
        }

        /// <summary>
        /// Returns a list of PortalDto objects that represent the portal based from the provided query options.
        /// </summary>
        /// <param name="options">The filter options.</param>
        /// <returns>A list of PortalDto objects that represent the portal. Returns an empty list if there is no record.</returns>
        [HttpGet]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [MustHaveOneOfPermissions(
            Permission.EditMyAccount,
            Permission.ViewPortals,
            Permission.ManageCustomers,
            Permission.ManageAllCustomers,
            Permission.ManageAllCustomersForAllOrganisations,
            Permission.ManageUsers,
            Permission.ManageUsersForOtherOrganisations)]
        [RequiresFeature(Feature.PortalManagement)]
        [ProducesResponseType(typeof(IEnumerable<PortalModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPortals([FromQuery] PortalQueryOptionsModel options)
        {
            var userTenantId = this.User.GetTenantId();
            var tenantId = await this.GetContextTenantIdOrThrow(options.Tenant, "list portals from a different tenancy");

            // TODO: revisit restrictions - needs permission "View Portals In All Organisations"
            if (userTenantId != Tenant.MasterTenantId)
            {
                var organisation = await this.cachingResolver.GetOrganisationOrNull(tenantId, new GuidOrAlias(options.Organisation));
                await this.authorisationService.ThrowIfUserNotInOrganisationOrDefaultOrganisation(
                    this.User, organisation?.Id, tenantId);
            }

            PortalListFilters filters = (PortalListFilters)await options.ToFilters(
                tenantId,
                this.cachingResolver,
                $"{nameof(PortalReadModel.Name)}");
            var resources = (await this.mediator.Send(new GetPortalSummariesQuery(tenantId, filters)))
                .AsQueryable()
                .Paginate(filters)
                .Select(t => new PortalSummaryModel(t));
            return this.Ok(resources.ToList());
        }

        /// <summary>
        /// Gets the portal details from the provided portal Id.
        /// </summary>
        /// <param name="portal">The Id or Alias of the portal.</param>
        /// <param name="tenant">The Id or Alias of the tenant.</param>
        /// <param name="useCache">Indicates whether a cached copy of the portal should be returned if it exists.
        /// Defaults to true.</param>
        /// <returns>A portal record.</returns>
        [HttpGet("{portal}")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [MustHavePermission(Permission.ViewPortals)]
        [RequiresFeature(Feature.PortalManagement)]
        [ProducesResponseType(typeof(PortalDetailModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPortalDetails(
            string portal,
            [FromQuery] string tenant = null,
            [FromQuery] bool useCache = true)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(tenant, "get a portal from a different tenancy");
            var tenantModel = await this.cachingResolver.GetTenantOrThrow(tenantId);
            PortalReadModel portalModel = null;
            if (useCache)
            {
                portalModel = await this.cachingResolver.GetPortalOrThrow(tenantId, new GuidOrAlias(portal));
            }
            else
            {
                var guidOrAlias = new GuidOrAlias(portal);
                if (guidOrAlias.Guid.HasValue)
                {
                    portalModel = await this.mediator.Send(new GetPortalByIdQuery(tenantId, guidOrAlias.Guid.Value));
                    EntityHelper.ThrowIfNotFound(portalModel, guidOrAlias.Guid.Value);
                }
                else
                {
                    portalModel = await this.mediator.Send(new GetPortalByAliasQuery(tenantId, guidOrAlias.Alias));
                    EntityHelper.ThrowIfNotFound(portalModel, guidOrAlias.Alias);
                }
            }

            var additionalPropertyDtos = await this.additionalPropertyValueService.GetAdditionalPropertyValuesByEntityTypeAndEntityId(
                tenantId,
                AdditionalPropertyEntityType.Portal,
                portalModel.Id);
            var organisationModel = await this.cachingResolver.GetOrganisationOrThrow(tenantId, portalModel.OrganisationId);
            var defaultUrl = await this.mediator.Send(new GetDefaultPortalUrlQuery(portalModel));
            return this.Ok(new PortalDetailModel(portalModel, organisationModel.Name, tenantModel.Details.Name, defaultUrl, additionalPropertyDtos));
        }

        /// <summary>
        /// Gets the portal entity settings.
        /// </summary>
        /// <param name="tenant">The tenant.</param>
        /// <param name="portal">The portal.</param>
        /// <returns>The summary of the portal.</returns>
        [HttpGet]
        [Route("{portal}/entity-settings")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 80)]
        [MustHavePermission(Permission.ViewPortals)]
        [ProducesResponseType(typeof(PortalEntitySettings), StatusCodes.Status200OK)]
        public async Task<IActionResult> EntitySettings([FromQuery] string tenant, string portal)
        {
            var tenantDetail = await this.cachingResolver.GetTenantOrThrow(new GuidOrAlias(tenant));
            var portalDetail = await this.cachingResolver.GetPortalOrThrow(tenantDetail.Id, new GuidOrAlias(portal));
            var query = new GetPortalEntitySettingsQuery(tenantDetail.Id, portalDetail.Id);
            var entitySettings = await this.mediator.Send(query);
            return this.Ok(entitySettings);
        }

        /// <summary>
        /// Identify whether the portal allow self account creation.
        /// </summary>
        /// <param name="tenant">The tenant.</param>
        /// <param name="portal">The portal.</param>
        /// <returns>The summary of the organisation.</returns>
        [HttpGet]
        [Route("{portal}/entity-settings/customer-self-account-creation")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 80)]
        [AllowAnonymous]
        [ProducesResponseType(typeof(PortalEntitySettings), StatusCodes.Status200OK)]
        public async Task<IActionResult> DoesAllowCustomerSelfAccountCreation(string tenant, string portal)
        {
            var tenantDetail = await this.cachingResolver.GetTenantOrThrow(new GuidOrAlias(tenant));
            var portalDetail = await this.cachingResolver.GetPortalOrThrow(tenantDetail.Id, new GuidOrAlias(portal));
            var query = new GetPortalEntitySettingsQuery(tenantDetail.Id, portalDetail.Id);
            var entitySettings = await this.mediator.Send(query);
            return this.Ok(entitySettings.AllowCustomerSelfAccountCreation);
        }

        /// <summary>
        /// Creates a new portal record in the system.
        /// </summary>
        /// <param name="model">The portal details view model.</param>
        /// <returns>The newly created portal.</returns>
        [HttpPost]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 15)]
        [ValidateModel]
        [MustHavePermission(Permission.ManagePortals)]
        [RequiresFeature(Feature.PortalManagement)]
        [ProducesResponseType(typeof(PortalDetailModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreatePortal([FromBody] PortalRequestModel model)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(model.Tenant, "create a portal in a different tenancy");
            var organisation = await this.cachingResolver.GetOrganisationOrThrow(tenantId, new GuidOrAlias(model.Organisation));
            var portal = await this.mediator.Send(new CreatePortalCommand(
                tenantId,
                model.Name,
                model.Alias,
                model.Title,
                model.UserType,
                organisation.Id));
            var domainProperties = model.Properties.ToDomainAdditionalProperties();
            await this.additionalPropertyValueService.UpsertSetAdditionalPropertyValuesForNonAggregateEntity(
                portal.TenantId,
                portal.Id,
                domainProperties);
            return this.Ok(await this.GetPortalResult(portal));
        }

        /// <summary>
        /// Updates an existing portal from the provided portal Id.
        /// </summary>
        /// <param name="portal">The Id or Alias of the portal.</param>
        /// <param name="model">The new portal details view model.</param>
        /// <returns>The updated portal.</returns>
        [HttpPut("{portal}")]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 15)]
        [ValidateModel]
        [MustHavePermission(Permission.ManagePortals)]
        [RequiresFeature(Feature.PortalManagement)]
        [ProducesResponseType(typeof(PortalDetailModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdatePortal(string portal, [FromBody] PortalRequestModel model)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(model.Tenant, "update a portal in a different tenancy");
            var portalModel = await this.cachingResolver.GetPortalOrThrow(tenantId, new GuidOrAlias(portal));
            var organisationModel = await this.cachingResolver.GetOrganisationOrNull(tenantId, new GuidOrAlias(model.Organisation));
            portalModel = await this.mediator.Send(new UpdatePortalCommand(
                tenantId,
                portalModel.Id,
                model.Name,
                model.Alias,
                model.Title,
                organisationModel?.Id,
                model.UserType));
            var domainProperties = model.Properties.ToDomainAdditionalProperties();
            await this.additionalPropertyValueService.UpsertSetAdditionalPropertyValuesForNonAggregateEntity(
                portalModel.TenantId,
                portalModel.Id,
                domainProperties);
            return this.Ok(await this.GetPortalResult(portalModel));
        }

        /// <summary>
        /// Disables a portal.
        /// </summary>
        /// <param name="portal">The id or alias of the portal you wish to disable.</param>
        /// <param name="tenant">The tenant ID or alias (optional).</param>
        /// <returns>The updated portal.</returns>
        [HttpPatch("{portal}/disable")]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 15)]
        [MustHavePermission(Permission.ManagePortals)]
        [RequiresFeature(Feature.PortalManagement)]
        [ProducesResponseType(typeof(PortalDetailModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> DisablePortal(string portal, [FromQuery] string tenant)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(tenant, "update a portal in a different tenancy");
            var portalModel = await this.cachingResolver.GetPortalOrThrow(tenantId, new GuidOrAlias(portal));
            portalModel = await this.mediator.Send(new DisablePortalCommand(tenantId, portalModel.Id));
            return this.Ok(await this.GetPortalResult(portalModel));
        }

        /// <summary>
        /// Enable a portal.
        /// </summary>
        /// <param name="portal">The id or alias of the portal you wish to enable.</param>
        /// <param name="tenant">The tenant ID or alias (optional).</param>
        /// <returns>The updated portal.</returns>
        [HttpPatch("{portal}/enable")]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 15)]
        [MustHavePermission(Permission.ManagePortals)]
        [RequiresFeature(Feature.PortalManagement)]
        [ProducesResponseType(typeof(PortalDetailModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> EnablePortal(string portal, [FromQuery] string tenant)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(tenant, "update a portal in a different tenancy");
            var portalModel = await this.cachingResolver.GetPortalOrThrow(tenantId, new GuidOrAlias(portal));
            portalModel = await this.mediator.Send(new EnablePortalCommand(tenantId, portalModel.Id));
            return this.Ok(await this.GetPortalResult(portalModel));
        }

        /// <summary>
        /// Delete a portal.
        /// </summary>
        /// <param name="portal">The id or alias of the portal you wish to delete.</param>
        /// <param name="tenant">The tenant ID or alias (optional).</param>
        /// <returns>A success or failure response.</returns>
        [HttpDelete("{portal}")]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 15)]
        [MustHavePermission(Permission.ManagePortals)]
        [RequiresFeature(Feature.PortalManagement)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> DeletePortal(string portal, [FromQuery] string tenant)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(tenant, "update a portal in a different tenancy");
            var portalModel = await this.cachingResolver.GetPortalOrThrow(tenantId, new GuidOrAlias(portal));
            await this.mediator.Send(new DeletePortalCommand(tenantId, portalModel.Id));
            return this.Ok();
        }

        /// <summary>
        /// Gets the default portal features that comes from the tenant settings.
        /// </summary>
        /// <param name="tenant">The Id or Alias of the tenant.</param>
        /// <returns>The list of active portal features.</returns>
        [MustBeLoggedIn]
        [HttpGet("default/feature")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [ProducesResponseType(typeof(IEnumerable<PortalFeatureModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDefaultPortalActiveFeatures(
            [FromQuery] string tenant = null)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(tenant, "get default portal features for a different tenancy");
            var resources = this.settingService.GetActiveSettings(tenantId).Select(t => new PortalFeatureModel(t));
            return this.Ok(resources);
        }

        /// <summary>
        /// Gets the list of active portal features.
        /// </summary>
        /// <param name="portal">The ID or Alias of the portal.</param>
        /// <param name="tenant">The ID or Alias of the tenant.</param>
        /// <returns>The list of active portal features.</returns>
        [HttpGet("{portal}/feature")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [RequiresFeature(Feature.PortalManagement)]
        [ProducesResponseType(typeof(IEnumerable<PortalSettings>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetActiveFeatures(string portal, [FromQuery] string tenant = null)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(tenant, "get portal features for a different tenancy");
            var portalModel = await this.cachingResolver.GetPortalOrThrow(tenantId, new GuidOrAlias(portal));

            // TODO: This has not been properly implemented.
            return this.Ok(this.portalService.GetPortalSettings(tenantId, portalModel.Id));
        }

        /// <summary>
        /// Updates an existing portal from the provided new feature setting.
        /// </summary>
        /// <param name="portal">The Id or Alias of the portal the setting is for.</param>
        /// <param name="model">The new portal setting detail.</param>
        /// <returns>The updated portal.</returns>
        [HttpPut("{portal}/feature")]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 30)]
        [ValidateModel]
        [MustHavePermission(Permission.ManagePortals)]
        [RequiresFeature(Feature.PortalManagement)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdatePortalFeatures(string portal, [FromBody] FeatureSettingsUpdateResourceModel model)
        {
            var userTenantId = this.User.GetTenantId();
            var tenantId = model.TenantId ?? userTenantId.ToString();
            await this.GetContextTenantIdOrThrow(
                 tenantId,
                 "update the list of domains which a portal can be displayed on",
                 "your user account does not have access to the master tenancy");

            var portalModel = await this.cachingResolver.GetPortalOrThrow(userTenantId, new GuidOrAlias(portal));

            foreach (var item in model.Features)
            {
                this.portalService.UpdatePortalSettings(userTenantId, portalModel.Id, item.PortalSettingId, item.Active);
            }

            return this.Ok();
        }

        /// <summary>
        /// Sets or unsets a portal as the default for it's organisation.
        /// </summary>
        /// <param name="portal">The portal ID or alias.</param>
        /// <param name="isDefault">"true" or "false" to indicate whether it is to be set or unset as the default.</param>
        /// <param name="tenant">The tenant ID or alias.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [HttpPatch]
        [Route("{portal}/default")]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 30)]
        [MustHavePermission(Permission.ManagePortals)]
        [RequiresFeature(Feature.PortalManagement)]
        [ProducesResponseType(typeof(PortalDetailModel), StatusCodes.Status200OK)]
        [Consumes("text/plain", "application/json")]
        public async Task<IActionResult> SetOrUnsetAsDefault(
            string portal,
            [FromBody] string isDefault,
            [FromQuery] string tenant)
        {
            bool setToDefault = bool.Parse(isDefault);
            var tenantId = await this.GetContextTenantIdOrThrow(
                 tenant,
                 "change whether a portal is the default for an organisation",
                 "your user account does not have access to the master tenancy");

            var portalModel = await this.cachingResolver.GetPortalOrThrow(tenantId, new GuidOrAlias(portal));
            if (setToDefault)
            {
                portalModel = await this.mediator.Send(new SetPortalAsDefaultCommand(
                    tenantId, portalModel.OrganisationId, portalModel.Id));
            }
            else
            {
                portalModel = await this.mediator.Send(new UnsetPortalAsDefaultCommand(
                    tenantId, portalModel.OrganisationId, portalModel.Id));
            }

            return this.Ok(await this.GetPortalResult(portalModel));
        }

        /// <summary>
        /// Updates the portal's location (website address).
        /// </summary>
        /// <param name="portal">The ID or Alias of the portal.</param>
        /// <param name="environment">The environment's URL to be updated.</param>
        /// <param name="url">The new url that the portal will be embedded at.</param>
        /// <param name="tenant">The tenant ID or alias.</param>
        /// <returns>Returns an OK status code with PortalDto attached entity.</returns>
        [HttpPatch]
        [Route("{portal}/url/{environment}")]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 30)]
        [MustHavePermission(Permission.ManagePortals)]
        [RequiresFeature(Feature.PortalManagement)]
        [ProducesResponseType(typeof(PortalDetailModel), StatusCodes.Status200OK)]
        [Consumes("text/plain")]
        public async Task<IActionResult> UpdatePortalLocation(
            string portal,
            DeploymentEnvironment environment,
            [FromBody] string url,
            [FromQuery] string tenant)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(
                 tenant,
                 "update the location or website a portal can be embedded on",
                 "your user account does not have access to the master tenancy");

            var portalModel = await this.cachingResolver.GetPortalOrThrow(tenantId, new GuidOrAlias(portal));
            Url? urlValue = url.IsNullOrEmpty() ? null : new Url(url);
            portalModel = await this.mediator.Send(new SetPortalLocationCommand(tenantId, portalModel.Id, environment, urlValue));
            return this.Ok(await this.GetPortalResult(portalModel));
        }

        /// <summary>
        /// Updates a portal's styles.
        /// </summary>
        /// <param name="portal">The ID or Alias of the portal.</param>
        /// <param name="styleSettingsModel">The style settings.</param>
        /// <returns>Returns an OK status code.</returns>
        [HttpPatch]
        [Route("{portal}/styles")]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 30)]
        [ValidateModel]
        [MustHavePermission(Permission.ManagePortals)]
        [RequiresFeature(Feature.PortalManagement)]
        [ProducesResponseType(typeof(PortalDetailModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdatePortalStyles(
            string portal,
            [FromBody] PortalStyleSettingsModel styleSettingsModel)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(
                 styleSettingsModel.Tenant,
                 "update the location or website a portal can be embedded on",
                 "your user account does not have access to the master tenancy");

            var portalModel = await this.cachingResolver.GetPortalOrThrow(tenantId, new GuidOrAlias(portal));
            portalModel = await this.mediator.Send(new UpdatePortalStylesCommand(
                tenantId,
                portalModel.Id,
                styleSettingsModel.StylesheetUrl,
                styleSettingsModel.Styles));
            return this.Ok(await this.GetPortalResult(portalModel));
        }

        /// <summary>
        /// Enable self account creation.
        /// </summary>
        /// <param name="tenant">The tenant.</param>
        /// <param name="portal">The portal.</param>
        /// <returns>The summary of the porta.</returns>
        [HttpPatch]
        [Route("{portal}/entity-settings/customer-self-account-creation/enable")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 80)]
        [MustHavePermission(Permission.ManagePortals)]
        [RequiresFeature(Feature.PortalManagement)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> EnableCustomerSelfAccountCreation(string tenant, string portal)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(tenant, "update a portal from a different tenancy");
            var portalDetail = await this.cachingResolver.GetPortalOrThrow(tenantId, new GuidOrAlias(portal));
            var command = new UpdatePortalCustomerSelfAccountCreationSettingCommand(tenantId, portalDetail.Id, true);
            await this.mediator.Send(command);
            return this.Ok();
        }

        /// <summary>
        /// Disable self account creation.
        /// </summary>
        /// <param name="tenant">The tenant.</param>
        /// <param name="portal">The portal.</param>
        /// <returns>The summary of the porta.</returns>
        [HttpPatch]
        [Route("{portal}/entity-settings/customer-self-account-creation/disable")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 80)]
        [MustHavePermission(Permission.ManagePortals)]
        [RequiresFeature(Feature.PortalManagement)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> DisableCustomerSelfAccountCreation(string tenant, string portal)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(tenant, "update a portal from a different tenancy");
            var portalDetail = await this.cachingResolver.GetPortalOrThrow(tenantId, new GuidOrAlias(portal));
            var command = new UpdatePortalCustomerSelfAccountCreationSettingCommand(tenantId, portalDetail.Id, false);
            await this.mediator.Send(command);
            return this.Ok();
        }

        /// <summary>
        /// Gets the sign in methods for a portal.
        /// </summary>
        /// <param name="portal">The portal ID or alias.</param>
        /// <param name="tenant">The tenant ID or alias.</param>
        [HttpGet]
        [Route("{portal}/sign-in-method")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 80)]
        [MustHavePermission(Permission.ViewPortals)]
        [ProducesResponseType(typeof(IEnumerable<PortalSignInMethodModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSignInMethods(string portal, [FromQuery] string? tenant)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(tenant, "get details about a portal from a different tenancy");
            var portalDetail = await this.cachingResolver.GetPortalOrThrow(tenantId, new GuidOrAlias(portal));
            var readModels = await this.mediator.Send(new GetPortalSignInMethodsQuery(tenantId, portalDetail.Id));
            return this.Ok(readModels.Select(r => this.portalSignInMethodMapper.ReadModelToResourceModel(r)));
        }

        /// <summary>
        /// Disables a sign in method for a portal.
        /// </summary>
        /// <param name="portal">The id or alias of the portal.</param>
        /// <param name="tenant">The tenant ID or alias (optional).</param>
        /// <returns>The updated portal.</returns>
        [HttpPatch("{portal}/sign-in-method/{authenticationMethodId}/disable")]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 15)]
        [MustHavePermission(Permission.ManagePortals)]
        [RequiresFeature(Feature.PortalManagement)]
        [ProducesResponseType(typeof(PortalSignInMethodModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> DisableSignInMethod(string portal, Guid authenticationMethodId, [FromQuery] string tenant)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(tenant, "update a portal in a different tenancy");
            var portalModel = await this.cachingResolver.GetPortalOrThrow(tenantId, new GuidOrAlias(portal));
            await this.mediator.Send(new DisablePortalSignInMethodCommand(tenantId, portalModel.Id, authenticationMethodId));
            var readModel = await this.mediator.Send(new GetPortalSignInMethodQuery(tenantId, portalModel.Id, authenticationMethodId));
            return this.Ok(this.portalSignInMethodMapper.ReadModelToResourceModel(readModel));
        }

        /// <summary>
        /// Enables a sign in method for a portal.
        /// </summary>
        /// <param name="portal">The id or alias of the portal.</param>
        /// <param name="tenant">The tenant ID or alias (optional).</param>
        /// <returns>The updated portal.</returns>
        [HttpPatch("{portal}/sign-in-method/{authenticationMethodId}/enable")]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 15)]
        [MustHavePermission(Permission.ManagePortals)]
        [RequiresFeature(Feature.PortalManagement)]
        [ProducesResponseType(typeof(PortalSignInMethodModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> EnableSignInMethod(string portal, Guid authenticationMethodId, [FromQuery] string tenant)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(tenant, "update a portal in a different tenancy");
            var portalModel = await this.cachingResolver.GetPortalOrThrow(tenantId, new GuidOrAlias(portal));
            await this.mediator.Send(new EnablePortalSignInMethodCommand(tenantId, portalModel.Id, authenticationMethodId));
            var readModel = await this.mediator.Send(new GetPortalSignInMethodQuery(tenantId, portalModel.Id, authenticationMethodId));
            return this.Ok(this.portalSignInMethodMapper.ReadModelToResourceModel(readModel));
        }

        /// <summary>
        /// Updates the sort order for a sign-in method.
        /// </summary>
        /// <param name="portal">The id or alias of the portal.</param>
        /// <param name="tenant">The tenant ID or alias (optional).</param>
        /// <returns>The updated portal.</returns>
        [HttpPatch("{portal}/sign-in-method/{authenticationMethodId}/sort-order")]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 15)]
        [MustHavePermission(Permission.ManagePortals)]
        [RequiresFeature(Feature.PortalManagement)]
        [ProducesResponseType(typeof(PortalSignInMethodModel), StatusCodes.Status200OK)]
        [Consumes("text/plain")]
        public async Task<IActionResult> ReOrderSignInMethod(string portal, Guid authenticationMethodId, [FromBody] string sortOrderString, [FromQuery] string tenant)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(tenant, "update a portal in a different tenancy");
            var portalModel = await this.cachingResolver.GetPortalOrThrow(tenantId, new GuidOrAlias(portal));
            var sortOrder = int.Parse(sortOrderString);
            await this.mediator.Send(new ReOrderPortalSignInMethodCommand(tenantId, portalModel.Id, authenticationMethodId, sortOrder));
            var readModel = await this.mediator.Send(new GetPortalSignInMethodQuery(tenantId, portalModel.Id, authenticationMethodId));
            return this.Ok(this.portalSignInMethodMapper.ReadModelToResourceModel(readModel));
        }

        /// <summary>
        /// Gets the enabled login methods for a portal.
        /// This is used on the login page to determine which login methods to display.
        /// </summary>
        /// <returns>A list of the login methods.</returns>
        [HttpGet]
        [Route("{portal}/login-method")]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 15)]
        [ProducesResponseType(typeof(IEnumerable<AuthenticationMethodSummaryModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetList(string portal, [FromQuery] string tenant)
        {
            var tenantModel = await this.cachingResolver.GetTenantOrThrow(new GuidOrAlias(tenant));
            var portalModel = await this.cachingResolver.GetPortalOrThrow(tenantModel.Id, new GuidOrAlias(portal));
            var loginMethods = await this.mediator.Send(new GetPortalLoginMethodsQuery(tenantModel.Id, portalModel.Id));
            return this.Ok(loginMethods);
        }

        private async Task<PortalDetailModel> GetPortalResult(PortalReadModel portalModel)
        {
            var tenantModel = await this.cachingResolver.GetTenantOrThrow(portalModel.TenantId);
            var additionalPropertyDtos
                = await this.additionalPropertyValueService.GetAdditionalPropertyValuesByEntityTypeAndEntityId(
                    tenantModel.Id,
                    AdditionalPropertyEntityType.Portal,
                    portalModel.Id);
            var organisationModel
                = await this.cachingResolver.GetOrganisationOrThrow(tenantModel.Id, portalModel.OrganisationId);
            var defaultUrl = await this.mediator.Send(new GetDefaultPortalUrlQuery(portalModel));
            return new PortalDetailModel(
                portalModel, organisationModel.Name, tenantModel.Details.Name, defaultUrl, additionalPropertyDtos);
        }
    }
}
