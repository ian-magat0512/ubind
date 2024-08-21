// <copyright file="IFinancialTransactionReadModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel.Accounting
{
    using System;
    using System.Collections.Generic;
    using NodaTime;
    using UBind.Domain.ValueTypes;

    /// <summary>
    /// Contract for financial transaction readmodels.
    /// </summary>
    /// <typeparam name="TAllocation">The allocation type.</typeparam>
    public interface IFinancialTransactionReadModel<TAllocation>
        : IEntityReadModel<Guid>
        where TAllocation : class
    {
        /// <summary>
        /// Gets or sets the Payer Id.
        /// </summary>
        Guid PayerId { get; set; }

        /// <summary>
        /// Gets or sets the Payer type.
        /// </summary>
        TransactionPartyType PayerType { get; set; }

        /// <summary>
        /// Gets or sets the PayeeId.
        /// </summary>
        Guid? PayeeId { get; set; }

        /// <summary>
        /// Gets or sets the Payee Type.
        /// </summary>
        TransactionPartyType? PayeeType { get; set; }

        /// <summary>
        /// Gets the transaction amount.
        /// </summary>
        decimal Amount { get; }

        /// <summary>
        /// Gets the currency of the transaction.
        /// </summary>
        string Currency { get; }

        /// <summary>
        /// Gets the Reference Number.
        /// </summary>
        string ReferenceNumber { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the readmodel is deleted.
        /// </summary>
        bool IsDeleted { get; set; }

        /// <summary>
        /// Gets or sets the commercial document Ids on which this transaction would be allocatedOnto.
        /// </summary>
        ICollection<TAllocation> Allocations { get; set; }

        /// <summary>
        /// Gets the time of the transaction transaction.
        /// </summary>
        Instant TransactionTimestamp { get; }
    }
}
