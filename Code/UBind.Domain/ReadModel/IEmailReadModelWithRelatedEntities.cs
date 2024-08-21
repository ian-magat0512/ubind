// <copyright file="IEmailReadModelWithRelatedEntities.cs" company="uBind">
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
    /// For projecting email read model summaries from the database.
    /// </summary>
    public interface IEmailReadModelWithRelatedEntities : IEntityReadModelWithRelatedEntities
    {
        /// <summary>
        /// Gets or sets the tenant.
        /// </summary>
        Tenant Tenant { get; set; }

        /// <summary>
        /// Gets or sets the list of tenant details.
        /// </summary>
        IEnumerable<TenantDetails> TenantDetails { get; set; }

        /// <summary>
        /// Gets or sets the organisation associated with the email.
        /// </summary>
        OrganisationReadModel Organisation { get; set; }

        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        ReadWriteModel.Email.Email Email { get; set; }

        /// <summary>
        /// Gets or sets the email attachments.
        /// </summary>
        IEnumerable<EmailAttachment> Attachments { get; set; }

        /// <summary>
        /// Gets or sets the email attachment documents.
        /// </summary>
        IEnumerable<DocumentFile> Documents { get; set; }

        /// <summary>
        /// Gets or sets the email tags.
        /// </summary>
        IEnumerable<Tag> Tags { get; set; }

        /// <summary>
        /// Gets or sets the source relationships associated with the email.
        /// </summary>
        IEnumerable<Relationship> FromRelationships { get; set; }

        /// <summary>
        /// Gets or sets the target relationships associated with the email.
        /// </summary>
        IEnumerable<Relationship> ToRelationships { get; set; }
    }
}
