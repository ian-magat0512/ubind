// <copyright file="PaymentReadModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel.Accounting
{
    using System;
    using NodaMoney;
    using NodaTime;
    using UBind.Domain.ValueTypes;

    /// <summary>
    /// Read model of payments for accounting feature.
    /// </summary>
    public class PaymentReadModel : FinancialTransactionReadModel<PaymentAllocationReadModel>, IReadModel<Guid>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PaymentReadModel"/> class.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="id">The payment id.</param>
        /// <param name="paymentAmount">The payment amount.</param>
        /// <param name="referenceNumber">The reference number.</param>
        /// <param name="payerId">The payer id.</param>
        /// <param name="payerType">The payer type.</param>
        /// <param name="payeeId">The payee id.</param>
        /// <param name="payeeType">The payee type.</param>
        /// <param name="createdTimestamp">The time the payment was created.</param>
        /// <param name="transactionTime">The time of payment transaction.</param>
        public PaymentReadModel(
            Guid tenantId,
            Guid id,
            Money paymentAmount,
            string referenceNumber,
            Guid payerId,
            TransactionPartyType payerType,
            Guid? payeeId,
            TransactionPartyType? payeeType,
            Instant createdTimestamp,
            Instant transactionTime)
            : base(tenantId, id, paymentAmount, referenceNumber, payerId, payerType, payeeId, payeeType, createdTimestamp, transactionTime)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PaymentReadModel"/> class.
        /// public constructor for PaymentReadModel.
        /// </summary>
        public PaymentReadModel()
        {
        }
    }
}
