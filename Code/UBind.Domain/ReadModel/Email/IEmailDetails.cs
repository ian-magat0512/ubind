// <copyright file="IEmailDetails.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel.Email
{
    using System;
    using System.Collections.Generic;
    using NodaTime;
    using UBind.Domain.ReadWriteModel;

    /// <summary>
    /// Presentation model for email and its related records.
    /// </summary>
    public interface IEmailDetails
    {
        /// <summary>
        /// Gets the email Id.
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// Gets Tenant Id of the email.
        /// </summary>
        Guid TenantId { get; }

        /// <summary>
        /// Gets the Organisation Id of the email.
        /// </summary>
        Guid OrganisationId { get; }

        /// <summary>
        /// Gets Product Id of the email.
        /// </summary>
        Guid? ProductId { get; }

        /// <summary>
        /// Gets Product name of the email.
        /// </summary>
        string ProductName { get; }

        /// <summary>
        /// Gets environment of the email.
        /// </summary>
        DeploymentEnvironment Environment { get; }

        /// <summary>
        /// Gets the email's recipient.
        /// </summary>
        string Recipient { get; }

        /// <summary>
        /// Gets the emails sender.
        /// </summary>
        string From { get; }

        /// <summary>
        /// Gets the email's cc.
        /// </summary>
        string CC { get; }

        /// <summary>
        /// Gets the email's BCC.
        /// </summary>
        string BCC { get; }

        /// <summary>
        /// Gets the email's subject.
        /// </summary>
        string Subject { get; }

        /// <summary>
        /// Gets the email's html message.
        /// </summary>
        string HtmlMessage { get; }

        /// <summary>
        /// Gets the email's plain message.
        /// </summary>
        string PlainMessage { get; }

        /// <summary>
        /// Gets the email's customer record.
        /// </summary>
        CustomerData Customer { get; }

        /// <summary>
        /// Gets the email's policy record.
        /// </summary>
        PolicyData Policy { get; }

        /// <summary>
        /// Gets the email's quote record.
        /// </summary>
        QuoteData Quote { get; }

        /// <summary>
        /// Gets the email's claim record.
        /// </summary>
        ClaimData Claim { get; }

        /// <summary>
        /// Gets the email's policy transaction record.
        /// </summary>
        PolicyTransactionData PolicyTransaction { get; }

        /// <summary>
        /// Gets the email's user record.
        /// </summary>
        UserData User { get; }

        /// <summary>
        /// Gets the email's top level organisation, didnt come from relationship.
        /// </summary>
        OrganisationData Organisation { get; }

        /// <summary>
        /// Gets the attachments.
        /// </summary>
        IEnumerable<EmailAttachment> EmailAttachments { get; }

        /// <summary>
        /// Gets the documents.
        /// </summary>
        IEnumerable<DocumentFile> Documents { get; }

        /// <summary>
        /// Gets the File Contents.
        /// </summary>
        IEnumerable<FileContent> FileContents { get; }

        /// <summary>
        /// Gets the email's type.
        /// </summary>
        EmailType EmailType { get; }

        /// <summary>
        /// Gets the date the quote email was created.
        /// </summary>
        Instant CreatedTimestamp { get; }

        /// <summary>
        /// Gets the associated tags.
        /// </summary>
        IEnumerable<Tag> Tags { get; }

        /// <summary>
        /// Gets the associated relationships.
        /// </summary>
        IEnumerable<Relationship> Relationships { get; }

        /// <summary>
        /// Add the email attachment to this email summary.
        /// </summary>
        /// <param name="emailAttachments">The email attachment.</param>
        void AddEmailAttachments(ICollection<EmailAttachment> emailAttachments);
    }
}
