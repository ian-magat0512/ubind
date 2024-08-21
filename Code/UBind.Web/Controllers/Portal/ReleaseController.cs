// <copyright file="ReleaseController.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using UBind.Application;
    using UBind.Application.Commands.Release;
    using UBind.Application.Queries.Product;
    using UBind.Application.Queries.ProductRelease;
    using UBind.Domain;
    using UBind.Domain.Enums;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Permissions;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Web.Extensions;
    using UBind.Web.Filters;
    using UBind.Web.Helpers;
    using UBind.Web.Mapping;
    using UBind.Web.ResourceModels;
    using UBind.Web.ResourceModels.ProductRelease;

    /// <summary>
    /// Controller for product requests.
    /// </summary>
    [Produces("application/json")]
    [Route("api/v1/release")]
    [MustBeLoggedIn(Policy = UserTypePolicies.ClientOrMaster)]
    public class ReleaseController : PortalBaseController
    {
        private readonly IReleaseRepository releaseRepository;
        private readonly IReleaseService releaseService;
        private readonly ICqrsMediator mediator;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReleaseController"/> class.
        /// </summary>
        /// <param name="releaseRepository">The release repository.</param>
        /// <param name="cachingResolver">The tenant, product and portal resolver.</param>
        /// <param name="releaseService">The release application service.</param>
        /// <param name="mediator">The cqrs mediator.</param>
        public ReleaseController(
            IReleaseRepository releaseRepository,
            ICachingResolver cachingResolver,
            IReleaseService releaseService,
            ICqrsMediator mediator)
            : base(cachingResolver)
        {
            this.releaseRepository = releaseRepository;
            this.releaseService = releaseService;
            this.mediator = mediator;
        }

        /// <summary>
        /// Gets all releases for products.
        /// </summary>
        /// <param name="product">The ID or Alias of the product to retrieve releases for.</param>
        /// <param name="tenant">The ID or Alias of the product's tenant to retrieve releases for.</param>
        /// <returns>All releases related to products.</returns>
        [HttpGet("product")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [ValidateModel]
        [MustHavePermission(Permission.ViewReleases)]
        [ProducesResponseType(typeof(IEnumerable<ReleaseModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetReleasesForProduct([FromQuery, Required] string product, [FromQuery, Required] string tenant)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(tenant, "list releases");
            Product productModel = await this.CachingResolver.GetProductOrThrow(tenantId, new GuidOrAlias(product));
            IEnumerable<Release> releases = this.releaseRepository.GetReleasesForProduct(
                tenantId,
                productModel.Id,
                null);
            IEnumerable<ReleaseModel> vms = releases.Select(r => new ReleaseModel(r, productModel));
            return this.Ok(vms);
        }

        /// <summary>
        /// Gets all releases based on the options.
        /// </summary>
        /// <param name="options">The filter options.</param>
        /// <returns>All releases.</returns>
        [HttpGet]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [MustHavePermission(Permission.ViewReleases)]
        [ProducesResponseType(typeof(IEnumerable<ReleaseModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetReleases([FromQuery] QueryOptionsModel options)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(options.Tenant, "list releases");
            IEnumerable<ReleaseModel> resources = this.releaseService.GetReleases(
                tenantId,
                await options.ToFilters(tenantId, this.CachingResolver, nameof(Release.CreatedTicksSinceEpoch)))
                         .Select(r => new ReleaseModel(r));
            return this.Ok(resources);
        }

        /// <summary>
        /// Gets a release by ID.
        /// </summary>
        /// <param name="releaseId">The ID of the release to return.</param>
        /// <returns>The product.</returns>
        [HttpGet]
        [Route("{releaseId}")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [MustHavePermission(Permission.ViewReleases)]
        [ProducesResponseType(typeof(ReleaseModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetReleaseById(Guid releaseId, [FromQuery] string tenant)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(tenant, "get a release");
            Release release = this.releaseRepository.GetReleaseWithoutAssetFileContents(tenantId, releaseId);
            if (release == null)
            {
                return Errors.General.NotFound("release", releaseId).ToProblemJsonResult();
            }

            Product product = await this.CachingResolver.GetProductOrThrow(release.TenantId, release.ProductId);
            var releaseVM = new ReleaseModel(release, product);
            return this.Ok(releaseVM);
        }

        /// <summary>
        /// Gets a list of source files for a specific release.
        /// </summary>
        /// <param name="releaseId">The id of the release.</param>
        /// <returns>Ok.</returns>
        [HttpGet]
        [Route("{releaseId}/file")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [MustHavePermission(Permission.ViewReleases)]
        [ProducesResponseType(typeof(IEnumerable<ConfigurationFileModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSourceFilesForRelease(Guid releaseId, [FromQuery] string tenant)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(tenant, "list source files for a release");
            Release release = this.releaseRepository.GetReleaseWithoutAssetFileContents(tenantId, releaseId);
            var query = GetProductReleaseSourceFilesQuery.CreateForRelease(tenantId, releaseId);
            var files = await this.mediator.Send(query);
            var models = files.Select(file => new ConfigurationFileModel(file));
            return this.Ok(models);
        }

        /// <summary>
        /// Gets a release file by path.
        /// Does not use caching, so lookups are slow but accurate.
        /// </summary>
        /// <param name="releaseId">The ID of the release.</param>
        /// <param name="filePath">The file path.</param>
        /// <param name="formType">The type of form configuration to load.</param>
        /// <returns>The product.</returns>
        [HttpGet]
        [Route("{releaseId}/{formType}/file/{*filePath}")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [MustHavePermission(Permission.ViewReleases)]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetReleaseFileByFilePath(
            Guid releaseId, FormType formType, string filePath, [FromQuery] string tenant)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(tenant, "get a release file by path");
            var webFormAppType = formType.ToWebFormAppType();
            var fileContent = await this.mediator.Send(new GetReleaseFileWithoutCachingQuery(
                tenantId,
                releaseId,
                webFormAppType,
                filePath));

            string contentType = fileContent.ContentType ?? MimeTypeHelper.DetectMimeType(filePath);
            var fileContentResult = new FileContentResult(fileContent.Content, contentType);
            return fileContentResult;
        }

        /// <summary>
        /// Create a new release.
        /// </summary>
        /// <param name="releaseModel">Release model with details of the release to create.</param>
        /// <returns>The newly created release.</returns>
        [HttpPost]
        [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 5)]
        [ValidateModel]
        [MustHavePermission(Permission.ManageReleases)]
        [ProducesResponseType(typeof(ReleaseModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateRelease([FromBody] ReleaseUpsertModel releaseModel)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(releaseModel.Tenant, "create a release");
            var product = await this.CachingResolver.GetProductOrThrow(tenantId, new GuidOrAlias(releaseModel.Product));
            Release release = await this.releaseService.CreateReleaseAsync(
                product.TenantId,
                product.Id,
                releaseModel.Description,
                releaseModel.Type);
            var releaseVM = new ReleaseModel(release, product);
            return this.Ok(releaseVM);
        }

        /// <summary>
        /// Update an existing relese.
        /// </summary>
        /// <param name="releaseId">The ID of the release to update.</param>
        /// <param name="releaseModel">New product details.</param>
        /// <returns>The updated product.</returns>
        [HttpPut]
        [Route("{releaseId}")]
        [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 5)]
        [ValidateModel]
        [MustHavePermission(Permission.ManageReleases)]
        [ProducesResponseType(typeof(ReleaseModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateRelease(Guid releaseId, [FromBody] ReleaseUpsertModel releaseModel)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(releaseModel.Tenant, "update a release");
            Release release = this.releaseRepository.GetReleaseWithoutAssetFileContents(tenantId, releaseId);
            if (release == null)
            {
                return Errors.General.NotFound("release", releaseId).ToProblemJsonResult();
            }

            var product = await this.CachingResolver.GetProductOrThrow(tenantId, new GuidOrAlias(releaseModel.Product));
            Release releaseResult = this.releaseService.UpdateRelease(
                tenantId,
                releaseId,
                product.Id,
                release.Number,
                release.MinorNumber,
                releaseModel.Description,
                releaseModel.Type);
            var releaseVM = new ReleaseModel(releaseResult, product);
            return this.Ok(releaseVM);
        }

        /// <summary>
        /// Delete a release.
        /// </summary>
        /// <param name="releaseId">The ID of the release to delete.</param>
        /// <returns>Ok.</returns>
        [HttpDelete]
        [Route("{releaseId}")]
        [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 5)]
        [MustHavePermission(Permission.ManageReleases)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteRelease(Guid releaseId, [FromQuery] string tenant)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(tenant, "delete a release");
            Release release = this.releaseRepository.GetReleaseWithoutAssetFileContents(tenantId, releaseId);
            if (release == null)
            {
                return Errors.General.NotFound("release", releaseId).ToProblemJsonResult();
            }

            this.releaseService.DeleteRelease(tenantId, releaseId);
            return this.NoContent();
        }

        /// <summary>
        /// Restore a release to the development environment.
        /// </summary>
        /// <param name="releaseId">The ID of the release to restore.</param>
        /// <returns>No content.</returns>
        [HttpPost]
        [Route("{releaseId}/restore")]
        [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 5)]
        [MustHavePermission(Permission.ManageReleases)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Restore(Guid releaseId, [FromQuery] string tenant)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(tenant, "restore a release to the development environment");
            await this.releaseService.RestoreReleaseToDevelopmentEnvironment(tenantId, releaseId);
            return this.NoContent();
        }

        /// <summary>
        /// This is to migrate quotes and policy transactions associated with a release to new release.
        /// </summary>
        /// <param name="releaseId">The ID of the release.</param>
        /// <param name="newReleaseId">The ID of the new release.</param>
        /// <returns>No content.</returns>
        [HttpPost]
        [Route("{releaseId}/quote-policy-transaction/migrate/{newReleaseId}")]
        [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 5)]
        [MustHavePermission(Permission.ManageReleases)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> MigrateQuotesAndPolicyTransactionsToRelease(
            Guid releaseId, Guid newReleaseId, string environment, [FromQuery] string tenant)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(
                tenant,
                "migrate quotes and policy transactions associated with a release to new release");
            var env = environment.ToEnumOrThrow<DeploymentEnvironment>();
            await this.mediator.Send(new MigrateEntitiesToNewProductReleaseCommand(tenantId, releaseId, newReleaseId, env));
            return this.NoContent();
        }

        /// <summary>
        /// Controller action for migrating unassociated quotes and policy transactions to a new release
        /// </summary>
        /// <param name="newReleaseId">The ID of the new release.</param>
        /// <returns>No content.</returns>
        [HttpPost]
        [Route("unassociated/quote-policy-transaction/migrate/{newReleaseId}")]
        [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 5)]
        [MustHavePermission(Permission.ManageReleases)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> MigrateUnassociatedQuotesAndPolicyTransactionsToRelease(
            Guid newReleaseId, string environment, [FromQuery] string tenant, [FromQuery] string product)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(
                tenant,
                "migrate unassociated quotes and policy transactions to new release");
            var productId = await this.CachingResolver.GetProductIdOrThrow(
                tenantId, new GuidOrAlias(product));
            var env = environment.ToEnumOrThrow<DeploymentEnvironment>();
            await this.mediator.Send(
                new MigrateUnassociatedEntitiesToProductReleaseCommand(tenantId, productId, newReleaseId, env));
            return this.NoContent();
        }

        /// <summary>
        /// Gets the count of quote and policy transaction that is associated with product release.
        /// </summary>
        /// <param name="releaseId">The ID of the release.</param>
        /// <param name="environment">The environment that is associated with the quotes and policy transaciton.</param>
        /// <returns>Return the count of quote and policy transaction.</returns>
        [HttpGet]
        [Route("{releaseId}/quote-policy-transaction/count")]
        [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 5)]
        [MustHavePermission(Permission.ManageReleases)]
        [ProducesResponseType(typeof(QuotePolicyTransactionCountModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> QuoteAndPolicyTransactionCountAssociatedWithProductRelease(
            Guid releaseId, string environment, [FromQuery] string tenant)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(
                tenant,
                "count quotes and policy transactions associated with a release");
            var env = environment.ToEnumOrThrow<DeploymentEnvironment>();
            var result = await this.mediator.Send(new GetQuoteAndPolicyTransactionCountByProductReleaseIdQuery(tenantId, releaseId, env));
            return this.Ok(result);
        }

        /// <summary>
        /// Retrieves the count of quotes and policy transactions that are not associated with a release.
        /// </summary>
        /// <param name="environment">The deployment environment (e.g., Production, Staging).</param>
        /// <param name="tenant">The alias or unique identifier of the tenant.</param>
        /// <param name="product">The alias or unique identifier of the product.</param>
        /// <returns>Return the count of quote and policy transaction that are not associated with a release.</returns>
        [HttpGet]
        [Route("unassociated/quote-policy-transaction/count")]
        [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 5)]
        [MustHavePermission(Permission.ManageReleases)]
        [ProducesResponseType(typeof(QuotePolicyTransactionCountModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUnassociatedQuoteAndPolicyTransactionCount(
            string environment, [FromQuery] string tenant, [FromQuery] string product)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(
                tenant,
                "count quotes and policy transactions not associated with a release");
            var productId = await this.CachingResolver.GetProductIdOrThrow(
                tenantId, new GuidOrAlias(product));
            var env = environment.ToEnumOrThrow<DeploymentEnvironment>();
            var result = await this.mediator.Send(new GetUnassociatedQuoteAndPolicyTransactionCountQuery(tenantId, productId, env));
            return this.Ok(result);
        }
    }
}
