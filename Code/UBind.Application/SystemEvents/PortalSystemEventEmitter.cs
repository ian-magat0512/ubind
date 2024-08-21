// <copyright file="PortalSystemEventEmitter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.SystemEvents
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Aggregates;
    using UBind.Domain.Aggregates.Portal;
    using UBind.Domain.Events;

    /// <summary>
    /// This class listens to events on the PortalAggregate
    /// and raises SystemEvents so that automations can listen
    /// and respond to when something changes on a portal.
    /// </summary>
    public class PortalSystemEventEmitter : IPortalSystemEventEmitter
    {
        private readonly IClock clock;
        private readonly ISystemEventService systemEventService;

        public PortalSystemEventEmitter(
            ISystemEventService systemEventService,
            IClock clock)
        {
            this.clock = clock;
            this.systemEventService = systemEventService;
        }

        public void Dispatch(
            PortalAggregate aggregate,
            IEvent<PortalAggregate, Guid> @event,
            int sequenceNumber,
            IEnumerable<Type> observerTypes = null)
        {
            this.DispatchIfHandlerExists(aggregate, @event, sequenceNumber);
        }

        public void Handle(PortalAggregate aggregate, PortalAggregate.PortalCreatedEvent @event, int sequenceNumber)
        {
            this.CreateAndProcessSystemEvents(aggregate, @event, @event.AggregateId);
        }

        public void Handle(PortalAggregate aggregate, PortalAggregate.PortalUpdatedEvent @event, int sequenceNumber)
        {
            this.CreateAndProcessSystemEvents(aggregate, @event, @event.AggregateId);
        }

        public void Handle(PortalAggregate aggregate, PortalAggregate.UpdatePortalStylesEvent @event, int sequenceNumber)
        {
            this.CreateAndProcessSystemEvents(aggregate, @event, @event.AggregateId);
        }

        public void Handle(PortalAggregate aggregate, PortalAggregate.SetPortalLocationEvent @event, int sequenceNumber)
        {
            this.CreateAndProcessSystemEvents(aggregate, @event, @event.AggregateId);
        }

        public void Handle(PortalAggregate aggregate, PortalAggregate.DisablePortalEvent @event, int sequenceNumber)
        {
            this.CreateAndProcessSystemEvents(aggregate, @event, @event.AggregateId);
        }

        public void Handle(PortalAggregate aggregate, PortalAggregate.EnablePortalEvent @event, int sequenceNumber)
        {
            this.CreateAndProcessSystemEvents(aggregate, @event, @event.AggregateId);
        }

        public void Handle(PortalAggregate aggregate, PortalAggregate.DeletePortalEvent @event, int sequenceNumber)
        {
            this.CreateAndProcessSystemEvents(aggregate, @event, @event.AggregateId);
        }

        private void CreateAndProcessSystemEvents(
            PortalAggregate aggregate, IEvent<PortalAggregate, Guid> aggregateEvent, Guid? portalId = null)
        {
            var systemEvents = this.CreateSystemEvents(aggregate, aggregateEvent, portalId);

            // once the aggregate has been saved to the database, trigger the system events and automations.
            this.systemEventService.OnAggregateSavedPersistAndEmitSystemEventsInBackgroundOnce(aggregate, systemEvents);
        }

        private List<SystemEvent> CreateSystemEvents(
            PortalAggregate aggregate, IEvent<PortalAggregate, Guid> aggregateEvent, Guid? portalId = null)
        {
            var systemEvents = SystemEventTypeMap.Map(aggregateEvent)
                .Select(systemEvent =>
                    SystemEvent.CreateWithoutPayload(
                      aggregateEvent.TenantId,
                      aggregate.OrganisationId,
                      null,
                      DeploymentEnvironment.None,
                      systemEvent,
                      aggregateEvent.Timestamp))
                .ToList();

            systemEvents.ForEach(se => this.AddStandardPortalRelationships(se, aggregate, aggregateEvent));
            return systemEvents;
        }

        private void AddStandardPortalRelationships(
            SystemEvent systemEvent, PortalAggregate aggregate, IEvent<PortalAggregate, Guid> aggregateEvent)
        {
            systemEvent.AddRelationshipFromEntity(
                RelationshipType.OrganisationEvent, EntityType.Organisation, aggregate.OrganisationId);
            systemEvent.AddRelationshipFromEntity(
                RelationshipType.PortalEvent, EntityType.Portal, aggregate.Id);

            if (aggregateEvent.PerformingUserId != null)
            {
                systemEvent.AddRelationshipToEntity(
                    RelationshipType.EventPerformingUser, EntityType.User, aggregateEvent.PerformingUserId.Value);
            }
        }
    }
}
