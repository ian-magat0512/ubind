// <copyright file="IThirdPartyDataSetsSearchService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Search.ThirdPartyDataSets
{
    using System;
    using System.Collections.Generic;
    using Lucene.Net.Index;

    /// <summary>
    /// Provides the contract to be used by third-party data sets search service.
    /// </summary>
    public interface IThirdPartyDataSetsSearchService
    {
        /// <summary>
        /// Create item index.
        /// </summary>
        /// <typeparam name="T">The object type to be index.</typeparam>
        /// <param name="indexName">The index name.</param>
        /// <param name="itemsToIndex">The list of items to be index.</param>
        /// <param name="actionAddDoc">The action hook to clean and add the item to the index.</param>
        void IndexItems<T>(string indexName, IEnumerable<T> itemsToIndex, Action<IndexWriter, T> actionAddDoc);

        /// <summary>
        /// Create item index to temporary location.
        /// </summary>
        /// <typeparam name="T">The object type to be index.</typeparam>
        /// <param name="indexName">The index name.</param>
        /// <param name="itemsToIndex">The list of items to be index.</param>
        /// <param name="actionAddDoc">The action hook to clean and add the item to the index..</param>
        void IndexItemsToTemporaryLocation<T>(string indexName, IEnumerable<T> itemsToIndex, Action<IndexWriter, T> actionAddDoc);

        /// <summary>
        /// Search single by field id.
        /// </summary>
        /// <typeparam name="T">The object type to be search.</typeparam>
        /// <param name="indexName">The index name.</param>
        /// <param name="fieldId">The field target to be search.</param>
        /// <param name="searchString">The search string to be search.</param>
        /// <returns>Returns a single, specific element of a sequence, or a default value if that element is not found.</returns>
        T SearchSingleOrDefault<T>(string indexName, string fieldId, string searchString);

        /// <summary>
        /// Search by field ids with maximum results.
        /// </summary>
        /// <typeparam name="T">The object type to be search.</typeparam>
        /// <param name="indexName">The index name.</param>
        /// <param name="searchFields">The fields target to be search.</param>
        /// <param name="searchString">The search string to be search.</param>
        /// <param name="maxResults">The maximum results.</param>
        /// <param name="cancellationToken">The request will canceled.</param>
        /// <returns>Returns a collections, specific element of a sequence, or a default value if that element is not found.</returns>
        IReadOnlyList<T> Search<T>(string indexName, string[] searchFields, string searchString, int maxResults, CancellationToken cancellationToken);

        /// <summary>
        /// Switch index from temporary path to default path.
        /// </summary>
        /// <param name="temporaryIndex">The temporary index name.</param>
        /// <param name="defaultIndex">The default index name.</param>
        void SwitchIndexFromTemporaryLocationToTargetIndexBasePath(string temporaryIndex, string defaultIndex);
    }
}
