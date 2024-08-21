// <copyright file="ILuceneDirectoryConfiguration.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Search
{
    using UBind.Domain.Search;

    /// <summary>
    /// The Interface for the Lucene Directory Configuration.
    /// </summary>
    public interface ILuceneDirectoryConfiguration
    {
        /// <summary>
        /// Gets or sets the base lucene Directory in the File system.
        /// </summary>
        string BaseLuceneDirectory { get; set; }

        /// <summary>
        /// Gets or sets the quotes lucene index generation configs in the File system.
        /// </summary>
        LuceneIndexGenerationConfig Quote { get; set; }

        /// <summary>
        /// Gets or sets the policies lucene index generation configs in the File system.
        /// </summary>
        LuceneIndexGenerationConfig Policy { get; set; }
    }
}
