// <copyright file="GetFileAttachmentContentQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Queries.FileAttachment
{
    using System.Threading;
    using System.Threading.Tasks;
    using UBind.Domain;
    using UBind.Domain.Entities;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;

    public class GetFileAttachmentContentQueryHandler
        : IQueryHandler<GetFileAttachmentContentQuery, IFileContentReadModel>
    {
        private readonly IFileAttachmentRepository<QuoteFileAttachment> quoteFileAttachmentRepository;
        private readonly IFileAttachmentRepository<ClaimFileAttachment> claimFileAttachmentRepository;

        public GetFileAttachmentContentQueryHandler(
            IFileAttachmentRepository<QuoteFileAttachment> quoteFileAttachmentRepository,
            IFileAttachmentRepository<ClaimFileAttachment> claimFileAttachmentRepository)
        {
            this.quoteFileAttachmentRepository = quoteFileAttachmentRepository;
            this.claimFileAttachmentRepository = claimFileAttachmentRepository;
        }

        public Task<IFileContentReadModel> Handle(GetFileAttachmentContentQuery query, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var content = this.quoteFileAttachmentRepository.GetAttachmentContent(query.TenantId, query.AttachmentId).Value;
            if (content == null)
            {
                content = this.claimFileAttachmentRepository.GetAttachmentContent(query.TenantId, query.AttachmentId).Value;
            }

            return Task.FromResult(content);
        }
    }
}
