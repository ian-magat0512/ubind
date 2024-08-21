// <copyright file="ClaimSystemEventEmitter.cs" company="uBind">
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
    using UBind.Domain.Aggregates.Claim;
    using UBind.Domain.Events;
    using UBind.Domain.Extensions;

    /// <summary>
    /// This class listens to events on the ClaimAggregate
    /// and raises SystemEvents so that automations can listen
    /// and respond to when something changes on a claim.
    /// </summary>
    public class ClaimSystemEventEmitter : IClaimSystemEventEmitter
    {
        private readonly IClock clock;
        private readonly ISystemEventService systemEventService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClaimSystemEventEmitter"/> class.
        /// </summary>
        /// <param name="systemEventService">The system event service.</param>
        /// <param name="clock">The clock instance to get current timestamp.</param>
        public ClaimSystemEventEmitter(
            ISystemEventService systemEventService,
            IClock clock)
        {
            this.clock = clock;
            this.systemEventService = systemEventService;
        }

        public void Dispatch(
            ClaimAggregate aggregate,
            IEvent<ClaimAggregate, Guid> @event,
            int sequenceNumber,
            IEnumerable<Type> observerTypes = null)
        {
            this.DispatchIfHandlerExists(aggregate, @event, sequenceNumber);
        }

        public void Handle(ClaimAggregate aggregate, ClaimAggregate.ClaimActualisedEvent @event, int sequenceNumber)
        {
            this.CreateAndProcessSystemEvents(aggregate, @event, @event.AggregateId);
        }

        public void Handle(ClaimAggregate aggregate, ClaimAggregate.ClaimInitializedEvent @event, int sequenceNumber)
        {
            this.CreateAndProcessSystemEvents(aggregate, @event, @event.AggregateId);
        }

        public void Handle(ClaimAggregate aggregate, ClaimAggregate.ClaimOrganisationMigratedEvent @event, int sequenceNumber)
        {
            this.CreateAndProcessSystemEvents(aggregate, @event, @event.AggregateId);
        }

        public void Handle(ClaimAggregate aggregate, ClaimAggregate.ClaimTransferredToAnotherOrganisationEvent @event, int sequenceNumber)
        {
            this.CreateAndProcessSystemEvents(aggregate, @event, @event.AggregateId);
        }

        public void Handle(ClaimAggregate aggregate, ClaimAggregate.ClaimAmountUpdatedEvent @event, int sequenceNumber)
        {
            this.CreateAndProcessSystemEvents(aggregate, @event, @event.AggregateId);
        }

        public void Handle(ClaimAggregate aggregate, ClaimAggregate.ClaimStatusUpdatedEvent @event, int sequenceNumber)
        {
            this.CreateAndProcessSystemEvents(aggregate, @event, @event.AggregateId);
        }

        public void Handle(ClaimAggregate aggregate, ClaimAggregate.ClaimStateChangedEvent @event, int sequenceNumber)
        {
            var eventPayload = new Dictionary<string, string>
            {
                { "originalState", @event.OriginalState.ToCamelCase() },
                { "resultingState", @event.ResultingState.ToCamelCase() },
            };
            this.CreateAndProcessSystemEventsWithPayload(aggregate, @event, eventPayload, @event.AggregateId);
        }

        public void Handle(ClaimAggregate aggregate, ClaimAggregate.ClaimNumberUpdatedEvent @event, int sequenceNumber)
        {
            this.CreateAndProcessSystemEvents(aggregate, @event, @event.AggregateId);
        }

        public void Handle(ClaimAggregate aggregate, ClaimAggregate.ClaimFormDataUpdatedEvent @event, int sequenceNumber)
        {
            this.CreateAndProcessSystemEvents(aggregate, @event, @event.AggregateId);
        }

        public void Handle(ClaimAggregate aggregate, ClaimAggregate.ClaimCalculationResultCreatedEvent @event, int sequenceNumber)
        {
            this.CreateAndProcessSystemEvents(aggregate, @event, @event.AggregateId);
        }

        public void Handle(ClaimAggregate aggregate, ClaimAggregate.ClaimVersionCreatedEvent @event, int sequenceNumber)
        {
            var systemEvents = this.CreateSystemEvents(aggregate, @event, @event.AggregateId);
            systemEvents.ForEach(se => se.AddRelationshipFromEntity(
                RelationshipType.ClaimVersionEvent, EntityType.ClaimVersion, @event.VersionId));
            this.systemEventService.OnAggregateSavedPersistAndEmitSystemEventsInBackgroundOnce(aggregate, systemEvents);
        }

        public void Handle(ClaimAggregate aggregate, ClaimAggregate.ClaimImportedEvent @event, int sequenceNumber)
        {
            this.CreateAndProcessSystemEvents(aggregate, @event, @event.AggregateId);
        }

        public void Handle(ClaimAggregate aggregate, ClaimAggregate.ClaimUpdateImportedEvent @event, int sequenceNumber)
        {
            this.CreateAndProcessSystemEvents(aggregate, @event, @event.AggregateId);
        }

        public void Handle(ClaimAggregate aggregate, ClaimAggregate.ClaimDescriptionUpdatedEvent @event, int sequenceNumber)
        {
            this.CreateAndProcessSystemEvents(aggregate, @event, @event.AggregateId);
        }

        public void Handle(ClaimAggregate aggregate, ClaimAggregate.ClaimIncidentDateUpdatedEvent @event, int sequenceNumber)
        {
            this.CreateAndProcessSystemEvents(aggregate, @event, @event.AggregateId);
        }

        public void Handle(ClaimAggregate aggregate, ClaimAggregate.ClaimWorkflowStepAssignedEvent @event, int sequenceNumber)
        {
            this.CreateAndProcessSystemEvents(aggregate, @event, @event.AggregateId);
        }

        public void Handle(ClaimAggregate aggregate, ClaimAggregate.ClaimEnquiryMadeEvent @event, int sequenceNumber)
        {
            this.CreateAndProcessSystemEvents(aggregate, @event, @event.AggregateId);
        }

        public void Handle(ClaimAggregate aggregate, ClaimAggregate.ClaimFileAttachedEvent @event, int sequenceNumber)
        {
            var systemEvents = this.CreateSystemEvents(aggregate, @event, @event.AggregateId);

            // TODO: Create attachment relationship, it doesn't exist yet. This will be addressed in the document
            // management Epic: https://confluence.aptiture.com/display/UBIND/Document+Management
            /* systemEvents.ForEach(se => ); */

            this.systemEventService.OnAggregateSavedPersistAndEmitSystemEventsInBackgroundOnce(aggregate, systemEvents);
        }

        public void Handle(ClaimAggregate aggregate, ClaimAggregate.AssociateClaimWithPolicyEvent @event, int sequenceNumber)
        {
            var systemEvents = this.CreateSystemEvents(aggregate, @event, @event.AggregateId);
            systemEvents.ForEach(se => se.AddRelationshipFromEntity(
                RelationshipType.PolicyEvent, EntityType.Policy, @event.PolicyId));
            this.systemEventService.OnAggregateSavedPersistAndEmitSystemEventsInBackgroundOnce(aggregate, systemEvents);
        }

        public void Handle(ClaimAggregate aggregate, ClaimAggregate.DisassociateClaimWithPolicyEvent @event, int sequenceNumber)
        {
            var systemEvents = this.CreateSystemEvents(aggregate, @event, @event.AggregateId);
            systemEvents.ForEach(se => se.RemoveRelationshipFromEntity(RelationshipType.PolicyEvent, EntityType.Policy, @event.PolicyId));
            this.systemEventService.OnAggregateSavedPersistAndEmitSystemEventsInBackgroundOnce(aggregate, systemEvents);
        }

        public void Handle(ClaimAggregate aggregate, ClaimAggregate.ClaimDeletedEvent @event, int sequenceNumber)
        {
            this.CreateAndProcessSystemEvents(aggregate, @event, @event.AggregateId);
        }

        public void Handle(ClaimAggregate aggregate, ClaimAggregate.ClaimVersionFileAttachedEvent @event, int sequenceNumber)
        {
            var systemEvents = this.CreateSystemEvents(aggregate, @event, @event.AggregateId);
            systemEvents.ForEach(se => se.AddRelationshipFromEntity(
                RelationshipType.ClaimVersionEvent, EntityType.ClaimVersion, @event.VersionId));
            this.systemEventService.OnAggregateSavedPersistAndEmitSystemEventsInBackgroundOnce(aggregate, systemEvents);
        }

        public void Handle(ClaimAggregate aggregate, ClaimAggregate.ApplyNewIdEvent @event, int sequenceNumber)
        {
            this.CreateAndProcessSystemEvents(aggregate, @event, @event.AggregateId);
        }

        public void Handle(ClaimAggregate aggregate, ClaimAggregate.OwnershipAssignedEvent @event, int sequenceNumber)
        {
            var systemEvents = this.CreateSystemEvents(aggregate, @event, @event.AggregateId);
            systemEvents.ForEach(se => se.AddRelationshipFromEntity(
                RelationshipType.UserEvent, EntityType.User, @event.UserId));
            this.systemEventService.OnAggregateSavedPersistAndEmitSystemEventsInBackgroundOnce(aggregate, systemEvents);
        }

        public void Handle(ClaimAggregate aggregate, ClaimAggregate.OwnershipUnassignedEvent @event, int sequenceNumber)
        {
            this.CreateAndProcessSystemEvents(aggregate, @event, @event.AggregateId);
        }

        public void Handle(ClaimAggregate aggregate, ClaimAggregate.CustomerAssignedEvent @event, int sequenceNumber)
        {
            this.CreateAndProcessSystemEvents(aggregate, @event, @event.AggregateId);
        }

        private List<SystemEvent> CreateSystemEvents(
            ClaimAggregate aggregate, IEvent<ClaimAggregate, Guid> aggregateEvent, Guid? claimId = null)
        {
            var systemEvents = SystemEventTypeMap.Map(aggregateEvent)
                .Select(systemEvent =>
                    SystemEvent.CreateWithoutPayload(
                      aggregateEvent.TenantId,
                      aggregate.OrganisationId,
                      aggregate.ProductId,
                      aggregate.Environment,
                      systemEvent,
                      aggregateEvent.Timestamp))
                .ToList();

            systemEvents.ForEach(se =>
            {
                se.AddRelationshipFromEntity(
                    RelationshipType.ClaimEvent, EntityType.Claim, claimId);
                this.AddStandardClaimRelationships(se, aggregate, aggregateEvent);
            });

            return systemEvents;
        }

        private List<SystemEvent> CreateSystemEventsWithPayload<TPayload>(
            ClaimAggregate aggregate, IEvent<ClaimAggregate, Guid> aggregateEvent, TPayload payload, Guid? claimId = null)
        {
            var systemEvents = SystemEventTypeMap.Map(aggregateEvent)
                .Select(systemEvent =>
                    SystemEvent.CreateWithPayload(
                      aggregateEvent.TenantId,
                      aggregate.OrganisationId,
                      aggregate.ProductId,
                      aggregate.Environment,
                      systemEvent,
                      payload,
                      aggregateEvent.Timestamp))
                .ToList();

            systemEvents.ForEach(se =>
            {
                se.AddRelationshipFromEntity(
                    RelationshipType.ClaimEvent, EntityType.Claim, claimId);
                this.AddStandardClaimRelationships(se, aggregate, aggregateEvent);
            });

            return systemEvents;
        }

        private void CreateAndProcessSystemEvents(
            ClaimAggregate aggregate, IEvent<ClaimAggregate, Guid> aggregateEvent, Guid? claimId = null)
        {
            var systemEvents = this.CreateSystemEvents(aggregate, aggregateEvent, claimId);
            this.systemEventService.OnAggregateSavedPersistAndEmitSystemEventsInBackgroundOnce(aggregate, systemEvents);
        }

        private void CreateAndProcessSystemEventsWithPayload<TPayload>(
            ClaimAggregate aggregate, IEvent<ClaimAggregate, Guid> aggregateEvent, TPayload payload, Guid? claimId = null)
        {
            var systemEvents = this.CreateSystemEventsWithPayload(aggregate, aggregateEvent, payload, claimId);
            this.systemEventService.OnAggregateSavedPersistAndEmitSystemEventsInBackgroundOnce(aggregate, systemEvents);
        }

        private void AddStandardClaimRelationships(
            SystemEvent systemEvent, ClaimAggregate aggregate, IEvent<ClaimAggregate, Guid> aggregateEvent)
        {
            systemEvent.AddRelationshipFromEntity(
                RelationshipType.OrganisationEvent, EntityType.Organisation, aggregate.OrganisationId);
            systemEvent.AddRelationshipFromEntity(
                RelationshipType.ProductEvent, EntityType.Product, aggregate.ProductId);
            systemEvent.AddRelationshipFromEntity(
                RelationshipType.ClaimEvent, EntityType.Claim, aggregate.Id);

            if (aggregate.PolicyId.HasValue)
            {
                systemEvent.AddRelationshipToEntity(
                    RelationshipType.PolicyEvent, EntityType.Policy, aggregate.PolicyId.Value);
            }

            if (aggregateEvent.PerformingUserId != null)
            {
                systemEvent.AddRelationshipToEntity(
                    RelationshipType.EventPerformingUser, EntityType.User, aggregateEvent.PerformingUserId.Value);
            }

            if (aggregate.CustomerId.GetValueOrDefault() != default)
            {
                systemEvent.AddRelationshipFromEntity(
                    RelationshipType.CustomerEvent, EntityType.Customer, aggregate.CustomerId.Value);
            }
        }
    }
}
