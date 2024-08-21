// <copyright file="QuoteVersion.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote
{
    using System;
    using System.Text.Json.Serialization;
    using NodaTime;

    /// <summary>
    /// Represents a version of a quote with a number and data snapshot.
    /// </summary>
    public class QuoteVersion
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QuoteVersion"/> class.
        /// </summary>
        /// <param name="versionId">The versionId.</param>
        /// <param name="number">The version number.</param>
        /// <param name="dataSnapshot">A snapshot of the quote data in this version.</param>
        /// <param name="eventSequenceNumber">The sequence number of the version creation event.</param>
        /// <param name="createdTimestamp">The time the version was created.</param>
        [JsonConstructor]
        public QuoteVersion(
            Guid versionId, int number, QuoteDataSnapshot dataSnapshot, int eventSequenceNumber, Instant createdTimestamp)
        {
            this.VersionId = versionId;
            this.Number = number;
            this.EventSequenceNumber = eventSequenceNumber;
            this.DataSnapshot = dataSnapshot;
            this.CreatedTimestamp = createdTimestamp;
        }

        /// <summary>
        /// Gets the version ID.
        /// </summary>
        public Guid VersionId { get; private set; }

        /// <summary>
        /// Gets the version number.
        /// </summary>
        public int Number { get; private set; }

        /// <summary>
        /// Gets the sequence number of the version creation event that created this version.
        /// </summary>
        public int EventSequenceNumber { get; private set; }

        /// <summary>
        /// Gets the data snapshot in this version.
        /// </summary>
        public QuoteDataSnapshot DataSnapshot { get; private set; }

        /// <summary>
        /// Gets the time the version was created.
        /// </summary>
        public Instant CreatedTimestamp { get; private set; }
    }
}
