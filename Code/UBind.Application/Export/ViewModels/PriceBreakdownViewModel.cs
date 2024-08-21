// <copyright file="PriceBreakdownViewModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Export.ViewModels
{
    using Newtonsoft.Json;
    using NodaMoney;
    using UBind.Domain.JsonConverters;

    /// <summary>
    /// The model exposing price breakdown for an insurance quote.
    /// </summary>
    public class PriceBreakdownViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PriceBreakdownViewModel "/> class.
        /// </summary>
        /// <param name="priceBreakdown">The price Breakdown object.</param>
        public PriceBreakdownViewModel(Domain.ReadWriteModel.PriceBreakdown priceBreakdown)
        {
            this.CurrencyCode = priceBreakdown.CurrencyCode;
            this.BasePremium = new Money(priceBreakdown.BasePremium, this.CurrencyCode).ToString();
            this.ESL = new Money(priceBreakdown.Esl, this.CurrencyCode).ToString();
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
            this.Interest = new Money(priceBreakdown.InterestCharges, this.CurrencyCode).ToString();
            this.MerchantFees = new Money(priceBreakdown.MerchantFees, this.CurrencyCode).ToString();
            this.TransactionCosts = new Money(priceBreakdown.TransactionCosts, this.CurrencyCode).ToString();
            this.StampDutyTotal = new Money(priceBreakdown.StampDutyTotal, this.CurrencyCode).ToString();
            this.TotalPremium = new Money(priceBreakdown.TotalPremium, this.CurrencyCode).ToString();
            this.TotalPayable = new Money(priceBreakdown.TotalPayable, this.CurrencyCode).ToString();
            this.TotalGST = new Money(priceBreakdown.TotalGst, this.CurrencyCode).ToString();
        }

        /// <summary>
        /// Gets the currency code for the price breakdown, e.g. AUD, USD, PGK, GBP, PHP.
        /// </summary>
        [JsonProperty]
        public string CurrencyCode { get; private set; }

        /// <summary>
        /// Gets the base premium for all risks (excluding all taxes and fees).
        /// </summary>
        [JsonProperty]
        public string BasePremium { get; private set; }

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

        /// <summary>
        /// Gets the total merchant fees (i.e. credit card charges).
        /// </summary>
        [JsonProperty]
        public string MerchantFees { get; private set; }

        /// <summary>
        /// Gets the total transaction costs (e.g. from payment gateway provider).
        /// </summary>
        [JsonProperty]
        public string TransactionCosts { get; private set; }

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
