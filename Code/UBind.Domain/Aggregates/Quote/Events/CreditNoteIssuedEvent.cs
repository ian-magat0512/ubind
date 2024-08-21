// <copyright file="CreditNoteIssuedEvent.cs" company="uBind">
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
        /// Event raised when a credit note has been created.
        /// </summary>
        public class CreditNoteIssuedEvent : QuoteSnapshotEvent<CreditNoteIssuedEvent>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="CreditNoteIssuedEvent"/> class.
            /// </summary>
            /// <param name="tenantId">The tenant Id.</param>
            /// <param name="aggregateId">The ID of the quote aggregate.</param>
            /// <param name="quote">The quote.</param>
            /// <param name="creditNoteNumber">The reference number of the credit note.</param>
            /// <param name="performingUserId">The userId who created the note.</param>
            /// <param name="createdTimestamp">A created time.</param>
            public CreditNoteIssuedEvent(Guid tenantId, Guid aggregateId, Quote quote, string creditNoteNumber, Guid? performingUserId, Instant createdTimestamp)
                : base(tenantId, aggregateId, quote, performingUserId, createdTimestamp)
            {
                this.CreditNoteNumber = creditNoteNumber;
            }

            [JsonConstructor]
            private CreditNoteIssuedEvent()
            {
            }

            /// <summary>
            /// Gets the credit note number.
            /// </summary>
            [JsonProperty]
            public string CreditNoteNumber { get; private set; }
        }
    }
}
