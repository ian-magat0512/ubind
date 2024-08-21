// <copyright file="RenewalQuoteCreatedEvent.cs" company="uBind">
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
        private void Apply(RenewalQuoteCreatedEvent @event, int sequenceNumber)
        {
            var quote = new RenewalQuote(
                @event.QuoteId,
                this,
                sequenceNumber,
                @event.QuoteNumber,
                @event.FormDataJson,
                @event.Timestamp,
                this.CustomerId,
                @event.ProductReleaseId,
                @event.InitialQuoteState);
            this.quotes.Add(quote);
        }

        /// <summary>
        /// Event raised when a child quote has been created.
        /// </summary>
        public class RenewalQuoteCreatedEvent : Event<QuoteAggregate, Guid>, IQuoteCreatedEvent
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="RenewalQuoteCreatedEvent"/> class.
            /// </summary>
            /// <param name="aggregateId">The ID of the aggregate.</param>
            /// <param name="organisationId">The ID of the orgranisation the quote is for.</param>
            /// <param name="quoteNumber">The number assigned to the quote.</param>
            /// <param name="formDataJson">The initial form data for the renewal quote.</param>
            /// <param name="performingUserId">The userId who creates renewal quotes.</param>
            /// <param name="timestamp">The event created time.</param>
            public RenewalQuoteCreatedEvent(
                Guid tenantId,
                Guid aggregateId,
                Guid organisationId,
                string quoteNumber,
                string formDataJson,
                Guid? performingUserId,
                Instant timestamp,
                Guid? productReleaseId,
                string? initialQuoteState = null,
                List<AdditionalPropertyValueUpsertModel>? additionalProperties = null)
                : base(tenantId, aggregateId, performingUserId, timestamp)
            {
                this.QuoteId = Guid.NewGuid();
                this.OrganisationId = organisationId;
                this.QuoteNumber = quoteNumber;
                this.FormDataJson = formDataJson;
                this.InitialQuoteState = initialQuoteState;
                this.AdditionalProperties = additionalProperties;
                this.ProductReleaseId = productReleaseId;
            }

            [JsonConstructor]
            public RenewalQuoteCreatedEvent()
            {
            }

            /// <summary>
            /// Gets the ID of the quote.
            /// </summary>
            [JsonProperty]
            public Guid QuoteId { get; private set; }

            /// <summary>
            /// Gets the organisation ID the qoute is for.
            /// </summary>
            [JsonProperty]
            public Guid OrganisationId { get; private set; }

            /// <summary>
            /// Gets the quote number assigned to the quote.
            /// </summary>
            [JsonProperty]
            public string QuoteNumber { get; private set; }

            /// <summary>
            /// Gets the initial form data to use in the renewal quote.
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
