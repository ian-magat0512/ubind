// <copyright file="UserSystemEventEmitterTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Tests.SystemEvents;

using System;
using System.Linq;
using System.Linq.Expressions;
using FluentAssertions;
using Humanizer;
using Moq;
using NodaTime;
using UBind.Application.Automation;
using UBind.Application.SystemEvents;
using UBind.Application.Tests.Fakes;
using UBind.Domain;
using UBind.Domain.Aggregates;
using UBind.Domain.Aggregates.Person;
using UBind.Domain.Aggregates.User;
using UBind.Domain.Events;
using UBind.Domain.Events.Models;
using UBind.Domain.Events.Payload;
using UBind.Domain.Extensions;
using UBind.Domain.Processing;
using UBind.Domain.ReadModel;
using UBind.Domain.ReadModel.User;
using UBind.Domain.Repositories;
using UBind.Domain.Services.SystemEvents;
using UBind.Domain.Tests.Fakes;
using UBind.Persistence.Services;
using Xunit;

[SystemEventTypeExtensionInitialize]
public class UserSystemEventEmitterTests
{
    private Guid tenantId = Guid.NewGuid();
    private Guid userId = Guid.NewGuid();
    private DeploymentEnvironment environment = DeploymentEnvironment.None;

    [Theory]
    [InlineData(nameof(UserAggregate.ActivationInvitationCreatedEvent))]
    [InlineData(nameof(UserAggregate.UserActivatedEvent))]
    [InlineData(nameof(UserAggregate.PasswordChangedEvent))]
    [InlineData(nameof(UserAggregate.UserInitializedEvent))]
    [InlineData(nameof(UserAggregate.UserBlockedEvent))]
    [InlineData(nameof(UserAggregate.UserUnblockedEvent))]
    [InlineData(nameof(UserAggregate.UserTransferredToAnotherOrganisationEvent))]
    [InlineData(nameof(UserAggregate.UserImportedEvent))]
    [InlineData(nameof(UserAggregate.LoginEmailSetEvent))]
    [InlineData(nameof(UserAggregate.RoleAddedEvent))]
    [InlineData(nameof(UserAggregate.UserTypeUpdatedEvent))]
    [InlineData(nameof(UserAggregate.RoleAssignedEvent))]
    [InlineData(nameof(UserAggregate.RoleRetractedEvent))]
    [InlineData(nameof(UserAggregate.UserModifiedTimeUpdatedEvent))]
    private void Handle_Successfully_EmitEvent(string eventName, int persistTimeHours = -1)
    {
        // Arrange
        UserEventPayload fakePayload = new FakeUserEventPayload(this.userId);
        var mockIEventPayloadFactory = new Mock<IEventPayloadFactory>();
        mockIEventPayloadFactory.Setup((p) => p.CreateUserEventPayload(It.IsAny<UserAggregate>(), It.IsAny<Guid>())).Returns(Task.FromResult(fakePayload));
        var mockUserReadModelRepository = new Mock<IUserReadModelRepository>();
        var systemEventRepository = new FakeSystemEventRepository();
        var automationEventTriggerService = new Mock<IAutomationEventTriggerService>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockBackgroundJobClient = new Mock<IJobClient>();
        var systemEventPersistenceService = new SystemEventPersistenceService(
            new Mock<IUBindDbContext>().Object,
            new Mock<IRelationshipRepository>().Object,
            new Mock<ITagRepository>().Object,
            systemEventRepository);
        var systemEventService = new SystemEventService(
            automationEventTriggerService.Object,
            mockBackgroundJobClient.Object,
            systemEventPersistenceService);
        var testClock = new TestClock(false);
        var oneHourLater = testClock.Now().Plus(Duration.FromHours(1));
        var performingUserId = Guid.NewGuid();
        var personDetails = new FakePersonalDetails();
        var personAggregate = PersonAggregate.CreatePersonFromPersonalDetails(
            this.tenantId, Guid.NewGuid(), personDetails, performingUserId, default);
        var userAggregate = UserAggregate.CreateUser(this.tenantId, this.userId, UserType.Client, personAggregate, performingUserId, null, testClock.Timestamp);
        var userSystemEventEmitter = new UserSystemEventEmitter(
            systemEventService,
            mockIEventPayloadFactory.Object,
            mockUserReadModelRepository.Object,
            testClock);
        var @event = this.EventFactory(eventName, userAggregate, personAggregate, performingUserId, testClock.Now());

        // skip enqueing using hangfire and just call the method directly
        mockBackgroundJobClient.Setup(s => s.Enqueue(
            It.IsAny<Expression<Func<SystemEventService, Task>>>(),
            (Guid?)null,
            (Guid?)null,
            (DeploymentEnvironment?)null,
            It.IsAny<TimeSpan>())).Callback((
                Expression<Func<SystemEventService, Task>> methodCall,
                Guid? tenantId,
                Guid? productId,
                DeploymentEnvironment? environment,
                TimeSpan expireAt) => methodCall.Compile().Invoke(systemEventService).GetAwaiter().GetResult());

        // Act
        userSystemEventEmitter.Dispatch(userAggregate, @event, 20);
        userAggregate.OnSavedChanges();

        // Assert
        var systemEvent = systemEventRepository.GetAll().FirstOrDefault();
        systemEvent.Should().NotBeNull();
        systemEvent!.Id.Should().NotBeEmpty();
        systemEvent.TenantId.Should().Be(this.tenantId);
        systemEvent.Environment.Should().Be(this.environment);
        systemEvent.Relationships.Any(
            x => x.Type == RelationshipType.OrganisationEvent && x.FromEntityId == userAggregate.OrganisationId);
        systemEvent.Relationships.Any(
            x => x.Type == RelationshipType.PersonEvent && x.FromEntityId == userAggregate.PersonId);
        systemEvent.Relationships.Any(
            x => x.Type == RelationshipType.CustomerEvent && x.FromEntityId == userAggregate.CustomerId);

        if (persistTimeHours < 0)
        {
            // persists indefinitely
            systemEvent.ExpiryTimestamp.Should().BeNull();
        }
        else
        {
            // persist for X hours
            systemEvent.ExpiryTimestamp.Should().NotBeNull();
            var expectedExpiryTimestamp = systemEvent.CreatedTimestamp.Plus(Duration.FromHours(persistTimeHours));
            systemEvent.ExpiryTimestamp!.Value.Should().Be(expectedExpiryTimestamp);
        }

        var payload = systemEvent.GetPayload<UserEventPayload>();
        if (payload != null)
        {
            payload.PerformingUser.Should().NotBeNull();
        }
    }

    [Theory]
    [InlineData(nameof(SystemEventType.UserSessionInvalidated), 1)]
    [InlineData(nameof(SystemEventType.UserLoggedOut), 8760)]
    [InlineData(nameof(SystemEventType.UserLoginAttemptSucceeded), 8760)]
    [InlineData(nameof(SystemEventType.UserEdited))]
    private async Task CreateAndEmitSystemEvents_Successful(string eventTypeName, int persistTimeHours = -1)
    {
        // Arrange
        var performingUserId = Guid.NewGuid();
        var testClock = new TestClock(false);
        UserEventPayload fakePayload = new FakeUserEventPayload(this.userId);
        var mockIEventPayloadFactory = new Mock<IEventPayloadFactory>();
        mockIEventPayloadFactory.Setup((p) => p.CreateUserEventPayload(It.IsAny<UserReadModel>())).Returns(Task.FromResult(fakePayload));
        var mockUserAggregateRepository = new Mock<IUserAggregateRepository>();
        var mockUserLoginRepository = new Mock<IUserLoginEmailRepository>();
        var systemEventRepository = new FakeSystemEventRepository();
        var automationEventTriggerService = new Mock<IAutomationEventTriggerService>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockBackgroundJobClient = new Mock<IJobClient>();
        var systemEventPersistenceService = new SystemEventPersistenceService(
            new Mock<IUBindDbContext>().Object,
            new Mock<IRelationshipRepository>().Object,
            new Mock<ITagRepository>().Object,
            systemEventRepository);
        var systemEventService = new SystemEventService(
            automationEventTriggerService.Object,
            mockBackgroundJobClient.Object,
            systemEventPersistenceService);
        var personDetails = new FakePersonalDetails();
        var personAggregate = PersonAggregate.CreatePersonFromPersonalDetails(
            this.tenantId, Guid.NewGuid(), personDetails, performingUserId, default);
        var user = new UserReadModel(
            this.userId,
            new PersonData(personAggregate),
            Guid.NewGuid(),
            null,
            testClock.Now(),
            UserType.Client,
            this.environment);
        var mockUserReadModelRepository = new Mock<IUserReadModelRepository>();
        mockUserReadModelRepository
            .Setup((p) => p.GetUser(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(user);
        var oneHourLater = testClock.Now().Plus(Duration.FromHours(1));
        var userSystemEventEmitter = new UserSystemEventEmitter(
            systemEventService,
            mockIEventPayloadFactory.Object,
            mockUserReadModelRepository.Object,
            testClock);

        // skip enqueing using hangfire and just call the method directly
        mockBackgroundJobClient.Setup(s => s.Enqueue(
            It.IsAny<Expression<Func<SystemEventService, Task>>>(),
            (Guid?)null,
            (Guid?)null,
            (DeploymentEnvironment?)null,
            It.IsAny<TimeSpan>())).Callback((
                Expression<Func<SystemEventService, Task>> methodCall,
                Guid? tenantId,
                Guid? productId,
                DeploymentEnvironment? environment,
                TimeSpan expireAt) => methodCall.Compile().Invoke(systemEventService).GetAwaiter().GetResult());

        // Act
        var eventType = Enum.Parse<SystemEventType>(eventTypeName);
        await userSystemEventEmitter.CreateAndEmitSystemEvents(
            this.tenantId,
            user.Id,
            new List<Domain.Events.SystemEventType> { eventType },
            performingUserId,
            testClock.Now());

        // Assert
        var systemEvent = systemEventRepository.GetAll().FirstOrDefault();
        systemEvent.Should().NotBeNull();
        systemEvent!.Id.Should().NotBeEmpty();
        systemEvent.TenantId.Should().Be(this.tenantId);
        systemEvent.Environment.Should().Be(this.environment);
        systemEvent.Relationships.Any(
            x => x.Type == RelationshipType.OrganisationEvent && x.FromEntityId == user.OrganisationId);
        systemEvent.Relationships.Any(
            x => x.Type == RelationshipType.PersonEvent && x.FromEntityId == user.PersonId);
        systemEvent.Relationships.Any(
            x => x.Type == RelationshipType.CustomerEvent && x.FromEntityId == user.CustomerId);

        if (persistTimeHours < 0)
        {
            // persists indefinitely
            systemEvent.ExpiryTimestamp.Should().BeNull();
        }
        else
        {
            // persist for X hours
            systemEvent.ExpiryTimestamp.Should().NotBeNull();
            var expectedExpiryTimestamp = systemEvent.CreatedTimestamp.Plus(Duration.FromHours(persistTimeHours));
            systemEvent.ExpiryTimestamp!.Value.Should().Be(expectedExpiryTimestamp);
        }

        var payload = systemEvent.GetPayload<UserEventPayload>();
        if (payload != null)
        {
            payload.PerformingUser.Should().NotBeNull();
        }
    }

    [Theory]
    [InlineData(nameof(SystemEventType.UserLoginAttemptFailed), 8760)]
    [InlineData(nameof(SystemEventType.UserEmailAddressBlocked), 8760)]
    private async Task CreateAndEmitLoginSystemEvents_Successful(string eventTypeName, int persistTimeHours = -1)
    {
        // Arrange
        var performingUserId = Guid.NewGuid();
        var testClock = new TestClock(false);
        UserEventPayload fakePayload = new FakeUserEventPayload(this.userId);
        var mockIEventPayloadFactory = new Mock<IEventPayloadFactory>();
        mockIEventPayloadFactory.Setup((p) => p.CreateUserEventPayload(It.IsAny<UserReadModel>())).Returns(Task.FromResult(fakePayload));
        var mockUserAggregateRepository = new Mock<IUserAggregateRepository>();
        var mockUserLoginRepository = new Mock<IUserLoginEmailRepository>();
        var systemEventRepository = new FakeSystemEventRepository();
        var automationEventTriggerService = new Mock<IAutomationEventTriggerService>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockBackgroundJobClient = new Mock<IJobClient>();
        var systemEventPersistenceService = new SystemEventPersistenceService(
            new Mock<IUBindDbContext>().Object,
            new Mock<IRelationshipRepository>().Object,
            new Mock<ITagRepository>().Object,
            systemEventRepository);
        var systemEventService = new SystemEventService(
            automationEventTriggerService.Object,
            mockBackgroundJobClient.Object,
            systemEventPersistenceService);
        var personDetails = new FakePersonalDetails();
        var personAggregate = PersonAggregate.CreatePersonFromPersonalDetails(
            this.tenantId, Guid.NewGuid(), personDetails, performingUserId, default);
        var user = new UserReadModel(
            this.userId,
            new PersonData(personAggregate),
            Guid.NewGuid(),
            null,
            testClock.Now(),
            UserType.Client,
            this.environment);
        var mockUserReadModelRepository = new Mock<IUserReadModelRepository>();
        mockUserReadModelRepository
            .Setup((p) => p.GetUser(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(user);
        var oneHourLater = testClock.Now().Plus(Duration.FromHours(1));
        var userSystemEventEmitter = new UserSystemEventEmitter(
            systemEventService,
            mockIEventPayloadFactory.Object,
            mockUserReadModelRepository.Object,
            testClock);

        // skip enqueing using hangfire and just call the method directly
        mockBackgroundJobClient.Setup(s => s.Enqueue(
            It.IsAny<Expression<Func<SystemEventService, Task>>>(),
            (Guid?)null,
            (Guid?)null,
            (DeploymentEnvironment?)null,
            It.IsAny<TimeSpan>())).Callback((
                Expression<Func<SystemEventService, Task>> methodCall,
                Guid? tenantId,
                Guid? productId,
                DeploymentEnvironment? environment,
                TimeSpan expireAt) => methodCall.Compile().Invoke(systemEventService).GetAwaiter().GetResult());

        // Act
        var eventType = Enum.Parse<SystemEventType>(eventTypeName);
        await userSystemEventEmitter.CreateAndEmitLoginSystemEvents(
            this.tenantId,
            user.Id,
            new List<SystemEventType> { eventType },
            testClock.Now());

        // Assert
        var systemEvent = systemEventRepository.GetAll().FirstOrDefault();
        systemEvent.Should().NotBeNull();
        systemEvent!.Id.Should().NotBeEmpty();
        systemEvent.TenantId.Should().Be(this.tenantId);
        systemEvent.Environment.Should().Be(this.environment);
        systemEvent.Relationships.Any(
            x => x.Type == RelationshipType.OrganisationEvent && x.FromEntityId == user.OrganisationId);
        systemEvent.Relationships.Any(
            x => x.Type == RelationshipType.PersonEvent && x.FromEntityId == user.PersonId);
        systemEvent.Relationships.Any(
            x => x.Type == RelationshipType.CustomerEvent && x.FromEntityId == user.CustomerId);

        if (persistTimeHours < 0)
        {
            // persists indefinitely
            systemEvent.ExpiryTimestamp.Should().BeNull();
        }
        else
        {
            // persist for X hours
            systemEvent.ExpiryTimestamp.Should().NotBeNull();
            var expectedExpiryTimestamp = systemEvent.CreatedTimestamp.Plus(Duration.FromHours(persistTimeHours));
            systemEvent.ExpiryTimestamp!.Value.Should().Be(expectedExpiryTimestamp);
        }

        var payload = systemEvent.GetPayload<UserEventPayload>();
        if (payload != null)
        {
            payload.EmailAddress.Should().NotBeNull();
        }
    }

    private IEvent<UserAggregate, Guid> EventFactory(
        string eventName,
        UserAggregate userAggregate,
        PersonAggregate personAggregate,
        Guid performingUserId,
        Instant timestamp)
    {
        var tenantId = userAggregate.TenantId;
        var userId = userAggregate.Id;
        var personId = personAggregate.Id;
        Guid? customerId = null;
        Guid invitationId = Guid.NewGuid();
        string usertype = UserType.Customer.Humanize();
        string newSaltedHashedPassword = "LxgfKTrokpd6$!J0";

        switch (eventName)
        {
            case nameof(UserAggregate.ActivationInvitationCreatedEvent):
                return new UserAggregate.ActivationInvitationCreatedEvent(tenantId, userId, customerId, personId, performingUserId, timestamp);
            case nameof(UserAggregate.UserActivatedEvent):
                return new UserAggregate.UserActivatedEvent(tenantId, userId, invitationId, newSaltedHashedPassword, customerId, personId, performingUserId, timestamp);
            case nameof(UserAggregate.PasswordChangedEvent):
                return new UserAggregate.PasswordChangedEvent(tenantId, userId, invitationId, newSaltedHashedPassword, performingUserId, timestamp);
            case nameof(UserAggregate.UserInitializedEvent):
                return new UserAggregate.UserInitializedEvent(tenantId, userId, UserType.Client, new PersonData(personAggregate), customerId, performingUserId, null, timestamp);
            case nameof(UserAggregate.UserBlockedEvent):
                return new UserAggregate.UserBlockedEvent(tenantId, userId, customerId, personId, performingUserId, timestamp);
            case nameof(UserAggregate.UserUnblockedEvent):
                return new UserAggregate.UserUnblockedEvent(tenantId, userId, customerId, personId, performingUserId, timestamp);
            case nameof(UserAggregate.UserTransferredToAnotherOrganisationEvent):
                return new UserAggregate.UserTransferredToAnotherOrganisationEvent(tenantId, userId, userAggregate.OrganisationId, Guid.NewGuid(), personId, false, performingUserId, timestamp);
            case nameof(UserAggregate.UserImportedEvent):
                return new UserAggregate.UserImportedEvent(tenantId, customerId, new PersonData(personAggregate), performingUserId, null, timestamp);
            case nameof(UserAggregate.LoginEmailSetEvent):
                return new UserAggregate.LoginEmailSetEvent(tenantId, userId, performingUserId, "testemail@email.com", timestamp);
            case nameof(UserAggregate.RoleAddedEvent):
                return new UserAggregate.RoleAddedEvent(tenantId, userId, usertype, performingUserId, timestamp);
            case nameof(UserAggregate.UserTypeUpdatedEvent):
                return new UserAggregate.UserTypeUpdatedEvent(tenantId, userId, usertype, performingUserId, timestamp);
            case nameof(UserAggregate.RoleAssignedEvent):
                return new UserAggregate.RoleAssignedEvent(tenantId, userId, Guid.NewGuid(), performingUserId, timestamp);
            case nameof(UserAggregate.RoleRetractedEvent):
                return new UserAggregate.RoleRetractedEvent(tenantId, userId, Guid.NewGuid(), performingUserId, timestamp);
            case nameof(UserAggregate.UserModifiedTimeUpdatedEvent):
                return new UserAggregate.UserModifiedTimeUpdatedEvent(tenantId, userId, personId, timestamp, performingUserId, timestamp);
            default:
                throw new Exception($"Unrecognized event {eventName} for user");
        }
    }

    private class FakeUserEventPayload : UserEventPayload
    {
        public FakeUserEventPayload(Guid userId)
        {
            this.Tenant = new Domain.Events.Models.Tenant(Guid.NewGuid(), "test-tenant");
            this.Organisation = new Organisation
            {
                Id = Guid.NewGuid(),
                Alias = "test-organisation",
            };
            this.User = new User
            {
                Id = userId,
                DisplayName = "Test User",
            };
            this.PerformingUser = new User
            {
                Id = Guid.NewGuid(),
                DisplayName = "Test User",
            };
        }
    }
}
