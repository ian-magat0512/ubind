// <copyright file="IPolicyReadModelRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.ReadModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NodaTime;
    using UBind.Domain.ReadModel.Policy;
    using UBind.Domain.Search;

    /// <summary>
    /// For retrieving persisted quote details.
    /// </summary>
    public interface IPolicyReadModelRepository : IRepository
    {
        /// <summary>
        /// Gets persisted policy based on its id.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant.</param>
        /// <param name="id">The ID of the policy.</param>
        /// <returns>The policy id found, otherwise null.</returns>
        PolicyReadModel GetById(Guid tenantId, Guid id);

        /// <summary>
        /// Get the list of up to 40 policies that have a recently become active state.
        /// this to compare the current timestamp so that will get the active states.
        /// this came from pending status that become active.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant.</param>
        /// <returns>Return the list of no more than 40 policies for state change.</returns>
        IEnumerable<PolicyReadModel> GetPoliciesThatHaveRecentlyBecomeActive(Guid tenantId);

        /// <summary>
        /// Get the list of up to 40 policies that have a recently become expired state.
        /// this to compare the current timestamp so that will get the expired states.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant.</param>
        /// <returns>Return the list no more than 40 policies for state change.</returns>
        IEnumerable<PolicyReadModel> GetPoliciesThatHaveRecentlyExpired(Guid tenantId);

        /// <summary>
        /// Get the list of up to 40 policies that have a recently become cancelled state.
        /// this to compare the current timestamp so that will get the cancelled states.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant.</param>
        /// <returns>Return the list of no more than 40 policies for state change.</returns>
        IEnumerable<PolicyReadModel> GetPoliciesThatHaveRecentlyCancelled(Guid tenantId);

        /// <summary>
        /// List policies in a given tenant and environment that match given filters.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant.</param>
        /// <param name="filters">The filters to apply.</param>
        /// <returns>The set of summaries of matching quotes.</returns>
        IEnumerable<IPolicyReadModelSummary> ListPolicies(
            Guid tenantId, PolicyReadModelFilters filters);

        /// <summary>
        /// List policy ids in a given tenant and environment that match given filters.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant.</param>
        /// <param name="filters">The filters to apply.</param>
        /// <returns>The set of policy ids of matching quotes.</returns>
        IEnumerable<Guid> ListPolicyIds(
            Guid tenantId,
            PolicyReadModelFilters filters);

        /// <summary>
        /// List policies for export in a given tenant and environment that match given filters.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="filters">The filters to apply.</param>
        /// <returns>The set of summaries of matching policies for export.</returns>
        IEnumerable<IPolicyReadModelSummary> ListPoliciesForExport(
            Guid tenantId, PolicyReadModelFilters filters);

        /// <summary>
        /// Lists all policy Ids associated with the given customer.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="customerId">The customer Id.</param>
        /// <param name="environment">The deployment environment.</param>
        /// <returns>List of policy Ids.</returns>
        IEnumerable<Guid> ListPolicyIdsFromCustomer(Guid tenantId, Guid customerId, DeploymentEnvironment environment);

        /// <summary>
        /// Checks if any policy exists for the specified customer.
        /// </summary>
        /// <param name="filters">The filters to apply.</param>
        /// <param name="excludedPolicyIds">The policy Ids to exclude on the search.</param>
        /// <returns>True if policies exist, false otherwise.</returns>
        bool HasPoliciesForCustomer(PolicyReadModelFilters filters, IEnumerable<Guid> excludedPolicyIds);

        /// <summary>
        /// Checks if the specified policy ID exists.
        /// </summary>
        /// <param name="policyId">The ID of the policy to search.</param>
        /// <returns>True if the policy exist, false otherwise.</returns>
        bool HasPolicy(Guid policyId);

        /// <summary>
        /// Checks if the specified policy ID exists.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="policyId">The ID of the policy to search.</param>
        /// <returns>True if the policy exist, false otherwise.</returns>
        bool HasPolicyForTenant(Guid tenantId, Guid policyId);

        /// <summary>
        /// Gets details of a given policy.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant.</param>
        /// <param name="policyId">The ID of the quote whose policy to fetch.</param>
        /// <returns>The policy details if found, otherwise null.</returns>
        IPolicyReadModelDetails GetPolicyDetails(Guid tenantId, Guid policyId);

        /// <summary>
        /// Gets details of a given policy.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant.</param>
        /// <param name="quoteId">The ID of the quote whose details to fetch.</param>
        /// <returns>The policy details if found, otherwise null.</returns>
        IPolicyReadModelDetails GetPolicyTransactionDetails(Guid tenantId, Guid quoteId);

        /// <summary>
        /// Checks whether the policy is being issued or not.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="productId">The product ID.</param>
        /// <param name="environment">The environment.</param>
        /// <param name="policyNumber">The policy number to check for.</param>
        /// <returns>The policy read model.</returns>
        PolicyReadModel GetPolicyByNumber(
            Guid tenantId, Guid productId, DeploymentEnvironment environment, string policyNumber);

        /// <summary>
        /// Gets details with related entities of a given policy by policy id.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="environment">The deployment environment.</param>
        /// <param name="policyId">The policy id.</param>
        /// <param name="relatedEntities">The list of related entities to include.</param>
        /// <returns>The policy details with related entities.</returns>
        IPolicyReadModelWithRelatedEntities GetPolicyWithRelatedEntities(
            Guid tenantId, DeploymentEnvironment? environment, Guid policyId, IEnumerable<string> relatedEntities);

        /// <summary>
        /// Gets details with related entities of a given policy by policy number.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="productId">The product Id.</param>
        /// <param name="environment">The environment.</param>
        /// <param name="policyNumber">The policy number.</param>
        /// <param name="relatedEntities">The list of related entities to include.</param>
        /// <returns>The policy details with related entities.</returns>
        IPolicyReadModelWithRelatedEntities GetPolicyWithRelatedEntities(
            Guid tenantId,
            Guid productId,
            DeploymentEnvironment? environment,
            string policyNumber,
            IEnumerable<string> relatedEntities);

        /// <summary>
        /// Gets the ID of the quote aggregate for the given policy ID.
        /// </summary>
        /// <param name="policyId">The ID of the policy.</param>
        /// <returns>The quote aggregate ID.</returns>
        Guid GetQuoteAggregateId(Guid policyId);

        /// <summary>
        /// Gets the quote ID for the given policy ID.
        /// </summary>
        /// <param name="policyId">The policy ID.</param>
        /// <returns>The quote ID.</returns>
        Guid? GetQuoteId(Guid policyId);

        /// <summary>
        /// Method for creating IQueryable method that retrieve policies and its related entities.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="environment">The deployment environment.</param>
        /// <param name="relatedEntities">The related entities to retrieve.</param>
        /// <returns>IQueryable for policies.</returns>
        IQueryable<IPolicyReadModelWithRelatedEntities> CreateQueryForPolicyDetailsWithRelatedEntities(
            Guid tenantId, DeploymentEnvironment? environment, IEnumerable<string> relatedEntities);

        /// <summary>
        /// Retrieve all policies that doesnt have aggregates.
        /// </summary>
        /// <param name="tenantId">The tenant id.</param>
        /// <param name="productId">The product id.</param>
        /// <returns>List of policy Ids.</returns>
        IEnumerable<Guid> ListPolicyIdsWithoutAggregates(Guid tenantId, Guid productId);

        /// <summary>
        ///  Get the policy count between dates so we can check if there are missing policies on the lucene index.
        /// </summary>
        /// <param name="fromTimestamp">The from datetime instant.</param>
        /// <param name="toTimestamp">The to datetime instant.</param>
        /// <returns>Return the counts of policy between the dates.</returns>
        int GetPolicyCountBetweenDates(
            Guid tenantId,
            DeploymentEnvironment environment,
            Instant fromTimestamp,
            Instant toTimestamp);

        /// <summary>
        /// Get the policy models needed for writing a lucene document.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="environment">The environment.</param>
        /// <param name="filters">The filters to apply.</param>
        /// <param name="lastUpdatedSearchIndexDateInTicks">The last updated search index's last updated date in ticks. </param>
        /// <returns>The set of summaries of matching policies.</returns>
        IEnumerable<IPolicySearchIndexWriteModel> GetPolicyForSearchIndexCreation(
            Guid tenantId,
            DeploymentEnvironment environment,
            EntityListFilters filters,
            long? lastUpdatedSearchIndexDateInTicks = null);

        /// <summary>
        /// Gets all valid policy ids by filters.
        /// </summary>
        /// <param name="entityFilters">Instance of <see cref="EntityFilters"/>.</param>
        /// <returns>List of converted to string policy ids.</returns>
        List<Guid> GetAllPolicyIdsWithQuoteByEntityFilters(EntityFilters entityFilters);

        /// <summary>
        /// Retrieve all policies of a tenant specifically for the organisation migration.
        /// </summary>
        /// <returns>List of policies with its quote, policy and organisation id only.</returns>
        IEnumerable<OrganisationMigrationPolicyReadModel> ListPoliciesForOrganisationMigration(Guid tenantId);

        /// <summary>
        /// Gets the QuoteAggregate ID for the given policy ID.
        /// </summary>
        Guid GetQuoteAggregateIdForPolicyId(Guid tenantId, Guid policyId);

        /// <summary>
        /// Gets query for quote from a given tenant and environment that match given filters.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant.</param>
        /// <param name="filters">The filters to apply.</param>
        /// <returns>A query for quotes.</returns>
        IQueryable<PolicyReadModel> QueryPolicyReadModels(Guid tenantId, PolicyReadModelFilters filters);

        /// <summary>
        /// Gets all policies within the tenant, matching a given filter.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant the request is for.</param>
        /// <param name="filters">Additional filters to apply.</param>
        /// <param name="relatedEntities">The list of related entities to include.</param>
        /// <returns>All the policies owned by a given user and matching a given filter.</returns>
        IEnumerable<IPolicyReadModelWithRelatedEntities> GetPoliciesWithRelatedEntities(
            Guid tenantId, PolicyReadModelFilters filters, IEnumerable<string> relatedEntities);

        Guid? GetProductReleaseIdForLatestPolicyPeriodTransaction(
            Guid tenantId, Guid productId, Guid policyId, DeploymentEnvironment environment);

        /// <summary>
        /// Gets the product release ID for a specific policy.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant.</param>
        /// <param name="policyId">The ID of the policy.</param>
        /// <returns>The product release ID for the specified policy.</returns>
        Guid? GetProductReleaseId(Guid tenantId, Guid policyId);

        IEnumerable<Guid> GetPolicyTransactionAggregateIdsByProductReleaseId(Guid tenantId, Guid productReleaseId, DeploymentEnvironment environment);

        /// <summary>
        /// Gets the list of aggregate IDs that are not associated with a product release.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant the quote belongs to.</param>
        /// <param name="productId">The ID of the product.</param>
        /// <param name="environment">The environment associated with the policy.</param>
        /// <returns>The list of unassociated aggregate IDs; otherwise, an empty list.</returns>
        IEnumerable<Guid> GetUnassociatedPolicyTransactionAggregateIds(Guid tenantId, Guid productId, DeploymentEnvironment environment);
    }
}
