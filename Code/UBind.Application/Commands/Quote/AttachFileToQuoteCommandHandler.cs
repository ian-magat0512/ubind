// <copyright file="AttachFileToQuoteCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Commands.Quote
{
    using System;
    using System.Threading.Tasks;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Entities;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services;
    using FileContent = Domain.ReadWriteModel.FileContent;

    /// <summary>
    /// Command handler to attach a file to a quote. This is used by the attachment operation from the quotes form.
    /// </summary>
    public class AttachFileToQuoteCommandHandler : ICommandHandler<AttachFileToQuoteCommand, QuoteFileAttachmentReadModel>
    {
        private readonly IClock clock;
        private readonly IQuoteAggregateRepository quoteAggregateRepository;
        private readonly IQuoteAggregateResolverService quoteAggregateResolverService;
        private readonly IFileContentRepository fileContentRepository;
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
        private readonly IAggregateLockingService aggregateLockingService;

        public AttachFileToQuoteCommandHandler(
            IClock clock,
            IQuoteAggregateRepository quoteAggregateRepository,
            IFileContentRepository fileContentRepository,
            IHttpContextPropertiesResolver httpContextPropertiesResolver,
            IQuoteAggregateResolverService quoteAggregateResolverService,
            IAggregateLockingService aggregateLockingService)
        {
            this.clock = clock;
            this.quoteAggregateRepository = quoteAggregateRepository;
            this.fileContentRepository = fileContentRepository;
            this.httpContextPropertiesResolver = httpContextPropertiesResolver;
            this.quoteAggregateResolverService = quoteAggregateResolverService;
            this.aggregateLockingService = aggregateLockingService;
        }

        /// <inheritdoc/>
        public async Task<QuoteFileAttachmentReadModel> Handle(AttachFileToQuoteCommand command, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var quoteAggregateId = this.quoteAggregateResolverService.GetQuoteAggregateIdForQuoteId(command.QuoteId);
            using (await this.aggregateLockingService.CreateLockOrThrow(command.TenantId, quoteAggregateId, AggregateType.Quote))
            {
                Guid attachmentId = command.AttachmentId is Guid id ? id : Guid.NewGuid();
                Instant now = this.clock.Now();
                QuoteAggregate? quoteAggregate = this.quoteAggregateRepository.GetById(command.TenantId, quoteAggregateId);
                quoteAggregate = EntityHelper.ThrowIfNotFound(quoteAggregate, quoteAggregateId, "quoteAggregate");
                var content = FileContent.CreateFromBase64String(quoteAggregate.TenantId, Guid.NewGuid(), command.FileData);
                var fileSize = content.Size;
                var fileContentId = this.fileContentRepository.Insert(content);
                var attachment = new QuoteFileAttachment(
                    quoteAggregate.TenantId, attachmentId, command.QuoteId, fileContentId, command.FileName, command.FileType, fileSize, now);
                quoteAggregate.AttachFile(
                    attachment.Id,
                    fileContentId,
                    command.FileName,
                    command.FileType,
                    fileSize,
                    this.httpContextPropertiesResolver.PerformingUserId,
                    now,
                    command.QuoteId);
                await this.quoteAggregateRepository.Save(quoteAggregate);
                return QuoteFileAttachmentReadModel.Create(attachment);
            }
        }
    }
}
