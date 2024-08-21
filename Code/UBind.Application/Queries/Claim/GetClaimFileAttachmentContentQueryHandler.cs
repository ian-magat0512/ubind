// <copyright file="GetClaimFileAttachmentContentQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.Claim
{
    using System.Threading;
    using System.Threading.Tasks;
    using CSharpFunctionalExtensions;
    using UBind.Domain;
    using UBind.Domain.Entities;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;

    public class GetClaimFileAttachmentContentQueryHandler
        : IQueryHandler<GetClaimFileAttachmentContentQuery, Maybe<IFileContentReadModel>>
    {
        private readonly IFileAttachmentRepository<ClaimFileAttachment> claimAttachmentRepository;

        public GetClaimFileAttachmentContentQueryHandler(
            IFileAttachmentRepository<ClaimFileAttachment> claimAttachmentRepository)
        {
            this.claimAttachmentRepository = claimAttachmentRepository;
        }

        public Task<Maybe<IFileContentReadModel>> Handle(GetClaimFileAttachmentContentQuery query, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var attachmentContent = this.claimAttachmentRepository.GetAttachmentContent(
                query.TenantId, query.FileAttachmentId);
            return Task.FromResult(attachmentContent);
        }
    }
}
