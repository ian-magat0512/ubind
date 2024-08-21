// <copyright file="QuoteActualisedEvent.cs" company="uBind">
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
        /// Event raised when a quote has been actualised.
        /// </summary>
        public class QuoteActualisedEvent : Event<QuoteAggregate, Guid>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="QuoteActualisedEvent"/> class.
            /// </summary>
            /// <param name="tenantId">The tenant Id.</param>
            /// <param name="aggregateId">The ID of the aggregate the quote belongs to.</param>
            /// <param name="quoteId">The ID of the quote.</param>
            /// <param name="performingUserId">The identifier of the performing user.</param>
            /// <param name="createdTimestamp">A created timestamp.</param>
            public QuoteActualisedEvent(Guid tenantId, Guid aggregateId, Guid quoteId, Guid? performingUserId, Instant createdTimestamp)
                : base(tenantId, aggregateId, performingUserId, createdTimestamp)
            {
                this.QuoteId = quoteId;
            }

            [JsonConstructor]
            private QuoteActualisedEvent()
            {
            }

            /// <summary>
            /// Gets the ID of the quote.
            /// </summary>
            [JsonProperty]
            public Guid QuoteId { get; private set; }
        }
    }
}
