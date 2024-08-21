// <copyright file="FormDataUpdatedEvent.cs" company="uBind">
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
        /// Event raised when a quote's form data has been updated.
        /// </summary>
        public class FormDataUpdatedEvent : Event<QuoteAggregate, Guid>
        {
            private Guid quoteId;

            /// <summary>
            /// Initializes a new instance of the <see cref="FormDataUpdatedEvent"/> class.
            /// </summary>
            /// <param name="tenantId">The ID of the tenant.</param>
            /// <param name="aggregateId">The ID of the quote aggregate.</param>
            /// <param name="quoteId">The ID of the quote.</param>
            /// <param name="formData">Form data as JSON.</param>
            /// <param name="performingUserId">The userId who updates form data.</param>
            /// <param name="createdTimestamp">A created timestamp.</param>
            public FormDataUpdatedEvent(Guid tenantId, Guid aggregateId, Guid quoteId, string formData, Guid? performingUserId, Instant createdTimestamp)
                : base(tenantId, aggregateId, performingUserId, createdTimestamp)
            {
                this.FormUpdateId = Guid.NewGuid();
                this.QuoteId = quoteId;
                this.FormData = formData;
            }

            [JsonConstructor]
            private FormDataUpdatedEvent()
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
            /// Gets an Id uniquely identifying the form update.
            /// </summary>
            [JsonProperty]
            public Guid FormUpdateId { get; private set; }

            /// <summary>
            /// Gets the updated form data.
            /// </summary>
            [JsonProperty]
            public string FormData { get; private set; }
        }
    }
}
