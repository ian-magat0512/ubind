// <copyright file="ApplicationDocumentService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Services
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using MimeKit;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadWriteModel;
    using UBind.Domain.Repositories;

    /// <summary>
    /// Service for handling application documents.
    /// </summary>
    public class ApplicationDocumentService : IApplicationDocumentService
    {
        private readonly IFileContentRepository fileContentRepository;
        private readonly IQuoteAggregateRepository quoteAggregateRepository;
        private readonly IClock clock;
        private readonly ILogger<ApplicationDocumentService> logger;
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationDocumentService"/> class.
        /// </summary>
        /// <param name="fileContentRepository">Repository for file content.</param>
        /// <param name="quoteAggregateRepository">The quote aggregtae repository.</param>
        /// <param name="httpContextPropertiesResolver">The performing user resolver.</param>
        /// <param name="clock">A clock for obtaining the current time.</param>
        /// <param name="logger">A logger.</param>
        public ApplicationDocumentService(
            IFileContentRepository fileContentRepository,
            IQuoteAggregateRepository quoteAggregateRepository,
            IHttpContextPropertiesResolver httpContextPropertiesResolver,
            IClock clock,
            ILogger<ApplicationDocumentService> logger)
        {
            this.fileContentRepository = fileContentRepository;
            this.quoteAggregateRepository = quoteAggregateRepository;
            this.clock = clock;
            this.logger = logger;
            this.httpContextPropertiesResolver = httpContextPropertiesResolver;
        }

        /// <inheritdoc/>
        public async Task AttachDocumentsAsync(ApplicationEvent applicationEvent, params MimeEntity[] attachments)
        {
            var aggregate = this.quoteAggregateRepository.GetById(applicationEvent.Aggregate.TenantId, applicationEvent.Aggregate.Id);
            var policy = aggregate?.Policy ?? throw new ErrorException(Errors.Policy.NotFound(applicationEvent.Aggregate.Id));
            var policyTransactionId = policy.Transactions?
                .OrderByDescending(t => t.CreatedTimestamp).FirstOrDefault()?.Id
                ?? throw new ErrorException(Errors.Policy.Transaction.NotFoundForPolicy(applicationEvent.Aggregate.Id));

            foreach (var attachment in attachments.Where(a => a != null))
            {
                var fileContentId = this.CreateFileContentFromAttachment(aggregate.TenantId, attachment);
                var part = (MimePart)attachment;
                var document = new QuoteDocument(
                    attachment.ContentDisposition.FileName,
                    attachment.ContentType.MimeType,
                    part.Content.Stream.Length,
                    fileContentId,
                    this.clock.Now());

                if (applicationEvent.EventType.IsPolicyTransactionCreation())
                {
                    aggregate.AttachPolicyDocument(document, policyTransactionId, this.httpContextPropertiesResolver.PerformingUserId, this.clock.Now());
                }
                else if (applicationEvent.EventType == ApplicationEventType.QuoteVersionCreated)
                {
                    aggregate.AttachQuoteVersionDocument(document, applicationEvent.EventSequenceNumber, this.httpContextPropertiesResolver.PerformingUserId, this.clock.Now());
                }
                else
                {
                    aggregate.AttachQuoteDocument(document, applicationEvent.EventSequenceNumber, this.httpContextPropertiesResolver.PerformingUserId, this.clock.Now());
                }
            }

            await this.quoteAggregateRepository.Save(aggregate);
        }

        /// <summary>
        /// Gets the content for a given document.
        /// </summary>
        /// <param name="quoteDocument">The document.</param>
        /// <returns>A the file content for the document.</returns>
        public byte[] GetDocumentContent(QuoteDocument quoteDocument)
        {
            var content = this.fileContentRepository.GetFileContentById(quoteDocument.FileContentId);
            return content.Content;
        }

        private Guid CreateFileContentFromAttachment(Guid tenantId, MimeEntity attachment)
        {
            if (attachment == null)
            {
                throw new ArgumentNullException(nameof(attachment));
            }

            try
            {
                using (var stream = new MemoryStream())
                {
                    var mimePart = (MimePart)attachment;
                    mimePart.Content.DecodeTo(stream);
                    var byteContent = stream.ToArray();
                    if (byteContent.Length != mimePart.Content.Stream.Length)
                    {
                        throw new ErrorException(
                            Errors.Document.AttachmentContentLengthMismatch(mimePart, byteContent.Length));
                    }

                    var fileContent = FileContent.CreateFromBytes(tenantId, Guid.NewGuid(), byteContent);
                    var fileContentId = this.fileContentRepository.Insert(fileContent);
                    return fileContentId;
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(
                    $"Error while processing attachment: '{attachment.ContentDisposition?.FileName}' " +
                    $"for tenant {tenantId}. Error details: {ex}");
                throw;
            }
        }
    }
}
