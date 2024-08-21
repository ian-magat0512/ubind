// <copyright file="DkimSettingController.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Controllers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using UBind.Application.Authorisation;
    using UBind.Application.Commands.DkimSettings;
    using UBind.Application.ExtensionMethods;
    using UBind.Application.Queries.DkimSettings;
    using UBind.Domain;
    using UBind.Domain.Enums;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Permissions;
    using UBind.Web.Filters;
    using UBind.Web.ResourceModels;

    /// <summary>
    /// Controller for DKIM Settings.
    /// </summary>
    [Produces("application/json")]
    [Route("/api/v1/dkim-settings")]
    public class DkimSettingController : PortalBaseController
    {
        private readonly ICqrsMediator mediator;
        private readonly IAuthorisationService authorisationService;
        private readonly ICachingResolver cachingResolver;

        /// <summary>
        /// Initializes a new instance of the <see cref="DkimSettingController"/> class.
        /// </summary>
        /// <param name="mediator">The mediator.</param>
        /// <param name="authorisationService">The authorisation service.</param>
        /// <param name="cachingResolver">The caching resolver.</param>
        public DkimSettingController(
            ICqrsMediator mediator,
            IAuthorisationService authorisationService,
            ICachingResolver cachingResolver)
            : base(cachingResolver)
        {
            this.mediator = mediator;
            this.authorisationService = authorisationService;
            this.cachingResolver = cachingResolver;
        }

        /// <summary>
        /// Create DKIM settings.
        /// </summary>
        /// <param name="dkimSettingsModel">The DKIM settings model.</param>
        /// <returns>returns created DKIM setting.</returns>
        [MustHaveOneOfPermissions(Permission.ManageOrganisations, Permission.ManageAllOrganisations)]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [HttpPost]
        public async Task<IActionResult> CreateDkimSettings(
            [FromBody] DkimSettingsUpsertModel dkimSettingsModel)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(dkimSettingsModel.Tenant, "create dkim settings in a different tenancy");
            if (!this.User.IsMasterUser())
            {
                await this.authorisationService.ThrowIfUserNotInOrganisationOrDefaultOrganisation(this.User, dkimSettingsModel.OrganisationId);
            }

            var command = new CreateDkimSettingsCommand(
                tenantId,
                dkimSettingsModel.OrganisationId,
                dkimSettingsModel.DomainName,
                dkimSettingsModel.PrivateKey,
                dkimSettingsModel.DnsSelector,
                dkimSettingsModel.AgentOrUserIdentifier,
                dkimSettingsModel.ApplicableDomainNameList);

            var dkimSetting = await this.mediator.Send(command, CancellationToken.None);
            var dkimSettingModel = new DkimSettingsResourceModel(dkimSetting);
            return this.Ok(dkimSettingModel);
        }

        /// <summary>
        /// update DKIM settings.
        /// </summary>
        /// <param name="dkimSettingsId">The DKIM settings Id.</param>
        /// <param name="dkimSettingsModel">The DKIM settings model.</param>
        /// <param name="organisationId">The organisation Id.</param>
        /// <returns>returns created DKIM setting.</returns>
        [MustHaveOneOfPermissions(Permission.ManageOrganisations, Permission.ManageAllOrganisations)]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [HttpPatch]
        [Route("{dkimSettingsId}/organisation/{organisationId}")]
        public async Task<IActionResult> UpdateDkimSettings(
            Guid dkimSettingsId,
            [FromBody] DkimSettingsUpsertModel dkimSettingsModel,
            Guid organisationId)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(dkimSettingsModel.Tenant, "update dkim settings in a different tenancy");
            if (!this.User.IsMasterUser())
            {
                await this.authorisationService.ThrowIfUserNotInOrganisationOrDefaultOrganisation(this.User, organisationId);
            }

            var command = new UpdateDkimSettingsCommand(
                tenantId,
                dkimSettingsId,
                organisationId,
                dkimSettingsModel.DomainName,
                dkimSettingsModel.PrivateKey,
                dkimSettingsModel.DnsSelector,
                dkimSettingsModel.AgentOrUserIdentifier,
                dkimSettingsModel.ApplicableDomainNameList);

            var dkimSetting = await this.mediator.Send(command, CancellationToken.None);
            var dkimSettingModel = new DkimSettingsResourceModel(dkimSetting);
            return this.Ok(dkimSettingModel);
        }

        /// <summary>
        /// Delete DKIM settings.
        /// </summary>
        /// <param name="dkimSettingsId">The DKIM settings Id.</param>
        /// <param name="organisationId">The organisation Id.</param>
        /// <returns>returns created DKIM setting.</returns>
        [MustBeLoggedIn]
        [HttpDelete]
        [MustHaveOneOfPermissions(Permission.ManageOrganisations, Permission.ManageAllOrganisations)]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [Route("{dkimSettingsId}/organisation/{organisationId}")]
        public async Task<IActionResult> DeleteDkimSettings(Guid dkimSettingsId, Guid organisationId, [FromQuery] string? tenant = null)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(tenant, "delete dkim settings in a different tenancy");
            if (!this.User.IsMasterUser())
            {
                await this.authorisationService.ThrowIfUserNotInOrganisationOrDefaultOrganisation(this.User, organisationId);
            }

            var command = new DeleteDkimSettingsCommand(
                tenantId,
                dkimSettingsId,
                organisationId);
            await this.mediator.Send(command, CancellationToken.None);
            return this.Ok();
        }

        /// <summary>
        /// Gets DKIM settings by organisation Id.
        /// </summary>
        /// <param name="organisation">The organisation ID or alias.</param>
        /// <param name="tenant">The tenant ID or alias.</param>
        /// <returns>The list of DKIM settings.</returns>
        [MustHaveOneOfPermissions(Permission.ManageOrganisations, Permission.ManageAllOrganisations)]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [HttpGet]
        public async Task<IActionResult> GetDkimSettingsByOrganisationId(
            [FromQuery] string organisation, [FromQuery] string? tenant)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(tenant, "get DKIM settings from a different tenancy");
            var organisationModel = await this.cachingResolver.GetOrganisationOrThrow(tenantId, new GuidOrAlias(organisation));
            if (this.User.GetTenantId() != Tenant.MasterTenantId)
            {
                await this.authorisationService.ThrowIfUserNotInOrganisationOrDefaultOrganisation(this.User, organisationModel.Id, tenantId);
            }

            var request = new GetDkimSettingsByTenantIdAndOrganisationIdQuery(tenantId, organisationModel.Id);
            var dkimSettings = await this.mediator.Send(request);
            return this.Ok(dkimSettings.Select(m => new DkimSettingsResourceModel(m)));
        }

        /// <summary>
        /// Gets DKIM settings for organisation.
        /// </summary>
        /// <param name="dkimSettingsId">The DKIM settings Id.</param>
        /// <param name="organisation">The organisation Id.</param>
        /// <param name="tenant">The tenant ID or alias.</param>
        /// <returns>The list of DKIM settings.</returns>
        [MustHaveOneOfPermissions(Permission.ManageOrganisations, Permission.ManageAllOrganisations)]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [HttpGet]
        [Route("{dkimSettingsId}/organisation/{organisation}")]
        public async Task<IActionResult> GetDkimSettingsById(Guid dkimSettingsId, string organisation, [FromQuery] string? tenant)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(tenant, "get DKIM settings from a different tenancy");
            var organisationModel = await this.cachingResolver.GetOrganisationOrThrow(tenantId, new GuidOrAlias(organisation));
            if (this.User.GetTenantId() != Tenant.MasterTenantId)
            {
                await this.authorisationService.ThrowIfUserNotInOrganisationOrDefaultOrganisation(this.User, organisationModel.Id, tenantId);
            }

            var request = new GetDkimSettingByIdQuery(tenantId, dkimSettingsId, organisationModel.Id);
            var dkimSetting = await this.mediator.Send(request);
            return this.Ok(new DkimSettingsResourceModel(dkimSetting));
        }

        /// <summary>
        /// Send Test DKIM test email.
        /// </summary>
        /// <param name="dkimTestEmailModel">The DKIM test email model.</param>
        /// <returns>Ok.</returns>
        [MustHaveOneOfPermissions(Permission.ManageOrganisations, Permission.ManageAllOrganisations)]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [HttpPost]
        [Route("send-dkim-test-email")]
        public async Task<IActionResult> SendTestDkimTestEmail(
            [FromBody] DkimTestEmailModel dkimTestEmailModel)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(dkimTestEmailModel.Tenant, "access DKIM settings from a different tenancy");
            if (this.User.GetTenantId() != Tenant.MasterTenantId)
            {
                await this.authorisationService.ThrowIfUserNotInOrganisationOrDefaultOrganisation(this.User, dkimTestEmailModel.OrganisationId);
            }

            var command = new SendDkimTestEmailCommand(
                tenantId,
                dkimTestEmailModel.DkimSettingsId,
                dkimTestEmailModel.OrganisationId,
                dkimTestEmailModel.RecipientEmailAddress,
                dkimTestEmailModel.SenderEmailAddress);

            await this.mediator.Send(command, CancellationToken.None);
            return this.Ok();
        }
    }
}
