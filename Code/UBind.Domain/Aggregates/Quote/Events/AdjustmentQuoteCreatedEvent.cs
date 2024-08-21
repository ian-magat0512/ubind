// <copyright file="AdjustmentQuoteCreatedEvent.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote
{
    using System;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Domain.Aggregates.AdditionalPropertyValue;

    /// <summary>
    /// Aggregate for quotes.
    /// </summary>
    public partial class QuoteAggregate
    {
        private void Apply(AdjustmentQuoteCreatedEvent @event, int sequenceNumber)
        {
            var quote = new AdjustmentQuote(
                @event.QuoteId,
                this,
                sequenceNumber,
                @event.QuoteNumber,
                this.CustomerId,
                @event.Timestamp,
                @event.FormDataJson,
                @event.InitialQuoteState,
                @event.ProductReleaseId);
            this.quotes.Add(quote);
        }

        /// <summary>
        /// Event raised when a child quote has been created.
        /// </summary>
        public class AdjustmentQuoteCreatedEvent : Event<QuoteAggregate, Guid>, IQuoteCreatedEvent
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="AdjustmentQuoteCreatedEvent"/> class.
            /// </summary>
            /// <param name="aggregateId">The ID of the quote aggregate.</param>
            /// <param name="organisationId">The organisation id the quote is for.</param>
            /// <param name="quoteNumber">The quote number.</param>
            /// <param name="formDataJson">The initial form data for the quote.</param>
            /// <param name="parentQuoteId">The ID of the quote that created the policy being adjusted.</param>
            /// <param name="performingUserId">The userId who created adjustment quote.</param>
            /// <param name="createdTimestamp">The event created time.</param>
            public AdjustmentQuoteCreatedEvent(
                Guid tenantId,
                Guid aggregateId,
                Guid organisationId,
                string quoteNumber,
                string formDataJson,
                Guid? parentQuoteId,
                Guid? performingUserId,
                Instant createdTimestamp,
                Guid? productReleaseId,
                string? intitialQuoteState = null,
                List<AdditionalPropertyValueUpsertModel>? additionalProperties = null)
                : base(tenantId, aggregateId, performingUserId, createdTimestamp)
            {
                this.QuoteId = Guid.NewGuid();
                this.QuoteNumber = quoteNumber;
                this.ParentQuoteId = parentQuoteId;
                this.FormDataJson = formDataJson;
                this.OrganisationId = organisationId;
                this.InitialQuoteState = intitialQuoteState;
                this.AdditionalProperties = additionalProperties;
                this.ProductReleaseId = productReleaseId;
            }

            [JsonConstructor]
            public AdjustmentQuoteCreatedEvent()
            {
            }

            /// <summary>
            /// Gets the ID of the adjustment quote.
            /// </summary>
            [JsonProperty]
            public Guid QuoteId { get; private set; }

            /// <summary>
            /// Gets the ID of the quote that last updated the policy that the adustment is for.
            /// </summary>
            [JsonProperty]
            public Guid? ParentQuoteId { get; private set; }

            /// <summary>
            /// Gets the ID of the organisation the quote is for.
            /// </summary>
            [JsonProperty]
            public Guid OrganisationId { get; private set; }

            /// <summary>
            /// Gets the quote number assigned to the quote.
            /// </summary>
            [JsonProperty]
            public string QuoteNumber { get; private set; }

            /// <summary>
            /// Gets the initial form data for the adjustment quote.
            /// </summary>
            [JsonProperty]
            public string FormDataJson { get; private set; }

            [JsonProperty]
            public string? InitialQuoteState { get; private set; }

            [JsonProperty]
            public List<AdditionalPropertyValueUpsertModel>? AdditionalProperties { get; }

            [JsonProperty]
            public Guid? ProductReleaseId { get; private set; }
        }
    }
}
