// <copyright file="IQuoteReadModelRepository.cs" company="uBind">
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
    using UBind.Domain.Search;

    /// <summary>
    /// For retrieving persisted quote details.
    /// </summary>
    public interface IQuoteReadModelRepository : IRepository
    {
        /// <summary>
        /// Gets an IQueryable that defines quotes based on tenant ID, organisation ID and owner user ID.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant.</param>
        /// <param name="organisationId">The ID of the organisation.</param>
        /// <param name="ownerUserId">The ID of the owner user.</param>
        /// <returns>Returns a new <see cref="IQueryable"/> of <see cref="NewQuoteReadModel"/>.</returns>
        IQueryable<NewQuoteReadModel> GetQuotesForCustomerIdTenantIdAndOrganisationId(
            Guid tenantId, Guid organisationId, Guid ownerUserId);

        /// <summary>
        /// Gets an IQueryable that defines quotes based on tenant ID and organisation ID.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant.</param>
        /// <param name="organisationId">The ID of the organisation.</param>
        /// <returns>Returns a new <see cref="IQueryable"/> of <see cref="NewQuoteReadModel"/>.</returns>
        IQueryable<NewQuoteReadModel> GetQuotesForTenantIdAndOrganisationId(Guid tenantId, Guid organisationId);

        IQueryable<NewQuoteReadModel> GetQuotes(Guid tenantId, QuoteReadModelFilters filters);

        /// <summary>
        /// Gets all quotes within the tenant, matching a given filter.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant the request is for.</param>
        /// <param name="filters">Additional filters to apply.</param>
        /// <param name="relatedEntities">The list of related entities to include.</param>
        /// <returns>All the quotes owned by a given user and matching a given filter.</returns>
        IEnumerable<IQuoteReadModelWithRelatedEntities> GetQuotesWithRelatedEntities(
            Guid tenantId, QuoteReadModelFilters filters, IEnumerable<string> relatedEntities);

        /// <summary>
        /// Checks if any quote exists for the specified customer.
        /// </summary>
        /// <param name="filters">The filters to apply.</param>
        /// <param name="excludedQuoteIds">The quote Ids to exclude on the search.</param>
        /// <returns>True if quotes exist, false otherwise.</returns>
        bool HasQuotesForCustomer(QuoteReadModelFilters filters, IEnumerable<Guid> excludedQuoteIds);

        /// <summary>
        /// List all quotes of a tenant specifically for the organisation migration.
        /// </summary>
        /// <returns>List of quotes with its aggregate, policy and organisation id only.</returns>
        IEnumerable<OrganisationMigrationQuoteReadModel> ListQuotesForOrganisationMigration(Guid tenantId);

        /// <summary>
        /// Lists all quote Ids associated with the given policy.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="policyId">The policy Id.</param>
        /// <param name="environment">The deployment environment.</param>
        /// <returns>List of quote Ids.</returns>
        IEnumerable<Guid> ListQuoteIdsFromPolicy(Guid tenantId, Guid policyId, DeploymentEnvironment environment);

        /// <summary>
        /// Lists all quote Ids associated with the given customer.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="customerId">The customer Id.</param>
        /// <param name="environment">The deployment environment.</param>
        /// <returns>List of quote Ids.</returns>
        IEnumerable<Guid> ListQuoteIdsFromCustomer(Guid tenantId, Guid customerId, DeploymentEnvironment environment);

        /// <summary>
        /// List quotes in a given tenant that match given filters.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant.</param>
        /// <param name="filters">The filters to apply.</param>
        /// <returns>The set of summaries of matching quotes.</returns>
        IEnumerable<IQuoteReadModelSummary> ListQuotes(Guid tenantId, QuoteReadModelFilters filters);

        /// <summary>
        /// List quote summaries in a given tenant that match provided filters.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant.</param>
        /// <param name="filters">The filters to apply.</param>
        /// <returns>A collection of matching quote summaries of type <see cref="QuoteDashboardSummaryModel"/>.</returns>
        Task<IEnumerable<QuoteDashboardSummaryModel>> ListQuotesForPeriodicSummary(Guid tenantId, QuoteReadModelFilters filters, CancellationToken cancellationToken);

        /// <summary>
        /// Count quotes in a given tenant that match given filters.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant.</param>
        /// <param name="filters">The filters to apply.</param>
        /// <returns>The number of matching quotes.</returns>
        int CountQuotes(
            Guid tenantId, QuoteReadModelFilters filters);

        /// <summary>
        /// List quotes in a given tenant and evironment belonging to a given owner that match given filters.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="productId">The product ID.</param>
        /// <param name="environment">The environment.</param>
        /// <param name="filters">The filters to apply.</param>
        /// <returns>The set of summaries of matching quotes.</returns>
        IEnumerable<IQuoteReadModelSummary> GetQuotesByTenantAndProduct(
            Guid tenantId, Guid productId, DeploymentEnvironment environment, EntityListFilters filters);

        /// <summary>
        /// Gets the quote in raw form
        /// Used Only for permission checking, if you want more complete information, check
        /// GetQuoteSummary() or GetQuoteDetails().
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="quoteId">The Id of the quote to fetch.</param>
        /// <returns>The quote saved on the database.</returns>
        NewQuoteReadModel GetById(Guid tenantId, Guid? quoteId);

        /// <summary>
        /// Gets the ID of the product release associated with a given quote.
        /// </summary>
        Guid? GetProductReleaseId(Guid tenantId, Guid quoteId);

        /// <summary>
        /// Gets summary details of a given quote (for permission checks).
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="quoteId">The ID of the quote to fetch.</param>
        /// <returns>The quote details if found, otherwise null.</returns>
        IQuoteReadModelSummary GetQuoteSummary(Guid tenantId, Guid quoteId);

        /// <summary>
        /// Gets details of a given quote.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="quoteId">The ID of the quote to fetch.</param>
        /// <returns>The quote details if found, otherwise null.</returns>
        IQuoteReadModelDetails GetQuoteDetails(Guid tenantId, Guid quoteId);

        /// <summary>
        /// Get quote details and related entities.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="environment">The deployment environment.</param>
        /// <param name="quoteId">The quote Id.</param>
        /// <param name="relatedEntities">The list of related entities to include.</param>
        /// <returns>The quote details with related entities.</returns>
        IQuoteReadModelWithRelatedEntities GetQuoteWithRelatedEntities(
            Guid tenantId, DeploymentEnvironment? environment, Guid quoteId, IEnumerable<string> relatedEntities);

        /// <summary>
        /// Get quote details and related entities.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="quoteReference">The quote reference.</param>
        /// <param name="environment">The deployment environment.</param>
        /// <param name="relatedEntities">The list of related entities to include.</param>
        /// <returns>The quote details with related entities.</returns>
        IQuoteReadModelWithRelatedEntities GetQuoteWithRelatedEntities(
            Guid tenantId, string quoteReference, DeploymentEnvironment? environment, IEnumerable<string> relatedEntities);

        /// <summary>
        /// Gets summary details of a given quote (for permission checks).
        /// </summary>
        /// <param name="tenantId">The ID of the tenant..</param>
        /// <param name="organisationId">The ID of the organisation.</param>
        /// <param name="productIds">The list of product ids.</param>
        /// <param name="environment">The deployment environment.</param>
        /// <returns>The quote details if found, otherwise null.</returns>
        IEnumerable<IQuoteReadModelSummary> GetQuotesByListOfProducts(
            Guid tenantId, Guid organisationId, IEnumerable<Guid> productIds, DeploymentEnvironment environment);

        /// <summary>
        /// Gets quote data specifically for reports.
        /// </summary>
        /// <param name="tenantId">The tenant ID to filter from.</param>
        /// <param name="organisationId">The organisation ID to filter from.</param>
        /// <param name="productIds">The list of product ids to filter from.</param>
        /// <param name="environment">The environment to filter from.</param>
        /// <param name="fromTimestamp">The from date.</param>
        /// <param name="toTimestamp">The to date.</param>
        /// <param name="includeTestData">The value indicating whether the data is for testing.</param>
        /// <returns>The report records retrieved.</returns>
        IEnumerable<IQuoteReportItem> GetQuoteDataForReports(
            Guid tenantId,
            Guid organisationId,
            IEnumerable<Guid> productIds,
            DeploymentEnvironment environment,
            Instant fromTimestamp,
            Instant toTimestamp,
            bool includeTestData);

        /// <summary>
        /// Get the quote counts between dates so we can compare it to quote lucene index
        /// if there a missing quote on the lucene index.
        /// </summary>
        /// <param name="fromDateTime">The from datetime instant.</param>
        /// <param name="toDateTime">The to datetime instant.</param>
        /// <returns>Return the counts of quote between the dates.</returns>
        int GetQuoteCountBetweenDates(
            Guid tenantId,
            DeploymentEnvironment environment,
            Instant fromDateTime,
            Instant toDateTime);

        /// <summary>
        /// Get the quote models needed for writing a lucene document.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="environment">The environment.</param>
        /// <param name="filters">The filters to apply.</param>
        /// <param name="searchIndexLastUpdatedTicksSinceEpoch">The last updated search index's last updated date in ticks. </param>
        /// <returns>The set of summaries of matching quotes.</returns>
        IEnumerable<IQuoteSearchIndexWriteModel> GetQuotesForSearchIndexCreation(
            Guid tenantId,
            DeploymentEnvironment environment,
            EntityListFilters filters,
            long? searchIndexLastUpdatedTicksSinceEpoch = null);

        /// <summary>
        /// Gets the IDs of quotes that are incomplete for a given tenant and product.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="productId">The product ID.</param>
        /// <returns>The IDs of  matching quotes.</returns>
        IEnumerable<(Guid QuoteAggregateId, Guid QuoteId)> GetIncompleteQuotesIds(Guid tenantId, Guid productId);

        /// <summary>
        /// Gets the IDs of quotes that don't have expiry dates for a given tenant and product.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="productId">The product ID.</param>
        /// <returns>The set of summaries of matching quotes.</returns>
        IEnumerable<(Guid QuoteAggregateId, Guid QuoteId)> GetIncompleteQuoteIdsWithoutExpiryDates(
            Guid tenantId, Guid productId);

        /// <summary>
        /// Gets the quote aggregate ID for a given quote Id.
        /// </summary>
        /// <param name="quoteId">The quote ID.</param>
        /// <returns>The QuoteAggregate ID if found, otherwise null.</returns>
        Guid? GetQuoteAggregateId(Guid quoteId);

        /// <summary>
        /// Method for creating IQueryable method that retrieve quotes and its related entities.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="relatedEntities">The related entities to retrieve.</param>
        /// <param name="environment">The deployment environment.</param>
        /// <returns>IQueryable for quotes.</returns>
        IQueryable<QuoteReadModelWithRelatedEntities> CreateQueryForQuoteDetailsWithRelatedEntities(
            Guid tenantId, DeploymentEnvironment? environment, IEnumerable<string> relatedEntities);

        /// <summary>
        /// Gets all quote ids converted as string by filters and type.
        /// </summary>
        /// <param name="entityFilters">Instance of <see cref="EntityFilters"/>.</param>
        /// <param name="quoteType"><see cref="QuoteType"/> (optional).</param>
        /// <returns>Collection of quote id converted to string.</returns>
        List<Guid> GetQuoteIdsByEntityFilters(EntityFilters entityFilters, QuoteType? quoteType = null);

        /// <summary>
        /// Retrieves a collection of quotes that should be expiring based on the set expiry times of the product
        /// it's for.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant.</param>
        /// <returns>A collection of expiring quotes.</returns>
        /// <remarks>This only returns the latest 40 records, if there are more than 40 records to be updated.</remarks>
        IEnumerable<NewQuoteReadModel> GetQuotesThatAreRecentlyExpired(Guid tenantId);

        NewQuoteReadModel? GetLatestQuoteOfTypeForPolicy(Guid tenantId, Guid policyId, QuoteType quoteType);

        Guid? GetQuoteIdByReferenceNumber(Guid tenantId, DeploymentEnvironment environment, string referenceNumber);

        /// <summary>
        /// Gets the list of aggregate IDs associated with a specific product release.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant the quote belongs to.</param>
        /// <param name="productReleaseId">The ID of the product release.</param>
        /// <param name="environment">The environment associated with the quote.</param>
        /// <returns>The list of aggregate IDs associated with the specified product release; otherwise, an empty list.</returns>
        IEnumerable<Guid> GetQuoteAggregateIdsByProductReleaseId(Guid tenantId, Guid productReleaseId, DeploymentEnvironment environment);

        /// <summary>
        /// Gets the list of aggregate IDs that are not associated with a product release.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant the quote belongs to.</param>
        /// <param name="productId">The ID of the product.</param>
        /// <param name="environment">The environment associated with the quote.</param>
        /// <returns>The list of unassociated aggregate IDs; otherwise, an empty list.</returns>
        IEnumerable<Guid> GetUnassociatedQuoteAggregateIds(Guid tenantId, Guid productId, DeploymentEnvironment environment);
    }
}
