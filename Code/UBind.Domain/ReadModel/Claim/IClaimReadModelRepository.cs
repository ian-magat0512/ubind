// <copyright file="IClaimReadModelRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.ReadModel.Claim
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NodaTime;

    /// <summary>
    /// For retrieving persisted quote details.
    /// </summary>
    public interface IClaimReadModelRepository
    {
        /// <summary>
        /// Retrieve a all claim read model by quoteid.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant.</param>
        /// <param name="quoteId">The quote ID.</param>
        /// <param name="filters">The filters to apply.</param>
        /// <returns>The matching claim read model, if any, otherwise null.</returns>
        IEnumerable<IClaimReadModelSummary> ListClaimsByQuoteId(Guid tenantId, Guid quoteId, EntityListFilters filters);

        /// <summary>
        /// Retrieve a claim read model by ID.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="id">The claim ID.</param>
        /// <returns>The matching claim read model, if any, otherwise null.</returns>
        ClaimReadModel? GetById(Guid tenantId, Guid id);

        /// <summary>
        /// Retrieve a claim read model summary by ID.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="id">The claim ID.</param>
        /// <returns>The matching claim read model, if any, otherwise null.</returns>
        IClaimReadModelSummary? GetSummaryById(Guid tenantId, Guid id);

        /// <summary>
        /// Gets the details of a claim.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="claimId">The claim ID.</param>
        /// <returns>The claim details.</returns>
        IClaimReadModelDetails? GetClaimDetails(Guid tenantId, Guid claimId);

        /// <summary>
        /// Retrieve a claim read model by reference number.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="productId">The product ID.</param>
        /// <param name="environment">The environment.</param>
        /// <param name="referenceNumber">The reference number to check for.</param>
        /// <returns>The claim read model instance.</returns>
        IClaimReadModel? GetByClaimNumber(
            Guid tenantId, Guid productId, DeploymentEnvironment environment, string referenceNumber);

        /// <summary>
        /// Gets an IQueryable that defines claims based on tenant ID, and customer ID.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant.</param>
        /// <param name="organisationId">The ID of the organisation.</param>
        /// <param name="customerId">The ID of the customer.</param>
        /// <returns>Returns a new <see cref="IQueryable"/> of <see cref="ClaimReadModel"/>.</returns>
        IQueryable<ClaimReadModel> GetClaimsForCustomerId(Guid tenantId, Guid organisationId, Guid customerId);

        /// <summary>
        /// Gets an IQueryable that defines claims based on tenant ID and organisation ID.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant.</param>
        /// <param name="organisationId">The ID of the organisation.</param>
        /// <returns>Returns a new <see cref="IQueryable"/> of <see cref="ClaimReadModel"/>.</returns>
        IQueryable<ClaimReadModel> GetClaimsForTenantIdAndOrganisationId(Guid tenantId, Guid organisationId);

        /// <summary>
        /// Checks if any claim exists for the specified customer.
        /// </summary>
        /// <param name="filters">The filters to apply.</param>
        /// <param name="excludedClaimIds">The claim Ids to exclude on the search.</param>
        /// <returns>True if claims exist, false otherwise.</returns>
        bool HasClaimsForCustomer(EntityListFilters filters, IEnumerable<Guid> excludedClaimIds);

        /// <summary>
        /// Gets the list of all claims for a given tenant and environment that match a given filter..
        /// </summary>
        /// <param name="tenantId">The ID of the tenant.</param>
        /// <param name="filters">The filters to apply.</param>
        /// <returns>All the claimsm for a given tenant and environment.</returns>
        IEnumerable<IClaimReadModelSummary> ListClaims(Guid tenantId, EntityListFilters filters);

        /// <summary>
        /// Gets the list of all claims for a given tenant and environment that match a given filter without product
        /// details.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant.</param>
        /// <param name="filters">The filters to apply.</param>
        /// <returns>All the claimsm for a given tenant and environment.</returns>
        IEnumerable<IClaimReadModel> ListClaimsWithoutJoiningProducts(Guid tenantId, EntityListFilters filters);

        /// <summary>
        /// Gets the list of all claims for a given tenant whose lodged, settled, declined dates are not set
        /// and that match a given filters.
        /// This is for migration purposes.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant.</param>
        /// <param name="filters">The filters to apply.</param>
        /// <returns>All the claims.</returns>
        IEnumerable<ClaimReadModel> ListClaimsWithLodgeSettledDeclinedDatesNotSet(Guid tenantId, EntityListFilters filters);

        /// <summary>
        /// Gets the list of claims for a given customer.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant.</param>
        /// <param name="customerId">The ID of the user.</param>
        /// <param name="filters">The filters to apply.</param>
        /// <returns>All the claims belonging to the given customer.</returns>
        IEnumerable<IClaimReadModelSummary> ListAllClaimsByCustomer(Guid tenantId, Guid customerId, EntityListFilters filters);

        /// <summary>
        /// Gets the claims for a given policy.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant.</param>
        /// <param name="policyId">The ID of the policy (quote).</param>
        /// <param name="filters">The filters to apply.</param>
        /// <returns>All the claims belonging to the given customer.</returns>
        IEnumerable<IClaimReadModelSummary> ListClaimsByPolicy(Guid tenantId, Guid policyId, EntityListFilters filters);

        /// <summary>
        /// Gets the claims for a given policy without product details.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant.</param>
        /// <param name="policyId">The ID of the policy (quote).</param>
        /// <returns>All the claims belonging to the given policy.</returns>
        IEnumerable<ClaimReadModel> ListClaimsByPolicyWithoutJoiningProducts(Guid tenantId, Guid policyId);

        /// <summary>
        /// Determines whether claims exist for the given policy.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="policyId">The policy Id.</param>
        /// <returns>True if claims exist, false otherwise.</returns>
        bool HasClaimsByPolicyId(Guid tenantId, Guid policyId);

        /// <summary>
        /// Check if a claimNumber is in-use with other claims.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant the reference numbers are for.</param>
        /// <param name="productId">The ID of the product the reference numbers are for.</param>
        /// <param name="claimNumber">The claim Number.</param>
        /// <returns>if assigned to other claims.</returns>
        bool IsClaimNumberInUseByOtherClaim(Guid tenantId, Guid productId, string claimNumber);

        /// <summary>
        /// Gets the total claims amount for a given policy.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="productId">The product Id.</param>
        /// <param name="policyNumber">The policy number.</param>
        /// <returns>Total claims amount for the specified policy number.</returns>
        decimal GetTotalClaimsAmountByPolicyNumberInPastFiveYears(Guid tenantId, Guid productId, string policyNumber);

        /// <summary>
        /// Gets the claims for a given policy in the past 5 years.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="productId">The product Id.</param>
        /// <param name="policyNumber">The policy number.</param>
        /// <returns>The claims amount for the specified policy number.</returns>
        IEnumerable<IClaimReadModel> GetClaimsByPolicyNumberInPastFiveYears(
            Guid tenantId, Guid productId, string policyNumber);

        /// <summary>
        /// Gets a claim with related entities by claim id.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="environment">The deployment environment.</param>
        /// <param name="claimId">The claim ID.</param>
        /// <param name="relatedEntities">The list of related entities to include.</param>
        /// <returns>The claim with related entities.</returns>
        IClaimReadModelWithRelatedEntities? GetClaimWithRelatedEntities(
            Guid tenantId, DeploymentEnvironment? environment, Guid claimId, IEnumerable<string> relatedEntities);

        /// <summary>
        /// Gets a claim with related entities by claim reference.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="claimReference">The claim reference.</param>
        /// <param name="environment">The environment.</param>
        /// <param name="relatedEntities">The list of related entities to include.</param>
        /// <returns>The claim with related entities.</returns>
        IClaimReadModelWithRelatedEntities? GetClaimWithRelatedEntitiesByReference(
            Guid tenantId, string claimReference, DeploymentEnvironment? environment, IEnumerable<string> relatedEntities);

        /// <summary>
        /// Gets a claim with related entities by claim number.
        /// </summary>
        /// <param name="claimNumber">The claim number.</param>
        /// <param name="environment">The environment.</param>
        /// <param name="relatedEntities">The list of related entities to include.</param>
        /// <returns>The claim with related entities.</returns>
        IClaimReadModelWithRelatedEntities? GetClaimWithRelatedEntitiesByNumber(
            Guid tenantId, string claimNumber, DeploymentEnvironment? environment, IEnumerable<string> relatedEntities);

        /// <summary>
        /// Get the claim document content by document Id.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant.</param>
        /// <param name="documentId">The document Id.</param>
        /// <param name="claimOrClaimVersionId">The claim or the claim version Id.</param>
        /// <returns>The content of the document.</returns>
        IFileContentReadModel? GetDocumentContent(Guid tenantId, Guid documentId, Guid claimOrClaimVersionId);

        /// <summary>
        /// Method for creating IQueryable method that retrieve claims and its related entities.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="environment">The deployment environment.</param>
        /// <param name="relatedEntities">The related entities to retrieve.</param>
        /// <returns>IQueryable for claims.</returns>
        IQueryable<ClaimReadModelWithRelatedEntities> CreateQueryForClaimDetailsWithRelatedEntities(
            Guid tenantId,
            DeploymentEnvironment? environment,
            IEnumerable<string> relatedEntities);

        /// <summary>
        /// Gets all claims as queryable.
        /// </summary>
        /// <returns>A queryable of <see cref="IClaimReadModel"/>.</returns>
        IQueryable<IClaimReadModel> GetAllClaimsAsQueryable();

        /// <summary>
        /// Get all the ids for non nascent claims by filters.
        /// </summary>
        /// <param name="entityFilters">Instance of <see cref="EntityFilters"/>.</param>
        /// <returns>List of ids converted as string.</returns>
        List<Guid> GetAllClaimIdsByEntityFilters(EntityFilters entityFilters);

        IQueryable<ClaimReadModel> QueryAllClaims(Guid tenantId, EntityListFilters filters);

        /// <summary>
        /// Gets all claims within the tenant, matching a given filter.
        /// </summary>
        /// <param name="environment">The deployment environment.</param>
        /// <param name="tenantId">The ID of the tenant the request is for.</param>
        /// <param name="filters">Additional filters to apply.</param>
        /// <param name="relatedEntities">The list of related entities to include.</param>
        /// <returns>All the claims owned by a given user and matching a given filter.</returns>
        IEnumerable<IClaimReadModelWithRelatedEntities> GetClaimsWithRelatedEntities(
            Guid tenantId,
            DeploymentEnvironment environment,
            EntityListFilters filters,
            IEnumerable<string> relatedEntities);

        Task<IEnumerable<ClaimDashboardSummaryModel>> ListClaimsForPeriodicSummary(Guid tenantId, EntityListFilters entityFilters, CancellationToken cancellationToken);

        /// <summary>
        /// Generate Report for Data Source Claims
        /// </summary>
        /// <param name="tenantId">Filter claims by tenant id</param>
        /// <param name="organisationId">Filter claims by organisation id</param>
        /// <param name="productIds">Filter claims by list of product id</param>
        /// <param name="environment">Filter claims by environment</param>
        /// <param name="fromTimestamp">Filter claims based on creation date timestamp</param>
        /// <param name="toTimestamp">Filter claims based on creation date timestamp</param>
        /// <param name="includeTestData">If true, Include all claims test data</param>
        /// <returns>IEnumerable of IClaimReportItem</returns>
        IEnumerable<IClaimReportItem> GetClaimsDataForReports(
            Guid tenantId,
            Guid organisationId,
            IEnumerable<Guid> productIds,
            DeploymentEnvironment environment,
            Instant fromTimestamp,
            Instant toTimestamp,
            bool includeTestData);

        /// <summary>
        /// Get all the filtered Claims, already filtered before joining with other entity
        /// </summary>
        /// <param name="tenantId">Filter claims by tenant id</param>
        /// <param name="organisationId">Filter claims by organisation id</param>
        /// <param name="productIds">Filter claims by list of product id</param>
        /// <param name="environment">Filter claims by environment</param>
        /// <param name="fromTimestamp">Filter claims based on creation date timestamp</param>
        /// <param name="toTimestamp">Filter claims based on creation date timestamp</param>
        /// <param name="includeTestData">If true, Include all claims test data</param>
        /// <returns>IEnumerable of IClaimReadModel</returns>
        IEnumerable<IClaimReportItem> GetFilteredClaims(
            Guid tenantId,
            Guid organisationId,
            IEnumerable<Guid> productIds,
            DeploymentEnvironment environment,
            Instant fromTimestamp,
            Instant toTimestamp,
            bool includeTestData);
    }
}
