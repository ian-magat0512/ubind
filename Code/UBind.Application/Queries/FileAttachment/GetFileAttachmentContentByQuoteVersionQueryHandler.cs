// <copyright file="GetFileAttachmentContentByQuoteVersionQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.FileAttachment
{
    using System.Threading;
    using System.Threading.Tasks;
    using UBind.Domain;
    using UBind.Domain.Entities;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;

    public class GetFileAttachmentContentByQuoteVersionQueryHandler
        : IQueryHandler<GetFileAttachmentContentByQuoteVersionQuery, IFileContentReadModel>
    {
        private readonly IFileAttachmentRepository<QuoteFileAttachment> fileAttachmentRepository;

        public GetFileAttachmentContentByQuoteVersionQueryHandler(
            IFileAttachmentRepository<QuoteFileAttachment> fileAttachmentRepository)
        {
            this.fileAttachmentRepository = fileAttachmentRepository;
        }

        public Task<IFileContentReadModel> Handle(GetFileAttachmentContentByQuoteVersionQuery query, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var content = this.fileAttachmentRepository.GetAttachmentContent(query.TenantId, query.AttachmentId);
            return Task.FromResult(content.Value);
        }
    }
}
