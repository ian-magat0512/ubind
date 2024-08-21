// <copyright file="FinancialTransactionEventAggregator.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence
{
    using System;
    using System.Collections.Generic;
    using UBind.Domain.Accounting;
    using UBind.Domain.Aggregates;
    using UBind.Domain.Aggregates.Accounting;

    /// <summary>
    /// Aggregator for multiple dispatch of events from payment aggregates.
    /// </summary>
    /// <typeparam name="TCommercialDocument">The commercial document type (invoice or credit note).</typeparam>
    public abstract class FinancialTransactionEventAggregator<TCommercialDocument> : IFinancialTransactionEventObserver<TCommercialDocument>
        where TCommercialDocument : class, ICommercialDocument<Guid>
    {
        private readonly List<IFinancialTransactionEventObserver<TCommercialDocument>> observers;

        /// <summary>
        /// Initializes a new instance of the <see cref="FinancialTransactionEventAggregator{TCommercialDocument}"/> class.
        /// </summary>
        /// <param name="writer">The readmodel writer.</param>
        public FinancialTransactionEventAggregator(
            IFinancialTransactionReadModelWriter<TCommercialDocument> writer)
        {
            this.observers = new List<IFinancialTransactionEventObserver<TCommercialDocument>>
            {
                writer,
            };
        }

        public void Dispatch(
            FinancialTransactionAggregate<TCommercialDocument> aggregate,
            IEvent<FinancialTransactionAggregate<TCommercialDocument>, Guid> @event,
            int sequenceNumber,
            IEnumerable<Type> observerTypes = null)
        {
            this.observers.ForEach(o => o.Dispatch(aggregate, @event, sequenceNumber));
        }
    }
}
