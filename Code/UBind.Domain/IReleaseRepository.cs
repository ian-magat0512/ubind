// <copyright file="IReleaseRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using System;
    using System.Collections.Generic;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// A repository for product releases.
    /// </summary>
    public interface IReleaseRepository
    {
        Guid? GetReleaseIdByReleaseNumber(Guid tenantId, Guid productId, int majorNumber, int? minorNumber);

        Release? GetReleaseWithoutAssetFileContents(Guid tenantId, Guid productReleaseId);

        Release? GetReleaseWithoutAssets(Guid tenantId, Guid productReleaseId);

        /// <summary>
        /// Get all the releases for a given tenant (or all tenants).
        /// </summary>
        /// <param name="tenantId">The ID of the tenant to get release for.</param>
        /// <param name="filters">Filters to apply.</param>
        /// <returns>All the releases for the product.</returns>
        IEnumerable<Release> GetReleases(Guid tenantId, EntityListFilters filters);

        /// <summary>
        /// Get all the releases for a given product.
        /// </summary>
        /// <param name="tenantId">The ID of the product's tenant.</param>
        /// <param name="productId">The ID of the product.</param>
        /// <param name="filters">The filters to apply.</param>
        /// <returns>All the releases for the product.</returns>
        IEnumerable<Release> GetReleasesForProduct(Guid tenantId, Guid productId, EntityListFilters filters);

        /// <summary>
        /// Get a release by ID, including all of the file contents.
        /// This is slow, use with caution.
        /// </summary>
        /// <param name="releaseId">The release ID.</param>
        /// <returns>The release.</returns>
        Release? GetReleaseByIdWithFileContents(Guid tenantId, Guid releaseId);

        /// <summary>
        /// Insert a new release into the repository.
        /// </summary>
        /// <param name="release">The release to insert.</param>
        void Insert(Release release);

        void Delete(Release release);

        /// <summary>
        /// gets the highest release number for product.
        /// </summary>
        /// <param name="tenantId">tenantId id.</param>
        /// <param name="productId">product id.</param>
        /// <returns>release number model.</returns>
        ReleaseNumberModel GetHighestReleaseNumberForProduct(Guid tenantId, Guid productId);

        /// <summary>
        /// Persist insertions.
        /// </summary>
        void SaveChanges();
    }
}
