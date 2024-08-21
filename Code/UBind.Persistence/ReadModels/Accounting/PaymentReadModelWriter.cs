// <copyright file="PaymentReadModelWriter.cs" company="uBind">
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
    /// Responsible for creating,updating payment readmodels.
    /// </summary>
    public class PaymentReadModelWriter : FinancialTransactionReadModelWriter<PaymentReadModel, PaymentAllocationReadModel, Invoice>
    {
        private readonly IReadModelRepository<Invoice> invoiceRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="PaymentReadModelWriter"/> class.
        /// </summary>
        /// <param name="paymentRepository">The payment read model repository.</param>
        /// <param name="allocationRepository">The payment allocation repository.</param>
        /// <param name="invoiceRepository">The invoice repository.</param>
        public PaymentReadModelWriter(
            IWritableReadModelRepository<PaymentReadModel> paymentRepository,
            IReadModelRepository<PaymentAllocationReadModel> allocationRepository,
            IReadModelRepository<Invoice> invoiceRepository)
            : base(paymentRepository, allocationRepository)
        {
            this.invoiceRepository = invoiceRepository;
        }

        /// <inheritdoc/>
        public override void Handle(
            FinancialTransactionAggregate<Invoice> aggregate,
            FinancialTransactionAggregate<Invoice>.FinancialTransactionInitializedEvent @event,
            int sequenceNumber)
        {
            var payment = new PaymentReadModel(
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

            this.FinancialTransactionRepository.Add(payment);
        }

        /// <inheritdoc/>
        public override void Handle(
            FinancialTransactionAggregate<Invoice> aggregate,
            FinancialTransactionAggregate<Invoice>.TransactionAllocatedEvent @event,
            int sequenceNumber)
        {
            var errorData = new JObject
            {
                { "eventName", nameof(FinancialTransactionAggregate<Invoice>.TransactionAllocatedEvent) },
                { "eventData", JsonConvert.SerializeObject(@event) },
            };

            var invoice = this.invoiceRepository.GetByIdMaybe(@event.TenantId, @event.CommercialDocumentId);

            if (invoice.HasNoValue)
            {
                throw new ErrorException(Errors.Accounting.AccountTransactionDoesNotExist(@event.CommercialDocumentId, nameof(Invoice), null, errorData));
            }

            var payment = this.GetFinancialTransactionReadModel(@event.TenantId, @event.AggregateId);

            var newAllocation = new PaymentAllocationReadModel(payment, @event.Amount, invoice.Value, @event.Timestamp);
            payment.Allocations.Add(newAllocation);
        }
    }
}
