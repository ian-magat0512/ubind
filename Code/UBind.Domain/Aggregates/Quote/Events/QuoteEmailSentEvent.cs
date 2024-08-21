// <copyright file="QuoteEmailSentEvent.cs" company="uBind">
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
        /// Event raised when an email has been sent.
        /// </summary>
        public class QuoteEmailSentEvent : Event<QuoteAggregate, Guid>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="QuoteEmailSentEvent"/> class.
            /// </summary>
            /// <param name="tenantId">The tenant Id.</param>
            /// <param name="quoteId">The ID of the quote aggregate.</param>
            /// <param name="quoteEmailReadModelId">The ID of the quote email read model.</param>
            /// <param name="performingUserId">The userId who sends the email.</param>
            /// <param name="createdTimestamp">A created timestamp.</param>
            public QuoteEmailSentEvent(Guid tenantId, Guid quoteId, Guid quoteEmailReadModelId, Guid? performingUserId, Instant createdTimestamp)
                : base(tenantId, quoteId, performingUserId, createdTimestamp)
            {
                this.QuoteEmailReadModelId = quoteEmailReadModelId;
            }

            [JsonConstructor]
            private QuoteEmailSentEvent()
            {
            }

            /// <summary>
            /// Gets the ID of the quote email read model.
            /// </summary>
            public Guid QuoteEmailReadModelId { get; private set; }
        }
    }
}
