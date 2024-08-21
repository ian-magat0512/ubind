// <copyright file="PolicyTransactionDocumentAttacher.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.DocumentAttacher
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using NodaTime;
    using UBind.Application.Automation.Attachment;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.ReadWriteModel;
    using UBind.Domain.Repositories;
    using SerialisedEntitySchemaObject = UBind.Domain.SerialisedEntitySchemaObject;

    /// <summary>
    /// This class is needed because we need a functionality to attach file(s) to a policy transaction
    /// (e.g. renew, adjust, cancel) entity.
    /// </summary>
    public class PolicyTransactionDocumentAttacher : IDocumentAttacher
    {
        private readonly IQuoteAggregateRepository quoteAggregateRepository;
        private readonly IFileContentRepository fileContentRepository;
        private readonly IClock clock;
        private SerialisedEntitySchemaObject.PolicyTransaction? policyTransaction;

        /// <summary>
        /// Initializes a new instance of the <see cref="PolicyTransactionDocumentAttacher"/> class.
        /// </summary>
        /// <param name="quoteAggregateRepository">The quote aggregate repository.</param>
        /// <param name="fileContentRepository">The file content repository.</param>
        /// <param name="clock">The system clock.</param>
        public PolicyTransactionDocumentAttacher(
            IQuoteAggregateRepository quoteAggregateRepository,
            IFileContentRepository fileContentRepository,
            IClock clock)
        {
            this.quoteAggregateRepository = quoteAggregateRepository;
            this.fileContentRepository = fileContentRepository;
            this.clock = clock;
        }

        /// <inheritdoc/>
        public bool CanAttach(SerialisedEntitySchemaObject.IEntity entity)
        {
            if (entity == null)
            {
                return false;
            }

            if (entity.GetType() == typeof(SerialisedEntitySchemaObject.PolicyTransaction))
            {
                return true;
            }

            return false;
        }

        public void SetEntity(SerialisedEntitySchemaObject.IEntity entity)
        {
            this.policyTransaction = entity as SerialisedEntitySchemaObject.PolicyTransaction;
        }

        public AggregateType GetAggregateType()
        {
            return AggregateType.Quote;
        }

        public Guid GetAggregateId()
        {
            if (this.policyTransaction == null)
            {
                throw new ErrorException(Errors.Automation.Provider.Entity.EntityNotFoundForAttachment("policy transaction"));
            }

            return this.policyTransaction.AggregateId;
        }

        /// <inheritdoc/>
        public async Task AttachFiles(Guid tenantId, List<FileAttachmentInfo> attachments)
        {
            if (this.policyTransaction == null)
            {
                throw new ErrorException(Errors.Automation.Provider.Entity.EntityNotFoundForAttachment("policyTransaction"));
            }

            var aggregateId = this.policyTransaction.AggregateId;
            var policyTransactionId = this.policyTransaction.Id;
            var quoteAggregate = this.quoteAggregateRepository.GetById(tenantId, aggregateId);
            quoteAggregate = EntityHelper.ThrowIfNotFound(quoteAggregate, aggregateId, "quote aggregate");
            foreach (var attachment in attachments)
            {
                var fileContent = FileContent.CreateFromBytes(
                    quoteAggregate.TenantId, Guid.NewGuid(), attachment.File.Content);
                var fileContentId = this.fileContentRepository.Insert(fileContent);
                var document = new QuoteDocument(
                    attachment.FileName.ToString(),
                    attachment.MimeType,
                    attachment.File.Content.Length,
                    fileContentId,
                    this.clock.Now());

                if (quoteAggregate.Policy == null)
                {
                    throw new NotFoundException(Errors.Policy.PolicyNotFoundForAggregate(aggregateId));
                }

                var policyTransaction = quoteAggregate.Policy.Transactions
                    .FirstOrDefault(c => c.Id == policyTransactionId);
                if (policyTransaction == null)
                {
                    throw new NotFoundException(Errors.Policy.Transaction.NotFound(policyTransactionId));
                }

                var eventSequenceNumber = policyTransaction.EventSequenceNumber;
                quoteAggregate.AttachPolicyDocument(document, policyTransactionId, default, this.clock.Now());
            }

            await this.quoteAggregateRepository.Save(quoteAggregate);
        }
    }
}
