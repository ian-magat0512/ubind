// <copyright file="QuoteSystemEventEmitter.cs" company="uBind">
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
using UBind.Domain.Aggregates.Quote;
using UBind.Domain.Events;
using UBind.Domain.Events.Payload;
using UBind.Domain.Extensions;
using UBind.Domain.ReadModel;
using UBind.Domain.Services.SystemEvents;

/// <summary>
/// This class listens to events on the QuoteAggregate
/// and raises SystemEvents so that automations can listen
/// and respond to when something changes on a quote.
/// </summary>
public class QuoteSystemEventEmitter : IQuoteSystemEventEmitter
{
    private readonly ISystemEventService systemEventService;
    private readonly IEventPayloadFactory payloadFactory;
    private readonly IClock clock;

    /// <summary>
    /// Initializes a new instance of the <see cref="QuoteSystemEventEmitter"/> class.
    /// </summary>
    /// <param name="systemEventService">The system event servie.</param>
    /// <param name="clock">The clock instance to get current timestamp.</param>
    public QuoteSystemEventEmitter(
        ISystemEventService systemEventService,
        IEventPayloadFactory payloadFactory,
        IClock clock)
    {
        this.systemEventService = systemEventService;
        this.payloadFactory = payloadFactory;
        this.clock = clock;
    }

    public void Dispatch(
        QuoteAggregate aggregate,
        IEvent<QuoteAggregate, Guid> @event,
        int sequenceNumber,
        IEnumerable<Type>? observerTypes = null)
    {
        this.DispatchIfHandlerExists(aggregate, @event, sequenceNumber);
    }

    public async Task Handle(QuoteAggregate aggregate, QuoteAggregate.QuoteInitializedEvent @event, int sequenceNumber)
    {
        var payload = await this.payloadFactory.CreateQuoteEventPayload(aggregate, @event.QuoteId);
        this.CreateAndProcessSystemEventsWithPayload<QuoteOperationEventPayload>(aggregate, @event, payload, @event.QuoteId);
    }

    public void Handle(QuoteAggregate aggregate, QuoteAggregate.QuoteImportedEvent @event, int sequenceNumber)
    {
        this.CreateAndProcessSystemEvents(aggregate, @event, @event.QuoteId);
    }

    public void Handle(QuoteAggregate aggregate, QuoteAggregate.QuoteTransferredToAnotherOrganisationEvent @event, int sequenceNumber)
    {
        this.CreateAndProcessSystemEvents(aggregate, @event, @event.QuoteId);
    }

    public void Handle(QuoteAggregate aggregate, QuoteAggregate.QuoteMigratedEvent @event, int sequenceNumber)
    {
        this.CreateAndProcessSystemEvents(aggregate, @event);
    }

    public void Handle(QuoteAggregate aggregate, QuoteAggregate.CalculationResultCreatedEvent @event, int sequenceNumber)
    {
        this.CreateAndProcessSystemEvents(aggregate, @event, @event.QuoteId);
    }

    public void Handle(QuoteAggregate aggregate, QuoteAggregate.CustomerAssignedEvent @event, int sequenceNumber)
    {
        var quote = aggregate.GetQuoteBySequenceNumber(sequenceNumber);
        this.CreateAndProcessSystemEvents(aggregate, @event, quote?.Id);
    }

    public void Handle(QuoteAggregate aggregate, QuoteAggregate.CustomerDetailsUpdatedEvent @event, int sequenceNumber)
    {
        this.CreateAndProcessSystemEvents(aggregate, @event, @event.QuoteId);
    }

    public void Handle(QuoteAggregate aggregate, QuoteAggregate.EnquiryMadeEvent @event, int sequenceNumber)
    {
        this.CreateAndProcessSystemEvents(aggregate, @event, @event.QuoteId);
    }

    public async Task Handle(QuoteAggregate aggregate, QuoteAggregate.FileAttachedEvent @event, int sequenceNumber)
    {
        var payload = await this.payloadFactory.CreateQuoteEventPayload(aggregate, @event.QuoteId);
        payload.SetFile(@event.Name);
        var systemEvents = this.CreateSystemEventsWithPayload<QuoteOperationEventPayload>(aggregate, @event, payload, @event.QuoteId);

        // TODO: Create attachment relationship, it doesn't exist yet. This will be addressed in the document
        // management Epic: https://confluence.aptiture.com/display/UBIND/Document+Management
        /* systemEvents.ForEach(se => ); */

        this.systemEventService.OnAggregateSavedPersistAndEmitSystemEventsInBackgroundOnce(aggregate, systemEvents);
    }

    public void Handle(QuoteAggregate aggregate, QuoteAggregate.FormDataUpdatedEvent @event, int sequenceNumber)
    {
        this.CreateAndProcessSystemEvents(aggregate, @event, @event.QuoteId);
    }

    public void Handle(QuoteAggregate aggregate, QuoteAggregate.FundingProposalAcceptanceFailedEvent @event, int sequenceNumber)
    {
        var quote = aggregate.GetQuoteBySequenceNumber(sequenceNumber);
        this.CreateAndProcessSystemEvents(aggregate, @event, quote.Id);
    }

    public void Handle(QuoteAggregate aggregate, QuoteAggregate.FundingProposalAcceptedEvent @event, int sequenceNumber)
    {
        this.CreateAndProcessSystemEvents(aggregate, @event, @event.QuoteId);
    }

    public void Handle(QuoteAggregate aggregate, QuoteAggregate.FundingProposalCreatedEvent @event, int sequenceNumber)
    {
        this.CreateAndProcessSystemEvents(aggregate, @event, @event.QuoteId);
    }

    public void Handle(QuoteAggregate aggregate, QuoteAggregate.FundingProposalCreationFailedEvent @event, int sequenceNumber)
    {
        this.CreateAndProcessSystemEvents(aggregate, @event, @event.QuoteId);
    }

    public void Handle(QuoteAggregate aggregate, QuoteAggregate.InvoiceIssuedEvent @event, int sequenceNumber)
    {
        this.CreateAndProcessSystemEvents(aggregate, @event, @event.QuoteId);
    }

    public void Handle(QuoteAggregate aggregate, QuoteAggregate.OwnershipAssignedEvent @event, int sequenceNumber)
    {
        var quote = aggregate.GetQuoteBySequenceNumber(sequenceNumber);

        // Imported policies do not have an associated quote
        if (quote == null)
        {
            return;
        }

        var systemEvents = this.CreateSystemEvents(aggregate, @event, quote.Id);
        systemEvents.ForEach(se => se.AddRelationshipFromEntity(RelationshipType.UserEvent, EntityType.User, @event.UserId));
        this.systemEventService.OnAggregateSavedPersistAndEmitSystemEventsInBackgroundOnce(aggregate, systemEvents);
    }

    public void Handle(QuoteAggregate aggregate, QuoteAggregate.OwnershipUnassignedEvent @event, int sequenceNumber)
    {
        var quote = aggregate.GetQuoteBySequenceNumber(sequenceNumber);
        this.CreateAndProcessSystemEvents(aggregate, @event, quote.Id);
    }

    public async Task Handle(QuoteAggregate aggregate, QuoteAggregate.PaymentFailedEvent @event, int sequenceNumber)
    {
        var payload = await this.payloadFactory.CreateQuoteEventPayload(aggregate, @event.QuoteId);
        this.CreateAndProcessSystemEventsWithPayload(aggregate, @event, payload, @event.QuoteId);
    }

    public void Handle(QuoteAggregate aggregate, QuoteAggregate.PaymentMadeEvent @event, int sequenceNumber)
    {
        this.CreateAndProcessSystemEvents(aggregate, @event, @event.QuoteId);
    }

    public void Handle(QuoteAggregate aggregate, QuoteAggregate.PolicyImportedEvent @event, int sequenceNumber)
    {
        this.CreateAndProcessSystemEvents(aggregate, @event, @event.QuoteId);
    }

    public void Handle(QuoteAggregate aggregate, QuoteAggregate.PolicyIssuedWithoutQuoteEvent @event, int sequenceNumber)
    {
        this.CreateAndProcessSystemEvents(aggregate, @event, @event.QuoteId);
    }

    public void Handle(QuoteAggregate aggregate, QuoteAggregate.PolicyAdjustedEvent @event, int sequenceNumber)
    {
        var systemEvents = this.CreateSystemEvents(aggregate, @event, @event.QuoteId);
        var policyTransaction = aggregate.Policy?.Transactions.LastOrDefault();
        if (policyTransaction != null)
        {
            systemEvents.ForEach(se => se.AddRelationshipFromEntity(
                RelationshipType.PolicyTransactionEvent, EntityType.PolicyTransaction, policyTransaction.Id));
        }
        this.systemEventService.OnAggregateSavedPersistAndEmitSystemEventsInBackgroundOnce(aggregate, systemEvents);
    }

    public void Handle(QuoteAggregate aggregate, QuoteAggregate.PolicyCancelledEvent @event, int sequenceNumber)
    {
        var systemEvents = this.CreateSystemEvents(aggregate, @event, @event.QuoteId);
        var policyTransaction = aggregate.Policy?.Transactions.LastOrDefault();
        if (policyTransaction != null)
        {
            systemEvents.ForEach(se => se.AddRelationshipFromEntity(
                RelationshipType.PolicyTransactionEvent, EntityType.PolicyTransaction, policyTransaction.Id));
        }
        this.systemEventService.OnAggregateSavedPersistAndEmitSystemEventsInBackgroundOnce(aggregate, systemEvents);
    }

    public void Handle(QuoteAggregate aggregate, QuoteAggregate.PolicyIssuedEvent @event, int sequenceNumber)
    {
        var systemEvents = this.CreateSystemEvents(aggregate, @event, @event.QuoteId);
        var policyTransaction = aggregate.Policy?.Transactions.LastOrDefault();
        if (policyTransaction != null)
        {
            systemEvents.ForEach(se => se.AddRelationshipFromEntity(
                RelationshipType.PolicyTransactionEvent, EntityType.PolicyTransaction, policyTransaction.Id));
        }
        this.systemEventService.OnAggregateSavedPersistAndEmitSystemEventsInBackgroundOnce(aggregate, systemEvents);
    }

    public void Handle(QuoteAggregate aggregate, QuoteAggregate.PolicyRenewedEvent @event, int sequenceNumber)
    {
        var systemEvents = this.CreateSystemEvents(aggregate, @event, @event.QuoteId);
        var policyTransaction = aggregate.Policy?.Transactions.LastOrDefault();
        if (policyTransaction != null)
        {
            systemEvents.ForEach(se => se.AddRelationshipFromEntity(
                RelationshipType.PolicyTransactionEvent, EntityType.PolicyTransaction, policyTransaction.Id));
        }
        this.systemEventService.OnAggregateSavedPersistAndEmitSystemEventsInBackgroundOnce(aggregate, systemEvents);
    }

    public void Handle(QuoteAggregate aggregate, QuoteAggregate.PolicyDataPatchedEvent @event, int sequenceNumber)
    {
        this.CreateAndProcessSystemEvents(aggregate, @event);
    }

    public void Handle(QuoteAggregate aggregate, QuoteAggregate.PolicyNumberUpdatedEvent @event, int sequenceNumber)
    {
        this.CreateAndProcessSystemEvents(aggregate, @event);
    }

    public void Handle(QuoteAggregate aggregate, QuoteAggregate.QuoteDiscardEvent @event, int sequenceNumber)
    {
        this.CreateAndProcessSystemEvents(aggregate, @event, @event.QuoteId);
    }

    public void Handle(QuoteAggregate aggregate, QuoteAggregate.PolicyDeletedEvent @event, int sequenceNumber)
    {
        this.CreateAndProcessSystemEvents(aggregate, @event);
    }

    public void Handle(QuoteAggregate aggregate, QuoteAggregate.QuoteNumberAssignedEvent @event, int sequenceNumber)
    {
        this.CreateAndProcessSystemEvents(aggregate, @event, @event.QuoteId);
    }

    public void Handle(QuoteAggregate aggregate, QuoteAggregate.QuoteSavedEvent @event, int sequenceNumber)
    {
        this.CreateAndProcessSystemEvents(aggregate, @event, @event.QuoteId);
    }

    public void Handle(QuoteAggregate aggregate, QuoteAggregate.QuoteSubmittedEvent @event, int sequenceNumber)
    {
        this.CreateAndProcessSystemEvents(aggregate, @event, @event.QuoteId);
    }

    public void Handle(QuoteAggregate aggregate, QuoteAggregate.QuoteVersionCreatedEvent @event, int sequenceNumber)
    {
        var systemEvents = this.CreateSystemEvents(aggregate, @event, @event.QuoteId);
        systemEvents.ForEach(se => se.AddRelationshipFromEntity(
            RelationshipType.QuoteVersionEvent, EntityType.QuoteVersion, @event.VersionId));
        this.systemEventService.OnAggregateSavedPersistAndEmitSystemEventsInBackgroundOnce(aggregate, systemEvents);
    }

    public void Handle(QuoteAggregate aggregate, QuoteAggregate.QuoteDocumentGeneratedEvent @event, int sequenceNumber)
    {
        var systemEvents = this.CreateSystemEvents(aggregate, @event, @event.QuoteId);
        systemEvents.ForEach(se => se.AddRelationshipFromEntity(
            RelationshipType.DocumentEvent, EntityType.Document, @event.Document.Id));
        this.systemEventService.OnAggregateSavedPersistAndEmitSystemEventsInBackgroundOnce(aggregate, systemEvents);
    }

    public void Handle(QuoteAggregate aggregate, QuoteAggregate.QuoteVersionDocumentGeneratedEvent @event, int sequenceNumber)
    {
        var systemEvents = this.CreateSystemEvents(aggregate, @event, @event.QuoteId);
        systemEvents.ForEach(se =>
        {
            se.AddRelationshipFromEntity(
                RelationshipType.DocumentEvent, EntityType.Document, @event.Document.Id);
            se.AddRelationshipFromEntity(
                RelationshipType.QuoteVersionEvent, EntityType.QuoteVersion, @event.VersionId);
        });
        this.systemEventService.OnAggregateSavedPersistAndEmitSystemEventsInBackgroundOnce(aggregate, systemEvents);
    }

    public void Handle(QuoteAggregate aggregate, QuoteAggregate.QuoteRollbackEvent @event, int sequenceNumber)
    {
        this.CreateAndProcessSystemEvents(aggregate, @event, @event.QuoteId);
    }

    public void Handle(QuoteAggregate aggregate, QuoteAggregate.PolicyDocumentGeneratedEvent @event, int sequenceNumber)
    {
        var systemEvents = this.CreateSystemEvents(aggregate, @event, aggregate.Policy?.QuoteId);
        var policyTransaction = aggregate.Policy?.Transactions.LastOrDefault();
        if (policyTransaction != null)
        {
            systemEvents.ForEach(se => se.AddRelationshipFromEntity(
                RelationshipType.PolicyTransactionEvent, EntityType.PolicyTransaction, policyTransaction.Id));
        }
        this.systemEventService.OnAggregateSavedPersistAndEmitSystemEventsInBackgroundOnce(aggregate, systemEvents);
    }

    public void Handle(QuoteAggregate aggregate, QuoteAggregate.RenewalQuoteCreatedEvent @event, int sequenceNumber)
    {
        this.CreateAndProcessSystemEvents(aggregate, @event, @event.QuoteId);
    }

    public void Handle(QuoteAggregate aggregate, QuoteAggregate.AdjustmentQuoteCreatedEvent @event, int sequenceNumber)
    {
        this.CreateAndProcessSystemEvents(aggregate, @event, @event.QuoteId);
    }

    public void Handle(QuoteAggregate aggregate, QuoteAggregate.CancellationQuoteCreatedEvent @event, int sequenceNumber)
    {
        this.CreateAndProcessSystemEvents(aggregate, @event, @event.QuoteId);
    }

    public void Handle(QuoteAggregate aggregate, QuoteAggregate.WorkflowStepAssignedEvent @event, int sequenceNumber)
    {
        this.CreateAndProcessSystemEvents(aggregate, @event, @event.QuoteId);
    }

    public void Handle(QuoteAggregate aggregate, QuoteAggregate.QuoteEmailGeneratedEvent @event, int sequenceNumber)
    {
        var systemEvents = this.CreateSystemEvents(aggregate, @event, aggregate.Policy?.QuoteId);
        systemEvents.ForEach(se => se.AddRelationshipFromEntity(
            RelationshipType.EmailEvent, EntityType.Message, @event.EmailId));
        this.systemEventService.OnAggregateSavedPersistAndEmitSystemEventsInBackgroundOnce(aggregate, systemEvents);
    }

    public void Handle(QuoteAggregate aggregate, QuoteAggregate.PolicyEmailGeneratedEvent @event, int sequenceNumber)
    {
        var systemEvents = this.CreateSystemEvents(aggregate, @event, aggregate.Policy?.QuoteId);
        var policyTransaction = aggregate.Policy?.Transactions.LastOrDefault();
        systemEvents.ForEach(se =>
        {
            if (policyTransaction != null)
            {
                se.AddRelationshipFromEntity(
                RelationshipType.PolicyTransactionEvent, EntityType.PolicyTransaction, policyTransaction.Id);
            }
            se.AddRelationshipFromEntity(
                RelationshipType.EmailEvent, EntityType.Message, @event.EmailId);
        });
        this.systemEventService.OnAggregateSavedPersistAndEmitSystemEventsInBackgroundOnce(aggregate, systemEvents);
    }

    public void Handle(QuoteAggregate aggregate, QuoteAggregate.QuoteEmailSentEvent @event, int sequenceNumber)
    {
        var systemEvents = this.CreateSystemEvents(aggregate, @event, aggregate.Policy?.QuoteId);
        systemEvents.ForEach(se => se.AddRelationshipFromEntity(
            RelationshipType.EmailEvent, EntityType.Message, @event.QuoteEmailReadModelId));
        this.systemEventService.OnAggregateSavedPersistAndEmitSystemEventsInBackgroundOnce(aggregate, systemEvents);
    }

    public void Handle(QuoteAggregate aggregate, QuoteAggregate.QuoteStateChangedEvent @event, int sequenceNumber)
    {
        var eventPayload = new Dictionary<string, string>
        {
            { "originalState", @event.OriginalState.ToCamelCase() },
            { "resultingState", @event.ResultingState.ToCamelCase() },
        };
        this.CreateAndProcessSystemEventsWithPayload(aggregate, @event, eventPayload, @event.QuoteId);
    }

    public void Handle(QuoteAggregate aggregate, QuoteAggregate.QuoteBoundEvent @event, int sequenceNumber)
    {
        this.CreateAndProcessSystemEvents(aggregate, @event, @event.QuoteId);
    }

    public void Handle(QuoteAggregate aggregate, QuoteAggregate.CreditNoteIssuedEvent @event, int sequenceNumber)
    {
        this.CreateAndProcessSystemEvents(aggregate, @event, @event.QuoteId);
    }

    public void Handle(QuoteAggregate aggregate, QuoteAggregate.QuoteCustomerAssociationInvitationCreatedEvent @event, int sequenceNumber)
    {
        var systemEvents = this.CreateSystemEvents(aggregate, @event, @event.QuoteId);
        systemEvents.ForEach(se => se.AddRelationshipFromEntity(RelationshipType.UserEvent, EntityType.User, @event.CustomerUserId));
        this.systemEventService.OnAggregateSavedPersistAndEmitSystemEventsInBackgroundOnce(aggregate, systemEvents);
    }

    public void Handle(QuoteAggregate aggregate, QuoteAggregate.QuoteExpiryTimestampSetEvent @event, int sequenceNumber)
    {
        this.CreateAndProcessSystemEvents(aggregate, @event, @event.QuoteId);
    }

    public void Handle(QuoteAggregate aggregate, QuoteAggregate.QuoteActualisedEvent @event, int sequenceNumber)
    {
        this.CreateAndProcessSystemEvents(aggregate, @event, @event.QuoteId);
    }

    public void Handle(QuoteAggregate aggregate, QuoteAggregate.FormDataPatchedEvent @event, int sequenceNumber)
    {
        this.CreateAndProcessSystemEvents(aggregate, @event, @event.QuoteId);
    }

    public void Handle(QuoteAggregate aggregate, QuoteAggregate.ApplyNewIdEvent @event, int sequenceNumber)
    {
        this.CreateAndProcessSystemEvents(aggregate, @event, @event.QuoteId);
    }

    /// <inheritdoc/>
    public async Task CreateAndEmitSystemEvent(NewQuoteReadModel quote, SystemEventType eventType, Guid? performingUserId, Instant? timestamp = null)
    {
        if (timestamp == null)
        {
            timestamp = this.clock.GetCurrentInstant();
        }

        var payload = await this.payloadFactory.CreateQuoteEventPayload(quote);
        var systemEvent = SystemEvent.CreateWithPayload(
                quote.TenantId,
                quote.OrganisationId,
                quote.ProductId,
                quote.Environment,
                eventType,
                payload,
                timestamp.Value);
        systemEvent.AddRelationshipFromEntity(
            RelationshipType.QuoteEvent, EntityType.Quote, quote.Id);
        this.AddStandardQuoteRelationships(
            systemEvent,
            quote.OrganisationId,
            quote.ProductId,
            quote.CustomerId,
            quote.PolicyId,
            performingUserId);
        this.systemEventService.BackgroundPersistAndEmit(new List<SystemEvent> { systemEvent });
    }

    private List<SystemEvent> CreateSystemEvents(
        QuoteAggregate aggregate, IEvent<QuoteAggregate, Guid> aggregateEvent, Guid? quoteId = null)
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
            if (quoteId.HasValue)
            {
                se.AddRelationshipFromEntity(
                    RelationshipType.QuoteEvent, EntityType.Quote, quoteId.Value);
            }
            this.AddStandardQuoteRelationships(se, aggregate, aggregateEvent);
        });

        return systemEvents;
    }

    private List<SystemEvent> CreateSystemEventsWithPayload<TPayload>(
        QuoteAggregate aggregate, IEvent<QuoteAggregate, Guid> aggregateEvent, TPayload payload, Guid? quoteId = null)
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
            if (quoteId.HasValue)
            {
                se.AddRelationshipFromEntity(
                    RelationshipType.QuoteEvent, EntityType.Quote, quoteId);
            }
            this.AddStandardQuoteRelationships(se, aggregate, aggregateEvent);
        });

        return systemEvents;
    }

    private void CreateAndProcessSystemEvents(
        QuoteAggregate aggregate, IEvent<QuoteAggregate, Guid> aggregateEvent, Guid? quoteId = null)
    {
        var systemEvents = this.CreateSystemEvents(aggregate, aggregateEvent, quoteId);

        // once the aggregate has been saved to the database, trigger the system events and automations.
        this.systemEventService.OnAggregateSavedPersistAndEmitSystemEventsInBackgroundOnce(aggregate, systemEvents);
    }

    private void CreateAndProcessSystemEventsWithPayload<TPayload>(
        QuoteAggregate aggregate, IEvent<QuoteAggregate, Guid> aggregateEvent, TPayload payload, Guid? quoteId = null)
    {
        using (MiniProfiler.Current.Step("CreateAndProcessSystemEventsWithPayload " + aggregateEvent.GetType().Name))
        {
            var systemEvents = this.CreateSystemEventsWithPayload(aggregate, aggregateEvent, payload, quoteId);
            this.systemEventService.OnAggregateSavedPersistAndEmitSystemEventsInBackgroundOnce(aggregate, systemEvents);
        }
    }

    private void AddStandardQuoteRelationships(
        SystemEvent systemEvent, QuoteAggregate aggregate, IEvent<QuoteAggregate, Guid> aggregateEvent)
    {
        this.AddStandardQuoteRelationships(
            systemEvent,
            aggregate.OrganisationId,
            aggregate.ProductId,
            aggregate.CustomerId,
            aggregate.Policy?.PolicyId,
            aggregateEvent.PerformingUserId);
    }

    private void AddStandardQuoteRelationships(
        SystemEvent systemEvent,
        Guid organisationId,
        Guid productId,
        Guid? customerId,
        Guid? policyId,
        Guid? performingUserId = null)
    {
        systemEvent.AddRelationshipFromEntity(
            RelationshipType.OrganisationEvent, EntityType.Organisation, organisationId);
        systemEvent.AddRelationshipFromEntity(
            RelationshipType.ProductEvent, EntityType.Product, productId);

        if (policyId.GetValueOrDefault() != default && policyId.HasValue)
        {
            systemEvent.AddRelationshipFromEntity(
                RelationshipType.PolicyEvent, EntityType.Policy, policyId.Value);
        }

        if (performingUserId.HasValue)
        {
            systemEvent.AddRelationshipToEntity(
                RelationshipType.EventPerformingUser, EntityType.User, performingUserId.Value);
        }

        if (customerId.GetValueOrDefault() != default && customerId.HasValue)
        {
            systemEvent.AddRelationshipFromEntity(
                RelationshipType.CustomerEvent, EntityType.Customer, customerId.Value);
        }
    }
}
