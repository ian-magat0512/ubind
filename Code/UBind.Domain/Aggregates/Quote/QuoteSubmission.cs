// <copyright file="QuoteSubmission.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote
{
    using NodaTime;

    /// <summary>
    /// Captures details from the quote that were submitted.
    /// </summary>
    public class QuoteSubmission : QuoteCorollary
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QuoteSubmission"/> class.
        /// </summary>
        /// <param name="dataSnapshotIds">The IDs of the datasets that form a snapshot of the quote data used in the submission.</param>
        /// <param name="createdTimestamp">The time the submission was created.</param>
        public QuoteSubmission(QuoteDataSnapshotIds dataSnapshotIds, Instant createdTimestamp)
            : base(dataSnapshotIds, createdTimestamp)
        {
        }
    }
}
