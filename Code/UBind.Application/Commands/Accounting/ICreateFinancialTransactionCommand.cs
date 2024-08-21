// <copyright file="ICreateFinancialTransactionCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Commands.Accounting
{
    using System;
    using NodaMoney;
    using NodaTime;
    using UBind.Domain.Accounting;

    /// <summary>
    /// Command for creating a financial transaction.
    /// </summary>
    public interface ICreateFinancialTransactionCommand
    {
        /// <summary>
        /// Gets the tenant Id associated to this command.
        /// </summary>
        Guid TenantId { get; }

        /// <summary>
        /// Gets the amount of the transaction.
        /// </summary>
        Money Amount { get; }

        /// <summary>
        /// Gets the parties involved in the transaction.
        /// </summary>
        TransactionParties TransactionParties { get; }

        /// <summary>
        /// Gets the instant in time the transaction occurred.
        /// </summary>
        Instant TransactionTime { get; }

        /// <summary>
        /// Gets the user the performs this command.
        /// </summary>
        Guid? PerformingUserId { get; }

        /// <summary>
        /// Gets the reference number.
        /// </summary>
        string ReferenceNumber { get; }
    }
}
