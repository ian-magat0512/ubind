// <copyright file="ISearchLuceneFacade.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Search
{
    using System.IO;
    using Lucene.Net.Index;
    using Lucene.Net.Search;

    /// <summary>
    /// Interface for Search Lucene facade.
    /// </summary>
    public interface ISearchLuceneFacade
    {
        /// <summary>
        /// Create lucene Index searcher.
        /// </summary>
        /// <param name="directoryInfo">The directory info.</param>
        /// <returns>The index Searcher.</returns>
        IndexSearcher CreateIndexSearcher(DirectoryInfo directoryInfo);

        /// <summary>
        /// Creates lucene indexwriter.
        /// </summary>
        /// <param name="directoryInfo">The directory info.</param>
        /// <returns>The writer.</returns>
        IndexWriter CreateIndexWriter(DirectoryInfo directoryInfo = null);

        /// <summary>
        /// Gets the latest live index directory.
        /// </summary>
        /// <param name="suffixPath">The specific folder.</param>
        /// <returns>The directory info.</returns>
        DirectoryInfo GetLatestLiveIndexDirectory(string suffixPath);

        /// <summary>
        /// Gets the base directory where indexes are stored.
        /// </summary>
        /// <param name="suffixPath">The specific folder.</param>
        /// <returns>The directory info.</returns>
        DirectoryInfo GetLiveIndexBaseDirectory(string suffixPath);

        /// <summary>
        /// Creates a new lucene index directory.
        /// </summary>
        /// <param name="suffixPath">The specific folder.</param>
        /// <returns>The directory info.</returns>
        DirectoryInfo CreateNewLuceneDirectory(string suffixPath);

        /// <summary>
        /// Creates and returns a directory for a fresh lucene index to be generated in.
        /// </summary>
        /// <param name="suffixPath">The specific folder.</param>
        /// <returns>The directory info.</returns>
        DirectoryInfo CreateRegenerationIndexDirectory(string suffixPath);

        /// <summary>
        /// Returns the latest regeneration index directory.
        /// </summary>
        /// <param name="suffixPath">The specific folder.</param>
        /// <returns>The directory info.</returns>
        DirectoryInfo GetLatestRegenerationIndexDirectory(string suffixPath);

        /// <summary>
        /// Deletes old live index directories.
        /// </summary>
        /// <param name="suffixPath">The specific folder.</param>
        void DeleteOldLiveIndexDirectories(string suffixPath);

        /// <summary>
        /// Deletes old regeneration index directories.
        /// </summary>
        /// <param name="suffixPath">The specific folder.</param>
        void DeleteOldRegenerationIndexDirectories(string suffixPath);

        /// <summary>
        /// Makes the regeneration index the live index.
        /// This is used during periodic regeneration. Once the regeneration index has been completed
        /// it moves the regeneration index over to the live index so it's the one that's now live.
        /// It also deletes any old lucene index directories that are no longer needed for this environment.
        /// </summary>
        /// <param name="suffixPath">The specific folder.</param>
        void MakeRegenerationIndexTheLiveIndex(string suffixPath);
    }
}
