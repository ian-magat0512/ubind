// <copyright file="BankAccountDetails.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote.Payment
{
    /// <summary>
    /// Bank account details required for setting up direct debit.
    /// </summary>
    public class BankAccountDetails : IPaymentMethodDetails
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BankAccountDetails"/> class.
        /// </summary>
        /// <param name="name">The bank account name.</param>
        /// <param name="number">The bank account number.</param>
        /// <param name="bsb">The BSB number of the bank.</param>
        public BankAccountDetails(
            string name,
            string number,
            string bsb)
        {
            this.Name = name;
            this.Number = number;
            this.BSB = bsb;
        }

        /// <summary>
        /// Gets the bank account number.
        /// </summary>
        public string Number { get; }

        /// <summary>
        /// Gets the bank account name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the bank BSB number.
        /// </summary>
        public string BSB { get; }
    }
}
