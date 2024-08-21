// <copyright file="RefundReadModelWriter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using UBind.Domain;
    using UBind.Domain.Accounting;
    using UBind.Domain.Aggregates.Accounting;
    using UBind.Domain.Exceptions;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Accounting;
    using UBind.Domain.Repositories;

    /// <summary>
    /// Responsible for creating, updating refund readmodel.
    /// </summary>
    public class RefundReadModelWriter : FinancialTransactionReadModelWriter<RefundReadModel, RefundAllocationReadModel, CreditNote>
    {
        private readonly IReadModelRepository<CreditNote> creditNoteRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="RefundReadModelWriter"/> class.
        /// </summary>
        /// <param name="refundRepository">The refund read model repository.</param>
        /// <param name="allocationRepository">The refund allocation repository.</param>
        /// <param name="creditNoteRepository">The credit note repository.</param>
        public RefundReadModelWriter(
            IWritableReadModelRepository<RefundReadModel> refundRepository,
            IReadModelRepository<RefundAllocationReadModel> allocationRepository,
            IReadModelRepository<CreditNote> creditNoteRepository)
            : base(refundRepository, allocationRepository)
        {
            this.creditNoteRepository = creditNoteRepository;
        }

        /// <inheritdoc/>
        public override void Handle(
            FinancialTransactionAggregate<CreditNote> aggregate,
            FinancialTransactionAggregate<CreditNote>.FinancialTransactionInitializedEvent @event,
            int sequenceNumber)
        {
            var refund = new RefundReadModel(
              @event.TenantId,
              @event.AggregateId,
              @event.Amount,
              @event.ReferenceNumber,
              @event.PayerId,
              @event.PayerType,
              @event.PayeeId,
              @event.PayeeType,
              @event.Timestamp,
              @event.TransactionTime);

            this.FinancialTransactionRepository.Add(refund);
        }

        /// <inheritdoc/>
        public override void Handle(
            FinancialTransactionAggregate<CreditNote> aggregate,
            FinancialTransactionAggregate<CreditNote>.TransactionAllocatedEvent @event,
            int sequenceNumber)
        {
            var errorData = new JObject
            {
                { "eventName", nameof(FinancialTransactionAggregate<CreditNote>.TransactionAllocatedEvent) },
                { "eventData", JsonConvert.SerializeObject(@event) },
            };

            var creditNote = this.creditNoteRepository.GetByIdMaybe(@event.TenantId, @event.CommercialDocumentId);

            if (creditNote.HasNoValue)
            {
                throw new ErrorException(Errors.Accounting.AccountTransactionDoesNotExist(@event.CommercialDocumentId, nameof(CreditNote), null, errorData));
            }

            var refund = this.GetFinancialTransactionReadModel(@event.TenantId, @event.AggregateId);
            var newAllocation = new RefundAllocationReadModel(refund, @event.Amount, creditNote.Value, @event.Timestamp);
            refund.Allocations.Add(newAllocation);
        }
    }
}
