// <copyright file="AggregateRootEntity.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Common functionality for aggregates. A class which extends this will be the root entity
    /// for the Aggregate.
    /// Non root entities will just extend IEntity<typeparamref name="TId"/> directly.
    /// The root entity of an aggregate stores all of the aggregate events for the entities within
    /// the aggregate bounded context.
    /// To better understand Aggregates, Domain Events, and Domain Driven Design, please watch the
    /// following pluralsight course:
    /// https://app.pluralsight.com/library/courses/domain-driven-design-in-practice/table-of-contents.
    /// </summary>
    /// <typeparam name="TAggregate">The derived type of the aggregate's root entity.</typeparam>
    /// <typeparam name="TId">The type of the aggregate Id.</typeparam>
    public abstract class AggregateRootEntity<TAggregate, TId> : MutableEntity<TId>, IAggregateRootEntity<TAggregate, TId>
        where TAggregate : AggregateRootEntity<TAggregate, TId>
    {
        private readonly List<IEvent<TAggregate, TId>> unsavedEvents = new List<IEvent<TAggregate, TId>>();
        private int nextEventSequenceNumber;
        private IList<IEvent<TAggregate, TId>> unstrippedEvents = new List<IEvent<TAggregate, TId>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateRootEntity{TAggregate,TId}"/> class.
        /// </summary>
        protected AggregateRootEntity()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateRootEntity{TAggregate,TId}"/> class.
        /// </summary>
        /// <param name="events">Events.</param>
        protected AggregateRootEntity(IEnumerable<IEvent<TAggregate, TId>> events)
        {
            this.ApplyEvents(events, 0);
        }

        /// <summary>
        /// Raised when changes to this instance have been saved to the database
        /// </summary>
        public event EventHandler SavedChanges;

        /// <summary>
        /// Gets the Aggregate Type.
        /// </summary>
        public abstract AggregateType AggregateType { get; }

        /// <summary>
        /// Gets or sets the number of events that have been already persisted.
        /// </summary>
        [JsonIgnore]
        public int PersistedEventCount { get; set; }

        /// <summary>
        /// Gets any unsaved events from the aggregate for persisting.
        /// </summary>
        [JsonIgnore]
        public List<IEvent<TAggregate, TId>> UnsavedEvents => this.unsavedEvents;

        /// <summary>
        /// Gets or sets a value indicating whether events are currently being replayed for this aggregate.
        /// During a replay of all events this flag is set so that read model writers can act differently.
        /// Typically during a full replay, read model writers will want to delete the existing read model so that a fresh
        /// one can be generated.
        /// </summary>
        public bool IsBeingReplayed { get; set; }

        public abstract TAggregate ApplyEventsAfterSnapshot(IEnumerable<IEvent<TAggregate, TId>> events, int latestSnapshotVersion);

        /// <summary>
        /// Called by the aggregate repository when this aggregate has been saved to the database.
        /// </summary>
        public void OnSavedChanges()
        {
            this.SavedChanges?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Gets the next sequence number to use when applying new events.
        /// </summary>
        /// <returns>An integer.</returns>
        public int GetNextEventSequenceNumber()
        {
            return this.nextEventSequenceNumber++;
        }

        /// <summary>
        /// Gets the events that created this aggregate, without any that were silenced due to rollback events.
        /// </summary>
        /// <returns>the events which make up this aggregate, without those events silenced due to rollbacks.</returns>
        public IEnumerable<IEvent<TAggregate, TId>> GetRemainingEventsAfterRollbacks()
        {
            return this.StripOutRolledBackEvents(this.unstrippedEvents).Select(eventWithSequenceNumber => eventWithSequenceNumber.Value);
        }

        /// <inheritdoc/>
        public IEnumerable<KeyValuePair<int, IEvent<TAggregate, TId>>> GetRemainingEventsWithSequenceNumberAfterRollbacks()
        {
            return this.StripOutRolledBackEvents(this.unstrippedEvents);
        }

        /// <inheritdoc/>
        public IEnumerable<IEvent<TAggregate, TId>> GetEventsStrippedByRollbacks()
        {
            return this.GetEventsStrippedByRollbacks(this.unstrippedEvents).Select(eventWithSequenceNumber => eventWithSequenceNumber.Value);
        }

        /// <summary>
        /// This is temporary for debug purposes.
        /// </summary>
        /// <returns>This is temporary.</returns>
        public IEnumerable<IEvent<TAggregate, TId>> GetUnstrippedEvents()
        {
            return this.unstrippedEvents;
        }

        /// <summary>
        /// Apply an event to the aggregate and store it for later persisting.
        /// </summary>
        /// <param name="event">The event to apply.</param>
        public void ApplyNewEvent(IEvent<TAggregate, TId> @event)
        {
            @event.Sequence = this.GetNextEventSequenceNumber();
            this.unsavedEvents.Add(@event);
            this.unstrippedEvents.Add(@event);
            this.ApplyDerivedEvent(@event, @event.Sequence);
        }

        /// <summary>
        /// Clears the list of unprocessed events, releasing associated resources like memory.
        /// It's best called after processing events or when they're no longer needed to maintain performance.
        /// If this method is called after events have been successfully processed, the aggregate remains usable.
        /// </summary>
        public void ClearUnstrippedEvents()
        {
            this.unstrippedEvents.Clear();
        }

        protected void ApplyEvents(IEnumerable<IEvent<TAggregate, TId>> events, int latestSnapshotVersion)
        {
            this.unstrippedEvents = events.ToList();
            var strippedEventWithSequenceNumber = this.StripOutRolledBackEvents(events);
            foreach (var eventWithSequenceNumber in strippedEventWithSequenceNumber)
            {
                var @event = eventWithSequenceNumber.Value;
                var sequenceNumber = eventWithSequenceNumber.Key;
                this.ApplyDerivedEvent(@event, sequenceNumber);
                this.LastModifiedTimestamp = @event.Timestamp;
            }

            var lastEvent = events.LastOrDefault();
            var nextSequenceNumber = (lastEvent?.Sequence + 1) ?? (latestSnapshotVersion != 0 ? latestSnapshotVersion + 1 : 0);
            this.PersistedEventCount = nextSequenceNumber;
            this.nextEventSequenceNumber = nextSequenceNumber;
        }

        /// <summary>
        /// Applies the event on the most derived (non-abstract) class, so that it's handled correctly.
        /// </summary>
        protected abstract void ApplyDerivedEvent(dynamic @event, int sequenceNumber);

        /// <summary>
        /// In reverse sequence order finds any rollback events and removes events since the "rollback to" event up to and including the rollback event.
        /// </summary>
        /// <param name="events">the list of events for this aggregate.</param>
        /// <returns>a list of events for the aggregate that had any rollbacks processed.</returns>
        private IEnumerable<KeyValuePair<int, IEvent<TAggregate, TId>>> StripOutRolledBackEvents(IEnumerable<IEvent<TAggregate, TId>> events)
        {
            // project the events list to a list of KeyValuePairs containing the event AND sequence number
            var eventsWithSequenceNumber = events.Select((@event) => new KeyValuePair<int, IEvent<TAggregate, TId>>(@event.Sequence, @event));

            // do the stripping out
            return this.StripOutRolledBackEventsWithSequenceNumbers(eventsWithSequenceNumber);
        }

        private IEnumerable<KeyValuePair<int, IEvent<TAggregate, TId>>> StripOutRolledBackEventsWithSequenceNumbers(IEnumerable<KeyValuePair<int, IEvent<TAggregate, TId>>> eventsWithSequenceNumber)
        {
            // get all of the rollbackEvents and their sequence number
            var rollbackEventsWithSequenceNumber = eventsWithSequenceNumber
                    .Where(eventWithSequenceNumber => eventWithSequenceNumber.Value is IAggregateRollbackEvent<TAggregate, TId>);

            // if there are none we can return the full list of events - nothing needs to be stripped out
            int numberOfRollbackEvents = rollbackEventsWithSequenceNumber.Count();
            if (numberOfRollbackEvents == 0)
            {
                return eventsWithSequenceNumber;
            }

            // get the last rollback event and cast it to the more specialised event type
            KeyValuePair<int, IEvent<TAggregate, TId>> ePair = rollbackEventsWithSequenceNumber.LastOrDefault();
            KeyValuePair<int, IAggregateRollbackEvent<TAggregate, TId>> lastRollbackEventWithSequenceNumber =
                new KeyValuePair<int, IAggregateRollbackEvent<TAggregate, TId>>(ePair.Key, ePair.Value as IAggregateRollbackEvent<TAggregate, TId>);

            // set up a filter which will match only those events which have NOT been excluded by the rollback
            Func<int, bool> filterKeepUnRolledBackEventsWithSequenceNumber = (sequenceNumber) =>
            {
                return sequenceNumber > lastRollbackEventWithSequenceNumber.Key || sequenceNumber <= lastRollbackEventWithSequenceNumber.Value.RollbackToSequenceNumber;
            };

            // filter the events out which have been rolled back by this rollback event
            var filteredEventsWithSequenceNumber = eventsWithSequenceNumber.Where(eventWithSequenceNumber => filterKeepUnRolledBackEventsWithSequenceNumber(eventWithSequenceNumber.Key));

            if (numberOfRollbackEvents > 1)
            {
                // further stripping out is required as there are still rollback events yet to be processed
                return this.StripOutRolledBackEventsWithSequenceNumbers(filteredEventsWithSequenceNumber);
            }
            else
            {
                // there are no more rollback events so no more stripping out required, we can return the list.
                return filteredEventsWithSequenceNumber;
            }
        }

        private IEnumerable<KeyValuePair<int, IEvent<TAggregate, TId>>> GetEventsStrippedByRollbacks(
            IEnumerable<IEvent<TAggregate, TId>> events)
        {
            // project the events list to a list of KeyValuePairs containing the event AND sequence number
            var eventsWithSequenceNumber = events.Select((@event) => new KeyValuePair<int, IEvent<TAggregate, TId>>(@event.Sequence, @event));

            return this.GetEventsStrippedByRollbacksWithSequenceNumbers(eventsWithSequenceNumber);
        }

        private IEnumerable<KeyValuePair<int, IEvent<TAggregate, TId>>> GetEventsStrippedByRollbacksWithSequenceNumbers(
            IEnumerable<KeyValuePair<int, IEvent<TAggregate, TId>>> eventsWithSequenceNumber)
        {
            // get all of the rollbackEvents and their sequence number
            var rollbackEventsWithSequenceNumber = eventsWithSequenceNumber
                    .Where(eventWithSequenceNumber => eventWithSequenceNumber.Value is IAggregateRollbackEvent<TAggregate, TId>);

            // if there are none we can return an empty list
            int numberOfRollbackEvents = rollbackEventsWithSequenceNumber.Count();
            if (numberOfRollbackEvents == 0)
            {
                return Enumerable.Empty<KeyValuePair<int, IEvent<TAggregate, TId>>>();
            }

            // filter the events out which have been rolled back by this rollback event
            List<KeyValuePair<int, IEvent<TAggregate, TId>>> rollbackEventsList = rollbackEventsWithSequenceNumber.ToList();
            foreach (var rollbackEventWithSequenceNumber in rollbackEventsWithSequenceNumber.Reverse())
            {
                foreach (var rollbackEvent in rollbackEventsWithSequenceNumber)
                {
                    if (rollbackEvent.Key != rollbackEventWithSequenceNumber.Key
                        && rollbackEvent.Key < rollbackEventWithSequenceNumber.Key
                        && rollbackEvent.Key > ((IAggregateRollbackEvent<TAggregate, TId>)rollbackEventWithSequenceNumber.Value).RollbackToSequenceNumber)
                    {
                        rollbackEventsList.Remove(rollbackEvent);
                    }
                }
            }
            var filteredEventsWithSequenceNumber = eventsWithSequenceNumber.Where(
                e => rollbackEventsList.Any(r =>
                    e.Key <= r.Key
                    && e.Key > ((IAggregateRollbackEvent<TAggregate, TId>)r.Value).RollbackToSequenceNumber));

            // there are no more rollback events so no more stripping out required, we can return the list.
            return filteredEventsWithSequenceNumber;
        }
    }
}
