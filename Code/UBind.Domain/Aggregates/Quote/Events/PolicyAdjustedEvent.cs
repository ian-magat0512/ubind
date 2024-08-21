﻿// <copyright file="PolicyAdjustedEvent.cs" company="uBind">
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
        /// Event raised when a quote has been created.
        /// </summary>
        public class PolicyAdjustedEvent : Event<QuoteAggregate, Guid>, IPolicyUpsertEvent
        {
            private QuoteDataSnapshot dataSnapshot;
            private LocalDateTime? effectiveDateTime;
            private LocalDateTime? expiryDateTime;
            private Instant? effectiveTimestamp;
            private Instant? expiryTimestamp;

            /// <summary>
            /// Initializes a new instance of the <see cref="PolicyAdjustedEvent"/> class.
            /// </summary>
            /// <param name="tenantId">The tenant Id.</param>
            /// <param name="aggregateId">The ID of the quote aggregate.</param>
            /// <param name="quoteId">The ID of the quote for the adjustment.</param>
            /// <param name="quoteNumber">The quote number of the quote for the adjustment.</param>
            /// <param name="dataSnapshot">The policy data.</param>
            /// <param name="performingUserId">The userId who adjusted the policy.</param>
            /// <param name="createdTimestamp">A created timestamp.</param>
            public PolicyAdjustedEvent(
                Guid tenantId,
                Guid aggregateId,
                Guid quoteId,
                string quoteNumber,
                LocalDateTime effectiveDateTime,
                Instant effectiveTimestamp,
                LocalDateTime? expiryDateTime,
                Instant? expiryTimestamp,
                QuoteDataSnapshot dataSnapshot,
                Guid? performingUserId,
                Instant createdTimestamp,
                Guid? productReleaseId)
                : base(tenantId, aggregateId, performingUserId, createdTimestamp)
            {
                this.AdjustmentTransactionId = Guid.NewGuid();
                this.QuoteId = quoteId;
                this.QuoteNumber = quoteNumber;
                this.dataSnapshot = dataSnapshot;
                this.EffectiveDateTime = effectiveDateTime;
                this.EffectiveTimestamp = effectiveTimestamp;
                this.ExpiryDateTime = expiryDateTime;
                this.ExpiryTimestamp = expiryTimestamp;
                this.ProductReleaseId = productReleaseId;
            }

            [JsonConstructor]
            private PolicyAdjustedEvent()
            {
            }

            /// <summary>
            /// Gets the ID of the adjustment transaction.
            /// </summary>
            [JsonProperty]
            public Guid AdjustmentTransactionId { get; private set; }

            /// <summary>
            /// Gets an ID uniquely identifying the quote whose form data was updated.
            /// </summary>
            [JsonProperty]
            public Guid? QuoteId { get; private set; }

            /// <summary>
            /// Gets the quote number of the quote for the adjustment.
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
                    : this.PolicyData != null ? this.PolicyData.EffectiveTime != default
                        ? this.PolicyData.EffectiveTime.InZone(Timezones.AET).LocalDateTime
                        : this.PolicyData.EffectiveDate.GetInstantAt4pmAet().InZone(Timezones.AET).LocalDateTime
                    : default;
                set => this.effectiveDateTime = value;
            }

            public Instant EffectiveTimestamp
            {
                get => this.effectiveTimestamp ?? this.PolicyData?.EffectiveTime ?? Instant.MinValue;
                set => this.effectiveTimestamp = value;
            }

            [JsonProperty]
            public LocalDateTime? ExpiryDateTime
            {
                // This uses the expiryDateTime property if available, but has backwards compatibility
                // to support the obsolete PolicyData properties.
                get => this.expiryDateTime.HasValue
                    ? this.expiryDateTime.Value
                    : this.PolicyData != null ? this.PolicyData.ExpiryTime.HasValue && this.PolicyData.ExpiryTime.Value != default
                        ? this.PolicyData.ExpiryTime.Value.InZone(Timezones.AET).LocalDateTime
                        : this.PolicyData.ExpiryDate?.GetInstantAt4pmAet().InZone(Timezones.AET).LocalDateTime : default;
                set => this.expiryDateTime = value;
            }

            public Instant? ExpiryTimestamp
            {
                get => this.expiryTimestamp ?? this.PolicyData?.ExpiryTime ?? Instant.MinValue;
                set => this.expiryTimestamp = value;
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
            [Obsolete("Use DataSnapshot and this class's date properties instead.")]
            public PolicyData PolicyData { get; private set; }

            [JsonProperty]
            public Guid? ProductReleaseId { get; private set; }
        }
    }
}
