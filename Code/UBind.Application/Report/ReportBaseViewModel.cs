// <copyright file="ReportBaseViewModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Report
{
    using DotLiquid;
    using Newtonsoft.Json;
    using UBind.Application.Payment.Deft;
    using UBind.Domain.Enums;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// Base class for report view model.
    /// </summary>
    public class ReportBaseViewModel : Drop
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReportBaseViewModel"/> class.
        /// </summary>
        /// <param name="reportReadModel">The report read model.</param>
        public ReportBaseViewModel(IBaseReportReadModel reportReadModel)
        {
            var calculationResult = new CalculationResultReadModel(reportReadModel.SerializedLatestCalculationResult);
            var payablePrice = calculationResult.PayablePrice;
            this.BasePremium = payablePrice.BasePremium.ToDollarsAndCents();
            this.TerrorismPremium = payablePrice.TerrorismPremium.ToDollarsAndCents();
            this.EmergencyServicesLevyNsw = payablePrice.EmergencyServicesLevyNSW.ToDollarsAndCents();
            this.EmergencyServicesLevyTas = payablePrice.EmergencyServicesLevyTAS.ToDollarsAndCents();
            this.Esl = payablePrice.Esl.ToDollarsAndCents();
            this.PremiumGst = payablePrice.PremiumGst.ToDollarsAndCents();
            this.StampDutyAct = payablePrice.StampDutyAct.ToDollarsAndCents();
            this.StampDutyNsw = payablePrice.StampDutyNsw.ToDollarsAndCents();
            this.StampDutyNt = payablePrice.StampDutyNt.ToDollarsAndCents();
            this.StampDutyQld = payablePrice.StampDutyQld.ToDollarsAndCents();
            this.StampDutySa = payablePrice.StampDutySa.ToDollarsAndCents();
            this.StampDutyTas = payablePrice.StampDutyTas.ToDollarsAndCents();
            this.StampDutyVic = payablePrice.StampDutyVic.ToDollarsAndCents();
            this.StampDutyWa = payablePrice.StampDutyWa.ToDollarsAndCents();
            this.StampDutyTotal = payablePrice.StampDutyTotal.ToDollarsAndCents();
            this.TotalPremium = payablePrice.TotalPremium.ToDollarsAndCents();
            this.Commission = payablePrice.Commission.ToDollarsAndCents();
            this.CommissionGst = payablePrice.CommissionGst.ToDollarsAndCents();
            this.BrokerFee = payablePrice.BrokerFee.ToDollarsAndCents();
            this.BrokerFeeGst = payablePrice.BrokerFeeGst.ToDollarsAndCents();
            this.UnderwriterFee = payablePrice.UnderwriterFee.ToDollarsAndCents();
            this.UnderwriterFeeGst = payablePrice.UnderwriterFeeGst.ToDollarsAndCents();
            this.RoadsideAssistanceFee = payablePrice.RoadsideAssistanceFee.ToDollarsAndCents();
            this.RoadsideAssistanceFeeGst = payablePrice.RoadsideAssistanceFeeGst.ToDollarsAndCents();
            this.PolicyFee = payablePrice.PolicyFee.ToDollarsAndCents();
            this.PolicyFeeGst = payablePrice.PolicyFeeGst.ToDollarsAndCents();
            this.PartnerFee = payablePrice.PartnerFee.ToDollarsAndCents();
            this.PartnerFeeGst = payablePrice.PartnerFeeGst.ToDollarsAndCents();
            this.AdministrationFee = payablePrice.AdministrationFee.ToDollarsAndCents();
            this.AdministrationFeeGst = payablePrice.AdministrationFeeGst.ToDollarsAndCents();
            this.EstablishmentFee = payablePrice.EstablishmentFee.ToDollarsAndCents();
            this.EstablishmentFeeGst = payablePrice.EstablishmentFeeGst.ToDollarsAndCents();
            this.MerchantFees = payablePrice.MerchantFees.ToDollarsAndCents();
            this.MerchantFeesGst = payablePrice.MerchantFeesGst.ToDollarsAndCents();
            this.TransactionCosts = payablePrice.TransactionCosts.ToDollarsAndCents();
            this.TransactionCostsGst = payablePrice.TransactionCostsGst.ToDollarsAndCents();
            this.TotalGst = payablePrice.TotalGst.ToDollarsAndCents();
            this.TotalPayable = payablePrice.TotalPayable.ToDollarsAndCents();

            if (!string.IsNullOrWhiteSpace(reportReadModel.PaymentGateway) &&
                !string.IsNullOrWhiteSpace(reportReadModel.PaymentResponseJson) &&
                reportReadModel.PaymentGateway.EqualsIgnoreCase(nameof(PaymentGatewayName.Deft)))
            {
                var settings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    MissingMemberHandling = MissingMemberHandling.Ignore,
                };

                this.DeftPaymentResponse = JsonConvert.DeserializeObject<DeftPaymentResponse>(reportReadModel.PaymentResponseJson, settings);
            }
        }

        /// <summary>
        /// Gets the base premium for all risks (excluding all taxes and fees).
        /// </summary>
        public string BasePremium { get; }

        public string TerrorismPremium { get; }

        public string EmergencyServicesLevyNsw { get; }

        public string EmergencyServicesLevyTas { get; }

        /// <summary>
        /// Gets  the total Emergency Services Levy (ESL).
        /// </summary>
        public string Esl { get; }

        /// <summary>
        /// Gets the GST payable on the base premium and ESL.
        /// </summary>
        public string PremiumGst { get; }

        /// <summary>
        /// Gets the Stamp Duty payable to the ACT.
        /// </summary>
        public string StampDutyAct { get; }

        /// <summary>
        /// Gets the Stamp Duty payable to NSW.
        /// </summary>
        public string StampDutyNsw { get; }

        /// <summary>
        /// Gets the Stamp Duty payable to the NT.
        /// </summary>
        public string StampDutyNt { get; }

        /// <summary>
        /// Gets the Stamp Duty payable to QLD.
        /// </summary>
        public string StampDutyQld { get; }

        /// <summary>
        /// Gets the Stamp Duty payable to SA.
        /// </summary>
        public string StampDutySa { get; }

        /// <summary>
        /// Gets the Stamp Duty payable to TAS.
        /// </summary>
        public string StampDutyTas { get; }

        /// <summary>
        /// Gets  the Stamp Duty payable to VIC.
        /// </summary>
        public string StampDutyVic { get; }

        /// <summary>
        /// Gets the Stamp Duty payable to WA.
        /// </summary>
        public string StampDutyWa { get; }

        /// <summary>
        /// Gets the total Stamp Duty.
        /// </summary>
        public string StampDutyTotal { get; }

        /// <summary>
        /// Gets the total premium.
        /// </summary>
        public string TotalPremium { get; }

        /// <summary>
        /// Gets the commisssion payable to the broker (already incuded in base premium).
        /// </summary>
        public string Commission { get; }

        /// <summary>
        /// Gets the GST on the commisssion payable to the broker (already included in premium GST).
        /// </summary>
        public string CommissionGst { get; }

        /// <summary>
        /// Gets the broker fee.
        /// </summary>
        public string BrokerFee { get; }

        /// <summary>
        /// Gets the GST payable on the broker fee.
        /// </summary>
        public string BrokerFeeGst { get; }

        /// <summary>
        /// Gets the underwriter fee.
        /// </summary>
        public string UnderwriterFee { get; }

        /// <summary>
        /// Gets the GST payable on the underwriter fee.
        /// </summary>
        public string UnderwriterFeeGst { get; }

        /// <summary>
        /// Gets the roadside assistance fee.
        /// </summary>
        public string RoadsideAssistanceFee { get; }

        /// <summary>
        /// Gets the GST payable on the roadside assistance fee.
        /// </summary>
        public string RoadsideAssistanceFeeGst { get; }

        /// <summary>
        /// Gets the policy fee.
        /// </summary>
        public string PolicyFee { get; }

        /// <summary>
        /// Gets the GST payable on the policy fee.
        /// </summary>
        public string PolicyFeeGst { get; }

        /// <summary>
        /// Gets the partner fee.
        /// </summary>
        public string PartnerFee { get; }

        /// <summary>
        /// Gets the GST payable on the partner fee.
        /// </summary>
        public string PartnerFeeGst { get; }

        /// <summary>
        /// Gets the administration fee.
        /// </summary>
        public string AdministrationFee { get; }

        /// <summary>
        /// Gets the GST payable on the administration fee.
        /// </summary>
        public string AdministrationFeeGst { get; }

        /// <summary>
        /// Gets the establishment fee.
        /// </summary>
        public string EstablishmentFee { get; }

        /// <summary>
        /// Gets the GST payable on the establishment fee
        /// </summary>
        public string EstablishmentFeeGst { get; }

        /// <summary>
        /// Gets the merchant fees.
        /// </summary>
        public string MerchantFees { get; }

        /// <summary>
        /// Gets the GST payable on the merchant fees.
        /// </summary>
        public string MerchantFeesGst { get; }

        /// <summary>
        /// Gets the transaction costs.
        /// </summary>
        public string TransactionCosts { get; }

        /// <summary>
        /// Gets the GST payable on the transaction costs.
        /// </summary>
        public string TransactionCostsGst { get; }

        /// <summary>
        /// Gets the total gst.
        /// </summary>
        public string TotalGst { get; }

        /// <summary>
        /// Gets the total payable.
        /// </summary>
        public string TotalPayable { get; }

        /// <summary>
        /// Gets the deft payment response.
        /// </summary>
        public DeftPaymentResponse DeftPaymentResponse { get; }
    }
}
