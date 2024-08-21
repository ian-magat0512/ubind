// <copyright file="IApplicationQuoteFileAttachmentService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Services
{
    using System;
    using CSharpFunctionalExtensions;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// Service for file attachment.
    /// </summary>
    public interface IApplicationQuoteFileAttachmentService
    {
        /// <summary>
        /// Get file attachment content.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant.</param>
        /// <param name="attachmentId">The attachment Id.</param>
        /// <returns>File content.</returns>
        Maybe<IFileContentReadModel> GetFileAttachmentContent(Guid tenantId, Guid attachmentId);

        /// <summary>
        /// Gets the file attachment content for a policy based on the provided file name.
        /// </summary>
        /// <param name="tenantId">The unique identifier of the tenant.</param>
        /// <param name="aggregateId">The aggregate ID.</param>
        /// <param name="fileName">The name of the file attachment to retrieve.</param>
        /// <returns>The file content if found; otherwise, a Maybe with no value.</returns>
        Maybe<IFileContentReadModel> GetFileAttachmentContent(Guid tenantId, Guid aggregateId, string fileName);
    }
}
