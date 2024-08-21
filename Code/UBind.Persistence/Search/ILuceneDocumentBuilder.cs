// <copyright file="ILuceneDocumentBuilder.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Search
{
    using Lucene.Net.Documents;

    /// <summary>
    /// Builds Lucene Documents of the entity.
    /// </summary>
    public interface ILuceneDocumentBuilder<TItemSearchIndexWriteModel>
    {
        /// <summary>
        /// Builds a Lucene Document out of Write Model..
        /// </summary>
        /// <param name="model">The write Model.</param>
        /// <param name="currentDateTime">Current date time.</param>
        /// <returns>The lucene document.</returns>
        Document BuildLuceneDocument(TItemSearchIndexWriteModel model, long currentDateTime);
    }
}
