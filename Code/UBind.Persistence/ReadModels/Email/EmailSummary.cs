// <copyright file="EmailSummary.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.ReadModels.Email
{
    using System;
    using UBind.Domain;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// For projecting email read model summaries from the database.
    /// Used for Email Listing.
    /// </summary>
    public class EmailSummary : EntityReadModel<Guid>, IEmailSummary
    {
        /// <summary>
        /// Gets or sets the email's recipient.
        /// </summary>
        public string Recipient { get; set; }

        /// <summary>
        /// Gets or sets the email's subject.
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the email has an attachment.
        /// </summary>
        public bool HasAttachment { get; set; }
    }
}
