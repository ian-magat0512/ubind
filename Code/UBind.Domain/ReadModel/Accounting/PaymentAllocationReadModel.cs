// <copyright file="PaymentAllocationReadModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel.Accounting
{
    using System;
    using NodaMoney;
    using NodaTime;
    using UBind.Domain.Accounting;

    /// <summary>
    /// Payment allocation (to invoice) read model.
    /// </summary>
    public class PaymentAllocationReadModel : FinancialTransactionAllocationReadModel<PaymentReadModel, Invoice>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PaymentAllocationReadModel"/> class.
        /// </summary>
        /// <param name="payment">The payment.</param>
        /// <param name="amount">The amount of allocation.</param>
        /// <param name="invoice">The invoice.</param>
        /// <param name="createdTimestamp">The created time of this allocation.</param>
        public PaymentAllocationReadModel(
            PaymentReadModel payment,
            Money amount,
            Invoice invoice,
            Instant createdTimestamp)
            : base(payment.TenantId, Guid.NewGuid(), amount, payment, invoice, createdTimestamp)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PaymentAllocationReadModel"/> class.
        /// Parameterless constructor for EF.
        /// </summary>
        public PaymentAllocationReadModel()
        {
        }
    }
}
