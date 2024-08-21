// <copyright file="AttachFileToClaimCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Claim
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Claim;
    using UBind.Domain.Entities;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Claim;
    using UBind.Domain.ReadWriteModel;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services;

    public class AttachFileToClaimCommandHandler : ICommandHandler<AttachFileToClaimCommand, ClaimAttachmentReadModel>
    {
        private readonly IClaimAggregateRepository claimAggregateRepository;
        private readonly IFileAttachmentRepository<ClaimFileAttachment> claimAttachmentRepository;
        private readonly IFileContentRepository fileContentRepository;
        private readonly IClaimReadModelRepository claimReadModelRepository;
        private readonly IPolicyReadModelRepository policyReadModelRepository;
        private readonly IClaimService claimService;
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;

        public AttachFileToClaimCommandHandler(
            IClaimAggregateRepository claimAggregateRepository,
            IFileAttachmentRepository<ClaimFileAttachment> claimAttachmentRepository,
            IFileContentRepository fileContentRepository,
            IClaimReadModelRepository claimReadModelRepository,
            IPolicyReadModelRepository policyReadModelRepository,
            IClaimService claimService,
            IHttpContextPropertiesResolver currentUserIdentification)
        {
            this.claimAggregateRepository = claimAggregateRepository;
            this.claimAttachmentRepository = claimAttachmentRepository;
            this.fileContentRepository = fileContentRepository;
            this.claimReadModelRepository = claimReadModelRepository;
            this.policyReadModelRepository = policyReadModelRepository;
            this.claimService = claimService;
            this.httpContextPropertiesResolver = currentUserIdentification;
        }

        public async Task<ClaimAttachmentReadModel> Handle(AttachFileToClaimCommand request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var now = SystemClock.Instance.GetCurrentInstant();
            var claimAggregate = this.claimAggregateRepository.GetById(request.TenantId, request.ClaimId);
            var content = FileContent.CreateFromBase64String(request.TenantId, Guid.NewGuid(), request.FileContent);
            var fileSize = content.Size;
            var fileContentId = this.fileContentRepository.Insert(content);
            claimAggregate.AttachFile(
                fileContentId,
                request.FileName,
                request.FileType,
                fileSize,
                this.httpContextPropertiesResolver.PerformingUserId,
                now);

            await this.claimAggregateRepository.Save(claimAggregate);

            var attachment = new ClaimFileAttachment(
                request.FileName,
                request.FileType,
                fileSize,
                fileContentId,
                now);
            return ClaimAttachmentReadModel.CreateClaimAttachmentReadModel(request.ClaimId, attachment);
        }
    }
}
