// <copyright file="CreditCardPaymentResultModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// Model for responses to payment requests.
    /// </summary>
    public class CreditCardPaymentResultModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CreditCardPaymentResultModel"/> class.
        /// </summary>
        /// <param name="quote">The quote of the payment attempt was for.</param>
        public CreditCardPaymentResultModel(Domain.Aggregates.Quote.Quote quote)
        {
            var paymentAttempt = quote.LatestPaymentAttemptResult;
            if (paymentAttempt == null)
            {
                throw new InvalidOperationException("Payment results cannot be returned if no attempt was made.");
            }

            this.Outcome = paymentAttempt.Outcome;
            this.Errors = paymentAttempt.Errors;
        }

        /// <summary>
        /// Gets a value indicating the outcome of the payment attempt.
        /// </summary>
        [JsonProperty]
        [JsonConverter(typeof(StringEnumConverter))]
        public Domain.Aggregates.Quote.Outcome Outcome { get; }

        /// <summary>
        /// Gets any errors returned by the payment gateway.
        /// </summary>
        [JsonProperty]
        public IEnumerable<string> Errors { get; }

        /// <summary>
        /// Gets a value indicating whether the payment succeeded.
        /// </summary>
        public bool Succeeded => this.Outcome == Domain.Aggregates.Quote.Outcome.Success;
    }
}
