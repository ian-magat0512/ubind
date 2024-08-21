// <copyright file="ApplicationQuoteFileAttachmentService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Services
{
    using System;
    using CSharpFunctionalExtensions;
    using UBind.Domain;
    using UBind.Domain.Entities;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Services;

    /// <inheritdoc />
    public class ApplicationQuoteFileAttachmentService : IApplicationQuoteFileAttachmentService
    {
        private readonly IFileAttachmentRepository<QuoteFileAttachment> quoteFileAttachmentRepository;
        private readonly IQuoteAggregateResolverService quoteAggregateResolverService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationQuoteFileAttachmentService"/> class.
        /// </summary>
        /// <param name="fileAttachmentRepository">The file attachment repository.</param>
        /// <param name="quoteAggregateResolverService">The service to resolve the quote aggregate for a given quote ID.</param>
        public ApplicationQuoteFileAttachmentService(
            IFileAttachmentRepository<QuoteFileAttachment> fileAttachmentRepository,
            IQuoteAggregateResolverService quoteAggregateResolverService)
        {
            this.quoteFileAttachmentRepository = fileAttachmentRepository;
            this.quoteAggregateResolverService = quoteAggregateResolverService;
        }

        public Maybe<IFileContentReadModel> GetFileAttachmentContent(Guid tenantId, Guid attachmentId)
        {
            return this.quoteFileAttachmentRepository.GetAttachmentContent(tenantId, attachmentId);
        }

        public Maybe<IFileContentReadModel> GetFileAttachmentContent(Guid tenantId, Guid aggregateId, string fileName)
        {
            return this.quoteFileAttachmentRepository.GetAttachmentContent(tenantId, aggregateId, fileName);
        }

        public Maybe<IFileContentReadModel> GetFileAttachmentContentForPolicy(Guid tenantId, Guid policyId, string fileName)
        {
            var quoteAggregate = this.quoteAggregateResolverService.GetQuoteAggregateForPolicy(tenantId, policyId);
            return this.quoteFileAttachmentRepository.GetAttachmentContent(quoteAggregate.TenantId, quoteAggregate.Id, fileName);
        }
    }
}
