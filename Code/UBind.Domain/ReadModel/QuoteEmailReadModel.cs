// <copyright file="QuoteEmailReadModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using NodaTime;

    /// <summary>
    /// Stores the contents of email generated.
    /// </summary>
    public class QuoteEmailReadModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QuoteEmailReadModel"/> class.
        /// </summary>
        /// <param name="emailId">The ID of the email model.</param>
        /// <param name="recipient">The recipient of the quote email model.</param>
        /// <param name="subject">The subject of the quote email model.</param>
        /// <param name="hasAttachment">The attachment status of the quote email model.</param>
        /// <param name="createdTimestamp">The time the quote email was created.</param>
        /// <param name="policyId">The ID of the policy the email relates to.</param>
        /// <param name="quoteId">The quote id.</param>
        /// <param name="policyTransactionId">The policy transaction ID.</param>
        /// <param name="emailSourceType">The email source Type.</param>
        /// <param name="emailType">The email Type.</param>
        public QuoteEmailReadModel(
            Guid emailId,
            string recipient,
            string subject,
            bool hasAttachment,
            Instant createdTimestamp,
            Guid policyId,
            Guid quoteId,
            Guid policyTransactionId,
            EmailSourceType emailSourceType,
            EmailType emailType)
        {
            this.Id = Guid.NewGuid();
            this.EmailId = emailId;
            this.Recipient = recipient;
            this.Subject = subject;
            this.HasAttachment = hasAttachment;
            this.CreatedTimestamp = createdTimestamp;
            this.PolicyId = policyId;
            this.QuoteId = quoteId;
            this.PolicyTransactionId = policyTransactionId;
            this.EmailSourceType = emailSourceType;
            this.EmailType = emailType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QuoteEmailReadModel"/> class.
        /// Parameterless constructor for EF.
        /// </summary>
        private QuoteEmailReadModel()
        {
            // Nothing to do
        }

        /// <summary>
        /// Gets the entity ID.
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// Gets the Email Type.
        /// </summary>
        public EmailType EmailType { get; private set; }

        /// <summary>
        /// Gets the EmailSourceType ID.
        /// </summary>
        public EmailSourceType EmailSourceType { get; private set; }

        /// <summary>
        /// Gets the ID of the quote email model.
        /// </summary>
        public Guid EmailId { get; private set; }

        /// <summary>
        /// Gets the ID of the policy the email is associated with.
        /// </summary>
        public Guid PolicyId { get; private set; }

        /// <summary>
        /// Gets the ID of the quote the email is associated with.
        /// </summary>
        public Guid QuoteId { get; private set; }

        /// <summary>
        /// Gets the ID of the policy transaction the email is associated with.
        /// </summary>
        public Guid PolicyTransactionId { get; private set; }

        /// <summary>
        /// Gets the recipient of the quote email model.
        /// </summary>
        public string Recipient { get; private set; }

        /// <summary>
        /// Gets the subject of the quote email model.
        /// </summary>
        public string Subject { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the attachment of the quote email model.
        /// </summary>
        public bool HasAttachment { get; private set; }

        /// <summary>
        /// Gets or sets the created time as the number of ticks since the epoch for persistance in EF.
        /// </summary>
        public long CreatedTicksSinceEpoch { get; set; }

        /// <summary>
        /// Gets the time the claim was created.
        /// </summary>
        public Instant CreatedTimestamp
        {
            get
            {
                return Instant.FromUnixTimeTicks(this.CreatedTicksSinceEpoch);
            }

            private set
            {
                this.CreatedTicksSinceEpoch = value.ToUnixTimeTicks();
            }
        }

        /// <summary>
        /// Gets the quote email sending.
        /// </summary>
        public ICollection<QuoteEmailSendingReadModel> QuoteEmailSendings { get; private set; }
            = new Collection<QuoteEmailSendingReadModel>();

        /// <summary>
        /// Record the sending of read model.
        /// </summary>
        /// <param name="createdTimestamp">The time the quote email was sent.</param>
        public void RecordEmailSending(Instant createdTimestamp)
        {
            // Begin record
            this.QuoteEmailSendings.Add(new QuoteEmailSendingReadModel(createdTimestamp));
        }
    }
}
