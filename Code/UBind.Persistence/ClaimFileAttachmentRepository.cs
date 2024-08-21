// <copyright file="ClaimFileAttachmentRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence
{
    using System;
    using System.Linq;
    using CSharpFunctionalExtensions;
    using UBind.Domain;
    using UBind.Domain.Entities;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Repositories;

    /// <summary>
    /// Repository for retrieving file attachments for a claim.
    /// </summary>
    public class ClaimFileAttachmentRepository : IFileAttachmentRepository<ClaimFileAttachment>
    {
        private readonly IUBindDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClaimFileAttachmentRepository"/> class.
        /// </summary>
        /// <param name="dbContext">The UBind database context.</param>
        public ClaimFileAttachmentRepository(IUBindDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        /// <inheritdoc/>
        public void Insert(ClaimFileAttachment claimFileAttachment)
        {
            this.dbContext.ClaimFileAttachments.Add(claimFileAttachment);
        }

        /// <inheritdoc/>
        public void SaveChanges()
        {
            this.dbContext.SaveChanges();
        }

        /// <inheritdoc/>
        public Maybe<IFileContentReadModel> GetAttachmentContent(Guid tenantId, Guid fileContentId)
        {
            var attachments = this.dbContext.ClaimAttachment
                .Where(ca => ca.TenantId == tenantId
                && ca.FileContentId == fileContentId);
            return this.GetFileContent(attachments);
        }

        public Maybe<IFileContentReadModel> GetAttachmentContent(Guid tenantId, Guid claimOrClaimVersionId, string fileName)
        {
            var attachments = this.dbContext.ClaimAttachment
                .Where(ca => ca.TenantId == tenantId
                && ca.ClaimOrClaimVersionId == claimOrClaimVersionId
                && ca.Name == fileName);
            return this.GetFileContent(attachments);
        }

        private Maybe<IFileContentReadModel> GetFileContent(IQueryable<ClaimAttachmentReadModel> attachments)
        {
            var fileContent = attachments.Join(
                this.dbContext.FileContents,
                ca => new { fileContentId = ca.FileContentId },
                fc => new { fileContentId = fc.Id },
                (ca, fc) => new FileContentReadModel
                {
                    FileContent = fc.Content,
                    ContentType = ca.Type,
                    Name = ca.Name,
                });
            return fileContent.FirstOrDefault();
        }
    }
}
