// <copyright file="PriceComponentFilter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadWriteModel
{
    /// <summary>
    /// For selecting components of a price breakdown for inclusion or exclusion.
    /// </summary>
    public class PriceComponentFilter
    {
        public PriceComponentFilter(
            bool basePremium,
            bool terrorismPremium,
            bool esl,
            bool eslNsw,
            bool eslTas,
            bool premiumGst,
            bool stampDuty,
            bool stampDutyAct,
            bool stampDutyNsw,
            bool stampDutyNt,
            bool stampDutyQld,
            bool stampDutySa,
            bool stampDutyTas,
            bool stampDutyWa,
            bool stampDutyVic,
            bool commission,
            bool commissionGst,
            bool brokerFee,
            bool brokerFeeGst,
            bool underwriterFee,
            bool underwriterFeeGst,
            bool roadsideAssistanceFee,
            bool roadsideAssistanceFeeGst,
            bool policyFee,
            bool policyFeeGst,
            bool partnerFee,
            bool partnerFeeGst,
            bool administrationFee,
            bool administrationFeeGst,
            bool establishmentFee,
            bool establishmentFeeGst,
            bool paymentFee,
            bool paymentFeeGst,
            bool interest,
            bool interestGst,
            bool merchantFees,
            bool merchantFeesGst,
            bool transactionCosts,
            bool transactionCostsGst,
            bool serviceFees)
        {
            this.BasePremium = basePremium;
            this.TerrorismPremium = terrorismPremium;
            this.Esl = esl;
            this.EslNsw = eslNsw;
            this.EslTas = eslTas;
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
            this.PartnerFeeGst = partnerFeeGst;
            this.AdministrationFee = administrationFee;
            this.AdministrationFeeGst = administrationFeeGst;
            this.EstablishmentFee = establishmentFee;
            this.EstablishmentFeeGst = establishmentFeeGst;
            this.PaymentFee = paymentFee;
            this.PaymentFeeGst = paymentFeeGst;
            this.InterestCharges = interest;
            this.InterestChargesGst = interestGst;
            this.MerchantFees = merchantFees;
            this.MerchantFeesGst = merchantFeesGst;
            this.TransactionCosts = transactionCosts;
            this.TransactionCostsGst = transactionCostsGst;
            this.ServiceFees = serviceFees;
        }

        /// <summary>
        /// Gets a default filter for selecting fixed price components.
        /// </summary>
        public static PriceComponentFilter DefaultFixedComponentFilter =>
            new PriceComponentFilter(
                basePremium: false,
                terrorismPremium: false,
                esl: false,
                eslNsw: false,
                eslTas: false,
                premiumGst: false,
                stampDuty: false,
                stampDutyAct: false,
                stampDutyNsw: false,
                stampDutyNt: false,
                stampDutyQld: false,
                stampDutySa: false,
                stampDutyTas: false,
                stampDutyVic: false,
                stampDutyWa: false,
                commission: false,
                commissionGst: false,
                brokerFee: true,
                brokerFeeGst: true,
                underwriterFee: true,
                underwriterFeeGst: true,
                roadsideAssistanceFee: true,
                roadsideAssistanceFeeGst: true,
                policyFee: true,
                policyFeeGst: true,
                partnerFee: true,
                partnerFeeGst: true,
                administrationFee: true,
                administrationFeeGst: true,
                establishmentFee: true,
                establishmentFeeGst: true,
                paymentFee: true,
                paymentFeeGst: true,
                interest: true,
                interestGst: true,
                merchantFees: true,
                merchantFeesGst: true,
                transactionCosts: true,
                transactionCostsGst: true,
                serviceFees: true);

        /// <summary>
        /// Gets a default filter for excluding payment fees (i.e. merchant fees and transaction costs).
        /// </summary>
        /// <remarks>
        /// For use until payment fees are separated out of price calculations.
        /// </remarks>
        public static PriceComponentFilter ExcludePaymentFees =>
            new PriceComponentFilter(
                basePremium: true,
                terrorismPremium: true,
                esl: true,
                eslNsw: true,
                eslTas: true,
                premiumGst: true,
                stampDuty: true,
                stampDutyAct: true,
                stampDutyNsw: true,
                stampDutyNt: true,
                stampDutyQld: true,
                stampDutySa: true,
                stampDutyTas: true,
                stampDutyVic: true,
                stampDutyWa: true,
                commission: true,
                commissionGst: true,
                brokerFee: true,
                brokerFeeGst: true,
                underwriterFee: true,
                underwriterFeeGst: true,
                roadsideAssistanceFee: true, // check first if they are indeed not the same as payment fees
                roadsideAssistanceFeeGst: true,
                policyFee: true,
                policyFeeGst: true,
                partnerFee: true,
                partnerFeeGst: true,
                administrationFee: true,
                administrationFeeGst: true,
                establishmentFee: true,
                establishmentFeeGst: true, // up to here.
                paymentFee: false,
                paymentFeeGst: false,
                interest: true,
                interestGst: true,
                merchantFees: false,
                merchantFeesGst: false,
                transactionCosts: false,
                transactionCostsGst: false,
                serviceFees: true);

        /// <summary>
        /// Gets a value indicating whether to select the Base Premium.
        /// </summary>
        public bool BasePremium { get; private set; }

        /// <summary>
        /// Gets a value indicating whether to select the Terrorism Premium.
        /// </summary>
        public bool TerrorismPremium { get; private set; }

        /// <summary>
        /// Gets a value indicating whether to select the total ESL.
        /// </summary>
        public bool Esl { get; private set; }

        /// <summary>
        /// Gets a value indicating whether to select the ESL for NSW.
        /// </summary>
        public bool EslNsw { get; private set; }

        /// <summary>
        /// Gets a value indicating whether to select the ESL for TAS.
        /// </summary>
        public bool EslTas { get; private set; }

        /// <summary>
        /// Gets a value indicating whether to select the premium GST.
        /// </summary>
        public bool PremiumGst { get; private set; }

        /// <summary>
        /// Gets a value indicating whether to select the total stamp duty.
        /// </summary>
        public bool StampDuty { get; private set; }

        /// <summary>
        /// Gets a value indicating whether to select the Stamp Duty ACT.
        /// </summary>
        public bool StampDutyAct { get; private set; }

        /// <summary>
        /// Gets a value indicating whether to select the Stamp Duty NSW.
        /// </summary>
        public bool StampDutyNsw { get; private set; }

        /// <summary>
        /// Gets a value indicating whether to select the Stamp Duty NT.
        /// </summary>
        public bool StampDutyNt { get; private set; }

        /// <summary>
        /// Gets a value indicating whether to select the Stamp Duty NT.
        /// </summary>
        public bool StampDutyQld { get; private set; }

        /// <summary>
        /// Gets a value indicating whether to select the Stamp Duty NT.
        /// </summary>
        public bool StampDutySa { get; private set; }

        /// <summary>
        /// Gets a value indicating whether to select the Stamp Duty TAS.
        /// </summary>
        public bool StampDutyTas { get; private set; }

        /// <summary>
        /// Gets a value indicating whether to select the Stamp Duty WA.
        /// </summary>
        public bool StampDutyWa { get; private set; }

        /// <summary>
        /// Gets a value indicating whether to select the Stamp Duty VIC.
        /// </summary>
        public bool StampDutyVic { get; private set; }

        /// <summary>
        /// Gets a value indicating whether to select the Commission.
        /// </summary>
        public bool Commission { get; private set; }

        /// <summary>
        /// Gets a value indicating whether to select the Commission GST.
        /// </summary>
        public bool CommissionGst { get; private set; }

        /// <summary>
        /// Gets a value indicating whether to select the Broker Fee.
        /// </summary>
        public bool BrokerFee { get; private set; }

        /// <summary>
        /// Gets a value indicating whether to select the Broker Fee GST.
        /// </summary>
        public bool BrokerFeeGst { get; private set; }

        /// <summary>
        /// Gets a value indicating whether to select the Underwriter.
        /// </summary>
        public bool UnderwriterFee { get; private set; }

        /// <summary>
        /// Gets a value indicating whether to select the Underwriter GST.
        /// </summary>
        public bool UnderwriterFeeGst { get; private set; }

        /// <summary>
        /// Gets a value indicating whether to select the Roadside Assistance Fee.
        /// </summary>
        public bool RoadsideAssistanceFee { get; private set; }

        /// <summary>
        /// Gets a value indicating whether to select the Roadside Assistance Fee GST.
        /// </summary>
        public bool RoadsideAssistanceFeeGst { get; private set; }

        /// <summary>
        /// Gets a value indicating whether to select the Policy Fee.
        /// </summary>
        public bool PolicyFee { get; private set; }

        /// <summary>
        /// Gets a value indicating whether to select the Policy Fee GST.
        /// </summary>
        public bool PolicyFeeGst { get; private set; }

        /// <summary>
        /// Gets a value indicating whether to select the Partner Fee.
        /// </summary>
        public bool PartnerFee { get; private set; }

        /// <summary>
        /// Gets a value indicating whether to select the Partner Fee GST.
        /// </summary>
        public bool PartnerFeeGst { get; private set; }

        /// <summary>
        /// Gets a value indicating whether to select the administration fee.
        /// </summary>
        public bool AdministrationFee { get; private set; }

        /// <summary>
        /// Gets a value indicating whether to select the administration fee GST.
        /// </summary>
        public bool AdministrationFeeGst { get; private set; }

        /// <summary>
        /// Gets a value indicating whether to select the establishment fee.
        /// </summary>
        public bool EstablishmentFee { get; private set; }

        /// <summary>
        /// Gets a value indicating whether to select the establishment fee GST.
        /// </summary>
        public bool EstablishmentFeeGst { get; private set; }

        /// <summary>
        /// Gets a value indicating whether to select the payment fees.
        /// </summary>
        public bool PaymentFee { get; private set; }

        /// <summary>
        /// Gets a value indicating whether to select the payment fee GST.
        /// </summary>
        public bool PaymentFeeGst { get; private set; }

        /// <summary>
        /// Gets a value indicating whether to select the interest charges.
        /// </summary>
        public bool InterestCharges { get; private set; }

        /// <summary>
        /// Gets a value indiating whether to select the interest charges GST.
        /// </summary>
        public bool InterestChargesGst { get; private set; }

        /// <summary>
        /// Gets a value indicating whether to select the merchant fees.
        /// </summary>
        public bool MerchantFees { get; private set; }

        /// <summary>
        /// Gets a value indicating whether to select the merchant fees GST.
        /// </summary>
        public bool MerchantFeesGst { get; private set; }

        /// <summary>
        /// Gets a value indicating whether to select the transaction costs.
        /// </summary>
        public bool TransactionCosts { get; private set; }

        /// <summary>
        /// Gets a value indicating whether to select the transaction costs GST.
        /// </summary>
        public bool TransactionCostsGst { get; private set; }

        /// <summary>
        /// Gets a value indicating whether to select the total service fees.
        /// </summary>
        public bool ServiceFees { get; private set; }
    }
}
