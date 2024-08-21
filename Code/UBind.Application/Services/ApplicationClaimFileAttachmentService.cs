// <copyright file="ApplicationClaimFileAttachmentService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Services
{
    using System;
    using System.Threading.Tasks;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Claim;
    using UBind.Domain.Entities;
    using UBind.Domain.Helpers;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadWriteModel;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services;

    /// <inheritdoc />
    public class ApplicationClaimFileAttachmentService : IApplicationClaimFileAttachmentService
    {
        private readonly IClaimAggregateRepository claimAggregateRepository;
        private readonly IFileAttachmentRepository<ClaimFileAttachment> claimAttachmentRepository;
        private readonly IFileContentRepository fileContentRepository;
        private readonly IClaimService claimService;
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationClaimFileAttachmentService"/> class.
        /// </summary>
        /// <param name="claimAggregateRepository">Claim Aggregate Repository.</param>
        /// <param name="claimAttachmentRepository">The claim attachment repository.</param>
        /// <param name="fileContentRepository">The file content repository.</param>
        /// <param name="claimService">The claim service.</param>
        /// <param name="currentUserIdentification">The current user identification.</param>
        public ApplicationClaimFileAttachmentService(
            IClaimAggregateRepository claimAggregateRepository,
            IFileAttachmentRepository<ClaimFileAttachment> claimAttachmentRepository,
            IFileContentRepository fileContentRepository,
            IClaimService claimService,
            IHttpContextPropertiesResolver currentUserIdentification)
        {
            this.claimAggregateRepository = claimAggregateRepository;
            this.claimAttachmentRepository = claimAttachmentRepository;
            this.fileContentRepository = fileContentRepository;
            this.claimService = claimService;
            this.httpContextPropertiesResolver = currentUserIdentification;
        }

        /// <inheritdoc />
        public async Task<ClaimAttachmentReadModel> AttachFile(
            Guid tenantId,
            Guid claimId,
            string fileName,
            string fileType,
            string fileContent)
        {
            var now = SystemClock.Instance.GetCurrentInstant();
            var claimAggregate = this.claimAggregateRepository.GetById(tenantId, claimId);
            async Task<ClaimAttachmentReadModel> AttachAndSaveAsync()
            {
                var content = FileContent.CreateFromBase64String(claimAggregate.TenantId, Guid.NewGuid(), fileContent);
                var fileSize = content.Size;
                var fileContentId = this.fileContentRepository.Insert(content);
                claimAggregate.AttachFile(
                    fileContentId,
                    fileName,
                    fileType,
                    fileSize,
                    this.httpContextPropertiesResolver.PerformingUserId,
                    now);

                await this.claimAggregateRepository.Save(claimAggregate);

                var attachment = new ClaimFileAttachment(
                    fileName,
                    fileType,
                    fileSize,
                    fileContentId,
                    now);
                return ClaimAttachmentReadModel.CreateClaimAttachmentReadModel(claimId, attachment);
            }

            return await ConcurrencyPolicy.ExecuteWithRetriesAsync(
                AttachAndSaveAsync,
                () => claimAggregate = this.claimAggregateRepository.GetById(tenantId, claimId));
        }
    }
}
