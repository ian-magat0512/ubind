// <copyright file="IQuoteFileAttachmentRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel
{
    using System;
    using UBind.Domain.Entities;

    /// <summary>
    /// For retrieving persisted quote file attachments.
    /// </summary>
    public interface IQuoteFileAttachmentRepository
    {
        /// <summary>
        /// Gets the quote file attachment.
        /// </summary>
        /// <param name="id">The ID of the quote document read model.</param>
        /// <returns>Quote details.</returns>
        /// Remarks: It is important to not have tenant Id parameter for this,
        /// especially for UB-7141 migration, as this will fix issues with attachments
        /// not having tenant id.
        QuoteFileAttachment GetById(Guid id);

        /// <summary>
        /// Updates the record with the proper tenant id.
        /// </summary>
        /// <param name="tenantId">The tenant id to update.</param>
        /// <param name="id">The ID of the quote document.</param>
        void UpdateTenantId(Guid tenantId, Guid id);

        /// <summary>
        /// Gets all attachments Ids associated with quote.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="quoteId">The quote Id.</param>
        /// <returns>The attachment Ids.</returns>
        IEnumerable<Guid> GetAttachmentIdsForQuote(Guid tenantId, Guid quoteId);
    }
}
