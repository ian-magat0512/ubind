// <copyright file="IAggregateRootEntity.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Interface for aggregate root entities.
    /// </summary>
    /// <typeparam name="TAggregate">The type of the aggregate's root entity.</typeparam>
    /// <typeparam name="TId">The type of the aggregate's ID.</typeparam>
    public interface IAggregateRootEntity<TAggregate, TId>
    {
        /// <summary>
        /// Raised when changes to this instance have been saved to the database
        /// </summary>
        event EventHandler SavedChanges;

        /// <summary>
        /// Gets an ID uniquely identifying the aggregate.
        /// </summary>
        TId Id { get; }

        /// <summary>
        /// Gets the Aggregate Type.
        /// </summary>
        AggregateType AggregateType { get; }

        /// <summary>
        /// Gets or sets the count of events already persisted.
        /// </summary>
        int PersistedEventCount { get; set; }

        /// <summary>
        /// Gets the unsaved events that require persisting.
        /// </summary>
        List<IEvent<TAggregate, TId>> UnsavedEvents { get; }

        /// <summary>
        /// Gets or sets a value indicating whether events are currently being replayed for this aggregate.
        /// During a replay of all events this flag is set so that read model writers can act differently.
        /// Typically during a full replay, read model writers will want to delete the existing read model so that a fresh
        /// one can be generated.
        /// </summary>
        bool IsBeingReplayed { get; set; }

        /// <summary>
        /// Gets the events that created this aggregate, without any that were silenced due to rollback events.
        /// </summary>
        /// <returns>the events which make up this aggregate, without those events silenced due to rollbacks.</returns>
        IEnumerable<IEvent<TAggregate, TId>> GetRemainingEventsAfterRollbacks();

        /// <summary>
        /// Gets the events with sequence numbers that created this aggregate, without any that were silenced due to rollback events.
        /// </summary>
        /// <returns>The events which make up this aggregate, without those events silenced due to rollbacks.</returns>
        IEnumerable<KeyValuePair<int, IEvent<TAggregate, TId>>> GetRemainingEventsWithSequenceNumberAfterRollbacks();

        /// <summary>
        /// Gets the events that were stripped due to rollbacks.
        /// </summary>
        /// <returns>The events that were stripped due to rollbacks.</returns>
        IEnumerable<IEvent<TAggregate, TId>> GetEventsStrippedByRollbacks();

        /// <summary>
        /// Called by the aggregate repository after this aggregate has been saved to the database.
        /// </summary>
        void OnSavedChanges();

        /// <summary>
        /// Apply the aggregate events from a snapshot.
        /// If the aggregate snapshot has a version of 100, the aggregate will apply the events starting from version 100 onwards,
        /// saving time by not applying all events from the beginning.
        /// </summary>
        /// <param name="events">The events to be loaded.</param>
        /// <returns>Return the aggregate.</returns>
        TAggregate ApplyEventsAfterSnapshot(IEnumerable<IEvent<TAggregate, TId>> events, int finalVersion);
    }
}
