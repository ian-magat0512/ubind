// <copyright file="ClaimVersion.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Entities
{
    using System;
    using NodaTime;
    using UBind.Domain.Aggregates.Claim;

    /// <summary>
    /// Represents a version of a claim with a number and data snapshot.
    /// </summary>
    public class ClaimVersion
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClaimVersion"/> class.
        /// </summary>
        /// <param name="versionId">The version Id.</param>
        /// <param name="number">The version number.</param>
        /// <param name="dataSnapshot">A snapshot of the claim data in this version.</param>
        /// <param name="createdTimestamp">The time the version was created.</param>
        public ClaimVersion(Guid versionId, int number, ClaimDataSnapshot dataSnapshot, Instant createdTimestamp)
        {
            this.VersionId = versionId;
            this.Number = number;
            this.DataSnapshot = dataSnapshot;
            this.CreatedTimestamp = createdTimestamp;
        }

        /// <summary>
        /// Gets the version ID.
        /// </summary>
        public Guid VersionId { get; }

        /// <summary>
        /// Gets the version number.
        /// </summary>
        public int Number { get; }

        /// <summary>
        /// Gets the data snapshot in this version.
        /// </summary>
        public ClaimDataSnapshot DataSnapshot { get; }

        /// <summary>
        /// Gets the time the version was created.
        /// </summary>
        public Instant CreatedTimestamp { get; }
    }
}
