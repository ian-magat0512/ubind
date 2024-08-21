// <copyright file="IClaimAttachmentReadModelRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel.Claim
{
    using System;
    using System.Linq;

    /// <summary>
    /// For querying or doing persistence to claim attachments.
    /// Note: This is created to query directly the claim attachments and to modify them directly
    /// instead of going through the claim aggregates and creating events, which is a time and resource consuming process.
    /// </summary>
    public interface IClaimAttachmentReadModelRepository
    {
        /// <summary>
        /// Retrieve attachments filtered by tenantId.
        /// Note: this is temporarily allowed to query claim attachments by string tenantId,
        /// so that the migration calling this can set the guid tenantIds.
        /// </summary>
        /// <param name="tenantId">The tenant id filter.</param>
        /// <returns>A list of claim attachments.</returns>
        IQueryable<ClaimAttachmentReadModel> GetAll(Guid tenantId);
    }
}
