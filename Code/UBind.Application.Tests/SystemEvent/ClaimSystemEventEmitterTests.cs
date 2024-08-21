// <copyright file="ClaimSystemEventEmitterTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Tests.Quote
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using FluentAssertions;
    using Moq;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Application.Automation;
    using UBind.Application.SystemEvents;
    using UBind.Application.Tests.Fakes;
    using UBind.Domain;
    using UBind.Domain.Aggregates;
    using UBind.Domain.Aggregates.Claim;
    using UBind.Domain.Aggregates.Claim.Workflow;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Entities;
    using UBind.Domain.Extensions;
    using UBind.Domain.Imports;
    using UBind.Domain.Processing;
    using UBind.Domain.ReadModel.Policy;
    using UBind.Domain.Repositories;
    using UBind.Domain.Tests.Fakes;
    using UBind.Persistence.Services;
    using Xunit;
    using static UBind.Domain.Aggregates.Quote.QuoteAggregate;

    [SystemEventTypeExtensionInitialize]
    public class ClaimSystemEventEmitterTests
    {
        private Guid tenantId = Guid.NewGuid();
        private Guid productId = Guid.NewGuid();
        private DeploymentEnvironment environment = DeploymentEnvironment.Development;
        private string customerPreferredName = "Test Customer preferred Name";
        private string customerFullName = "Test Customer full Name";

        [Theory]
        [InlineData("ClaimInitializedEvent")]
        [InlineData("ClaimActualisedEvent")]
        [InlineData("ClaimAmountUpdatedEvent")]
        [InlineData("ClaimStateChangedEvent")]
        [InlineData("ClaimNumberUpdatedEvent")]
        [InlineData("ClaimFormDataUpdatedEvent", 1)]
        [InlineData("ClaimCalculationResultCreatedEvent")]
        [InlineData("ClaimVersionCreatedEvent")]
        [InlineData("ClaimImportedEvent")]
        [InlineData("ClaimUpdateImportedEvent")]
        [InlineData("ClaimDescriptionUpdatedEvent")]
        [InlineData("ClaimIncidentDateUpdatedEvent")]
        [InlineData("ClaimWorkflowStepAssignedEvent", 1)]
        [InlineData("ClaimEnquiryMadeEvent")]
        [InlineData("ClaimFileAttachedEvent")]
        [InlineData("AssociateClaimWithPolicyEvent")]
        private void Handle_Successfully_EmitEvent(string eventName, int persistTimeHours = -1)
        {
            // Arrange
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
            var claimWorkflowProvider = new Mock<IClaimWorkflowProvider>();
            var performingUserId = Guid.NewGuid();
            var quote = QuoteFactory.CreateNewBusinessQuote(this.tenantId, this.productId, this.environment);
            var quoteAggregate = quote.Aggregate
                .WithCalculationResult(
                    quote.Id,
                    FormDataJsonFactory.GetSampleFormDataJsonForPatching(),
                    CalculationResultJsonFactory.GetSampleCalculationResultForPatching())
                .WithCustomerDetails(quote.Id)
                .WithCustomer()
                .WithCustomerDetails(quote.Id)
                .WithQuoteVersion(quote.Id)
                .WithPolicy(quote.Id);
            var claimAggregate = ClaimAggregate.CreateForPolicy(
                "qwe", quoteAggregate, Guid.NewGuid(), "test", "Test", Guid.Empty, testClock.Now());
            var systemEventEmitter = new ClaimSystemEventEmitter(systemEventService, testClock);
            var @event = this.EventFactory(eventName, claimAggregate, performingUserId);

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
            systemEventEmitter.Dispatch(claimAggregate, @event, 20);
            claimAggregate.OnSavedChanges();

            // Assert
            var systemEvent = systemEventRepository.GetAll().FirstOrDefault();
            systemEvent.Should().NotBeNull();
            systemEvent!.Id.Should().NotBeEmpty();
            systemEvent.ProductId.Should().Be(this.productId);
            systemEvent.TenantId.Should().Be(this.tenantId);
            systemEvent.Environment.Should().Be(this.environment);
            systemEvent.Relationships.Any(
                    x =>
                        x.Type == RelationshipType.OrganisationEvent
                        && x.FromEntityId == claimAggregate.OrganisationId);
            systemEvent.Relationships.Any(
                x =>
                    x.Type == RelationshipType.ClaimEvent
                    && x.FromEntityId == claimAggregate.Id);

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
        }

        private IEvent<ClaimAggregate, Guid> EventFactory(string eventName, ClaimAggregate claimAggregate, Guid? performingUserId)
        {
            var claimId = claimAggregate.Claim.ClaimId;
            var tenantId = claimAggregate.TenantId;
            var claimImportData = JsonConvert.DeserializeObject<ClaimImportData>("{'Description':'description','Amount':200,'PolicyNumber':'BOO1','ClaimNumber':'BOO1','incidentDate':'1/1/01', 'NotifiedDate':'1/1/01'}");

            if (eventName == "ClaimInitializedEvent")
            {
                return new ClaimAggregate.ClaimInitializedEvent(
                    claimAggregate.TenantId,
                    claimAggregate.OrganisationId,
                    claimAggregate.ProductId,
                    claimAggregate.Environment,
                    claimAggregate.IsTestData,
                    claimId,
                    claimAggregate.Claim.PolicyId,
                    string.Empty,
                    string.Empty,
                    claimAggregate.CustomerId,
                    Guid.NewGuid(),
                    string.Empty,
                    string.Empty,
                    performingUserId,
                    default,
                    Timezones.AET);
            }
            else if (eventName == "ClaimOrganisationMigratedEvent")
            {
                return new ClaimAggregate.ClaimOrganisationMigratedEvent(tenantId, claimAggregate.OrganisationId, claimId, performingUserId, default);
            }
            else if (eventName == "ClaimActualisedEvent")
            {
                return new ClaimAggregate.ClaimActualisedEvent(tenantId, claimId, performingUserId, default);
            }
            else if (eventName == "ClaimAmountUpdatedEvent")
            {
                return new ClaimAggregate.ClaimAmountUpdatedEvent(tenantId, claimId, null, performingUserId, default);
            }
            else if (eventName == "ClaimStateChangedEvent")
            {
                return new ClaimAggregate.ClaimStateChangedEvent(tenantId, claimId, ClaimActions.Acknowledge, performingUserId, "incomplete", "acknowledged", default);
            }
            else if (eventName == "ClaimNumberUpdatedEvent")
            {
                return new ClaimAggregate.ClaimNumberUpdatedEvent(tenantId, claimId, string.Empty, performingUserId, default);
            }
            else if (eventName == "ClaimFormDataUpdatedEvent")
            {
                return new ClaimAggregate.ClaimFormDataUpdatedEvent(tenantId, claimId, claimId, string.Empty, performingUserId, default);
            }
            else if (eventName == "ClaimCalculationResultCreatedEvent")
            {
                return new ClaimAggregate.ClaimCalculationResultCreatedEvent(tenantId, claimId, It.IsAny<ClaimCalculationResult>(), performingUserId, default);
            }
            else if (eventName == "ClaimVersionCreatedEvent")
            {
                return new ClaimAggregate.ClaimVersionCreatedEvent(tenantId, claimId, claimAggregate.Claim, performingUserId, default, new List<ClaimFileAttachment>());
            }
            else if (eventName == "ClaimImportedEvent")
            {
                var quoteInitializedEvent = new QuoteInitializedEvent(
                    tenantId,
                    default,
                    Guid.NewGuid(),
                    default,
                    default,
                    default,
                    default,
                    performingUserId,
                    default,
                    Timezones.AET,
                    false,
                    Guid.NewGuid(),
                    default,
                    default);
                var policyReadModel = new PolicyReadModel(quoteInitializedEvent);
                var personAggregate = PersonAggregate.CreatePerson(claimAggregate.TenantId, Guid.NewGuid(), performingUserId, default);
                return new ClaimAggregate.ClaimImportedEvent(
                    string.Empty,
                    policyReadModel,
                    personAggregate,
                    claimImportData!,
                    performingUserId,
                    default,
                    Timezones.AET);
            }
            else if (eventName == "ClaimUpdateImportedEvent")
            {
                return new ClaimAggregate.ClaimUpdateImportedEvent(tenantId, claimId, claimImportData!, performingUserId, default);
            }
            else if (eventName == "ClaimDescriptionUpdatedEvent")
            {
                return new ClaimAggregate.ClaimDescriptionUpdatedEvent(tenantId, claimId, string.Empty, performingUserId, default);
            }
            else if (eventName == "ClaimIncidentDateUpdatedEvent")
            {
                return new ClaimAggregate.ClaimIncidentDateUpdatedEvent(tenantId, claimId, default, performingUserId, default);
            }
            else if (eventName == "ClaimWorkflowStepAssignedEvent")
            {
                return new ClaimAggregate.ClaimWorkflowStepAssignedEvent(tenantId, claimId, claimId, string.Empty, performingUserId, default);
            }
            else if (eventName == "ClaimEnquiryMadeEvent")
            {
                return new ClaimAggregate.ClaimEnquiryMadeEvent(tenantId, claimId, claimAggregate.Claim, performingUserId, default);
            }
            else if (eventName == "ClaimFileAttachedEvent")
            {
                return new ClaimAggregate.ClaimFileAttachedEvent(
                    tenantId, claimId, new ClaimFileAttachment(string.Empty, string.Empty, default, Guid.NewGuid(), default), performingUserId, default);
            }
            else if (eventName == "AssociateClaimWithPolicyEvent")
            {
                return new ClaimAggregate.AssociateClaimWithPolicyEvent(
                    tenantId, claimId, Guid.NewGuid(), claimAggregate.PolicyId != null ? claimAggregate.PolicyId.Value : Guid.Empty, "QQQ", Guid.NewGuid(), claimAggregate.CustomerId, this.customerPreferredName, this.customerFullName, performingUserId, default);
            }

            throw new Exception($"Unrecognized event {eventName} for claims");
        }
    }
}
