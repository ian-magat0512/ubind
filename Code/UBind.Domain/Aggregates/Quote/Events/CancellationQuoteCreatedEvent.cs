// <copyright file="CancellationQuoteCreatedEvent.cs" company="uBind">
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
        private void Apply(CancellationQuoteCreatedEvent @event, int sequenceNumber)
        {
            var quote = new CancellationQuote(
                @event.QuoteId,
                this,
                sequenceNumber,
                @event.QuoteNumber,
                this.CustomerId,
                @event.Timestamp,
                @event.FormDataJson,
                @event.ProductReleaseId,
                @event.InitialQuoteState);
            this.quotes.Add(quote);
        }

        /// <summary>
        /// Event raised when a cancellation quote has been created.
        /// </summary>
        public class CancellationQuoteCreatedEvent : Event<QuoteAggregate, Guid>, IQuoteCreatedEvent
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="CancellationQuoteCreatedEvent"/> class.
            /// </summary>
            /// <param name="aggregateId">The ID of the quote aggregate.</param>
            /// <param name="organisationId">The ID of the oragnisation the cancelled quote is for.</param>
            /// <param name="quoteNumber">The quote number.</param>
            /// <param name="performingUserId">The userId who created cancellation quote.</param>
            /// <param name="parentQuoteId">The ID of the quote that created the policy being cancel.</param>
            /// <param name="createdTimestamp">The event created time.</param>
            /// <param name="formDataJson">The initial form data for the quote.</param>
            public CancellationQuoteCreatedEvent(
                Guid tenantId,
                Guid aggregateId,
                Guid organisationId,
                string quoteNumber,
                Guid? parentQuoteId,
                Guid? performingUserId,
                Instant createdTimestamp,
                Guid? productReleaseId,
                string formDataJson = null,
                string? initialQuoteState = null,
                List<AdditionalPropertyValueUpsertModel>? additionalProperties = null)
                : base(tenantId, aggregateId, performingUserId, createdTimestamp)
            {
                this.QuoteId = Guid.NewGuid();
                this.OrganisationId = organisationId;
                this.QuoteNumber = quoteNumber;
                this.ParentQuoteId = parentQuoteId;
                this.FormDataJson = formDataJson;
                this.InitialQuoteState = initialQuoteState;
                this.AdditionalProperties = additionalProperties;
                this.ProductReleaseId = productReleaseId;
            }

            [JsonConstructor]
            public CancellationQuoteCreatedEvent()
            {
            }

            /// <summary>
            /// Gets the ID of the Cancelled quote.
            /// </summary>
            [JsonProperty]
            public Guid QuoteId { get; private set; }

            /// <summary>
            /// Gets the organisation ID of the Cancelled quote.
            /// </summary>
            [JsonProperty]
            public Guid OrganisationId { get; private set; }

            /// <summary>
            /// Gets the ID of the quote that last updated the policy that the cancellation is for.
            /// </summary>
            [JsonProperty]
            public Guid? ParentQuoteId { get; private set; }

            /// <summary>
            /// Gets the initial form data for the adjustment quote.
            /// </summary>
            [JsonProperty]
            public string FormDataJson { get; private set; }

            /// <summary>
            /// Gets the quote number assigned to the quote.
            /// </summary>
            [JsonProperty]
            public string QuoteNumber { get; private set; }

            [JsonProperty]
            public string? InitialQuoteState { get; set; }

            [JsonProperty]
            public List<AdditionalPropertyValueUpsertModel>? AdditionalProperties { get; }

            [JsonProperty]
            public Guid? ProductReleaseId { get; private set; }
        }
    }
}
