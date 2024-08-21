// <copyright file="QuoteFileAttachmentReadModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel
{
    using System.Diagnostics.Contracts;
    using UBind.Domain.Entities;

    /// <summary>
    /// Read model for quote file attachment.
    /// </summary>
    public class QuoteFileAttachmentReadModel : QuoteFileAttachment
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QuoteFileAttachmentReadModel"/> class.
        /// Parameterless constructor for EF.
        /// </summary>
        protected QuoteFileAttachmentReadModel()
        {
        }

        private QuoteFileAttachmentReadModel(QuoteFileAttachment attachment)
            : base(
                  attachment.TenantId,
                  attachment.Id,
                  attachment.QuoteId,
                  attachment.FileContentId,
                  attachment.Name,
                  attachment.Type,
                  attachment.FileSize,
                  attachment.CreatedTimestamp)
        {
        }

        /// <summary>
        /// Create new quote file attachment read model.
        /// </summary>
        /// <param name="attachment">The quote file attachment.</param>
        /// <returns>A new instance of <see cref="QuoteFileAttachmentReadModel"/>.</returns>
        public static QuoteFileAttachmentReadModel Create(QuoteFileAttachment attachment)
        {
            return new QuoteFileAttachmentReadModel(attachment);
        }

        /// <summary>
        /// Update the read model with the latest version and metadata of attachment.
        /// </summary>
        /// <param name="attachment">The quote file attachment.</param>
        public void Update(QuoteFileAttachment attachment)
        {
            Contract.Assert(attachment.Name == this.Name);
            this.FileSize = attachment.FileSize;
            this.FileContentId = attachment.FileContentId;
            this.Type = attachment.Type;
            this.CreatedTimestamp = attachment.CreatedTimestamp;
        }
    }
}
