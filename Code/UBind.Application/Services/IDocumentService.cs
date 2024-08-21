// <copyright file="IDocumentService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Services
{
    using System;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// Portal service for handling quote email functionality.
    /// </summary>
    public interface IDocumentService
    {
        /// <summary>
        /// Retrieves the file content.
        /// </summary>
        /// <param name="documentId">the id of the document.</param>
        /// <param name="emailId">the id of the email the document is from.</param>
        /// <returns>the file content.</returns>
        IFileContentReadModel GetEmailDocument(Guid documentId, Guid emailId);
    }
}
