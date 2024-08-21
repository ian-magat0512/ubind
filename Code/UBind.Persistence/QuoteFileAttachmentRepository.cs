// <copyright file="QuoteFileAttachmentRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence
{
    using System;
    using System.Linq;
    using CSharpFunctionalExtensions;
    using UBind.Domain;
    using UBind.Domain.Entities;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Repositories;

    /// <summary>
    /// Repository for retrieving file attachments for a quote.
    /// </summary>
    public class QuoteFileAttachmentRepository : IFileAttachmentRepository<QuoteFileAttachment>, IQuoteFileAttachmentRepository
    {
        private readonly IUBindDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="QuoteFileAttachmentRepository"/> class.
        /// </summary>
        /// <param name="dbContext">The UBind database context.</param>
        public QuoteFileAttachmentRepository(
            IUBindDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        /// <inheritdoc/>
        public QuoteFileAttachment GetById(Guid id)
        {
            return this.dbContext.QuoteFileAttachments.SingleOrDefault(x => x.Id == id);
        }

        /// <inheritdoc/>
        public void UpdateTenantId(Guid tenantId, Guid id)
        {
            var readModel = this.dbContext.QuoteFileAttachments.SingleOrDefault(x => x.Id == id);

            if (readModel != null)
            {
                readModel.TenantId = tenantId;
            }
        }

        /// <inheritdoc/>
        public Maybe<IFileContentReadModel> GetAttachmentContent(Guid tenantId, Guid attachmentId)
        {
            var attachments = this.dbContext.QuoteFileAttachments
                .Where(fa => fa.TenantId == tenantId && fa.Id == attachmentId);
            var fileContents = attachments.Join(
                this.dbContext.FileContents,
                fa => fa.FileContentId,
                fc => fc.Id,
                (ca, fc) => new FileContentReadModel
                {
                    FileContent = fc.Content,
                    ContentType = ca.Type,
                    Name = ca.Name,
                });
            return fileContents.FirstOrDefault();
        }

        /// <inheritdoc/>
        public Maybe<IFileContentReadModel> GetAttachmentContent(Guid tenantId, Guid attachmentId, Guid quoteId)
        {
            var attachments = this.dbContext.QuoteFileAttachments
                .Where(fa => fa.TenantId == tenantId && fa.QuoteId == quoteId && fa.Id == attachmentId);
            if (!attachments.Any())
            {
                // Adjustment/Renewal creates a new Quote but keeps the QuoteFileAttachment Id
                // in the calculation result so we use Aggregate Id as a fallback mechanism.
                var aggregateId = this.dbContext.QuoteReadModels
                    .Where(q => q.TenantId == tenantId && q.Id == quoteId)
                    .Select(q => q.AggregateId)
                    .FirstOrDefault();
                attachments = this.dbContext.QuoteFileAttachments
                    .Join(this.dbContext.QuoteReadModels, qfa => qfa.QuoteId, q => q.Id, (qfa, q) => new { qfa, q })
                    .Where(x => x.qfa.TenantId == tenantId && x.qfa.Id == attachmentId && x.q.AggregateId == aggregateId)
                    .Select(x => x.qfa);
            }

            return this.GetFileContent(attachments);
        }

        /// <inheritdoc/>
        public void Insert(QuoteFileAttachment fileAttachment)
        {
            this.dbContext.QuoteFileAttachments.Add(fileAttachment);
        }

        /// <inheritdoc/>
        public void SaveChanges()
        {
            this.dbContext.SaveChanges();
        }

        public Maybe<IFileContentReadModel> GetAttachmentContent(Guid tenantId, Guid aggregateId, string fileName)
        {
            var attachments = this.dbContext.QuoteFileAttachments
                .Join(
                    this.dbContext.QuoteReadModels,
                    fa => fa.QuoteId,
                    q => q.Id,
                    (fa, q) => new { FileAttachment = fa, Quote = q })
                .Where(quotes => quotes.FileAttachment.TenantId == tenantId &&
                    quotes.Quote.AggregateId == aggregateId &&
                    quotes.FileAttachment.Name == fileName)
                .Select(quotes => quotes.FileAttachment);

            return this.GetFileContent(attachments);
        }

        /// <inheritdoc/>
        public IEnumerable<Guid> GetAttachmentIdsForQuote(Guid tenantId, Guid quoteId)
        {
            var query = this.dbContext.QuoteFileAttachments
                .Where(a => a.TenantId == tenantId)
                .Where(a => a.QuoteId == quoteId)
                .Select(a => a.Id);
            return query.ToList();
        }

        private Maybe<IFileContentReadModel> GetFileContent(IQueryable<QuoteFileAttachment> attachments)
        {
            var fileContents = attachments.Join(
                this.dbContext.FileContents,
                fa => new { fileContentId = fa.FileContentId },
                fc => new { fileContentId = fc.Id },
                (ca, fc) => new FileContentReadModel
                {
                    FileContent = fc.Content,
                    ContentType = ca.Type,
                    Name = ca.Name,
                });
            return fileContents.FirstOrDefault();
        }
    }
}
