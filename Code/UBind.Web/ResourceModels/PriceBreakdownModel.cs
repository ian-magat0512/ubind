// <copyright file="PriceBreakdownModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using Newtonsoft.Json;
    using NodaMoney;
    using UBind.Domain.JsonConverters;
    using UBind.Domain.ReadWriteModel;

    /// <summary>
    /// The model exposing price breakdown for an insurance quote.
    /// </summary>
    public class PriceBreakdownModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PriceBreakdownModel"/> class.
        /// </summary>
        /// <param name="priceBreakdown">The price Breakdown object.</param>
        public PriceBreakdownModel(PriceBreakdown priceBreakdown)
        {
            this.CurrencyCode = priceBreakdown.CurrencyCode;
            this.BasePremium = new Money(priceBreakdown.BasePremium, this.CurrencyCode).ToString();
            this.TerrorismPremium = new Money(priceBreakdown.TerrorismPremium, this.CurrencyCode).ToString();
            this.ESL = new Money(priceBreakdown.Esl, this.CurrencyCode).ToString();
            this.EmergencyServicesLevyNSW = priceBreakdown.EmergencyServicesLevyNSW == 0m && priceBreakdown.EmergencyServicesLevyTAS == 0m
                ? new Money(priceBreakdown.Esl, this.CurrencyCode).ToString()
                : new Money(priceBreakdown.EmergencyServicesLevyNSW, this.CurrencyCode).ToString();
            this.EmergencyServicesLevyTAS = new Money(priceBreakdown.EmergencyServicesLevyTAS, this.CurrencyCode).ToString();
            this.PremiumGST = new Money(priceBreakdown.PremiumGst, this.CurrencyCode).ToString();
            this.StampDutyACT = new Money(priceBreakdown.StampDutyAct, this.CurrencyCode).ToString();
            this.StampDutyNSW = new Money(priceBreakdown.StampDutyNsw, this.CurrencyCode).ToString();
            this.StampDutyNT = new Money(priceBreakdown.StampDutyNt, this.CurrencyCode).ToString();
            this.StampDutyQLD = new Money(priceBreakdown.StampDutyQld, this.CurrencyCode).ToString();
            this.StampDutySA = new Money(priceBreakdown.StampDutySa, this.CurrencyCode).ToString();
            this.StampDutyTAS = new Money(priceBreakdown.StampDutyTas, this.CurrencyCode).ToString();
            this.StampDutyWA = new Money(priceBreakdown.StampDutyWa, this.CurrencyCode).ToString();
            this.StampDutyVIC = new Money(priceBreakdown.StampDutyVic, this.CurrencyCode).ToString();
            this.Commission = new Money(priceBreakdown.Commission, this.CurrencyCode).ToString();
            this.CommissionGST = new Money(priceBreakdown.CommissionGst, this.CurrencyCode).ToString();
            this.BrokerFee = new Money(priceBreakdown.BrokerFee, this.CurrencyCode).ToString();
            this.BrokerFeeGST = new Money(priceBreakdown.BrokerFeeGst, this.CurrencyCode).ToString();
            this.UnderwriterFee = new Money(priceBreakdown.UnderwriterFee, this.CurrencyCode).ToString();
            this.UnderwriterFeeGST = new Money(priceBreakdown.UnderwriterFeeGst, this.CurrencyCode).ToString();
            this.RoadsideAssistanceFee = new Money(priceBreakdown.RoadsideAssistanceFee, this.CurrencyCode).ToString();
            this.RoadsideAssistanceFeeGST = new Money(priceBreakdown.RoadsideAssistanceFeeGst, this.CurrencyCode).ToString();
            this.PolicyFee = new Money(priceBreakdown.PolicyFee, this.CurrencyCode).ToString();
            this.PolicyFeeGST = new Money(priceBreakdown.PolicyFeeGst, this.CurrencyCode).ToString();
            this.PartnerFee = new Money(priceBreakdown.PartnerFee, this.CurrencyCode).ToString();
            this.PartnerFeeGST = new Money(priceBreakdown.PartnerFeeGst, this.CurrencyCode).ToString();
            this.AdministrationFee = new Money(priceBreakdown.AdministrationFee, this.CurrencyCode).ToString();
            this.AdministrationFeeGST = new Money(priceBreakdown.AdministrationFeeGst, this.CurrencyCode).ToString();
            this.EstablishmentFee = new Money(priceBreakdown.EstablishmentFee, this.CurrencyCode).ToString();
            this.EstablishmentFeeGST = new Money(priceBreakdown.EstablishmentFeeGst, this.CurrencyCode).ToString();
            this.Interest = new Money(priceBreakdown.InterestCharges, this.CurrencyCode).ToString();
            this.InterestGST = new Money(priceBreakdown.InterestGst, this.CurrencyCode).ToString();
            this.MerchantFees = new Money(priceBreakdown.MerchantFees, this.CurrencyCode).ToString();
            this.MerchantFeesGST = new Money(priceBreakdown.MerchantFeesGst, this.CurrencyCode).ToString();
            this.TransactionCosts = new Money(priceBreakdown.TransactionCosts, this.CurrencyCode).ToString();
            this.TransactionCostsGST = new Money(priceBreakdown.TransactionCostsGst, this.CurrencyCode).ToString();
            this.StampDutyTotal = new Money(priceBreakdown.StampDutyTotal, this.CurrencyCode).ToString();
            this.TotalPremium = new Money(priceBreakdown.TotalPremium, this.CurrencyCode).ToString();
            this.TotalPayable = new Money(priceBreakdown.TotalPayable, this.CurrencyCode).ToString();
            this.TotalGST = new Money(priceBreakdown.TotalGst, this.CurrencyCode).ToString();
        }

        /// <summary>
        /// Gets the currency code for the price breakdown, e.g. AUD, USD, PGK, GBP, PHP.
        /// </summary>
        [JsonProperty]
        public string CurrencyCode { get; private set; } = PriceBreakdown.DefaultCurrencyCode;

        /// <summary>
        /// Gets the base premium for all risks (excluding all taxes and fees).
        /// </summary>
        [JsonProperty]
        public string BasePremium { get; private set; }

        /// <summary>
        /// Gets the terrorism premium.
        /// </summary>
        [JsonProperty]
        public string TerrorismPremium { get; private set; }

        [JsonProperty("emergencyServicesLevyNSW")]
        public string EmergencyServicesLevyNSW { get; private set; }

        [JsonProperty("emergencyServicesLevyTAS")]
        public string EmergencyServicesLevyTAS { get; private set; }

        /// <summary>
        /// Gets the total Emergency Services Levy (ESL).
        /// </summary>
        [JsonProperty("ESL")]
        public string ESL { get; private set; }

        /// <summary>
        /// Gets the GST payable on the base premium and ESL.
        /// </summary>
        [JsonProperty]
        public string PremiumGST { get; private set; }

        /// <summary>
        /// Gets the Stamp Duty payable to the ACT.
        /// </summary>
        [JsonProperty]
        public string StampDutyACT { get; private set; }

        /// <summary>
        /// Gets the Stamp Duty payable to NSW.
        /// </summary>
        [JsonProperty]
        public string StampDutyNSW { get; private set; }

        /// <summary>
        /// Gets the Stamp Duty payable to the NT.
        /// </summary>
        [JsonProperty]
        public string StampDutyNT { get; private set; }

        /// <summary>
        /// Gets the Stamp Duty payable to QLD.
        /// </summary>
        [JsonProperty]
        public string StampDutyQLD { get; private set; }

        /// <summary>
        /// Gets the Stamp Duty payable to SA.
        /// </summary>
        [JsonProperty]
        public string StampDutySA { get; private set; }

        /// <summary>
        /// Gets the Stamp Duty payable to TAS.
        /// </summary>
        [JsonProperty]
        public string StampDutyTAS { get; private set; }

        /// <summary>
        /// Gets the Stamp Duty payable to VIC.
        /// </summary>
        [JsonProperty]
        public string StampDutyVIC { get; private set; }

        /// <summary>
        /// Gets the Stamp Duty payable to WA.
        /// </summary>
        [JsonProperty]
        public string StampDutyWA { get; private set; }

        /// <summary>
        /// Gets the commisssion payable to the broker (already incuded in base premium).
        /// </summary>
        [JsonProperty]
        public string Commission { get; private set; }

        /// <summary>
        /// Gets the GST on the commisssion payable to the broker (already included in premium GST).
        /// </summary>
        [JsonProperty]
        public string CommissionGST { get; private set; }

        /// <summary>
        /// Gets the broker fee.
        /// </summary>
        [JsonProperty]
        public string BrokerFee { get; private set; }

        /// <summary>
        /// Gets the GST payable on the broker fee.
        /// </summary>
        [JsonProperty]
        public string BrokerFeeGST { get; private set; }

        /// <summary>
        /// Gets the underwriter fee.
        /// </summary>
        [JsonProperty]
        public string UnderwriterFee { get; private set; }

        /// <summary>
        /// Gets the GST payable on the underwriter fee.
        /// </summary>
        [JsonProperty]
        public string UnderwriterFeeGST { get; private set; }

        [JsonProperty]
        public string RoadsideAssistanceFee { get; private set; }

        [JsonProperty]
        public string RoadsideAssistanceFeeGST { get; private set; }

        [JsonProperty]
        public string PolicyFee { get; private set; }

        [JsonProperty]
        public string PolicyFeeGST { get; private set; }

        [JsonProperty]
        public string PartnerFee { get; private set; }

        [JsonProperty]
        public string PartnerFeeGST { get; private set; }

        [JsonProperty]
        public string AdministrationFee { get; private set; }

        [JsonProperty]
        public string AdministrationFeeGST { get; private set; }

        [JsonProperty]
        public string EstablishmentFee { get; private set; }

        [JsonProperty]
        public string EstablishmentFeeGST { get; private set; }

        /// <summary>
        /// Gets the total GST payable.
        /// </summary>
        /// <remarks>Note that commission GST is already included in premium GST.</remarks>
        [JsonProperty]
        public string TotalGST { get; private set; }

        /// <summary>
        /// Gets the total interest charges where applicable.
        /// </summary>
        [JsonProperty]
        public string Interest { get; private set; }

        [JsonProperty]
        public string InterestGST { get; private set; }

        /// <summary>
        /// Gets the total merchant fees (i.e. credit card charges).
        /// </summary>
        [JsonProperty]
        public string MerchantFees { get; private set; }

        [JsonProperty]
        public string MerchantFeesGST { get; private set; }

        /// <summary>
        /// Gets the total transaction costs (e.g. from payment gateway provider).
        /// </summary>
        [JsonProperty]
        public string TransactionCosts { get; private set; }

        [JsonProperty]
        public string TransactionCostsGST { get; private set; }

        /// <summary>
        /// Gets the total Stamp Duty.
        /// </summary>
        [JsonProperty]
        public string StampDutyTotal { get; private set; }

        /// <summary>
        /// Gets the total Premium.
        /// </summary>
        [JsonProperty]
        [GetOnlyJsonProperty]
        public string TotalPremium { get; private set; }

        /// <summary>
        /// Gets the total payable.
        /// </summary>
        [JsonProperty]
        public string TotalPayable { get; private set; }
    }
}
