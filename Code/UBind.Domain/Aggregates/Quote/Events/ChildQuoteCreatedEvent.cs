// <copyright file="ChildQuoteCreatedEvent.cs" company="uBind">
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
        /// Event raised when a child quote has been created.
        /// </summary>
        /// <typeparam name="TEvent">The type of the concrete derived event.</typeparam>
        public abstract class ChildQuoteCreatedEvent<TEvent> : Event<QuoteAggregate, Guid>
            where TEvent : Event<QuoteAggregate, Guid>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ChildQuoteCreatedEvent{TConcreteEvent}"/> class.
            /// </summary>
            /// <param name="tenantId">The ID of the tenant.</param>
            /// <param name="quoteId">The ID of the quote.</param>
            /// <param name="childQuoteId">The ID of the child quote.</param>
            /// <param name="performingUserId">The userId who created child quote.</param>
            /// <param name="createdTimestamp">The event created time.</param>
            protected ChildQuoteCreatedEvent(Guid tenantId, Guid quoteId, Guid childQuoteId, Guid? performingUserId, Instant createdTimestamp)
                : base(tenantId, quoteId, performingUserId, createdTimestamp)
            {
                this.ChildQuoteId = childQuoteId;
            }

            /// <summary>
            /// Gets the ID of the child quote.
            /// </summary>
            [JsonProperty]
            public Guid ChildQuoteId { get; private set; }
        }
    }
}
