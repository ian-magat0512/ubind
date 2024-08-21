// <copyright file="SetTotalPayableOnExistingPolicyTransactionsFromLatestCalculationResultCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Commands.Migration
{
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// Command for setting the TotalPayable of existing PolicyTransactions from LatestCalculationResult.
    /// This is a migration command called on startup.
    /// </summary>
    public class SetTotalPayableOnExistingPolicyTransactionsFromLatestCalculationResultCommand : ICommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SetTotalPayableOnExistingPolicyTransactionsFromLatestCalculationResultCommand"/> class.
        /// </summary>
        public SetTotalPayableOnExistingPolicyTransactionsFromLatestCalculationResultCommand()
        {
        }
    }
}