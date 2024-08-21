// <copyright file="SystemEventService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.SystemEvents
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using Humanizer;
    using StackExchange.Profiling;
    using UBind.Application.Automation;
    using UBind.Domain.Aggregates;
    using UBind.Domain.Events;
    using UBind.Domain.Processing;
    using UBind.Domain.ReadWriteModel;
    using UBind.Domain.Repositories;

    /// <inheritdoc/>
    public class SystemEventService : ISystemEventService
    {
        private readonly IAutomationEventTriggerService automationEventTriggerService;
        private readonly IJobClient backgroundJobClient;
        private readonly ISystemEventPersistenceService systemEventPersistenceService;

        /// <summary>
        /// Initializes a new instance of the <see cref="SystemEventService"/> class.
        /// </summary>
        /// <param name="eventRepository">The event repository to persist events.</param>
        public SystemEventService(
            IAutomationEventTriggerService automationEventTriggerService,
            IJobClient backgroundJobClient,
            ISystemEventPersistenceService systemEventPersistenceService)
        {
            this.automationEventTriggerService = automationEventTriggerService;
            this.backgroundJobClient = backgroundJobClient;
            this.systemEventPersistenceService = systemEventPersistenceService;
        }

        /// <summary>
        /// Sets up an event handler so that after the aggregate is saved, it persists and emits the system events
        /// in a background job.
        /// It does it only once. If the aggregate is saved twice, the system events will not be persisted and emitted twice.
        /// </summary>
        /// <param name="aggregate">The aggregate to be saved.</param>
        /// <param name="systemEvents">The system events to be persisted and emitted.</param>
        public void OnAggregateSavedPersistAndEmitSystemEventsInBackgroundOnce<TAggregate>(
            TAggregate aggregate,
            List<SystemEvent> systemEvents)
            where TAggregate : IAggregateRootEntity<TAggregate, Guid>
        {
            EventHandler? handler = null;
            aggregate.SavedChanges += handler = (object? instance, EventArgs args) =>
            {
                aggregate.SavedChanges -= handler;
                this.BackgroundPersistAndEmit(systemEvents);
            };
        }

        /// <inheritdoc/>
        public void BackgroundPersistAndEmit(List<SystemEvent> systemEvents)
        {
            using (MiniProfiler.Current.Step(nameof(SystemEventService) + "." + nameof(this.BackgroundPersistAndEmit) + "(Hangfire Enqueue)"))
            {
                string description = string.Join(", ", systemEvents.Select(se => se.EventType.Humanize()));
                var persist = this.backgroundJobClient.Enqueue<SystemEventService>(
                    j => j.Persist(
                        systemEvents,
                        description,
                        CancellationToken.None),
                    expireAfter: TimeSpan.FromMinutes(1));
                this.backgroundJobClient.ContinueJobWith<SystemEventService>(
                    persist,
                    j => j.Emit(
                        systemEvents,
                        description,
                        CancellationToken.None),
                    TimeSpan.FromMinutes(1));
            }
        }

        /// <inheritdoc/>
        public async Task PersistAndEmit(List<SystemEvent> systemEvents)
        {
            string description = string.Join(", ", systemEvents.Select(se => se.EventType.Humanize()));
            await this.Persist(systemEvents, description);
            await this.Emit(systemEvents, description);
        }

        /// <inheritdoc/>
        [DisplayName("Emit System Events | {1}")]
        public async Task Emit(List<SystemEvent> systemEvents, string description)
        {
            await this.Emit(systemEvents, description, CancellationToken.None);
        }

        /// <inheritdoc/>
        [DisplayName("Emit System Events | {1}")]
        public async Task Emit(List<SystemEvent> systemEvents, string description, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await this.CompleteEmit(systemEvents, this.automationEventTriggerService, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task Emit(SystemEvent systemEvent)
        {
            await this.Emit(new List<SystemEvent> { systemEvent }, systemEvent.EventType.Humanize());
        }

        [DisplayName("Persist System Events | {1}")]
        public Task Persist(List<SystemEvent> systemEvents, string description)
        {
            return this.Persist(systemEvents, description, CancellationToken.None);
        }

        [DisplayName("Persist System Events | {1}")]
        public Task Persist(List<SystemEvent> systemEvents, string description, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            this.systemEventPersistenceService.Persist(systemEvents);
            cancellationToken.ThrowIfCancellationRequested();
            IEnumerable<Relationship> relationships = systemEvents.SelectMany(se => se.Relationships);
            IEnumerable<Tag> tags = systemEvents.SelectMany(se => se.Tags);
            this.systemEventPersistenceService.Persist(relationships, tags);
            return Task.CompletedTask;
        }

        private async Task CompleteEmit(
            List<SystemEvent> systemEvents,
            IAutomationEventTriggerService automationEventTriggerService,
            CancellationToken cancellationToken)
        {
            foreach (var systemEvent in systemEvents)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // trigger automations which might be using the event trigger.
                await automationEventTriggerService.HandleSystemEvent(systemEvent, cancellationToken);
            }
        }
    }
}
