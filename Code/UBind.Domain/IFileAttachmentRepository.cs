// <copyright file="IFileAttachmentRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using System;
    using CSharpFunctionalExtensions;
    using UBind.Domain.ReadModel;

    /// <summary>
    ///  Repository for retrieving file attachments.
    /// </summary>
    /// <typeparam name="TAttachment">The type of attachment being handled.</typeparam>
    public interface IFileAttachmentRepository<in TAttachment>
    {
        /// <summary>
        /// Gets the attachment content.
        /// </summary>
        /// <param name="fileAttachment">The file attachment to insert.</param>
        void Insert(TAttachment fileAttachment);

        /// <summary>
        /// Save Changes.
        /// </summary>
        void SaveChanges();

        /// <summary>
        /// Retrieves the content of an attachment.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant.</param>
        /// <param name="attachmentId">The quote's file attachment ID.</param>
        /// <returns>File Content Read Model.</returns>
        Maybe<IFileContentReadModel> GetAttachmentContent(Guid tenantId, Guid attachmentId);

        /// <summary>
        /// Gets the attachment content based on the provided file name, associated with a specific entity and tenant.
        /// </summary>
        /// <param name="tenantId">The unique identifier of the tenant.</param>
        /// <param name="aggregateId">The aggregate ID.</param>
        /// <param name="fileName">The name of the attachment to retrieve.</param>
        /// <returns>A file content representing the attachment content if found; otherwise, a Maybe with no value.</returns>
        Maybe<IFileContentReadModel> GetAttachmentContent(Guid tenantId, Guid aggregateId, string fileName);
    }
}
