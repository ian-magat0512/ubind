// <copyright file="IApplicationClaimFileAttachmentService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Services
{
    using System;
    using System.Threading.Tasks;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// Service for file attachment in claims.
    /// </summary>
    public interface IApplicationClaimFileAttachmentService
    {
        /// <summary>
        /// Attach a file to the claim.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant.</param>
        /// <param name="claimId">The ID of the claim the file attachment is for.</param>
        /// <param name="fileName">The file name.</param>
        /// <param name="fileType">The file type.</param>
        /// <param name="fileContent">The file blob content.</param>
        /// <returns>The updated claim aggregate.</returns>
        Task<ClaimAttachmentReadModel> AttachFile(
            Guid tenantId,
            Guid claimId,
            string fileName,
            string fileType,
            string fileContent);
    }
}
