// <copyright file="SavedFileProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.FileHandling
{
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Threading.Tasks;
    using MimeKit;
    using UBind.Application.Export;
    using UBind.Application.Services;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions.Domain;

    /// <summary>
    /// Saved documents file provider.
    /// </summary>
    public class SavedFileProvider : IAttachmentProvider
    {
        private readonly string sourceObject;
        private readonly string sourceFileName;
        private readonly string outputFileName;
        private readonly EventExporterCondition condition;
        private IApplicationDocumentService documentService;

        /// <summary>
        /// Initializes a new instance of the <see cref="SavedFileProvider"/> class.
        /// </summary>
        /// <param name="sourceObject">The source object the document tracked against.</param>
        /// <param name="sourceFileName">The file name of the document.</param>
        /// <param name="outputFileName">The output file name to be used.</param>
        /// <param name="condition">The condition of the file whether to include in attachments or not.</param>
        /// <param name="documentService">The document service.</param>
        public SavedFileProvider(
            string sourceObject,
            string sourceFileName,
            string outputFileName,
            EventExporterCondition condition,
            IApplicationDocumentService documentService)
        {
            Contract.Assert(!string.IsNullOrEmpty(sourceObject));
            Contract.Assert(!string.IsNullOrEmpty(sourceFileName));
            Contract.Assert(!string.IsNullOrEmpty(outputFileName));

            this.sourceFileName = sourceFileName;
            this.sourceObject = sourceObject;
            this.outputFileName = outputFileName;
            this.condition = condition;
            this.documentService = documentService;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SavedFileProvider"/> class.
        /// </summary>
        /// <remarks>Force to use parameterized version of provider.</remarks>
        private SavedFileProvider()
        {
        }

        /// <inheritdoc/>
        public Task<MimeEntity> Invoke(ApplicationEvent applicationEvent)
        {
            var quoteAggregate = applicationEvent.Aggregate;
            var quote = quoteAggregate.GetQuoteOrThrow(applicationEvent.QuoteId);
            var document = this.sourceObject == nameof(Queries.Policy)
                ? quoteAggregate.GetPolicyDocument(this.sourceFileName)
                : quote.GetQuoteDocument(this.sourceFileName);
            byte[] content = null;

            if (document == null)
            {
                throw new ErrorException(Errors.File.NotFound(this.sourceFileName, this.sourceObject + " " + quoteAggregate.Id));
            }

            content = this.documentService.GetDocumentContent(document);
            var contentType = ContentType.Parse(document.Type);
            var attachment = MimeEntity.Load(contentType, new MemoryStream(content));
            attachment = attachment.ResolveAttachment(this.outputFileName, content);
            return Task.FromResult(attachment);
        }

        /// <inheritdoc/>
        public async Task<bool> IsIncluded(ApplicationEvent applicationEvent)
        {
            return this.condition != null
                ? await this.condition.Evaluate(applicationEvent)
                : true;
        }
    }
}
