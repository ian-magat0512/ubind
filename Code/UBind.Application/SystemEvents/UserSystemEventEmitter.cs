// <copyright file="UserSystemEventEmitter.cs" company="uBind">
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
using UBind.Domain.Aggregates.User;
using UBind.Domain.Events;
using UBind.Domain.Events.Payload;
using UBind.Domain.Exceptions;
using UBind.Domain.ReadModel;
using UBind.Domain.ReadModel.User;
using UBind.Domain.Services.SystemEvents;

/// <summary>
/// This class listens to events on the UserAggregate
/// and raises SystemEvents so that automations can listen
/// and respond to when something changes on a user.
/// </summary>
public class UserSystemEventEmitter : IUserSystemEventEmitter
{
    private readonly ISystemEventService systemEventService;
    private readonly IClock clock;
    private readonly IEventPayloadFactory payloadFactory;
    private readonly IUserReadModelRepository userReadModelRepository;

    public UserSystemEventEmitter(
        ISystemEventService systemEventService,
        IEventPayloadFactory payloadFactory,
        IUserReadModelRepository userReadModelRepository,
        IClock clock)
    {
        this.systemEventService = systemEventService;
        this.payloadFactory = payloadFactory;
        this.clock = clock;
        this.userReadModelRepository = userReadModelRepository;
    }

    public void Dispatch(
        UserAggregate aggregate,
        IEvent<UserAggregate, Guid> @event,
        int sequenceNumber,
        IEnumerable<Type>? observerTypes = null)
    {
        this.DispatchIfHandlerExists(aggregate, @event, sequenceNumber);
    }

    public async Task Handle(UserAggregate aggregate, UserAggregate.ActivationInvitationCreatedEvent @event, int sequenceNumber)
    {
        var payload = await this.payloadFactory.CreateUserEventPayload(aggregate);
        this.CreateAndProcessSystemEventsWithPayload<UserEventPayload>(aggregate, @event, payload, @event.AggregateId);
    }

    public async Task Handle(UserAggregate aggregate, UserAggregate.UserActivatedEvent @event, int sequenceNumber)
    {
        var payload = await this.payloadFactory.CreateUserEventPayload(aggregate);
        this.CreateAndProcessSystemEventsWithPayload<UserEventPayload>(aggregate, @event, payload, @event.AggregateId);
    }

    public async Task Handle(UserAggregate aggregate, UserAggregate.PasswordChangedEvent @event, int sequenceNumber)
    {
        var payload = await this.payloadFactory.CreateUserEventPayload(aggregate);
        this.CreateAndProcessSystemEventsWithPayload<UserEventPayload>(aggregate, @event, payload, @event.AggregateId);
    }

    public async Task Handle(UserAggregate aggregate, UserAggregate.PasswordResetInvitationCreatedEvent @event, int sequenceNumber)
    {
        if (string.IsNullOrEmpty(aggregate.LoginEmail))
        {
            throw new ErrorException(Errors.User.CannotCreateIncompleteDetails("LoginEmail"));
        }
        var payload = await this.payloadFactory.CreateUserEventPayload(aggregate.TenantId, aggregate.LoginEmail);
        this.CreateAndProcessSystemEventsWithPayload<UserEventPayload>(aggregate, @event, payload, @event.AggregateId);
    }

    public async Task Handle(UserAggregate aggregate, UserAggregate.UserInitializedEvent @event, int sequenceNumber)
    {
        var payload = await this.payloadFactory.CreateUserEventPayload(aggregate);
        this.CreateAndProcessSystemEventsWithPayload<UserEventPayload>(aggregate, @event, payload, @event.AggregateId);
    }

    public async Task Handle(UserAggregate aggregate, UserAggregate.UserBlockedEvent @event, int sequenceNumber)
    {
        var payload = await this.payloadFactory.CreateUserEventPayload(aggregate);
        this.CreateAndProcessSystemEventsWithPayload<UserEventPayload>(aggregate, @event, payload, @event.AggregateId);
    }

    public async Task Handle(UserAggregate aggregate, UserAggregate.UserUnblockedEvent @event, int sequenceNumber)
    {
        var payload = await this.payloadFactory.CreateUserEventPayload(aggregate);
        this.CreateAndProcessSystemEventsWithPayload<UserEventPayload>(aggregate, @event, payload, @event.AggregateId);
    }

    public async Task Handle(UserAggregate aggregate, UserAggregate.UserTransferredToAnotherOrganisationEvent @event, int sequenceNumber)
    {
        var payload = await this.payloadFactory.CreateUserEventPayload(aggregate);
        this.CreateAndProcessSystemEventsWithPayload<UserEventPayload>(aggregate, @event, payload, @event.AggregateId);
    }

    public async Task Handle(UserAggregate aggregate, UserAggregate.UserImportedEvent @event, int sequenceNumber)
    {
        var payload = await this.payloadFactory.CreateUserEventPayload(aggregate);
        this.CreateAndProcessSystemEventsWithPayload<UserEventPayload>(aggregate, @event, payload, @event.AggregateId);
    }

    public async Task Handle(UserAggregate aggregate, UserAggregate.LoginEmailSetEvent @event, int sequenceNumber)
    {
        var payload = await this.payloadFactory.CreateUserEventPayload(aggregate);
        this.CreateAndProcessSystemEventsWithPayload<UserEventPayload>(aggregate, @event, payload, @event.AggregateId);
    }

    public async Task Handle(UserAggregate aggregate, UserAggregate.RoleAddedEvent @event, int sequenceNumber)
    {
        var payload = await this.payloadFactory.CreateUserEventPayload(aggregate);
        this.CreateAndProcessSystemEventsWithPayload<UserEventPayload>(aggregate, @event, payload, @event.AggregateId);
    }

    public async Task Handle(UserAggregate aggregate, UserAggregate.UserTypeUpdatedEvent @event, int sequenceNumber)
    {
        var payload = await this.payloadFactory.CreateUserEventPayload(aggregate);
        this.CreateAndProcessSystemEventsWithPayload<UserEventPayload>(aggregate, @event, payload, @event.AggregateId);
    }

    public async Task Handle(UserAggregate aggregate, UserAggregate.RoleAssignedEvent @event, int sequenceNumber)
    {
        var payload = await this.payloadFactory.CreateUserEventPayload(aggregate, @event.RoleId);
        this.CreateAndProcessSystemEventsWithPayload<UserEventPayload>(aggregate, @event, payload, @event.AggregateId);
    }

    public async Task Handle(UserAggregate aggregate, UserAggregate.RoleRetractedEvent @event, int sequenceNumber)
    {
        var payload = await this.payloadFactory.CreateUserEventPayload(aggregate, @event.RoleId);
        this.CreateAndProcessSystemEventsWithPayload<UserEventPayload>(aggregate, @event, payload, @event.AggregateId);
    }

    public async Task Handle(UserAggregate aggregate, UserAggregate.UserModifiedTimeUpdatedEvent @event, int sequenceNumber)
    {
        var payload = await this.payloadFactory.CreateUserEventPayload(aggregate);
        this.CreateAndProcessSystemEventsWithPayload<UserEventPayload>(aggregate, @event, payload, @event.AggregateId);
    }

    public async Task Handle(UserAggregate aggregate, UserAggregate.ProfilePictureAssignedToUserEvent @event, int sequenceNumber)
    {
        var payload = await this.payloadFactory.CreateUserEventPayload(aggregate);
        this.CreateAndProcessSystemEventsWithPayload<UserEventPayload>(aggregate, @event, payload, @event.AggregateId);
    }

    public async Task CreateAndEmitSystemEvents(Guid tenantId, Guid userId, List<SystemEventType> eventTypes, Guid? performingUserId = null, Instant? timestamp = null)
    {
        var user = this.userReadModelRepository.GetUser(tenantId, userId);
        if (user == null)
        {
            return;
        }

        timestamp = timestamp ?? this.clock.GetCurrentInstant();
        var payload = await this.payloadFactory.CreateUserEventPayload(user);
        var systemEvents = this.CreateSystemEvents(user, eventTypes, payload, timestamp.Value, performingUserId);
        this.systemEventService.BackgroundPersistAndEmit(systemEvents);
    }

    public async Task CreateAndEmitLoginSystemEvents(
        Guid tenantId,
        Guid userId,
        List<SystemEventType> eventTypes,
        Instant? timestamp = null)
    {
        var user = this.userReadModelRepository.GetUser(tenantId, userId);
        if (user == null)
        {
            return;
        }

        await this.CreateAndEmitLoginSystemEvents(user, eventTypes, timestamp);
    }

    public async Task CreateAndEmitLoginSystemEvents(
        UserReadModel user,
        List<SystemEventType> eventTypes,
        Instant? timestamp = null)
    {
        timestamp ??= this.clock.GetCurrentInstant();
        var payload = await this.payloadFactory.CreateUserEventPayload(user.TenantId, user.LoginEmail);
        var systemEvents = this.CreateSystemEvents(user, eventTypes, payload, timestamp.Value);
        this.systemEventService.BackgroundPersistAndEmit(systemEvents);
    }

    private List<SystemEvent> CreateSystemEvents(
        UserReadModel user,
        List<SystemEventType> eventTypes,
        UserEventPayload payload,
        Instant timestamp,
        Guid? performingUserId = null)
    {
        var systemEvents = new List<SystemEvent>();
        foreach (var eventType in eventTypes)
        {
            var systemEvent = SystemEvent.CreateWithPayload(
                user.TenantId,
                user.OrganisationId,
                user.Environment ?? DeploymentEnvironment.None,
                eventType,
                payload,
                timestamp);
            this.AddStandardUserRelationships(
                systemEvent,
                user.Id,
                user.OrganisationId,
                user.PersonId,
                user.CustomerId,
                performingUserId);
            systemEvents.Add(systemEvent);
        }

        return systemEvents;
    }

    private List<SystemEvent> CreateMappedSystemEventsWithPayload<TPayload>(
        UserAggregate aggregate, IEvent<UserAggregate, Guid> aggregateEvent, TPayload payload, Guid? userId = null)
    {
        var systemEvents = SystemEventTypeMap.Map(aggregateEvent)
            .Select(systemEvent =>
                SystemEvent.CreateWithPayload(
                  aggregateEvent.TenantId,
                  aggregate.OrganisationId,
                  aggregate.Environment ?? DeploymentEnvironment.None,
                  systemEvent,
                  payload,
                  aggregateEvent.Timestamp))
            .ToList();

        systemEvents.ForEach(se =>
        {
            this.AddStandardUserRelationships(se, aggregate, userId, aggregateEvent.PerformingUserId);
        });

        return systemEvents;
    }

    private void CreateAndProcessSystemEventsWithPayload<TPayload>(
        UserAggregate aggregate, IEvent<UserAggregate, Guid> aggregateEvent, TPayload payload, Guid? userId = null)
    {
        using (MiniProfiler.Current.Step("CreateAndProcessSystemEventsWithPayload " + aggregateEvent.GetType().Name))
        {
            var systemEvents = this.CreateMappedSystemEventsWithPayload(aggregate, aggregateEvent, payload, userId);
            this.systemEventService.OnAggregateSavedPersistAndEmitSystemEventsInBackgroundOnce(aggregate, systemEvents);
        }
    }

    private void AddStandardUserRelationships(SystemEvent systemEvent, UserAggregate aggregate, Guid? userId, Guid? performingUserId = null)
    {
        this.AddStandardUserRelationships(
            systemEvent,
            userId,
            aggregate.OrganisationId,
            aggregate.PersonId,
            aggregate.CustomerId,
            performingUserId);
    }

    private void AddStandardUserRelationships(
        SystemEvent systemEvent,
        Guid? userId,
        Guid organisationId,
        Guid? personId,
        Guid? customerId,
        Guid? performingUserId = null)
    {
        systemEvent.AddRelationshipFromEntity(
                RelationshipType.UserEvent, EntityType.User, userId);

        systemEvent.AddRelationshipFromEntity(
            RelationshipType.OrganisationEvent, EntityType.Organisation, organisationId);

        if (performingUserId != null)
        {
            systemEvent.AddRelationshipToEntity(
                RelationshipType.EventPerformingUser, EntityType.User, performingUserId.Value);
        }

        if (customerId.HasValue && customerId.GetValueOrDefault() != default)
        {
            systemEvent.AddRelationshipFromEntity(
                RelationshipType.CustomerEvent, EntityType.Customer, customerId.Value);
        }

        if (personId != default)
        {
            systemEvent.AddRelationshipFromEntity(
                RelationshipType.PersonEvent, EntityType.Person, personId);
        }
    }
}
