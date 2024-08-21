// <copyright file="IDevReleaseRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Repositories
{
    using System;
    using NodaTime;

    /// <summary>
    /// A repository for developement releases.
    /// </summary>
    public interface IDevReleaseRepository
    {
        /// <summary>
        /// Get a release by ID.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant.</param>
        /// <param name="releaseId">The release ID.</param>
        /// <returns>The release.</returns>
        DevRelease? GetDevReleaseByIdWithFileContents(Guid tenantId, Guid releaseId);

        DevRelease? GetDevReleaseForProductWithoutAssetFileContents(Guid tenantId, Guid productId);

        DevRelease? GetDevReleaseForProductWithoutAssets(Guid tenantId, Guid productId);

        DevRelease? GetDevReleaseWithoutAssets(Guid tenantId, Guid productReleaseId);

        DevRelease? GetDevReleaseWithoutAssetFileContents(Guid tenantId, Guid productReleaseId);

        /// <summary>
        /// Gets the ID of the latest initalized dev release for a given product. I.e. the one to be used currently.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant the product is in.</param>
        /// <param name="productId">The ID of the product.</param>
        /// <returns>The ID of the latest initialized dev release, or default if none.</returns>
        Guid? GetIdOfDevReleaseForProduct(Guid tenantId, Guid productId);

        /// <summary>
        /// Gets the timestamp representing the last synchronization point for the cached release.
        /// This timestamp indicates the latest moment when either quote or claim details were synchronized.
        /// If both quote and claim details exist, it reflects the timestamp of the more recent synchronization.
        /// This property provides insight into when the release information was last updated or synchronized.
        /// </summary>
        Instant? GetLastModifiedTimestamp(Guid guid, Guid productReleaseId);

        /// <summary>
        /// Insert a new release into the repository.
        /// </summary>
        /// <param name="devRelease">The release to insert.</param>
        void Insert(DevRelease devRelease);

        /// <summary>
        /// Persist insertions.
        /// </summary>
        void SaveChanges();

        Task SaveChanges(ReleaseDetailsChangeTracker changeTracker);
    }
}
