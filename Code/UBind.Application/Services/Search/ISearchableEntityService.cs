// <copyright file="ISearchableEntityService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Services.Search
{
    using System.Collections.Generic;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Product;
    using UBind.Domain.Search;

    /// <summary>
    /// Service that allows searching indexes.
    /// </summary>
    /// <typeparam name="TItemSearchResultModel">The type of the item model which we'll be reading from the search index.</typeparam>
    /// <typeparam name="TItemReadModelFilters">The type of the item model which we'll be a read model filters.</typeparam>
    public interface ISearchableEntityService<TItemSearchResultModel, TItemReadModelFilters>
    {
        /// <summary>
        /// Finds items which need to be added to the search index, and then adds them.
        /// </summary>
        /// <param name="environment">The environment.</param>
        void ProcessIndexUpdates(DeploymentEnvironment environment, IEnumerable<Product> products, CancellationToken cancellationToken);

        /// <summary>
        /// Get the repository and lucene index counts so we can compare it if have differences.
        /// </summary>
        /// <param name="fromDateTime">The from datetime instant.</param>
        /// <param name="toDateTime">The to datetime instant.</param>
        /// <returns>Return the model for repository and lucene index counts.</returns>
        IEntityRepositoryAndLuceneIndexCountModel GetRepositoryAndLuceneIndexCount(
            Tenant tenant,
            DeploymentEnvironment environment,
            Instant fromDateTime,
            Instant toDateTime);

        /// <summary>
        /// Searches the search indexes/documents.
        /// </summary>
        /// <returns>The quotes.</returns>
        IEnumerable<TItemSearchResultModel> Search(Tenant tenant, DeploymentEnvironment environment, TItemReadModelFilters filters);

        /// <summary>
        /// Regenerates lucene indexes for a tenant in all environments.
        /// </summary>
        void RegenerateLuceneIndexesForTenant(Tenant tenant, IEnumerable<Product> products, CancellationToken cancellationToken);

        /// <summary>
        /// Regenerate the Lucene Index Search Indexes/Documents from DB.
        /// </summary>
        /// <param name="environment">The environment.</param>
        /// <param name="tenants">A list of tenants.</param>
        void RegenerateLuceneIndexes(
            DeploymentEnvironment environment,
            IEnumerable<Product> products,
            CancellationToken cancellationToken,
            IEnumerable<Tenant> tenants = null);
    }
}
