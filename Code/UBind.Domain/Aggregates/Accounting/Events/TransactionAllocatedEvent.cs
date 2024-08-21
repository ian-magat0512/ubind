// <copyright file="TransactionAllocatedEvent.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Accounting
{
    using System;
    using Newtonsoft.Json;
    using NodaMoney;
    using NodaTime;
    using UBind.Domain.Aggregates;

    /// <content>
    /// Nested event and its handling for the <see cref="FinancialTransactionAggregate{TCommercialDocument}" />.
    /// </content>
    public abstract partial class FinancialTransactionAggregate<TCommercialDocument>
    {
        /// <summary>
        /// Payments are Allocated to Invoices. Refunds to Credit Notes.
        /// </summary>
        public class TransactionAllocatedEvent : Event<FinancialTransactionAggregate<TCommercialDocument>, Guid>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="TransactionAllocatedEvent"/> class.
            /// </summary>
            /// <param name="transactionId">The financial transaction id.</param>
            /// <param name="amount">The amount to be allocated.</param>
            /// <param name="commercialDocumentId">The commercial document id which this transaction is being allocated onto.</param>
            /// <param name="createdTimestamp">The created time (for auditing purposes).</param>
            /// <param name="performingUserId">The user that performs this event.</param>
            public TransactionAllocatedEvent(
                Guid tenantId,
                Guid transactionId,
                Money amount,
                Guid commercialDocumentId,
                Instant createdTimestamp,
                Guid? performingUserId)
                : base(tenantId, transactionId, performingUserId, createdTimestamp)
            {
                this.CommercialDocumentId = commercialDocumentId;
                this.Amount = amount;
            }

            [JsonConstructor]
            private TransactionAllocatedEvent()
                : base(default, default, default, default)
            {
            }

            /// <summary>
            /// Gets the Commercial Document Id which this transaction is being allocated onto.(ie. invoice Id or credit note Id).
            /// </summary>
            [JsonProperty]
            public Guid CommercialDocumentId { get; private set; }

            /// <summary>
            /// Gets the amount to be allocated.
            /// </summary>
            [JsonProperty]
            public Money Amount { get; private set; }
        }
    }
}
