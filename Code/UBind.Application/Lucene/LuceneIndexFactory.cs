// <copyright file="LuceneIndexFactory.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Lucene
{
    using System.IO;
    using global::Lucene.Net.Analysis.Standard;
    using global::Lucene.Net.Index;
    using global::Lucene.Net.Store;
    using global::Lucene.Net.Util;

    /// <summary>
    /// A Lucene index factory class.
    /// </summary>
    public static class LuceneIndexFactory
    {
        private static LuceneVersion luceneVersion = LuceneVersion.LUCENE_48;

        /// <summary>
        /// A Lucene index factory class.
        /// </summary>
        /// <param name="luceneIndexDirectory">The index directory name.</param>
        /// <returns>Lucent index Directory.</returns>
        public static FSDirectory CreateIndexDirectory(string luceneIndexDirectory)
        {
            var directoryIndex = FSDirectory.Open(new DirectoryInfo(luceneIndexDirectory));

            if (IndexWriter.IsLocked(directoryIndex))
            {
                IndexWriter.Unlock(directoryIndex);
            }

            var lockFilePath = Path.Combine(luceneIndexDirectory, "write.lock");
            if (File.Exists(lockFilePath))
            {
                File.Delete(lockFilePath);
            }

            var analyzer = new StandardAnalyzer(luceneVersion);
            {
                var config = new IndexWriterConfig(luceneVersion, analyzer)
                {
                    OpenMode = OpenMode.CREATE_OR_APPEND,
                };

                using (var writer = new IndexWriter(directoryIndex, config))
                {
                    writer.DeleteAll(); // Remove older index entries
                }
            }

            return directoryIndex;
        }
    }
}
