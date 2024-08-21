// <copyright file="UserEmailReadModel.cs" company="uBind">
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
    public class UserEmailReadModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserEmailReadModel"/> class.
        /// </summary>
        /// <param name="emailId">The ID of the email model.</param>
        /// <param name="userId">The User Id.</param>
        /// <param name="recipient">The recipient of the quote email model.</param>
        /// <param name="subject">The subject of the quote email model.</param>
        /// <param name="hasAttachment">The attachment status of the quote email model.</param>
        /// <param name="createdTimestamp">The time the quote email was created.</param>
        /// <param name="emailSourceType">The email source Type.</param>
        public UserEmailReadModel(
            Guid emailId,
            Guid userId,
            string recipient,
            string subject,
            bool hasAttachment,
            Instant createdTimestamp,
            EmailSourceType emailSourceType)
        {
            this.Id = Guid.NewGuid();
            this.UserId = userId;
            this.EmailId = emailId;
            this.Recipient = recipient;
            this.Subject = subject;
            this.HasAttachment = hasAttachment;
            this.CreatedTimestamp = createdTimestamp;
            this.EmailSourceType = emailSourceType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserEmailReadModel"/> class.
        /// Parameterless constructor for EF.
        /// </summary>
        private UserEmailReadModel()
        {
            // Nothing to do
        }

        /// <summary>
        /// Gets the entity ID.
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// Gets the user ID.
        /// </summary>
        public Guid UserId { get; private set; }

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
        public ICollection<UserEmailSentReadModel> QuoteEmailSendings { get; private set; }
            = new Collection<UserEmailSentReadModel>();

        /// <summary>
        /// Record the sending of read model.
        /// </summary>
        public void RecordEmailSending()
        {
            // Begin record
            this.QuoteEmailSendings.Add(new UserEmailSentReadModel(this.CreatedTimestamp));
        }
    }
}
