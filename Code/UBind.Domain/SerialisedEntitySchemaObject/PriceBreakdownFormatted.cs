// <copyright file="PriceBreakdownFormatted.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.SerialisedEntitySchemaObject
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using NodaMoney;

    public class PriceBreakdownFormatted : PremiumBreakdownFormatted
    {
        public PriceBreakdownFormatted(JObject payableComponent)
            : base(payableComponent)
        {
            this.FormatPropertiesToMoneyString(payableComponent);
        }

        public PriceBreakdownFormatted(JObject payableComponent, bool skipWhenTotalPayableIsZero)
           : base(payableComponent, skipWhenTotalPayableIsZero)
        {
            this.FormatPropertiesToMoneyString(payableComponent, skipWhenTotalPayableIsZero);
        }

        public PriceBreakdownFormatted(JObject priceObject, string currencyCode)
            : base(priceObject, currencyCode)
        {
            this.Commission = GetValueOrDefault(priceObject.GetValue("commission"), currencyCode);
            this.CommissionGst = GetValueOrDefault(priceObject.GetValue("commissionGST"), currencyCode);
            this.BrokerFee = GetValueOrDefault(priceObject.GetValue("brokerFee"), currencyCode);
            this.BrokerFeeGst = GetValueOrDefault(priceObject.GetValue("brokerFeeGST"), currencyCode);
            this.UnderwriterFee = GetValueOrDefault(priceObject.GetValue("underwriterFee"), currencyCode);
            this.UnderwriterFeeGst = GetValueOrDefault(priceObject.GetValue("underwriterFeeGST"), currencyCode);
            this.RoadsideAssistanceFee = GetValueOrDefault(priceObject.GetValue("roadsideAssistanceFee"), currencyCode);
            this.RoadsideAssistanceFeeGst = GetValueOrDefault(priceObject.GetValue("roadsideAssistanceFeeGST"), currencyCode);
            this.PolicyFee = GetValueOrDefault(priceObject.GetValue("policyFee"), currencyCode);
            this.PolicyFeeGst = GetValueOrDefault(priceObject.GetValue("policyFeeGST"), currencyCode);
            this.PartnerFee = GetValueOrDefault(priceObject.GetValue("partnerFee"), currencyCode);
            this.PartnerFeeGst = GetValueOrDefault(priceObject.GetValue("partnerFeeGST"), currencyCode);
            this.AdministrationFee = GetValueOrDefault(priceObject.GetValue("administrationFee"), currencyCode);
            this.AdministrationFeeGst = GetValueOrDefault(priceObject.GetValue("administrationFeeGST"), currencyCode);
            this.EstablishmentFee = GetValueOrDefault(priceObject.GetValue("establishmentFee"), currencyCode);
            this.EstablishmentFeeGst = GetValueOrDefault(priceObject.GetValue("establishmentFeeGST"), currencyCode);
            this.InterestCharges = GetValueOrDefault(priceObject.GetValue("interestCharges"), currencyCode);
            this.InterestGst = GetValueOrDefault(priceObject.GetValue("interestGST"), currencyCode);
            this.MerchantFees = GetValueOrDefault(priceObject.GetValue("merchantFees"), currencyCode);
            this.MerchantFeesGst = GetValueOrDefault(priceObject.GetValue("merchantFeesGST"), currencyCode);
            this.TransactionCosts = GetValueOrDefault(priceObject.GetValue("transactionCosts"), currencyCode);
            this.TransactionCostsGst = GetValueOrDefault(priceObject.GetValue("transactionCostsGST"), currencyCode);
            this.TotalGst = GetValueOrDefault(priceObject.GetValue("totalGST"), currencyCode);
            this.TotalPayable = GetValueOrDefault(priceObject.GetValue("totalPayable"), currencyCode);
        }

        /// <summary>
        /// Gets or sets the commission.
        /// </summary>
        [JsonProperty(PropertyName = "commission", Order = 17, NullValueHandling = NullValueHandling.Ignore)]
        public string Commission { get; set; }

        /// <summary>
        /// Gets or sets the commission GST.
        /// </summary>
        [JsonProperty(PropertyName = "commissionGst", Order = 18, NullValueHandling = NullValueHandling.Ignore)]
        public string CommissionGst { get; set; }

        /// <summary>
        /// Gets or sets the broker fee.
        /// </summary>
        [JsonProperty(PropertyName = "brokerFee", Order = 19, NullValueHandling = NullValueHandling.Ignore)]
        public string BrokerFee { get; set; }

        /// <summary>
        /// Gets or sets the broker fee GST.
        /// </summary>
        [JsonProperty(PropertyName = "brokerFeeGst", Order = 20, NullValueHandling = NullValueHandling.Ignore)]
        public string BrokerFeeGst { get; set; }

        /// <summary>
        /// Gets or sets the under writer fee.
        /// </summary>
        [JsonProperty(PropertyName = "underwriterFee", Order = 21, NullValueHandling = NullValueHandling.Ignore)]
        public string UnderwriterFee { get; set; }

        /// <summary>
        /// Gets or sets the under writer fee GST.
        /// </summary>
        [JsonProperty(PropertyName = "underwriterFeeGst", Order = 22, NullValueHandling = NullValueHandling.Ignore)]
        public string UnderwriterFeeGst { get; set; }

        [JsonProperty("roadsideAssistanceFee", Order = 23, NullValueHandling = NullValueHandling.Ignore)]
        public string RoadsideAssistanceFee { get; set; }

        [JsonProperty("roadsideAssistanceFeeGst", Order = 24, NullValueHandling = NullValueHandling.Ignore)]
        public string RoadsideAssistanceFeeGst { get; set; }

        [JsonProperty("policyFee", Order = 25, NullValueHandling = NullValueHandling.Ignore)]
        public string PolicyFee { get; set; }

        [JsonProperty("policyFeeGst", Order = 26, NullValueHandling = NullValueHandling.Ignore)]
        public string PolicyFeeGst { get; set; }

        [JsonProperty("partnerFee", Order = 27, NullValueHandling = NullValueHandling.Ignore)]
        public string PartnerFee { get; set; }

        [JsonProperty("partnerFeeGst", Order = 28, NullValueHandling = NullValueHandling.Ignore)]
        public string PartnerFeeGst { get; set; }

        [JsonProperty("administrationFee", Order = 29, NullValueHandling = NullValueHandling.Ignore)]
        public string AdministrationFee { get; set; }

        [JsonProperty("administrationFeeGst", Order = 30, NullValueHandling = NullValueHandling.Ignore)]
        public string AdministrationFeeGst { get; set; }

        [JsonProperty("establishmentFee", Order = 31, NullValueHandling = NullValueHandling.Ignore)]
        public string EstablishmentFee { get; set; }

        [JsonProperty("establishmentFeeGst", Order = 32, NullValueHandling = NullValueHandling.Ignore)]
        public string EstablishmentFeeGst { get; set; }

        /// <summary>
        /// Gets or sets the interest charges.
        /// </summary>
        [JsonProperty(PropertyName = "interestCharges", Order = 33, NullValueHandling = NullValueHandling.Ignore)]
        public string InterestCharges { get; set; }

        [JsonProperty("interestGst", Order = 34, NullValueHandling = NullValueHandling.Ignore)]
        public string InterestGst { get; set; }

        /// <summary>
        /// Gets or sets the merchant fees.
        /// </summary>
        [JsonProperty(PropertyName = "merchantFees", Order = 35, NullValueHandling = NullValueHandling.Ignore)]
        public string MerchantFees { get; set; }

        [JsonProperty("merchantFeesGst", Order = 36, NullValueHandling = NullValueHandling.Ignore)]
        public string MerchantFeesGst { get; set; }

        /// <summary>
        /// Gets or sets the transaction costs.
        /// </summary>
        [JsonProperty(PropertyName = "transactionCosts", Order = 37, NullValueHandling = NullValueHandling.Ignore)]
        public string TransactionCosts { get; set; }

        [JsonProperty("transactionCostsGst", Order = 38, NullValueHandling = NullValueHandling.Ignore)]
        public string TransactionCostsGst { get; set; }

        /// <summary>
        /// Gets or sets the Total GST.
        /// </summary>
        [JsonProperty(PropertyName = "totalGst", Order = 39)]
        public string TotalGst { get; set; }

        /// <summary>
        /// Gets or sets the total payable.
        /// </summary>
        [JsonProperty(PropertyName = "totalPayable", Order = 40)]
        public string TotalPayable { get; set; }

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
                this.Commission = new Money(priceBreakdown.Commission, currencyCode).ToString();
                this.CommissionGst = new Money(priceBreakdown.CommissionGst, currencyCode).ToString();
                this.BrokerFee = new Money(priceBreakdown.BrokerFee, currencyCode).ToString();
                this.BrokerFeeGst = new Money(priceBreakdown.BrokerFeeGst, currencyCode).ToString();
                this.UnderwriterFee = new Money(priceBreakdown.UnderwriterFee, currencyCode).ToString();
                this.UnderwriterFeeGst = new Money(priceBreakdown.UnderwriterFeeGst, currencyCode).ToString();
                this.RoadsideAssistanceFee = new Money(priceBreakdown.RoadsideAssistanceFee, currencyCode).ToString();
                this.RoadsideAssistanceFeeGst = new Money(priceBreakdown.RoadsideAssistanceFeeGst, currencyCode).ToString();
                this.PolicyFee = new Money(priceBreakdown.PolicyFee, currencyCode).ToString();
                this.PolicyFeeGst = new Money(priceBreakdown.PolicyFeeGst, currencyCode).ToString();
                this.PartnerFee = new Money(priceBreakdown.PartnerFee, currencyCode).ToString();
                this.PartnerFeeGst = new Money(priceBreakdown.PartnerFeeGst, currencyCode).ToString();
                this.AdministrationFee = new Money(priceBreakdown.AdministrationFee, currencyCode).ToString();
                this.AdministrationFeeGst = new Money(priceBreakdown.AdministrationFeeGst, currencyCode).ToString();
                this.EstablishmentFee = new Money(priceBreakdown.EstablishmentFee, currencyCode).ToString();
                this.EstablishmentFeeGst = new Money(priceBreakdown.EstablishmentFeeGst, currencyCode).ToString();
                this.InterestCharges = new Money(priceBreakdown.InterestCharges, currencyCode).ToString();
                this.InterestGst = new Money(priceBreakdown.InterestGst, currencyCode).ToString();
                this.MerchantFees = new Money(priceBreakdown.MerchantFees, currencyCode).ToString();
                this.MerchantFeesGst = new Money(priceBreakdown.MerchantFeesGst, currencyCode).ToString();
                this.TransactionCosts = new Money(priceBreakdown.TransactionCosts, currencyCode).ToString();
                this.TransactionCostsGst = new Money(priceBreakdown.TransactionCostsGst, currencyCode).ToString();
                this.TotalGst = new Money(priceBreakdown.TotalGst, currencyCode).ToString();
                this.TotalPayable = new Money(priceBreakdown.TotalPayable, currencyCode).ToString();
            }
        }
    }
}
