// <copyright file="QuoteInitializedEvent.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote
{
    using System;
    using FluentAssertions;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Domain.Aggregates.AdditionalPropertyValue;

    /// <summary>
    /// Aggregate for quotes.
    /// </summary>
    public partial class QuoteAggregate
    {
        /// <summary>
        /// Event raised when a quote has been created.
        /// </summary>
        public class QuoteInitializedEvent : Event<QuoteAggregate, Guid>, IQuoteCreatedEvent
        {
            private Guid quoteId;

            /// <summary>
            /// Initializes a new instance of the <see cref="QuoteInitializedEvent"/> class.
            /// </summary>
            public QuoteInitializedEvent(
                Guid tenantId,
                Guid aggregateId,
                Guid quoteId,
                Guid organisationId,
                Guid productId,
                DeploymentEnvironment environment,
                QuoteType type,
                Guid? performingUserId,
                Instant createdTimestamp,
                DateTimeZone timeZone,
                bool areTimestampsAuthoritative,
                Guid? customerId,
                bool isTestData,
                Guid? productReleaseId,
                Guid parentId = default,
                string? formDataJson = null,
                string? quoteNumber = null,
                string? initialQuoteState = null,
                List<AdditionalPropertyValueUpsertModel>? additionalProperties = null)
                : base(tenantId, aggregateId, performingUserId, createdTimestamp)
            {
                timeZone.Should().NotBeNull();
                this.ProductId = productId;
                this.OrganisationId = organisationId;
                this.Environment = environment;
                this.Type = type;
                this.IsTestData = isTestData;
                this.QuoteId = quoteId;
                this.FormDataJson = formDataJson;
                this.QuoteNumber = quoteNumber;
                this.CustomerId = customerId;
                this.TimeZoneId = timeZone.ToString();
                this.AreTimestampsAuthoritative = areTimestampsAuthoritative;
                this.InitialQuoteState = initialQuoteState;
                this.AdditionalProperties = additionalProperties;
                this.ProductReleaseId = productReleaseId;
            }

            [JsonConstructor]
            private QuoteInitializedEvent()
            {
            }

            /// <summary>
            /// Gets the ID of the quote.
            /// </summary>
            [JsonProperty]
            public Guid QuoteId
            {
                get
                {
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
            /// Gets the tenant string Id.
            /// Note: for backward compatibility only.
            /// </summary>
            /// Remark: used for UB-7141 migration, you can remove right after.
            [JsonProperty("TenantId")]
            public string TenantStringId { get; private set; }

            /// <summary>
            /// Gets the ID of the organisation the quote is for.
            /// </summary>
            [JsonProperty]
            public Guid OrganisationId { get; private set; }

            /// <summary>
            /// Gets the product string Id.
            /// Note: for backward compatibility only.
            /// </summary>
            [JsonProperty("ProductId")]
            public string ProductStringId { get; private set; }

            /// <summary>
            /// Gets the ID of the product the quote is for.
            /// Note: For Backward compatibility with events, It is to be converted to
            /// JsonProperty("ProductNewId") is important.
            /// </summary>
            [JsonProperty("ProductNewId")]
            public Guid ProductId { get; private set; }

            /// <summary>
            /// Gets the environment the quote belongs to.
            /// </summary>
            [JsonProperty]
            public DeploymentEnvironment Environment { get; private set; }

            /// <summary>
            /// Gets the customer ID this quote was created against, if any.
            /// </summary>
            [JsonProperty]
            public Guid? CustomerId { get; private set; }

            /// <summary>
            /// Gets the type of quote.
            /// </summary>
            [JsonProperty]
            public QuoteType Type { get; private set; }

            /// <summary>
            /// Gets a value indicating whether to return a test data.
            /// </summary>
            [JsonProperty]
            public bool IsTestData { get; private set; }

            /// <summary>
            /// Gets the initial form data to seed the quote with.
            /// </summary>
            [JsonProperty]
            public string? FormDataJson { get; private set; }

            [JsonProperty]
            public string? QuoteNumber { get; private set; }

            /// <summary>
            /// Gets the TZDB timezone ID, e.g. "Australia/Melbourne".
            /// </summary>
            [JsonProperty]
            public string TimeZoneId { get; private set; }

            [JsonProperty]
            public bool AreTimestampsAuthoritative { get; }

            [JsonProperty]
            public string? InitialQuoteState { get; private set; }

            [JsonProperty]
            public List<AdditionalPropertyValueUpsertModel>? AdditionalProperties { get; }

            [JsonProperty]
            public Guid? ProductReleaseId { get; private set; }
        }
    }
}
