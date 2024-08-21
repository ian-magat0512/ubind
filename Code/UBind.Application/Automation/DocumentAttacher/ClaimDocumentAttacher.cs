// <copyright file="ClaimDocumentAttacher.cs" company="uBind">
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
    using UBind.Domain.Aggregates.Claim;
    using UBind.Domain.Entities;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.ReadWriteModel;
    using UBind.Domain.Repositories;
    using static UBind.Domain.Aggregates.Claim.ClaimAggregate;
    using SerialisedEntitySchemaObject = UBind.Domain.SerialisedEntitySchemaObject;

    /// <summary>
    /// This class is needed because we need a functionality to attach file(s) to a claim entity.
    /// </summary>
    public class ClaimDocumentAttacher : IDocumentAttacher
    {
        private readonly IClaimAggregateRepository claimAggregateRepository;
        private readonly IFileContentRepository fileContentRepository;
        private readonly IClock clock;
        private SerialisedEntitySchemaObject.Claim? claim;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClaimDocumentAttacher"/> class.
        /// </summary>
        /// <param name="claimAggregateRepository">The quote aggregate repository.</param>
        /// <param name="fileContentRepository">The file content repository.</param>
        /// <param name="clock">The system clock.</param>
        public ClaimDocumentAttacher(
            IClaimAggregateRepository claimAggregateRepository,
            IFileContentRepository fileContentRepository,
            IClock clock)
        {
            this.claimAggregateRepository = claimAggregateRepository;
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

            if (entity.GetType() == typeof(SerialisedEntitySchemaObject.Claim))
            {
                return true;
            }

            return false;
        }

        public void SetEntity(SerialisedEntitySchemaObject.IEntity entity)
        {
            this.claim = entity as SerialisedEntitySchemaObject.Claim;
        }

        public AggregateType GetAggregateType()
        {
            return AggregateType.Claim;
        }

        public Guid GetAggregateId()
        {
            if (this.claim == null)
            {
                throw new ErrorException(Errors.Automation.Provider.Entity.EntityNotFoundForAttachment("claim"));
            }

            return this.claim.AggregateId;
        }

        /// <inheritdoc/>
        public async Task AttachFiles(Guid tenantId, List<FileAttachmentInfo> attachments)
        {
            if (this.claim == null)
            {
                throw new ErrorException(Errors.Automation.Provider.Entity.EntityNotFoundForAttachment("claim"));
            }

            var aggregateId = this.claim.AggregateId;
            var claimAggregate = this.claimAggregateRepository.GetById(tenantId, aggregateId);
            claimAggregate = EntityHelper.ThrowIfNotFound(claimAggregate, aggregateId, "claim");
            foreach (var attachment in attachments)
            {
                var fileContent =
                    FileContent.CreateFromBytes(claimAggregate.TenantId, Guid.NewGuid(), attachment.File.Content);
                var fileContentId = this.fileContentRepository.Insert(fileContent);
                var document = new ClaimFileAttachment(
                    attachment.FileName.ToString(),
                    attachment.MimeType,
                    attachment.File.Content.Length,
                    fileContentId,
                    this.clock.Now());

                var @event = new ClaimFileAttachedEvent(
                    claimAggregate.TenantId, aggregateId, document, default, this.clock.Now());
                claimAggregate.ApplyNewEvent(@event);
            }

            await this.claimAggregateRepository.Save(claimAggregate);
        }
    }
}
