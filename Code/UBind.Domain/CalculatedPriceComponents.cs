// <copyright file="CalculatedPriceComponents.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using System;
    using Newtonsoft.Json;
    using UBind.Domain.JsonConverters;
    using UBind.Domain.ReadWriteModel;

    /// <summary>
    /// Total premium information from calculation result.
    /// </summary>
    public class CalculatedPriceComponents
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CalculatedPriceComponents"/> class.
        /// </summary>
        /// <param name="basePremium">The base premium.</param>
        /// <param name="esl">EmergencyServicesLevy.</param>
        /// <param name="premiumGst">GST.</param>
        /// <param name="stampDutyAct">Stamp duty ACT.</param>
        /// <param name="stampDutyNsw">Stamp duty NSW.</param>
        /// <param name="stampDutyNt">Stamp duty NT.</param>
        /// <param name="stampDutyQld">Stamp duty QLD.</param>
        /// <param name="stampDutySa">Stamp duty SA.</param>
        /// <param name="stampDutyTas">Stamp duty TAS.</param>
        /// <param name="stampDutyWa">Stamp duty WA.</param>
        /// <param name="stampDutyVic">Stamp duty VIC.</param>
        /// <param name="commission">Commission.</param>
        /// <param name="commissionGst">Commission GST.</param>
        /// <param name="brokerFee">Broker Fee.</param>
        /// <param name="brokerFeeGst">Broker Fee Gst.</param>
        /// <param name="underwriterFee">Under Writer Fee.</param>
        /// <param name="underwriterFeeGst">Underwriter Fee.</param>
        /// <param name="interest">Interest.</param>
        /// <param name="merchantFees">Merchant Fees.</param>
        /// <param name="transactionCosts">TransactionCost.</param>
        /// <param name="stampDutyTotal">Total stamp duty.</param>
        /// <param name="totalPremium">Total premium.</param>
        /// <param name="totalGst">Total GST.</param>
        /// <param name="totalPayable">Total Payable.</param>
        /// <param name="currencyCode">Currency Code.</param>
        public CalculatedPriceComponents(
            decimal basePremium,
            decimal terrorismPremium,
            decimal esl,
            decimal eslNsw,
            decimal eslTas,
            decimal premiumGst,
            decimal stampDutyAct,
            decimal stampDutyNsw,
            decimal stampDutyNt,
            decimal stampDutyQld,
            decimal stampDutySa,
            decimal stampDutyTas,
            decimal stampDutyWa,
            decimal stampDutyVic,
            decimal commission,
            decimal commissionGst,
            decimal brokerFee,
            decimal brokerFeeGst,
            decimal underwriterFee,
            decimal underwriterFeeGst,
            decimal roadsideAssistanceFee,
            decimal roadsideAssistanceFeeGst,
            decimal policyFee,
            decimal policyFeeGst,
            decimal partnerFee,
            decimal partnerFeeGst,
            decimal administrationFee,
            decimal administrationFeeGst,
            decimal establishmentFee,
            decimal establishmentFeeGst,
            decimal interest,
            decimal interestGst,
            decimal merchantFees,
            decimal merchantFeesGst,
            decimal transactionCosts,
            decimal transactionCostsGst,
            decimal stampDutyTotal,
            decimal totalPremium,
            decimal totalGst,
            decimal totalPayable,
            string currencyCode)
        {
            if (currencyCode == null)
            {
                throw new NullReferenceException("When instantiating CalculatedPriceComponents, the passed parameter for currencyCode was null.");
            }

            this.BasePremium = basePremium;
            this.TerrorismPremium = terrorismPremium;
            this.Esl = esl;
            this.EmergencyServicesLevyNsw = eslNsw;
            this.EmergencyServicesLevyTas = eslTas;
            this.PremiumGst = premiumGst;
            this.StampDutyAct = stampDutyAct;
            this.StampDutyNsw = stampDutyNsw;
            this.StampDutyNt = stampDutyNt;
            this.StampDutyQld = stampDutyQld;
            this.StampDutySa = stampDutySa;
            this.StampDutyTas = stampDutyTas;
            this.StampDutyWa = stampDutyWa;
            this.StampDutyVic = stampDutyVic;
            this.Commission = commission;
            this.CommissionGst = commissionGst;
            this.BrokerFee = brokerFee;
            this.BrokerFeeGst = brokerFeeGst;
            this.UnderwriterFee = underwriterFee;
            this.UnderwriterFeeGst = underwriterFeeGst;
            this.RoadsideAssistanceFee = roadsideAssistanceFee;
            this.RoadsideAssistanceFeeGst = roadsideAssistanceFeeGst;
            this.PolicyFee = policyFee;
            this.PolicyFeeGst = policyFeeGst;
            this.PartnerFee = partnerFee;
            this.PartnerFeeGst = partnerFeeGst;
            this.AdministrationFee = administrationFee;
            this.AdministrationFeeGst = administrationFeeGst;
            this.EstablishmentFee = establishmentFee;
            this.EstablishmentFeeGst = establishmentFeeGst;
            this.Interest = interest;
            this.InterestGst = interestGst;
            this.MerchantFees = merchantFees;
            this.MerchantFeesGst = merchantFeesGst;
            this.TransactionCosts = transactionCosts;
            this.TransactionCostsGst = transactionCostsGst;
            this.StampDutyTotal = stampDutyTotal;
            this.TotalPremium = totalPremium;
            this.TotalGst = totalGst;
            this.TotalPayable = totalPayable;
            this.CurrencyCode = currencyCode;
        }

        [JsonConstructor]
        private CalculatedPriceComponents()
        {
        }

        /// <summary>
        /// Gets or sets the currency code, e.g. AUD, USD, PGK, GBP, PHP.
        /// </summary>
        [JsonProperty]
        public string CurrencyCode { get; set; } = PriceBreakdown.DefaultCurrencyCode;

        /// <summary>
        /// Gets the base premium.
        /// </summary>
        [JsonProperty]
        [JsonConverter(typeof(CurrencyStringConverter))]
        public decimal BasePremium { get; private set; }

        [JsonProperty]
        [JsonConverter(typeof(CurrencyStringConverter))]
        public decimal TerrorismPremium { get; private set; }

        /// <summary>
        /// Gets the total ESL.
        /// </summary>
        [JsonProperty]
        [JsonConverter(typeof(CurrencyStringConverter))]
        public decimal Esl { get; private set; }

        [JsonProperty]
        [JsonConverter(typeof(CurrencyStringConverter))]
        public decimal EmergencyServicesLevyNsw { get; private set; }

        [JsonProperty]
        [JsonConverter(typeof(CurrencyStringConverter))]
        public decimal EmergencyServicesLevyTas { get; private set; }

        /// <summary>
        /// Gets the total interest.
        /// </summary>
        [JsonProperty]
        [JsonConverter(typeof(CurrencyStringConverter))]
        public decimal Interest { get; private set; }

        [JsonProperty]
        [JsonConverter(typeof(CurrencyStringConverter))]
        public decimal InterestGst { get; private set; }

        /// <summary>
        /// Gets the total merchant fees.
        /// </summary>
        [JsonProperty]
        [JsonConverter(typeof(CurrencyStringConverter))]
        public decimal MerchantFees { get; private set; }

        [JsonProperty]
        [JsonConverter(typeof(CurrencyStringConverter))]
        public decimal MerchantFeesGst { get; private set; }

        /// <summary>
        /// Gets the total transaction costs.
        /// </summary>
        [JsonProperty]
        [JsonConverter(typeof(CurrencyStringConverter))]
        public decimal TransactionCosts { get; private set; }

        [JsonProperty]
        [JsonConverter(typeof(CurrencyStringConverter))]
        public decimal TransactionCostsGst { get; private set; }

        /// <summary>
        /// Gets the Premium GST.
        /// </summary>
        [JsonProperty]
        [JsonConverter(typeof(CurrencyStringConverter))]
        public decimal PremiumGst { get; private set; }

        /// <summary>
        /// Gets the Stamp Duty ACT.
        /// </summary>
        [JsonProperty]
        [JsonConverter(typeof(CurrencyStringConverter))]
        public decimal StampDutyAct { get; private set; }

        /// <summary>
        /// Gets the Stamp Duty NSW.
        /// </summary>
        [JsonProperty]
        [JsonConverter(typeof(CurrencyStringConverter))]
        public decimal StampDutyNsw { get; private set; }

        /// <summary>
        /// Gets the Stamp Duty NT.
        /// </summary>
        [JsonProperty]
        [JsonConverter(typeof(CurrencyStringConverter))]
        public decimal StampDutyNt { get; private set; }

        /// <summary>
        /// Gets the Stamp Duty QLD.
        /// </summary>
        [JsonProperty]
        [JsonConverter(typeof(CurrencyStringConverter))]
        public decimal StampDutyQld { get; private set; }

        /// <summary>
        /// Gets the Stamp Duty SA.
        /// </summary>
        [JsonProperty]
        [JsonConverter(typeof(CurrencyStringConverter))]
        public decimal StampDutySa { get; private set; }

        /// <summary>
        /// Gets the Stamp Duty TAS.
        /// </summary>
        [JsonProperty]
        [JsonConverter(typeof(CurrencyStringConverter))]
        public decimal StampDutyTas { get; private set; }

        /// <summary>
        /// Gets the Stamp Duty WA.
        /// </summary>
        [JsonProperty]
        [JsonConverter(typeof(CurrencyStringConverter))]
        public decimal StampDutyWa { get; private set; }

        /// <summary>
        /// Gets the Stamp Duty VIC.
        /// </summary>
        [JsonProperty]
        [JsonConverter(typeof(CurrencyStringConverter))]
        public decimal StampDutyVic { get; private set; }

        /// <summary>
        /// Gets the Commission.
        /// </summary>
        [JsonProperty]
        [JsonConverter(typeof(CurrencyStringConverter))]
        public decimal Commission { get; private set; }

        /// <summary>
        /// Gets the Commission GST.
        /// </summary>
        [JsonProperty]
        [JsonConverter(typeof(CurrencyStringConverter))]
        public decimal CommissionGst { get; private set; }

        /// <summary>
        /// Gets the Broker Fee.
        /// </summary>
        [JsonProperty]
        [JsonConverter(typeof(CurrencyStringConverter))]
        public decimal BrokerFee { get; private set; }

        /// <summary>
        /// Gets the Broker Fee GST.
        /// </summary>
        [JsonProperty]
        [JsonConverter(typeof(CurrencyStringConverter))]
        public decimal BrokerFeeGst { get; private set; }

        /// <summary>
        /// Gets the Underwriter Fee.
        /// </summary>
        [JsonProperty]
        [JsonConverter(typeof(CurrencyStringConverter))]
        public decimal UnderwriterFee { get; private set; }

        /// <summary>
        /// Gets the Underwriter Fee GST.
        /// </summary>
        [JsonProperty]
        [JsonConverter(typeof(CurrencyStringConverter))]
        public decimal UnderwriterFeeGst { get; private set; }

        [JsonProperty]
        [JsonConverter(typeof(CurrencyStringConverter))]
        public decimal RoadsideAssistanceFee { get; private set; }

        [JsonProperty]
        [JsonConverter(typeof(CurrencyStringConverter))]
        public decimal RoadsideAssistanceFeeGst { get; private set; }

        [JsonProperty]
        [JsonConverter(typeof(CurrencyStringConverter))]
        public decimal PolicyFee { get; private set; }

        [JsonProperty]
        [JsonConverter(typeof(CurrencyStringConverter))]
        public decimal PolicyFeeGst { get; private set; }

        [JsonProperty]
        [JsonConverter(typeof(CurrencyStringConverter))]
        public decimal PartnerFee { get; private set; }

        [JsonProperty]
        [JsonConverter(typeof(CurrencyStringConverter))]
        public decimal PartnerFeeGst { get; private set; }

        [JsonProperty]
        [JsonConverter(typeof(CurrencyStringConverter))]
        public decimal AdministrationFee { get; private set; }

        [JsonProperty]
        [JsonConverter(typeof(CurrencyStringConverter))]
        public decimal AdministrationFeeGst { get; private set; }

        [JsonProperty]
        [JsonConverter(typeof(CurrencyStringConverter))]
        public decimal EstablishmentFee { get; private set; }

        [JsonProperty]
        [JsonConverter(typeof(CurrencyStringConverter))]
        public decimal EstablishmentFeeGst { get; private set; }

        [JsonProperty]
        [JsonConverter(typeof(CurrencyStringConverter))]
        public decimal PaymentFee { get; private set; }

        [JsonProperty]
        [JsonConverter(typeof(CurrencyStringConverter))]
        public decimal PaymentFeeGst { get; private set; }

        /// <summary>
        /// Gets the total Stamp Duty.
        /// </summary>
        [JsonProperty]
        [JsonConverter(typeof(CurrencyStringConverter))]
        public decimal StampDutyTotal { get; private set; }

        /// <summary>
        /// Gets the total Premium.
        /// </summary>
        [JsonProperty]
        [JsonConverter(typeof(CurrencyStringConverter))]
        public decimal TotalPremium { get; private set; }

        /// <summary>
        /// Gets the Total GST.
        /// </summary>
        [JsonProperty]
        [JsonConverter(typeof(CurrencyStringConverter))]
        public decimal TotalGst { get; private set; }

        /// <summary>
        /// Gets the Total Payable.
        /// </summary>
        [JsonProperty]
        [JsonConverter(typeof(CurrencyStringConverter))]
        public decimal TotalPayable { get; private set; }
    }
}
