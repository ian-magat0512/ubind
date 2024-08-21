// <copyright file="ClaimVersionDocumentAttacher.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

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
    /// This class is needed because we need a functionality to attach file(s) to a claim version entity.
    /// </summary>
    public class ClaimVersionDocumentAttacher : IDocumentAttacher
    {
        private readonly IClaimAggregateRepository claimAggregateRepository;
        private readonly IFileContentRepository fileContentRepository;
        private readonly IClock clock;
        private SerialisedEntitySchemaObject.ClaimVersion claimVersion;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClaimVersionDocumentAttacher"/> class.
        /// </summary>
        /// <param name="claimAggregateRepository">The quote aggregate repository.</param>
        /// <param name="fileContentRepository">The file content repository.</param>
        /// <param name="clock">The system clock.</param>
        public ClaimVersionDocumentAttacher(
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

            if (entity.GetType() == typeof(SerialisedEntitySchemaObject.ClaimVersion))
            {
                return true;
            }

            return false;
        }

        public void SetEntity(SerialisedEntitySchemaObject.IEntity entity)
        {
            this.claimVersion = entity as SerialisedEntitySchemaObject.ClaimVersion;
        }

        public AggregateType GetAggregateType()
        {
            return AggregateType.Claim;
        }

        public Guid GetAggregateId()
        {
            if (this.claimVersion == null)
            {
                throw new ErrorException(Errors.Automation.Provider.Entity.EntityNotFoundForAttachment("claim"));
            }

            return this.claimVersion.AggregateId;
        }

        /// <inheritdoc/>
        public async Task AttachFiles(Guid tenantId, List<FileAttachmentInfo> attachments)
        {
            if (this.claimVersion == null)
            {
                throw new ErrorException(Errors.Automation.Provider.Entity.EntityNotFoundForAttachment("claim"));
            }

            var aggregateId = this.claimVersion.AggregateId;
            var claimAggregate = this.claimAggregateRepository.GetById(tenantId, aggregateId);
            claimAggregate = EntityHelper.ThrowIfNotFound(claimAggregate, aggregateId, "claim");
            var claimVersionId = this.claimVersion.Id;
            foreach (var attachment in attachments)
            {
                var fileContent = FileContent.CreateFromBytes(
                    claimAggregate.TenantId, Guid.NewGuid(), attachment.File.Content);
                var fileContentId = this.fileContentRepository.Insert(fileContent);
                var document = new ClaimFileAttachment(
                    attachment.FileName.ToString(),
                    attachment.MimeType,
                    attachment.File.Content.Length,
                    fileContentId,
                    this.clock.Now());

                var @event = new ClaimVersionFileAttachedEvent(
                    claimAggregate.TenantId, aggregateId, claimVersionId, document, default, this.clock.Now());
                claimAggregate.ApplyNewEvent(@event);
            }

            await this.claimAggregateRepository.Save(claimAggregate);
        }
    }
}
