// <copyright file="IGlobalReleaseCache.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Releases
{
    using System;
    using System.Collections.Generic;
    using UBind.Domain;
    using UBind.Domain.Product;
    using UBind.Domain.Services;

    /// <summary>
    /// Global cache of current releases by product context.
    /// </summary>
    public interface IGlobalReleaseCache
    {
        /// <summary>
        /// Event raised when a release is cached.
        /// </summary>
        event EventHandler<ReleaseCachingArgs> ReleaseCached;

        /// <summary>
        /// Pro-actively cache a new dev release.
        /// </summary>
        /// <param name="artefacts">The artefacts from dev release initialization.</param>
        ActiveDeployedRelease CacheNewDevRelease(DevReleaseInitializationArtefacts artefacts);

        /// <summary>
        /// Invalidate the cache where items match the given parameters.
        /// </summary>
        /// <param name="productReleaseService">The service for managing product releases.</param>
        /// <param name="tenantId">The tenant ID. Pass in null or leave empty to delete releases from all tenants.</param>
        /// <param name="productId">The product ID. Pass in null or leave this empty to delete releases for all products.</param>
        /// <param name="environment">The environment. Pass in null or leave this empty to delete releases in all environments.</param>
        /// <param name="productReleaseId">The product release ID. Provide a value for this if you need to delete a specific release only.</param>
        /// <returns>The number of cached releases that were cleared.</returns>
        int InvalidateCache(
            IProductReleaseService productReleaseService,
            Guid? tenantId = null,
            Guid? productId = null,
            DeploymentEnvironment? environment = null,
            Guid? productReleaseId = null);

        /// <summary>
        /// Gets the current release for a given product context.
        /// <returns>
        /// The cached release, if it exists and is still current,
        /// otherwise the release from the database, or null if non exists.
        /// </returns>
        ActiveDeployedRelease GetRelease(
            ReleaseContext context,
            IProductReleaseService productReleaseService);

        /// <summary>
        /// Get a summary of what is currently cached.
        /// </summary>
        /// <returns>A map of product context to release ID for all cached releases.</returns>
        IEnumerable<KeyValuePair<ReleaseContext, Guid>> GetCacheState();
    }
}
