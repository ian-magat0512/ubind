// <copyright file="PremiumBreakdownFormatted.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.SerialisedEntitySchemaObject
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using NodaMoney;

    public class PremiumBreakdownFormatted : ISchemaObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PremiumBreakdown"/> class.
        /// </summary>
        /// <param name="payableComponent">The payable/refund component object.</param>
        public PremiumBreakdownFormatted(JObject payableComponent)
        {
            this.FormatPropertiesToMoneyString(payableComponent);
        }

        public PremiumBreakdownFormatted(JObject payableComponent, bool skipIfTotalPayableIsZero)
        {
            this.FormatPropertiesToMoneyString(payableComponent, skipIfTotalPayableIsZero);
        }

        public PremiumBreakdownFormatted(JObject priceObject, string currencyCode)
        {
            this.BasePremium = GetValueOrDefault(priceObject.GetValue("basePremium"), currencyCode);
            this.TerrorismPremium = GetValueOrDefault(priceObject.GetValue("terrorismPremium"), currencyCode);
            var esl = priceObject.GetValue("esl") ?? priceObject.GetValue("ESL");
            this.EslNsw = priceObject.GetValue("emergencyServicesLevyNSW") == null
                ? GetValueOrDefault(esl, currencyCode)
                : GetValueOrDefault(priceObject.GetValue("emergencyServicesLevyNSW"), currencyCode);
            this.EslTas = GetValueOrDefault(priceObject.GetValue("emergencyServicesLevyTAS"), currencyCode);
            this.Esl = GetValueOrDefault(esl, currencyCode);
            this.PremiumGst = GetValueOrDefault(priceObject.GetValue("premiumGST"), currencyCode);
            this.StampDutyAct = GetValueOrDefault(priceObject.GetValue("stampDutyACT"), currencyCode);
            this.StampDutyNsw = GetValueOrDefault(priceObject.GetValue("stampDutyNSW"), currencyCode);
            this.StampDutyNt = GetValueOrDefault(priceObject.GetValue("stampDutyNT"), currencyCode);
            this.StampDutyQld = GetValueOrDefault(priceObject.GetValue("stampDutyQLD"), currencyCode);
            this.StampDutySa = GetValueOrDefault(priceObject.GetValue("stampDutySA"), currencyCode);
            this.StampDutyTas = GetValueOrDefault(priceObject.GetValue("stampDutyTAS"), currencyCode);
            this.StampDutyVic = GetValueOrDefault(priceObject.GetValue("stampDutyVIC"), currencyCode);
            this.StampDutyWa = GetValueOrDefault(priceObject.GetValue("stampDutyWA"), currencyCode);
            this.StampDutyTotal = GetValueOrDefault(priceObject.GetValue("stampDutyTotal"), currencyCode);
            this.TotalPremium = GetValueOrDefault(priceObject.GetValue("totalPremium"), currencyCode);
        }

        /// <summary>
        /// Gets or sets the base premium.
        /// </summary>
        [JsonProperty(PropertyName = "basePremium", Order = 1)]
        public string BasePremium { get; set; }

        /// <summary>
        /// Gets or sets the terrorism premium.
        /// </summary>
        [JsonProperty(PropertyName = "terrorismPremium", Order = 2)]
        public string TerrorismPremium { get; set; }

        [JsonProperty(PropertyName = "emergencyServicesLevyNSW", Order = 3)]
        public string EslNsw { get; set; }

        [JsonProperty(PropertyName = "emergencyServicesLevyTAS", Order = 4)]
        public string EslTas { get; set; }

        /// <summary>
        /// Gets or sets the esl.
        /// </summary>
        [JsonProperty(PropertyName = "emergencyServicesLevyTotal", Order = 5)]
        public string Esl { get; set; }

        /// <summary>
        /// Gets or sets the premium Gst.
        /// </summary>
        [JsonProperty(PropertyName = "premiumGst", Order = 6)]
        public string PremiumGst { get; set; }

        /// <summary>
        /// Gets or sets the stamp duty ACT.
        /// </summary>
        [JsonProperty(PropertyName = "stampDutyAct", Order = 7)]
        public string StampDutyAct { get; set; }

        /// <summary>
        /// Gets or sets the stamp duty NSW.
        /// </summary>
        [JsonProperty(PropertyName = "stampDutyNsw", Order = 8)]
        public string StampDutyNsw { get; set; }

        /// <summary>
        /// Gets or sets the stamp duty NT.
        /// </summary>
        [JsonProperty(PropertyName = "stampDutyNt", Order = 9)]
        public string StampDutyNt { get; set; }

        /// <summary>
        /// Gets or sets the stamp duty QLD.
        /// </summary>
        [JsonProperty(PropertyName = "stampDutyQld", Order = 10)]
        public string StampDutyQld { get; set; }

        /// <summary>
        /// Gets or sets the stamp duty SA.
        /// </summary>
        [JsonProperty(PropertyName = "stampDutySa", Order = 11)]
        public string StampDutySa { get; set; }

        /// <summary>
        /// Gets or sets the stamp duty TAS.
        /// </summary>
        [JsonProperty(PropertyName = "stampDutyTas", Order = 12)]
        public string StampDutyTas { get; set; }

        /// <summary>
        /// Gets or sets the stamp duty VIC.
        /// </summary>
        [JsonProperty(PropertyName = "stampDutyVic", Order = 13)]
        public string StampDutyVic { get; set; }

        /// <summary>
        /// Gets or sets the stamp duty WA.
        /// </summary>
        [JsonProperty(PropertyName = "stampDutyWa", Order = 14)]
        public string StampDutyWa { get; set; }

        /// <summary>
        /// Gets or sets the stamp duty total.
        /// </summary>
        [JsonProperty(PropertyName = "stampDutyTotal", Order = 15)]
        public string StampDutyTotal { get; set; }

        /// <summary>
        /// Gets or sets the total premium.
        /// </summary>
        [JsonProperty(PropertyName = "totalPremium", Order = 16)]
        public string TotalPremium { get; set; }

        protected static string GetValueOrDefault(JToken token, string currencyCode)
        {
            if (token == null)
            {
                return new Money(0.00, currencyCode).ToString();
            }

            return token.Value<string>();
        }

        private void FormatPropertiesToMoneyString(JObject payableComponent, bool skipIfTotalPayableIsZero = true)
        {
            var priceBreakdown = payableComponent.ToObject<Domain.ReadWriteModel.PriceBreakdown>();
            if (priceBreakdown != null)
            {
                if (skipIfTotalPayableIsZero && priceBreakdown.TotalPayable == 0)
                {
                    return;
                }

                var currencyCode = priceBreakdown.CurrencyCode;
                this.BasePremium = new Money(priceBreakdown.BasePremium, currencyCode).ToString();
                this.TerrorismPremium = new Money(priceBreakdown.TerrorismPremium, currencyCode).ToString();
                this.EslNsw = priceBreakdown.EmergencyServicesLevyNSW == 0m
                    ? new Money(priceBreakdown.Esl, currencyCode).ToString()
                    : new Money(priceBreakdown.EmergencyServicesLevyNSW, currencyCode).ToString();
                this.EslTas = new Money(priceBreakdown.EmergencyServicesLevyTAS, currencyCode).ToString();
                this.Esl = new Money(priceBreakdown.Esl, currencyCode).ToString();
                this.PremiumGst = new Money(priceBreakdown.PremiumGst, currencyCode).ToString();
                this.StampDutyAct = new Money(priceBreakdown.StampDutyAct, currencyCode).ToString();
                this.StampDutyNsw = new Money(priceBreakdown.StampDutyNsw, currencyCode).ToString();
                this.StampDutyNt = new Money(priceBreakdown.StampDutyNt, currencyCode).ToString();
                this.StampDutyQld = new Money(priceBreakdown.StampDutyQld, currencyCode).ToString();
                this.StampDutySa = new Money(priceBreakdown.StampDutySa, currencyCode).ToString();
                this.StampDutyTas = new Money(priceBreakdown.StampDutyTas, currencyCode).ToString();
                this.StampDutyVic = new Money(priceBreakdown.StampDutyVic, currencyCode).ToString();
                this.StampDutyWa = new Money(priceBreakdown.StampDutyWa, currencyCode).ToString();
                this.StampDutyTotal = new Money(priceBreakdown.StampDutyTotal, currencyCode).ToString();
                this.TotalPremium = new Money(priceBreakdown.TotalPremium, currencyCode).ToString();
            }
        }
    }
}
