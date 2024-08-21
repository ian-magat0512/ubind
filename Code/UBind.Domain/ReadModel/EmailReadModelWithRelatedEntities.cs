// <copyright file="EmailReadModelWithRelatedEntities.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel
{
    using System.Collections.Generic;
    using UBind.Domain;
    using UBind.Domain.ReadWriteModel;

    /// <summary>
    /// Interface for email and its related entities.
    /// </summary>
    public class EmailReadModelWithRelatedEntities : IEmailReadModelWithRelatedEntities
    {
        /// <inheritdoc/>
        public Tenant Tenant { get; set; }

        /// <inheritdoc/>
        public OrganisationReadModel Organisation { get; set; }

        /// <inheritdoc/>
        public ReadWriteModel.Email.Email Email { get; set; }

        /// <inheritdoc/>
        public IEnumerable<Tag> Tags { get; set; }

        /// <inheritdoc/>
        public IEnumerable<TenantDetails> TenantDetails { get; set; }

        /// <inheritdoc/>
        public IEnumerable<EmailAttachment> Attachments { get; set; }

        /// <inheritdoc/>
        public IEnumerable<DocumentFile> Documents { get; set; }

        /// <inheritdoc/>
        public IEnumerable<Relationship> FromRelationships { get; set; }

        /// <inheritdoc/>
        public IEnumerable<Relationship> ToRelationships { get; set; }
    }
}
