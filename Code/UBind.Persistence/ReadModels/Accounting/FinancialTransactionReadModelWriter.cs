// <copyright file="FinancialTransactionReadModelWriter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UBind.Domain;
    using UBind.Domain.Accounting;
    using UBind.Domain.Aggregates;
    using UBind.Domain.Aggregates.Accounting;
    using UBind.Domain.Exceptions;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Accounting;
    using UBind.Domain.Repositories;

    /// <summary>
    /// Responsible for updating financial transaction readmodel.
    /// </summary>
    /// <typeparam name="TReadModel">The readmodel type.</typeparam>
    /// <typeparam name="TAllocation">The allocation type.</typeparam>
    /// <typeparam name="TCommercialDocument">The commercial document type(invoice or credit note).</typeparam>
    public abstract class FinancialTransactionReadModelWriter<TReadModel, TAllocation, TCommercialDocument> : IFinancialTransactionReadModelWriter<TCommercialDocument>
         where TReadModel : class, IFinancialTransactionReadModel<TAllocation>
         where TAllocation : class, IFinancialTransactionAllocationReadModel<TReadModel, TCommercialDocument>
         where TCommercialDocument : class, ICommercialDocument<Guid>
    {
        private readonly IWritableReadModelRepository<TReadModel> financialTransactionRepository;
        private readonly IReadModelRepository<TAllocation> allocationRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="FinancialTransactionReadModelWriter{TReadModel, TAllocation, TCommercialDocument}"/> class.
        /// </summary>
        /// <param name="financialTransactionRepository">The read model repository.</param>
        /// <param name="allocationRepository">The allocation repository.</param>
        public FinancialTransactionReadModelWriter(
            IWritableReadModelRepository<TReadModel> financialTransactionRepository,
            IReadModelRepository<TAllocation> allocationRepository)
        {
            this.financialTransactionRepository = financialTransactionRepository;
            this.allocationRepository = allocationRepository;
        }

        /// <summary>
        /// Gets the financial transaction repository.
        /// </summary>
        protected IWritableReadModelRepository<TReadModel> FinancialTransactionRepository => this.financialTransactionRepository;

        public void Dispatch(
            FinancialTransactionAggregate<TCommercialDocument> aggregate,
            IEvent<FinancialTransactionAggregate<TCommercialDocument>, Guid> @event,
            int sequenceNumber,
            IEnumerable<Type> observerTypes = null)
        {
            this.DispatchIfHandlerExists(aggregate, @event, sequenceNumber);
        }

        public abstract void Handle(
            FinancialTransactionAggregate<TCommercialDocument> aggregate,
            FinancialTransactionAggregate<TCommercialDocument>.FinancialTransactionInitializedEvent @event,
            int sequenceNumber);

        public void Handle(
            FinancialTransactionAggregate<TCommercialDocument> aggregate,
            FinancialTransactionAggregate<TCommercialDocument>.ParticipantsAssignedEvent @event,
            int sequenceNumber)
        {
            var payment = this.GetFinancialTransactionReadModel(@event.TenantId, @event.AggregateId);
            payment.PayerId = @event.PayerId;
            payment.PayerType = @event.PayerType;
            payment.PayeeId = @event.PayeeId;
            payment.PayeeType = @event.PayeeType.Value;
            payment.LastModifiedTimestamp = @event.Timestamp;
        }

        public abstract void Handle(
            FinancialTransactionAggregate<TCommercialDocument> aggregate,
            FinancialTransactionAggregate<TCommercialDocument>.TransactionAllocatedEvent @event,
            int sequenceNumber);

        public void Handle(
            FinancialTransactionAggregate<TCommercialDocument> aggregate,
            FinancialTransactionAggregate<TCommercialDocument>.TransactionDeletedEvent @event,
            int sequenceNumber)
        {
            var payment = this.GetFinancialTransactionReadModel(@event.TenantId, @event.AggregateId);
            payment.IsDeleted = @event.IsDeleted;

            // All allocations should be deleted
            var allocationsToBeDeleted = this.allocationRepository
                 .Where(@event.TenantId, x => x.FinancialTransaction.Id == @event.AggregateId);

            foreach (var allocation in allocationsToBeDeleted)
            {
                allocation.IsDeleted = true;
            }
        }

        public void Handle(
            FinancialTransactionAggregate<TCommercialDocument> aggregate,
            FinancialTransactionAggregate<TCommercialDocument>.AllocationsDeletedEvent @event,
            int sequenceNumber)
        {
            var allocationsToBeDeleted = this.allocationRepository
                     .Where(@event.TenantId, x => @event.AllocationsToBeDeleted.Contains(x.CommercialDocument.Id))
                     .Where(x => x.FinancialTransaction.Id == @event.AggregateId)
                     .Where(x => !x.IsDeleted);

            foreach (var allocation in allocationsToBeDeleted)
            {
                allocation.IsDeleted = true;
            }
        }

        /// <summary>
        /// Get the transaction By Id.
        /// </summary>
        /// <param name="id">The transaction id.</param>
        /// <returns>The readmodel.</returns>
        protected TReadModel GetFinancialTransactionReadModel(Guid tenantId, Guid id)
        {
            var readModel = this.financialTransactionRepository.GetByIdMaybe(tenantId, id);

            if (readModel.HasNoValue)
            {
                throw new ErrorException(Errors.Accounting.AccountTransactionDoesNotExist(id, nameof(TReadModel), null, null));
            }

            return readModel.Value;
        }
    }
}
