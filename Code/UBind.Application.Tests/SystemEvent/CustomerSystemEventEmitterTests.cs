// <copyright file="CustomerSystemEventEmitterTests.cs" company="uBind">
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
using UBind.Domain.Aggregates.Customer;
using UBind.Domain.Aggregates.Person;
using UBind.Domain.Events;
using UBind.Domain.Events.Models;
using UBind.Domain.Events.Payload;
using UBind.Domain.Extensions;
using UBind.Domain.Processing;
using UBind.Domain.ReadModel;
using UBind.Domain.ReadModel.Customer;
using UBind.Domain.Repositories;
using UBind.Domain.Services.SystemEvents;
using UBind.Domain.Tests.Fakes;
using UBind.Persistence.Services;
using Xunit;

[SystemEventTypeExtensionInitialize]
public class CustomerSystemEventEmitterTests
{
    private Guid tenantId = Guid.NewGuid();
    private Guid customerId = Guid.NewGuid();
    private DeploymentEnvironment environment = DeploymentEnvironment.None;

    [Theory]
    [InlineData(nameof(CustomerAggregate.CustomerInitializedEvent))]
    [InlineData(nameof(CustomerAggregate.CustomerImportedEvent), 1)]
    [InlineData(nameof(CustomerAggregate.CustomerOrganisationMigratedEvent), 1)]
    [InlineData(nameof(CustomerAggregate.CustomerModifiedTimeUpdatedEvent), 1)]
    [InlineData(nameof(CustomerAggregate.OwnershipAssignedEvent))]
    [InlineData(nameof(CustomerAggregate.OwnershipUnassignedEvent))]
    [InlineData(nameof(CustomerAggregate.CustomerTransferredToAnotherOrganisationEvent))]
    [InlineData(nameof(CustomerAggregate.PortalChangedEvent), 1)]
    [InlineData(nameof(CustomerAggregate.CustomerSetPrimaryPersonEvent), 1)]
    [InlineData(nameof(CustomerAggregate.CustomerDeletedEvent))]
    [InlineData(nameof(CustomerAggregate.CustomerUndeletedEvent))]
    private void Handle_Successfully_EmitEvent(string eventName, int persistTimeHours = -1)
    {
        // Arrange
        CustomerEventPayload fakePayload = new FakeCustomerEventPayload(this.customerId);
        var mockIEventPayloadFactory = new Mock<IEventPayloadFactory>();
        mockIEventPayloadFactory.Setup((p) => p.CreateCustomerEventPayload(It.IsAny<CustomerAggregate>())).Returns(Task.FromResult(fakePayload));
        var systemEventRepository = new FakeSystemEventRepository();
        var automationEventTriggerService = new Mock<IAutomationEventTriggerService>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockBackgroundJobClient = new Mock<IJobClient>();
        var mockCustomerReadModelRepository = new Mock<ICustomerReadModelRepository>();
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
        var customerAggregate = CustomerAggregate.CreateNewCustomer(this.tenantId, personAggregate, this.environment, performingUserId, null, testClock.Timestamp);
        var customerSystemEventEmitter = new CustomerSystemEventEmitter(systemEventService, mockIEventPayloadFactory.Object, mockCustomerReadModelRepository.Object, testClock);
        var @event = this.EventFactory(eventName, customerAggregate, personAggregate, performingUserId, testClock.Now());

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
        customerSystemEventEmitter.Dispatch(customerAggregate, @event, 20);
        customerAggregate.OnSavedChanges();

        // Assert
        var systemEvent = systemEventRepository.GetAll().FirstOrDefault();
        systemEvent.Should().NotBeNull();
        systemEvent!.Id.Should().NotBeEmpty();
        systemEvent.TenantId.Should().Be(this.tenantId);
        systemEvent.Environment.Should().Be(this.environment);
        systemEvent.Relationships.Any(
            x => x.Type == RelationshipType.OrganisationEvent && x.FromEntityId == customerAggregate.OrganisationId);

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

        var payload = systemEvent.GetPayload<CustomerEventPayload>();
        if (payload != null)
        {
            payload.PerformingUser.Should().NotBeNull();
        }
    }

    [Fact]
    private async Task CreateAndEmitSystemEvents_Successful()
    {
        // Arrange
        int persistTimeHours = -1;
        CustomerEventPayload fakePayload = new FakeCustomerEventPayload(this.customerId);
        var mockIEventPayloadFactory = new Mock<IEventPayloadFactory>();
        mockIEventPayloadFactory.Setup((p) => p.CreateCustomerEventPayload(It.IsAny<CustomerReadModelDetail>())).Returns(Task.FromResult(fakePayload));
        var systemEventRepository = new FakeSystemEventRepository();
        var automationEventTriggerService = new Mock<IAutomationEventTriggerService>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockBackgroundJobClient = new Mock<IJobClient>();
        var mockCustomerReadModelRepository = new Mock<ICustomerReadModelRepository>();
        CustomerReadModelDetail customer = new CustomerReadModelDetail()
        {
            Id = this.customerId,
            OrganisationId = Guid.NewGuid(),
            TenantId = this.tenantId,
            Environment = this.environment,
        };
        mockCustomerReadModelRepository
            .Setup((p) => p.GetCustomerById(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>())).Returns(customer);
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
        var customerSystemEventEmitter =
            new CustomerSystemEventEmitter(systemEventService, mockIEventPayloadFactory.Object, mockCustomerReadModelRepository.Object, testClock);

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
        var eventType = SystemEventType.CustomerEdited;

        // Act
        await customerSystemEventEmitter.CreateAndEmitSystemEvents(
            this.tenantId,
            this.customerId,
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
            x => x.Type == RelationshipType.OrganisationEvent && x.FromEntityId == customer.OrganisationId);

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

        var payload = systemEvent.GetPayload<CustomerEventPayload>();
        if (payload != null)
        {
            payload.PerformingUser.Should().NotBeNull();
        }
    }

    private IEvent<CustomerAggregate, Guid> EventFactory(
        string eventName,
        CustomerAggregate customerAggregate,
        PersonAggregate personAggregate,
        Guid performingUserId,
        Instant timestamp)
    {
        var tenantId = customerAggregate.TenantId;
        var customerId = customerAggregate.Id;
        var personId = personAggregate.Id;
        var userId = Guid.NewGuid();
        var organisationId = customerAggregate.OrganisationId;
        switch (eventName)
        {
            case nameof(CustomerAggregate.CustomerInitializedEvent):
                return new CustomerAggregate.CustomerInitializedEvent(tenantId, customerId, new PersonData(personAggregate), this.environment, performingUserId, null, timestamp, userId, false);
            case nameof(CustomerAggregate.CustomerImportedEvent):
                return new CustomerAggregate.CustomerImportedEvent(tenantId, new PersonData(personAggregate), this.environment, performingUserId, null, timestamp, false);
            case nameof(CustomerAggregate.CustomerOrganisationMigratedEvent):
                return new CustomerAggregate.CustomerOrganisationMigratedEvent(tenantId, organisationId, personId, customerId, performingUserId, timestamp);
            case nameof(CustomerAggregate.CustomerModifiedTimeUpdatedEvent):
                return new CustomerAggregate.CustomerModifiedTimeUpdatedEvent(tenantId, customerId, personId, timestamp, performingUserId, timestamp);
            case nameof(CustomerAggregate.OwnershipAssignedEvent):
                return new CustomerAggregate.OwnershipAssignedEvent(tenantId, customerId, userId, personId, "Test Owner", performingUserId, timestamp);
            case nameof(CustomerAggregate.OwnershipUnassignedEvent):
                return new CustomerAggregate.OwnershipUnassignedEvent(tenantId, customerId, performingUserId, timestamp);
            case nameof(CustomerAggregate.CustomerTransferredToAnotherOrganisationEvent):
                return new CustomerAggregate.CustomerTransferredToAnotherOrganisationEvent(tenantId, organisationId, customerId, personId, performingUserId, timestamp);
            case nameof(CustomerAggregate.PortalChangedEvent):
                return new CustomerAggregate.PortalChangedEvent(tenantId, customerId, null, performingUserId, timestamp);
            case nameof(CustomerAggregate.CustomerSetPrimaryPersonEvent):
                return new CustomerAggregate.CustomerSetPrimaryPersonEvent(tenantId, customerId, personId, performingUserId, timestamp);
            case nameof(CustomerAggregate.CustomerDeletedEvent):
                return new CustomerAggregate.CustomerDeletedEvent(tenantId, customerId, performingUserId, timestamp);
            case nameof(CustomerAggregate.CustomerUndeletedEvent):
                return new CustomerAggregate.CustomerUndeletedEvent(tenantId, customerId, performingUserId, timestamp);
            default:
                throw new Exception($"Unrecognized event {eventName} for customer");
        }
    }

    private class FakeCustomerEventPayload : CustomerEventPayload
    {
        public FakeCustomerEventPayload(Guid customerId)
        {
            this.Tenant = new Domain.Events.Models.Tenant(Guid.NewGuid(), "test-tenant");
            this.Organisation = new Organisation
            {
                Id = Guid.NewGuid(),
                Alias = "test-organisation",
            };
            this.Customer = new Customer
            {
                Id = customerId,
                DisplayName = "Test Customer",
            };
            this.PerformingUser = new User
            {
                Id = Guid.NewGuid(),
                DisplayName = "Test User",
            };
        }
    }
}
