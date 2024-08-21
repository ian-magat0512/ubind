// <copyright file="IFinancialTransactionAllocationReadModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel.Accounting
{
    using System;
    using NodaTime;

    /// <summary>
    /// The base class for financial transaction allocations .
    /// </summary>
    /// <typeparam name="TFinancialTransactionReadModel">The financial Transaction readmodel type.</typeparam>
    /// <typeparam name="TCommercialDocumentReadModel">The commercial document readmodel type.</typeparam>
    public interface IFinancialTransactionAllocationReadModel<TFinancialTransactionReadModel, TCommercialDocumentReadModel>
        : IReadModel<Guid>
        where TFinancialTransactionReadModel : class
        where TCommercialDocumentReadModel : class
    {
        /// <summary>
        /// Gets the financial transaction(paymentor or refund).
        /// </summary>
        TFinancialTransactionReadModel FinancialTransaction { get; }

        /// <summary>
        /// Gets the commercial document(invoice or refund).
        /// </summary>
        TCommercialDocumentReadModel CommercialDocument { get; }

        /// <summary>
        /// Gets the time the transaction was created.
        /// </summary>
        Instant CreatedTimestamp { get; }

        /// <summary>
        /// Gets or sets the time the transaction was created in ticks.
        /// </summary>
        long CreatedTicksSinceEpoch { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the transaction was deleted.
        /// </summary>
        bool IsDeleted { get; set; }

        /// <summary>
        /// Gets the amount of the allocation.
        /// </summary>
        decimal Amount { get; }
    }
}
