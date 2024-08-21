// <copyright file="Payment.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.SerialisedEntitySchemaObject
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// This class is needed because we need to generate json representation of payment object that conforms with serialized-entity-schema.json file.
    /// </summary>
    public class Payment : ISchemaObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Payment"/> class.
        /// </summary>
        /// <param name="calculation">The calculation object.</param>
        /// <param name="isFormatted">An optional flag that indicates whether the payment-related fields should have their values formatted.</param>
        public Payment(JObject calculation, bool isFormatted = false)
        {
            var priceComponents = calculation.SelectToken("JObject.payment.priceComponents");
            var currencyCode = calculation.SelectToken("PayablePrice.CurrencyCode")?.ToString() ??
                calculation.SelectToken("RefundBreakdown.CurrencyCode")?.ToString() ??
                "AUD";
            if (priceComponents?.Type == JTokenType.Object)
            {
                if (isFormatted)
                {
                    this.PriceComponents = new PriceBreakdownFormatted(priceComponents.ToObject<JObject>(), currencyCode);
                }
                else
                {
                    this.PriceComponents = new PriceBreakdown(priceComponents.ToObject<JObject>());
                }
            }

            var payableComponents = calculation.SelectToken("PayablePrice");
            if (payableComponents?.Type == JTokenType.Object)
            {
                if (isFormatted)
                {
                    this.PayableComponents = new PriceBreakdownFormatted(payableComponents.ToObject<JObject>(), false);
                }
                else
                {
                    this.PayableComponents = new PriceBreakdown(payableComponents.ToObject<JObject>());
                }
            }

            var refundComponents = calculation.SelectToken("RefundBreakdown");
            if (refundComponents?.Type == JTokenType.Object)
            {
                if (isFormatted)
                {
                    var refundComponentFormatted = new PriceBreakdownFormatted(refundComponents.ToObject<JObject>());
                    this.RefundComponents = string.IsNullOrEmpty(refundComponentFormatted.TotalPayable) ? null : refundComponentFormatted;
                }
                else
                {
                    var refundComponent = new PriceBreakdown(refundComponents.ToObject<JObject>());
                    this.RefundComponents = refundComponent.TotalPayable == 0 ? null : refundComponent;
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Payment"/> class.
        /// </summary>
        [JsonConstructor]
        private Payment()
        {
        }

        /// <summary>
        /// Gets or sets the breakdown of the price calculated for the full term of a policy.
        /// </summary>
        [JsonProperty(PropertyName = "priceComponents", Order = 1, NullValueHandling = NullValueHandling.Ignore)]
        public dynamic PriceComponents { get; set; }

        /// <summary>
        /// Gets or sets the breakdown of the amount the customer needs to pay for a particular policy transaction.
        /// This includes the pro rata calculation applied to mid-term adjustment transactions.
        /// </summary>
        [JsonProperty(PropertyName = "payableComponents", Order = 2, NullValueHandling = NullValueHandling.Ignore)]
        public dynamic PayableComponents { get; set; }

        /// <summary>
        /// Gets or sets the breakdown of the amount refundable in the case of a transaction where the payable amount is negative.
        /// These values are used if a refundable amount needs to be merged into a template.
        /// </summary>
        [JsonProperty(PropertyName = "refundComponents", Order = 3, NullValueHandling = NullValueHandling.Ignore)]
        public dynamic RefundComponents { get; set; }
    }
}
