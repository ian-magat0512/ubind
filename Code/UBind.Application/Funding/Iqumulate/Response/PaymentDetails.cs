// <copyright file="PaymentDetails.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Funding.Iqumulate.Response
{
    using Newtonsoft.Json;

    /// <summary>
    /// Iqumulate Premium Funding Response Payment Details.
    /// </summary>
    public class PaymentDetails
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PaymentDetails"/> class.
        /// </summary>
        [JsonConstructor]
        public PaymentDetails()
        {
        }

        /// <summary>
        /// Gets Credit Card payment details.
        /// </summary>
        [JsonProperty]
        public CreditCard CreditCard { get; private set; }

        /// <summary>
        /// Gets Direct Debit information.
        /// </summary>
        [JsonProperty]
        public DirectDebit DirectDebit { get; private set; }

        /// <summary>
        /// Gets DirectDebit.
        /// </summary>
        [JsonProperty]
        public string InitialPaymentMethod { get; private set; }

        /// <summary>
        /// Gets a value indicating whether initial payment is already paid.
        /// </summary>
        [JsonProperty]
        public bool InitialPaymentPaid { get; private set; }

        /// <summary>
        /// Gets the ongoing payment method.
        /// </summary>
        [JsonProperty]
        public string OngoingPaymentMethod { get; private set; }
    }
}
