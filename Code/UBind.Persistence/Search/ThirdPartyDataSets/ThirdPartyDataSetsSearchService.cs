// <copyright file="ThirdPartyDataSetsSearchService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Search.ThirdPartyDataSets
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using global::Lucene.Net.Documents;
    using global::Lucene.Net.Search;
    using global::Lucene.Net.Store;
    using Lucene.Net.Analysis.Standard;
    using Lucene.Net.Index;
    using Lucene.Net.QueryParsers.Classic;
    using Lucene.Net.Util;
    using Microsoft.Extensions.Logging;
    using UBind.Domain.Helpers;
    using UBind.Domain.Repositories.FileSystem;
    using UBind.Domain.Search.ThirdPartyDataSets;
    using UBind.Domain.ThirdPartyDataSets;

    /// <inheritdoc/>
    public class ThirdPartyDataSetsSearchService : IThirdPartyDataSetsSearchService
    {
        private const string DefaultIndex = "GnafAddressLuceneIndex";

        /// <summary>
        /// Number of retries and how long each retry would delay to execute.
        /// </summary>
        private readonly TimeSpan[] timeSpanRetryAttempts;
        private readonly IThirdPartyDataSetsConfiguration thirdPartyDataSetsConfiguration;
        private readonly IFileSystemService fileSystemService;
        private readonly ILogger<ThirdPartyDataSetsSearchService> logger;
        private readonly DirectoryInfo directoryInfo;
        private readonly FSDirectory directory;

        /// <summary>
        /// Initializes a new instance of the <see cref="ThirdPartyDataSetsSearchService"/> class.
        /// </summary>
        /// <param name="thirdPartyDataSetsConfiguration">The third party data sets configuration.</param>
        /// <param name="fileSystemService">The file system service.</param>
        /// <param name="logger">The logging service.</param>
        /// </summary>
        public ThirdPartyDataSetsSearchService(
            IThirdPartyDataSetsConfiguration thirdPartyDataSetsConfiguration,
            IFileSystemService fileSystemService,
            ILogger<ThirdPartyDataSetsSearchService> logger)
        {
            this.thirdPartyDataSetsConfiguration = thirdPartyDataSetsConfiguration;
            this.fileSystemService = fileSystemService;
            this.logger = logger;
            this.timeSpanRetryAttempts = new[]
                {
                    TimeSpan.FromSeconds(3), // retry attempt 2 will take 10 sec delay to trigger
                    TimeSpan.FromSeconds(10), // retry attempt 2 will take 10 sec delay to trigger
                    TimeSpan.FromSeconds(30), // retry attempt 3 will take 30 sec delay to trigger
                };

            this.directoryInfo = new DirectoryInfo(Path.Combine(this.thirdPartyDataSetsConfiguration.IndexBasePath, DefaultIndex));
            this.directory = FSDirectory.Open(this.directoryInfo);
        }

        /// <inheritdoc />
        public T SearchSingleOrDefault<T>(string indexName, string fieldId, string searchValue)
        {
            var directoryInfo = new DirectoryInfo(
                Path.Combine(this.thirdPartyDataSetsConfiguration.IndexBasePath, indexName));
            using (var directory = FSDirectory.Open(directoryInfo))
            {
                using (var reader = DirectoryReader.Open(directory))
                {
                    using (var analyzer = new StandardAnalyzer(LuceneVersion.LUCENE_48))
                    {
                        var searcher = new IndexSearcher(reader);
                        var parser = new QueryParser(LuceneVersion.LUCENE_48, fieldId, analyzer)
                        {
                            AllowLeadingWildcard = true,
                        };
                        var query = parser.Parse(QueryParser.Escape(searchValue));
                        var hits = searcher.Search(query, 1).ScoreDocs.Select(doc => searcher.Doc(doc.Doc)).FirstOrDefault();
                        return hits == null ? default : this.ConvertObjectToEntity(hits, (T)Activator.CreateInstance(typeof(T)));
                    }
                }
            }
        }

        /// <inheritdoc />
        public IReadOnlyList<T> Search<T>(string indexName, string[] searchFields, string searchString, int maxResults, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (string.IsNullOrEmpty(searchString))
            {
                return new List<T>();
            }

            PerformanceHelper.ThrowIfCancellationRequested(cancellationToken);
            using (var reader = DirectoryReader.Open(this.directory))
            using (var analyzer = new StandardAnalyzer(LuceneVersion.LUCENE_48))
            {
                var searcher = new IndexSearcher(reader);

                PerformanceHelper.ThrowIfCancellationRequested(cancellationToken);
                var parser = new MultiFieldQueryParser(LuceneVersion.LUCENE_48, searchFields, analyzer);

                PerformanceHelper.ThrowIfCancellationRequested(cancellationToken);
                var query = parser.Parse(searchString.ToUpper());

                PerformanceHelper.ThrowIfCancellationRequested(cancellationToken);
                var hits = searcher
                    .Search(query, null, maxResults)
                    .ScoreDocs
                    .Select(doc => searcher.Doc(doc.Doc));
                return hits.Select(hit => this.ConvertObjectToEntity(hit, (T)Activator.CreateInstance(typeof(T)))).ToList();
            }
        }

        /// <inheritdoc/>
        public void IndexItems<T>(string indexName, IEnumerable<T> itemsToIndex, Action<IndexWriter, T> actionAddDoc)
        {
            var indexPath = Path.Combine(this.thirdPartyDataSetsConfiguration.IndexBasePath, indexName);
            this.IndexItemsToFolderLocation<T>(indexPath, itemsToIndex, actionAddDoc);
        }

        /// <inheritdoc/>
        public void IndexItemsToTemporaryLocation<T>(string indexName, IEnumerable<T> itemsToIndex, Action<IndexWriter, T> actionAddDoc)
        {
            var indexPath = Path.Combine(this.thirdPartyDataSetsConfiguration.IndexTemporaryPath, indexName);
            this.IndexItemsToFolderLocation<T>(indexPath, itemsToIndex, actionAddDoc);
        }

        /// <inheritdoc/>
        public void SwitchIndexFromTemporaryLocationToTargetIndexBasePath(string temporaryIndex, string defaultIndex)
        {
            var temporaryIndexPath = Path.Combine(this.thirdPartyDataSetsConfiguration.IndexTemporaryPath, temporaryIndex);
            var temporaryIndexToBasePath = Path.Combine(this.thirdPartyDataSetsConfiguration.IndexBasePath, defaultIndex);

            var defaultIndexPath = Path.Combine(this.thirdPartyDataSetsConfiguration.IndexBasePath, defaultIndex);
            var defaultIndexPathToTemporary = Path.Combine(this.thirdPartyDataSetsConfiguration.IndexTemporaryPath, defaultIndex);

            if (!this.fileSystemService.Directory.Exists(defaultIndexPath))
            {
                this.fileSystemService.Directory.CreateDirectory(defaultIndexPath);
            }

            if (!this.fileSystemService.Directory.Exists(temporaryIndexPath))
            {
                this.fileSystemService.Directory.CreateDirectory(temporaryIndexPath);
            }

            if (this.fileSystemService.Directory.Exists(defaultIndexPathToTemporary))
            {
                this.fileSystemService.Directory.Delete(defaultIndexPathToTemporary, true);
            }

            RetryPolicyHelper.Execute<Exception>(
               () =>
               {
                   this.logger.LogInformation($"Moving {defaultIndexPath} to {defaultIndexPathToTemporary}");
                   this.fileSystemService.Directory.Move(defaultIndexPath, defaultIndexPathToTemporary);
               }, this.timeSpanRetryAttempts);

            RetryPolicyHelper.Execute<Exception>(
               () =>
               {
                   this.logger.LogInformation($"Moving {temporaryIndexPath} to {temporaryIndexToBasePath}");
                   this.fileSystemService.Directory.Move(temporaryIndexPath, temporaryIndexToBasePath);
               }, this.timeSpanRetryAttempts);

            RetryPolicyHelper.Execute<Exception>(
                () =>
                {
                    this.logger.LogInformation($"Delete folder {defaultIndexPathToTemporary}");
                    this.fileSystemService.Directory.Delete(defaultIndexPathToTemporary, true);
                }, this.timeSpanRetryAttempts);
        }

        private void IndexItemsToFolderLocation<T>(string indexPath, IEnumerable<T> itemsToIndex, Action<IndexWriter, T> actionAddDoc)
        {
            using (var directoryIndex = FSDirectory.Open(new DirectoryInfo(indexPath)))
            {
                using (var analyzer = new StandardAnalyzer(LuceneVersion.LUCENE_48))
                {
                    var config = new IndexWriterConfig(LuceneVersion.LUCENE_48, analyzer)
                    {
                        OpenMode = OpenMode.CREATE_OR_APPEND,
                    };

                    using (var writer = new IndexWriter(directoryIndex, config))
                    {
                        foreach (var item in itemsToIndex)
                        {
                            actionAddDoc.Invoke(writer, item);
                        }

                        writer.Commit();
                    }
                }
            }
        }

        private T ConvertObjectToEntity<T>(Document objectToConvert, T entity)
        {
            if (objectToConvert == null || entity == null)
            {
                return default;
            }

            var entityType = entity.GetType();
            var entityProperties = entityType.GetProperties();

            foreach (var businessPropInfo in entityProperties)
            {
                businessPropInfo.SetValue(
                    entity,
                    Convert.ChangeType(objectToConvert.Get(businessPropInfo.Name), businessPropInfo.PropertyType),
                    null);
            }

            return entity;
        }
    }
}
