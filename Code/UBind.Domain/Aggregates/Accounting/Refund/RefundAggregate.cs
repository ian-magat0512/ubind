// <copyright file="RefundAggregate.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Accounting.Refund
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;
    using NodaMoney;
    using NodaTime;
    using UBind.Domain.Accounting;
    using UBind.Domain.Aggregates;

    /// <summary>
    /// THe refund aggregate for accounting purposes.
    /// </summary>
    public partial class RefundAggregate : FinancialTransactionAggregate<CreditNote>
    {
        /// <summary>
        /// It should be needed during de/serialization of the aggregate.
        /// </summary>
        [JsonConstructor]
        public RefundAggregate()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RefundAggregate"/> class,
        /// calculating state by re-applying existing events.
        /// </summary>
        /// <param name="events">Existing events.</param>
        private RefundAggregate(IEnumerable<IEvent<FinancialTransactionAggregate<CreditNote>, Guid>> events)
            : base(events)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RefundAggregate"/> class.
        /// </summary>
        private RefundAggregate(
            Guid tenantId,
            Money amount,
            string referenceNumber,
            Instant transactionTime,
            Instant createdTimestamp,
            TransactionParties transactionPartyModel,
            Guid? performingUserId)
            : base(tenantId, amount, referenceNumber, transactionTime, createdTimestamp, transactionPartyModel, performingUserId)
        {
        }

        /// <summary>
        /// Initializes static members of the <see cref="RefundAggregate"/> class.
        /// </summary>
        /// <param name="amount">The transaction amount.</param>
        /// <param name="referenceNumber">The reference number.</param>
        /// <param name="transactionTime">The time of transaction.</param>
        /// <param name="createdTimestamp">The created time (for auditing purposes).</param>
        /// <param name="transactionPartyModel">The parties involved in the transaction.</param>
        /// <param name="performingUserId">The user that performs this action.</param>
        /// <returns>The refund aggregate.</returns>
        public static RefundAggregate CreateNewRefund(
            Guid tenantId,
            Money amount,
            string referenceNumber,
            Instant transactionTime,
            Instant createdTimestamp,
            TransactionParties transactionPartyModel,
            Guid? performingUserId)
        {
            return new RefundAggregate(tenantId, amount, referenceNumber, transactionTime, createdTimestamp, transactionPartyModel, performingUserId);
        }

        /// <summary>
        /// Initializes static members of the <see cref="RefundAggregate"/> class.
        /// </summary>
        /// <param name="events">Existing events.</param>
        /// <returns>The new instance of the refund aggregate.</returns>
        public static RefundAggregate LoadFromEvents(IEnumerable<IEvent<FinancialTransactionAggregate<CreditNote>, Guid>> events)
        {
            return new RefundAggregate(events);
        }

        /// <summary>
        /// Allocate refund to credit note.
        /// </summary>
        /// <param name="transactionPartyModel">The parties involved in the transaction.</param>
        /// <param name="commercialDocument">The credit note.</param>
        /// <param name="timestamp">The timestamp.</param>
        /// <param name="performingUserId">The user that performs this action.</param>
        public override void AllocateToCommercialDocument(
            TransactionParties transactionPartyModel,
            CreditNote commercialDocument,
            Instant timestamp,
            Guid? performingUserId)
        {
            var allocateEvent = new TransactionAllocatedEvent(this.TenantId, this.Id, this.Amount, commercialDocument.Id, timestamp, performingUserId);
            this.ApplyNewEvent(allocateEvent);
        }
    }
}
