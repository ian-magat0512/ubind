// <copyright file="QuoteBoundEvent.cs" company="uBind">
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
        /// Event raised when a quote has been bound.
        /// </summary>
        public class QuoteBoundEvent : Event<QuoteAggregate, Guid>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="QuoteBoundEvent"/> class.
            /// </summary>
            /// <param name="aggregateId">The ID of the quote aggregate.</param>
            /// <param name="quoteId">The ID of the quote.</param>
            /// <param name="performingUserId">The ID of the user who bound the quote.</param>
            /// <param name="createdTimestamp">A created timestamp.</param>
            public QuoteBoundEvent(Guid tenantId, Guid aggregateId, Guid quoteId, Guid? performingUserId, Instant createdTimestamp)
                : base(tenantId, aggregateId, performingUserId, createdTimestamp)
            {
                this.QuoteId = quoteId;
                this.UserId = performingUserId;
            }

            [JsonConstructor]
            private QuoteBoundEvent()
            {
            }

            /// <summary>
            /// Gets an ID uniquely identifying the quote.
            /// </summary>
            public Guid QuoteId { get; private set; }

            /// <summary>
            /// Gets the ID of the user who bound the quote.
            /// </summary>
            public Guid? UserId { get; private set; }
        }
    }
}
