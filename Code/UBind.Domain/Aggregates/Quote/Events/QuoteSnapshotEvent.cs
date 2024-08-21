// <copyright file="QuoteSnapshotEvent.cs" company="uBind">
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
        /// Base class for events that include a snapshot of quote data.
        /// </summary>
        /// <typeparam name="TEvent">The type of the derived event.</typeparam>
        public abstract class QuoteSnapshotEvent<TEvent> : Event<QuoteAggregate, Guid>
            where TEvent : QuoteSnapshotEvent<TEvent>
        {
            private Guid quoteId;

            /// <summary>
            /// Initializes a new instance of the <see cref="QuoteSnapshotEvent{TEvent}"/> class.
            /// </summary>
            /// <param name="aggregateId">The ID of the aggregate.</param>
            /// <param name="quote">The quote.</param>
            /// <param name="performingUserId">The performing userId.</param>
            /// <param name="createdTimestamp">A created timestamp.</param>
            public QuoteSnapshotEvent(Guid tenantId, Guid aggregateId, Quote quote, Guid? performingUserId, Instant createdTimestamp)
                : base(tenantId, aggregateId, performingUserId, createdTimestamp)
            {
                this.quoteId = quote.Id;
                this.DataSnapshotIds = quote.GetLatestDataSnapshotIds();
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="QuoteSnapshotEvent{TEvent}"/> class.
            /// </summary>
            /// <remarks>
            /// /// Parameterless constructor for JSON deserialization.
            /// .</remarks>
            [JsonConstructor]
            protected QuoteSnapshotEvent()
            {
            }

            /// <summary>
            /// Gets an ID uniquely identifying the quote whose form data was updated.
            /// </summary>
            [JsonProperty]
            public Guid QuoteId
            {
                get
                {
                    // Quote ID will not exist for quotes created before introduction of renewals and updates, in which case the ID
                    // of the aggregate will be used.
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
            /// Gets the ID of the the form data update submitted.
            /// </summary>
            [JsonProperty]
            public QuoteDataSnapshotIds DataSnapshotIds { get; private set; }
        }
    }
}
