// <copyright file="IQuoteEventRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// A repositry for querying aggregate events.
    /// </summary>
    public interface IQuoteEventRepository
    {
        /// <summary>
        /// Gets a summary of events for a given aggregate with the given ID.
        /// </summary>
        /// <param name="aggregateId">The ID of the aggregate.</param>
        /// <returns>A collection of events.</returns>
        IEnumerable<IQuoteEventSummary> GetEventSummaries(Guid aggregateId);
    }
}
