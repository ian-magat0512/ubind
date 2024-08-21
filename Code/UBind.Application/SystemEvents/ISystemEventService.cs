// <copyright file="ISystemEventService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.SystemEvents
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using UBind.Domain.Aggregates;
    using UBind.Domain.Events;

    /// <summary>
    /// Service for system events.
    /// </summary>
    public interface ISystemEventService
    {
        /// <summary>
        /// Broadcasts an event to subscribers without persisting it, or it's relationships.
        /// </summary>
        /// <param name="systemEvent">The system event.</param>
        Task Emit(SystemEvent systemEvent);

        /// <summary>
        /// DO NOT USE this method, instead use the other Emit method with CancellationToken.
        /// TODO: remove this method once UB-12201 is deployed on all nodes.
        /// Broadcasts events to subscribers without persisting it, or it's relationships.
        /// </summary>
        /// <param name="description">A description of the events being emitted, so that it can be shown
        /// in hangfire.</param>
        [DisplayName("Emit System Events | {1}")]
        [Obsolete]
        Task Emit(List<SystemEvent> systemEvents, string description);

        /// <summary>
        /// Broadcasts events to subscribers without persisting it, or it's relationships.
        /// </summary>
        /// <param name="description">A description of the events being emitted, so that it can be shown
        /// in hangfire.</param>
        /// <param name="cancellationToken">
        /// If this is enqueued as a background job, pass a CancellationToken.None
        /// It will be replaced by Hangfire with it's own cancellation token
        /// and manage the job cancellation.
        /// </param>
        [DisplayName("Emit System Events | {1}")]
        Task Emit(List<SystemEvent> systemEvents, string description, CancellationToken cancellationToken);

        /// <summary>
        /// Sets up an event handler so that after the aggregate is saved, it persists and emits the system events
        /// in a background job.
        /// It does it only once. If the aggregate is saved twice, the system events will not be persisted and emitted twice.
        /// </summary>
        /// <param name="aggregate">The aggregate to be saved.</param>
        /// <param name="systemEvents">The system events to be persisted and emitted.</param>
        void OnAggregateSavedPersistAndEmitSystemEventsInBackgroundOnce<TAggregate>(
                    TAggregate aggregate,
                    List<SystemEvent> systemEvents)
                    where TAggregate : IAggregateRootEntity<TAggregate, Guid>;

        /// <summary>
        /// Persists the system events, tags and relationships in the background,
        /// then broadcasts the event to subsribers.
        /// </summary>
        /// <param name="systemEvents">The systemEvent And Metadata list.</param>
        void BackgroundPersistAndEmit(List<SystemEvent> systemEvents);

        /// <summary>
        /// DO NOT USE this method, instead use the other Persist method with CancellationToken.
        /// TODO: remove this method once UB-12201 is deployed on all nodes.
        /// Persists the system events.
        /// </summary>
        /// <param name="description">A description of the events being emitted, so that it can be shown
        /// in hangfire.</param>
        [DisplayName("Persist System Events | {1}")]
        [Obsolete]
        Task Persist(List<SystemEvent> systemEvents, string description);

        /// <summary>
        /// Persists the system events.
        /// </summary>
        /// <param name="description">A description of the events being emitted, so that it can be shown
        /// in hangfire.</param>
        /// <param name="cancellationToken">
        /// If this is enqueued as a background job, pass a CancellationToken.None
        /// It will be replaced by Hangfire with it's own cancellation token
        /// and manage the job cancellation.
        /// </param>
        [DisplayName("Persist System Events | {1}")]
        Task Persist(List<SystemEvent> systemEvents, string description, CancellationToken cancellationToken);

        /// <summary>
        /// Persists the system events, tags and relationships, then broadcasts the event to subsribers.
        /// </summary>
        /// <param name="systemEvents">The systemEvent And Metadata list.</param>
        Task PersistAndEmit(List<SystemEvent> systemEvents);
    }
}
