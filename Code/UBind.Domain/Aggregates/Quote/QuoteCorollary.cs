// <copyright file="QuoteCorollary.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote
{
    using System.Text.Json.Serialization;
    using NodaTime;

    /// <summary>
    /// Base class for things arising from applications.
    /// </summary>
    public class QuoteCorollary
    {
        [JsonConstructor]
        public QuoteCorollary()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QuoteCorollary"/> class.
        /// </summary>
        /// <param name="dataSnapshotIds">The IDs of the datasets that form a snapshot of the quote data used in the corollary.</param>
        /// <param name="createdTimestamp">The time the corollary was created.</param>
        public QuoteCorollary(QuoteDataSnapshotIds dataSnapshotIds, Instant createdTimestamp)
        {
            this.DataSnapshotIds = dataSnapshotIds;
            this.CreatedTimestamp = createdTimestamp;
        }

        /// <summary>
        /// Gets the ID of the form update that was used for this corollary.
        /// </summary>
        public QuoteDataSnapshotIds DataSnapshotIds { get; protected set; }

        /// <summary>
        /// Gets the time this corollary was created.
        /// </summary>
        public Instant CreatedTimestamp { get; protected set; }
    }
}
