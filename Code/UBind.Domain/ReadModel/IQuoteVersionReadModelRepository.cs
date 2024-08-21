// <copyright file="IQuoteVersionReadModelRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// For retrieving persisted quote version details.
    /// </summary>
    public interface IQuoteVersionReadModelRepository
    {
        QuoteVersionReadModel GetById(Guid tenantId, Guid quoteVersionId);

        /// <summary>
        /// Gets all versions of a given quote.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="quoteId">The ID of the quote.</param>
        /// <returns>All versions of a given quote.</returns>
        IEnumerable<IQuoteVersionReadModelSummary> GetDetailVersionsOfQuote(Guid tenantId, Guid quoteId);

        /// <summary>
        /// Gets a specific version of a given quote.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="quoteId">The ID of the quote.</param>
        /// <param name="versionNumber">The number of the version to retrieve.</param>
        /// <returns>The specific version of the quote with the given number.</returns>
        IQuoteVersionReadModelDetails GetVersionDetailsByVersionNumber(Guid tenantId, Guid quoteId, int versionNumber);

        /// <summary>
        /// Gets a specific version of a given quote by Id.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="versionId">The ID of the quote version to retrieve.</param>
        /// <returns>The specific version of the quote with the given ID.</returns>
        IQuoteVersionReadModelDetails GetVersionDetailsById(Guid tenantId, Guid versionId);

        /// <summary>
        /// Get quote details and related entities.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="environment">The deployment environment.</param>
        /// <param name="quoteVersionId">The quote version Id.</param>
        /// <param name="relatedEntities">The list of related entities to include.</param>
        /// <returns>The quote details with related entities.</returns>
        IQuoteVersionReadModelWithRelatedEntities GetQuoteVersionWithRelatedEntities(
            Guid tenantId, DeploymentEnvironment? environment, Guid quoteVersionId, IEnumerable<string> relatedEntities);

        /// <summary>
        /// Get quote details and related entities.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="environment">The deployment environment.</param>
        /// <param name="quoteId">The quote Id.</param>
        /// <param name="versionNumber">The quote version number.</param>
        /// <param name="relatedEntities">The list of related entities to include.</param>
        /// <returns>The quote details with related entities.</returns>
        IQuoteVersionReadModelWithRelatedEntities GetQuoteVersionWithRelatedEntities(
            Guid tenantId,
            DeploymentEnvironment? environment,
            Guid quoteId,
            int versionNumber,
            IEnumerable<string> relatedEntities);

        /// <summary>
        /// Get quote details and related entities.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="quoteReference">The quote reference.</param>
        /// <param name="environment">The deployment environment.</param>
        /// <param name="versionNumber">The quote version number.</param>
        /// <param name="relatedEntities">The list of related entities to include.</param>
        /// <returns>The quote details with related entities.</returns>
        IQuoteVersionReadModelWithRelatedEntities GetQuoteVersionWithRelatedEntities(
            Guid tenantId,
            string quoteReference,
            DeploymentEnvironment? environment,
            int versionNumber,
            IEnumerable<string> relatedEntities);

        /// <summary>
        /// Method for creating IQueryable method that retrieve quote versions and its related entities.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="environment">The deployment environment.</param>
        /// <param name="relatedEntities">The related entities to retrieve.</param>
        /// <returns>IQueryable for quote versions.</returns>
        IQueryable<QuoteVersionReadModelWithRelatedEntities> CreateQueryForQuoteDetailsWithRelatedEntities(
            Guid tenantId, DeploymentEnvironment? environment, IEnumerable<string> relatedEntities);

        /// <summary>
        /// Gets the list of quote version ids converted as string by filters.
        /// </summary>
        /// <param name="entityFilters">Instance of <see cref="EntityFilters"/>.</param>
        /// <returns>List of version ids.</returns>
        List<Guid> GetQuoteVersionIdsByEntityFilters(EntityFilters entityFilters);
    }
}
