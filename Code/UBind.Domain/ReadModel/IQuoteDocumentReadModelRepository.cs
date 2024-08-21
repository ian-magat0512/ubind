// <copyright file="IQuoteDocumentReadModelRepository.cs" company="uBind">
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
    /// Represents a repository for retrieving quote document details and related entities.
    /// </summary>
    public interface IQuoteDocumentReadModelRepository : IRepository
    {
        /// <summary>
        /// Gets the content of a given document.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant.</param>
        /// <param name="documentId">The ID of the quote the document belongs to.</param>
        /// <returns>Quote details.</returns>
        IFileContentReadModel GetDocumentContent(Guid tenantId, Guid documentId);

        /// <summary>
        /// Get document with related entities by id.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="environment">The deployment environment.</param>
        /// <param name="id">The document id.</param>
        /// <param name="relatedEntities">The list of related entities to include.</param>
        /// <returns>The document with related entities.</returns>
        IDocumentReadModelWithRelatedEntities GetDocumentWithRelatedEntities(
            Guid tenantId, DeploymentEnvironment environment, Guid id, IEnumerable<string> relatedEntities);

        /// <summary>
        /// Creates an IQueryable method for retrieving documents and their related entities.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="environment">The deployment environment.</param>
        /// <param name="relatedEntities">The related entities to retrieve.</param>
        /// <returns>IQueryable for documents.</returns>
        IQueryable<DocumentReadModelWithRelatedEntities> CreateQueryForDocumentDetailsWithRelatedEntities(
            Guid tenantId, DeploymentEnvironment environment, IEnumerable<string> relatedEntities);
    }
}
