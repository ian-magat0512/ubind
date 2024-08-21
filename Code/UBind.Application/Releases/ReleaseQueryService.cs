// <copyright file="ReleaseQueryService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Releases
{
    using System.Collections.Generic;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Product;
    using UBind.Domain.Product.Component;
    using UBind.Domain.Services;
    using UBind.Domain.ValueTypes;

    /// <inheritdoc/>
    public class ReleaseQueryService : IReleaseQueryService
    {
        private readonly ICachingResolver cachingResolver;
        private readonly IGlobalReleaseCache globalReleaseCache;
        private readonly IFieldSerializationBinder fieldSerializationBinder;
        private readonly IProductFeatureSettingRepository productFeatureSettingRepository;
        private readonly IProductReleaseService productReleaseService;
        private readonly IReleaseRepository releaseRepository;

        /// <summary>
        /// Since ReleaseQueryService is a scope service, this cache will store releases for fast access
        /// for the duration of the request.
        /// However we also use GlobalReleaseCache which stores releases in the cache for longer.
        /// </summary>
        private Dictionary<ReleaseContext, ActiveDeployedRelease> scopedReleaseCache =
            new Dictionary<ReleaseContext, ActiveDeployedRelease>();

        private Dictionary<ProductContext, Guid> scopedDefaultReleaseIdCache = new Dictionary<ProductContext, Guid>();

        public ReleaseQueryService(
            IGlobalReleaseCache globalReleaseCache,
            IFieldSerializationBinder fieldSerializationBinder,
            ICachingResolver cachingResolver,
            IProductFeatureSettingRepository productFeatureSettingRepository,
            IProductReleaseService productReleaseService,
            IReleaseRepository releaseRepository)
        {
            this.cachingResolver = cachingResolver;
            this.globalReleaseCache = globalReleaseCache;
            this.fieldSerializationBinder = fieldSerializationBinder;
            this.productFeatureSettingRepository = productFeatureSettingRepository;
            this.productReleaseService = productReleaseService;
            this.releaseRepository = releaseRepository;
        }

        public Guid? GetDefaultProductReleaseIdOrNull(Guid tenantId, Guid productId, DeploymentEnvironment environment)
        {
            var productContext = new ProductContext(tenantId, productId, environment);
            if (this.scopedDefaultReleaseIdCache.TryGetValue(productContext, out Guid cachedDefaultReleaseId))
            {
                return cachedDefaultReleaseId;
            }

            Guid? defaultReleaseId = this.productReleaseService.GetDefaultProductReleaseId(tenantId, productId, environment);
            if (defaultReleaseId != null)
            {
                this.scopedDefaultReleaseIdCache[productContext] = defaultReleaseId.Value;
            }

            return defaultReleaseId;
        }

        public Guid GetDefaultProductReleaseIdOrThrow(Guid tenantId, Guid productId, DeploymentEnvironment environment)
        {
            Guid? releaseId = this.GetDefaultProductReleaseIdOrNull(tenantId, productId, environment);
            if (releaseId == null)
            {
                var tenantAlias = this.cachingResolver.GetTenantAliasOrThrow(tenantId);
                var productAlias = this.cachingResolver.GetProductAliasOrThrow(tenantId, productId);
                throw new ErrorException(Errors.Release.DefaultReleaseNotFound(tenantAlias, productAlias, environment));
            }

            return releaseId.Value;
        }

        public ReleaseContext? GetDefaultReleaseContextOrNull(Guid tenantId, Guid productId, DeploymentEnvironment environment)
        {
            Guid? releaseId = this.GetDefaultProductReleaseIdOrNull(tenantId, productId, environment);
            if (releaseId == null)
            {
                return null;
            }

            return new ReleaseContext(tenantId, productId, environment, releaseId.Value);
        }

        public ReleaseContext GetDefaultReleaseContextOrThrow(Guid tenantId, Guid productId, DeploymentEnvironment environment)
        {
            Guid releaseId = this.GetDefaultProductReleaseIdOrThrow(tenantId, productId, environment);
            return new ReleaseContext(tenantId, productId, environment, releaseId);
        }

        public ReleaseContext GetReleaseContextForReleaseOrDefaultRelease(
            Guid tenantId,
            Guid productId,
            DeploymentEnvironment environment,
            Guid? productReleaseId)
        {
            productReleaseId = productReleaseId
                ?? this.GetDefaultProductReleaseIdOrThrow(tenantId, productId, environment);
            return new ReleaseContext(
                tenantId,
                productId,
                environment,
                productReleaseId.Value);
        }

        /// <inheritdoc/>
        public ActiveDeployedRelease GetRelease(ReleaseContext context)
        {
            if (this.scopedReleaseCache.TryGetValue(context, out ActiveDeployedRelease? releaseFromLocalCache))
            {
                return releaseFromLocalCache;
            }

            var releaseFromGlobalCache = this.globalReleaseCache.GetRelease(
                context,
                this.productReleaseService);
            this.scopedReleaseCache[context] = releaseFromGlobalCache;
            return releaseFromGlobalCache;
        }

        /// <inheritdoc/>
        public ActiveDeployedRelease GetReleaseWithoutCachingOrAssets(ReleaseContext context)
        {
            ActiveDeployedRelease activeDeployedRelease;
            ReleaseBase? release = this.productReleaseService.GetReleaseFromDatabaseWithoutAssets(context.TenantId, context.ProductReleaseId);
            return new ActiveDeployedRelease(
                        release,
                        context.Environment,
                        this.fieldSerializationBinder);
        }

        public async Task<Guid> GetProductReleaseIdByReleaseNumber(Guid tenantId, Guid productId, string releaseNumber)
        {
            try
            {
                var releaseNumberObject = new ReleaseNumber(releaseNumber);
                var releaseId = this.releaseRepository.GetReleaseIdByReleaseNumber(
                    tenantId,
                    productId,
                    releaseNumberObject.Major,
                    releaseNumberObject.Minor);
                if (releaseId == null)
                {
                    var tenantAlias = await this.cachingResolver.GetTenantAliasOrThrowAsync(tenantId);
                    var productAlias = await this.cachingResolver.GetProductAliasOrThrowAsync(tenantId, productId);
                    throw new ErrorException(Errors.Release.ReleaseNumberNotFound(tenantAlias, productAlias, releaseNumber));
                }

                return releaseId.Value;
            }
            catch (ErrorException ex) when (ex.Error.Code == "release.number.invalid")
            {
                var tenantAlias = await this.cachingResolver.GetTenantAliasOrThrowAsync(tenantId);
                var productAlias = await this.cachingResolver.GetProductAliasOrThrowAsync(tenantId, productId);
                throw new ErrorException(Errors.Release.NumberInvalid(tenantAlias, productAlias, releaseNumber));
            }
        }
    }
}
