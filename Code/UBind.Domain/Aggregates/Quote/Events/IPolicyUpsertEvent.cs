// <copyright file="IPolicyUpsertEvent.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote
{
    using System;
    using NodaTime;

    /// <summary>
    /// Interface for exposing common data from policy creation or update events.
    /// </summary>
    public interface IPolicyUpsertEvent
    {
        /// <summary>
        /// Gets the ID of the quote the policy was issued for.
        /// </summary>
        Guid? QuoteId { get; }

        Guid? ProductReleaseId { get; }

        /// <summary>
        /// Gets the policy data.
        /// </summary>
        [Obsolete("Please use the direct properties of the event.")]
        PolicyData PolicyData { get; }

        QuoteDataSnapshot DataSnapshot { get; }

        /// <summary>
        /// Gets the time the event was created.
        /// </summary>
        Instant Timestamp { get; }

        LocalDateTime EffectiveDateTime { get; }

        Instant EffectiveTimestamp { get; }

        LocalDateTime? ExpiryDateTime { get; }

        Instant? ExpiryTimestamp { get; }
    }
}
