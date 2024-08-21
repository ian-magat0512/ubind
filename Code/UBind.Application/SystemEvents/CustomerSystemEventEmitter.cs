// <copyright file="CustomerSystemEventEmitter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.SystemEvents;

using System;
using System.Collections.Generic;
using System.Linq;
using NodaTime;
using StackExchange.Profiling;
using UBind.Domain;
using UBind.Domain.Aggregates;
using UBind.Domain.Aggregates.Customer;
using UBind.Domain.Events;
using UBind.Domain.Events.Payload;
using UBind.Domain.ReadModel;
using UBind.Domain.Services.SystemEvents;

/// <summary>
/// This class listens to events on the CustomerAggregate
/// and raises SystemEvents so that automations can listen
/// and respond to when something changes on a customer.
/// </summary>
public class CustomerSystemEventEmitter : ICustomerSystemEventEmitter
{
    private readonly ISystemEventService systemEventService;
    private readonly IEventPayloadFactory payloadFactory;
    private readonly IClock clock;
    private readonly ICustomerReadModelRepository customerReadModelRepository;

    public CustomerSystemEventEmitter(
        ISystemEventService systemEventService,
        IEventPayloadFactory payloadFactory,
        ICustomerReadModelRepository customReadModelRepository,
        IClock clock)
    {
        this.systemEventService = systemEventService;
        this.payloadFactory = payloadFactory;
        this.clock = clock;
        this.customerReadModelRepository = customReadModelRepository;
    }

    public void Dispatch(
        CustomerAggregate aggregate,
        IEvent<CustomerAggregate, Guid> @event,
        int sequenceNumber,
        IEnumerable<Type>? observerTypes = null)
    {
        this.DispatchIfHandlerExists(aggregate, @event, sequenceNumber);
    }

    public async Task Handle(CustomerAggregate aggregate, CustomerAggregate.CustomerInitializedEvent @event, int sequenceNumber)
    {
        var payload = await this.payloadFactory.CreateCustomerEventPayload(aggregate);
        this.CreateAndProcessSystemEventsWithPayload<CustomerEventPayload>(aggregate, @event, payload, @event.AggregateId);
    }

    public async Task Handle(CustomerAggregate aggregate, CustomerAggregate.CustomerImportedEvent @event, int sequenceNumber)
    {
        var payload = await this.payloadFactory.CreateCustomerEventPayload(aggregate);
        this.CreateAndProcessSystemEventsWithPayload<CustomerEventPayload>(aggregate, @event, payload, @event.AggregateId);
    }

    public async Task Handle(CustomerAggregate aggregate, CustomerAggregate.CustomerOrganisationMigratedEvent @event, int sequenceNumber)
    {
        var payload = await this.payloadFactory.CreateCustomerEventPayload(aggregate);
        this.CreateAndProcessSystemEventsWithPayload<CustomerEventPayload>(aggregate, @event, payload, @event.AggregateId);
    }

    public async Task Handle(CustomerAggregate aggregate, CustomerAggregate.CustomerModifiedTimeUpdatedEvent @event, int sequenceNumber)
    {
        var payload = await this.payloadFactory.CreateCustomerEventPayload(aggregate);
        this.CreateAndProcessSystemEventsWithPayload<CustomerEventPayload>(aggregate, @event, payload, @event.AggregateId);
    }

    public async Task Handle(CustomerAggregate aggregate, CustomerAggregate.OwnershipAssignedEvent @event, int sequenceNumber)
    {
        var payload = await this.payloadFactory.CreateCustomerEventPayload(aggregate);
        this.CreateAndProcessSystemEventsWithPayload<CustomerEventPayload>(aggregate, @event, payload, @event.AggregateId);
    }

    public async Task Handle(CustomerAggregate aggregate, CustomerAggregate.OwnershipUnassignedEvent @event, int sequenceNumber)
    {
        var payload = await this.payloadFactory.CreateCustomerEventPayload(aggregate);
        this.CreateAndProcessSystemEventsWithPayload<CustomerEventPayload>(aggregate, @event, payload, @event.AggregateId);
    }

    public async Task Handle(CustomerAggregate aggregate, CustomerAggregate.CustomerDeletedEvent @event, int sequenceNumber)
    {
        var payload = await this.payloadFactory.CreateCustomerEventPayload(aggregate);
        this.CreateAndProcessSystemEventsWithPayload<CustomerEventPayload>(aggregate, @event, payload, @event.AggregateId);
    }

    public async Task Handle(CustomerAggregate aggregate, CustomerAggregate.CustomerUndeletedEvent @event, int sequenceNumber)
    {
        var payload = await this.payloadFactory.CreateCustomerEventPayload(aggregate);
        this.CreateAndProcessSystemEventsWithPayload<CustomerEventPayload>(aggregate, @event, payload, @event.AggregateId);
    }

    public async Task Handle(CustomerAggregate aggregate, CustomerAggregate.CustomerTransferredToAnotherOrganisationEvent @event, int sequenceNumber)
    {
        var payload = await this.payloadFactory.CreateCustomerEventPayload(aggregate);
        this.CreateAndProcessSystemEventsWithPayload<CustomerEventPayload>(aggregate, @event, payload, @event.AggregateId);
    }

    public async Task Handle(CustomerAggregate aggregate, CustomerAggregate.PortalChangedEvent @event, int sequenceNumber)
    {
        var payload = await this.payloadFactory.CreateCustomerEventPayload(aggregate);
        this.CreateAndProcessSystemEventsWithPayload<CustomerEventPayload>(aggregate, @event, payload, @event.AggregateId);
    }

    public async Task Handle(CustomerAggregate aggregate, CustomerAggregate.CustomerSetPrimaryPersonEvent @event, int sequenceNumber)
    {
        var payload = await this.payloadFactory.CreateCustomerEventPayload(aggregate);
        this.CreateAndProcessSystemEventsWithPayload<CustomerEventPayload>(aggregate, @event, payload, @event.AggregateId);
    }

    /// <inheritdoc/>
    public async Task CreateAndEmitSystemEvents(Guid tenantId, Guid customerId, List<SystemEventType> eventTypes, Guid? performingUserId, Instant? timestamp = null)
    {
        var customer = this.customerReadModelRepository.GetCustomerById(tenantId, customerId);
        if (customer == null)
        {
            return;
        }

        if (timestamp == null)
        {
            timestamp = this.clock.GetCurrentInstant();
        }

        var payload = await this.payloadFactory.CreateCustomerEventPayload(customer);
        var systemEvents = new List<SystemEvent>();
        foreach (var eventType in eventTypes)
        {
            var systemEvent = SystemEvent.CreateWithPayload(
                customer.TenantId,
                customer.OrganisationId,
                customer.Environment,
                eventType,
                payload,
                timestamp.Value);
            this.AddStandardCustomerRelationships(systemEvent, customerId, customer.OrganisationId, customer.OwnerUserId, performingUserId);
            systemEvents.Add(systemEvent);
        }

        this.systemEventService.BackgroundPersistAndEmit(systemEvents);
    }

    private List<SystemEvent> CreateSystemEventsWithPayload<TPayload>(
        CustomerAggregate aggregate, IEvent<CustomerAggregate, Guid> aggregateEvent, TPayload payload, Guid? customerId = null, int expiryTimeHours = -1)
    {
        var systemEvents = SystemEventTypeMap.Map(aggregateEvent)
            .Select(systemEvent =>
                SystemEvent.CreateWithPayload(
                  aggregateEvent.TenantId,
                  aggregate.OrganisationId,
                  aggregate.Environment,
                  systemEvent,
                  payload,
                  aggregateEvent.Timestamp))
            .ToList();

        systemEvents.ForEach(se =>
        {
            this.AddStandardCustomerRelationships(se, aggregate, aggregateEvent.PerformingUserId);
        });

        return systemEvents;
    }

    private void CreateAndProcessSystemEventsWithPayload<TPayload>(
        CustomerAggregate aggregate, IEvent<CustomerAggregate, Guid> aggregateEvent, TPayload payload, Guid? customerId = null, int persistTimeHours = -1)
    {
        using (MiniProfiler.Current.Step("CreateAndProcessSystemEventsWithPayload " + aggregateEvent.GetType().Name))
        {
            var systemEvents = this.CreateSystemEventsWithPayload(aggregate, aggregateEvent, payload, customerId, persistTimeHours);
            this.systemEventService.OnAggregateSavedPersistAndEmitSystemEventsInBackgroundOnce(aggregate, systemEvents);
        }
    }

    private void AddStandardCustomerRelationships(SystemEvent systemEvent, CustomerAggregate aggregate, Guid? performingUserId = null)
    {
        this.AddStandardCustomerRelationships(systemEvent, aggregate.Id, aggregate.OrganisationId, aggregate.OwnerUserId, performingUserId);
    }

    private void AddStandardCustomerRelationships(SystemEvent systemEvent, Guid customerId, Guid organisationId, Guid? ownerUserId, Guid? performingUserId = null)
    {
        systemEvent.AddRelationshipFromEntity(
                RelationshipType.CustomerEvent, EntityType.Customer, customerId);

        systemEvent.AddRelationshipFromEntity(
            RelationshipType.OrganisationEvent, EntityType.Organisation, organisationId);

        if (performingUserId.HasValue)
        {
            systemEvent.AddRelationshipToEntity(
                RelationshipType.EventPerformingUser, EntityType.User, performingUserId.Value);
        }

        if (ownerUserId.GetValueOrDefault(default) != default)
        {
            systemEvent.AddRelationshipFromEntity(
                RelationshipType.UserEvent, EntityType.User, ownerUserId);
        }
    }
}
