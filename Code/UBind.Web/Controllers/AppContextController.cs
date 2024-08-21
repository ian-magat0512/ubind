// <copyright file="AppContextController.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Controllers
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using UBind.Application.Queries.Quote;
    using UBind.Domain;
    using UBind.Domain.Configuration;
    using UBind.Domain.Enums;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;
    using UBind.Web.Filters;
    using UBind.Web.ResourceModels;

    /// <summary>
    /// Makes essential contextual information available to the FormsApp and the PortalApp which helps
    /// determine the environment a users portal session or forms app session runs in.
    /// </summary>
    [Produces("application/json")]
    [Route("api/v1/app-context")]
    public class AppContextController : Controller
    {
        private readonly ICachingResolver cachingResolver;
        private readonly ICqrsMediator mediator;
        private readonly IEmailInvitationConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="AppContextController"/> class.
        /// </summary>
        /// <param name="mediator">The mediator.</param>
        /// <param name="cachingResolver">The tenant, product and portal resolver.</param>
        /// <param name="configuration">The configuration to get the app base URL.</param>
        public AppContextController(
            ICqrsMediator mediator,
            ICachingResolver cachingResolver,
            IEmailInvitationConfiguration configuration)
        {
            this.cachingResolver = cachingResolver;
            this.mediator = mediator;
            this.configuration = configuration;
        }

        /// <summary>
        /// Gets the contextual information for a portal app session.
        /// This information includes the details of the portal and the organisation, so that the portal
        /// theme can be applied before users log in.
        /// </summary>
        /// <param name="tenant">The alias or ID of the tenant.</param>
        /// <param name="organisation">The alias of the organisation.</param>
        /// <param name="portal">The alias of the portal.</param>
        /// <returns>The summary of the organisation.</returns>
        [HttpGet]
        [Route("portal")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 80)]
        [AllowAnonymous]
        [ProducesResponseType(typeof(PortalAppContextModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPortalAppContext(
            string tenant,
            string organisation = "",
            string portal = "")
        {
            var tenantModel = await this.cachingResolver.GetTenantOrThrow(new GuidOrAlias(tenant));
            var portalTitle = string.Empty;
            string portalStylesheetUrl = null;
            string portalStyles = null;
            PortalUserType portalUserType = PortalUserType.Agent;
            Guid? portalId = null;
            string portalAlias = null;
            bool isDefaultAgentPortal = false;
            OrganisationReadModel organisationModel = null;

            if (!string.IsNullOrEmpty(organisation))
            {
                organisationModel = await this.cachingResolver.GetOrganisationOrThrow(tenantModel.Id, new GuidOrAlias(organisation));
                if (!organisationModel.IsActive)
                {
                    throw new ErrorException(Errors.Organisation.Login.Disabled(organisationModel.Name));
                }
            }

            if (!string.IsNullOrEmpty(portal))
            {
                var portalModel = await this.cachingResolver.GetPortalOrThrow(tenantModel.Id, new GuidOrAlias(portal));
                if (portalModel.Disabled)
                {
                    throw new ErrorException(Errors.Organisation.Login.OrganisationPortalDisabled(portalModel.Name));
                }
                portalId = portalModel.Id;
                portalAlias = portalModel.Alias;
                portalUserType = portalModel.UserType;
                isDefaultAgentPortal = portalModel.IsDefault && portalModel.UserType == PortalUserType.Agent;
                portalTitle = portalModel.Title;
                portalStylesheetUrl = portalModel.StyleSheetUrl;
                portalStyles = portalModel.Styles;
                if (organisationModel == null)
                {
                    organisationModel
                        = await this.cachingResolver.GetOrganisationOrThrow(tenantModel.Id, new GuidOrAlias(portalModel.OrganisationId));
                }
            }

            if (organisationModel == null)
            {
                organisationModel
                    = await this.cachingResolver.GetOrganisationOrThrow(tenantModel.Id, tenantModel.Details.DefaultOrganisationId);
            }

            if (portalId == null)
            {
                // get the default portal for the organisation, or tenant (which will always be an agent portal)
                portalId = organisationModel.DefaultPortalId.HasValue
                    ? organisationModel.DefaultPortalId.Value
                    : tenantModel.Details.DefaultPortalId;

                if (!tenantModel.IsMasterTenant && portalId != default(Guid))
                {
                    var portalModel = await this.cachingResolver.GetPortalOrThrow(tenantModel.Id, portalId.Value);
                    portalId = portalModel.Id;
                    portalAlias = portalModel.Alias;
                    isDefaultAgentPortal = portalModel.IsDefault && portalModel.UserType == PortalUserType.Agent;
                    portalUserType = portalModel.UserType;
                    portalTitle = portalModel.Title;
                    portalStylesheetUrl = portalModel.StyleSheetUrl;
                    portalStyles = portalModel.Styles;
                }
            }

            string appBaseUrl = tenantModel.Details.CustomDomain.IsNullOrEmpty()
                ? this.configuration.InvitationLinkHost
                : $"https://{tenantModel.Details.CustomDomain}/";
            var portalAppContext = new PortalAppContextModel(
                organisationModel.TenantId,
                tenantModel.Details.Name,
                tenantModel.Details.Alias,
                organisationModel.Id,
                organisationModel.Alias,
                organisationModel.Id == tenantModel.Details.DefaultOrganisationId,
                organisationModel.Name,
                portalId.Value,
                portalAlias,
                portalUserType,
                isDefaultAgentPortal,
                portalTitle,
                portalStylesheetUrl,
                portalStyles,
                tenantModel.Details.CustomDomain,
                appBaseUrl);
            return this.Ok(portalAppContext);
        }

        /// <summary>
        /// Gets the contextual information for a forms app session.
        /// This information includes only the necessary details to ensure it's fast and doesn't slow down the loading
        /// stage of the forms app.
        /// </summary>
        /// <param name="tenant">The alias or ID of the tenant.</param>
        /// <param name="organisation">The alias of the organisation.</param>
        /// <param name="portal">The alias of the portal.</param>
        /// <param name="quoteId">The ID of the quote.</param>
        /// <returns>The summary of the organisation.</returns>
        [HttpGet]
        [Route("forms")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 80)]
        [AllowAnonymous]
        [ProducesResponseType(typeof(FormsAppContextModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetFormsAppContext(
            string tenant,
            string product,
            string organisation = "",
            string portal = "",
            Guid? quoteId = null)
        {
            var tenantModel = await this.cachingResolver.GetTenantOrThrow(new GuidOrAlias(tenant));
            if (tenantModel.Details.Disabled)
            {
                throw new ErrorException(Errors.Tenant.Disabled(tenantModel.Details.Name));
            }

            Guid? portalId = null;
            string portalAlias = null;
            OrganisationReadModel organisationModel = null;

            if (!string.IsNullOrEmpty(organisation))
            {
                organisationModel = await this.cachingResolver.GetOrganisationOrThrow(tenantModel.Id, new GuidOrAlias(organisation));
            }

            if (!string.IsNullOrEmpty(portal))
            {
                var portalModel = await this.cachingResolver.GetPortalOrThrow(tenantModel.Id, new GuidOrAlias(portal));
                portalId = portalModel.Id;
                portalAlias = portalModel.Alias;
                if (organisationModel == null)
                {
                    organisationModel
                        = await this.cachingResolver.GetOrganisationOrThrow(tenantModel.Id, new GuidOrAlias(portalModel.OrganisationId));
                }
            }

            if (organisationModel == null)
            {
                if (quoteId.HasValue)
                {
                    organisationModel
                        = await this.GetOrganisationForQuote(tenantModel.Id, quoteId.Value);
                }
                else
                {
                    organisationModel
                        = await this.cachingResolver.GetOrganisationOrThrow(tenantModel.Id, tenantModel.Details.DefaultOrganisationId);
                }
            }

            var productModel = await this.cachingResolver.GetProductOrThrow(tenantModel.Id, new GuidOrAlias(product));
            if (productModel.Details.Disabled)
            {
                throw new ErrorException(Errors.Product.Disabled(productModel.Details.Alias));
            }

            var organisationSummary = new FormsAppContextModel(
                organisationModel.TenantId,
                tenantModel.Details.Name,
                tenantModel.Details.Alias,
                organisationModel.Id,
                organisationModel.Name,
                organisationModel.Alias,
                productModel.Id,
                productModel.Details.Alias,
                portalId,
                portalAlias,
                organisationModel.Id == tenantModel.Details.DefaultOrganisationId);
            return this.Ok(organisationSummary);
        }

        private async Task<OrganisationReadModel> GetOrganisationForQuote(Guid tenantId, Guid quoteId)
        {
            var quote = await this.mediator.Send(new GetQuoteSummaryByIdQuery(tenantId, quoteId));
            if (quote == null)
            {
                throw new ErrorException(Errors.Quote.NotFound(quoteId));
            }

            return await this.cachingResolver.GetOrganisationOrThrow(tenantId, quote.OrganisationId);
        }
    }
}
