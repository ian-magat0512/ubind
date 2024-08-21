// <copyright file="FinancialTransactionAggregate.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

// Suppress IDE0060 because there are Apply Event method in which parameter are not in used.
// And we cannot remove the apply method otherwise it will throw an exception.
#pragma warning disable IDE0060 // Remove unused parameter

namespace UBind.Domain.Aggregates.Accounting
{
    using System;
    using System.Collections.Generic;
    using NodaMoney;
    using NodaTime;
    using UBind.Domain.Accounting;
    using UBind.Domain.Aggregates;
    using UBind.Domain.ValueTypes;

    /// <summary>
    /// Financial Transaction Aggregate, Base class for accounting purposes.
    /// </summary>
    /// <typeparam name="TCommercialDocument">The commercial document type.</typeparam>
    public abstract partial class FinancialTransactionAggregate<TCommercialDocument> : AggregateRootEntity<FinancialTransactionAggregate<TCommercialDocument>, Guid>
           where TCommercialDocument : class, ICommercialDocument<Guid>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FinancialTransactionAggregate{TCommercialDocument}"/> class.
        /// </summary>
        protected FinancialTransactionAggregate()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FinancialTransactionAggregate{TCommercialDocument}"/> class,
        /// calculating state by re-applying existing events.
        /// </summary>
        /// <param name="events">Existing events.</param>
        protected FinancialTransactionAggregate(IEnumerable<IEvent<FinancialTransactionAggregate<TCommercialDocument>, Guid>> events)
            : base(events)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FinancialTransactionAggregate{TCommercialDocument}"/> class.
        /// </summary>
        /// <param name="amount">The transaction amount.</param>
        /// <param name="referenceNumber">The reference number.</param>
        /// <param name="transactionTime">The time of transaction.</param>
        /// <param name="createdTimestamp">The created time (for auditing purposes).</param>
        /// <param name="transactionPartyModel">The parties involved in the transaction.</param>
        /// <param name="performingUserId">The user that performs this action.</param>
        protected FinancialTransactionAggregate(
            Guid tenantId,
            Money amount,
            string referenceNumber,
            Instant transactionTime,
            Instant createdTimestamp,
            TransactionParties transactionPartyModel,
            Guid? performingUserId)
        {
            var newEvent = new FinancialTransactionInitializedEvent(tenantId, Guid.NewGuid(), amount, referenceNumber, transactionTime, createdTimestamp, transactionPartyModel, performingUserId);
            this.ApplyNewEvent(newEvent);
        }

        public override AggregateType AggregateType => AggregateType.FinancialTransaction;

        /// <summary>
        /// Gets the tenantId associated with this aggregate.
        /// </summary>
        public Guid TenantId { get; private set; }

        /// <summary>
        /// Gets the instant in time the financial transaction occurred.
        /// </summary>
        public Instant TransactionTime { get; private set; }

        /// <summary>
        /// Gets the amount of the payment.
        /// </summary>
        public Money Amount { get; private set; }

        /// <summary>
        /// Gets the allocations(the commercial document Ids list) for this financial transaction.
        /// </summary>
        public List<Guid> Allocations { get; private set; } = new List<Guid>();

        /// <summary>
        /// Gets the payeeId.
        /// </summary>
        public Guid? PayeeId { get; private set; }

        /// <summary>
        /// Gets the payee type.
        /// </summary>
        public TransactionPartyType? PayeeType { get; private set; }

        /// <summary>
        /// Gets the payer id.
        /// </summary>
        public Guid PayerId { get; private set; }

        /// <summary>
        /// Gets the payer type.
        /// </summary>
        public TransactionPartyType PayerType { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the financial transaction(payment or refund) is deleted.
        /// </summary>
        public bool IsDeleted { get; private set; }

        /// <summary>
        /// Assign Payer and Payee to Payment.
        /// </summary>
        /// <param name="transactionPartyModel">The parties involved in the transaction.</param>
        /// <param name="timestamp">The timestamp.</param>
        /// <param name="performingUserId">The user that performs this action.</param>
        public void AssignParticipants(TransactionParties transactionPartyModel, Instant timestamp, Guid? performingUserId)
        {
            var transactionEvent = new ParticipantsAssignedEvent(this.TenantId, this.Id, timestamp, transactionPartyModel, performingUserId);
            this.ApplyNewEvent(transactionEvent);
        }

        /// <summary>
        /// Delete allocations of financial transaction(payment/refund) to commercial document(invoice, creditNote).
        /// </summary>
        /// <param name="timestamp">The time when the allocations are deleted.</param>
        /// <param name="performingUserId">The user that performs this action.</param>
        public void DeleteAllocations(Instant timestamp, Guid? performingUserId)
        {
            var transactionEvent = new AllocationsDeletedEvent(this.TenantId, this.Id, timestamp, this.Allocations, performingUserId);
            this.ApplyNewEvent(transactionEvent);
        }

        /// <summary>
        /// Delete a financial transaction(payment or refund).
        /// </summary>
        /// <param name="timestamp">The time when the financial transaction is deleted.</param>
        /// <param name="performingUserId">The user that performs this action.</param>
        public void Delete(Instant timestamp, Guid? performingUserId)
        {
            var transactionEvent = new TransactionDeletedEvent(this.TenantId, this.Id, timestamp, performingUserId);
            this.ApplyNewEvent(transactionEvent);
        }

        /// <summary>
        /// Allocate payment/refund to invoice/crediutNote.
        /// </summary>
        /// <param name="transactionPartyModel">The parties involved in the transaction.</param>
        /// <param name="commercialDocument">The invoices.</param>
        /// <param name="timestamp">The timestamp.</param>
        /// <param name="performingUserId">The user that performs this action.</param>
        public abstract void AllocateToCommercialDocument(
            TransactionParties transactionPartyModel,
            TCommercialDocument commercialDocument,
            Instant timestamp,
            Guid? performingUserId);

        public override FinancialTransactionAggregate<TCommercialDocument> ApplyEventsAfterSnapshot(
            IEnumerable<IEvent<FinancialTransactionAggregate<TCommercialDocument>, Guid>> events,
            int latestSnapshotVersion)
        {
            this.ApplyEvents(events, latestSnapshotVersion);
            return this;
        }

        protected override void ApplyDerivedEvent(dynamic @event, int sequenceNumber)
        {
            this.Apply(@event, sequenceNumber);
        }

        private void Apply(FinancialTransactionInitializedEvent @event, int sequenceNumber)
        {
            this.TenantId = @event.TenantId;
            this.Id = @event.AggregateId;
            this.Amount = @event.Amount;
            this.TransactionTime = @event.TransactionTime;
            this.CreatedTimestamp = @event.Timestamp;
            this.IsDeleted = false;
            this.Id = @event.AggregateId;
            this.PayerId = @event.PayerId;
            this.PayerType = @event.PayerType;
            this.PayeeId = @event.PayeeId;
            this.PayeeType = @event.PayeeType;
        }

        private void Apply(ParticipantsAssignedEvent @event, int sequenceNumber)
        {
            this.Id = @event.AggregateId;
            this.PayerId = @event.PayerId;
            this.PayerType = @event.PayerType;
            this.PayeeId = @event.PayeeId;
            this.PayeeType = @event.PayeeType;
        }

        private void Apply(TransactionAllocatedEvent @event, int sequenceNumber)
        {
            this.Id = @event.AggregateId;
            this.Allocations.Add(@event.CommercialDocumentId);
        }

        private void Apply(TransactionDeletedEvent @event, int sequenceNumber)
        {
            this.Id = @event.AggregateId;
            this.IsDeleted = @event.IsDeleted;
        }

        private void Apply(AllocationsDeletedEvent @event, int sequenceNumber)
        {
            this.Allocations.RemoveAll(x => @event.AllocationsToBeDeleted.Contains(x));
        }
    }
}
