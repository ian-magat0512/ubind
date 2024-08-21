// <copyright file="ApplyNewIdEvent.cs" company="uBind">
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
        /// Event raised when specifically to apply New Ids on a quote aggregate.
        /// </summary>
        public class ApplyNewIdEvent : Event<QuoteAggregate, Guid>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ApplyNewIdEvent"/> class.
            /// </summary>
            /// <param name="aggregateId">The ID of the aggregate.</param>
            /// <param name="quoteId">The quote id.</param>
            /// <param name="tenantId">The new tenant id.</param>
            /// <param name="productId">The new product id.</param>
            /// <param name="performingUserId">The performing user id.</param>
            /// <param name="createdTimestamp">The created time.</param>
            public ApplyNewIdEvent(
                Guid tenantId,
                Guid aggregateId,
                Guid quoteId,
                Guid productId,
                Guid? performingUserId,
                Instant createdTimestamp)
                : base(tenantId, aggregateId, performingUserId, createdTimestamp)
            {
                this.QuoteId = quoteId;
                this.ProductId = productId;
            }

            [JsonConstructor]
            private ApplyNewIdEvent()
            {
            }

            /// <summary>
            /// Gets the quote id.
            /// </summary>
            [JsonProperty]
            public Guid QuoteId { get; private set; }

            /// <summary>
            /// Gets the guid ID of the product the quote belongs to.
            /// Note: since we already named this to ProductNewId, we cant change it.
            /// </summary>
            [JsonProperty("ProductNewId")]
            public Guid ProductId { get; private set; }
        }
    }
}
