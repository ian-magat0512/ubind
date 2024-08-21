// <copyright file="PriceBreakdown.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadWriteModel
{
    using System;
    using Newtonsoft.Json;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Json;
    using UBind.Domain.JsonConverters;

    /// <summary>
    /// Interface exposing price breakdown for an insurance quote.
    /// </summary>
    /// <remarks>
    /// Existing workbooks output breakdown in one of two formats with different components.
    /// To handle this, there are two different constructors for the expected formats.
    /// The breakdown class contains the superset of these two sets of components, and where one
    /// is not used by a given format, it is just left as zero and will not impact the breakdown.
    /// .</remarks>
    public class PriceBreakdown : IPriceBreakdown, IEquatable<PriceBreakdown>
    {
        /// <summary>
        /// The default currency code to use if not specified.
        /// </summary>
        public static readonly string DefaultCurrencyCode = "AUD";

        private string currencyCode = DefaultCurrencyCode;

        [System.Text.Json.Serialization.JsonConstructor]
        public PriceBreakdown(
            string currencyCode,
            decimal basePremium,
            decimal terrorismPremium,
            decimal emergencyServicesLevyNSW,
            decimal emergencyServicesLevyTAS,
            decimal esl,
            decimal premiumGst,
            decimal stampDuty,
            decimal stampDutyAct,
            decimal stampDutyNsw,
            decimal stampDutyNt,
            decimal stampDutyQld,
            decimal stampDutySa,
            decimal stampDutyTas,
            decimal stampDutyVic,
            decimal stampDutyWa,
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
            decimal serviceFees,
            decimal interestCharges,
            decimal interestGst,
            decimal merchantFees,
            decimal merchantFeesGst,
            decimal transactionCosts,
            decimal transactionCostsGst,
            int periodInDays)
        {
            this.currencyCode = currencyCode;
            this.BasePremium = basePremium;
            this.TerrorismPremium = terrorismPremium;
            this.EmergencyServicesLevyNSW = emergencyServicesLevyNSW;
            this.EmergencyServicesLevyTAS = emergencyServicesLevyTAS;
            this.Esl = esl;
            this.PremiumGst = premiumGst;
            this.StampDuty = stampDuty;
            this.StampDutyAct = stampDutyAct;
            this.StampDutyNsw = stampDutyNsw;
            this.StampDutyNt = stampDutyNt;
            this.StampDutyQld = stampDutyQld;
            this.StampDutySa = stampDutySa;
            this.StampDutyTas = stampDutyTas;
            this.StampDutyVic = stampDutyVic;
            this.StampDutyWa = stampDutyWa;
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
            this.ServiceFees = serviceFees;
            this.InterestCharges = interestCharges;
            this.InterestGst = interestGst;
            this.MerchantFees = merchantFees;
            this.MerchantFeesGst = merchantFeesGst;
            this.TransactionCosts = transactionCosts;
            this.TransactionCostsGst = transactionCostsGst;
            this.PeriodInDays = periodInDays;
        }

        private PriceBreakdown(
            decimal basePremium,
            decimal terrorismPremium,
            decimal esl,
            decimal eslNsw,
            decimal eslTas,
            decimal premiumGst,
            decimal stampDuty,
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
            decimal parterFeeGst,
            decimal administrationFee,
            decimal administrationFeeGst,
            decimal establishmentFee,
            decimal establishmentFeeGst,
            decimal interestCharges,
            decimal interestGst,
            decimal merchantFees,
            decimal merchantFeesGst,
            decimal transactionCosts,
            decimal transactionCostsGst,
            decimal serviceFees,
            string currencyCode)
        {
            if (currencyCode == null)
            {
                throw new NullReferenceException("When instantiating PriceBreakdown, the passed parameter for currencyCode was null.");
            }

            this.BasePremium = basePremium;
            this.TerrorismPremium = terrorismPremium;
            this.Esl = esl;
            this.EmergencyServicesLevyNSW = eslNsw;
            this.EmergencyServicesLevyTAS = eslTas;
            this.PremiumGst = premiumGst;
            this.StampDuty = stampDuty;
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
            this.PartnerFeeGst = parterFeeGst;
            this.AdministrationFee = administrationFee;
            this.AdministrationFeeGst = administrationFeeGst;
            this.EstablishmentFee = establishmentFee;
            this.EstablishmentFeeGst = establishmentFeeGst;
            this.InterestCharges = interestCharges;
            this.InterestGst = interestGst;
            this.MerchantFees = merchantFees;
            this.MerchantFeesGst = merchantFeesGst;
            this.TransactionCosts = transactionCosts;
            this.TransactionCostsGst = transactionCostsGst;
            this.ServiceFees = serviceFees;
            this.CurrencyCode = currencyCode;
        }

        [JsonConstructor]
        private PriceBreakdown()
        {
        }

        private PriceBreakdown(string currencyCode)
        {
            this.CurrencyCode = currencyCode;
        }

        private PriceBreakdown(decimal merchantFees, decimal merchantFeesGst, decimal transactionCosts, decimal transactionCostsGst, string currencyCode)
        {
            this.MerchantFees = merchantFees;
            this.MerchantFeesGst = merchantFeesGst;
            this.TransactionCosts = transactionCosts;
            this.TransactionCostsGst = transactionCostsGst;
            this.CurrencyCode = currencyCode;
        }

        /// <summary>
        /// Gets the currency code for the price breakdown, e.g. AUD, USD, PGK, GBP, PHP.
        /// </summary>
        [JsonProperty]
        public string CurrencyCode
        {
            get
            {
                return this.currencyCode;
            }

            private set
            {
                this.currencyCode = value ?? DefaultCurrencyCode;
            }
        }

        /// <summary>
        /// Gets the base premium for all risks (excluding all taxes and fees).
        /// </summary>
        [JsonProperty]
        public decimal BasePremium { get; private set; }

        /// <summary>
        /// Gets the total terrorism premium, if applicable.
        /// </summary>
        [JsonProperty]
        public decimal TerrorismPremium { get; private set; }

        /// <summary>
        /// Gets the Emergency Services Levy (ESL) for the state of NSW, if applicable.
        /// </summary>
        [JsonProperty]
        public decimal EmergencyServicesLevyNSW { get; private set; }

        /// <summary>
        /// Gets the Emergency Services Levy (ESL) for the state of TAS, if applicable.
        /// </summary>
        [JsonProperty]
        public decimal EmergencyServicesLevyTAS { get; private set; }

        /// <summary>
        /// Gets the total Emergency Services Levy (ESL).
        /// </summary>
        [JsonProperty]
        public decimal Esl { get; private set; }

        /// <summary>
        /// Gets the GST payable on the base premium and ESL.
        /// </summary>
        [JsonProperty]
        public decimal PremiumGst { get; private set; }

        /// <summary>
        /// Gets the Stamp Duty when not specified on a per-state basis, otherwise zero.
        /// </summary>
        [JsonProperty]
        public decimal StampDuty { get; private set; }

        /// <summary>
        /// Gets the Stamp Duty payable to the ACT.
        /// </summary>
        [JsonProperty]
        public decimal StampDutyAct { get; private set; }

        /// <summary>
        /// Gets the Stamp Duty payable to NSW.
        /// </summary>
        [JsonProperty]
        public decimal StampDutyNsw { get; private set; }

        /// <summary>
        /// Gets the Stamp Duty payable to the NT.
        /// </summary>
        [JsonProperty]
        public decimal StampDutyNt { get; private set; }

        /// <summary>
        /// Gets the Stamp Duty payable to QLD.
        /// </summary>
        [JsonProperty]
        public decimal StampDutyQld { get; private set; }

        /// <summary>
        /// Gets the Stamp Duty payable to SA.
        /// </summary>
        [JsonProperty]
        public decimal StampDutySa { get; private set; }

        /// <summary>
        /// Gets the Stamp Duty payable to TAS.
        /// </summary>
        [JsonProperty]
        public decimal StampDutyTas { get; private set; }

        /// <summary>
        /// Gets the Stamp Duty payable to VIC.
        /// </summary>
        [JsonProperty]
        public decimal StampDutyVic { get; private set; }

        /// <summary>
        /// Gets the Stamp Duty payable to WA.
        /// </summary>
        [JsonProperty]
        public decimal StampDutyWa { get; private set; }

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
        /// Gets the broker fee.
        /// </summary>
        [JsonProperty]
        public decimal BrokerFee { get; private set; }

        /// <summary>
        /// Gets the GST payable on the broker fee.
        /// </summary>
        [JsonProperty]
        public decimal BrokerFeeGst { get; private set; }

        /// <summary>
        /// Gets the underwriter fee.
        /// </summary>
        [JsonProperty]
        public decimal UnderwriterFee { get; private set; }

        /// <summary>
        /// Gets the GST payable on the underwriter fee.
        /// </summary>
        [JsonProperty]
        public decimal UnderwriterFeeGst { get; private set; }

        /// <summary>
        /// Gets the optional roadside assistance fee, if required.
        /// </summary>
        [JsonProperty]
        public decimal RoadsideAssistanceFee { get; private set; }

        /// <summary>
        /// Gets the GST component for the Roadside Assistance Fee, if applicable.
        /// </summary>
        [JsonProperty]
        public decimal RoadsideAssistanceFeeGst { get; private set; }

        /// <summary>
        /// Gets the optional policy fee, if required.
        /// </summary>
        [JsonProperty]
        public decimal PolicyFee { get; private set; }

        /// <summary>
        /// Gets the GST payable for the policy fee, if applicable.
        /// </summary>
        [JsonProperty]
        public decimal PolicyFeeGst { get; private set; }

        /// <summary>
        /// Gets the optional partner fee, if required.
        /// </summary>
        [JsonProperty]
        public decimal PartnerFee { get; private set; }

        /// <summary>
        /// Gets the GST payable for the partner fee, if applicable.
        /// </summary>
        [JsonProperty]
        public decimal PartnerFeeGst { get; private set; }

        /// <summary>
        /// Gets the optional administration fee, if required.
        /// </summary>
        [JsonProperty]
        public decimal AdministrationFee { get; private set; }

        /// <summary>
        /// Gets the GST payable for the administration fee, if applicable.
        /// </summary>
        [JsonProperty]
        public decimal AdministrationFeeGst { get; private set; }

        /// <summary>
        /// Gets the optional establishment fee, if required.
        /// </summary>
        [JsonProperty]
        public decimal EstablishmentFee { get; private set; }

        /// <summary>
        /// Gets the GST payable for the establishment fee, if applicable.
        /// </summary>
        [JsonProperty]
        public decimal EstablishmentFeeGst { get; private set; }

        /// <summary>
        /// Gets the total GST payable.
        /// </summary>
        /// <remarks>Note that commission GST is already included in premium GST.</remarks>
        [JsonProperty]
        public decimal TotalGst =>
            this.PremiumGst +
            this.BrokerFeeGst +
            this.UnderwriterFeeGst +
            this.RoadsideAssistanceFeeGst +
            this.PolicyFeeGst +
            this.PartnerFeeGst +
            this.AdministrationFeeGst +
            this.EstablishmentFeeGst +
            this.InterestGst +
            this.MerchantFeesGst +
            this.TransactionCostsGst;

        /// <summary>
        /// Gets the total service fees (as specified in old-style price breakdowns).
        /// </summary>
        /// <remarks>This will be zero in a new-style price breakdown.</remarks>
        [JsonProperty]
        public decimal ServiceFees { get; private set; }

        /// <summary>
        /// Gets the total interest charges where applicable.
        /// </summary>
        [JsonProperty]
        public decimal InterestCharges { get; private set; }

        /// <summary>
        /// Gets the gst component of the interest charges, if desired.
        /// </summary>
        [JsonProperty]
        public decimal InterestGst { get; private set; }

        /// <summary>
        /// Gets or sets the total merchant fees (i.e. credit card charges).
        /// </summary>
        [JsonProperty]
        public decimal MerchantFees { get; set; }

        /// <summary>
        /// Gets the gst component of the merchant fees, if desired.
        /// </summary>
        [JsonProperty]
        public decimal MerchantFeesGst { get; set; }

        /// <summary>
        /// Gets the total transaction costs (e.g. from payment gateway provider).
        /// </summary>
        [JsonProperty]
        public decimal TransactionCosts { get; private set; }

        /// <summary>
        /// Gets the gst component of the total transaction costs, if desired.
        /// </summary>
        [JsonProperty]
        public decimal TransactionCostsGst { get; private set; }

        /// <summary>
        /// Gets the period the price covers.
        /// </summary>
        [JsonProperty]
        public int PeriodInDays { get; private set; }

        /// <summary>
        /// Gets the total Stamp Duty.
        /// </summary>
        /// <remarks>
        /// In old-style breakdowns the stamp duty was calculated as the single value <c>StampDuty</c>.
        /// In new-style breakdowns the stamp duty is calculated on a per-state basis, but leave <c>StampDuty</c> as zero.
        /// So we can calculate the total for either style just bby summing both <c>StampDuty</c> and the state-based properties.
        /// </remarks>
        [JsonProperty]
        [GetOnlyJsonProperty]
        public decimal StampDutyTotal =>
            this.StampDuty + // will be zero, when stamp duty is specified by state.
            this.StampDutyAct +
            this.StampDutyNsw +
            this.StampDutyNt +
            this.StampDutyQld +
            this.StampDutySa +
            this.StampDutyTas +
            this.StampDutyVic +
            this.StampDutyWa;

        /// <summary>
        /// Gets the total premium, excluding broker and underwriter fees and payment charges.
        /// </summary>
        [JsonProperty]
        [GetOnlyJsonProperty]
        public decimal TotalPremium =>
            this.BasePremium +
            this.TerrorismPremium +
            this.Esl +
            this.PremiumGst +
            this.StampDutyTotal;

        /// <summary>
        /// Gets the total payable.
        /// </summary>
        [JsonProperty]
        [GetOnlyJsonProperty]
        public decimal TotalExcludingPaymentCharges =>
            this.TotalPremium +
            this.BrokerFee +
            this.BrokerFeeGst +
            this.UnderwriterFee +
            this.UnderwriterFeeGst +
            this.RoadsideAssistanceFee +
            this.RoadsideAssistanceFeeGst +
            this.PolicyFee +
            this.PolicyFeeGst +
            this.PartnerFee +
            this.PartnerFeeGst +
            this.AdministrationFee +
            this.AdministrationFeeGst +
            this.EstablishmentFee +
            this.EstablishmentFeeGst;

        /// <summary>
        /// Gets the total payable.
        /// </summary>
        [JsonProperty]
        [GetOnlyJsonProperty]
        public decimal TotalPayable
        {
            get
            {
                var sum =
                    this.TotalPremium +
                    this.BrokerFee +
                    this.BrokerFeeGst +
                    this.UnderwriterFee +
                    this.UnderwriterFeeGst +
                    this.RoadsideAssistanceFee +
                    this.RoadsideAssistanceFeeGst +
                    this.PolicyFee +
                    this.PolicyFeeGst +
                    this.PartnerFee +
                    this.PartnerFeeGst +
                    this.AdministrationFee +
                    this.AdministrationFeeGst +
                    this.EstablishmentFee +
                    this.EstablishmentFeeGst +
                    this.InterestCharges +
                    this.InterestGst +
                    this.MerchantFees +
                    this.MerchantFeesGst +
                    this.TransactionCosts +
                    this.TransactionCostsGst +
                    this.ServiceFees;

                return sum.RoundToWholeCents();
            }
        }

        /// <summary>
        /// Subtraction operation.
        /// </summary>
        /// <param name="firstAddend">The amounts to be subtracted from.</param>
        /// <param name="secondAddend">The amounts to subtract.</param>
        /// <returns>A new instance of <see cref="PriceBreakdown"/> with each component representing the difference of the minuendn and subtrahend.</returns>
        public static PriceBreakdown operator +(PriceBreakdown firstAddend, PriceBreakdown secondAddend)
        {
            if (!CurrenciesMatch(firstAddend, secondAddend))
            {
                throw new ErrorException(Errors.Calculation.UnableToAddTwoPriceBreakdownsWhenCurrenciesDontMatch(firstAddend, secondAddend));
            }

            return new PriceBreakdown(
                firstAddend.BasePremium + secondAddend.BasePremium,
                firstAddend.TerrorismPremium + secondAddend.TerrorismPremium,
                firstAddend.Esl + secondAddend.Esl,
                firstAddend.EmergencyServicesLevyNSW + secondAddend.EmergencyServicesLevyNSW,
                firstAddend.EmergencyServicesLevyTAS + secondAddend.EmergencyServicesLevyTAS,
                firstAddend.PremiumGst + secondAddend.PremiumGst,
                firstAddend.StampDuty + secondAddend.StampDuty,
                firstAddend.StampDutyAct + secondAddend.StampDutyAct,
                firstAddend.StampDutyNsw + secondAddend.StampDutyNsw,
                firstAddend.StampDutyNt + secondAddend.StampDutyNt,
                firstAddend.StampDutyQld + secondAddend.StampDutyQld,
                firstAddend.StampDutySa + secondAddend.StampDutySa,
                firstAddend.StampDutyTas + secondAddend.StampDutyTas,
                firstAddend.StampDutyWa + secondAddend.StampDutyWa,
                firstAddend.StampDutyVic + secondAddend.StampDutyVic,
                firstAddend.Commission + secondAddend.Commission,
                firstAddend.CommissionGst + secondAddend.CommissionGst,
                firstAddend.BrokerFee + secondAddend.BrokerFee,
                firstAddend.BrokerFeeGst + secondAddend.BrokerFeeGst,
                firstAddend.UnderwriterFee + secondAddend.UnderwriterFee,
                firstAddend.UnderwriterFeeGst + secondAddend.UnderwriterFeeGst,
                firstAddend.RoadsideAssistanceFee + secondAddend.RoadsideAssistanceFee,
                firstAddend.RoadsideAssistanceFeeGst + secondAddend.RoadsideAssistanceFeeGst,
                firstAddend.PolicyFee + secondAddend.PolicyFee,
                firstAddend.PolicyFeeGst + secondAddend.PolicyFeeGst,
                firstAddend.PartnerFee + secondAddend.PartnerFee,
                firstAddend.PartnerFeeGst + secondAddend.PartnerFeeGst,
                firstAddend.AdministrationFee + secondAddend.AdministrationFee,
                firstAddend.AdministrationFeeGst + secondAddend.AdministrationFeeGst,
                firstAddend.EstablishmentFee + secondAddend.EstablishmentFee,
                firstAddend.EstablishmentFeeGst + secondAddend.EstablishmentFeeGst,
                firstAddend.InterestCharges + secondAddend.InterestCharges,
                firstAddend.InterestGst + secondAddend.InterestGst,
                firstAddend.MerchantFees + secondAddend.MerchantFees,
                firstAddend.MerchantFeesGst + secondAddend.MerchantFeesGst,
                firstAddend.TransactionCosts + secondAddend.TransactionCosts,
                firstAddend.TransactionCostsGst + secondAddend.TransactionCostsGst,
                firstAddend.ServiceFees + secondAddend.ServiceFees,
                GetFirstAvailableCurrencyCode(firstAddend, secondAddend));
        }

        /// <summary>
        /// Subtraction operation.
        /// </summary>
        /// <param name="minuend">The amounts to be subtracted from.</param>
        /// <param name="subtrahend">The amounts to subtract.</param>
        /// <returns>A new instance of <see cref="PriceBreakdown"/> with each component representing the difference of the minuendn and subtrahend.</returns>
        public static PriceBreakdown operator -(PriceBreakdown minuend, PriceBreakdown subtrahend)
        {
            if (!CurrenciesMatch(minuend, subtrahend))
            {
                throw new ErrorException(Errors.Calculation.UnableToSubtractTwoPriceBreakdownsWhenCurrenciesDontMatch(minuend, subtrahend));
            }

            return new PriceBreakdown(
                minuend.BasePremium - subtrahend.BasePremium,
                minuend.TerrorismPremium - subtrahend.TerrorismPremium,
                minuend.Esl - subtrahend.Esl,
                minuend.EmergencyServicesLevyNSW - subtrahend.EmergencyServicesLevyNSW,
                minuend.EmergencyServicesLevyTAS - subtrahend.EmergencyServicesLevyTAS,
                minuend.PremiumGst - subtrahend.PremiumGst,
                minuend.StampDuty - subtrahend.StampDuty,
                minuend.StampDutyAct - subtrahend.StampDutyAct,
                minuend.StampDutyNsw - subtrahend.StampDutyNsw,
                minuend.StampDutyNt - subtrahend.StampDutyNt,
                minuend.StampDutyQld - subtrahend.StampDutyQld,
                minuend.StampDutySa - subtrahend.StampDutySa,
                minuend.StampDutyTas - subtrahend.StampDutyTas,
                minuend.StampDutyWa - subtrahend.StampDutyWa,
                minuend.StampDutyVic - subtrahend.StampDutyVic,
                minuend.Commission - subtrahend.Commission,
                minuend.CommissionGst - subtrahend.CommissionGst,
                minuend.BrokerFee - subtrahend.BrokerFee,
                minuend.BrokerFeeGst - subtrahend.BrokerFeeGst,
                minuend.UnderwriterFee - subtrahend.UnderwriterFee,
                minuend.UnderwriterFeeGst - subtrahend.UnderwriterFeeGst,
                minuend.RoadsideAssistanceFee - subtrahend.RoadsideAssistanceFee,
                minuend.RoadsideAssistanceFeeGst - subtrahend.RoadsideAssistanceFeeGst,
                minuend.PolicyFee - subtrahend.PolicyFee,
                minuend.PolicyFeeGst - subtrahend.PolicyFeeGst,
                minuend.PartnerFee - subtrahend.PartnerFee,
                minuend.PartnerFeeGst - subtrahend.PartnerFeeGst,
                minuend.AdministrationFee - subtrahend.AdministrationFee,
                minuend.AdministrationFeeGst - subtrahend.AdministrationFeeGst,
                minuend.EstablishmentFee - subtrahend.EstablishmentFee,
                minuend.EstablishmentFeeGst - subtrahend.EstablishmentFeeGst,
                minuend.InterestCharges - subtrahend.InterestCharges,
                minuend.InterestGst - subtrahend.InterestGst,
                minuend.MerchantFees - subtrahend.MerchantFees,
                minuend.MerchantFeesGst - subtrahend.MerchantFeesGst,
                minuend.TransactionCosts - subtrahend.TransactionCosts,
                minuend.TransactionCostsGst - subtrahend.TransactionCostsGst,
                minuend.ServiceFees - subtrahend.ServiceFees,
                GetFirstAvailableCurrencyCode(minuend, subtrahend));
        }

        /// <summary>
        /// Equality operation.
        /// </summary>
        /// <param name="a">First operand.</param>
        /// <param name="b">Second operand.</param>
        /// <returns>True if the operands are equal using value semantics, otherwise false.</returns>
        public static bool operator ==(PriceBreakdown? a, PriceBreakdown? b)
        {
            // If both are null, or both are same instance, return true.
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            // Return true if the fields match:
            return a.Equals(b);
        }

        /// <summary>
        /// Inequality operation.
        /// </summary>
        /// <param name="a">First operand.</param>
        /// <param name="b">Second operand.</param>
        /// <returns>True if the operands are not equal using value semantics, otherwise false.</returns>
        public static bool operator !=(PriceBreakdown a, PriceBreakdown b)
        {
            return !(a == b);
        }

        /// <summary>
        /// Multiply the breakdown by a scalar factor.
        /// </summary>
        /// <param name="breakdown">The breakdown to multiply.</param>
        /// <param name="scalar">The amount to multiply by.</param>
        /// <returns>A new instance of <see cref="PriceBreakdown"/> with each component representing the difference of the minuendn and subtrahend.</returns>
        public static PriceBreakdown operator *(PriceBreakdown breakdown, decimal scalar)
        {
            return new PriceBreakdown(
                breakdown.BasePremium * scalar,
                breakdown.TerrorismPremium * scalar,
                breakdown.Esl * scalar,
                breakdown.EmergencyServicesLevyNSW * scalar,
                breakdown.EmergencyServicesLevyTAS * scalar,
                breakdown.PremiumGst * scalar,
                breakdown.StampDuty * scalar,
                breakdown.StampDutyAct * scalar,
                breakdown.StampDutyNsw * scalar,
                breakdown.StampDutyNt * scalar,
                breakdown.StampDutyQld * scalar,
                breakdown.StampDutySa * scalar,
                breakdown.StampDutyTas * scalar,
                breakdown.StampDutyWa * scalar,
                breakdown.StampDutyVic * scalar,
                breakdown.Commission * scalar,
                breakdown.CommissionGst * scalar,
                breakdown.BrokerFee * scalar,
                breakdown.BrokerFeeGst * scalar,
                breakdown.UnderwriterFee * scalar,
                breakdown.UnderwriterFeeGst * scalar,
                breakdown.RoadsideAssistanceFee * scalar,
                breakdown.RoadsideAssistanceFeeGst * scalar,
                breakdown.PolicyFee * scalar,
                breakdown.PolicyFeeGst * scalar,
                breakdown.PartnerFee * scalar,
                breakdown.PartnerFeeGst * scalar,
                breakdown.AdministrationFee * scalar,
                breakdown.AdministrationFeeGst * scalar,
                breakdown.EstablishmentFee * scalar,
                breakdown.EstablishmentFeeGst * scalar,
                breakdown.InterestCharges * scalar,
                breakdown.InterestGst * scalar,
                breakdown.MerchantFees * scalar,
                breakdown.MerchantFeesGst * scalar,
                breakdown.TransactionCosts * scalar,
                breakdown.TransactionCostsGst * scalar,
                breakdown.ServiceFees * scalar,
                breakdown.CurrencyCode);
        }

        /// <summary>
        /// Gets a price breakdown with all components zero.
        /// </summary>
        /// <param name="currencyCode">The currency code.</param>
        /// <returns>A price breakdown that is all zeroes.</returns>
        public static PriceBreakdown Zero(string currencyCode) => new PriceBreakdown(currencyCode);

        /// <summary>
        /// Gets a price breakdown with base premium only.
        /// </summary>
        /// <param name="currencyCode">The currency code.</param>
        /// <returns>A price breakdown with base premium only.</returns>
        public static PriceBreakdown OnlyBasePremium(string currencyCode, decimal premium)
        {
            return new PriceBreakdown(premium, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, currencyCode);
        }

        /// <summary>
        /// Gets a price breakdown with base premium set to 1c and all other components zero.
        /// </summary>
        /// <param name="currencyCode">The currency code.</param>
        /// <returns>A price breakdown which is equal to once of the minor units in the given currency.</returns>
        public static PriceBreakdown OneMinorUnits(string currencyCode) => new PriceBreakdown { BasePremium = 0.01m, CurrencyCode = currencyCode };

        /// <summary>
        /// Creates a PriceBreakDown from the source CalculationResult.
        /// </summary>
        /// <param name="calculationResultData">The calculation result data.</param>
        /// <returns>The price break down.</returns>
        public static PriceBreakdown CreateFromCalculationResultData(CachingJObjectWrapper calculationResultData)
        {
            var calculatedPayment = CalculatedPayment.CreateFromCalculationResult(calculationResultData);
            return (calculatedPayment.OutputVersion > 1)
                ? CreateFromCalculationFormatV2(calculatedPayment.ComponentsV2)
                : CreateFromCalculationFormatV1(calculatedPayment.ComponentsV1);
        }

        /// <summary>
        /// Create a new instance of <see cref="PriceBreakdown"/> setting components used in old format.
        /// </summary>
        /// <param name="payment">The breakdown data from an old-format calculation.</param>
        /// <returns>A new instance of <see cref="PriceBreakdown"/>,.</returns>
        public static PriceBreakdown CreateFromCalculationFormatV1(CalculatedPaymentTotal payment)
        {
            return new PriceBreakdown
            {
                BasePremium = payment.Premium,
                Esl = payment.Esl,
                PremiumGst = payment.Gst,
                StampDuty = payment.StampDuty,
                InterestCharges = payment.Interest,
                MerchantFees = payment.MerchantFees,
                TransactionCosts = payment.TransactionCosts,
                ServiceFees = payment.ServiceFees,
                CurrencyCode = payment.CurrencyCode,
            };
        }

        /// <summary>
        /// Creates a new instance of the <see cref="PriceBreakdown"/> class, setting components used in new
        /// calculation output format.
        /// </summary>
        /// <param name="payment">The calculated payment total from the calculation output.</param>
        /// <returns>a new instance of the <see cref="PriceBreakdown"/> class.</returns>
        public static PriceBreakdown CreateFromCalculationFormatV2(CalculatedPriceComponents payment)
        {
            return new PriceBreakdown
            {
                BasePremium = payment.BasePremium,
                TerrorismPremium = payment.TerrorismPremium,
                Esl = payment.Esl,
                EmergencyServicesLevyNSW = payment.EmergencyServicesLevyNsw,
                EmergencyServicesLevyTAS = payment.EmergencyServicesLevyTas,
                PremiumGst = payment.PremiumGst,
                StampDutyAct = payment.StampDutyAct,
                StampDutyNsw = payment.StampDutyNsw,
                StampDutyNt = payment.StampDutyNt,
                StampDutyQld = payment.StampDutyQld,
                StampDutySa = payment.StampDutySa,
                StampDutyTas = payment.StampDutyTas,
                StampDutyWa = payment.StampDutyWa,
                StampDutyVic = payment.StampDutyVic,
                Commission = payment.Commission,
                CommissionGst = payment.CommissionGst,
                BrokerFee = payment.BrokerFee,
                BrokerFeeGst = payment.BrokerFeeGst,
                UnderwriterFee = payment.UnderwriterFee,
                UnderwriterFeeGst = payment.UnderwriterFeeGst,
                InterestCharges = payment.Interest,
                InterestGst = payment.InterestGst,
                MerchantFees = payment.MerchantFees,
                MerchantFeesGst = payment.MerchantFeesGst,
                TransactionCosts = payment.TransactionCosts,
                TransactionCostsGst = payment.TransactionCostsGst,
                CurrencyCode = payment.CurrencyCode,
                RoadsideAssistanceFee = payment.RoadsideAssistanceFee,
                RoadsideAssistanceFeeGst = payment.RoadsideAssistanceFeeGst,
                PolicyFee = payment.PolicyFee,
                PolicyFeeGst = payment.PolicyFeeGst,
                PartnerFee = payment.PartnerFee,
                PartnerFeeGst = payment.PartnerFeeGst,
                AdministrationFee = payment.AdministrationFee,
                AdministrationFeeGst = payment.AdministrationFeeGst,
                EstablishmentFee = payment.EstablishmentFee,
                EstablishmentFeeGst = payment.EstablishmentFeeGst,
            };
        }

        /// <summary>
        /// Creates a new price breakdown including only payment charges.
        /// </summary>
        /// <param name="merchantFees">Merchant fees.</param>
        /// <param name="merchantFeesGst">The GST for the merchant fees, if used. Defaults to 0.</param>
        /// <param name="transactionCosts">Transaction costs.</param>
        /// <param name="transactionCostsGst">The GST for the transaction costs, if used. Defaults to 0.</param>
        /// <param name="currencyCode">The currency code, e.g. "AUD", "USD", "GBP".</param>
        /// <returns>A new instance of <see cref="PriceBreakdown"/> containing only payment charges.</returns>
        public static PriceBreakdown CreateForPaymentCharges(decimal merchantFees, decimal merchantFeesGst, decimal transactionCosts, decimal transactionCostsGst, string currencyCode)
        {
            return new PriceBreakdown(merchantFees, merchantFeesGst, transactionCosts, transactionCostsGst, currencyCode);
        }

        /// <summary>
        /// Checks if the currencies of the two PriceBreakdowns match.
        /// </summary>
        /// <param name="first">The first PriceBreakDown.</param>
        /// <param name="second">The second PriceBreakDown.</param>
        /// <returns>true if currencies of the PriceBreakDowns match.</returns>
        public static bool CurrenciesMatch(PriceBreakdown first, PriceBreakdown second)
        {
            return first.CurrencyCode == second.CurrencyCode;
        }

        /// <summary>
        /// Gets the first available currency code.
        /// If the first currency code is null it returns the second.
        /// </summary>
        /// <param name="first">The first price breakdown.</param>
        /// <param name="second">The second price breakdown.</param>
        /// <returns>The currency code.</returns>
        public static string GetFirstAvailableCurrencyCode(PriceBreakdown first, PriceBreakdown second)
        {
            return first.CurrencyCode != null ? first.CurrencyCode : second.CurrencyCode;
        }

        /// <summary>
        /// Filter a price breakdown to include only components specified in a filter.
        /// </summary>
        /// <param name="filter">The filter specifying which components to include.</param>
        /// <returns>A new instance of <see cref="PriceBreakdown"/> with only included components set.</returns>
        public PriceBreakdown Filter(PriceComponentFilter filter)
        {
            return new PriceBreakdown(
                filter.BasePremium ? this.BasePremium : 0m,
                filter.TerrorismPremium ? this.TerrorismPremium : 0m,
                filter.Esl ? this.Esl : 0m,
                filter.EslNsw ? this.EmergencyServicesLevyNSW : 0m,
                filter.EslTas ? this.EmergencyServicesLevyTAS : 0m,
                filter.PremiumGst ? this.PremiumGst : 0m,
                filter.StampDuty ? this.StampDuty : 0m,
                filter.StampDutyAct ? this.StampDutyAct : 0m,
                filter.StampDutyNsw ? this.StampDutyNsw : 0m,
                filter.StampDutyNt ? this.StampDutyNt : 0m,
                filter.StampDutyQld ? this.StampDutyQld : 0m,
                filter.StampDutySa ? this.StampDutySa : 0m,
                filter.StampDutyTas ? this.StampDutyTas : 0m,
                filter.StampDutyWa ? this.StampDutyWa : 0m,
                filter.StampDutyVic ? this.StampDutyVic : 0m,
                filter.Commission ? this.Commission : 0m,
                filter.CommissionGst ? this.CommissionGst : 0m,
                filter.BrokerFee ? this.BrokerFee : 0m,
                filter.BrokerFeeGst ? this.BrokerFeeGst : 0m,
                filter.UnderwriterFee ? this.UnderwriterFee : 0m,
                filter.UnderwriterFeeGst ? this.UnderwriterFeeGst : 0m,
                filter.RoadsideAssistanceFee ? this.RoadsideAssistanceFee : 0m,
                filter.RoadsideAssistanceFeeGst ? this.RoadsideAssistanceFeeGst : 0m,
                filter.PolicyFee ? this.PolicyFee : 0m,
                filter.PolicyFeeGst ? this.PolicyFeeGst : 0m,
                filter.PartnerFee ? this.PartnerFee : 0m,
                filter.PartnerFeeGst ? this.PartnerFeeGst : 0m,
                filter.AdministrationFee ? this.AdministrationFee : 0m,
                filter.AdministrationFeeGst ? this.AdministrationFeeGst : 0m,
                filter.EstablishmentFee ? this.EstablishmentFee : 0m,
                filter.EstablishmentFeeGst ? this.EstablishmentFeeGst : 0m,
                filter.InterestCharges ? this.InterestCharges : 0m,
                filter.InterestChargesGst ? this.InterestGst : 0m,
                filter.MerchantFees ? this.MerchantFees : 0m,
                filter.MerchantFeesGst ? this.MerchantFeesGst : 0m,
                filter.TransactionCosts ? this.TransactionCosts : 0m,
                filter.TransactionCostsGst ? this.TransactionCostsGst : 0m,
                filter.ServiceFees ? this.ServiceFees : 0m,
                this.CurrencyCode);
        }

        /// <summary>
        /// Filter a price breakdown to exclude components specified in a filter.
        /// </summary>
        /// <param name="filter">The filter specifying which components to exclude.</param>
        /// <returns>A new instance of <see cref="PriceBreakdown"/> with only non-excluded components set.</returns>
        public PriceBreakdown FilterInverse(PriceComponentFilter filter)
        {
            return new PriceBreakdown(
                filter.BasePremium ? 0m : this.BasePremium,
                filter.TerrorismPremium ? 0m : this.TerrorismPremium,
                filter.Esl ? 0m : this.Esl,
                filter.EslNsw ? 0m : this.EmergencyServicesLevyNSW,
                filter.EslTas ? 0m : this.EmergencyServicesLevyTAS,
                filter.PremiumGst ? 0m : this.PremiumGst,
                filter.StampDuty ? 0m : this.StampDuty,
                filter.StampDutyAct ? 0m : this.StampDutyAct,
                filter.StampDutyNsw ? 0m : this.StampDutyNsw,
                filter.StampDutyNt ? 0m : this.StampDutyNt,
                filter.StampDutyQld ? 0m : this.StampDutyQld,
                filter.StampDutySa ? 0m : this.StampDutySa,
                filter.StampDutyTas ? 0m : this.StampDutyTas,
                filter.StampDutyWa ? 0m : this.StampDutyWa,
                filter.StampDutyVic ? 0m : this.StampDutyVic,
                filter.Commission ? 0m : this.Commission,
                filter.CommissionGst ? 0m : this.CommissionGst,
                filter.BrokerFee ? 0m : this.BrokerFee,
                filter.BrokerFeeGst ? 0m : this.BrokerFeeGst,
                filter.UnderwriterFee ? 0m : this.UnderwriterFee,
                filter.UnderwriterFeeGst ? 0m : this.UnderwriterFeeGst,
                filter.RoadsideAssistanceFee ? 0m : this.RoadsideAssistanceFee,
                filter.RoadsideAssistanceFeeGst ? 0m : this.RoadsideAssistanceFeeGst,
                filter.PolicyFee ? 0m : this.PolicyFee,
                filter.PolicyFeeGst ? 0m : this.PolicyFeeGst,
                filter.PartnerFee ? 0m : this.PartnerFee,
                filter.PartnerFeeGst ? 0m : this.PartnerFeeGst,
                filter.AdministrationFee ? 0m : this.AdministrationFee,
                filter.AdministrationFeeGst ? 0m : this.AdministrationFeeGst,
                filter.EstablishmentFee ? 0m : this.EstablishmentFee,
                filter.EstablishmentFeeGst ? 0m : this.EstablishmentFeeGst,
                filter.InterestCharges ? 0m : this.InterestCharges,
                filter.InterestChargesGst ? 0m : this.InterestGst,
                filter.MerchantFees ? 0m : this.MerchantFees,
                filter.MerchantFeesGst ? 0m : this.MerchantFeesGst,
                filter.TransactionCosts ? 0m : this.TransactionCosts,
                filter.TransactionCostsGst ? 0m : this.TransactionCostsGst,
                filter.ServiceFees ? 0m : this.ServiceFees,
                this.CurrencyCode);
        }

        /// <inheritdoc/>
        public bool Equals(PriceBreakdown? other)
        {
            if (other == null)
            {
                return false;
            }

            return this.BasePremium == other.BasePremium
                && this.Esl == other.Esl
                && this.PremiumGst == other.PremiumGst
                && this.StampDuty == other.StampDuty
                && this.StampDutyAct == other.StampDutyAct
                && this.StampDutyNsw == other.StampDutyNsw
                && this.StampDutyNt == other.StampDutyNt
                && this.StampDutyQld == other.StampDutyQld
                && this.StampDutySa == other.StampDutySa
                && this.StampDutyTas == other.StampDutyTas
                && this.StampDutyWa == other.StampDutyWa
                && this.StampDutyVic == other.StampDutyVic
                && this.Commission == other.Commission
                && this.CommissionGst == other.CommissionGst
                && this.BrokerFee == other.BrokerFee
                && this.BrokerFeeGst == other.BrokerFeeGst
                && this.UnderwriterFee == other.UnderwriterFee
                && this.UnderwriterFeeGst == other.UnderwriterFeeGst
                && this.InterestCharges == other.InterestCharges
                && this.MerchantFees == other.MerchantFees
                && this.TransactionCosts == other.TransactionCosts
                && this.ServiceFees == other.ServiceFees
                && this.CurrencyCode == other.CurrencyCode
                && this.TerrorismPremium == other.TerrorismPremium
                && this.EmergencyServicesLevyNSW == other.EmergencyServicesLevyNSW
                && this.EmergencyServicesLevyTAS == other.EmergencyServicesLevyTAS
                && this.RoadsideAssistanceFee == other.RoadsideAssistanceFee
                && this.RoadsideAssistanceFeeGst == other.RoadsideAssistanceFeeGst
                && this.PolicyFee == other.PolicyFee
                && this.PolicyFeeGst == other.PolicyFeeGst
                && this.PartnerFee == other.PartnerFee
                && this.PartnerFeeGst == other.PartnerFeeGst
                && this.AdministrationFee == other.AdministrationFee
                && this.AdministrationFeeGst == other.AdministrationFeeGst
                && this.EstablishmentFee == other.EstablishmentFee
                && this.EstablishmentFeeGst == other.EstablishmentFeeGst
                && this.InterestGst == other.InterestGst
                && this.MerchantFeesGst == other.MerchantFeesGst
                && this.TransactionCostsGst == other.TransactionCostsGst;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return this.Equals(obj as PriceBreakdown);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = (int)2166136261;
                hash = (hash * 16777619) ^ this.BasePremium.GetHashCode();
                hash = (hash * 16777619) ^ this.TerrorismPremium.GetHashCode();
                hash = (hash * 16777619) ^ this.Esl.GetHashCode();
                hash = (hash * 16777619) ^ this.EmergencyServicesLevyNSW.GetHashCode();
                hash = (hash * 16777619) ^ this.EmergencyServicesLevyTAS.GetHashCode();
                hash = (hash * 16777619) ^ this.PremiumGst.GetHashCode();
                hash = (hash * 16777619) ^ this.StampDuty.GetHashCode();
                hash = (hash * 16777619) ^ this.StampDutyAct.GetHashCode();
                hash = (hash * 16777619) ^ this.StampDutyNsw.GetHashCode();
                hash = (hash * 16777619) ^ this.StampDutyNt.GetHashCode();
                hash = (hash * 16777619) ^ this.StampDutyQld.GetHashCode();
                hash = (hash * 16777619) ^ this.StampDutySa.GetHashCode();
                hash = (hash * 16777619) ^ this.StampDutyTas.GetHashCode();
                hash = (hash * 16777619) ^ this.StampDutyWa.GetHashCode();
                hash = (hash * 16777619) ^ this.StampDutyVic.GetHashCode();
                hash = (hash * 16777619) ^ this.Commission.GetHashCode();
                hash = (hash * 16777619) ^ this.CommissionGst.GetHashCode();
                hash = (hash * 16777619) ^ this.BrokerFee.GetHashCode();
                hash = (hash * 16777619) ^ this.BrokerFeeGst.GetHashCode();
                hash = (hash * 16777619) ^ this.UnderwriterFee.GetHashCode();
                hash = (hash * 16777619) ^ this.UnderwriterFeeGst.GetHashCode();
                hash = (hash * 16777619) ^ this.RoadsideAssistanceFee.GetHashCode();
                hash = (hash * 16777619) ^ this.RoadsideAssistanceFeeGst.GetHashCode();
                hash = (hash * 16777619) ^ this.PolicyFee.GetHashCode();
                hash = (hash * 16777619) ^ this.PolicyFeeGst.GetHashCode();
                hash = (hash * 16777619) ^ this.PartnerFee.GetHashCode();
                hash = (hash * 16777619) ^ this.PartnerFeeGst.GetHashCode();
                hash = (hash * 16777619) ^ this.AdministrationFee.GetHashCode();
                hash = (hash * 16777619) ^ this.AdministrationFeeGst.GetHashCode();
                hash = (hash * 16777619) ^ this.EstablishmentFee.GetHashCode();
                hash = (hash * 16777619) ^ this.EstablishmentFeeGst.GetHashCode();
                hash = (hash * 16777619) ^ this.InterestCharges.GetHashCode();
                hash = (hash * 16777619) ^ this.InterestGst.GetHashCode();
                hash = (hash * 16777619) ^ this.MerchantFees.GetHashCode();
                hash = (hash * 16777619) ^ this.MerchantFeesGst.GetHashCode();
                hash = (hash * 16777619) ^ this.TransactionCosts.GetHashCode();
                hash = (hash * 16777619) ^ this.TransactionCostsGst.GetHashCode();
                hash = (hash * 16777619) ^ this.ServiceFees.GetHashCode();
                hash = (hash * 16777619) ^ this.CurrencyCode.GetHashCode();
                return hash;
            }
        }
    }
}
