// <copyright file="OrganisationController.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Controllers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UBind.Application.Authorisation;
using UBind.Application.Commands.Organisation;
using UBind.Application.Queries.Organisation;
using UBind.Application.Queries.Quote;
using UBind.Domain;
using UBind.Domain.Enums;
using UBind.Domain.Exceptions;
using UBind.Domain.Helpers;
using UBind.Domain.Models;
using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.Permissions;
using UBind.Domain.ReadModel;
using UBind.Domain.Services;
using UBind.Domain.Services.AdditionalPropertyValue;
using UBind.Web.Extensions;
using UBind.Web.Filters;
using UBind.Web.ResourceModels.Organisation;

/// <summary>
/// Controller for organisation requests.
/// </summary>
[Produces("application/json")]
[Route("api/v1/organisation")]
public class OrganisationController : PortalBaseController
{
    private readonly IAuthorisationService authorisationService;
    private readonly ICachingResolver cachingResolver;
    private readonly IOrganisationService organisationService;
    private readonly ICqrsMediator mediator;
    private readonly IAdditionalPropertyValueService additionalPropertyValueService;
    private readonly IOrganisationAuthorisationService organisationAuthorisationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="OrganisationController"/> class.
    /// </summary>
    /// <param name="authorisationService">The authorisation service.</param>
    /// <param name="organisationService">The organisation service.</param>
    /// <param name="mediator">The mediator.</param>
    /// <param name="cachingResolver">The tenant, product and portal resolver.</param>
    /// <param name="additionalPropertyValueService">Additional property value service.</param>
    public OrganisationController(
        IAuthorisationService authorisationService,
        IOrganisationService organisationService,
        ICqrsMediator mediator,
        ICachingResolver cachingResolver,
        IAdditionalPropertyValueService additionalPropertyValueService,
        IOrganisationAuthorisationService organisationAuthorisationService)
        : base(cachingResolver)
    {
        this.authorisationService = authorisationService;
        this.cachingResolver = cachingResolver;
        this.organisationService = organisationService;
        this.mediator = mediator;
        this.additionalPropertyValueService = additionalPropertyValueService;
        this.organisationAuthorisationService = organisationAuthorisationService;
    }

    /// <summary>
    /// An API that gets all organisation based on the options.
    /// </summary>
    /// <param name="options">The filter options.</param>
    /// <returns>An enumerable collection of filtered organisations.</returns>
    /// <exception cref="UnauthorizedException">An error occured for unauthorised user access.</exception>
    [HttpGet]
    [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
    [MustHaveOneOfPermissions(Permission.ViewOrganisations, Permission.ViewAllOrganisations)]
    [ProducesResponseType(typeof(IEnumerable<OrganisationModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrganisations([FromQuery] OrganisationQueryOptionsModel options)
    {
        var tenantId = await this.GetContextTenantIdOrThrow(options.Tenant, "list organisations from a different tenancy");
        await this.authorisationService.CheckAndStandardiseOptions(this.User, options, restrictToOwnOrganisation: false);
        var filters = await options.ToFilters(tenantId, this.cachingResolver);

        // If the EligibleToManageOrganisationId is provided in the request, then we need to
        // restrict the list of organisations to only those that can manage the given organisation.
        if (options.EligibleToManageOrganisationId.HasValue)
        {
            // Get the list of organisations that can manage the given organisation.
            var eligibleOrganisations = await this.mediator.Send(
                new GetEligibleOrganisationIdsToManageOrganisationQuery(
                    tenantId,
                    options.EligibleToManageOrganisationId.Value));

            // If there are no organisations that can manage the given organisation, then return an empty list.
            // We shouldn't continue the next step as it will return all organisations instead.
            if (!eligibleOrganisations.Any())
            {
                return this.Ok(Enumerable.Empty<OrganisationModel>());
            }

            // If there are organisations that can manage the given organisation, then let's set them as the
            // OrganisationIds filter so that we only filter from these organisations.
            filters.OrganisationIds = eligibleOrganisations;
        }
        else
        {
            await this.organisationAuthorisationService.ApplyRestrictionsToFilters(this.User, filters);
        }

        var organisations = await this.mediator.Send(new GetOrganisationSummariesQuery(tenantId, filters));
        return this.Ok(organisations.Select(o => new OrganisationModel(o)));
    }

    /// <summary>
    /// An API that gets the organisation record for a given Id.
    /// </summary>
    /// <param name="organisation">The Id or Alias of the organisation.</param>
    /// <param name="tenant">The tenant ID or alias (optional).</param>
    /// <returns>The organisation record that matches the given Id.</returns>
    /// <exception cref="UnauthorizedException">An error occured for unauthorised user access.</exception>
    [HttpGet("{organisation}")]
    [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
    [MustHaveOneOfPermissions(Permission.ViewOrganisations, Permission.ViewAllOrganisations)]
    [ProducesResponseType(typeof(OrganisationModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrganisation(string organisation, [FromQuery] string tenant)
    {
        var tenantId = await this.GetContextTenantIdOrThrow(tenant, "access an organisation from a different tenancy");
        var organisationModel = await this.cachingResolver.GetOrganisationOrThrow(
            tenantId,
            new GuidOrAlias(organisation));
        await this.organisationAuthorisationService.ThrowIfUserCannotView(tenantId, organisationModel.Id, this.User);
        var organisationDetails = await this.mediator.Send(new GetOrganisationByIdQuery(
            tenantId,
            organisationModel.Id));
        var additionalPropertyValueDtos = await this.additionalPropertyValueService.GetAdditionalPropertyValuesByEntityTypeAndEntityId(
            organisationModel.TenantId,
            AdditionalPropertyEntityType.Organisation,
            organisationModel.Id);
        string managingOrgansationName = null;
        if (organisationDetails.ManagingOrganisationId != null)
        {
            var managingOrganisation = await this.cachingResolver.GetOrganisationOrThrow(
                tenantId,
                organisationDetails.ManagingOrganisationId.Value);
            managingOrgansationName = managingOrganisation.Name;
        }

        var model = new OrganisationDetailsModel(organisationDetails, additionalPropertyValueDtos, managingOrgansationName);
        return this.Ok(model);
    }

    /// <summary>
    /// Gets the organisation entity settings.
    /// </summary>
    /// <param name="organisation">The organisation ID or alias.</param>
    /// <param name="tenant">The tenant ID or alias (optional).</param>
    /// <returns>The summary of the organisation.</returns>
    [HttpGet]
    [Route("{organisation}/entity-settings")]
    [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 80)]
    [MustHaveOneOfPermissions(Permission.ViewOrganisations, Permission.ViewAllOrganisations)]
    [ProducesResponseType(typeof(OrganisationEntitySettings), StatusCodes.Status200OK)]
    public async Task<IActionResult> EntitySettings(string organisation, [FromQuery] string tenant)
    {
        var tenantId = await this.GetContextTenantIdOrThrow(tenant, "access an organisation from a different tenancy");
        var organisationModel = await this.cachingResolver.GetOrganisationOrThrow(
            tenantId,
            new GuidOrAlias(organisation));
        await this.organisationAuthorisationService.ThrowIfUserCannotView(tenantId, organisationModel.Id, this.User);
        var query = new GetOrganisationEntitySettingsQuery(tenantId, organisationModel.Id);
        var entitySettings = await this.mediator.Send(query);
        return this.Ok(entitySettings);
    }

    /// <summary>
    /// Identify whether the organisation allow renewal invitation.
    /// </summary>
    /// <param name="tenantId">The tenant id.</param>
    /// <param name="organisationId">The organisation id.</param>
    /// <returns>Returns a boolean indicating whether the organization allows renewal invitations.</returns>
    [HttpGet]
    [Route("{organisationId}/entity-settings/renewal-invitation")]
    [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 80)]
    [AllowAnonymous]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public async Task<IActionResult> OrganisationRenewalInvitationSetting([FromQuery] Guid tenantId, Guid organisationId)
    {
        var query = new GetOrganisationEntitySettingsQuery(tenantId, organisationId);
        var entitySetting = await this.mediator.Send(query);
        return this.Ok(entitySetting.AllowOrganisationRenewalInvitation);
    }

    /// <summary>
    /// An API that creates a new active non-default organisation.
    /// </summary>
    /// <param name="model">The view model with organisation details.</param>
    /// <returns>The newly created organisation.</returns>
    /// <exception cref="UnauthorizedException">An error occured for unauthorised user access.</exception>
    [HttpPost]
    [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 15)]
    [ValidateModel]
    [MustHaveOneOfPermissions(Permission.ManageOrganisations, Permission.ManageAllOrganisations)]
    [ProducesResponseType(typeof(OrganisationModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> Create([FromBody] OrganisationUpsertModel model)
    {
        var tenantId = await this.GetContextTenantIdOrThrow(model.Tenant, "create an organisation in a different tenancy");
        await this.organisationAuthorisationService.ThrowIfUserCannotCreate(tenantId, default, this.User);
        Guid? managingOrganisationId = null;
        OrganisationReadModel managingOrganisation = null;
        if (model.ManagingOrganisation != null)
        {
            managingOrganisation
                = await this.cachingResolver.GetOrganisationOrThrow(tenantId, new GuidOrAlias(model.ManagingOrganisation));
            managingOrganisationId = managingOrganisation.Id;
        }
        var properties = this.ConvertToDomainPropertyModels(model.Properties);
        var organisationDetails = await this.mediator.Send(new CreateOrganisationCommand(
            tenantId, model.Alias, model.Name, managingOrganisationId, properties, model.LinkedIdentities));
        var additionalPropertyValueDtos = await this.additionalPropertyValueService.GetAdditionalPropertyValuesByEntityTypeAndEntityId(
            organisationDetails.TenantId,
            AdditionalPropertyEntityType.Organisation,
            organisationDetails.Id);
        return this.Ok(new OrganisationDetailsModel(organisationDetails, additionalPropertyValueDtos, managingOrganisation?.Name));
    }

    /// <summary>
    /// An API that updates an existing organisation.
    /// </summary>
    /// <param name="organisation">The Id or Alias of the organisation to update.</param>
    /// <param name="model">The new organisation details.</param>
    /// <returns>Status code 200 (Ok) with the updated organisation.</returns>
    [HttpPut]
    [Route("{organisation}")]
    [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 15)]
    [ValidateModel]
    [MustHaveOneOfPermissions(Permission.ManageOrganisations, Permission.ManageAllOrganisations)]
    [ProducesResponseType(typeof(OrganisationModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(string organisation, [FromBody] OrganisationUpsertModel model)
    {
        var tenantId = await this.GetContextTenantIdOrThrow(model.Tenant, "create an organisation in a different tenancy");
        var organisationModel = await this.cachingResolver.GetOrganisationOrThrow(
            tenantId,
            new GuidOrAlias(organisation));
        await this.organisationAuthorisationService.ThrowIfUserCannotModify(tenantId, organisationModel.Id, this.User);
        var properties = this.ConvertToDomainPropertyModels(model.Properties);
        await this.mediator.Send(new UpdateOrganisationCommand(
            tenantId,
            organisationModel.Id,
            model.Alias,
            model.Name,
            properties,
            model.LinkedIdentities));
        var organisationDetails = await this.mediator.Send(new GetOrganisationByIdQuery(tenantId, organisationModel.Id));
        var additionalPropertyValueDtos = await this.additionalPropertyValueService.GetAdditionalPropertyValuesByEntityTypeAndEntityId(
            organisationDetails.TenantId,
            AdditionalPropertyEntityType.Organisation,
            organisationDetails.Id);
        string managingOrgansationName = null;
        if (organisationDetails.ManagingOrganisationId != null)
        {
            var managingOrganisation = await this.cachingResolver.GetOrganisationOrThrow(
                tenantId,
                organisationDetails.ManagingOrganisationId.Value);
            managingOrgansationName = managingOrganisation.Name;
        }

        return this.Ok(new OrganisationDetailsModel(organisationModel, additionalPropertyValueDtos, managingOrgansationName));
    }

    /// <summary>
    /// Updates the managing organisation of an organisation.
    /// </summary>
    /// <param name="organisation">The ID or alias of the organisation being modified.</param>
    /// <param name="managingOrganisation">The ID or alias of the managing organisation.
    /// Leave this empty to unset the managing organisation.</param>
    /// <param name="tenant">The tenant ID or alias that the organisation belongs to.</param>
    /// <returns>Status code 200 (Ok).</returns>
    [HttpPatch]
    [Route("{organisation}/managing-organisation")]
    [MustHaveOneOfPermissions(Permission.ManageOrganisations, Permission.ManageAllOrganisations)]
    [ProducesResponseType(typeof(OrganisationModel), StatusCodes.Status200OK)]
    [Consumes("text/plain")]
    public async Task<IActionResult> UpdateManagingOrganisation(
        string organisation,
        [FromBody] string managingOrganisation,
        [FromQuery] string tenant)
    {
        var tenantId = await this.GetContextTenantIdOrThrow(tenant, "update an organisation in a different tenancy");
        var organisationModel = await this.cachingResolver.GetOrganisationOrThrow(
            tenantId,
            new GuidOrAlias(organisation));
        await this.organisationAuthorisationService.ThrowIfUserCannotModify(tenantId, organisationModel.Id, this.User);
        var managingOrganisationModel = !string.IsNullOrEmpty(managingOrganisation)
            ? await this.cachingResolver.GetOrganisationOrThrow(tenantId, new GuidOrAlias(managingOrganisation))
            : null;
        await this.mediator.Send(new SetManagingOrganisationCommand(
            tenantId,
            organisationModel.Id,
            managingOrganisationModel?.Id));
        organisationModel = await this.mediator.Send(new GetOrganisationByIdQuery(tenantId, organisationModel.Id));
        var additionalPropertyValueDtos = await this.additionalPropertyValueService.GetAdditionalPropertyValuesByEntityTypeAndEntityId(
            organisationModel.TenantId,
            AdditionalPropertyEntityType.Organisation,
            organisationModel.Id);
        return this.Ok(new OrganisationDetailsModel(organisationModel, additionalPropertyValueDtos, managingOrganisationModel?.Name));
    }

    /// <summary>
    /// Gets a list of the potential linked identities for an organisation, shoud it be created by this managing
    /// organisation. This is need so we can show a list of empty linked identity unique identifiers to the user
    /// before they create the organisation, that the user can fill in with the actual unique identifiers from
    /// the identity providers, if they wish to do so. This is only necessary if the organisations are not auto
    /// provisioned. You therefore need to create the organisation manually and enter the unique ID from the
    /// identity provider in advance.
    /// </summary>
    /// <param name="managingOrganisation">The ID or alias of the managing organisation (which is effectively the
    /// organisation against which has the authentication methods are defined.</param>
    /// <param name="tenant">The tenant ID or alias.</param>
    [HttpGet]
    [Route("{managingOrganisation}/potential-linked-identities")]
    [MustHaveOneOfPermissions(Permission.ManageOrganisations, Permission.ManageAllOrganisations)]
    [ProducesResponseType(typeof(IEnumerable<OrganisationLinkedIdentityModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPotentialLinkedIdentities(string managingOrganisation, [FromQuery] string? tenant)
    {
        var tenantId = await this.GetContextTenantIdOrThrow(tenant, "access organisations in a different tenancy");
        var managingOrganisationModel
            = await this.cachingResolver.GetOrganisationOrThrow(tenantId, new GuidOrAlias(managingOrganisation));
        var potentialLinkedIdentities = await this.mediator.Send(
            new GetPotentialOrganisationLinkedIdentitiesQuery(tenantId, null, managingOrganisationModel.Id));
        return this.Ok(potentialLinkedIdentities.Select(li => new OrganisationLinkedIdentityModel(li)));
    }

    /// <summary>
    /// Gets a list of linked identities for an organisation.
    /// This endpoint can also include "potential" linked identities, which are ones which do not exist yet, but
    /// there is an authentication method in place which means if it was used then it could be created.
    /// </summary>
    /// <param name="organisation">The organisation ID or alias.</param>
    /// <param name="tenant">The tenant ID or alias.</param>
    /// <param name="includePotential">Whether to include potential linked identities.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpGet]
    [Route("{organisation}/linked-identities")]
    [MustHaveOneOfPermissions(Permission.ManageOrganisations, Permission.ManageAllOrganisations)]
    [ProducesResponseType(typeof(IEnumerable<OrganisationLinkedIdentityModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> LinkedIdentities(string organisation, [FromQuery] string? tenant, [FromQuery] bool includePotential = false)
    {
        var tenantId = await this.GetContextTenantIdOrThrow(tenant, "access organisations in a different tenancy");
        var organisationModel
            = await this.cachingResolver.GetOrganisationOrThrow(tenantId, new GuidOrAlias(organisation));
        var potentialLinkedIdentities = await this.mediator.Send(
            new GetOrganisationLinkedIdentitiesQuery(tenantId, organisationModel.Id, includePotential));
        return this.Ok(potentialLinkedIdentities.Select(li => new OrganisationLinkedIdentityModel(li)));
    }

    /// <summary>
    /// Enables an organisation.
    /// </summary>
    /// <param name="organisation">The Id or Alias of the organisation.</param>
    /// <param name="tenant">The tenant ID or alias (optional).</param>
    /// <returns>Status code 200 (Ok) with the updated organisation.</returns>
    [HttpPatch("{organisation}/enable")]
    [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 15)]
    [ValidateModel]
    [MustHaveOneOfPermissions(Permission.ManageOrganisations, Permission.ManageAllOrganisations)]
    [ProducesResponseType(typeof(OrganisationModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> Enable(string organisation, [FromQuery] string tenant)
    {
        var tenantId = await this.GetContextTenantIdOrThrow(tenant, "update an organisation in a different tenancy");
        var organisationModel = await this.cachingResolver.GetOrganisationOrThrow(
            tenantId,
            new GuidOrAlias(organisation));
        await this.organisationAuthorisationService.ThrowIfUserCannotModify(tenantId, organisationModel.Id, this.User);
        var organisationSummary = await this.organisationService
            .Activate(tenantId, organisationModel.Id);
        return this.Ok(new OrganisationModel(organisationSummary));
    }

    /// <summary>
    /// An API that disables an organisation.
    /// </summary>
    /// <param name="organisation">The Id or Alias of the organisation.</param>
    /// <param name="tenant">The ID or alias of the tenant (optional).</param>
    /// <returns>Status code 200 (Ok) with the updated organisation.</returns>
    [HttpPatch("{organisation}/disable")]
    [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 15)]
    [ValidateModel]
    [MustHaveOneOfPermissions(Permission.ManageOrganisations, Permission.ManageAllOrganisations)]
    [ProducesResponseType(typeof(OrganisationModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> Disable(string organisation, [FromQuery] string tenant)
    {
        var tenantId = await this.GetContextTenantIdOrThrow(tenant, "update an organisation in a different tenancy");
        var organisationModel = await this.cachingResolver.GetOrganisationOrThrow(
            tenantId,
            new GuidOrAlias(organisation));
        await this.organisationAuthorisationService.ThrowIfUserCannotModify(tenantId, organisationModel.Id, this.User);
        var organisationSummary = await this.organisationService
            .Disable(tenantId, organisationModel.Id);
        return this.Ok(new OrganisationModel(organisationSummary));
    }

    /// <summary>
    /// An API that marks an organisation as deleted.
    /// </summary>
    /// <param name="organisation">The Id or Alias of the organisation.</param>
    /// <param name="tenant">The ID or alias of the tenant (optional).</param>
    /// <returns>Status code 200 (Ok) with the updated organisation.</returns>
    [HttpDelete("{organisation}")]
    [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 15)]
    [ValidateModel]
    [MustHaveOneOfPermissions(Permission.ManageOrganisations, Permission.ManageAllOrganisations)]
    [ProducesResponseType(typeof(OrganisationModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(string organisation, [FromQuery] string tenant)
    {
        var tenantId = await this.GetContextTenantIdOrThrow(tenant, "delete an organisation in a different tenancy");
        var organisationModel = await this.cachingResolver.GetOrganisationOrThrow(
            tenantId,
            new GuidOrAlias(organisation));
        await this.organisationAuthorisationService.ThrowIfUserCannotDelete(tenantId, organisationModel.Id, this.User);
        var organisationSummary = await this.mediator.Send(new DeleteOrganisationAndAssociatedUsersCommand(
            tenantId,
            organisationModel.Id));
        return this.Ok(new OrganisationModel(organisationSummary));
    }

    /// <summary>
    /// An API that sets the organisation as the default from its tenancy.
    /// </summary>
    /// <param name="organisation">The Id or Alias of the organisation.</param>
    /// <param name="tenant">The ID or alias of the tenant (optional).</param>
    /// <returns>Status code 200 (Ok).</returns>
    [HttpPatch("{organisation}/default")]
    [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 5)]
    [ValidateModel]
    [MustHavePermission(Permission.ManageAllOrganisations)]
    [ProducesResponseType(typeof(OrganisationModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> SetAsDefault(string organisation, [FromQuery] string tenant)
    {
        var tenantId = await this.GetContextTenantIdOrThrow(tenant, "update an organisation in a different tenancy");
        var organisationModel = await this.cachingResolver.GetOrganisationOrThrow(
            tenantId,
            new GuidOrAlias(organisation));
        await this.organisationAuthorisationService.ThrowIfUserCannotModify(tenantId, organisationModel.Id, this.User);
        await this.mediator.Send(new SetDefaultOrganisationCommand(
            tenantId,
            organisationModel.Id));
        var organisationSummary = await this.mediator.Send(new GetOrganisationSummaryByIdQuery(
            tenantId,
            organisationModel.Id));
        return this.Ok(new OrganisationModel(organisationSummary));
    }

    /// <summary>
    /// Gets the organisation summary.
    /// </summary>
    /// <param name="tenant">The alias or ID of the tenant.</param>
    /// <param name="organisation">The alias of the organisation.</param>
    /// <param name="portal">The alias of the portal.</param>
    /// <param name="quoteId">The ID of the quote.</param>
    /// <returns>The summary of the organisation.</returns>
    [HttpGet]
    [Route("organisation-summary")]
    [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 80)]
    [AllowAnonymous]
    [Obsolete("This has been replacted by AppContextController. To be removed in UB-9510")]
    public async Task<IActionResult> GetOrganisationSummary(
        string tenant, string? organisation = null, string? portal = null, Guid? quoteId = null)
    {
        var tenantModel = await this.cachingResolver.GetTenantOrThrow(new GuidOrAlias(tenant));
        var portalName = string.Empty;
        var portalStylesheetUrl = string.Empty;
        Guid? portalId = null;
        Guid organisationId = default;

        if (!string.IsNullOrEmpty(portal))
        {
            var portalModel = await this.cachingResolver.GetPortalOrThrow(tenantModel.Id, new GuidOrAlias(portal));
            if (portalModel == null)
            {
                return Errors.Portal.NotFound(tenant, portal).ToProblemJsonResult();
            }

            portalId = portalModel.Id;
            portalName = portalModel.Title;
            portalStylesheetUrl = portalModel.StyleSheetUrl;
            organisationId = string.IsNullOrEmpty(organisation) ? portalModel.OrganisationId : organisationId;
        }

        var organisationModel = await this.GetOrganisation(tenantModel, quoteId, organisation, organisationId);

        if (string.IsNullOrEmpty(portalStylesheetUrl)
             && !string.IsNullOrEmpty(tenantModel.Details.DefaultPortalStylesheetUrl))
        {
            portalStylesheetUrl = tenantModel.Details.DefaultPortalStylesheetUrl;
        }

        if (string.IsNullOrEmpty(portalName)
            && !string.IsNullOrEmpty(tenantModel.Details.DefaultPortalTitle))
        {
            portalName = tenantModel.Details.DefaultPortalTitle;
        }

        bool isDefaultOrganisation = organisationModel?.Id == tenantModel.Details.DefaultOrganisationId;
        var organisationSummary = new OrganisationSummaryModel(
            organisationModel.TenantId,
            tenantModel.Details.Name,
            tenantModel.Details.Alias,
            organisationModel.Id,
            organisationModel.Name,
            organisationModel.Alias,
            portalId,
            portal,
            portalName,
            isDefaultOrganisation,
            portalStylesheetUrl,
            tenantModel.Details.CustomDomain);
        return this.Ok(organisationSummary);
    }

    /// <summary>
    /// Enable renewal invitation.
    /// </summary>
    /// <param name="organisationId">The organisation id.</param>
    /// <returns>The summary of the organisation.</returns>
    [HttpPatch]
    [Route("{organisationId}/entity-settings/renewal-invitation/allow")]
    [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 80)]
    [MustHavePermission(Permission.ManageOrganisations)]
    [RequiresFeature(Feature.OrganisationManagement)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> AllowSendingRenewalInvitations([FromQuery] Guid tenantId, Guid organisationId)
    {
        var query = new UpdateOrganisationRenewalInvitationEmailsSettingCommand(tenantId, organisationId, true);
        await this.mediator.Send(query);
        return this.Ok();
    }

    /// <summary>
    /// Disable renewal invitation.
    /// </summary>
    /// <param name="organisationId">The organisation id.</param>
    /// <returns>The summary of the organisation.</returns>
    [HttpPatch]
    [Route("{organisationId}/entity-settings/renewal-invitation/disallow")]
    [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 80)]
    [MustHavePermission(Permission.ManageOrganisations)]
    [RequiresFeature(Feature.OrganisationManagement)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> DisallowSendingRenewalInvitations([FromQuery] Guid tenantId, Guid organisationId)
    {
        var query = new UpdateOrganisationRenewalInvitationEmailsSettingCommand(tenantId, organisationId, false);
        await this.mediator.Send(query);
        return this.Ok();
    }

    [Obsolete("This has been replacted by AppContextController. To be removed in UB-9510")]
    private async Task<OrganisationReadModel> GetOrganisation(Tenant tenant, Guid? quoteId, string organisationAlias, Guid? organisationId)
    {
        OrganisationReadModel organisation;
        if (quoteId != null && quoteId != Guid.Empty)
        {
            organisation = await this.GetOrganisationForQuote(tenant.Id, quoteId);
        }
        else if (organisationId != null && organisationId != default)
        {
            organisation = await this.cachingResolver.GetOrganisationOrThrow(tenant.Id, organisationId.Value);
        }
        else if (!string.IsNullOrEmpty(organisationAlias))
        {
            organisation = await this.cachingResolver.GetOrganisationOrThrow(tenant.Id, new GuidOrAlias(organisationAlias));
        }
        else
        {
            organisation = await this.cachingResolver.GetOrganisationOrThrow(tenant.Id, tenant.Details.DefaultOrganisationId);
        }

        return organisation;
    }

    private async Task<OrganisationReadModel> GetOrganisationForQuote(Guid tenantId, Guid? quoteId)
    {
        var quote = await this.mediator.Send(new GetQuoteByIdQuery(tenantId, quoteId));
        if (quote == null)
        {
            throw new ErrorException(Errors.Quote.NotFound(quoteId));
        }

        return await this.cachingResolver.GetOrganisationOrThrow(tenantId, quote.OrganisationId);
    }

    private List<Domain.Aggregates.AdditionalPropertyValue.AdditionalPropertyValueUpsertModel>? ConvertToDomainPropertyModels(
        List<ResourceModels.AdditionalPropertyValueUpsertModel> properties)
    {
        if (properties != null && properties.Any())
        {
            return properties.ToDomainAdditionalProperties();
        }

        return null;
    }
}
