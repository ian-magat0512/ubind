// <copyright file="AssetController.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Controllers.Quoter
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.StaticFiles;
    using Microsoft.Net.Http.Headers;
    using UBind.Application;
    using UBind.Application.Queries.AssetFile;
    using UBind.Application.Queries.ProductRelease;
    using UBind.Domain;
    using UBind.Domain.Enums;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Product;
    using UBind.Web.Extensions;
    using UBind.Web.Filters;
    using UBind.Web.Mapping;

    /// <summary>
    /// Controller for asset requests.
    /// </summary>
    public class AssetController : Controller
    {
        private readonly ICachingResolver cachingResolver;
        private readonly ICqrsMediator cqrsMediator;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetController"/> class.
        /// </summary>
        /// <param name="cachingResolver">The tenant, product and portal resolver.</param>
        /// <param name="cqrsMediator">The mediator for requests.</param>
        public AssetController(
            ICachingResolver cachingResolver,
            ICqrsMediator cqrsMediator)
        {
            this.cachingResolver = cachingResolver;
            this.cqrsMediator = cqrsMediator;
        }

        /// <summary>
        /// Action handling asset request.
        /// </summary>
        /// <param name="tenant">The ID or Alias of the tenant.</param>
        /// <param name="product">The ID or Alias of the product.</param>
        /// <param name="environment">The environment.</param>
        /// <param name="formType">The form type to obtain the configuration for, either "quote" or "claim".</param>
        /// <param name="filename">The asset filename.</param>
        /// <param name="releaseId">The release ID. If not provided, the default release for the specified environment will be used.</param>
        /// <returns>The asset file for the requested environment.</returns>
        [HttpGet]
        [Route("/api/v1/tenant/{tenant}/product/{product}/environment/{environment}/form-type/{formType}/asset/{filename}")]
        [Route("/api/v1/tenant/{tenant}/product/{product}/environment/{environment}/form-type/{formType}/release/{releaseId}/asset/{filename}")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 180)]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAsset(
            string tenant,
            string product,
            string environment,
            FormType formType,
            string filename,
            Guid? releaseId = null)
        {
            if (!Enum.TryParse(environment, true, out DeploymentEnvironment env))
            {
                return Errors.General.NotFound("environment", environment).ToProblemJsonResult();
            }

            Product productModel = await this.cachingResolver.GetProductOrThrow(new GuidOrAlias(tenant), new GuidOrAlias(product));
            var releaseContext = await this.cqrsMediator.Send(new GetReleaseContextForReleaseOrDefaultReleaseQuery(
                productModel.TenantId,
                productModel.Id,
                env,
                releaseId));
            var webFormAppType = formType.ToWebFormAppType();
            byte[] contents = await this.cqrsMediator.Send(new GetProductFileContentsByFileNameQuery(
                releaseContext,
                webFormAppType,
                FileVisibility.Public,
                filename));
            string contentType;
            new FileExtensionContentTypeProvider().TryGetContentType(filename, out contentType);
            this.Response.Headers[HeaderNames.CacheControl] = (env != DeploymentEnvironment.Development) ? CacheDurations.OneDayCache : CacheDurations.NoCache;
            return this.File(contents, contentType ?? ContentTypes.Stream);
        }

        /// <summary>
        /// Gets the asset - Deprecated. Please use the version that includes the release ID.
        /// </summary>
        /// <param name="tenant">The ID or Alias of the tenant to obtain configuration for.</param>
        /// <param name="product">The ID or Alias of the product to obtain the configuration for.</param>
        /// <param name="environment">The environment to obtain the configuration for.</param>
        /// <param name="filename">asset filename.</param>
        /// <returns>The asset file for the requested environment.</returns>
        [HttpGet]
        [Route("/assets/{tenant}/{product}/{environment}/{filename}")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 180)]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        [Obsolete("To be removed in UB-")]
        public async Task<IActionResult> GetAssetDeprecated(
            string tenant,
            string product,
            string environment,
            string filename)
        {
            if (!Enum.TryParse(environment, true, out DeploymentEnvironment env))
            {
                return Errors.General.NotFound("environment", environment).ToProblemJsonResult();
            }

            Product productModel = await this.cachingResolver.GetProductOrThrow(new GuidOrAlias(tenant), new GuidOrAlias(product));
            var releaseContext = await this.cqrsMediator.Send(new GetReleaseContextForReleaseOrDefaultReleaseQuery(
                productModel.TenantId,
                productModel.Id,
                env,
                null));
            byte[] contents = await this.cqrsMediator.Send(new GetProductFileContentsByFileNameQuery(
                releaseContext,
                WebFormAppType.Quote,
                FileVisibility.Public,
                filename));
            string contentType;
            new FileExtensionContentTypeProvider().TryGetContentType(filename, out contentType);
            this.Response.Headers[HeaderNames.CacheControl] = (env != DeploymentEnvironment.Development) ? CacheDurations.OneDayCache : CacheDurations.NoCache;
            return this.File(contents, contentType ?? ContentTypes.Stream);
        }
    }
}
