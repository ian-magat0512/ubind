// <copyright file="QuoteDocumentReadModelWriter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Persistence.ReadModels.Quote
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using UBind.Domain.Aggregates;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Policy;
    using UBind.Domain.Repositories;
    using static UBind.Domain.Aggregates.Quote.QuoteAggregate;

    /// <summary>
    /// For updating the quote read model in response to events from the write model.
    /// </summary>
    public class QuoteDocumentReadModelWriter : IQuoteDocumentReadModelWriter
    {
        private readonly IWritableReadModelRepository<QuoteDocumentReadModel> readModelRepository;
        private readonly IWritableReadModelRepository<PolicyReadModel> policyReadModelRepository;
        private readonly IWritableReadModelRepository<NewQuoteReadModel> quoteReadModelRepository;

        public QuoteDocumentReadModelWriter(
            IWritableReadModelRepository<QuoteDocumentReadModel> readModelRepository,
            IWritableReadModelRepository<PolicyReadModel> policyReadModelRepository,
            IWritableReadModelRepository<NewQuoteReadModel> quoteReadModelRepository)
        {
            this.readModelRepository = readModelRepository;
            this.policyReadModelRepository = policyReadModelRepository;
            this.quoteReadModelRepository = quoteReadModelRepository;
        }

        public void Dispatch(
            QuoteAggregate aggregate,
            IEvent<QuoteAggregate, Guid> @event,
            int sequenceNumber,
            IEnumerable<Type>? observerTypes = null)
        {
            this.DispatchIfHandlerExists(aggregate, @event, sequenceNumber);
        }

        public void Handle(QuoteAggregate aggregate, QuoteAggregate.QuoteRollbackEvent @event, int sequenceNumber)
        {
            // Rollback should delete documents created during QuoteDocumentGeneratedEvent and PolicyDocumentGenerated events.
            var strippedEvents = @event.StrippedEvents;
            foreach (var strippedEvent in strippedEvents)
            {
                if (strippedEvent is QuoteDocumentGeneratedEvent quoteDocumentGeneratedEvent)
                {
                    var documentTargetId = quoteDocumentGeneratedEvent.QuoteId != quoteDocumentGeneratedEvent.AggregateId
                        ? quoteDocumentGeneratedEvent.QuoteId : quoteDocumentGeneratedEvent.AggregateId;
                    this.readModelRepository.Delete(
                        quoteDocumentGeneratedEvent.TenantId,
                        d => d.QuoteOrPolicyTransactionId == documentTargetId
                            && d.OwnerType == DocumentOwnerType.Quote
                            && d.Name == quoteDocumentGeneratedEvent.Document.Name
                            && d.FileContentId == quoteDocumentGeneratedEvent.Document.FileContentId);
                }
                else if (strippedEvent is PolicyDocumentGeneratedEvent policyDocumentGenerated)
                {
                    this.readModelRepository.Delete(
                        policyDocumentGenerated.TenantId,
                        d => d.QuoteOrPolicyTransactionId == policyDocumentGenerated.PolicyTransactionId
                            && d.OwnerType == DocumentOwnerType.Policy
                            && d.Name == policyDocumentGenerated.Document.Name
                            && d.FileContentId == policyDocumentGenerated.Document.FileContentId);
                }
            }
        }

        public void Handle(QuoteAggregate aggregate, QuoteAggregate.QuoteDocumentGeneratedEvent @event, int sequenceNumber)
        {
            var eventQuoteOrAggregateId = @event.QuoteId != @event.AggregateId ? @event.QuoteId : @event.AggregateId;
            if (aggregate.IsBeingReplayed)
            {
                this.readModelRepository.Delete(
                        @event.TenantId,
                        d => d.QuoteOrPolicyTransactionId == eventQuoteOrAggregateId
                            && d.OwnerType == DocumentOwnerType.Quote
                            && d.Name == @event.Document.Name
                            && d.FileContentId == @event.Document.FileContentId);
            }

            var quote = this.GetQuoteById(@event.TenantId, @event.QuoteId);
            Expression<Func<QuoteDocumentReadModel, bool>> isExists = (d) =>
                d.TenantId == @event.TenantId &&
                d.QuoteOrPolicyTransactionId == eventQuoteOrAggregateId &&
                d.OwnerType == DocumentOwnerType.Quote &&
                d.Name == @event.Document.Name;
            var readModel = QuoteDocumentReadModel.CreateQuoteDocumentReadModel(@event.AggregateId, @event.QuoteId, @event.Document);
            readModel.Environment = quote.Environment;
            readModel.TenantId = quote.TenantId;
            readModel.OrganisationId = quote.OrganisationId;
            readModel.IsTestData = quote.IsTestData;
            readModel.CustomerId = quote.CustomerId;
            this.readModelRepository.AddOrUpdate(quote.TenantId, readModel, isExists);
        }

        public void Handle(QuoteAggregate aggregate, QuoteAggregate.QuoteVersionDocumentGeneratedEvent @event, int sequenceNumber)
        {
            if (aggregate.IsBeingReplayed)
            {
                return;
            }

            var quote = this.GetQuoteById(@event.TenantId, @event.QuoteId);
            Expression<Func<QuoteDocumentReadModel, bool>> isExists = (d) =>
                d.TenantId == @event.TenantId &&
                d.QuoteOrPolicyTransactionId == @event.VersionId &&
                d.OwnerType == DocumentOwnerType.QuoteVersion &&
                d.Name == @event.Document.Name;

            var readModel = QuoteDocumentReadModel.CreateQuoteVersionDocumentReadModel(@event.AggregateId, @event.VersionId, @event.Document);
            readModel.Environment = quote.Environment;
            readModel.TenantId = quote.TenantId;
            readModel.OrganisationId = quote.OrganisationId;
            readModel.IsTestData = quote.IsTestData;
            readModel.CustomerId = quote.CustomerId;
            this.readModelRepository.AddOrUpdate(quote.TenantId, readModel, isExists);
        }

        public void Handle(QuoteAggregate aggregate, QuoteAggregate.PolicyDocumentGeneratedEvent @event, int sequenceNumber)
        {
            if (aggregate.IsBeingReplayed)
            {
                this.readModelRepository.Delete(
                        @event.TenantId,
                        d => d.QuoteOrPolicyTransactionId == @event.PolicyTransactionId
                            && d.OwnerType == DocumentOwnerType.Policy
                            && d.Name == @event.Document.Name
                            && d.FileContentId == @event.Document.FileContentId);
            }

            var policy = this.GetPolicyReadModel(@event.TenantId, @event.AggregateId);
            Expression<Func<QuoteDocumentReadModel, bool>> isExists = (d) =>
                d.TenantId == @event.TenantId &&
                d.QuoteOrPolicyTransactionId == @event.PolicyTransactionId &&
                d.OwnerType == DocumentOwnerType.Policy &&
                d.Name == @event.Document.Name;

            var readModel = QuoteDocumentReadModel.CreatePolicyDocumentReadModel(
            @event.AggregateId, @event.PolicyTransactionId, @event.Document);
            readModel.Environment = policy.Environment;
            readModel.TenantId = policy.TenantId;
            readModel.OrganisationId = policy.OrganisationId;
            readModel.IsTestData = policy.IsTestData;
            readModel.CustomerId = policy.CustomerId;
            this.readModelRepository.AddOrUpdate(policy.TenantId, readModel, isExists);
        }

        public void Handle(QuoteAggregate aggregate, QuoteAggregate.ApplyNewIdEvent @event, int sequenceNumber)
        {
            var documents = this.readModelRepository
                .Where(@event.TenantId, x => x.PolicyId == @event.AggregateId);

            foreach (var document in documents)
            {
                document.TenantId = @event.TenantId;
            }
        }

        public void Handle(QuoteAggregate aggregate, QuoteAggregate.QuoteTransferredToAnotherOrganisationEvent @event, int sequenceNumber)
        {
            var documents = this.readModelRepository
                .Where(@event.TenantId, x => x.PolicyId == @event.AggregateId);

            foreach (var document in documents)
            {
                document.OrganisationId = @event.OrganisationId;
            }
        }

        public void Handle(QuoteAggregate.CustomerAssignedEvent @event, int sequenceNumber)
        {
            var documents = this.readModelRepository
                .Where(@event.TenantId, x => x.PolicyId == @event.AggregateId && x.CustomerId != @event.CustomerId);

            foreach (var document in documents)
            {
                document.CustomerId = @event.CustomerId;
            }
        }

        private PolicyReadModel GetPolicyReadModel(Guid tenantId, Guid policyId)
        {
            return this.policyReadModelRepository.GetById(tenantId, policyId);
        }

        private NewQuoteReadModel GetQuoteById(Guid tenantId, Guid quoteId)
        {
            return this.quoteReadModelRepository.GetById(tenantId, quoteId);
        }
    }
}
