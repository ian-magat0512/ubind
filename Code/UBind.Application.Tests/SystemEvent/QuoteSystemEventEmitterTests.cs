// <copyright file="QuoteSystemEventEmitterTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Tests.Quote;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FluentAssertions;
using Moq;
using Newtonsoft.Json.Linq;
using NodaTime;
using UBind.Application.Automation;
using UBind.Application.SystemEvents;
using UBind.Application.Tests.Fakes;
using UBind.Domain;
using UBind.Domain.Aggregates;
using UBind.Domain.Aggregates.Person;
using UBind.Domain.Aggregates.Quote;
using UBind.Domain.Aggregates.Quote.DataLocator;
using UBind.Domain.Aggregates.Quote.DataLocator.StandardQuoteDataRetriever;
using UBind.Domain.Events;
using UBind.Domain.Events.Models;
using UBind.Domain.Events.Payload;
using UBind.Domain.Extensions;
using UBind.Domain.Json;
using UBind.Domain.Processing;
using UBind.Domain.ReadModel;
using UBind.Domain.ReadWriteModel;
using UBind.Domain.Repositories;
using UBind.Domain.Services.SystemEvents;
using UBind.Domain.Tests.Aggregates.Quote;
using UBind.Domain.Tests.Fakes;
using UBind.Persistence.Services;
using Xunit;

[SystemEventTypeExtensionInitialize]
public class QuoteSystemEventEmitterTests
{
    private Guid tenantId = Guid.NewGuid();
    private Guid productId = Guid.NewGuid();
    private DeploymentEnvironment environment = DeploymentEnvironment.Development;

    [Theory]
    [InlineData("QuoteInitializedEvent", 1)]
    [InlineData("QuoteActualisedEvent")]
    [InlineData("FormDataUpdatedEvent", 1)]
    [InlineData("CustomerAssignedEvent")]
    [InlineData("CalculationResultCreatedEvent", 1)]
    [InlineData("OwnershipAssignedEvent")]
    [InlineData("CustomerDetailsUpdatedEvent")]
    [InlineData("QuoteSubmittedEvent")]
    [InlineData("EnquiryMadeEvent")]
    [InlineData("QuoteSavedEvent")]
    [InlineData("PolicyIssuedEvent")]
    [InlineData("InvoiceIssuedEvent")]
    [InlineData("PaymentMadeEvent")]
    [InlineData("PaymentFailedEvent")]
    [InlineData("FundingProposalCreatedEvent", 1)]
    [InlineData("FundingProposalCreationFailedEvent", 1)]
    [InlineData("FundingProposalAcceptedEvent")]
    [InlineData("FundingProposalAcceptanceFailedEvent")]
    [InlineData("QuoteNumberAssignedEvent")]
    [InlineData("QuoteVersionCreatedEvent")]
    [InlineData("FormDataPatchedEvent", 1)]
    [InlineData("FileAttachedEvent", 1)]
    [InlineData("PolicyCancelledEvent")]
    [InlineData("QuoteDocumentGeneratedEvent")]
    [InlineData("QuoteVersionDocumentGeneratedEvent")]
    [InlineData("PolicyDocumentGeneratedEvent")]
    [InlineData("PolicyAdjustedEvent")]
    [InlineData("PolicyDataPatchedEvent")]
    [InlineData("QuoteDiscardEvent")]
    [InlineData("WorkflowStepAssignedEvent", 1)]
    [InlineData("AdjustmentQuoteCreatedEvent")]
    [InlineData("RenewalQuoteCreatedEvent")]
    [InlineData("PolicyRenewedEvent")]
    [InlineData("QuoteStateChangedEvent")]
    [InlineData("QuoteBoundEvent")]
    [InlineData("QuoteRollbackEvent")]
    [InlineData("CreditNoteIssuedEvent")]
    [InlineData("CancellationQuoteCreatedEvent")]
    [InlineData("QuoteCustomerAssociationInvitationCreatedEvent")]
    [InlineData("QuoteExpiryTimestampSetEvent")]
    [InlineData("QuoteTransferredToAnotherOrganisationEvent")]
    [InlineData("PolicyNumberUpdatedEvent")]
    private void Handle_Successfully_EmitEvent(string eventName, int persistTimeHours = -1)
    {
        // Arrange
        QuoteOperationEventPayload fakePayload = new FakeQuoteEventPayload(Guid.NewGuid());
        var mockIEventPayloadFactory = new Mock<IEventPayloadFactory>();
        mockIEventPayloadFactory.Setup((p) => p.CreateQuoteEventPayload(It.IsAny<QuoteAggregate>(), It.IsAny<Guid>())).Returns(Task.FromResult(fakePayload));
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
        var customerAggregate = Domain.Aggregates.Customer.CustomerAggregate.CreateNewCustomer(
            this.tenantId, personAggregate, this.environment, performingUserId, null, default);

        var quote = QuoteFactory.CreateNewBusinessQuote(this.tenantId, this.productId, this.environment);
        var quoteAggregate = quote.Aggregate
            .WithCustomer(customerAggregate)
            .WithCustomerDetails(quote.Id, personDetails)
            .WithCalculationResult(quote.Id)
            .WithPolicy(quote.Id, "P123");
        var quoteSystemEventEmitter = new QuoteSystemEventEmitter(systemEventService, mockIEventPayloadFactory.Object, testClock);
        var @event = this.EventFactory(eventName, quoteAggregate, performingUserId, testClock.Now(), quote.Id);

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
        quoteSystemEventEmitter.Dispatch(quoteAggregate, @event, 20);
        quoteAggregate.OnSavedChanges();

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
                && x.FromEntityId == quote.Aggregate.OrganisationId);
        systemEvent.Relationships.Any(
            x =>
                x.Type == RelationshipType.QuoteEvent
                && x.FromEntityId == quote.Aggregate.Id);

        switch (persistTimeHours)
        {
            case < 0:
                // persists indefinitely
                systemEvent.ExpiryTimestamp.Should().BeNull();
                break;
            default:
                {
                    // persist for X hours
                    systemEvent.ExpiryTimestamp.Should().NotBeNull();
                    var expectedExpiryTimestamp = systemEvent.CreatedTimestamp.Plus(Duration.FromHours(persistTimeHours));
                    systemEvent.ExpiryTimestamp!.Value.Should().Be(expectedExpiryTimestamp);
                    break;
                }
        }
    }

    [Theory]
    [InlineData("QuoteInitializedEvent")]
    [InlineData("PaymentFailedEvent")]
    [InlineData("FileAttachedEvent")]
    private void Handle_EventEmitted_HasPerformingUserInPayload(string eventName)
    {
        // Arrange
        QuoteOperationEventPayload fakePayload = new FakeQuoteEventPayload(Guid.NewGuid());
        var mockIEventPayloadFactory = new Mock<IEventPayloadFactory>();
        mockIEventPayloadFactory.Setup((p) => p.CreateQuoteEventPayload(It.IsAny<QuoteAggregate>(), It.IsAny<Guid>())).Returns(Task.FromResult(fakePayload));
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
        var customerAggregate = Domain.Aggregates.Customer.CustomerAggregate.CreateNewCustomer(
            this.tenantId, personAggregate, this.environment, performingUserId, null, default);

        var quote = QuoteFactory.CreateNewBusinessQuote(this.tenantId, this.productId, this.environment);
        var quoteAggregate = quote.Aggregate
            .WithCustomer(customerAggregate)
            .WithCustomerDetails(quote.Id, personDetails)
            .WithCalculationResult(quote.Id)
            .WithPolicy(quote.Id, "P123");
        var quoteSystemEventEmitter = new QuoteSystemEventEmitter(systemEventService, mockIEventPayloadFactory.Object, testClock);
        var @event = this.EventFactory(eventName, quoteAggregate, performingUserId, testClock.Now(), quote.Id);

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
        quoteSystemEventEmitter.Dispatch(quoteAggregate, @event, 20);
        quoteAggregate.OnSavedChanges();

        // Assert
        var systemEvent = systemEventRepository.GetAll().FirstOrDefault();
        systemEvent.Should().NotBeNull();
        var payload = systemEvent!.GetPayload<QuoteOperationEventPayload>();
        payload.Should().NotBeNull();
        payload.PerformingUser.Should().NotBeNull();
    }

    [Fact]
    private async Task CreateAndEmitSystemEvents_Successful()
    {
        // Arrange
        QuoteOperationEventPayload fakePayload = new FakeQuoteEventPayload(Guid.NewGuid());
        var mockIEventPayloadFactory = new Mock<IEventPayloadFactory>();
        mockIEventPayloadFactory.Setup((p) => p.CreateQuoteEventPayload(It.IsAny<NewQuoteReadModel>())).Returns(Task.FromResult(fakePayload));
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
        var customerAggregate = Domain.Aggregates.Customer.CustomerAggregate.CreateNewCustomer(
            this.tenantId, personAggregate, this.environment, performingUserId, null, default);
        var quote = QuoteFactory.CreateNewBusinessQuote(this.tenantId, this.productId, this.environment);
        var quoteAggregate = quote.Aggregate
            .WithCustomer(customerAggregate)
            .WithCustomerDetails(quote.Id, personDetails)
            .WithCalculationResult(quote.Id)
            .WithPolicy(quote.Id, "P123");
        var @event = this.EventFactory("QuoteInitializedEvent", quoteAggregate, performingUserId, testClock.Now(), quote.Id);
        var quoteReadModel = new NewQuoteReadModel((QuoteAggregate.QuoteInitializedEvent)@event);
        var quoteSystemEventEmitter = new QuoteSystemEventEmitter(systemEventService, mockIEventPayloadFactory.Object, testClock);

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
        await quoteSystemEventEmitter.CreateAndEmitSystemEvent(
            quoteReadModel,
            SystemEventType.CustomerExpiredQuoteOpened,
            performingUserId,
            testClock.Now());

        // Assert
        var systemEvent = systemEventRepository.GetAll().FirstOrDefault();
        systemEvent.Should().NotBeNull();
        var payload = systemEvent!.GetPayload<QuoteOperationEventPayload>();
        payload.Should().NotBeNull();
        payload.PerformingUser.Should().NotBeNull();
    }

    private IEvent<QuoteAggregate, Guid> EventFactory(
        string eventName,
        QuoteAggregate quoteAggregate,
        Guid performingUser,
        Instant timestamp,
        Guid quoteId)
    {
        var tenantId = quoteAggregate.TenantId;
        var policyTransaction = quoteAggregate.Policy!.Transactions.LastOrDefault();
        var quote = quoteAggregate.GetQuoteOrThrow(quoteId);
        var document = new QuoteDocument(string.Empty, string.Empty, default, Guid.NewGuid(), default);

        var fakeConfig = new FakeDataLocatorConfig(DefaultQuoteDatumLocations.Instance, DefaultDataLocations.Instance);
        var quoteDataRetriever = new StandardQuoteDataRetriever(fakeConfig, this.GetFormData(), this.GetCalculationData());

        switch (eventName)
        {
            case "AdjustmentQuoteCreatedEvent":
                return new QuoteAggregate.AdjustmentQuoteCreatedEvent(
                            tenantId, quoteAggregate.Id, quoteAggregate.OrganisationId, "QN10", string.Empty, Guid.Empty, performingUser, default, Guid.NewGuid());
            case "FundingProposalCreationFailedEvent":
                {
                    var cancellationQuote = new CancellationQuote(
                        Guid.Empty, quoteAggregate, 1, "2123", null, default, "{}", Guid.NewGuid());
                    var duration = 30;
                    var startDate = new NodaTime.LocalDate(2020, 7, 22);
                    var endDate = startDate.PlusDays(duration);
                    var formDataSchema = new FormDataSchema(new JObject());
                    var calculationData = new CachingJObjectWrapper(CalculationResultJsonFactory.Create());
                    var cancelFormDataJson = FormDataJsonFactory.GetSampleWithStartEndAndEffectiveAndCancelllationDatesInDays(startDate, duration, 30);
                    var quoteDataRetreiver = new StandardQuoteDataRetriever(
                                new FormData(cancelFormDataJson),
                                new CachingJObjectWrapper(CalculationResultJsonFactory.Create()));
                    var calculationResult = CalculationResult.CreateForCancellation(
                        calculationData,
                        quoteDataRetreiver,
                        quote.LatestCalculationResult.Data.CompoundPrice,
                        true,
                        startDate,
                        endDate);
                    calculationResult.FormDataId = cancellationQuote.LatestFormData.Id;
                    cancellationQuote.SeedWithCalculationResult(Guid.NewGuid(), calculationResult, default);
                    return new QuoteAggregate.FundingProposalCreationFailedEvent(
                        tenantId, quoteAggregate.Id, cancellationQuote, Enumerable.Empty<string>(), performingUser, default);
                }

            case "QuoteInitializedEvent":
                return new QuoteAggregate.QuoteInitializedEvent(
                            tenantId,
                            quoteAggregate.Id,
                            quote.Id,
                            default,
                            default,
                            default,
                            default,
                            performingUser,
                            default,
                            Timezones.AET,
                            false,
                            null,
                            false,
                            null);
            case "QuoteActualisedEvent":
                return new QuoteAggregate.QuoteActualisedEvent(
                            tenantId,
                            quoteAggregate.Id,
                            quote.Id,
                            performingUser,
                            default);
            case "FormDataUpdatedEvent":
                return new QuoteAggregate.FormDataUpdatedEvent(
                            tenantId, quoteAggregate.Id, Guid.NewGuid(), string.Empty, performingUser, default);
            case "CustomerAssignedEvent":
                return new QuoteAggregate.CustomerAssignedEvent(
                            tenantId,
                            quoteAggregate.Id,
                            Guid.NewGuid(),
                            Guid.NewGuid(),
                            new FakePersonalDetails(),
                            performingUser,
                            default);
            case "CalculationResultCreatedEvent":
                {
                    CalculationResult calculationResult = CalculationResult.CreateForNewPolicy(
                        this.GetCalculationData(),
                        quoteDataRetriever);
                    return new QuoteAggregate.CalculationResultCreatedEvent(
                        tenantId, quoteAggregate.Id, Guid.NewGuid(), calculationResult, performingUser, default);
                }

            case "FormDataPatchedEvent":
                return new QuoteAggregate.FormDataPatchedEvent(
                            tenantId, quoteAggregate.Id, Guid.NewGuid(), new Microsoft.AspNetCore.JsonPatch.JsonPatchDocument(), performingUser, default);
            case "OwnershipAssignedEvent":
                return new QuoteAggregate.OwnershipAssignedEvent(
                            tenantId, quoteAggregate.Id, Guid.NewGuid(), default, string.Empty, performingUser, default);
            case "CustomerDetailsUpdatedEvent":
                return new QuoteAggregate.CustomerDetailsUpdatedEvent(
                            tenantId, quoteAggregate.Id, Guid.NewGuid(), new FakePersonalDetails(), performingUser, default);
            case "QuoteSubmittedEvent":
                return new QuoteAggregate.QuoteSubmittedEvent(tenantId, quoteAggregate.Id, quote, performingUser, default);
            case "EnquiryMadeEvent":
                return new QuoteAggregate.EnquiryMadeEvent(tenantId, quoteAggregate.Id, quote, performingUser, default);
            case "QuoteSavedEvent":
                return new QuoteAggregate.QuoteSavedEvent(tenantId, quoteAggregate.Id, quote, performingUser, default);
            case "PolicyIssuedEvent":
                {
                    var snapshot = this.CreateQuoteSnapshot(timestamp);
                    return new QuoteAggregate.PolicyIssuedEvent(
                        quoteAggregate,
                        quote.Id,
                        string.Empty,
                        string.Empty,
                        quote.LatestCalculationResult.Data,
                        default,
                        default,
                        default,
                        default,
                        Timezones.AET,
                        snapshot,
                        performingUser,
                        default,
                        Guid.NewGuid());
                }

            case "InvoiceIssuedEvent":
                return new QuoteAggregate.InvoiceIssuedEvent(tenantId, quoteAggregate.Id, quote, "123", performingUser, default);
            case "PaymentMadeEvent":
                {
                    var paymentDetails = new PaymentDetails("test");
                    return new QuoteAggregate.PaymentMadeEvent(tenantId, quoteAggregate.Id, quote, paymentDetails, performingUser, default);
                }

            case "PaymentFailedEvent":
                return new QuoteAggregate.PaymentFailedEvent(tenantId, quoteAggregate.Id, quote, Enumerable.Empty<string>(), performingUser, default);
            case "FundingProposalCreatedEvent":
                return new QuoteAggregate.FundingProposalCreatedEvent(tenantId, quoteAggregate.Id, quote, It.IsAny<FundingProposal>(), performingUser, default);
            case "FundingProposalAcceptedEvent":
                return new QuoteAggregate.FundingProposalAcceptedEvent(tenantId, quoteAggregate.Id, quote.Id, It.IsAny<FundingProposal>(), performingUser, default);
            case "FundingProposalAcceptanceFailedEvent":
                return new QuoteAggregate.FundingProposalAcceptanceFailedEvent(
                            tenantId, quoteAggregate.Id, It.IsAny<FundingProposal>(), Enumerable.Empty<string>(), performingUser, default);
            case "QuoteNumberAssignedEvent":
                return new QuoteAggregate.QuoteNumberAssignedEvent(tenantId, quoteAggregate.Id, quote.Id, string.Empty, performingUser, default);
            case "QuoteVersionCreatedEvent":
                return new QuoteAggregate.QuoteVersionCreatedEvent(tenantId, quoteAggregate.Id, Guid.NewGuid(), quote, performingUser, default);
            case "FileAttachedEvent":
                return new QuoteAggregate.FileAttachedEvent(
                            tenantId,
                            quoteAggregate.Id,
                            quote.Id,
                            Guid.NewGuid(),
                            string.Empty,
                            string.Empty,
                            default,
                            0,
                            performingUser,
                            default);
            case "PolicyCancelledEvent":
                return new QuoteAggregate.PolicyCancelledEvent(
                            tenantId,
                            quoteAggregate.Id,
                            quote.Id,
                            string.Empty,
                            It.IsAny<QuoteDataSnapshot>(),
                            default,
                            default,
                            performingUser,
                            default,
                            Guid.NewGuid());
            case "QuoteDocumentGeneratedEvent":
                return new QuoteAggregate.QuoteDocumentGeneratedEvent(tenantId, quoteAggregate.Id, quote.Id, document, performingUser, default);
            case "QuoteVersionDocumentGeneratedEvent":
                return new QuoteAggregate.QuoteVersionDocumentGeneratedEvent(tenantId, quoteAggregate.Id, quote.Id, default, document, performingUser, default);
            case "PolicyDocumentGeneratedEvent":
                return new QuoteAggregate.PolicyDocumentGeneratedEvent(
                            tenantId, quoteAggregate.Id, policyTransaction!.Id, It.IsAny<QuoteDocument>(), performingUser, default);
            case "PolicyAdjustedEvent":
                return new QuoteAggregate.PolicyAdjustedEvent(
                            tenantId, quoteAggregate.Id, default, string.Empty, default, default, default, default, It.IsAny<QuoteDataSnapshot>(), default, default, Guid.NewGuid());
            case "PolicyDataPatchedEvent":
                return new QuoteAggregate.PolicyDataPatchedEvent(tenantId, quoteAggregate.Id, It.IsAny<PolicyDataPatch>(), performingUser, default);
            case "QuoteDiscardEvent":
                return new QuoteAggregate.QuoteDiscardEvent(tenantId, quoteAggregate.Id, quote.Id, performingUser, default);
            case "WorkflowStepAssignedEvent":
                return new QuoteAggregate.WorkflowStepAssignedEvent(tenantId, quoteAggregate.Id, quote.Id, string.Empty, performingUser, default);
            case "QuoteEmailGeneratedEvent":
                return new QuoteAggregate.QuoteEmailGeneratedEvent(
                            tenantId,
                            quoteAggregate.Id,
                            quote.Id,
                            Guid.NewGuid(),
                            string.Empty,
                            string.Empty,
                            default,
                            default,
                            default,
                            default,
                            performingUser);
            case "PolicyEmailGeneratedEvent":
                return new QuoteAggregate.PolicyEmailGeneratedEvent(
                            tenantId,
                            quoteAggregate.Id,
                            policyTransaction!.Id,
                            Guid.NewGuid(),
                            string.Empty,
                            string.Empty,
                            default,
                            default,
                            default,
                            default,
                            performingUser);
            case "QuoteEmailSentEvent":
                return new QuoteAggregate.QuoteEmailSentEvent(
                            tenantId, quoteAggregate.Id, Guid.NewGuid(), performingUser, default);
            case "RenewalQuoteCreatedEvent":
                return new QuoteAggregate.RenewalQuoteCreatedEvent(
                            tenantId, quoteAggregate.Id, quoteAggregate.OrganisationId, string.Empty, string.Empty, performingUser, default, Guid.NewGuid());
            case "PolicyRenewedEvent":
                return new QuoteAggregate.PolicyRenewedEvent(
                            tenantId, quoteAggregate.Id, quote.Id, "N22", default, default, default, default, It.IsAny<QuoteDataSnapshot>(), performingUser, default, Guid.NewGuid());
            case "QuoteStateChangedEvent":
                return new QuoteAggregate.QuoteStateChangedEvent(tenantId, quoteAggregate.Id, quote.Id, QuoteAction.Actualise, performingUser, "asd", "asd", default);
            case "QuoteBoundEvent":
                return new QuoteAggregate.QuoteBoundEvent(tenantId, quoteAggregate.Id, quote.Id, performingUser, default);
            case "QuoteRollbackEvent":
                return new QuoteAggregate.QuoteRollbackEvent(
                            tenantId, quoteAggregate.Id, quote.Id, 2, performingUser, default, this.SampleCallback, this.SampleCallback);
            case "CreditNoteIssuedEvent":
                return new QuoteAggregate.CreditNoteIssuedEvent(tenantId, quoteAggregate.Id, quote, string.Empty, performingUser, default);
            case "CancellationQuoteCreatedEvent":
                return new QuoteAggregate.CancellationQuoteCreatedEvent(
                            tenantId, quoteAggregate.Id, quoteAggregate.OrganisationId, string.Empty, default, performingUser, default, Guid.NewGuid());
            case "QuoteCustomerAssociationInvitationCreatedEvent":
                return new QuoteAggregate.QuoteCustomerAssociationInvitationCreatedEvent(
                            tenantId, quoteAggregate.Id, Guid.NewGuid(), Guid.NewGuid(), performingUser, default);
            case "QuoteExpiryTimestampSetEvent":
                return new QuoteAggregate.QuoteExpiryTimestampSetEvent(
                            tenantId, quoteAggregate.Id, default, default, default, performingUser, default);
            case "QuoteTransferredToAnotherOrganisationEvent":
                return new QuoteAggregate.QuoteTransferredToAnotherOrganisationEvent(
                            tenantId, quoteAggregate.OrganisationId, quoteAggregate.Id, Guid.NewGuid(), performingUser, default);
            case "PolicyNumberUpdatedEvent":
                return new QuoteAggregate.PolicyNumberUpdatedEvent(
                            tenantId, policyTransaction!.Id, quote.Id, "ABC-001", performingUser, default);
        }

        throw new Exception($"Unrecognized event {eventName} for quote");
    }

    private IEnumerable<IEvent<QuoteAggregate, Guid>> SampleCallback()
    {
        return new List<IEvent<QuoteAggregate, Guid>>();
    }

    private QuoteDataSnapshot CreateQuoteSnapshot(Instant timestamp)
    {
        var formDataJson = FormDataJsonFactory.GetSampleWithStartAndEndDates();
        var calculationResultJson = CalculationResultJsonFactory.Create();
        var quoteData = QuoteFactory.QuoteDataRetriever(
            new CachingJObjectWrapper(formDataJson), new CachingJObjectWrapper(calculationResultJson));
        var calculationResult = Domain.ReadWriteModel.CalculationResult.CreateForNewPolicy(
            new CachingJObjectWrapper(calculationResultJson), quoteData);

        var formDataQuoteDataUpdate = new QuoteDataUpdate<Domain.Aggregates.Quote.FormData>(
            Guid.NewGuid(), new Domain.Aggregates.Quote.FormData(formDataJson), timestamp);
        var calculationQuoteDataUpdate = new QuoteDataUpdate<Domain.ReadWriteModel.CalculationResult>(
            Guid.NewGuid(), calculationResult, timestamp);
        var detailsQuoteDataUpdate = new QuoteDataUpdate<IPersonalDetails>(
            Guid.NewGuid(), new FakePersonalDetails(), timestamp);

        return new QuoteDataSnapshot(formDataQuoteDataUpdate, calculationQuoteDataUpdate, detailsQuoteDataUpdate);
    }

    private CachingJObjectWrapper GetFormData()
    {
        var formModel = $@"{{
                  ""formModel"": {{
                    ""startDate"": ""20/09/21"",
                    ""endDate"": ""20/09/21"",
                    ""effectiveDate"": ""20/09/21"",
                    ""cancellationDate"": ""20/09/21"",
                    ""contactName"": ""Arthur Cruz"",
                    ""contactEmail"": ""arthur.cruz@ubind.io"",
                    ""contactMobile"": ""04 12345678"",
                    ""contactPhone"": ""04 12345678"",
                    ""insuredName"": ""Arthur Cruz2"",
                    ""insuredFullName"": ""Arthur Cruz Jr."",
                    ""contactAddressLine1"": ""1 Foo Street"",
                    ""contactAddressSuburb"": ""Fooville"",
                    ""contactAddressState"": ""VIC"",
                    ""contactAddressPostcode"": ""3000"",
                    ""inceptionDate"": ""20/09/21"",
                    ""expiryDate"": ""20/09/21"",
                    ""abn"": ""12345678901"",
                    ""tradingName"": ""My trading name"",
                    ""numberOfInstallments"": ""12"",
                    ""runoffQuestion"": ""no"",
                    ""businessEndDate"": ""20/09/21""
                  }}
                }}";
        return new CachingJObjectWrapper(formModel);
    }

    private CachingJObjectWrapper GetCalculationData()
    {
        var calculationModel = @"{
                                        ""payment"": {
                                            ""currencyCode"": ""AUD"",
                                            ""total"": {
                                                ""premium"": 110
                                            }
                                        }
                                    }";
        return new CachingJObjectWrapper(calculationModel);
    }

    private class FakeQuoteEventPayload : QuoteOperationEventPayload
    {
        public FakeQuoteEventPayload(Guid quoteId)
        {
            this.Tenant = new Domain.Events.Models.Tenant(Guid.NewGuid(), "test-tenant");
            this.Organisation = new Domain.Events.Models.Organisation
            {
                Id = Guid.NewGuid(),
                Alias = "test-organisation",
            };
            this.Product = new Domain.Events.Models.Product
            {
                Id = Guid.NewGuid(),
                Alias = "test-product",
            };
            this.Customer = new Customer
            {
                Id = Guid.NewGuid(),
                DisplayName = "Test Customer",
            };
            this.Quote = new Domain.Events.Models.Quote
            {
                Id = quoteId,
                QuoteReference = "CNBLUE",
            };
            this.PerformingUser = new User
            {
                Id = Guid.NewGuid(),
                DisplayName = "Test User",
            };
        }
    }
}
