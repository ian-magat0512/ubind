// <copyright file="PremiumResult.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using Newtonsoft.Json;
    using UBind.Domain.ReadWriteModel;

    /// <summary>
    /// Resource model for presenting the premium data in the calculatin result.
    /// </summary>
    public class PremiumResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PremiumResult"/> class.
        /// </summary>
        /// <param name="priceBreakdown">The price breakdown to use.</param>
        public PremiumResult(PriceBreakdown priceBreakdown)
        {
            this.BasePremium = priceBreakdown.BasePremium;
            this.TerrorismPremium = priceBreakdown.TerrorismPremium;
            this.Esl = priceBreakdown.Esl;
            this.EslNsw = priceBreakdown.EmergencyServicesLevyNSW;
            this.EslTas = priceBreakdown.EmergencyServicesLevyTAS;
            this.PremiumGst = priceBreakdown.PremiumGst;
            this.StampDuty = priceBreakdown.StampDuty;
            this.StampDutyAct = priceBreakdown.StampDutyAct;
            this.StampDutyNsw = priceBreakdown.StampDutyNsw;
            this.StampDutyNt = priceBreakdown.StampDutyNt;
            this.StampDutyQld = priceBreakdown.StampDutyQld;
            this.StampDutySa = priceBreakdown.StampDutySa;
            this.StampDutyTas = priceBreakdown.StampDutyTas;
            this.StampDutyVic = priceBreakdown.StampDutyVic;
            this.StampDutyWa = priceBreakdown.StampDutyWa;
            this.Commission = priceBreakdown.Commission;
            this.CommissionGst = priceBreakdown.CommissionGst;
            this.BrokerFee = priceBreakdown.BrokerFee;
            this.BrokerFeeGst = priceBreakdown.BrokerFeeGst;
            this.UnderwriterFee = priceBreakdown.UnderwriterFee;
            this.UnderwriterFeeGst = priceBreakdown.UnderwriterFeeGst;
            this.RoadsideAssistanceFee = priceBreakdown.RoadsideAssistanceFee;
            this.RoadsideAssistanceFeeGst = priceBreakdown.RoadsideAssistanceFeeGst;
            this.PolicyFee = priceBreakdown.PolicyFee;
            this.PolicyFeeGst = priceBreakdown.PolicyFeeGst;
            this.PartnerFee = priceBreakdown.PartnerFee;
            this.PartnerFeeGst = priceBreakdown.PartnerFeeGst;
            this.AdministrationFee = priceBreakdown.AdministrationFee;
            this.AdministrationFeeGst = priceBreakdown.AdministrationFeeGst;
            this.EstablishmentFee = priceBreakdown.EstablishmentFee;
            this.EstablishmentFeeGst = priceBreakdown.EstablishmentFeeGst;
            this.Interest = priceBreakdown.InterestCharges;
            this.InterestGst = priceBreakdown.InterestGst;
            this.MerchantFees = priceBreakdown.MerchantFees;
            this.MerchantFeesGst = priceBreakdown.MerchantFeesGst;
            this.TransactionCosts = priceBreakdown.TransactionCosts;
            this.TransactionCostsGst = priceBreakdown.TransactionCostsGst;
            this.ServiceFees = priceBreakdown.ServiceFees;
            this.TotalGst = priceBreakdown.TotalGst;
            this.StampDutyTotal = priceBreakdown.StampDutyTotal;
            this.TotalPremium = priceBreakdown.TotalPremium;
            this.TotalPayable = priceBreakdown.TotalPayable;
            this.CurrencyCode = priceBreakdown.CurrencyCode;
        }

        /// <summary>
        /// Gets the currency code for the price breakdown, e.g. AUD, USD, PGK, GBP, PHP.
        /// </summary>
        [JsonProperty]
        public string CurrencyCode { get; private set; } = PriceBreakdown.DefaultCurrencyCode;

        /// <summary>
        /// Gets the base premium.
        /// </summary>
        [JsonProperty]
        public decimal BasePremium { get; private set; }

        [JsonProperty]
        public decimal TerrorismPremium { get; private set; }

        /// <summary>
        /// Gets or sets the ESL value in the calculation result.
        /// </summary>
        [JsonProperty]
        public decimal Esl { get; private set; }

        [JsonProperty]
        public decimal EslNsw { get; private set; }

        [JsonProperty]
        public decimal EslTas { get; private set; }

        /// <summary>
        /// Gets or sets the GST value in the calculation result.
        /// </summary>
        [JsonProperty]
        public decimal PremiumGst { get; set; }

        /// <summary>
        /// Gets or sets the stamp duty value in the calculation result.
        /// </summary>
        /// <remarks>Only used old-format breakdowns where stamp duty is not broken down per state.</remarks>
        [JsonProperty]
        public decimal StampDuty { get; set; }

        /// <summary>
        /// Gets the Stamp Duty ACT.
        /// </summary>
        [JsonProperty]
        public decimal StampDutyAct { get; private set; }

        /// <summary>
        /// Gets the Stamp Duty NSW.
        /// </summary>
        [JsonProperty]
        public decimal StampDutyNsw { get; private set; }

        /// <summary>
        /// Gets the Stamp Duty NT.
        /// </summary>
        [JsonProperty]
        public decimal StampDutyNt { get; private set; }

        /// <summary>
        /// Gets the Stamp Duty QLD.
        /// </summary>
        [JsonProperty]
        public decimal StampDutyQld { get; private set; }

        /// <summary>
        /// Gets the Stamp Duty SA.
        /// </summary>
        [JsonProperty]
        public decimal StampDutySa { get; private set; }

        /// <summary>
        /// Gets the Stamp Duty TAS.
        /// </summary>
        [JsonProperty]
        public decimal StampDutyTas { get; private set; }

        /// <summary>
        /// Gets the Stamp Duty WA.
        /// </summary>
        [JsonProperty]
        public decimal StampDutyWa { get; private set; }

        /// <summary>
        /// Gets the Stamp Duty VIC.
        /// </summary>
        [JsonProperty]
        public decimal StampDutyVic { get; private set; }

        /// <summary>
        /// Gets the commisssion payable to the broker (already incuded in base premium).
        /// </summary>
        [JsonProperty]
        public decimal Commission { get; private set; }

        /// <summary>
        /// Gets the GST on the commisssion payable to the broker (already included in premium GST).
        /// </summary>
        [JsonProperty]
        public decimal CommissionGst { get; private set; }

        /// <summary>
        /// Gets the Broker Fee.
        /// </summary>
        [JsonProperty]
        public decimal BrokerFee { get; private set; }

        /// <summary>
        /// Gets the Broker Fee GST.
        /// </summary>
        [JsonProperty]
        public decimal BrokerFeeGst { get; private set; }

        /// <summary>
        /// Gets the Underwriter Fee.
        /// </summary>
        [JsonProperty]
        public decimal UnderwriterFee { get; private set; }

        /// <summary>
        /// Gets the Underwriter Fee GST.
        /// </summary>
        [JsonProperty]
        public decimal UnderwriterFeeGst { get; private set; }

        [JsonProperty]
        public decimal RoadsideAssistanceFee { get; private set; }

        [JsonProperty]
        public decimal RoadsideAssistanceFeeGst { get; private set; }

        [JsonProperty]
        public decimal PolicyFee { get; private set; }

        [JsonProperty]
        public decimal PolicyFeeGst { get; private set; }

        [JsonProperty]
        public decimal PartnerFee { get; private set; }

        [JsonProperty]
        public decimal PartnerFeeGst { get; private set; }

        [JsonProperty]
        public decimal AdministrationFee { get; private set; }

        [JsonProperty]
        public decimal AdministrationFeeGst { get; private set; }

        [JsonProperty]
        public decimal EstablishmentFee { get; private set; }

        [JsonProperty]
        public decimal EstablishmentFeeGst { get; private set; }

        /// <summary>
        /// Gets the total interest.
        /// </summary>
        [JsonProperty]
        public decimal Interest { get; private set; }

        [JsonProperty]
        public decimal InterestGst { get; private set; }

        /// <summary>
        /// Gets the total merchant fees.
        /// </summary>
        [JsonProperty]
        public decimal MerchantFees { get; private set; }

        [JsonProperty]
        public decimal MerchantFeesGst { get; private set; }

        /// <summary>
        /// Gets the total transaction costs.
        /// </summary>
        [JsonProperty]
        public decimal TransactionCosts { get; private set; }

        [JsonProperty]
        public decimal TransactionCostsGst { get; private set; }

        /// <summary>
        /// Gets or sets the service fees in the calculation result.
        /// </summary>
        /// <remarks>Only used in old-format breakdowns.</remarks>
        [JsonProperty]
        public decimal ServiceFees { get; set; }

        /// <summary>
        /// Gets the total Stamp Duty.
        /// </summary>
        [JsonProperty]
        public decimal StampDutyTotal { get; private set; }

        /// <summary>
        /// Gets the total Premium.
        /// </summary>
        [JsonProperty]
        public decimal TotalPremium { get; private set; }

        /// <summary>
        /// Gets the Total GST.
        /// </summary>
        [JsonProperty]
        public decimal TotalGst { get; private set; }

        /// <summary>
        /// Gets the Total Payable.
        /// </summary>
        [JsonProperty]
        public decimal TotalPayable { get; private set; }
    }
}
