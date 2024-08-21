// <copyright file="CreatePaymentCommand.cs" company="uBind">
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
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// Command for creating a payment.
    /// </summary>
    public class CreatePaymentCommand : CreateFinancialTransactionCommand, ICommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CreatePaymentCommand"/> class.
        /// </summary>
        /// <param name="amount">The amount of transaction.</param>
        /// <param name="transactionTime">The time of transaction.</param>
        /// <param name="transactionPartyModel">The parties involved in the transaction.</param>
        /// <param name="performingUserId">The performing user id.</param>
        /// <param name="referenceNumber">The reference number.</param>
        public CreatePaymentCommand(
            Guid tenantId,
            Money amount,
            Instant transactionTime,
            TransactionParties transactionPartyModel,
            Guid? performingUserId,
            string referenceNumber)
            : base(tenantId, amount, transactionTime, transactionPartyModel, performingUserId, referenceNumber)
        {
        }
    }
}
