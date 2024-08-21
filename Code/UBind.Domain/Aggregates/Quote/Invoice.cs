// <copyright file="Invoice.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote
{
    using System.Text.Json.Serialization;
    using NodaTime;

    /// <summary>
    /// A transaction record containing the policy amount for a client.
    /// </summary>
    public class Invoice : QuoteCorollary
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Invoice"/> class.
        /// </summary>
        /// <param name="invoiceNumber">The invoice number.</param>
        /// <param name="dataSnapshotIds">The IDs of the datasets that form a snapshot of the quote data used for the invoice.</param>
        /// <param name="createdTimestamp">The time the invoice was created.</param>
        [JsonConstructor]
        public Invoice(string invoiceNumber, QuoteDataSnapshotIds dataSnapshotIds, Instant createdTimestamp)
            : base(dataSnapshotIds, createdTimestamp)
        {
            this.InvoiceNumber = invoiceNumber;
        }

        /// <summary>
        /// Gets the invoice number.
        /// </summary>
        public string InvoiceNumber { get; private set; }
    }
}
