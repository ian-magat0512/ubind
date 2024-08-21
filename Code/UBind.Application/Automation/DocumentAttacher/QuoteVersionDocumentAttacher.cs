﻿// <copyright file="QuoteVersionDocumentAttacher.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.DocumentAttacher
{
    using System;
    using System.Collections.Generic;
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
    using UBind.Domain.Services;
    using static UBind.Domain.Aggregates.Quote.QuoteAggregate;
    using SerialisedEntitySchemaObject = UBind.Domain.SerialisedEntitySchemaObject;

    /// <summary>
    /// This class is needed because we need a functionality to attach file(s) to a quote version entity.
    /// </summary>
    public class QuoteVersionDocumentAttacher : IDocumentAttacher
    {
        private readonly IQuoteAggregateRepository quoteAggregateRepository;
        private readonly IFileContentRepository fileContentRepository;
        private readonly IClock clock;
        private SerialisedEntitySchemaObject.QuoteVersion? quoteVersion;

        /// <summary>
        /// Initializes a new instance of the <see cref="QuoteVersionDocumentAttacher"/> class.
        /// </summary>
        /// <param name="quoteAggregateRepository">The quote aggregate repository.</param>
        /// <param name="fileContentRepository">The file content repository.</param>
        /// <param name="clock">The system clock.</param>
        public QuoteVersionDocumentAttacher(
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

            if (entity.GetType() == typeof(SerialisedEntitySchemaObject.QuoteVersion))
            {
                return true;
            }

            return false;
        }

        public void SetEntity(SerialisedEntitySchemaObject.IEntity entity)
        {
            this.quoteVersion = entity as SerialisedEntitySchemaObject.QuoteVersion;
        }

        public AggregateType GetAggregateType()
        {
            return AggregateType.Quote;
        }

        public Guid GetAggregateId()
        {
            if (this.quoteVersion == null)
            {
                throw new ErrorException(Errors.Automation.Provider.Entity.EntityNotFoundForAttachment("quote version"));
            }

            return this.quoteVersion.AggregateId;
        }

        /// <inheritdoc/>
        public async Task AttachFiles(Guid tenantId, List<FileAttachmentInfo> attachments)
        {
            if (this.quoteVersion == null)
            {
                throw new ErrorException(Errors.Automation.Provider.Entity.EntityNotFoundForAttachment("quote version"));
            }

            var quoteVersionId = this.quoteVersion.Id;
            var aggregateId = this.quoteVersion.AggregateId;
            var quoteId = Guid.Parse(this.quoteVersion.QuoteId);
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

                var @event = new QuoteVersionDocumentGeneratedEvent(
                    quoteAggregate.TenantId, aggregateId, quoteId, quoteVersionId, document, default, this.clock.Now());
                quoteAggregate.ApplyNewEvent(@event);
            }

            await this.quoteAggregateRepository.Save(quoteAggregate);
        }
    }
}
