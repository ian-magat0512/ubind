// <copyright file="SetTotalPayableOnExistingQuotesFromLatestCalculationResultCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Commands.Migration
{
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// Command for setting the TotalPayable of existing quotes from LatestCalculationResult.
    /// This is a migration command called on startup.
    /// </summary>
    public class SetTotalPayableOnExistingQuotesFromLatestCalculationResultCommand : ICommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SetTotalPayableOnExistingQuotesFromLatestCalculationResultCommand"/> class.
        /// </summary>
        public SetTotalPayableOnExistingQuotesFromLatestCalculationResultCommand()
        {
        }
    }
}
