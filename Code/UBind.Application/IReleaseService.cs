// <copyright file="IReleaseService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using UBind.Domain;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// Service for managing release creation and initialization.
    /// </summary>
    public interface IReleaseService
    {
        /// <summary>
        /// Create a release for a given product.
        /// </summary>
        /// <param name="tenantId">The ID of the product's tenant.</param>
        /// <param name="productId">The product ID.</param>
        /// <param name="label">A label for the release.</param>
        /// <param name="type">A type for the release.</param>
        /// <returns>A <see cref="Task"/> that result to a Release object.</returns>
        Task<Release> CreateReleaseAsync(Guid tenantId, Guid productId, string label, ReleaseType type);

        /// <summary>
        /// Edit existing release.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant.</param>
        /// <param name="id">The release ID.</param>
        /// <param name="productId">The product ID.</param>
        /// <param name="number">The release number.</param>
        /// <param name="label">A label for the release.</param>
        /// <param name="type">A type for the release.</param>
        /// <returns>A the updated release.</returns>
        Release UpdateRelease(
            Guid tenantId,
            Guid id,
            Guid productId,
            int number,
            int minorNumber,
            string label,
            ReleaseType type);

        /// <summary>
        /// Deletea a release.
        /// </summary>
        /// <param name="tenantId">The ID of the requester's tenant.</param>
        /// <param name="releaseId">The ID of the release to delete.</param>
        void DeleteRelease(Guid tenantId, Guid releaseId);

        /// <summary>
        /// Retrieves a collection of release records in the system that satisfy the given parameters.
        /// </summary>
        /// <param name="tenantId">The tenant of the user requesting the releases.</param>
        /// <param name="filters">Additional filters to be applied to the result set.</param>
        /// <returns>A collection of release records.</returns>
        IEnumerable<Release> GetReleases(Guid tenantId, EntityListFilters filters);

        /// <summary>
        /// Restore a release back into the development environment by copying all files there.
        /// </summary>
        /// <param name="userTenantId">The Tenant ID of the requesting user.</param>
        /// <param name="releaseId">The ID of the release to restore.</param>
        /// <returns>An awaitable task.</returns>
        Task RestoreReleaseToDevelopmentEnvironment(Guid userTenantId, Guid releaseId);
    }
}
