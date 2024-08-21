﻿// <copyright file="DeftPaymentResponse.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Payment.Deft
{
    using DotLiquid;
    using Newtonsoft.Json;

    /// <summary>
    /// Payment response from Deft Payment gateway.
    /// </summary>
    public class DeftPaymentResponse : Drop
    {
        /// <summary>
        /// Gets the time the request was received in format ddMMyyyyHHmmss (probably - the docs are ambiguous).
        /// </summary>
        [JsonProperty]
        public string ResponseTimestamp { get; private set; }

        /// <summary>
        /// Gets the time the transaction took place in format yyyy-MM-dd.
        /// </summary>
        [JsonProperty]
        public string SettlementDate { get; private set; }

        /// <summary>
        /// Gets the DEFT reference number for the transaction.
        /// </summary>
        [JsonProperty]
        public string Drn { get; private set; }

        /// <summary>
        /// Gets or sets the customer reference number for the transaction.
        /// </summary>
        [JsonProperty]
        public string Crn { get; set; }

        /// <summary>
        /// Gets the reference number unique to each receipt for reconciliation of the transaction.
        /// </summary>
        [JsonProperty]
        public string ReceiptNumber { get; private set; }

        /// <summary>
        /// Gets the surcharge rate for the transaction (I assume - this is undocumented).
        /// </summary>
        [JsonProperty]
        public decimal SurchargeRate { get; private set; }

        /// <summary>
        /// Gets the flat fee (in the currency specified) charged to the Payer for the service.
        /// </summary>
        [JsonProperty]
        public decimal Fee { get; private set; }

        /// <summary>
        /// Gets the total surcharge amount to be charged for the transaction.
        /// </summary>
        [JsonProperty]
        public decimal SurchargeAmount { get; private set; }

        /// <summary>
        /// Gets the calculated total (in the currency specified) to charge the Payer.
        /// </summary>
        /// <remarks>
        /// The total will be calculated as the sum of the amount, surcharge and fee.
        /// .</remarks>
        [JsonProperty]
        public decimal TotalAmount { get; private set; }

        /// <summary>
        /// Gets the tokenisation of the payment information.
        /// </summary>
        /// <remarks>
        /// Token will not be returned in case of payment failure or if selected not to.
        /// .</remarks>
        [JsonProperty]
        public string SecureToken { get; private set; }
    }
}
