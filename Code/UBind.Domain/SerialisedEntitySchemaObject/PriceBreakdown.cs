// <copyright file="PriceBreakdown.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.SerialisedEntitySchemaObject
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// This class is needed because we need to generate json representation of price breakdown object that conforms with serialized-entity-schema.json file.
    /// </summary>
    public class PriceBreakdown : PremiumBreakdown
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PriceBreakdown"/> class.
        /// </summary>
        /// <param name="priceObject">The price object.</param>
        public PriceBreakdown(JObject priceObject)
            : base(priceObject)
        {
            if (priceObject != null && priceObject.Type != JTokenType.Null)
            {
                var properties = new Dictionary<string, string>(priceObject.ToObject<IDictionary<string, string>>(), StringComparer.CurrentCultureIgnoreCase);
                this.Commission = this.GetPropertyValue(properties, "commission");
                this.CommissionGst = this.GetPropertyValue(properties, "commissionGst");
                this.BrokerFee = this.GetPropertyValue(properties, "brokerFee");
                this.BrokerFeeGst = this.GetPropertyValue(properties, "brokerFeeGst");
                this.UnderwriterFee = this.GetPropertyValue(properties, "underwriterFee");
                this.UnderwriterFeeGst = this.GetPropertyValue(properties, "underwriterFeeGst");
                this.RoadsideAssistanceFee = this.GetPropertyValue(properties, "roadsideAssistanceFee");
                this.RoadsideAssistanceFeeGst = this.GetPropertyValue(properties, "roadsideAssistanceFeeGst");
                this.PolicyFee = this.GetPropertyValue(properties, "policyFee");
                this.PolicyFeeGst = this.GetPropertyValue(properties, "policyFeeGst");
                this.PartnerFee = this.GetPropertyValue(properties, "partnerFee");
                this.PartnerFeeGst = this.GetPropertyValue(properties, "partnerFeeGst");
                this.AdministrationFee = this.GetPropertyValue(properties, "administrationFee");
                this.AdministrationFeeGst = this.GetPropertyValue(properties, "administrationFeeGst");
                this.EstablishmentFee = this.GetPropertyValue(properties, "establishmentFee");
                this.EstablishmentFeeGst = this.GetPropertyValue(properties, "establishmentFeeGst");
                this.InterestCharges = this.GetPropertyValue(properties, "interestCharges");
                this.InterestChargesGst = this.GetPropertyValue(properties, "interestGst");
                this.MerchantFees = this.GetPropertyValue(properties, "merchantFees");
                this.MerchantFeesGst = this.GetPropertyValue(properties, "merchantFeesGst");
                this.TransactionCosts = this.GetPropertyValue(properties, "transactionCosts");
                this.TransactionCostsGst = this.GetPropertyValue(properties, "transactionCostsGst");
                this.TotalGst = this.GetPropertyValue(properties, "totalGst") ?? 0.00;
                this.TotalPayable = this.GetPropertyValue(properties, "totalPayable") ?? 0.00;
            }
        }

        /// <summary>
        /// Gets or sets the commission.
        /// </summary>
        [JsonProperty(PropertyName = "commission", Order = 17, NullValueHandling = NullValueHandling.Ignore)]
        public double? Commission { get; set; }

        /// <summary>
        /// Gets or sets the commission GST.
        /// </summary>
        [JsonProperty(PropertyName = "commissionGst", Order = 18, NullValueHandling = NullValueHandling.Ignore)]
        public double? CommissionGst { get; set; }

        /// <summary>
        /// Gets or sets the broker fee.
        /// </summary>
        [JsonProperty(PropertyName = "brokerFee", Order = 19, NullValueHandling = NullValueHandling.Ignore)]
        public double? BrokerFee { get; set; }

        /// <summary>
        /// Gets or sets the broker fee GST.
        /// </summary>
        [JsonProperty(PropertyName = "brokerFeeGst", Order = 20, NullValueHandling = NullValueHandling.Ignore)]
        public double? BrokerFeeGst { get; set; }

        /// <summary>
        /// Gets or sets the under writer fee.
        /// </summary>
        [JsonProperty(PropertyName = "underwriterFee", Order = 21, NullValueHandling = NullValueHandling.Ignore)]
        public double? UnderwriterFee { get; set; }

        /// <summary>
        /// Gets or sets the under writer fee GST.
        /// </summary>
        [JsonProperty(PropertyName = "underwriterFeeGst", Order = 22, NullValueHandling = NullValueHandling.Ignore)]
        public double? UnderwriterFeeGst { get; set; }

        /// <summary>
        /// Gets or sets the roadside assistance fee.
        /// </summary>
        [JsonProperty(PropertyName = "roadsideAssistanceFee", Order = 23, NullValueHandling = NullValueHandling.Ignore)]
        public double? RoadsideAssistanceFee { get; set; }

        /// <summary>
        /// Gets or sets the roadside assistance fee GST.
        /// </summary>
        [JsonProperty(PropertyName = "roadsideAssistanceFeeGst", Order = 24, NullValueHandling = NullValueHandling.Ignore)]
        public double? RoadsideAssistanceFeeGst { get; set; }

        /// <summary>
        /// Gets or sets the policy fee.
        /// </summary>
        [JsonProperty(PropertyName = "policyFee", Order = 25, NullValueHandling = NullValueHandling.Ignore)]
        public double? PolicyFee { get; set; }

        /// <summary>
        /// Gets or sets the policy fee GST.
        /// </summary>
        [JsonProperty(PropertyName = "policyFeeGst", Order = 26, NullValueHandling = NullValueHandling.Ignore)]
        public double? PolicyFeeGst { get; set; }

        /// <summary>
        /// Gets or sets the partner fee.
        /// </summary>
        [JsonProperty(PropertyName = "partnerFee", Order = 27, NullValueHandling = NullValueHandling.Ignore)]
        public double? PartnerFee { get; set; }

        /// <summary>
        /// Gets or sets the partner fee GST.
        /// </summary>
        [JsonProperty(PropertyName = "partnerFeeGst", Order = 28, NullValueHandling = NullValueHandling.Ignore)]
        public double? PartnerFeeGst { get; set; }

        /// <summary>
        /// Gets or sets the administration fee.
        /// </summary>
        [JsonProperty(PropertyName = "administrationFee", Order = 29, NullValueHandling = NullValueHandling.Ignore)]
        public double? AdministrationFee { get; set; }

        /// <summary>
        /// Gets or sets the administration fee GST.
        /// </summary>
        [JsonProperty(PropertyName = "administrationFeeGst", Order = 30, NullValueHandling = NullValueHandling.Ignore)]
        public double? AdministrationFeeGst { get; set; }

        /// <summary>
        /// Gets or sets the administration fee.
        /// </summary>
        [JsonProperty(PropertyName = "establishmentFee", Order = 31, NullValueHandling = NullValueHandling.Ignore)]
        public double? EstablishmentFee { get; set; }

        /// <summary>
        /// Gets or sets the administration fee GST.
        /// </summary>
        [JsonProperty(PropertyName = "establishmentFeeGst", Order = 32, NullValueHandling = NullValueHandling.Ignore)]
        public double? EstablishmentFeeGst { get; set; }

        /// <summary>
        /// Gets or sets the interest charges.
        /// </summary>
        [JsonProperty(PropertyName = "interestCharges", Order = 33, NullValueHandling = NullValueHandling.Ignore)]
        public double? InterestCharges { get; set; }

        /// <summary>
        /// Gets or sets the interest charges.
        /// </summary>
        [JsonProperty(PropertyName = "interestGst", Order = 34, NullValueHandling = NullValueHandling.Ignore)]
        public double? InterestChargesGst { get; set; }

        /// <summary>
        /// Gets or sets the merchant fees.
        /// </summary>
        [JsonProperty(PropertyName = "merchantFees", Order = 35, NullValueHandling = NullValueHandling.Ignore)]
        public double? MerchantFees { get; set; }

        /// <summary>
        /// Gets or sets the merchant fees GST.
        /// </summary>
        [JsonProperty(PropertyName = "merchantFeesGst", Order = 36, NullValueHandling = NullValueHandling.Ignore)]
        public double? MerchantFeesGst { get; set; }

        /// <summary>
        /// Gets or sets the transaction costs.
        /// </summary>
        [JsonProperty(PropertyName = "transactionCosts", Order = 37, NullValueHandling = NullValueHandling.Ignore)]
        public double? TransactionCosts { get; set; }

        /// <summary>
        /// Gets or sets the transaction costs GST.
        /// </summary>
        [JsonProperty(PropertyName = "transactionCostsGst", Order = 38, NullValueHandling = NullValueHandling.Ignore)]
        public double? TransactionCostsGst { get; set; }

        /// <summary>
        /// Gets or sets the Total GST.
        /// </summary>
        [JsonProperty(PropertyName = "totalGst", Order = 39)]
        [Required]
        public double TotalGst { get; set; }

        /// <summary>
        /// Gets or sets the total payable.
        /// </summary>
        [JsonProperty(PropertyName = "totalPayable", Order = 40)]
        [Required]
        public double TotalPayable { get; set; }
    }
}
