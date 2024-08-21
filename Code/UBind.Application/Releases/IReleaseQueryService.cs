// <copyright file="IReleaseQueryService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Releases
{
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Product;

    /// <summary>
    /// Service for obtaining the current release for a given product context.
    /// </summary>
    /// <remarks>
    /// This service caches releases the first time they are fetched, and is intended to be used to cache releases on a per-request basis.
    /// This means that any cost in checking the cache of the upstream service will only occur a single time per request.
    /// </remarks>
    public interface IReleaseQueryService
    {
        Guid? GetDefaultProductReleaseIdOrNull(Guid tenantId, Guid productId, DeploymentEnvironment environment);

        Guid GetDefaultProductReleaseIdOrThrow(Guid tenantId, Guid productId, DeploymentEnvironment environment);

        ReleaseContext? GetDefaultReleaseContextOrNull(Guid tenantId, Guid productId, DeploymentEnvironment environment);

        ReleaseContext GetDefaultReleaseContextOrThrow(Guid tenantId, Guid productId, DeploymentEnvironment environment);

        ReleaseContext GetReleaseContextForReleaseOrDefaultRelease(
            Guid tenantId,
            Guid productId,
            DeploymentEnvironment environment,
            Guid? productReleaseId);

        /// <summary>
        /// Get the current release for a given product context.
        /// This method will cache the release files and will create workbooks on the memory based on the configuration.
        /// </summary>
        /// <param name="context">The product context specifying which product and environment the release is for.</param>
        /// <returns>The currently deployed release if there is one, otherwise it throws an exception.</returns>
        /// <exception cref="ErrorException">thrown if no release is available for the given product context.</exception>
        ActiveDeployedRelease GetRelease(ReleaseContext context);

        /// <summary>
        /// Try and get the active release for a given product context without caching.
        /// This method will NOT cache the release files and will NOT create workbooks on the memory.
        /// </summary>
        /// <param name="context">The product context specifying which product and environment the release is for.</param>
        /// <returns>The currently deployed release, if there is one, otherwise none.</returns>
        ActiveDeployedRelease GetReleaseWithoutCachingOrAssets(ReleaseContext context);

        Task<Guid> GetProductReleaseIdByReleaseNumber(Guid tenantId, Guid productId, string releaseNumber);
    }
}
