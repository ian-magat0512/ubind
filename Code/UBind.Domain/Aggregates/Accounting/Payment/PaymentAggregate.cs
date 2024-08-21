// <copyright file="PaymentAggregate.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Accounting.Payment
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;
    using NodaMoney;
    using NodaTime;
    using UBind.Domain.Accounting;
    using UBind.Domain.Aggregates;

    /// <summary>
    /// The payment aggregate for accounting purposes.
    /// </summary>
    public partial class PaymentAggregate : FinancialTransactionAggregate<Invoice>
    {
        /// <summary>
        /// It should be needed during de/serialization of the aggregate.
        /// </summary>
        [JsonConstructor]
        public PaymentAggregate()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PaymentAggregate"/> class,
        /// calculating state by re-applying existing events.
        /// </summary>
        /// <param name="events">Existing events.</param>
        private PaymentAggregate(IEnumerable<IEvent<FinancialTransactionAggregate<Invoice>, Guid>> events)
            : base(events)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PaymentAggregate"/> class.
        /// </summary>
        private PaymentAggregate(
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
        /// Initializes static members of the <see cref="PaymentAggregate"/> class.
        /// </summary>
        /// <param name="amount">The transaction amount.</param>
        /// <param name="referenceNumber">The reference number.</param>
        /// <param name="transactionTime">The time of payment transaction.</param>
        /// <param name="createdTimestamp">The created time (for auditing purposes).</param>
        /// <param name="transactionPartyModel">The parties involved in the transaction.</param>
        /// <param name="performingUserId">The user performs this action.</param>
        /// <returns>The payment aggregate.</returns>
        public static PaymentAggregate CreateNewPayment(
            Guid tenantId,
            Money amount,
            string referenceNumber,
            Instant transactionTime,
            Instant createdTimestamp,
            TransactionParties transactionPartyModel,
            Guid? performingUserId)
        {
            return new PaymentAggregate(tenantId, amount, referenceNumber, transactionTime, createdTimestamp, transactionPartyModel, performingUserId);
        }

        /// <summary>
        /// Initializes static members of the <see cref="PaymentAggregate"/> class.
        /// </summary>
        /// <param name="events">Existing events.</param>
        /// <returns>The new instance of the payment aggregate.</returns>
        public static PaymentAggregate LoadFromEvents(IEnumerable<IEvent<FinancialTransactionAggregate<Invoice>, Guid>> events)
        {
            return new PaymentAggregate(events);
        }

        /// <inheritdoc/>
        public override void AllocateToCommercialDocument(
            TransactionParties transactionPartyModel,
            Invoice commercialDocument,
            Instant timestamp,
            Guid? performingUserId)
        {
            var allocateEvent = new TransactionAllocatedEvent(this.TenantId, this.Id, this.Amount, commercialDocument.Id, timestamp, performingUserId);
            this.ApplyNewEvent(allocateEvent);
        }
    }
}
