// <copyright file="RefundAllocationReadModel.cs" company="uBind">
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
    /// Refund allocation (to credit notes) read model.
    /// </summary>
    public class RefundAllocationReadModel : FinancialTransactionAllocationReadModel<RefundReadModel, CreditNote>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RefundAllocationReadModel"/> class.
        /// </summary>
        /// <param name="refund">The refund.</param>
        /// <param name="amount">The amount of allocation.</param>
        /// <param name="creditNote">The credit note.</param>
        /// <param name="createdTimestamp">The created time of this allocation.</param>
        public RefundAllocationReadModel(
            RefundReadModel refund,
            Money amount,
            CreditNote creditNote,
            Instant createdTimestamp)
            : base(refund.TenantId, Guid.NewGuid(), amount, refund, creditNote, createdTimestamp)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RefundAllocationReadModel"/> class.
        /// Parameterless constructor for EF.
        /// </summary>
        public RefundAllocationReadModel()
        {
        }
    }
}
