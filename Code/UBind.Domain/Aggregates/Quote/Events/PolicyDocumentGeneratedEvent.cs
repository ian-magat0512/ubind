// <copyright file="PolicyDocumentGeneratedEvent.cs" company="uBind">
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
        /// Event raised when a policy document has been generated.
        /// </summary>
        public class PolicyDocumentGeneratedEvent : DocumentGeneratedEvent<PolicyDocumentGeneratedEvent>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="PolicyDocumentGeneratedEvent"/> class.
            /// </summary>
            /// <param name="tenantId">The tenant Id.</param>
            /// <param name="quoteId">The ID of the quote aggregate.</param>
            /// <param name="policyTransactionId">The ID of the transaction the document is for.</param>
            /// <param name="document">The document.</param>
            /// <param name="performingUserId">The userId who generated the document.</param>
            /// <param name="createdTimestamp">A created timestamp.</param>
            public PolicyDocumentGeneratedEvent(Guid tenantId, Guid quoteId, Guid policyTransactionId, QuoteDocument document, Guid? performingUserId, Instant createdTimestamp)
                : base(tenantId, quoteId, document, policyTransactionId, performingUserId, createdTimestamp)
            {
            }

            [JsonConstructor]
            private PolicyDocumentGeneratedEvent()
            {
            }

            /// <summary>
            /// Gets an ID uniquely identifying the quote.
            /// </summary>
            [JsonProperty]
            public Guid PolicyTransactionId
            {
                get
                {
                    // Policy transaction ID will not exist for documents created before introduction of renewals and updates,
                    // in which case the ID of the aggregate will be used.
                    return this.DocumentTargetId == default
                        ? this.AggregateId
                        : this.DocumentTargetId;
                }
            }
        }
    }
}
