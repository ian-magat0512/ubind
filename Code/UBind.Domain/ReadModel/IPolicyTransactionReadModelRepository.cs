// <copyright file="IPolicyTransactionReadModelRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.ReadModel.Policy;

    /// <summary>
    /// Repository for reading policy transactions.
    /// </summary>
    public interface IPolicyTransactionReadModelRepository : IRepository
    {
        PolicyTransaction GetById(Guid tenantId, Guid policyTransactionId);

        /// <summary>
        /// List policy transactions.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant.</param>
        /// <param name="organisationId">The Id of the organisation.</param>
        /// <param name="productIds">Enumerable of product ids.</param>
        /// <param name="environment">The deployment environment.</param>
        /// <param name="fromTimestamp">The from date.</param>
        /// <param name="toTimestamp">The to date.</param>
        /// <param name="includeTestData">The value indicating whether the data is for testing.</param>
        /// <param name="transactionFilter">The list of transaction.</param>
        /// <returns>List of policy transactions.</returns>
        IEnumerable<IPolicyTransactionReadModelSummary> GetPolicyTransactions(
            Guid tenantId,
            Guid organisationId,
            IEnumerable<Guid> productIds,
            DeploymentEnvironment environment,
            Instant fromTimestamp,
            Instant toTimestamp,
            bool includeTestData,
            IEnumerable<TransactionType> transactionFilter);

        /// <summary>
        /// Gets policy transaction with related entities  by id.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="policyTransactionId">The policy transaction id.</param>
        /// <param name="relatedEntities">The list of related entities to include.</param>
        /// <returns>The policy transaction details with releted entities.</returns>
        IPolicyTransactionReadModelWithRelatedEntities GetPolicyTransactionWithRelatedEntities(
            Guid tenantId,
            DeploymentEnvironment environment,
            Guid policyTransactionId,
            IEnumerable<string> relatedEntities);

        /// <summary>
        /// Method for creating IQueryable method that retrieve policy transactions and its related entities.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="environment">The deployment environment.</param>
        /// <param name="relatedEntities">The related entities to retrieve.</param>
        /// <returns>IQueryable for policy transactions.</returns>
        IQueryable<PolicyTransactionReadModelWithRelatedEntities> CreateQueryForPolicyTransactionDetailsWithRelatedEntities(
            Guid tenantId, DeploymentEnvironment environment, IEnumerable<string> relatedEntities);

        /// <summary>
        /// Gets the ids of policy transactions by filters and type.
        /// </summary>
        /// <param name="entityFilters">Instance of <see cref="EntityFilters"/>.</param>
        /// <param name="type">Type of transaction (optional).</param>
        /// <returns>List of ids converted as string.</returns>
        List<Guid> GetPolicyTransactionIdsByEntityFilters(EntityFilters entityFilters, Type type = null);

        /// <summary>
        /// Gets the list of policy transaction that match the given filters.
        /// </summary>
        /// <param name="tenantId">The id for the tenant the transactions are for.</param>
        /// <param name="filters">Instance of <see cref="PolicyReadModelFilters"/>.</param>
        /// <returns>List of policy transactions.</returns>
        Task<IEnumerable<PolicyTransactionDashboardSummaryModel>> ListPolicyTransactionForPeriodicSummary(Guid tenantId, PolicyReadModelFilters filters, CancellationToken cancellation);

        /// <summary>
        /// Gets the list of policy transaction with the given policy ID.
        /// </summary>
        /// <param name="policyId">The policy ID.</param>
        IQueryable<PolicyTransaction> GetByPolicyId(Guid policyId);
    }
}
