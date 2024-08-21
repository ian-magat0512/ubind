// <copyright file="IClaimVersionReadModelRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel.Claim
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// For retrieving persisted claim version details.
    /// </summary>
    public interface IClaimVersionReadModelRepository
    {
        /// <summary>
        /// Gets the claim version read model by ID.
        /// </summary>
        ClaimVersionReadModel GetById(Guid tenantId, Guid claimVersionId);

        /// <summary>
        /// Gets all versions of a given claim.
        /// </summary>
        /// <returns>All versions of a given claim.</returns>
        /// <param name="claimId">The ID of the claim.</param>
        IEnumerable<IClaimVersionReadModelDetails> GetVersionsOfClaim(Guid tenantId, Guid claimId);

        /// <summary>
        /// Gets a specific version of a given claim.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant.</param>
        /// <param name="claimId">The ID of the claim.</param>
        /// <param name="versionNumber">The number of the version to retrieve.</param>
        /// <returns>The specific version of the claim with the given number.</returns>
        IClaimVersionReadModelDetails GetDetailsByVersionNumber(Guid tenantId, Guid claimId, int versionNumber);

        /// <summary>
        /// Gets a specific version of a given claim by ID.
        /// </summary>
        /// <param name="versionId">The ID of the claim version to retrieve.</param>
        /// <returns>The specific version of the claim with the given ID.</returns>
        IClaimVersionReadModelDetails GetDetailsById(Guid tenantId, Guid versionId);

        /// <summary>
        /// Gets a specific version of a given claim by ID.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="environment">The deployment environment.</param>
        /// <param name="claimVersionId">The claim version Id.</param>
        /// <param name="relatedEntities">The list of related entities to include.</param>
        /// <returns>The specific version of the claim.</returns>
        IClaimVersionReadModelWithRelatedEntities GetClaimVersionWithRelatedEntities(
            Guid tenantId, DeploymentEnvironment? environment, Guid claimVersionId, IEnumerable<string> relatedEntities);

        /// <summary>
        /// Gets a specific version of a given claim by ID.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="claimId">The claim Id.</param>
        /// <param name="versionNumber">The claim version number.</param>
        /// <param name="relatedEntities">The list of related entities to include.</param>
        /// <returns>The specific version of the claim.</returns>
        IClaimVersionReadModelWithRelatedEntities GetClaimVersionWithRelatedEntities(
            Guid tenantId,
            DeploymentEnvironment? environment,
            Guid claimId,
            int versionNumber,
            IEnumerable<string> relatedEntities);

        /// <summary>
        /// Gets a specific version of a given claim by reference.
        /// </summary>
        /// <param name="claimReference">The claim reference.</param>
        /// <param name="environment">The claim environment.</param>
        /// <param name="versionNumber">The claim version number.</param>
        /// <returns>The specific version of the claim.</returns>
        /// <param name="relatedEntities">The list of related entities to include.</param>
        /// <returns>The specific version of the claim matching the reference.</returns>
        IClaimVersionReadModelWithRelatedEntities GetClaimVersionWithRelatedEntitiesByReference(
            Guid tenantId,
            string claimReference,
            DeploymentEnvironment? environment,
            int versionNumber,
            IEnumerable<string> relatedEntities);

        /// <summary>
        /// Gets a specific version of a given claim by number.
        /// </summary>
        /// <param name="claimNumber">The claim number.</param>
        /// <param name="environment">The claim environment.</param>
        /// <param name="versionNumber">The claim version number.</param>
        /// <returns>The specific version of the claim.</returns>
        /// <param name="relatedEntities">The list of related entities to include.</param>
        /// <returns>The specific version of the claim that matched.</returns>
        IClaimVersionReadModelWithRelatedEntities GetClaimVersionWithRelatedEntitiesByNumber(
            Guid tenantId,
            string claimNumber,
            DeploymentEnvironment? environment,
            int versionNumber,
            IEnumerable<string> relatedEntities);

        /// <summary>
        /// Method for creating IQueryable method that retrieve claim versions and its related entities.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="environment">The deployment environment.</param>
        /// <param name="relatedEntities">The related entities to retrieve.</param>
        /// <returns>IQueryable for claim versions.</returns>
        IQueryable<ClaimVersionReadModelWithRelatedEntities> CreateQueryForClaimVersionDetailsWithRelatedEntities(
            Guid tenantId, DeploymentEnvironment? environment, IEnumerable<string> relatedEntities);

        /// <summary>
        /// Get all claim version ids by filters.
        /// </summary>
        /// <returns>List of claim version ids as string.</returns>
        /// <param name="entityFilters">Instance of <see cref="EntityFilters"/>.</param>
        /// <returns>A list of claim version ids as string.</returns>
        List<Guid> GetAllClaimVersionIdsByEntityFilters(EntityFilters entityFilters);
    }
}
