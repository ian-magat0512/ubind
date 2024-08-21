// <copyright file="ProductPortalSettingController.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Controllers.Portal
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using UBind.Application.Authorisation;
    using UBind.Application.Commands.ProductPortal;
    using UBind.Application.Queries.ProductPortal;
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
    [MustBeLoggedIn]
    [Produces("application/json")]
    [Route("api/v1/portal/{portal}/product")]
    public class ProductPortalSettingController : PortalBaseController
    {
        private readonly ICachingResolver cachingResolver;
        private readonly ICqrsMediator mediator;
        private readonly IAuthorisationService authorisationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductPortalSettingController"/> class.
        /// </summary>
        /// <param name="mediator">The mediator that encapsulates request/response and publishing interaction patterns.</param>
        /// <param name="cachingResolver">The tenant, product and portal resolver.</param>
        /// <param name="authorisationService">The service that provides a number of common authorisation checks.</param>
        public ProductPortalSettingController(
            ICqrsMediator mediator,
            ICachingResolver cachingResolver,
            IAuthorisationService authorisationService)
            : base(cachingResolver)
        {
            this.cachingResolver = cachingResolver;
            this.mediator = mediator;
            this.authorisationService = authorisationService;
        }

        /// <summary>
        /// Gets the product portal settings.
        /// </summary>
        /// <param name="portal">The portal Id or Alias.</param>
        /// <param name="tenant">The tenant Id or Alias.</param>
        /// <returns>All products.</returns>
        [HttpGet]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [ProducesResponseType(typeof(IEnumerable<ProductPortalSettingModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetProductPortalSettings(string portal, [FromQuery] string tenant)
        {
            var tenantModel = await this.CachingResolver.GetTenantOrThrow(new GuidOrAlias(tenant));
            var portalModel = await this.CachingResolver.GetPortalOrThrow(tenantModel, new GuidOrAlias(portal));
            await this.authorisationService.ThrowIfUserCannotAccessTenant(tenantModel.Id, this.User);
            IQuery<IEnumerable<ProductPortalSettingModel>> request =
                new GetProductPortalSettingsByPortalIdQuery(tenantModel.Id, portalModel.Id);

            var searchResult = await this.mediator.Send(request);
            return this.Ok(searchResult);
        }

        /// <summary>
        /// Enable the specific product setting.
        /// </summary>
        /// <param name="portal">The portal Id or Alias.</param>
        /// <param name="product">The product Id or Alias.</param>
        /// <param name="tenant">The tenant Id or Alias.</param>
        /// <returns>The enabled product setting.</returns>
        [HttpPatch]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 30)]
        [ValidateModel]
        [MustHavePermission(Permission.ManagePortals)]
        [Route("{product}/enable")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Enable(string portal, string product, [FromQuery] string tenant)
        {
            var tenantModel = await this.CachingResolver.GetTenantOrThrow(new GuidOrAlias(tenant));
            var productModel = await this.CachingResolver.GetProductOrThrow(tenantModel, new GuidOrAlias(product));
            var portalModel = await this.CachingResolver.GetPortalOrThrow(tenantModel, new GuidOrAlias(portal));
            await this.authorisationService.ThrowIfUserCannotAccessTenant(tenantModel.Id, this.User);
            var request = new AllowNewQuotesCommand(tenantModel.Id, portalModel.Id, productModel.Id, true);
            await this.mediator.Send(request);
            return this.Ok();
        }

        /// <summary>
        /// Disable the specific product setting.
        /// </summary>
        /// <param name="portal">The portal Id or Alias.</param>
        /// <param name="product">The product Id or Alias.</param>
        /// <param name="tenant">The tenant Id or Alias.</param>
        /// <returns>The disabled product setting.</returns>
        [HttpPatch]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 30)]
        [ValidateModel]
        [MustHavePermission(Permission.ManagePortals)]
        [Route("{product}/disable")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Disable(string portal, string product, [FromQuery] string tenant)
        {
            var tenantModel = await this.CachingResolver.GetTenantOrThrow(new GuidOrAlias(tenant));
            var productModel = await this.CachingResolver.GetProductOrThrow(tenantModel, new GuidOrAlias(product));
            var portalModel = await this.CachingResolver.GetPortalOrThrow(tenantModel, new GuidOrAlias(portal));
            await this.authorisationService.ThrowIfUserCannotAccessTenant(tenantModel.Id, this.User);
            var request = new AllowNewQuotesCommand(tenantModel.Id, portalModel.Id, productModel.Id, false);
            await this.mediator.Send(request);
            return this.Ok();
        }
    }
}
