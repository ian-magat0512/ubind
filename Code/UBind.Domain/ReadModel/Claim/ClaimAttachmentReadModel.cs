// <copyright file="ClaimAttachmentReadModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel
{
    using System;
    using System.Diagnostics.Contracts;
    using UBind.Domain.Entities;

    /// <summary>
    /// Read model for claim file attachment.
    /// </summary>
    public class ClaimAttachmentReadModel : ClaimFileAttachment, IReadModel<Guid>
    {
        private Guid claimOrClaimVersionId;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClaimAttachmentReadModel"/> class.
        /// Parameterless constructor for EF.
        /// </summary>
        protected ClaimAttachmentReadModel()
        {
        }

        private ClaimAttachmentReadModel(
            Guid claimId, Guid claimOrClaimVersionId, DocumentOwnerType ownerType, ClaimFileAttachment attachment)
            : base(
                  attachment.Id,
                  attachment.Name,
                  attachment.Type,
                  attachment.FileSize,
                  attachment.FileContentId,
                  attachment.CreatedTimestamp)
        {
            this.ClaimId = claimId;
            this.ClaimOrClaimVersionId = claimOrClaimVersionId;
            this.OwnerType = ownerType;
        }

        private ClaimAttachmentReadModel(Guid claimId, DocumentOwnerType ownerType, ClaimFileAttachment attachment)
            : base(
                  attachment.Id,
                  attachment.Name,
                  attachment.Type,
                  attachment.FileSize,
                  attachment.FileContentId,
                  attachment.CreatedTimestamp)
        {
            this.ClaimId = claimId;
            this.OwnerType = ownerType;
        }

        /// <summary>
        /// Gets the Claim Id.
        /// </summary>
        public Guid ClaimId { get; private set; }

        /// <summary>
        /// Gets the type of entity the document belongs to.
        /// </summary>
        public DocumentOwnerType OwnerType { get; private set; }

        /// <summary>
        /// Gets or sets the environment where the document is created.
        /// </summary>
        public DeploymentEnvironment Environment { get; set; }

        /// <summary>
        /// Gets or sets the Id of tenant the document belongs to.
        /// </summary>
        public Guid TenantId { get; set; }

        /// <summary>
        /// Gets or sets the Id of organisation the document belongs to.
        /// </summary>
        public Guid OrganisationId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to return a test data.
        /// </summary>
        public bool IsTestData { get; set; }

        /// <summary>
        /// Gets or sets the Id of customer the document belongs to.
        /// </summary>
        public Guid? CustomerId { get; set; }

        /// <summary>
        /// Gets the ID of the claim or claim version the document relates to.
        /// </summary>
        public Guid ClaimOrClaimVersionId
        {
            get
            {
                if (this.claimOrClaimVersionId == default)
                {
                    return this.ClaimId;
                }

                return this.claimOrClaimVersionId;
            }

            private set
            {
                this.claimOrClaimVersionId = value;
            }
        }

        /// <summary>
        /// Creates claim document read model for a document that belongs to a Claim.
        /// </summary>
        /// <param name="claimId">The claim Id.</param>
        /// <param name="attachment">The attachment.</param>
        /// <returns>A new instance of <see cref="QuoteDocumentReadModel"/>.</returns>
        public static ClaimAttachmentReadModel CreateClaimAttachmentReadModel(
            Guid claimId, ClaimFileAttachment attachment)
        {
            return new ClaimAttachmentReadModel(claimId, DocumentOwnerType.Claim, attachment);
        }

        /// <summary>
        /// Creates a claim document read model for a document that belongs to a claim version.
        /// </summary>
        /// <param name="claimId">The claim Id.</param>
        /// <param name="claimVersionId">The ID of the claimVersion version.</param>
        /// <param name="attachment">The attachment.</param>
        /// <returns>A new instance of <see cref="QuoteDocumentReadModel"/>.</returns>
        public static ClaimAttachmentReadModel CreateClaimVersionAttachmentReadModel(
            Guid claimId, Guid claimVersionId, ClaimFileAttachment attachment)
        {
            return new ClaimAttachmentReadModel(claimId, claimVersionId, DocumentOwnerType.ClaimVersion, attachment);
        }

        /// <summary>
        /// Update the read model with the latest version and metadata of attachment.
        /// </summary>
        /// <param name="attachment">The claim file attachment document.</param>
        public void Update(ClaimFileAttachment attachment)
        {
            Contract.Assert(attachment.Name == this.Name);
            this.FileSize = attachment.FileSize;
            this.FileContentId = attachment.FileContentId;
            this.CreatedTimestamp = attachment.CreatedTimestamp;
        }
    }
}
