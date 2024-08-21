// <copyright file="ContextEntityController.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Controllers.Portal
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using UBind.Application.Queries.ContextEntity;
    using UBind.Domain;
    using UBind.Domain.Enums;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Product;
    using UBind.Web.Filters;

    /// <summary>
    /// Controller for context entity requests.
    /// [Route("/api/v1/{environment}/{productAlias}/context-entity")].
    /// </summary>
    [Produces("application/json")]
    [Route("/api/v1/tenant/{tenant}/product/{product}/product-release/{productReleaseId}/environment/{environment}/context-entity")]
    [ApiController]
    public class ContextEntityController : ControllerBase
    {
        private readonly ICqrsMediator mediator;
        private readonly ICachingResolver cachingResolver;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContextEntityController"/> class.
        /// </summary>
        /// <param name="mediator">The mediator that encapsulates request/response and publishing interaction patterns.</param>
        /// <param name="cachingResolver">The caching resolver.</param>
        public ContextEntityController(
            ICqrsMediator mediator,
            ICachingResolver cachingResolver)
        {
            this.mediator = mediator;
            this.cachingResolver = cachingResolver;
        }

        /// <summary>
        /// Gets the context entities.
        /// </summary>
        /// <param name="tenant">The tenant alias or Id.</param>
        /// <param name="product">The product alias or Id.</param>
        /// <param name="productReleaseId">The product release ID.</param>
        /// <param name="environment">The deployment environment.</param>
        /// <param name="organisation">The organisation alias or Id.</param>
        /// <param name="entityId">The entity Id.</param>
        /// <param name="formType">The web form app type.</param>
        /// <param name="quoteType">The type of quote.</param>
        /// <returns>Context entities.</returns>
        [HttpGet]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 20)]
        public async Task<IActionResult> GetContextEntities(
            string tenant,
            string product,
            Guid productReleaseId,
            DeploymentEnvironment environment,
            [FromQuery] string organisation,
            [FromQuery] Guid entityId,
            [FromQuery] WebFormAppType formType,
            [FromQuery] QuoteType? quoteType = null)
        {
            var tenantModel = await this.cachingResolver.GetTenantOrThrow(new GuidOrAlias(tenant));
            var productModel = await this.cachingResolver.GetProductOrThrow(tenantModel.Id, new GuidOrAlias(product));
            var organisationModel = await this.cachingResolver.GetOrganisationOrThrow(tenantModel.Id, new GuidOrAlias(organisation));
            var releaseContext = new ReleaseContext(
                tenantModel.Id,
                productModel.Id,
                environment,
                productReleaseId);
            var request = new GetContextEntitiesQuery(
                releaseContext,
                organisationModel.Id,
                entityId,
                formType,
                quoteType);
            var searchResult = await this.mediator.Send(request);
            return this.Ok(searchResult);
        }
    }
}
