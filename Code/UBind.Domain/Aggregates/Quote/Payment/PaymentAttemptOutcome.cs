// <copyright file="PaymentAttemptOutcome.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote.Payment
{
    /// <summary>
    /// Possible outcomes of a payment attempt.
    /// </summary>
    public enum PaymentAttemptOutcome
    {
        /// <summary>
        /// Default value.
        /// </summary>
        None = 0,

        /// <summary>
        /// The payment attempt was successful.
        /// </summary>
        Success = 1,

        /// <summary>
        /// The payment attempt was not successful.
        /// The customer could try again with different card details.
        /// </summary>
        Failed = 2,

        /// <summary>
        /// There was an error attmpting to process the payment.
        /// This is not a problem with the customer's card, but a system error that will need developer attention.
        /// </summary>
        Error = 3,
    }
}
