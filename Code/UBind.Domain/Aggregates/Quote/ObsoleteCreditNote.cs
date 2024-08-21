// <copyright file="ObsoleteCreditNote.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote
{
    using NodaTime;

    /// <summary>
    /// A transaction record containing the credited amount for a client.
    /// This is OBSOLETE, we should now use the UBind.Domain.Accounting.CreditNote.
    /// </summary>
    public class ObsoleteCreditNote : QuoteCorollary
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ObsoleteCreditNote"/> class.
        /// </summary>
        /// <param name="creditNoteNumber">The credit note reference number.</param>
        /// <param name="dataSnapshotIds">The IDs of the datasets that form a snapshot of the quote data used for the credit note.</param>
        /// <param name="createdTimestamp">The timestamp the credit note was created.</param>
        public ObsoleteCreditNote(string creditNoteNumber, QuoteDataSnapshotIds dataSnapshotIds, Instant createdTimestamp)
            : base(dataSnapshotIds, createdTimestamp)
        {
            this.CreditNoteNumber = creditNoteNumber;
        }

        /// <summary>
        /// Gets the credit note number.
        /// </summary>
        public string CreditNoteNumber { get; private set; }
    }
}
