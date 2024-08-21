// <copyright file="IApplicationDocumentService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Services
{
    using System.Threading.Tasks;
    using MimeKit;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Quote;

    /// <summary>
    /// Service for handling document-related services.
    /// </summary>
    public interface IApplicationDocumentService
    {
        /// <summary>
        /// Attach document to quote or policy.
        /// </summary>
        /// <param name="application">The application event.</param>
        /// <param name="attachments">The documents to attach.</param>
        /// <returns>An awaitable task.</returns>
        Task AttachDocumentsAsync(ApplicationEvent application, params MimeEntity[] attachments);

        /// <summary>
        /// Gets the content for a given document.
        /// </summary>
        /// <param name="quoteDocument">The document.</param>
        /// <returns>A the file content for the document.</returns>
        byte[] GetDocumentContent(QuoteDocument quoteDocument);
    }
}
