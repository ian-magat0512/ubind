// <copyright file="Email.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadWriteModel.Email
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Domain.Entities;

    /// <summary>
    /// Stores the contents of quote emails.
    /// </summary>
    public class Email : Message
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Email"/> class.
        /// </summary>
        /// <param name="tenantId">The tenant id.</param>
        /// <param name="organisationId">The ID of the organisation.</param>
        /// <param name="productId">The  product id.</param>
        /// <param name="environment">The environment of the email.</param>
        /// <param name="id">The unique identifier of the email.</param>
        /// <param name="to">The to value of the email.</param>
        /// <param name="from">The from value of the email.</param>
        /// <param name="cc">The cc value of the email.</param>
        /// <param name="bcc">The bcc value of the email.</param>
        /// <param name="replyTo">The replyTo value of the email.</param>
        /// <param name="subject">The subject value of the email.</param>
        /// <param name="htmlBody">The html body value of the email.</param>
        /// <param name="plainTextBody">The plain text body value of the email.</param>
        /// <param name="emailAttachments">The collection of email attachments.</param>
        /// <param name="createdTimestamp">The time the user was created.</param>
        public Email(
            Guid tenantId,
            Guid organisationId, // array
            Guid? productId,  // array
            DeploymentEnvironment? environment,  // array
            Guid id,  // array
            IEnumerable<string> to,  // array
            string from,
            IEnumerable<string> cc,
            IEnumerable<string> bcc,
            IEnumerable<string> replyTo,
            string subject,
            string htmlBody,
            string plainTextBody,
            ICollection<EmailAttachment> emailAttachments,
            Instant createdTimestamp)
            : base(id, createdTimestamp)
        {
            this.To = string.Join(" , ", to);
            this.From = from;
            this.CC = cc != null ? string.Join(" , ", cc) : null;
            this.BCC = bcc != null ? string.Join(" , ", bcc) : null;
            this.ReplyTo = replyTo != null ? string.Join(" , ", replyTo) : null;
            this.Subject = subject;
            this.HtmlBody = htmlBody;
            this.PlainTextBody = plainTextBody;
            this.EmailAttachments = emailAttachments;
            this.TenantId = tenantId;
            this.OrganisationId = organisationId;
            this.ProductId = productId;
            this.Environment = environment;
            this.HasAttachments = emailAttachments?.Any() ?? false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Email"/> class.
        /// Parameterless constructor for EF.
        /// </summary>
        private Email()
            : base(default(Guid), default(Instant))
        {
            // Nothing to do
        }

        /// <summary>
        /// Gets the Integration Event ID of the quote email it relates to.
        /// </summary>
        [Obsolete("Email is no longer associated with the event and instead of an entity.")]
        public Guid IntegrationEventId { get; private set; }

        /// <summary>
        /// Gets the ID of the quote the document relates to.
        /// </summary>
        [Obsolete("we now use an association table to associate quote with an email.")]
        public Guid QuoteId { get; private set; }

        /// <summary>
        /// Gets the email's recipient.
        /// </summary>
        public string To { get; private set; }

        /// <summary>
        /// Gets the email type.
        /// </summary>
        [Obsolete("we now use relationships to know whether it's a customer email etc.")]
        public EmailType EmailType { get; private set; }

        /// <summary>
        /// Gets the email's sender.
        /// </summary>
        public string From { get; private set; }

        /// <summary>
        /// Gets the email's carbon copy.
        /// </summary>
        public string CC { get; private set; }

        /// <summary>
        /// Gets the email's blind carbon copy.
        /// </summary>
        public string BCC { get; private set; }

        /// <summary>
        /// Gets the email's reply to.
        /// </summary>
        public string ReplyTo { get; private set; }

        /// <summary>
        /// Gets the email's subject.
        /// </summary>
        public string Subject { get; private set; }

        /// <summary>
        /// Gets the email's HTML body.
        /// </summary>
        public string HtmlBody { get; private set; }

        /// <summary>
        /// Gets the email's plain text body.
        /// </summary>
        public string PlainTextBody { get; private set; }

        /// <summary>
        /// Gets gets the collection of email attachments.
        /// </summary>
        public ICollection<EmailAttachment> EmailAttachments { get; private set; }
            = new Collection<EmailAttachment>();

        /// <summary>
        /// Gets the tenant id.
        /// </summary>
        public Guid TenantId { get; private set; }

        /// <summary>
        /// Gets the product id, if available.
        /// </summary>
        public Guid? ProductId { get; private set; }

        /// <summary>
        /// Gets or sets the Id of organisation the email belongs to.
        /// </summary>
        public Guid OrganisationId { get; set; }

        /// <summary>
        /// Gets or sets environment of the email.
        /// </summary>
        [JsonProperty("environment", NullValueHandling = NullValueHandling.Ignore)]
        public DeploymentEnvironment? Environment { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether if the email has an attachment or not.
        /// </summary>
        [JsonProperty("hasAttachments", NullValueHandling = NullValueHandling.Ignore)]
        public bool HasAttachments { get; set; }

        public void SetOrganisationId(Guid organisationId)
        {
            this.OrganisationId = organisationId;
        }
    }
}
