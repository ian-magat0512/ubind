// <copyright file="OrganisationSystemEventEmitterTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Tests.SystemEvents;

using System;
using System.Linq;
using System.Linq.Expressions;
using FluentAssertions;
using Moq;
using NodaTime;
using UBind.Application.Automation;
using UBind.Application.SystemEvents;
using UBind.Application.Tests.Fakes;
using UBind.Domain;
using UBind.Domain.Aggregates;
using UBind.Domain.Aggregates.Organisation;
using UBind.Domain.Events;
using UBind.Domain.Events.Payload;
using UBind.Domain.Extensions;
using UBind.Domain.Processing;
using UBind.Domain.ReadModel;
using UBind.Domain.Repositories;
using UBind.Domain.Services.SystemEvents;
using UBind.Domain.Tests.Fakes;
using UBind.Persistence.Services;
using Xunit;

[SystemEventTypeExtensionInitialize]
public class OrganisationSystemEventEmitterTests
{
    private Guid tenantId = Guid.NewGuid();
    private Guid performingUserId = Guid.NewGuid();
    private Guid organisationId = Guid.NewGuid();
    private DeploymentEnvironment environment = DeploymentEnvironment.None;

    [Theory]
    [InlineData(nameof(Organisation.OrganisationDisabledEvent))]
    [InlineData(nameof(Organisation.OrganisationNameUpdatedEvent))]
    [InlineData(nameof(Organisation.OrganisationAliasUpdatedEvent))]
    [InlineData(nameof(Organisation.OrganisationDeletedEvent))]
    [InlineData(nameof(Organisation.OrganisationActivatedEvent))]
    [InlineData(nameof(Organisation.OrganisationTenantUpdatedEvent))]

    private void Handle_Successfully_EmitEvent(string eventName, int persistTimeHours = -1)
    {
        // Arrange
        OrganisationEventPayload fakePayload = new FakeOrganisationEventPayload(this.organisationId, this.performingUserId);
        var mockIEventPayloadFactory = new Mock<IEventPayloadFactory>();
        mockIEventPayloadFactory.Setup((p) => p.CreateOrganisationEventPayload(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult(fakePayload));
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
        var organisationAggregate = Organisation.CreateNewOrganisation(this.tenantId, "test-orgnisation", "Test Organisation", Guid.NewGuid(), this.performingUserId, testClock.Timestamp);
        var organisationSystemEventEmitter = new OrganisationSystemEventEmitter(systemEventService, mockIEventPayloadFactory.Object, testClock);
        var @event = this.EventFactory(eventName, organisationAggregate, this.performingUserId, testClock.Now());

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
        organisationSystemEventEmitter.Dispatch(organisationAggregate, @event, 20);
        organisationAggregate.OnSavedChanges();

        // Assert
        var systemEvent = systemEventRepository.GetAll().FirstOrDefault();
        systemEvent.Should().NotBeNull();
        if (systemEvent == null)
        {
            // since it is already asserted above
            return;
        }
        systemEvent.Id.Should().NotBeEmpty();
        systemEvent.TenantId.Should().Be(this.tenantId);
        systemEvent.Environment.Should().Be(this.environment);
        systemEvent.Relationships.Any(x => x.Type == RelationshipType.OrganisationEvent && x.FromEntityId == organisationAggregate.Id);

        if (persistTimeHours < 0)
        {
            // persists indefinitely
            systemEvent.ExpiryTimestamp.Should().BeNull();
        }
        else
        {
            // persist for X hours
            systemEvent.ExpiryTimestamp.Should().NotBeNull();
            if (systemEvent.ExpiryTimestamp == null)
            {
                // since it is already asserted above
                return;
            }
            var expectedExpiryTimestamp = systemEvent.CreatedTimestamp.Plus(Duration.FromHours(persistTimeHours));
            systemEvent.ExpiryTimestamp.Value.Should().Be(expectedExpiryTimestamp);
        }

        var payload = systemEvent.GetPayload<OrganisationEventPayload>();
        if (payload != null)
        {
            payload.PerformingUser.Should().NotBeNull();
        }
    }

    [Fact]
    private async Task CreateAndEmitOrganisationModifiedSystemEvent_Successful()
    {
        // Arrange
        int persistTimeHours = -1;
        OrganisationEventPayload fakePayload = new FakeOrganisationEventPayload(this.organisationId, this.performingUserId);
        var mockIEventPayloadFactory = new Mock<IEventPayloadFactory>();
        mockIEventPayloadFactory.Setup((p) => p.CreateOrganisationEventPayload(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult(fakePayload));
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

        var organisation = new OrganisationReadModel(
            this.tenantId,
            Guid.NewGuid(),
            "test-orgnisation",
            "Test Organisation",
            Guid.NewGuid(),
            true,
            false,
            testClock.Timestamp);
        var organisationSystemEventEmitter = new OrganisationSystemEventEmitter(systemEventService, mockIEventPayloadFactory.Object, testClock);

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
        await organisationSystemEventEmitter.CreateAndEmitModifiedSystemEvent(
            this.tenantId,
            organisation.Id);

        // Assert
        var systemEvent = systemEventRepository.GetAll().FirstOrDefault();
        systemEvent.Should().NotBeNull();
        systemEvent!.EventType.Should().Be(SystemEventType.OrganisationModified);
        systemEvent.Id.Should().NotBeEmpty();
        systemEvent.TenantId.Should().Be(this.tenantId);
        systemEvent.Environment.Should().Be(this.environment);
        systemEvent.Relationships.Any(x => x.Type == RelationshipType.OrganisationEvent && x.FromEntityId == organisation.Id);

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

        var payload = systemEvent.GetPayload<OrganisationEventPayload>();
        if (payload != null)
        {
            payload.PerformingUser.Should().NotBeNull();
        }
    }

    private IEvent<Organisation, Guid> EventFactory(
        string eventName,
        Organisation organisationAggregate,
        Guid performingUserId,
        Instant timestamp)
    {
        var tenantId = organisationAggregate.TenantId;
        var organisationId = organisationAggregate.Id;

        switch (eventName)
        {
            case nameof(Organisation.OrganisationDisabledEvent):
                return new Organisation.OrganisationDisabledEvent(tenantId, organisationId, performingUserId, timestamp);
            case nameof(Organisation.OrganisationNameUpdatedEvent):
                return new Organisation.OrganisationNameUpdatedEvent(tenantId, organisationId, "sub-organisation", performingUserId, timestamp);
            case nameof(Organisation.OrganisationAliasUpdatedEvent):
                return new Organisation.OrganisationAliasUpdatedEvent(tenantId, organisationId, "sub-organisation", performingUserId, timestamp);
            case nameof(Organisation.OrganisationDeletedEvent):
                return new Organisation.OrganisationDeletedEvent(tenantId, organisationId, performingUserId, timestamp);
            case nameof(Organisation.OrganisationActivatedEvent):
                return new Organisation.OrganisationActivatedEvent(tenantId, organisationId, performingUserId, timestamp);
            case nameof(Organisation.OrganisationTenantUpdatedEvent):
                return new Organisation.OrganisationTenantUpdatedEvent(tenantId, organisationId, performingUserId, timestamp);
            default:
                throw new Exception($"Unrecognized event {eventName} for organisation");
        }
    }

    private class FakeOrganisationEventPayload : OrganisationEventPayload
    {
        public FakeOrganisationEventPayload(Guid organisationId, Guid performingUserId)
        {
            this.Tenant = new Domain.Events.Models.Tenant(Guid.NewGuid(), "test-tenant");
            this.Organisation = new Domain.Events.Models.Organisation
            {
                Id = organisationId,
                Alias = "test-organisation",
            };
            this.PerformingUser = new Domain.Events.Models.User
            {
                Id = performingUserId,
                DisplayName = "Test User",
            };
        }
    }
}
