// <copyright file="IClaimService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using UBind.Domain.Aggregates.Claim;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Claim;

    /// <summary>
    /// Service for claims-related processes.
    /// </summary>
    public interface IClaimService
    {
        /// <summary>
        /// Retrieves a collection of claim records in the system that satisfy the given parameters.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="filters">Additional filters to be applied to the result set.</param>
        /// <returns>A collection of claim summary records.</returns>
        IEnumerable<IClaimReadModelSummary> GetClaims(
            Guid tenantId, EntityListFilters filters);

        /// <summary>
        /// Determines whether claims exist for the given policy.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="policyId">The policy Id.</param>
        /// <returns>True if claims exist, false otherwise.</returns>
        bool HasClaimsFromPolicy(Guid tenantId, Guid policyId);

        /// <summary>
        /// Checks if any claim exists for the specified customer.
        /// </summary>
        /// <param name="filters">The filters to apply.</param>
        /// <param name="excludedClaimIds">The claim Ids to exclude on the search.</param>
        /// <returns>True if claims exist, false otherwise.</returns>
        bool HasClaimsForCustomer(EntityListFilters filters, IEnumerable<Guid> excludedClaimIds);

        /// <summary>
        /// Retrieves a collection of claim records from the given policy.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="policyId">The associated policy Id.</param>
        /// <returns>A collection of claims.</returns>
        IEnumerable<ClaimReadModel> GetClaimsFromPolicy(Guid tenantId, Guid policyId);

        /// <summary>
        /// Associate claim with policy.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant.</param>
        /// <param name="policyId">The policy ID.</param>
        /// <param name="claimId">The claim ID.</param>
        /// <returns>ClaimAggregate.</returns>
        Task<ClaimAggregate> AssociateClaimWithPolicyAsync(Guid tenantId, Guid policyId, Guid claimId);

        /// <summary>
        /// Disassociate claim with policy.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant.</param>
        /// <param name="claimId">The claim ID.</param>
        /// <param name="policyId">The policy ID.</param>
        /// <returns>ClaimAggregate.</returns>
        Task<ClaimAggregate?> DisassociateClaimWithPolicyAsync(Guid tenantId, Guid claimId, Guid policyId);

        /// <summary>
        /// Change claim status.
        /// </summary>
        /// <param name="releaseContext">The product release to use.</param>
        /// <param name="aggregate">The claim aggregate.</param>
        /// <param name="operation">The claim action.</param>
        /// <param name="formDataJson">The form data json to update the claim with.</param>
        /// <returns>The updated claim record.</returns>
        Task<ClaimAggregate> ChangeClaimState(ReleaseContext releaseContext, ClaimAggregate aggregate, ClaimActions operation, string? formDataJson);

        /// <summary>
        /// Assign claim number.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="claimId">The ID of the claim to be updated.</param>
        /// <param name="newClaimNumber">The claim number.</param>
        /// <param name="environment">The environment.</param>
        /// <param name="isRestoreToList">Is to restore the old used claimNumber to list.</param>
        /// <returns>The updated claim record.</returns>
        Task<ClaimAggregate> AssignClaimNumber(Guid tenantId, Guid claimId, string newClaimNumber, DeploymentEnvironment environment, bool isRestoreToList = false);

        /// <summary>
        /// Unassign a claim number.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="claimId">The ID of the claim to be updated.</param>
        /// <param name="environment">The environment.</param>
        /// <param name="isRestoreToList">Is to restore the old used claimNumber to list.</param>
        /// <returns>The updated claim record.</returns>
        Task<ClaimAggregate> UnassignClaimNumber(Guid tenantId, Guid claimId, DeploymentEnvironment environment, bool isRestoreToList);

        /// <summary>
        /// Updates the claims' formData.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant.</param>
        /// <param name="claimId">The ID of the claim to be updated.</param>
        /// <param name="formDataJson">The form data json.</param>
        /// <returns>The updated claim record.</returns>
        Task<ClaimAggregate> UpdateFormData(Guid tenantId, Guid claimId, string formDataJson);

        /// <summary>
        /// Creates a claim version from the given parameters.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant.</param>
        /// <param name="claimId">The ID of the claim the version is for.</param>
        /// <param name="formDataJson">The form data json.</param>
        /// <returns>The updated claim aggregate.</returns>
        Task<ClaimAggregate> CreateVersion(Guid tenantId, Guid claimId, string formDataJson);

        /// <summary>
        /// Get the claim document content by document Id.
        /// </summary>
        /// <param name="tenantId">The tenantId.</param>
        /// <param name="documentId">The document Id.</param>
        /// <param name="claimId">The claim Id.</param>
        /// <returns>The claim document content.</returns>
        IFileContentReadModel GetClaimDocumentContent(Guid tenantId, Guid documentId, Guid claimId);
    }
}
