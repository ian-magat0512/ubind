// <copyright file="EmailAttachment.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using System;
    using NodaTime;

    /// <summary>
    /// Record of email attachments that holds the relationship between document files and quote email model.
    /// </summary>
    public class EmailAttachment : DocumentEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EmailAttachment"/> class.
        /// </summary>
        /// <remarks>Parameterless constructor for EF.</remarks>
        public EmailAttachment()
            : base(default(Guid), default(Instant))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailAttachment"/> class.
        /// </summary>
        /// <param name="emailId">The quote email ID.</param>
        /// <param name="documentFile">The document file.</param>
        /// <param name="createdTimestamp">The time the email attachment was created.</param>
        public EmailAttachment(Guid emailId, DocumentFile documentFile, Instant createdTimestamp)
            : base(Guid.NewGuid(), createdTimestamp)
        {
            this.EmailId = emailId;
            this.DocumentFile = documentFile;
        }

        /// <summary>
        /// Gets the quote email model Id.
        /// </summary>
        public Guid EmailId { get; private set; }

        /// <summary>
        /// Gets the document file.
        /// </summary>
        public DocumentFile DocumentFile { get; private set; }
    }
}
