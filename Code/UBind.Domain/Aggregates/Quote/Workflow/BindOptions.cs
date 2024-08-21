// <copyright file="BindOptions.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote.Workflow
{
    using System;

    /// <summary>
    /// Represents the configured optional actions that binding should trigger.
    /// </summary>
    [Flags]
    public enum BindOptions
    {
        /// <summary>
        /// No actions configured.
        /// </summary>
        None = 0,

        /// <summary>
        /// Binding should trigger a policy to be issued.
        /// </summary>
        Policy = 1,

        /// <summary>
        /// Binding should trigger a transaction record (i.e. invoice or credit note) to be issued.
        /// </summary>
        TransactionRecord = 2,

        /// <summary>
        /// Binding should trigger an invoice and policy to be used.
        /// </summary>
        PolicyAndTransactionRecord = Policy | TransactionRecord,
    }
}
