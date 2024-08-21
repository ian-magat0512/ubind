// <copyright file="IFileContentRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.Repositories
{
    using System;
    using UBind.Domain.ReadWriteModel;

    /// <summary>
    /// Repository for storing portal settings.
    /// </summary>
    public interface IFileContentRepository
    {
        /// <summary>
        /// Insert new, non-existing file content into the repository.
        /// </summary>
        /// <param name="fileContent">New file content.</param>
        /// <returns>The Filecontent ID.</returns>
        Guid Insert(FileContent fileContent);

        /// <summary>
        /// Retrieves a file content that matches the specified id.
        /// </summary>
        /// <param name="id">The id of the file content.</param>
        /// <returns>The file content.</returns>
        FileContent? GetFileContentById(Guid id);

        /// <summary>
        /// Update file contents and hash codes.
        /// </summary>
        void UpdateFileContentsAndHashCodes();

        /// <summary>
        /// Retrieves a file content that matches the specified quote file attachment id.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="attachmentId">The quote attachment id of the file content.</param>
        /// <returns>The file content.</returns>
        FileContent? GetFileContentByQuoteFileAttachmentId(Guid tenantId, Guid attachmentId);

        /// <summary>
        /// Retrieves a file content that matches the quote file attachment or file content ID.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="quoteAttachmentOrFileContentId">The quote attachment or file content ID.</param>
        /// <returns>The attachment file content.</returns>
        FileContent? GetFileContent(Guid tenantId, Guid quoteAttachmentOrFileContentId);

        FileContent? GetFileContentByHashCode(Guid tenantId, string hashCode);

        bool HasFileContentWithHashCode(Guid tenantId, string hashCode);

        Guid? GetFileContentIdOrNullForHashCode(Guid tenantId, string hashCode);
    }
}
