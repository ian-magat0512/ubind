// <copyright file="OrganisationSystemEventEmitter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.SystemEvents
{
    using System;
    using System.Collections.Generic;
    using NodaTime;
    using StackExchange.Profiling;
    using UBind.Domain;
    using UBind.Domain.Aggregates;
    using UBind.Domain.Aggregates.Organisation;
    using UBind.Domain.Events;
    using UBind.Domain.Events.Payload;
    using UBind.Domain.Services.SystemEvents;

    /// <summary>
    /// This class listens to events on the Organisation aggregate
    /// and raises SystemEvents so that automations can listen
    /// and respond to when something changes on a organisation.
    /// </summary>
    public class OrganisationSystemEventEmitter : IOrganisationSystemEventEmitter
    {
        private readonly ISystemEventService systemEventService;
        private readonly IEventPayloadFactory payloadFactory;
        private readonly IClock clock;

        public OrganisationSystemEventEmitter(
            ISystemEventService systemEventService,
            IEventPayloadFactory payloadFactory,
            IClock clock)
        {
            this.systemEventService = systemEventService;
            this.payloadFactory = payloadFactory;
            this.clock = clock;
        }

        public void Dispatch(
            Organisation aggregate,
            IEvent<Organisation, Guid> @event,
            int sequenceNumber,
            IEnumerable<Type>? observerTypes = null)
        {
            this.DispatchIfHandlerExists(aggregate, @event, sequenceNumber);
        }

        public void Handle(Organisation aggregate, Organisation.OrganisationInitializedEvent @event, int sequenceNumber)
        {
            this.CreateAndProcessSystemEvents(aggregate, @event, @event.AggregateId);
        }

        public async Task Handle(Organisation aggregate, Organisation.OrganisationNameUpdatedEvent @event, int sequenceNumber)
        {
            var payload = await this.payloadFactory.CreateOrganisationEventPayload(aggregate);
            this.CreateAndProcessSystemEventsWithPayload<OrganisationEventPayload>(aggregate, @event, payload, @event.AggregateId);
        }

        public async Task Handle(Organisation aggregate, Organisation.OrganisationAliasUpdatedEvent @event, int sequenceNumber)
        {
            var payload = await this.payloadFactory.CreateOrganisationEventPayload(aggregate);
            this.CreateAndProcessSystemEventsWithPayload<OrganisationEventPayload>(aggregate, @event, payload, @event.AggregateId);
        }

        public void Handle(Organisation aggregate, Organisation.ManagingOrganisationUpdatedEvent @event, int sequenceNumber)
        {
            this.CreateAndProcessSystemEvents(aggregate, @event, @event.AggregateId);
        }

        public async Task Handle(Organisation aggregate, Organisation.OrganisationTenantUpdatedEvent @event, int sequenceNumber)
        {
            var payload = await this.payloadFactory.CreateOrganisationEventPayload(aggregate);
            this.CreateAndProcessSystemEventsWithPayload<OrganisationEventPayload>(aggregate, @event, payload, @event.AggregateId);
        }

        public void Handle(Organisation aggregate, Organisation.SetDefaultOrganisationEvent @event, int sequenceNumber)
        {
            this.CreateAndProcessSystemEvents(aggregate, @event, @event.AggregateId);
        }

        public void Handle(Organisation aggregate, Organisation.SetOrganisationDefaultPortalEvent @event, int sequenceNumber)
        {
            this.CreateAndProcessSystemEvents(aggregate, @event, @event.AggregateId);
        }

        public async Task Handle(Organisation aggregate, Organisation.OrganisationDisabledEvent @event, int sequenceNumber)
        {
            var payload = await this.payloadFactory.CreateOrganisationEventPayload(aggregate);
            this.CreateAndProcessSystemEventsWithPayload<OrganisationEventPayload>(aggregate, @event, payload, @event.AggregateId);
        }

        public async Task Handle(Organisation aggregate, Organisation.OrganisationActivatedEvent @event, int sequenceNumber)
        {
            var payload = await this.payloadFactory.CreateOrganisationEventPayload(aggregate);
            this.CreateAndProcessSystemEventsWithPayload<OrganisationEventPayload>(aggregate, @event, payload, @event.AggregateId);
        }

        public async Task Handle(Organisation aggregate, Organisation.OrganisationDeletedEvent @event, int sequenceNumber)
        {
            var payload = await this.payloadFactory.CreateOrganisationEventPayload(aggregate);
            this.CreateAndProcessSystemEventsWithPayload<OrganisationEventPayload>(aggregate, @event, payload, @event.AggregateId);
        }

        public async Task Handle(Organisation aggregate, Organisation.OrganisationAuthenticationMethodAddedEvent @event, int sequenceNumber)
        {
            var payload = await this.payloadFactory.CreateOrganisationEventPayload(aggregate);
            this.CreateAndProcessSystemEventsWithPayload<OrganisationEventPayload>(aggregate, @event, payload, @event.AggregateId);
        }

        public async Task Handle(Organisation aggregate, Organisation.OrganisationAuthenticationMethodUpdatedEvent @event, int sequenceNumber)
        {
            var payload = await this.payloadFactory.CreateOrganisationEventPayload(aggregate);
            this.CreateAndProcessSystemEventsWithPayload<OrganisationEventPayload>(aggregate, @event, payload, @event.AggregateId);
        }

        public async Task Handle(Organisation aggregate, Organisation.OrganisationAuthenticationMethodDeletedEvent @event, int sequenceNumber)
        {
            var payload = await this.payloadFactory.CreateOrganisationEventPayload(aggregate);
            this.CreateAndProcessSystemEventsWithPayload<OrganisationEventPayload>(aggregate, @event, payload, @event.AggregateId);
        }

        public async Task Handle(Organisation aggregate, Organisation.OrganisationAuthenticationMethodDisabledEvent @event, int sequenceNumber)
        {
            var payload = await this.payloadFactory.CreateOrganisationEventPayload(aggregate);
            this.CreateAndProcessSystemEventsWithPayload<OrganisationEventPayload>(aggregate, @event, payload, @event.AggregateId);
        }

        public async Task Handle(Organisation aggregate, Organisation.OrganisationAuthenticationMethodEnabledEvent @event, int sequenceNumber)
        {
            var payload = await this.payloadFactory.CreateOrganisationEventPayload(aggregate);
            this.CreateAndProcessSystemEventsWithPayload<OrganisationEventPayload>(aggregate, @event, payload, @event.AggregateId);
        }

        public async Task Handle(Organisation aggregate, Organisation.OrganisationIdentityLinkedEvent @event, int sequenceNumber)
        {
            var payload = await this.payloadFactory.CreateOrganisationEventPayload(aggregate);
            this.CreateAndProcessSystemEventsWithPayload<OrganisationEventPayload>(aggregate, @event, payload, @event.AggregateId);
        }

        public async Task Handle(Organisation aggregate, Organisation.OrganisationIdentityUnlinkedEvent @event, int sequenceNumber)
        {
            var payload = await this.payloadFactory.CreateOrganisationEventPayload(aggregate);
            this.CreateAndProcessSystemEventsWithPayload<OrganisationEventPayload>(aggregate, @event, payload, @event.AggregateId);
        }

        public async Task Handle(Organisation aggregate, Organisation.OrganisationLinkedIdentityUpdatedEvent @event, int sequenceNumber)
        {
            var payload = await this.payloadFactory.CreateOrganisationEventPayload(aggregate);
            this.CreateAndProcessSystemEventsWithPayload<OrganisationEventPayload>(aggregate, @event, payload, @event.AggregateId);
        }

        public async Task CreateAndEmitModifiedSystemEvent(Guid tenantId, Guid organisationId)
        {
            var payload = await this.payloadFactory.CreateOrganisationEventPayload(tenantId, organisationId);
            var performingUserId = payload.PerformingUser?.Id;
            var systemEvent = SystemEvent.CreateWithPayload(
                    tenantId,
                    organisationId,
                    default,
                    SystemEventType.OrganisationModified,
                    payload,
                    this.clock.GetCurrentInstant());
            this.AddStandardOrganisationRelationships(systemEvent, organisationId, performingUserId);
            this.systemEventService.BackgroundPersistAndEmit(new List<SystemEvent> { systemEvent });
        }

        private void CreateAndProcessSystemEvents(
            Organisation aggregate, IEvent<Organisation, Guid> aggregateEvent, Guid organisationId, int persistTimeHours = -1)
        {
            var systemEvents = this.CreateSystemEvents(aggregate, aggregateEvent, organisationId, persistTimeHours);

            // once the aggregate has been saved to the database, trigger the system events and automations.
            this.systemEventService.OnAggregateSavedPersistAndEmitSystemEventsInBackgroundOnce(aggregate, systemEvents);
        }

        private void CreateAndProcessSystemEventsWithPayload<TPayload>(Organisation aggregate, IEvent<Organisation, Guid> aggregateEvent, TPayload payload, Guid organisationId, int persistTimeHours = -1)
        {
            using (MiniProfiler.Current.Step("CreateAndProcessSystemEventsWithPayload " + aggregateEvent.GetType().Name))
            {
                var systemEvents = this.CreateSystemEventsWithPayload(aggregate, aggregateEvent, payload, organisationId, persistTimeHours);
                this.systemEventService.OnAggregateSavedPersistAndEmitSystemEventsInBackgroundOnce(aggregate, systemEvents);
            }
        }

        private List<SystemEvent> CreateSystemEvents(
            Organisation aggregate, IEvent<Organisation, Guid> aggregateEvent, Guid organisationId, int expiryTimeHours = -1)
        {
            var systemEvents = SystemEventTypeMap.Map(aggregateEvent)
                .Select(systemEvent =>
                    SystemEvent.CreateWithoutPayload(
                      aggregateEvent.TenantId,
                      aggregate.Id,
                      DeploymentEnvironment.None,
                      systemEvent,
                      aggregateEvent.Timestamp))
                .ToList();

            systemEvents.ForEach(se => this.AddStandardOrganisationRelationships(se, aggregate.Id, aggregateEvent.PerformingUserId));
            return systemEvents;
        }

        private List<SystemEvent> CreateSystemEventsWithPayload<TPayload>(
            Organisation aggregate, IEvent<Organisation, Guid> aggregateEvent, TPayload payload, Guid organisationId, int expiryTimeHours = -1)
        {
            var systemEvents = SystemEventTypeMap.Map(aggregateEvent)
                .Select(systemEvent =>
                    SystemEvent.CreateWithPayload(
                      aggregateEvent.TenantId,
                      organisationId,
                      DeploymentEnvironment.None,
                      systemEvent,
                      payload,
                      aggregateEvent.Timestamp))
                .ToList();

            systemEvents.ForEach(se => this.AddStandardOrganisationRelationships(se, organisationId, aggregateEvent.PerformingUserId));

            return systemEvents;
        }

        private void AddStandardOrganisationRelationships(SystemEvent systemEvent, Guid organisationId, Guid? performingUserId = null)
        {
            systemEvent.AddRelationshipFromEntity(RelationshipType.OrganisationEvent, EntityType.Organisation, organisationId);
            if (performingUserId != null)
            {
                systemEvent.AddRelationshipToEntity(
                    RelationshipType.EventPerformingUser, EntityType.User, performingUserId.Value);
            }
        }
    }
}
