// <copyright file="PolicyDocumentAttacher.cs" company="uBind">
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
    /// This class is needed to support file attachment for Policy which can be configured in automations.json.
    /// Current functionality works like PolicyTransaction because it's using the same DocumentOwnerType
    /// so we can configure document attachments for policy OR policyTransaction.
    /// </summary>
    public class PolicyDocumentAttacher : IDocumentAttacher
    {
        private readonly IQuoteAggregateRepository quoteAggregateRepository;
        private readonly IFileContentRepository fileContentRepository;
        private readonly IClock clock;
        private SerialisedEntitySchemaObject.Policy? policy;

        public PolicyDocumentAttacher(
            IQuoteAggregateRepository quoteAggregateRepository,
            IFileContentRepository fileContentRepository,
            IClock clock)
        {
            this.quoteAggregateRepository = quoteAggregateRepository;
            this.fileContentRepository = fileContentRepository;
            this.clock = clock;
        }

        public bool CanAttach(SerialisedEntitySchemaObject.IEntity entity)
        {
            if (entity == null)
            {
                return false;
            }

            if (entity.GetType() == typeof(SerialisedEntitySchemaObject.Policy))
            {
                return true;
            }
            return false;
        }

        public void SetEntity(SerialisedEntitySchemaObject.IEntity entity)
        {
            this.policy = entity as SerialisedEntitySchemaObject.Policy;
        }

        public AggregateType GetAggregateType()
        {
            return AggregateType.Quote;
        }

        public Guid GetAggregateId()
        {
            if (this.policy == null)
            {
                throw new ErrorException(Errors.Automation.Provider.Entity.EntityNotFoundForAttachment("policy"));
            }

            return this.policy.AggregateId;
        }

        /// <inheritdoc/>
        public async Task AttachFiles(Guid tenantId, List<FileAttachmentInfo> attachments)
        {
            if (this.policy == null)
            {
                throw new ErrorException(Errors.Automation.Provider.Entity.EntityNotFoundForAttachment("policy"));
            }

            var aggregateId = this.policy.AggregateId;
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
                    .FirstOrDefault();
                if (policyTransaction == null)
                {
                    throw new NotFoundException(Errors.Policy.Transaction.NotFoundForPolicy(quoteAggregate.Policy.PolicyId));
                }

                var eventSequenceNumber = policyTransaction.EventSequenceNumber;
                quoteAggregate.AttachPolicyDocument(document, policyTransaction.Id, default, this.clock.Now());
            }

            await this.quoteAggregateRepository.Save(quoteAggregate);
        }
    }
}
