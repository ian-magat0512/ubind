// <copyright file="DocumentService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Services
{
    using System;
    using System.Diagnostics.Contracts;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Repositories;

    /// <inheritdoc/>
    public class DocumentService : IDocumentService
    {
        private IDocumentFileRepository documentFileRepository;
        private IQuoteService quoteService;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentService"/> class.
        /// </summary>
        /// <param name="quoteService">the quote service.</param>
        /// <param name="documentFileRepository">The document repository.</param>
        public DocumentService(
            IQuoteService quoteService,
            IDocumentFileRepository documentFileRepository)
        {
            Contract.Assert(documentFileRepository != null);

            this.quoteService = quoteService;
            this.documentFileRepository = documentFileRepository;
        }

        /// <inheritdoc/>
        public IFileContentReadModel GetEmailDocument(Guid documentId, Guid emailId)
        {
            return this.documentFileRepository.GetFileContent(documentId);
        }
    }
}
