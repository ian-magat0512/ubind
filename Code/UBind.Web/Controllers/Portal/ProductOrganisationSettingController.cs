// <copyright file="ProductOrganisationSettingController.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Controllers.Organisation
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using UBind.Application.Authorisation;
    using UBind.Application.Commands.ProductOrganisation;
    using UBind.Application.Queries.ProductOrganisation;
    using UBind.Domain;
    using UBind.Domain.Enums;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Permissions;
    using UBind.Domain.ReadWriteModel;
    using UBind.Web.Filters;

    /// <summary>
    /// Controller for product requests.
    /// </summary>
    [Authorize]
    [Produces("application/json")]
    [Route("api/v1/organisation/{organisation}/product")]
    public class ProductOrganisationSettingController : PortalBaseController
    {
        private readonly ICqrsMediator mediator;
        private readonly IAuthorisationService authorisationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductOrganisationSettingController"/> class.
        /// </summary>
        /// <param name="mediator">The mediator that encapsulates request/response and publishing interaction patterns.</param>
        /// <param name="cachingResolver">The caching resolver.</param>
        /// <param name="authorisationService">The service that provides a number of common authorisation checks.</param>
        public ProductOrganisationSettingController(
            ICqrsMediator mediator,
            ICachingResolver cachingResolver,
            IAuthorisationService authorisationService)
            : base(cachingResolver)
        {
            this.mediator = mediator;
            this.authorisationService = authorisationService;
        }

        /// <summary>
        /// Gets the organisation products settings.
        /// </summary>
        /// <param name="organisation">The orgarnisation Id or Alias.</param>
        /// <param name="tenant">The tenant Id or Alias.</param>
        /// <returns>All products.</returns>
        [HttpGet]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [MustHaveOneOfPermissions(
            Permission.ViewQuotes,
            Permission.ViewAllQuotes,
            Permission.ViewAllQuotesFromAllOrganisations,
            Permission.ManageQuotes,
            Permission.ManageAllQuotes,
            Permission.ManageAllQuotesForAllOrganisations,
            Permission.ManageAllOrganisations,
            Permission.ManageOrganisations)]
        [ProducesResponseType(typeof(IEnumerable<ProductOrganisationSettingModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetProductOrganisationSettings(
            string organisation,
            [FromQuery] string tenant)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(tenant, "get product organisation settings from a different tenancy");
            var organisationModel = await this.CachingResolver.GetOrganisationOrThrow(tenantId, new GuidOrAlias(organisation));
            var query = new GetProductOrganisationSettingsQuery(tenantId, organisationModel.Id);
            var settings = await this.mediator.Send(query);
            return this.Ok(settings);
        }

        /// <summary>
        /// Enable the specific product setting.
        /// </summary>
        /// <param name="organisation">The organisation Id or Alias.</param>
        /// <param name="product">The Id or Alias of the product to enable.</param>
        /// <param name="tenant">The tenant Id or Alias of the current user.</param>
        /// <returns>The enabled product setting.</returns>
        [HttpPatch]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 30)]
        [MustHaveOneOfPermissions(
            Permission.ManageQuotes,
            Permission.ManageAllQuotes,
            Permission.ManageAllQuotesForAllOrganisations,
            Permission.ManageAllOrganisations,
            Permission.ManageOrganisations)]
        [Route("{product}/enable")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Enable(string organisation, string product, [FromQuery] string tenant)
        {
            await this.AllowNewQuotes(organisation, product, tenant, true);
            return this.Ok();
        }

        /// <summary>
        /// Disable the specific product setting.
        /// </summary>
        /// <param name="organisation">The organisation Id or Alias.</param>
        /// <param name="product">The Id or Alias of the product to disable.</param>
        /// <param name="tenant">The tenant Id or Alias of the current user.</param>
        /// <returns>The disabled product setting.</returns>
        [HttpPatch]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 30)]
        [MustHaveOneOfPermissions(
            Permission.ManageQuotes,
            Permission.ManageAllQuotes,
            Permission.ManageAllQuotesForAllOrganisations,
            Permission.ManageAllOrganisations,
            Permission.ManageOrganisations)]
        [Route("{product}/disable")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Disable(string organisation, string product, [FromQuery] string tenant)
        {
            await this.AllowNewQuotes(organisation, product, tenant, false);
            return this.Ok();
        }

        private async Task AllowNewQuotes(string organisation, string product, string tenant, bool isNewQuotesAllowed)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(tenant, "allow/disallow new quotes for a product from another tenancy");
            var productModel = await this.CachingResolver.GetProductOrThrow(tenantId, new GuidOrAlias(product));
            var organisationModel = await this.CachingResolver.GetOrganisationOrThrow(tenantId, new GuidOrAlias(organisation));
            await this.authorisationService.ThrowIfUserCannotAccessTenant(tenantId, this.User);
            await this.authorisationService.ThrowIfUserCannotManageOrganisationsAndProducts(this.User, "AllowNewQuotes");
            var request = new UpdateProductOrganisationSettingsCommand(tenantId, organisationModel.Id, productModel.Id, isNewQuotesAllowed);
            await this.mediator.Send(request);
        }
    }
}
