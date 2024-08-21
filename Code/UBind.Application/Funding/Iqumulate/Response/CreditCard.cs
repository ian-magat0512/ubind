// <copyright file="CreditCard.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Funding.Iqumulate.Response
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// Iqumulate Premium Funding Response Credit Card info.
    /// </summary>
    public class CreditCard
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CreditCard"/> class.
        /// </summary>
        [JsonConstructor]
        public CreditCard()
        {
        }

        /// <summary>
        /// Gets the Amount.
        /// </summary>
        [JsonProperty]
        public decimal Amount { get; private set; }

        /// <summary>
        /// Gets the Amount Including Surcharge.
        /// </summary>
        [JsonProperty]
        public decimal AmountIncludingSurcharge { get; private set; }

        /// <summary>
        /// Gets the card type - VISA, Mastercard, Amex.
        /// </summary>
        [JsonProperty]
        public string CardType { get; private set; }

        /// <summary>
        /// Gets the cardholder name.
        /// </summary>
        [JsonProperty]
        public string CardholderName { get; private set; }

        /// <summary>
        /// Gets the surcharge.
        /// </summary>
        [JsonProperty]
        public decimal Surcharge { get; private set; }

        /// <summary>
        /// Gets the token.
        /// </summary>
        [JsonProperty]
        public string Token { get; private set; }

        /// <summary>
        /// Gets the transaction date time.
        /// </summary>
        [JsonProperty]
        public DateTime? TxnDateTime { get; private set; }
    }
}
