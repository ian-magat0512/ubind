// <copyright file="QuoteExpiryTimestampSetEvent.cs" company="uBind">
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
        /// Event raised when a quote has been assigned an expiry time.
        /// </summary>
        public class QuoteExpiryTimestampSetEvent : Event<QuoteAggregate, Guid>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="QuoteExpiryTimestampSetEvent"/> class.
            /// </summary>
            /// <param name="aggregateId">The ID of the quote aggregate.</param>
            /// <param name="quoteId">The ID of the quote.</param>
            /// <param name="expiryTimestamp">The expiry time.</param>
            /// <param name="performingUserId">The userId who sets the expiry time.</param>
            /// <param name="reason">The reason it was set.</param>
            /// <param name="createdTimestamp">A created timestamp.</param>
            public QuoteExpiryTimestampSetEvent(Guid tenantId, Guid aggregateId, Guid quoteId, Instant? expiryTimestamp, QuoteExpiryReason reason, Guid? performingUserId, Instant createdTimestamp)
                : base(tenantId, aggregateId, performingUserId, createdTimestamp)
            {
                this.QuoteId = quoteId;
                this.ExpiryTimestamp = expiryTimestamp;
                this.Reason = reason;
            }

            [JsonConstructor]
            private QuoteExpiryTimestampSetEvent()
            {
            }

            /// <summary>
            /// Gets the Id of the quote.
            /// </summary>
            [JsonProperty]
            public Guid QuoteId { get; private set; }

            /// <summary>
            /// Gets the expiry time.
            /// </summary>
            [JsonProperty]
            public Instant? ExpiryTimestamp { get; private set; }

            /// <summary>
            /// Gets the reason of expiry.
            /// </summary>
            [JsonProperty]
            public QuoteExpiryReason Reason { get; private set; }
        }
    }
}
