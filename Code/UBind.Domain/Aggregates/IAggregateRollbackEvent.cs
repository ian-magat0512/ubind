// <copyright file="IAggregateRollbackEvent.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates
{
    using System.Collections.Generic;

    /// <summary>
    /// Defines a method for getting the sequence number to roll back to.
    /// </summary>
    /// <typeparam name="TAggregate">The aggregate's root entity type.</typeparam>
    /// <typeparam name="TId">The id type.</typeparam>
    public interface IAggregateRollbackEvent<TAggregate, TId>
    {
        /// <summary>
        /// Gets the sequence number to roll back to.
        /// </summary>
        int RollbackToSequenceNumber { get; }

        /// <summary>
        /// Gets the events after the rolled back events have been stripped out.
        /// </summary>
        IEnumerable<IEvent<TAggregate, TId>> ReplayEvents { get; }
    }
}
