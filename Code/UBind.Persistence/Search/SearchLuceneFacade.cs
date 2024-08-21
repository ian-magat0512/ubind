// <copyright file="SearchLuceneFacade.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Search
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using Lucene.Net.Analysis;
    using Lucene.Net.Analysis.Standard;
    using Lucene.Net.Documents;
    using Lucene.Net.Index;
    using Lucene.Net.Search;
    using Lucene.Net.Store;
    using Lucene.Net.Util;
    using MoreLinq;
    using UBind.Domain.Enums;
    using UBind.Domain.Search;
    using Directory = System.IO.Directory;

    /// <summary>
    /// Lucene Facade.
    /// </summary>
    public abstract class SearchLuceneFacade : ISearchLuceneFacade
    {
        private const string Regeneration = "Regeneration";
        private const string Live = "Live";
        private readonly string baseIndexDirectory;
        private readonly string quotesDirectory;
        private readonly string policiesDirectory;

        /// <summary>
        /// Toggle for trace. For diagnostic purposes.
        /// </summary>
        private readonly bool traceOn;

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchLuceneFacade"/> class.
        /// </summary>
        /// <param name="luceneDirectoryConfiguration">The lucene Directory Configuration.</param>
        /// <param name="luceneIndexType">The type of the lucene entity.</param>
        public SearchLuceneFacade(ILuceneDirectoryConfiguration luceneDirectoryConfiguration, LuceneIndexType luceneIndexType)
        {
            this.traceOn = false;
            this.baseIndexDirectory = luceneDirectoryConfiguration.BaseLuceneDirectory;
            this.quotesDirectory = nameof(luceneDirectoryConfiguration.Quote);
            this.policiesDirectory = nameof(luceneDirectoryConfiguration.Policy);
            this.LuceneIndexType = luceneIndexType;
            this.LockFactory = new SimpleFSLockFactory();
            this.Analyzer = new StandardAnalyzer(LuceneVersion.LUCENE_48);
        }

        public virtual Analyzer Analyzer { get; set; }

        protected LuceneIndexType LuceneIndexType { get; }

        private LockFactory LockFactory { get; set; }

        /// <inheritdoc/>
        public DirectoryInfo GetLatestLiveIndexDirectory(string suffixPath)
        {
            var latestLiveDirectory = this.GetLatestLiveIndexPath(suffixPath);
            if (latestLiveDirectory == null)
            {
                return null;
            }

            return new DirectoryInfo($"{latestLiveDirectory}");
        }

        /// <inheritdoc/>
        public DirectoryInfo GetLiveIndexBaseDirectory(string suffixPath)
        {
            return new DirectoryInfo(this.GetLiveIndexBasePath(suffixPath));
        }

        /// <inheritdoc/>
        public DirectoryInfo CreateNewLuceneDirectory(string suffixPath)
        {
            string indexPath = this.GetLiveIndexBasePath(suffixPath);
            string directory = Path.Combine(indexPath, DateTime.Now.ToString("yyyyMMdd-HHmmss"));
            return Directory.CreateDirectory(directory);
        }

        /// <inheritdoc/>
        public DirectoryInfo CreateRegenerationIndexDirectory(string suffixPath)
        {
            string regenBasePath = this.GetRegenerationIndexBasePath(suffixPath);
            string regenDirectory = Path.Combine(regenBasePath, DateTime.Now.ToString("yyyyMMdd-HHmmss"));
            return Directory.CreateDirectory(regenDirectory);
        }

        /// <inheritdoc/>
        public DirectoryInfo GetLatestRegenerationIndexDirectory(string suffixPath)
        {
            var path = this.GetLatestRegenerationIndexPath(suffixPath);
            return path != null ? new DirectoryInfo(path) : null;
        }

        /// <inheritdoc/>
        public void DeleteOldLiveIndexDirectories(string suffixPath)
        {
            string latestIndexPath = this.GetLatestLiveIndexPath(suffixPath);
            if (latestIndexPath == null)
            {
                return;
            }

            string liveIndexPath = this.GetLiveIndexBasePath(suffixPath);
            if (Directory.Exists(liveIndexPath))
            {
                // Get all the directories in the current path.
                foreach (string indexPath in Directory.GetDirectories(liveIndexPath))
                {
                    if (indexPath != latestIndexPath)
                    {
                        DirectoryInfo dir = new DirectoryInfo(indexPath);
                        dir.Refresh();
                        dir.Delete(true);
                    }
                }
            }
        }

        /// <inheritdoc/>
        public void MakeRegenerationIndexTheLiveIndex(string suffixPath)
        {
            // e.g. "Live/tenant/environment/Quotes"
            var liveBaseDirectory = this.GetLiveIndexBaseDirectory(suffixPath);

            // e.g. "Regeneration/tenant/environment/Quotes/2021-09-01 13:22:32"
            var regenerationDirectory = this.GetLatestRegenerationIndexDirectory(suffixPath);

            // move it to new live directory
            var newLiveDirectory = new DirectoryInfo(Path.Combine(liveBaseDirectory.FullName, regenerationDirectory.Name));

            Directory.CreateDirectory(newLiveDirectory.Parent.FullName);

            if (!Directory.Exists(newLiveDirectory.FullName))
            {
                regenerationDirectory.MoveTo(newLiveDirectory.FullName);
            }

            // delete any old index directories
            this.DeleteOldLiveIndexDirectories(suffixPath);

            // delete any old regeneration indexes that might have been left over from interrupted
            this.DeleteOldRegenerationIndexDirectories(suffixPath);
        }

        /// <inheritdoc/>
        public void DeleteOldRegenerationIndexDirectories(string suffixPath)
        {
            string regenerationIndexPath = this.GetRegenerationIndexBasePath(suffixPath);
            if (Directory.Exists(regenerationIndexPath))
            {
                DirectoryInfo dir = new DirectoryInfo(regenerationIndexPath);
                dir.Refresh();
                dir.Delete(true);
            }
        }

        /// <inheritdoc/>
        public IndexWriter CreateIndexWriter(DirectoryInfo directoryInfo = null)
        {
            if (directoryInfo == null)
            {
                throw new Exception("IndexDirectory info not found.");
            }

            var fsDirectory = FSDirectory.Open(directoryInfo, this.LockFactory);
            if (IndexWriter.IsLocked(fsDirectory))
            {
                IndexWriter.Unlock(fsDirectory);
            }

            var lockFilePath = Path.Combine(directoryInfo.FullName, "write.lock");
            if (File.Exists(lockFilePath))
            {
                File.Delete(lockFilePath);
            }

            IndexWriterConfig config = new IndexWriterConfig(Lucene.Net.Util.LuceneVersion.LUCENE_48, this.Analyzer);
            return new IndexWriter(fsDirectory, config);
        }

        /// <inheritdoc/>
        public IndexSearcher CreateIndexSearcher(DirectoryInfo directoryInfo)
        {
            if (directoryInfo == null)
            {
                throw new Exception("IndexDirectory info not found.");
            }

            if (!directoryInfo.Exists)
            {
                directoryInfo.Create();
            }

            var fsDirectory = FSDirectory.Open(directoryInfo.FullName);
            DirectoryReader directoryReader = DirectoryReader.Open(fsDirectory);
            var indexSearcher = new IndexSearcher(directoryReader);
            this.Trace("Created IndexSearcher");
            return indexSearcher;
        }

        /// <summary>
        /// Trace issues via diagnostics.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="options">options.</param>
        protected void Trace(string format, params object[] options)
        {
            if (this.traceOn)
            {
                System.Diagnostics.Trace.WriteLine(string.Format(format, options));
            }
        }

        /// <summary>
        /// Update an index.
        /// </summary>
        /// <param name="doc">The lucene document for indexing.</param>
        /// <param name="uniqueTerm">The term that contains the unique id.</param>
        /// <param name="writer">The index writer.</param>
        /// <param name="boost">Boost value.</param>
        protected void AddOrUpdateDocumentToLuceneIndex(Document doc, Term uniqueTerm, IndexWriter writer = null, float? boost = null)
        {
            if (writer == null)
            {
                throw new Exception("IndexWriter not created.");
            }

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            // if boost specified, then assign it
            if (boost != null)
            {
                this.Trace("Boost=={0}", boost);
            }

            writer.UpdateDocument(uniqueTerm, doc);
            stopWatch.Stop();

            TimeSpan ts = stopWatch.Elapsed;

            // Format and display the TimeSpan value.
            string elapsedTime = string.Format(
                "{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours,
                ts.Minutes,
                ts.Seconds,
                ts.Milliseconds / 10);
            this.Trace("Document indexed in {0}", elapsedTime);
        }

        private string GetLatestLiveIndexPath(string suffixPath)
        {
            string indexPath = this.GetLiveIndexBasePath(suffixPath);
            return Directory.Exists(indexPath)
                ? Directory.GetDirectories(indexPath).OrderByDescending(d => d).FirstOrDefault()
                : null;
        }

        private string GetLatestRegenerationIndexPath(string suffixPath)
        {
            string indexPath = this.GetRegenerationIndexBasePath(suffixPath);
            return Directory.Exists(indexPath)
                ? Directory.GetDirectories(indexPath).OrderByDescending(d => d).FirstOrDefault()
                : null;
        }

        private string GetLiveIndexBasePath(string suffixPath)
        {
            return Path.Combine(this.baseIndexDirectory, Live, suffixPath, this.GetLuceneIndexDirectoryName());
        }

        private string GetRegenerationIndexBasePath(string suffixPath)
        {
            return Path.Combine(this.baseIndexDirectory, Regeneration, suffixPath, this.GetLuceneIndexDirectoryName());
        }

        private string GetLuceneIndexDirectoryName()
        {
            switch (this.LuceneIndexType)
            {
                case LuceneIndexType.Quote:
                    return this.quotesDirectory;
                case LuceneIndexType.Policy:
                    return this.policiesDirectory;
                default: return string.Empty;
            }
        }
    }
}
