// <copyright file="CalculatedPayment.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using System;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using UBind.Domain.Extensions;
    using UBind.Domain.Json;
    using UBind.Domain.ReadWriteModel;

    /// <summary>
    /// Payment information from calculation result.
    /// </summary>
    public class CalculatedPayment
    {
        private const string PaymentName = "payment";

        /// <summary>
        /// Initializes a new instance of the <see cref="CalculatedPayment"/> class.
        /// </summary>
        /// <param name="total">The payment breakdown.</param>
        /// <param name="installments">The payment schedule.</param>
        /// <param name="currencyCode">The currency code, e.g. "AUD".</param>
        public CalculatedPayment(
            CalculatedPaymentTotal total,
            CalculatedPaymentInstallments installments,
            string currencyCode)
        {
            if (currencyCode == null)
            {
                throw new NullReferenceException("When instantiating CalculatedPayment, the passed parameter for currencyCode was null.");
            }

            total.ThrowIfArgumentNull(nameof(total));
            installments.ThrowIfArgumentNull(nameof(installments));
            this.ComponentsV1 = total;
            this.Installments = installments;
            this.CurrencyCode = currencyCode;
        }

        [JsonConstructor]
        private CalculatedPayment()
        {
        }

        /// <summary>
        /// Gets or sets the currency code, e.g. "AUD", "USD", "GBP" etc.
        /// </summary>
        [JsonProperty(PropertyName = "currencyCode")]
        public string CurrencyCode { get; set; } = PriceBreakdown.DefaultCurrencyCode;

        /// <summary>
        /// Gets the total premium information.
        /// </summary>
        [JsonProperty(PropertyName = "total")]
        public CalculatedPaymentTotal ComponentsV1 { get; private set; }

        /// <summary>
        /// Gets the price components.
        /// </summary>
        [JsonProperty(PropertyName = "priceComponents")]
        public CalculatedPriceComponents ComponentsV2 { get; private set; }

        /// <summary>
        /// Gets the installments information.
        /// </summary>
        [JsonProperty(PropertyName = "instalments")]
        public CalculatedPaymentInstallments Installments { get; private set; }

        /// <summary>
        /// Gets the installments information.
        /// </summary>
        [JsonProperty(PropertyName = "outputVersion")]
        public int OutputVersion { get; private set; }

        /// <summary>
        /// Creates the CalculatedPayment instance from the CalculationResult.
        /// </summary>
        /// <param name="calculationResultData">The calculation result data.</param>
        /// <returns>A CalculatedPayment instance.</returns>
        public static CalculatedPayment CreateFromCalculationResult(CachingJObjectWrapper calculationResultData)
        {
            var paymentToken = calculationResultData.JObject[PaymentName];
            if (paymentToken == null)
            {
                throw new FormatException($"Calculation result JSON must include {PaymentName} object.");
            }

            var calculatedPayment = paymentToken.ToObject<CalculatedPayment>();
            if (calculatedPayment.ComponentsV2 != null)
            {
                JToken currencyCodeToken = calculationResultData.JObject.SelectToken("payment.currencyCode");
                if (currencyCodeToken != null)
                {
                    string currencyCode = currencyCodeToken.ToString();
                    if (currencyCode != null)
                    {
                        calculatedPayment.ComponentsV2.CurrencyCode = currencyCode;
                    }
                }
            }

            return calculatedPayment;
        }
    }
}
