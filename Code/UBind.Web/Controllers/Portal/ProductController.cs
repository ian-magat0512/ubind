// <copyright file="ProductController.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using UBind.Application;
    using UBind.Application.Commands.Product;
    using UBind.Application.Commands.Release;
    using UBind.Application.Queries.Product;
    using UBind.Application.Queries.ProductRelease;
    using UBind.Domain;
    using UBind.Domain.Enums;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Models;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Permissions;
    using UBind.Domain.Product;
    using UBind.Domain.Services.AdditionalPropertyValue;
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
    [Route("api/v1/product")]
    public class ProductController : PortalBaseController
    {
        private readonly ICachingResolver cachingResolver;
        private readonly IProductRepository productRepository;
        private readonly IReleaseRepository releaseRepository;
        private readonly IProductService productService;
        private readonly IConfigurationService configurationService;
        private readonly IReleaseService releaseService;
        private readonly IAdditionalPropertyValueService additionalPropertyValueService;
        private readonly ICqrsMediator mediator;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductController"/> class.
        /// </summary>
        /// <param name="productRepository">The product repository.</param>
        /// <param name="releaseRepository">The release repository.</param>
        /// <param name="productService">The product application service.</param>
        /// <param name="configurationService">The configuration service.</param>
        /// <param name="releaseService">The release service.</param>
        /// <param name="cachingResolver">The tenant, product and portal resolver.</param>
        /// <param name="additionalPropertyValueService">Additional property value service.</param>
        /// <param name="mediator">The mediator.</param>
        public ProductController(
            IProductRepository productRepository,
            IReleaseRepository releaseRepository,
            IProductService productService,
            IConfigurationService configurationService,
            IReleaseService releaseService,
            ICachingResolver cachingResolver,
            IAdditionalPropertyValueService additionalPropertyValueService,
            ICqrsMediator mediator)
            : base(cachingResolver)
        {
            this.productRepository = productRepository;
            this.cachingResolver = cachingResolver;
            this.releaseRepository = releaseRepository;
            this.productService = productService;
            this.configurationService = configurationService;
            this.releaseService = releaseService;
            this.additionalPropertyValueService = additionalPropertyValueService;
            this.mediator = mediator;
        }

        /// <summary>
        /// Gets products based on the options.
        /// </summary>
        /// <param name="options">The filter options.</param>
        /// <returns>All products.</returns>
        /* it seems all logged in users need to list products in order to create quotes
        [PermissionAuthorize(Type = Permission.ViewProducts)]
        */
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [MustBeLoggedIn]
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ProductSetModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetProducts([FromQuery] ProductQueryOptionsModel options)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(options.Tenant, "list products from a different tenancy");
            if (tenantId != Tenant.MasterTenantId)
            {
                options.Tenant = tenantId.ToString();
            }

            var filter = await options.ToFilters(
                tenantId,
                this.cachingResolver,
                nameof(Product.CreatedTicksSinceEpoch));
            var resources = this.productRepository.GetProductSummariesQuery(tenantId, filter)
                .Paginate(filter)
                .Select(p => new ProductSetModel(p));
            return this.Ok(resources.ToList());
        }

        /// <summary>
        /// Synchronises product assets for the development release.
        /// </summary>
        /// <param name="product">The Id or Alias of the product to deploy.</param>
        /// <param name="tenant">The Id or Alias of the product's tenant.</param>
        /// <returns>The newly created deployment.</returns>
        [HttpPost("{product}/{componentType}/sync")]
        [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 5)]
        [MustHavePermission(Permission.ManageProducts)]
        [ProducesResponseType(typeof(ProductSyncResponseModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> Synchronise(string product, WebFormAppType componentType, [FromQuery] string? tenant = null)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(tenant, "synchronise a product from a different tenancy");
            var tenantModel = await this.CachingResolver.GetTenantOrThrow(tenantId);
            var productModel = await this.CachingResolver.GetProductOrThrow(tenantModel, new GuidOrAlias(product));
            var devRelease = await this.mediator.Send(new SynchroniseProductComponentCommand(tenantId, productModel.Id, componentType));
            var syncdata = new ProductSyncResponseModel
            {
                Id = devRelease.Id,
                QuoteAssetsSynchronisedDateTime = devRelease.QuoteDetails?.LastSynchronisedTimestamp.ToString(),
                ClaimAssetsSynchronisedDateTime = devRelease.ClaimDetails?.LastSynchronisedTimestamp.ToString(),
            };
            return this.Ok(syncdata);
        }

        /// <summary>
        /// Gets the latest synchronisation result.
        /// </summary>
        /// <param name="product">The Id or Alias of the product.</param>
        /// <param name="tenant">The Id or Alias of the tenant the requested products are for.</param>
        /// <returns>All products.</returns>
        [HttpGet("{product}/sync/result")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [MustHavePermission(Permission.ViewProducts)]
        [ProducesResponseType(typeof(ProductSyncResponseModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSyncResult(string product, [FromQuery] string? tenant = null)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(
                tenant,
                "create a development release (or synchronise product assets)",
                "your user account does not have access to the master tenancy");

            Product productModel = await this.cachingResolver.GetProductOrThrow(tenantId, new GuidOrAlias(product));
            var devRelease = await this.mediator.Send(new GetProductDevReleaseWithoutAssetsQuery(productModel.TenantId, productModel.Id));
            var syncdata = new ProductSyncResponseModel
            {
                Id = devRelease.Id,
                QuoteAssetsSynchronisedDateTime = devRelease.QuoteDetails?.LastSynchronisedTimestamp.ToString(),
                ClaimAssetsSynchronisedDateTime = devRelease.ClaimDetails?.LastSynchronisedTimestamp.ToString(),
            };
            return this.Ok(syncdata);
        }

        /// <summary>
        /// Gets a product by aliases.
        /// </summary>
        /// <param name="productAlias">The alias of the product.</param>
        /// <param name="tenant">The ID or Alias of the tenant.</param>
        /// <returns>The product.</returns>
        [HttpGet("by-alias/{productAlias}")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [MustHavePermission(Permission.ViewProducts)]
        [ProducesResponseType(typeof(ProductSetModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByAlias(string productAlias, [FromQuery] string? tenant = null)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(
                 tenant,
                 "get a product from a different tenancy");

            var tenantModel = await this.cachingResolver.GetTenantOrThrow(tenantId);
            var product = await this.cachingResolver.GetProductByAliasOrThrow(tenantId, productAlias);
            var additionalPropertyValues = await this.additionalPropertyValueService.GetAdditionalPropertyValuesByEntityTypeAndEntityId(
                product.TenantId,
                Domain.Enums.AdditionalPropertyEntityType.Product,
                product.Id);
            var vm = new ProductSetModel(product, tenantModel, additionalPropertyValues);
            return this.Ok(vm);
        }

        /// <summary>
        /// Gets a product Id by aliases.
        /// </summary>
        /// <param name="productAlias">The alias of the product.</param>
        /// <param name="tenant">The ID or Alias of the tenant.</param>
        /// <returns>The product.</returns>
        [HttpGet("by-alias/{productAlias}/Id")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [Microsoft.AspNetCore.Authorization.AllowAnonymous]
        [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetIdByAlias(string productAlias, [FromQuery] string? tenant = null)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(
                  tenant,
                  "get a product from a different tenancy");

            var product = await this.cachingResolver.GetProductByAliasOrThrow(tenantId, productAlias);
            return this.Ok(product.Id);
        }

        /// <summary>
        /// Gets a product name by aliases.
        /// </summary>
        /// <param name="productAlias">The alias of the product.</param>
        /// <param name="tenant">The ID or Alias of the tenant.</param>
        /// <returns>The product.</returns>
        [HttpGet("by-alias/{productAlias}/name")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [Microsoft.AspNetCore.Authorization.AllowAnonymous]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetNameByAlias(string productAlias, [FromQuery] string? tenant = null)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(
                  tenant,
                  "get a product from a different tenancy");

            var product = await this.cachingResolver.GetProductByAliasOrThrow(tenantId, productAlias);
            return this.Ok(product.Details.Name);
        }

        /// <summary>
        /// Gets a product by id.
        /// </summary>
        /// <param name="product">The ID or Alias of the product.</param>
        /// <param name="tenant">The ID or Alias of the tenant.</param>
        /// <param name="environment">The deployment environment.</param>
        /// <returns>The product.</returns>
        [HttpGet("{product}")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [MustHaveOneOfPermissions(
            Permission.ViewProducts,
            Permission.ViewQuotes,
            Permission.ViewAllQuotes,
            Permission.ViewAllQuotesFromAllOrganisations)]
        [ProducesResponseType(typeof(ProductSetModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetProductById(string product, [FromQuery] string tenant, [FromQuery] DeploymentEnvironment environment)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(tenant, "get a product from a different tenancy");
            var tenantModel = await this.cachingResolver.GetTenantOrThrow(tenantId);
            var productModel = await this.cachingResolver.GetProductOrThrow(tenantModel.Id, new GuidOrAlias(product));
            var additionalPropertyValues = await this.additionalPropertyValueService.GetAdditionalPropertyValuesByEntityTypeAndEntityId(
                tenantModel.Id,
                AdditionalPropertyEntityType.Product,
                productModel.Id);
            var productSetModel = new ProductSetModel(productModel, tenantModel, additionalPropertyValues);
            var productContext = new ProductContext(tenantModel.Id, productModel.Id, environment);
            if (environment != DeploymentEnvironment.None && this.configurationService.DoesConfigurationExist(productContext, WebFormAppType.Claim))
            {
                productSetModel.HasClaimComponent = true;
            }

            if (environment != DeploymentEnvironment.None && this.configurationService.DoesConfigurationExist(productContext, WebFormAppType.Quote))
            {
                productSetModel.HasQuoteComponent = true;
            }

            return this.Ok(productSetModel);
        }

        /// <summary>
        /// Gets a value indicating whether the product has a claims component deployed to that environment.
        /// We need to allow this for people who don't have ViewProducts permission, because looking up the product
        /// details is used by a number of screens in the portal, e.g
        /// - Detail claim page : to know whether to allow someone to edit/update the claim.
        /// </summary>
        /// <param name="product">The Id or Alias of the product.</param>
        /// <param name="environment">The deployment environment that might have the claim component deployed to it.</param>
        /// <param name="tenant">The Id or Alias of the tenant.</param>
        /// <returns>True or false, depending upon whether the product has as claims component deployed to that environment.</returns>
        [HttpGet("{product}/has-claim")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [ProducesResponseType(typeof(ProductSetModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> DoesProductHaveClaimComponent(string product, [FromQuery][Required] DeploymentEnvironment environment, [FromQuery] string? tenant = null)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(tenant, "get a product from a different tenancy");
            var tenantModel = await this.cachingResolver.GetTenantOrThrow(tenantId);
            var productModel = await this.cachingResolver.GetProductOrThrow(tenantId, new GuidOrAlias(product));
            var model = new ProductSetModel(productModel, tenantModel);
            var productContext = new ProductContext(tenantId, productModel.Id, environment);
            if (environment != DeploymentEnvironment.None && this.configurationService.DoesConfigurationExist(productContext, WebFormAppType.Claim))
            {
                model.HasClaimComponent = true;
            }

            return this.Ok(model.HasClaimComponent);
        }

        /// <summary>
        /// Gets a value indicating whether the product has a quote component deployed to that environment.
        /// We need to allow this for people who don't have ViewProducts permission, to see the product details of their quote.
        /// </summary>
        /// <param name="product">The Id or Alias of the product.</param>
        /// <param name="environment">The deployment environment that might have the quote component deployed to it.</param>
        /// <param name="tenant">The Id or Alias of the tenant.</param>
        /// <returns>True or false, depending upon whether the product has as quote component deployed to that environment.</returns>
        [HttpGet("{product}/has-quote")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [ProducesResponseType(typeof(ProductSetModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> DoesProductHaveQuoteComponent(string product, [FromQuery][Required] DeploymentEnvironment environment, [FromQuery] string? tenant = null)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(tenant, "get a product from a different tenancy");
            var tenantModel = await this.cachingResolver.GetTenantOrThrow(tenantId);
            var productModel = await this.cachingResolver.GetProductOrThrow(tenantModel, new GuidOrAlias(product));
            var model = new ProductSetModel(productModel, tenantModel);
            var productContext = new ProductContext(productModel.TenantId, productModel.Id, environment);
            if (environment != DeploymentEnvironment.None && this.configurationService.DoesConfigurationExist(productContext, WebFormAppType.Quote))
            {
                model.HasQuoteComponent = true;
            }

            return this.Ok(model.HasQuoteComponent);
        }

        /// <summary>
        /// Gets a list of Development source files for a specific product.
        /// </summary>
        /// <param name="product">The Id of the product.</param>
        /// <param name="tenant">The ID or Alias of the product's tenant.</param>
        /// <returns>Ok.</returns>
        [HttpGet("{product}/file")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [MustHavePermission(Permission.ViewProducts)]
        [ProducesResponseType(typeof(IEnumerable<ConfigurationFileModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSourceFilesForProduct(string product, [FromQuery] string? tenant = null)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(
                  tenant,
                  "list source files for a product",
                  "your user account does not have access to the master tenancy");
            Product productModel = await this.cachingResolver.GetProductOrThrow(tenantId, new GuidOrAlias(product));
            var query = GetProductReleaseSourceFilesQuery.CreateForDevRelease(tenantId, productModel.Id);
            var files = await this.mediator.Send(query);
            var models = files.Select(file => new ConfigurationFileModel(file));
            return this.Ok(models);
        }

        /// <summary>
        /// Gets the development release file with the given path.
        /// Does not use caching, so lookups are slow but accurate.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="formType">The type of form configuration to load.</param>
        /// <returns>The product.</returns>
        [HttpGet]
        [Route("{product}/{formType}/file/{*filePath}")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [MustHavePermission(Permission.ViewReleases)]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetReleaseFileByFilePath(
            string product, FormType formType, string filePath, [FromQuery] string tenant)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(tenant, "get a release file by path");
            var productModel = await this.cachingResolver.GetProductOrThrow(tenantId, new GuidOrAlias(product));
            var productReleaseId = await this.mediator.Send(new GetDefaultProductReleaseIdOrThrowQuery(
                tenantId,
                productModel.Id,
                DeploymentEnvironment.Development));
            var webFormAppType = formType.ToWebFormAppType();
            var fileContent = await this.mediator.Send(new GetReleaseFileWithoutCachingQuery(
                tenantId,
                productReleaseId,
                webFormAppType,
                filePath));

            string contentType = fileContent.ContentType ?? MimeTypeHelper.DetectMimeType(filePath);
            var fileContentResult = new FileContentResult(fileContent.Content, contentType);
            return fileContentResult;
        }

        /// <summary>
        /// Create a new product.
        /// </summary>
        /// <param name="vm">View model with product details.</param>
        /// <returns>The newly created product.</returns>
        [HttpPost]
        [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 5)]
        [ValidateModel]
        [MustHavePermission(Permission.ManageProducts)]
        [ProducesResponseType(typeof(ProductSetModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateProduct([FromBody] ProductCreateRequestModel vm)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(
                 vm.Tenant,
                 "create a new product",
                 "your user account does not have access to the master tenancy");

            Tenant tenant = await this.cachingResolver.GetTenantOrThrow(tenantId);
            var product = await this.productService.CreateOrUpdateProduct(
                tenant.Id, vm.Alias, vm.Name, false, false);
            var domainProperties = vm.Properties.ToDomainAdditionalProperties();
            await this.additionalPropertyValueService.UpsertSetAdditionalPropertyValuesForNonAggregateEntity(
                tenant.Id,
                product.Id,
                domainProperties);
            return this.Ok(new ProductSetModel(product, tenant));
        }

        /// <summary>
        /// Update an existing product.
        /// </summary>
        /// <param name="product">The Id or Alias of the product to update.</param>
        /// <param name="vm">New product details.</param>
        /// <returns>The updated product.</returns>
        [HttpPut("{product}")]
        [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 5)]
        [ValidateModel]
        [MustHavePermission(Permission.ManageProducts)]
        [ProducesResponseType(typeof(ProductSetModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateProduct(string product, [FromBody] ProductUpdateRequestModel vm)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(
                 vm.Tenant,
                 "update a product",
                 "your user account does not have access to the master tenancy");

            Product productModel = await this.cachingResolver.GetProductOrThrow(tenantId, new GuidOrAlias(product));

            QuoteExpirySettings? productExpirySettings = null;

            if (vm.QuoteExpirySettings != null)
            {
                if (vm.QuoteExpirySettings.Enabled
                    && (vm.QuoteExpirySettings.ExpiryDays > 365 || vm.QuoteExpirySettings.ExpiryDays < 1))
                {
                    return Errors.General
                        .Forbidden("update a product", "the expiry days should be from 1 to 365.")
                        .ToProblemJsonResult();
                }

                productExpirySettings = new QuoteExpirySettings(
                    vm.QuoteExpirySettings.ExpiryDays, vm.QuoteExpirySettings.Enabled);
            }

            productModel = await this.mediator.Send(new UpdateProductCommand(
                tenantId,
                productModel.Id,
                vm.Name,
                vm.Alias,
                vm.Disabled,
                vm.Deleted,
                productExpirySettings,
                vm.QuoteExpirySettings != null
                    ? vm.QuoteExpirySettings.UpdateType
                    : ProductQuoteExpirySettingUpdateType.UpdateNone));

            var tenant = await this.cachingResolver.GetTenantOrThrow(tenantId);
            var domainProperties = vm.Properties.ToDomainAdditionalProperties();
            await this.additionalPropertyValueService.UpsertSetAdditionalPropertyValuesForNonAggregateEntity(
                tenant.Id,
                productModel.Id,
                domainProperties);
            var resultVm = new ProductSetModel(productModel, tenant);
            return this.Ok(resultVm);
        }

        /// <summary>
        /// Gets releases for a product.
        /// </summary>
        /// <param name="product">The Id or Alias of the product to retrieve releases for.</param>
        /// <param name="options">The filter options.</param>
        /// <returns>All releases related to products.</returns>
        [HttpGet("{product}/release")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [ValidateModel]
        [MustHavePermission(Permission.ViewReleases)]
        [ProducesResponseType(typeof(IEnumerable<ReleaseModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetReleasesForProduct([Required] string product, [FromQuery] QueryOptionsModel options)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(
                 options.Tenant,
                 "get the release for a product",
                 "your user account doesn't have access to the master tenancy");

            Product productModel = await this.cachingResolver.GetProductOrThrow(tenantId, new GuidOrAlias(product));
            IEnumerable<Release> releases = this.releaseRepository.GetReleasesForProduct(
                productModel.TenantId,
                productModel.Id,
                await options.ToFilters(tenantId, this.cachingResolver, nameof(Release.CreatedTicksSinceEpoch)));
            IEnumerable<ReleaseModel> vms = releases.Select(r => new ReleaseModel(r, productModel));
            return this.Ok(vms);
        }

        /// <summary>
        /// Gets the product release settings.
        /// </summary>
        /// <param name="tenant">The tenant.</param>
        /// <param name="product">The portal.</param>
        /// <returns>The summary of the portal.</returns>
        [HttpGet]
        [Route("{product}/release-settings")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 80)]
        [MustHavePermission(Permission.ManageProducts)]
        [ProducesResponseType(typeof(ProductReleaseEntitySettings), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetProductReleaseSettings([FromQuery] string tenant, string product)
        {
            var tenantModel = await this.cachingResolver.GetTenantOrThrow(new GuidOrAlias(tenant));
            var productModel = await this.cachingResolver.GetProductOrThrow(tenantModel.Id, new GuidOrAlias(product));
            var query = new GetProductReleaseSettingsQuery(tenantModel.Id, productModel.Id);
            var productReleaseSettings = await this.mediator.Send(query);
            return this.Ok(productReleaseSettings);
        }

        /// <summary>
        /// Update the product release settings for a quote type
        /// if use the default product release or affected policy period.
        /// </summary>
        /// <param name="product">The product.</param>
        /// <param name="productReleaseModel">The product release model.</param>
        [HttpPut]
        [Route("{product}/release-settings")]
        [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 5)]
        [MustHavePermission(Permission.ManageProducts)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateProductReleaseSettings(string product, [FromBody] ProductReleaseModel productReleaseModel)
        {
            var tenantModel = await this.cachingResolver.GetTenantOrThrow(new GuidOrAlias(productReleaseModel.Tenant));
            var productModel = await this.cachingResolver.GetProductOrThrow(tenantModel.Id, new GuidOrAlias(product));
            var query = new UpdateProductReleaseSettingsCommand(
                tenantModel.Id,
                productModel.Id,
                productReleaseModel.DoesAdjustmentUseDefaultProductRelease,
                productReleaseModel.DoesCancellationUseDefaultProductRelease);
            await this.mediator.Send(query);
            return this.Ok();
        }
    }
}
