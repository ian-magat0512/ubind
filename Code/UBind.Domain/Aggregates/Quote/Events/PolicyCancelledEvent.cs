// <copyright file="PolicyCancelledEvent.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote
{
    using System;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Domain.Extensions;

    /// <summary>
    /// Aggregate for quotes.
    /// </summary>
    public partial class QuoteAggregate
    {
        /// <summary>
        /// Event raised when a quote has been cancelled.
        /// </summary>
        public class PolicyCancelledEvent : Event<QuoteAggregate, Guid>, IPolicyUpsertEvent
        {
            private QuoteDataSnapshot dataSnapshot;
            private LocalDateTime? effectiveDateTime;
            private Instant effectiveTimestamp;

            /// <summary>
            /// Initializes a new instance of the <see cref="PolicyCancelledEvent"/> class.
            /// </summary>
            /// <param name="tenantId">The tenant Id.</param>
            /// <param name="aggregateId">The ID of the quote aggregate.</param>
            /// <param name="quoteId">The ID of the quote.</param>
            /// <param name="quoteNumber">The quote number of the quote for the cancellation.</param>
            /// <param name="dataSnapshot">The policy data.</param>
            /// <param name="effectiveDateTime">The Cancellation effective date time.</param>
            /// <param name="performingUserId">The userId who cancels the policy.</param>
            /// <param name="createdTimestamp">A created timestamp.</param>
            public PolicyCancelledEvent(
                Guid tenantId,
                Guid aggregateId,
                Guid quoteId,
                string quoteNumber,
                QuoteDataSnapshot dataSnapshot,
                LocalDateTime effectiveDateTime,
                Instant effectiveTimestamp,
                Guid? performingUserId,
                Instant createdTimestamp,
                Guid? productReleaseId)
                : base(tenantId, aggregateId, performingUserId, createdTimestamp)
            {
                this.CancellationTransactionId = Guid.NewGuid();
                this.QuoteId = quoteId;
                this.QuoteNumber = quoteNumber;
                this.dataSnapshot = dataSnapshot;
                this.EffectiveDateTime = effectiveDateTime;
                this.EffectiveTimestamp = effectiveTimestamp;
                this.ProductReleaseId = productReleaseId;
            }

            [JsonConstructor]
            private PolicyCancelledEvent()
            {
            }

            /// <summary>
            /// Gets the ID of the cancelled quote.
            /// </summary>
            [JsonProperty]
            public Guid? QuoteId { get; private set; }

            /// <summary>
            /// Gets the ID of the quote that last updated the policy that the cancellation is for.
            /// </summary>
            [JsonProperty]
            public Guid ParentQuoteId { get; private set; }

            /// <summary>
            /// Gets the quote number assigned to the quote.
            /// </summary>
            [JsonProperty]
            public string QuoteNumber { get; private set; }

#pragma warning disable CS0618 // Type or member is obsolete
            [JsonProperty]
            public LocalDateTime EffectiveDateTime
            {
                // This uses the effectiveDateTime property if available, but has backwards compatibility
                // to support the obsolete PolicyData properties.
                get => this.effectiveDateTime.HasValue
                    ? this.effectiveDateTime.Value
                    : (this.EffectiveTimestamp != default ? this.EffectiveTimestamp.InZone(Timezones.AET).LocalDateTime : this.PolicyData?.EffectiveDate.GetInstantAt4pmAet().InZone(Timezones.AET).LocalDateTime) ?? LocalDateTime.MinIsoValue;
                set => this.effectiveDateTime = value;
            }

            /// <summary>
            /// Gets the form data used for the policy.
            /// </summary>
            [JsonProperty]
            public QuoteDataSnapshot DataSnapshot
            {
                get => this.dataSnapshot ?? this.PolicyData?.QuoteDataSnapshot;
                private set => this.dataSnapshot = value;
            }
#pragma warning restore CS0618 // Type or member is obsolete

            /// <summary>
            /// Gets the policy data.
            /// </summary>
            [JsonProperty]
            [Obsolete("Use DataSnapshot and this class's LocalDateTime properties instead.")]
            public PolicyData PolicyData { get; private set; }

            /// <summary>
            /// Gets the ID of the cancellation transaction.
            /// </summary>
            /// <remarks>This may be a default value for cancellations that occurred before transactions were introduced.</remarks>
            [JsonProperty]
            public Guid CancellationTransactionId { get; private set; }

            /// <summary>
            /// Gets the cancellation time.
            /// </summary>
            [JsonProperty("cancellationEffectiveTime")]
            public Instant EffectiveTimestamp
            {
                get
                {
                    // When a cancellation time is not specified, it defaults to the time the
                    // cancellation transaction was created.
                    return this.effectiveTimestamp == default
                        ? this.Timestamp
                        : this.effectiveTimestamp;
                }

                set
                {
                    this.effectiveTimestamp = value;
                }
            }

            [JsonIgnore]
            public LocalDateTime? ExpiryDateTime => null;

            [JsonIgnore]
            public Instant? ExpiryTimestamp => null;

            [JsonProperty]
            public Guid? ProductReleaseId { get; private set; }
        }
    }
}
