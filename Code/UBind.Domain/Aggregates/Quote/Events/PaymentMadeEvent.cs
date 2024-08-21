// <copyright file="PaymentMadeEvent.cs" company="uBind">
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
        /// Event raised when payment has been made.
        /// </summary>
        public class PaymentMadeEvent : QuoteSnapshotEvent<PaymentMadeEvent>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="PaymentMadeEvent"/> class.
            /// </summary>
            /// <param name="tenantId">The tenant Id.</param>
            /// <param name="aggregateId">The ID of the quote aggregate.</param>
            /// <param name="quote">The quote.</param>
            /// <param name="paymentDetails">The payment details.</param>
            /// <param name="performingUserId">The userId who made the payment.</param>
            /// <param name="createdTimestamp">A created timestamp.</param>
            public PaymentMadeEvent(
                Guid tenantId,
                Guid aggregateId,
                Quote quote,
                PaymentDetails paymentDetails,
                Guid? performingUserId,
                Instant createdTimestamp)
                : base(tenantId, aggregateId, quote, performingUserId, createdTimestamp)
            {
                this.PaymentDetails = paymentDetails;
            }

            [JsonConstructor]
            private PaymentMadeEvent()
            {
            }

            /// <summary>
            /// Gets the payment details.
            /// </summary>
            [JsonProperty]
            public PaymentDetails PaymentDetails { get; private set; }
        }
    }
}
