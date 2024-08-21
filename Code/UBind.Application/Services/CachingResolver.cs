// <copyright file="CachingResolver.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Queries.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using StackExchange.Profiling;
    using UBind.Application.Commands.Sentry;
    using UBind.Application.Queries.Organisation;
    using UBind.Application.Queries.Portal;
    using UBind.Application.Queries.Product;
    using UBind.Application.Queries.ProductRelease;
    using UBind.Application.Queries.Tenant;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Portal;
    using UBind.Domain.Repositories;

    /// <summary>
    /// This service is needed to resolve tenant or product or portal from ids.
    /// </summary>
    public class CachingResolver : ICachingResolver
    {
        /// <summary>
        /// how many minutes the caching will happen.
        /// </summary>
        private static int defaultCacheDurationMinutes = 5;
        private ICqrsMediator mediator;
        private ITenantRepository tenantRepository;
        private IProductRepository productRepository;
        private IFeatureSettingRepository featureSettingRepository;
        private IProductFeatureSettingRepository productFeatureSettingRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="CachingResolver"/> class.
        /// </summary>
        public CachingResolver(
            ICqrsMediator mediator,
            ITenantRepository tenantRepository,
            IProductRepository productRepository,
            IFeatureSettingRepository featureSettingRepository,
            IProductFeatureSettingRepository productFeatureSettingRepository)
        {
            this.mediator = mediator;
            this.tenantRepository = tenantRepository;
            this.productRepository = productRepository;
            this.featureSettingRepository = featureSettingRepository;
            this.productFeatureSettingRepository = productFeatureSettingRepository;
        }

        private DateTimeOffset DefaultCacheExpiration => DateTimeOffset.Now.AddMinutes(defaultCacheDurationMinutes);

        private DateTimeOffset ReleaseNumberCacheExpiration => DateTimeOffset.Now.AddHours(1);

        /// <inheritdoc/>
        public void RemoveCachedTenants(Guid tenantId, List<string> tenantAliases)
        {
            MemoryCachingHelper.Remove(this.GetTenantCacheKey(tenantId));
            foreach (var tenantAlias in tenantAliases.Distinct())
            {
                MemoryCachingHelper.Remove(this.GetTenantCacheKey(tenantAlias));
            }
        }

        /// <inheritdoc/>
        public void RemoveCachedProducts(Guid tenantId, Guid productId, List<string> productAliases)
        {
            MemoryCachingHelper.Remove(this.GetProductCacheKey(tenantId, productId));
            foreach (var productAlias in productAliases.Distinct())
            {
                MemoryCachingHelper.Remove(this.GetProductCacheKey(tenantId, productAlias));
            }
        }

        /// <inheritdoc/>
        public void RemoveCachedPortals(Guid tenantId, Guid portalId, List<string> portalAliases)
        {
            MemoryCachingHelper.Remove(this.GetPortalCacheKey(tenantId, portalId));
            foreach (var portalAlias in portalAliases.Distinct())
            {
                MemoryCachingHelper.Remove(this.GetPortalCacheKey(tenantId, portalAlias));
            }
        }

        public void CacheOrganisation(Guid tenantId, Guid organisationId, OrganisationReadModel organisation)
        {
            var cacheKey = this.GetOrganisationCacheKey(tenantId, organisationId);
            MemoryCachingHelper.Upsert(cacheKey, organisation, this.DefaultCacheExpiration);
        }

        public void CacheTenant(Guid tenantId, Tenant tenant)
        {
            var cacheKey = this.GetTenantCacheKey(tenantId);
            MemoryCachingHelper.Upsert(cacheKey, tenant, this.DefaultCacheExpiration);
        }

        /// <inheritdoc/>
        public void RemoveCachedOrganisations(Guid tenantId, Guid organisationId, List<string> organisationAliases)
        {
            MemoryCachingHelper.Remove(this.GetOrganisationCacheKey(tenantId, organisationId));
            foreach (var organisationAlias in organisationAliases.Distinct())
            {
                MemoryCachingHelper.Remove(this.GetOrganisationCacheKey(tenantId, organisationAlias));
            }
        }

        public void RemoveCachedFeatureSettings(Guid tenantId)
        {
            using (MiniProfiler.Current.Step("CachingResolver.RemoveCachedFeatureSettings"))
            {
                MemoryCachingHelper.Remove(this.GetFeatureSettingCacheKey(tenantId));
            }
        }

        /// <inheritdoc/>
        public void RemoveCachedProductSettings(Guid tenantId, Guid productId)
        {
            using (MiniProfiler.Current.Step("CachingResolver.RemoveCachedProductSettings"))
            {
                MemoryCachingHelper.Remove(this.GetProductSettingCacheKey(tenantId, productId));
                foreach (DeploymentEnvironment env in Enum.GetValues(typeof(DeploymentEnvironment)))
                {
                    MemoryCachingHelper.Remove(this.GetProductSettingCacheKey(tenantId, env));
                }
            }
        }

        /// <inheritdoc/>
        public async Task<Tenant> GetTenantByAliasOrThrow(string tenantAlias)
        {
            var tenant = await this.GetTenantByAlias(tenantAlias);
            tenant = EntityHelper.ThrowIfNotFound(tenant, tenantAlias, "tenant");
            return tenant;
        }

        /// <inheritdoc/>
        public async Task<Tenant?> GetTenantByAliasOrNull(string tenantAlias)
        {
            return await this.GetTenantByAlias(tenantAlias);
        }

        /// <inheritdoc/>
        public async Task<Tenant> GetTenantOrThrow(GuidOrAlias tenantIdOrAlias)
        {
            var tenant = await this.GetTenant(tenantIdOrAlias);
            tenant = EntityHelper.ThrowIfNotFound(tenant, tenantIdOrAlias, "tenant");
            return tenant;
        }

        /// <inheritdoc/>
        public async Task<Tenant?> GetTenantOrNull(GuidOrAlias tenantIdOrAlias)
        {
            return await this.GetTenant(tenantIdOrAlias);
        }

        public async Task<Guid?> GetTenantIdOrNull(GuidOrAlias tenantIdOrAlias)
        {
            var tenant = await this.GetTenantOrNull(tenantIdOrAlias);
            return tenant?.Id;
        }

        public async Task<Guid> GetTenantIdOrThrow(GuidOrAlias tenantIdOrAlias)
        {
            var tenant = await this.GetTenantOrThrow(tenantIdOrAlias);
            return tenant.Id;
        }

        /// <inheritdoc/>
        public async Task<Tenant> GetTenantOrThrow(Guid? tenantId)
        {
            Guid id = tenantId.ThrowIfNullOrEmpty("tenant id");
            var tenant = await this.GetTenantAsync(id);
            tenant = EntityHelper.ThrowIfNotFound(tenant, id, "tenant");
            return tenant;
        }

        /// <inheritdoc/>
        public async Task<Tenant?> GetTenantOrNull(Guid? tenantId)
        {
            Guid id = tenantId.ThrowIfNullOrEmpty("tenant id");
            return await this.GetTenantAsync(id);
        }

        public async Task<string> GetTenantAliasOrThrowAsync(Guid tenantId)
        {
            var tenant = await this.GetTenantOrThrow(tenantId);
            return tenant.Details.Alias;
        }

        /// <inheritdoc/>
        public string GetTenantAliasOrThrow(Guid tenantId)
        {
            var tenant = this.GetTenant(tenantId);
            tenant = EntityHelper.ThrowIfNotFound(tenant, tenantId, "tenant");
            return tenant.Details.Alias;
        }

        /// <inheritdoc/>
        public async Task<Product> GetProductByAliasOrThrow(Guid tenantId, string productAlias)
        {
            var product = await this.GetProductByAlias(tenantId, productAlias);
            product = EntityHelper.ThrowIfNotFound(product, productAlias, "product");
            return product;
        }

        /// <inheritdoc/>
        public async Task<Product?> GetProductByAliasOrNull(Guid tenantId, string productAlias)
        {
            return await this.GetProductByAlias(tenantId, productAlias);
        }

        /// <inheritdoc/>
        public async Task<Product> GetProductByAliasOThrow(string tenantAlias, string productAlias)
        {
            var product = await this.GetProductByAlias(tenantAlias, productAlias);
            product = EntityHelper.ThrowIfNotFound(product, productAlias, "product");
            return product;
        }

        /// <inheritdoc/>
        public async Task<Product?> GetProductByAliasOrNull(string tenantAlias, string productAlias)
        {
            return await this.GetProductByAlias(tenantAlias, productAlias);
        }

        /// <inheritdoc/>
        public async Task<Product> GetProductOrThrow(Guid tenantId, GuidOrAlias productIdOrAlias)
        {
            var product = await this.GetProduct(tenantId, productIdOrAlias);
            product = EntityHelper.ThrowIfNotFound(product, productIdOrAlias, "product");
            return product;
        }

        /// <inheritdoc/>
        public async Task<Product?> GetProductOrNull(Guid tenantId, GuidOrAlias productIdOrAlias)
        {
            return await this.GetProduct(tenantId, productIdOrAlias);
        }

        public async Task<Guid?> GetProductIdOrNull(GuidOrAlias tenantIdOrAlias, GuidOrAlias productIdOrAlias)
        {
            var product = await this.GetProductOrNull(tenantIdOrAlias, productIdOrAlias);
            return product?.Id;
        }

        public async Task<Guid?> GetProductIdOrNull(Guid tenantId, GuidOrAlias productIdOrAlias)
        {
            var product = await this.GetProductOrNull(tenantId, productIdOrAlias);
            return product?.Id;
        }

        public async Task<Guid> GetProductIdOrThrow(GuidOrAlias tenantIdOrAlias, GuidOrAlias productIdOrAlias)
        {
            var product = await this.GetProductOrThrow(tenantIdOrAlias, productIdOrAlias);
            return product.Id;
        }

        public async Task<Guid> GetProductIdOrThrow(Guid tenantId, GuidOrAlias productIdOrAlias)
        {
            var product = await this.GetProductOrThrow(tenantId, productIdOrAlias);
            return product.Id;
        }

        /// <inheritdoc/>
        public async Task<Product> GetProductOrThrow(GuidOrAlias tenantIdOrAlias, GuidOrAlias productIdOrAlias)
        {
            var product = await this.GetProduct(tenantIdOrAlias, productIdOrAlias);
            product = EntityHelper.ThrowIfNotFound(product, productIdOrAlias, "product");
            return product;
        }

        public async Task<Product> GetProductOrThrow(Tenant tenantModel, GuidOrAlias productIdOrAlias)
        {
            var product = await this.GetProduct(tenantModel.Id, productIdOrAlias);
            product = EntityHelper.ThrowIfNotFound(product, productIdOrAlias, "product");
            return product;
        }

        /// <inheritdoc/>
        public async Task<Product?> GetProductOrNull(GuidOrAlias tenantIdOrAlias, GuidOrAlias productIdOrAlias)
        {
            return await this.GetProduct(tenantIdOrAlias, productIdOrAlias);
        }

        /// <inheritdoc/>
        public async Task<Product> GetProductOrThrow(Guid tenantId, Guid productId)
        {
            var product = await this.GetProduct(tenantId, productId);
            product = EntityHelper.ThrowIfNotFound(product, productId, "product");
            return product;
        }

        /// <inheritdoc/>
        public async Task<Product?> GetProductOrNull(Guid tenantId, Guid productId)
        {
            return await this.GetProduct(tenantId, productId);
        }

        /// <inheritdoc/>
        public string GetProductAliasOrThrow(Guid tenantId, Guid productId)
        {
            tenantId.ThrowIfEmpty("tenant Id");
            productId.ThrowIfEmpty("product Id");

            var tenant = this.GetTenantOrThrow(tenantId);
            Func<Product?> productCallback = () =>
            {
                return this.productRepository.GetProductById(tenantId, productId);
            };
            var product = MemoryCachingHelper.AddOrGet(
                 this.GetProductCacheKey(tenantId, productId),
                 productCallback,
                 this.DefaultCacheExpiration);
            product = EntityHelper.ThrowIfNotFound(product, productId, "product");

            if (product.Details == null)
            {
                product = this.productRepository.GetProductById(tenantId, productId);
                product = EntityHelper.ThrowIfNotFound(product, productId, "product");

                MemoryCachingHelper.Upsert(
                    this.GetProductCacheKey(tenantId, productId),
                    product,
                    this.DefaultCacheExpiration);

                // log in sentry but dont throw
                var exception = new ErrorException(
                    Errors.General.Unexpected(
                        $"Details for product returned by MemoryCachingHelper.AddOrGet is null. Product Id: {productId}"));
                this.mediator.Send(new CaptureSentryExceptionCommand(exception))
                      .ContinueWith(t => { throw t.Exception; }, TaskContinuationOptions.OnlyOnFaulted);
            }

            return product.Details.Alias;
        }

        /// <inheritdoc/>
        public async Task<string> GetProductAliasOrThrowAsync(Guid tenantId, Guid productId)
        {
            var product = await this.GetProduct(tenantId, productId);
            product = EntityHelper.ThrowIfNotFound(product, productId, "product");
            return product.Details.Alias;
        }

        /// <inheritdoc/>
        public async Task<OrganisationReadModel> GetOrganisationByAliasOrThrow(Guid tenantId, string organisationAlias)
        {
            tenantId.ThrowIfEmpty("tenant Id");
            this.ThrowErrorIfAliasIsNullOrEmpty(organisationAlias, "organisation alias");

            var organisation = await this.GetOrganisationCacheByAlias(tenantId, organisationAlias);
            organisation = EntityHelper.ThrowIfNotFound(organisation, organisationAlias, "organisation");

            // Seed the cache by ID as well
            MemoryCachingHelper.Upsert(this.GetOrganisationCacheKey(tenantId, organisation.Id), organisation, this.DefaultCacheExpiration);
            return organisation;
        }

        /// <inheritdoc/>
        public async Task<OrganisationReadModel?> GetOrganisationByAliasOrNull(Guid tenantId, string organisationAlias)
        {
            return await this.GetOrganisationByAlias(tenantId, organisationAlias);
        }

        /// <inheritdoc/>
        public async Task<OrganisationReadModel> GetOrganisationOrThrow(GuidOrAlias tenantIdOrAlias, GuidOrAlias organisationIdOrAlias)
        {
            var tenant = tenantIdOrAlias.Guid.IsNullOrEmpty() || !tenantIdOrAlias.Guid.HasValue
                ? await this.GetTenantByAlias(tenantIdOrAlias.Alias)
                : await this.GetTenantAsync(tenantIdOrAlias.Guid.Value);
            tenant = EntityHelper.ThrowIfNotFound(tenant, tenantIdOrAlias, "tenant");

            var organisation = organisationIdOrAlias.Guid.IsNullOrEmpty() || !organisationIdOrAlias.Guid.HasValue
                ? await this.GetOrganisationByAliasOrThrow(tenant.Id, organisationIdOrAlias.Alias)
                : await this.GetOrganisation(tenant.Id, organisationIdOrAlias.Guid.Value);
            organisation = EntityHelper.ThrowIfNotFound(organisation, organisationIdOrAlias, "organisation");
            return organisation;
        }

        public async Task<OrganisationReadModel> GetOrganisationOrThrow(Tenant tenantModel, GuidOrAlias organisationIdOrAlias)
        {
            return await this.GetOrganisationOrThrow(tenantModel.Id, organisationIdOrAlias);
        }

        /// <inheritdoc/>
        public async Task<OrganisationReadModel?> GetOrganisationOrNull(GuidOrAlias tenantIdOrAlias, GuidOrAlias organisationIdOrAlias)
        {
            return await this.GetOrganisation(tenantIdOrAlias, organisationIdOrAlias);
        }

        /// <inheritdoc/>
        public async Task<OrganisationReadModel> GetOrganisationOrThrow(Guid tenantId, GuidOrAlias organisationIdOrAlias)
        {
            tenantId.ThrowIfEmpty("tenant Id");
            this.ThrowErrorIfGuidOrAliasIsEmpty(organisationIdOrAlias, "organisation ID or alias");

            var tenant = await this.GetTenantAsync(tenantId);
            tenant = EntityHelper.ThrowIfNotFound(tenant, tenantId, "tenant");
            var organisation = organisationIdOrAlias.Guid.IsNullOrEmpty() || !organisationIdOrAlias.Guid.HasValue
                ? await this.GetOrganisationByAliasOrThrow(tenant.Id, organisationIdOrAlias.Alias)
                : await this.GetOrganisation(tenant.Id, organisationIdOrAlias.Guid.Value);
            organisation = EntityHelper.ThrowIfNotFound(organisation, organisationIdOrAlias, "organisation");
            return organisation;
        }

        /// <inheritdoc/>
        public async Task<OrganisationReadModel?> GetOrganisationOrNull(Guid tenantId, GuidOrAlias organisationIdOrAlias)
        {
            return await this.GetOrganisation(tenantId, organisationIdOrAlias);
        }

        /// <inheritdoc/>
        public async Task<OrganisationReadModel> GetOrganisationOrThrow(Guid tenantId, Guid organisationId)
        {
            var organisation = await this.GetOrganisation(tenantId, organisationId);
            organisation = EntityHelper.ThrowIfNotFound(organisation, organisationId, "organisation");
            return organisation;
        }

        /// <inheritdoc/>
        public async Task<OrganisationReadModel?> GetOrganisationOrNull(Guid tenantId, Guid organisationId)
        {
            return await this.GetOrganisation(tenantId, organisationId);
        }

        /// <inheritdoc/>
        public async Task<PortalReadModel> GetPortalOrThrow(Guid tenantId, Guid portalId)
        {
            portalId.ThrowIfEmpty("portal Id");

            var portal = await this.GetPortalCacheByGuid(tenantId, portalId);
            portal = EntityHelper.ThrowIfNotFound(portal, portalId, "portal");

            return portal;
        }

        /// <inheritdoc/>
        public async Task<PortalReadModel?> GetPortalOrNull(Guid tenantId, Guid portalId)
        {
            return await this.GetPortal(tenantId, portalId);
        }

        /// <inheritdoc/>
        public async Task<PortalReadModel> GetPortalOrThrow(Guid tenantId, GuidOrAlias portalIdOrAlias)
        {
            tenantId.ThrowIfEmpty("tenant Id");
            this.ThrowErrorIfGuidOrAliasIsEmpty(portalIdOrAlias, "portal ID or alias");

            var tenant = await this.GetTenantAsync(tenantId);
            tenant = EntityHelper.ThrowIfNotFound(tenant, tenantId, "tenant");
            if (portalIdOrAlias.Guid == null)
            {
                var portalAlias = portalIdOrAlias.Alias;
                var portal = await this.GetPortalCacheByAlias(tenant.Id, portalAlias);
                portal = EntityHelper.ThrowIfNotFound(portal, portalAlias, "portal");

                // Seed the cache by ID as well
                MemoryCachingHelper.Upsert(this.GetPortalCacheKey(tenant.Id, portal.Id), portal, this.DefaultCacheExpiration);
                return portal;
            }

            return await this.GetPortalOrThrow(tenant.Id, portalIdOrAlias.Guid.Value);
        }

        public async Task<PortalReadModel> GetPortalOrThrow(Tenant tenantModel, GuidOrAlias portalIdOrAlias)
        {
            return await this.GetPortalOrThrow(tenantModel.Id, portalIdOrAlias);
        }

        /// <inheritdoc/>
        public async Task<PortalReadModel?> GetPortalOrNull(Guid tenantId, GuidOrAlias portalIdOrAlias)
        {
            return await this.GetPortal(tenantId, portalIdOrAlias);
        }

        public async Task<PortalReadModel?> GetPortalOrNull(Tenant tenant, GuidOrAlias portalIdOrAlias)
        {
            return await this.GetPortal(tenant.Id, portalIdOrAlias);
        }

        public IEnumerable<Setting> GetSettingsOrThrow(Guid tenantId)
        {
            using (MiniProfiler.Current.Step($"{nameof(CachingResolver)}.{nameof(this.GetSettingsOrThrow)}"))
            {
                var tenant = this.GetTenant(tenantId);
                tenant = EntityHelper.ThrowIfNotFound(tenant, tenantId, "tenant");
                var settings = this.GetSettings(tenantId);
                if (settings == null)
                {
                    throw new ErrorException(Errors.General.NotFound("settings", tenantId, "tenant ID"));
                }
                return settings;
            }
        }

        public IEnumerable<Setting>? GetSettingsOrNull(Guid tenantId)
        {
            using (MiniProfiler.Current.Step($"{nameof(CachingResolver)}.{nameof(this.GetSettingsOrNull)}"))
            {
                if (this.IsTenantIdEmptyOrInvalid(tenantId))
                {
                    return null;
                }

                return this.GetSettings(tenantId);
            }
        }

        public IEnumerable<Setting> GetActiveSettingsOrThrow(Guid tenantId)
        {
            using (MiniProfiler.Current.Step($"{nameof(CachingResolver)}.{nameof(this.GetActiveSettingsOrThrow)}"))
            {
                var tenant = this.GetTenant(tenantId);
                EntityHelper.ThrowIfNotFound(tenant, tenantId, "tenant");
                var settings = this.GetActiveSettings(tenantId);
                if (settings == null)
                {
                    throw new ErrorException(Errors.General.NotFound("settings", tenantId, "tenant ID"));
                }
                return settings;
            }
        }

        public IEnumerable<Setting>? GetActiveSettingsOrNull(Guid tenantId)
        {
            using (MiniProfiler.Current.Step($"{nameof(CachingResolver)}.{nameof(this.GetActiveSettingsOrNull)}"))
            {
                if (this.IsTenantIdEmptyOrInvalid(tenantId))
                {
                    return null;
                }

                return this.GetActiveSettings(tenantId);
            }
        }

        public List<ProductFeatureSetting>? GetDeployedProductSettingsOrNull(Guid tenantId, DeploymentEnvironment environment)
        {
            using (MiniProfiler.Current.Step($"{nameof(CachingResolver)}.{nameof(this.GetDeployedProductSettingsOrNull)}"))
            {
                return this.GetDeployedProductSettings(tenantId, environment);
            }
        }

        public List<ProductFeatureSetting> GetDeployedProductSettingsOrThrow(Guid tenantId, DeploymentEnvironment environment)
        {
            using (MiniProfiler.Current.Step($"{nameof(CachingResolver)}.{nameof(this.GetDeployedProductSettingsOrThrow)}"))
            {
                tenantId.ThrowIfEmpty("tenant Id");
                var settings = this.GetDeployedProductSettingsCache(tenantId, environment);
                if (settings == null)
                {
                    throw new ErrorException(Errors.General.NotFound("deployed product feature settings", tenantId, "tenant ID"));
                }
                return settings;
            }
        }

        public ProductFeatureSetting? GetProductSettingOrNull(Guid tenantId, Guid productId)
        {
            using (MiniProfiler.Current.Step($"{nameof(CachingResolver)}.{nameof(this.GetProductSettingOrNull)}"))
            {
                return this.GetProductSetting(tenantId, productId);
            }
        }

        public ProductFeatureSetting GetProductSettingOrThrow(Guid tenantId, Guid productId)
        {
            using (MiniProfiler.Current.Step($"{nameof(CachingResolver)}.{nameof(this.GetProductSettingOrThrow)}"))
            {
                var tenant = this.GetTenant(tenantId);
                EntityHelper.ThrowIfNotFound(tenant, tenantId, "tenant");

                var setting = this.GetProductSetting(tenantId, productId);
                setting = EntityHelper.ThrowIfNotFound(setting, productId, "product feature setting");
                return setting;
            }
        }

        public async Task<List<Product>> GetActiveProducts()
        {
            using (MiniProfiler.Current.Step($"{nameof(CachingResolver)}.{nameof(this.GetActiveProducts)}"))
            {
                var activeProducts = await MemoryCachingHelper.AddOrGetAsync(
                    this.GetActiveProductsCacheKey(),
                    async () =>
                    {
                        return await this.mediator.Send(new GetActiveProductsQuery());
                    },
                    this.DefaultCacheExpiration);

                return activeProducts ?? new List<Product>();
            }
        }

        public async Task<Guid> GetProductReleaseIdOrThrow(Guid tenantId, Guid productId, GuidOrAlias productReleaseIdOrNumber)
        {
            this.ThrowErrorIfGuidOrAliasIsEmpty(productReleaseIdOrNumber, "product release ID or number");

            if (productReleaseIdOrNumber.Guid.HasValue)
            {
                return productReleaseIdOrNumber.Guid.Value;
            }

            var productReleaseNumber = productReleaseIdOrNumber.Alias;
            var productReleaseId = await MemoryCachingHelper.AddOrGetAsync(
                this.GetProductReleaseNumberCacheKey(tenantId, productId, productReleaseNumber),
                async () =>
                {
                    return await this.mediator.Send(new GetProductReleaseIdByReleaseNumberQuery(
                        tenantId,
                        productId,
                        productReleaseNumber));
                },
                this.ReleaseNumberCacheExpiration);
            return productReleaseId;
        }

        private async Task<Tenant?> GetTenantByAlias(string tenantAlias)
        {
            this.ThrowErrorIfAliasIsNullOrEmpty(tenantAlias, "tenant alias");

            var tenant = await MemoryCachingHelper.AddOrGetAsync(
                   this.GetTenantCacheKey(tenantAlias),
                   async () =>
                   {
                       return await this.mediator.Send(new GetTenantByAliasQuery(tenantAlias, true));
                   },
                   this.DefaultCacheExpiration);

            if (tenant != null)
            {
                // Seed the cache by ID
                MemoryCachingHelper.Upsert(this.GetTenantCacheKey(tenant.Id), tenant, this.DefaultCacheExpiration);
            }

            return tenant;
        }

        private async Task<Product?> GetProductByAlias(Guid tenantId, string productAlias)
        {
            tenantId.ThrowIfEmpty("tenant Id");
            this.ThrowErrorIfAliasIsNullOrEmpty(productAlias, "product alias");

            var product = await MemoryCachingHelper.AddOrGetAsync(
                    this.GetProductCacheKey(tenantId, productAlias),
                    async () =>
                    {
                        return await this.mediator.Send(new GetProductByAliasQuery(tenantId, productAlias));
                    },
                    this.DefaultCacheExpiration);

            if (product != null)
            {
                // Seed the cache by ID
                MemoryCachingHelper.Upsert(this.GetProductCacheKey(tenantId, product.Id), product, this.DefaultCacheExpiration);
            }

            return product;
        }

        private async Task<Tenant?> GetTenant(GuidOrAlias tenantIdOrAlias)
        {
            this.ThrowErrorIfGuidOrAliasIsEmpty(tenantIdOrAlias, "tenant ID or alias");

            if (!tenantIdOrAlias.Guid.IsNullOrEmpty() && tenantIdOrAlias.Guid.HasValue)
            {
                return await this.GetTenantAsync(tenantIdOrAlias.Guid.Value);
            }
            return await this.GetTenantByAlias(tenantIdOrAlias.Alias);
        }

        private Tenant? GetTenant(Guid tenantId)
        {
            tenantId.ThrowIfEmpty("tenant Id");
            var tenant = MemoryCachingHelper.AddOrGet(
                this.GetTenantCacheKey(tenantId),
                () => this.tenantRepository.GetTenantById(tenantId),
                this.DefaultCacheExpiration);

            if (tenant != null && tenant.Details == null)
            {
                tenant = this.tenantRepository.GetTenantById(tenantId);
                if (tenant == null)
                {
                    return null;
                }

                MemoryCachingHelper.Upsert(
                    this.GetTenantCacheKey(tenantId),
                    tenant,
                    this.DefaultCacheExpiration);

                // log in sentry but dont throw
                var exception = new ErrorException(
                    Errors.General.Unexpected(
                        $"Details for tenant returned by MemoryCachingHelper.AddOrGet is null. Tenant Id: {tenantId}"));
                this.mediator.Send(new CaptureSentryExceptionCommand(exception))
                    .ContinueWith(t => { throw t.Exception; }, TaskContinuationOptions.OnlyOnFaulted);
            }

            return tenant;
        }

        private async Task<Tenant?> GetTenantAsync(Guid tenantId)
        {
            var tenant = await MemoryCachingHelper.AddOrGetAsync(
                this.GetTenantCacheKey(tenantId),
                async () =>
                {
                    return await this.mediator.Send(new GetTenantByIdQuery(tenantId));
                },
                this.DefaultCacheExpiration);

            if (tenant != null && tenant.Details == null)
            {
                tenant = await this.mediator.Send(new GetTenantByIdQuery(tenantId));
                MemoryCachingHelper.Upsert(
                    this.GetTenantCacheKey(tenantId),
                    tenant,
                    this.DefaultCacheExpiration);

                // log in sentry but dont throw
                var exception = new ErrorException(
                    Errors.General.Unexpected(
                        $"Details for tenant returned by MemoryCachingHelper.AddOrGetAsync is null. Tenant Id: {tenantId}"));
                await this.mediator.Send(new CaptureSentryExceptionCommand(exception))
                    .ContinueWith(t => { throw t.Exception; }, TaskContinuationOptions.OnlyOnFaulted);
            }

            return tenant;
        }

        private async Task<Product?> GetProductByAlias(string tenantAlias, string productAlias)
        {
            if (string.IsNullOrEmpty(tenantAlias) || string.IsNullOrEmpty(productAlias))
            {
                return null;
            }

            var tenant = await this.GetTenantByAlias(tenantAlias);
            Product? product = null;

            if (tenant != null)
            {
                product = await this.GetProductByAlias(tenant.Id, productAlias);
            }

            return product;
        }

        private async Task<Product?> GetProduct(Guid tenantId, GuidOrAlias productIdOrAlias)
        {
            if (!productIdOrAlias.HasValue())
            {
                return null;
            }

            var tenant = await this.GetTenantAsync(tenantId);
            if (tenant == null)
            {
                return null;
            }

            if (productIdOrAlias.Guid.IsNullOrEmpty() || !productIdOrAlias.Guid.HasValue)
            {
                return await this.GetProductByAlias(tenant.Id, productIdOrAlias.Alias);
            }

            return await this.GetProduct(tenant.Id, productIdOrAlias.Guid.Value);
        }

        private async Task<Product?> GetProduct(GuidOrAlias tenantIdOrAlias, GuidOrAlias productIdOrAlias)
        {
            if (!tenantIdOrAlias.HasValue() || !productIdOrAlias.HasValue())
            {
                return null;
            }

            var tenant = tenantIdOrAlias.Guid.IsNullOrEmpty() || !tenantIdOrAlias.Guid.HasValue
                ? await this.GetTenantByAlias(tenantIdOrAlias.Alias)
                : await this.GetTenantAsync(tenantIdOrAlias.Guid.Value);

            if (tenant == null)
            {
                return null;
            }

            if (productIdOrAlias.Guid.IsNullOrEmpty() || !productIdOrAlias.Guid.HasValue)
            {
                return await this.GetProductByAlias(tenant.Id, productIdOrAlias.Alias);
            }

            return await this.GetProduct(tenant.Id, productIdOrAlias.Guid.Value);
        }

        private async Task<Product?> GetProduct(Guid tenantId, Guid productId)
        {
            tenantId.ThrowIfEmpty("tenant Id");
            productId.ThrowIfEmpty("product Id");

            var tenant = await this.GetTenantAsync(tenantId);
            if (tenant == null)
            {
                return null;
            }

            var product = await MemoryCachingHelper.AddOrGetAsync(
                 this.GetProductCacheKey(tenant.Id, productId),
                 async () =>
                 {
                     return await this.mediator.Send(new GetProductByIdQuery(tenant.Id, productId));
                 },
                 this.DefaultCacheExpiration);

            if (product != null && product.Details == null)
            {
                product = await this.mediator.Send(new GetProductByIdQuery(tenantId, productId));
                MemoryCachingHelper.Upsert(
                    this.GetProductCacheKey(tenantId, productId),
                    product,
                    this.DefaultCacheExpiration);

                // log in sentry but dont throw
                var exception = new ErrorException(
                    Errors.General.Unexpected(
                        $"Details for product returned by MemoryCachingHelper.AddOrGetAsync is null. Product Id: {productId}"));
                await this.mediator.Send(new CaptureSentryExceptionCommand(exception))
                     .ContinueWith(t => { throw t.Exception; }, TaskContinuationOptions.OnlyOnFaulted);
            }

            return product;
        }

        private IEnumerable<Setting>? GetSettings(Guid tenantId)
        {
            return MemoryCachingHelper.AddOrGet(
                this.GetFeatureSettingCacheKey(tenantId),
                () => this.featureSettingRepository.GetSettings(tenantId),
                this.DefaultCacheExpiration);
        }

        private IEnumerable<Setting>? GetActiveSettings(Guid tenantId)
        {
            return MemoryCachingHelper.AddOrGet(
                this.GetFeatureSettingCacheKey(tenantId),
                () => this.featureSettingRepository.GetActiveSettings(tenantId),
                this.DefaultCacheExpiration);
        }

        private async Task<OrganisationReadModel?> GetOrganisation(GuidOrAlias tenantIdOrAlias, GuidOrAlias organisationIdOrAlias)
        {
            if (!tenantIdOrAlias.HasValue() || !organisationIdOrAlias.HasValue())
            {
                return null;
            }

            var tenant = tenantIdOrAlias.Guid.IsNullOrEmpty() || !tenantIdOrAlias.Guid.HasValue
                ? await this.GetTenantByAlias(tenantIdOrAlias.Alias)
                : await this.GetTenantAsync(tenantIdOrAlias.Guid.Value);

            if (tenant == null)
            {
                return null;
            }

            return organisationIdOrAlias.Guid.IsNullOrEmpty() || !organisationIdOrAlias.Guid.HasValue
                ? await this.GetOrganisationByAlias(tenant.Id, organisationIdOrAlias.Alias)
                : await this.GetOrganisation(tenant.Id, organisationIdOrAlias.Guid.Value);
        }

        private async Task<OrganisationReadModel?> GetOrganisation(Guid tenantId, GuidOrAlias organisationIdOrAlias)
        {
            if (!organisationIdOrAlias.HasValue())
            {
                return null;
            }

            var tenant = await this.GetTenantAsync(tenantId);
            if (tenant == null)
            {
                return null;
            }

            return organisationIdOrAlias.Guid.IsNullOrEmpty() || !organisationIdOrAlias.Guid.HasValue
                ? await this.GetOrganisationByAlias(tenant.Id, organisationIdOrAlias.Alias)
                : await this.GetOrganisation(tenant.Id, organisationIdOrAlias.Guid.Value);
        }

        private async Task<OrganisationReadModel?> GetOrganisation(Guid tenantId, Guid organisationId)
        {
            if (tenantId == Guid.Empty || organisationId == Guid.Empty)
            {
                return null;
            }

            var tenant = await this.GetTenantAsync(tenantId);
            tenant = EntityHelper.ThrowIfNotFound(tenant, tenantId, "tenant");
            return await this.GetOrganisationCacheByGuid(tenant.Id, organisationId);
        }

        private async Task<OrganisationReadModel?> GetOrganisationCacheByGuid(Guid tenantId, Guid organisationId)
        {
            return await MemoryCachingHelper.AddOrGetAsync(
                 this.GetOrganisationCacheKey(tenantId, organisationId),
                 async () =>
                 {
                     return await this.mediator.Send(new GetOrganisationByIdQuery(tenantId, organisationId));
                 },
                 this.DefaultCacheExpiration);
        }

        private async Task<OrganisationReadModel?> GetOrganisationByAlias(Guid tenantId, string organisationAlias)
        {
            if (tenantId == Guid.Empty || string.IsNullOrEmpty(organisationAlias))
            {
                return null;
            }

            var organisation = await this.GetOrganisationCacheByAlias(tenantId, organisationAlias);
            if (organisation != null)
            {
                // Seed the cache by ID as well
                MemoryCachingHelper.Upsert(this.GetOrganisationCacheKey(tenantId, organisation.Id), organisation, this.DefaultCacheExpiration);
            }

            return organisation;
        }

        private async Task<OrganisationReadModel?> GetOrganisationCacheByAlias(Guid tenantId, string organisationAlias)
        {
            return await MemoryCachingHelper.AddOrGetAsync(
                this.GetOrganisationCacheKey(tenantId, organisationAlias),
                async () =>
                {
                    return await this.mediator.Send(new GetOrganisationByAliasQuery(tenantId, organisationAlias));
                },
                this.DefaultCacheExpiration);
        }

        private async Task<PortalReadModel?> GetPortal(Guid tenantId, GuidOrAlias portalIdOrAlias)
        {
            if (tenantId == default || !portalIdOrAlias.HasValue())
            {
                return null;
            }

            var tenant = await this.GetTenantAsync(tenantId);
            if (tenant == null)
            {
                return null;
            }

            if (portalIdOrAlias.Guid == null)
            {
                var portalAlias = portalIdOrAlias.Alias;
                var portal = await this.GetPortalCacheByAlias(tenant.Id, portalAlias);
                if (portal != null)
                {
                    // Seed the cache by ID as well
                    MemoryCachingHelper.Upsert(this.GetPortalCacheKey(tenant.Id, portal.Id), portal, this.DefaultCacheExpiration);
                }

                return portal;
            }

            return await this.GetPortal(tenantId, portalIdOrAlias.Guid.Value);
        }

        private async Task<PortalReadModel?> GetPortalCacheByAlias(Guid tenantId, string portalAlias)
        {
            return await MemoryCachingHelper.AddOrGetAsync(
                this.GetPortalCacheKey(tenantId, portalAlias),
                async () =>
                {
                    return await this.mediator.Send(new GetPortalByAliasQuery(tenantId, portalAlias));
                },
                this.DefaultCacheExpiration);
        }

        private async Task<PortalReadModel?> GetPortal(Guid tenantId, Guid portalId)
        {
            if (portalId == Guid.Empty)
            {
                return null;
            }

            return await this.GetPortalCacheByGuid(tenantId, portalId);
        }

        private async Task<PortalReadModel?> GetPortalCacheByGuid(Guid tenantId, Guid portalId)
        {
            return await MemoryCachingHelper.AddOrGetAsync(
                this.GetPortalCacheKey(tenantId, portalId),
                async () =>
                {
                    return await this.mediator.Send(new GetPortalByIdQuery(tenantId, portalId));
                },
                this.DefaultCacheExpiration);
        }

        private List<ProductFeatureSetting>? GetDeployedProductSettings(
            Guid tenantId,
            DeploymentEnvironment environment)
        {
            tenantId.ThrowIfEmpty("tenant Id");
            return this.GetDeployedProductSettingsCache(tenantId, environment);
        }

        private List<ProductFeatureSetting>? GetDeployedProductSettingsCache(Guid tenantId, DeploymentEnvironment environment)
        {
            return MemoryCachingHelper.AddOrGet(
                this.GetProductSettingCacheKey(tenantId, environment),
                () => this.productFeatureSettingRepository.GetDeployedProductFeatureSettings(tenantId, environment),
                this.DefaultCacheExpiration);
        }

        private ProductFeatureSetting? GetProductSetting(Guid tenantId, Guid productId)
        {
            tenantId.ThrowIfEmpty("tenant Id");
            return MemoryCachingHelper.AddOrGet(
                this.GetProductSettingCacheKey(tenantId, productId),
                () => this.productFeatureSettingRepository.GetProductFeatureSetting(tenantId, productId),
                this.DefaultCacheExpiration);
        }

        private bool IsTenantIdEmptyOrInvalid(Guid tenantId)
        {
            return this.GetTenant(tenantId) == null;
        }

        private string ThrowErrorIfAliasIsNullOrEmpty(string? alias, string aliasName)
        {
            if (string.IsNullOrEmpty(alias))
            {
                throw new ErrorException(Errors.General.BadRequest($"The {aliasName} was not passed or had an empty value."));
            }
            return alias;
        }

        private void ThrowErrorIfGuidOrAliasIsEmpty(GuidOrAlias guidOrAlias, string guidOrAliasName)
        {
            if (!guidOrAlias.HasValue())
            {
                throw new ErrorException(Errors.General.BadRequest($"The {guidOrAliasName} was not passed or had an empty value."));
            }
        }

        private string GetTenantCacheKey(Guid tenantId) => $"tenantId:{tenantId}";

        private string GetTenantCacheKey(string tenantAlias) => $"tenantAlias:{tenantAlias}";

        private string GetProductCacheKey(Guid tenantId, Guid productId) => $"tenantId:{tenantId}|productId:{productId}";

        private string GetProductCacheKey(Guid tenantId, string productAlias) => $"tenantId:{tenantId}|productAlias:{productAlias}";

        private string GetOrganisationCacheKey(Guid tenantId, Guid organisationId) => $"tenantId:{tenantId}|organisationId:{organisationId}";

        private string GetOrganisationCacheKey(Guid tenantId, string organisationAlias) => $"tenantId:{tenantId}|organisationAlias:{organisationAlias}";

        private string GetPortalCacheKey(Guid tenantId, Guid id) => $"tenantId:{tenantId}|portalId:{id}";

        private string GetPortalCacheKey(Guid tenantId, string alias) => $"tenantId:{tenantId}|portalAlias:{alias}";

        private string GetFeatureSettingCacheKey(Guid tenantId) => $"tenantIdFeatureSettings:{tenantId}";

        private string GetProductSettingCacheKey(Guid tenantId, DeploymentEnvironment environment) => $"tenantIdProductSettings:{tenantId}|environment:{environment}";

        private string GetProductSettingCacheKey(Guid tenantId, Guid productId) => $"tenantIdProductSettings:{tenantId}|productId:{productId}";

        private string GetProductReleaseNumberCacheKey(Guid tenantId, Guid productId, string productReleaseNumber)
            => $"tenantId:{tenantId}|productId:{productId}|productReleaseNumber:{productReleaseNumber}";

        private string GetActiveProductsCacheKey() => "activeProducts";
    }
}
