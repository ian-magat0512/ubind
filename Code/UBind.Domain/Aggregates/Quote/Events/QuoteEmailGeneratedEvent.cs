// <copyright file="QuoteEmailGeneratedEvent.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote
{
    using System;
    using Newtonsoft.Json;
    using NodaTime;

    /// <summary>
    /// Aggregate for quotes.
    /// </summary>
    public partial class QuoteAggregate
    {
        /// <summary>
        /// Event raised when an email has been generated.
        /// </summary>
        public class QuoteEmailGeneratedEvent
            : Event<QuoteAggregate, Guid>
        {
            private Guid quoteId;

            /// <summary>
            /// Initializes a new instance of the <see cref="QuoteEmailGeneratedEvent"/> class.
            /// </summary>
            /// <param name="tenantId">The tenant Id.</param>
            /// <param name="aggregateId">The aggregate Id.</param>
            /// <param name="quoteId">The ID of the quote aggregate.</param>
            /// <param name="emailId">The ID of the email model.</param>
            /// <param name="recipient">The recipient of the quote email model.</param>
            /// <param name="subject">The subject of the quote email model.</param>
            /// <param name="hasAttachment">The attachment status of the quote email model.</param>
            /// <param name="createdTimestamp">A created timestamp.</param>
            /// <param name="emailSourceType">The Email Source Type.</param>
            /// <param name="emailType">The Email Type.</param>
            /// <param name="performingUserId">The userId who generated email.</param>
            public QuoteEmailGeneratedEvent(
                Guid tenantId,
                Guid aggregateId,
                Guid quoteId,
                Guid emailId,
                string recipient,
                string subject,
                bool hasAttachment,
                Instant createdTimestamp,
                EmailSourceType emailSourceType,
                EmailType emailType,
                Guid? performingUserId)
                : base(tenantId, aggregateId, performingUserId, createdTimestamp)
            {
                this.EmailId = emailId;
                this.Recipient = recipient;
                this.Subject = subject;
                this.HasAttachment = hasAttachment;
                this.QuoteId = quoteId;
                this.EmailSourceType = emailSourceType;
                this.EmailType = emailType;
            }

            [JsonConstructor]
            private QuoteEmailGeneratedEvent()
            {
                // Nothing to do
            }

            /// <summary>
            /// Gets the ID of the quote email model.
            /// </summary>
            [JsonProperty]
            public Guid EmailId { get; private set; }

            /// <summary>
            /// Gets the recipient value of the quote email model.
            /// </summary>
            [JsonProperty]
            public string Recipient { get; private set; }

            /// <summary>
            /// Gets the subject value of the quote email model.
            /// </summary>
            [JsonProperty]
            public string Subject { get; private set; }

            /// <summary>
            /// Gets the Quote ID value of the quote email model.
            /// </summary>
            [JsonProperty]
            public Guid QuoteId
            {
                get
                {
                    return this.quoteId == default
                       ? this.AggregateId
                       : this.quoteId;
                }

                private set
                {
                    this.quoteId = value;
                }
            }

            /// <summary>
            /// Gets the Email Source type of the quote email model.
            /// </summary>
            [JsonProperty]
            public EmailSourceType EmailSourceType { get; private set; } = EmailSourceType.Quote;

            /// <summary>
            /// Gets the Email type of the quote email model.
            /// </summary>
            [JsonProperty]
            public EmailType EmailType { get; private set; } = EmailType.Admin;

            /// <summary>
            /// Gets a value indicating whether gets the attachment value of the quote email model.
            /// </summary>
            [JsonProperty]
            public bool HasAttachment { get; private set; }
        }
    }
}
