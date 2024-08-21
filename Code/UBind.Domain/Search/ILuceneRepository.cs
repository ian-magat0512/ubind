// <copyright file="ILuceneRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Search
{
    using System;
    using System.Collections.Generic;
    using NodaTime;

    /// <summary>
    /// Interface for quotes related search using Lucene.
    /// </summary>
    /// <typeparam name="TItemWriteModel">The type of the item model which we will be writing to the search index.</typeparam>
    /// <typeparam name="TItemSearchResultModel">The type of the item model which we'll be reading from the search index.</typeparam>
    /// <typeparam name="TItemReadModelFilters">The type of the item model which we'll be a read model filters.</typeparam>
    public interface ILuceneRepository<TItemWriteModel, TItemSearchResultModel, TItemReadModelFilters> : ISearchLuceneFacade
    {
        /// <summary>
        /// Get the entity index count between the given dates to compare against the database count for said entity.
        /// </summary>
        /// <returns>Return the count of the policy between dates provided.</returns>
        int GetEntityIndexCountBetweenDates(
            Tenant tenant,
            DeploymentEnvironment environment,
            Instant fromDateTime,
            Instant toDateTime);

        /// <summary>
        /// Multi field search for quote in lucene indexes.
        /// </summary>
        /// <returns>The lucene search hits.</returns>
        IEnumerable<TItemSearchResultModel> Search(Tenant tenant, DeploymentEnvironment environment, TItemReadModelFilters filters);

        /// <summary>
        /// Generate Lucene Indexes for quotes.
        /// </summary>
        /// <param name="items">The quotes for writing lucene indexes.</param>
        void AddItemsToIndex(Tenant tenant, DeploymentEnvironment environment, IEnumerable<TItemWriteModel> items);

        /// <summary>
        /// Search for the last updated lucene Index and get its LastUpdated date (by system or by user) field.
        /// </summary>
        /// <returns>The last updated lucene Index's Last Updated date field value in ticks.</returns>
        long? GetIndexLastUpdatedTicksSinceEpoch(Tenant tenant, DeploymentEnvironment environment);

        /// <summary>
        /// Delete quotes lucene documents by ids.
        /// </summary>
        /// <param name="itemIds">The the ids of the items to delete.</param>
        void DeleteItemsFromIndex(Tenant tenant, DeploymentEnvironment environment, IEnumerable<Guid> itemIds);

        /// <summary>
        /// Makes the regeneration index the live index.
        /// This is used during periodic regeneration. Once the regeneration index has been completed
        /// it moves the regeneration index over to the live index so it's the one that's now live.
        /// It also deletes any old lucene index directories that are no longer needed for this environment.
        /// </summary>
        void MakeRegenerationIndexTheLiveIndex(Tenant tenant, DeploymentEnvironment environment);

        /// <summary>
        /// Make sure the regeneration folder is empty before the regeneration of index will execute.
        /// Because there a issue when the regeneration is interrupt then the regeneration folder is created once it execute
        /// again the folder is not recreate for new regeneration folder so still exist on live folder
        /// and the rengeration index is not move on the live index directory.
        /// </summary>
        void MakeSureRegenerationFolderIsEmptyBeforeRegeneration(Tenant tenant, DeploymentEnvironment environment);

        /// <summary>
        /// Adds items to a temporary index used for regenerating the entire index.
        /// This is used during periodic regeneration tasks to generate the full index, before
        /// copying it over to replace the current index.
        /// </summary>
        /// <param name="items">The quotes for writing lucene indexes.</param>
        void AddItemsToRegenerationIndex(
            Tenant tenant, DeploymentEnvironment environment, IEnumerable<TItemWriteModel> items);
    }
}
