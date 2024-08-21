// <copyright file="AttachFileToClaimCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Claim
{
    using System;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;

    [RetryOnDbException(5)]
    public class AttachFileToClaimCommand : ICommand<ClaimAttachmentReadModel>
    {
        public AttachFileToClaimCommand(
            Guid tenantId,
            Guid claimId,
            string fileName,
            string fileType,
            string fileContent)
        {
            this.TenantId = tenantId;
            this.ClaimId = claimId;
            this.FileName = fileName;
            this.FileType = fileType;
            this.FileContent = fileContent;
        }

        public Guid TenantId { get; }

        public Guid ClaimId { get; }

        public string FileName { get; }

        public string FileType { get; }

        public string FileContent { get; }
    }
}
