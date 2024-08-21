// <copyright file="IPriceBreakdown.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadWriteModel
{
    /// <summary>
    /// Interface exposing price breakdown for an insurance quote.
    /// </summary>
    public interface IPriceBreakdown
    {
        /// <summary>
        /// Gets the currency code for the price breakdown, e.g. AUD, USD, PGK, GBP, PHP.
        /// </summary>
        string CurrencyCode { get; }

        /// <summary>
        /// Gets the base premium (excluding all taxes and fees.
        /// </summary>
        decimal BasePremium { get; }

        decimal TerrorismPremium { get; }

        /// <summary>
        /// Gets the total ESL.
        /// </summary>
        decimal Esl { get; }

        decimal EmergencyServicesLevyNSW { get; }

        decimal EmergencyServicesLevyTAS { get; }

        /// <summary>
        /// Gets the total stamp duty.
        /// </summary>
        decimal StampDuty { get; }

        /// <summary>
        /// Gets the Fire Services Levy.
        /// </summary>
        decimal PremiumGst { get; }

        /// <summary>
        /// Gets the Stamp Duty ACT.
        /// </summary>
        decimal StampDutyAct { get; }

        /// <summary>
        /// Gets the Stamp Duty NSW.
        /// </summary>
        decimal StampDutyNsw { get; }

        /// <summary>
        /// Gets the Stamp Duty NT.
        /// </summary>
        decimal StampDutyNt { get; }

        /// <summary>
        /// Gets the Stamp Duty QLD.
        /// </summary>
        decimal StampDutyQld { get; }

        /// <summary>
        /// Gets the Stamp Duty SA.
        /// </summary>
        decimal StampDutySa { get; }

        /// <summary>
        /// Gets the Stamp Duty TAS.
        /// </summary>
        decimal StampDutyTas { get; }

        /// <summary>
        /// Gets the Stamp Duty VIC.
        /// </summary>
        decimal StampDutyVic { get; }

        /// <summary>
        /// Gets the Stamp Duty WA.
        /// </summary>
        decimal StampDutyWa { get; }

        /// <summary>
        /// Gets the Commission.
        /// </summary>
        decimal Commission { get; }

        /// <summary>
        /// Gets the Commission Gst.
        /// </summary>
        decimal CommissionGst { get; }

        /// <summary>
        /// Gets the Broker Fee.
        /// </summary>
        decimal BrokerFee { get; }

        /// <summary>
        /// Gets the Broker Fee Gst.
        /// </summary>
        decimal BrokerFeeGst { get; }

        /// <summary>
        /// Gets the Underwriter Fee.
        /// </summary>
        decimal UnderwriterFee { get; }

        /// <summary>
        /// Gets the Underwriter Fee GST.
        /// </summary>
        decimal UnderwriterFeeGst { get; }

        decimal RoadsideAssistanceFee { get; }

        decimal RoadsideAssistanceFeeGst { get; }

        decimal PolicyFee { get; }

        decimal PolicyFeeGst { get; }

        decimal PartnerFee { get; }

        decimal PartnerFeeGst { get; }

        decimal AdministrationFee { get; }

        decimal AdministrationFeeGst { get; }

        decimal EstablishmentFee { get; }

        decimal EstablishmentFeeGst { get; }

        /// <summary>
        /// Gets the total payable.
        /// </summary>
        decimal TotalPremium { get; }

        /// <summary>
        /// Gets the total payable.
        /// </summary>
        decimal TotalGst { get; }

        /// <summary>
        /// Gets the total payable.
        /// </summary>
        decimal StampDutyTotal { get; }

        /// <summary>
        /// Gets the total payable.
        /// </summary>
        decimal TotalPayable { get; }

        /// <summary>
        /// Gets the total service fees.
        /// </summary>
        decimal ServiceFees { get; }

        /// <summary>
        /// Gets the total interest.
        /// </summary>
        decimal InterestCharges { get; }

        decimal InterestGst { get; }

        /// <summary>
        /// Gets the total merchant fees.
        /// </summary>
        decimal MerchantFees { get; }

        decimal MerchantFeesGst { get; }

        /// <summary>
        /// Gets the total transaction costs.
        /// </summary>
        decimal TransactionCosts { get; }

        decimal TransactionCostsGst { get; }

        /// <summary>
        /// Gets the period the price covers.
        /// </summary>
        int PeriodInDays { get; }

        /// <summary>
        /// Filter out components not selected by given filter.
        /// </summary>
        /// <param name="filter">Filter specifying which components to select.</param>
        /// <returns>A new instance of <see cref="PriceBreakdown"/> with only selected components set.</returns>
        PriceBreakdown Filter(PriceComponentFilter filter);

        /// <summary>
        /// Filter a price breakdown to exclude components selected by a filter.
        /// </summary>
        /// <param name="filter">The filter specifying which components to exclude.</param>
        /// <returns>A new instance of <see cref="PriceBreakdown"/> with only non-excluded components set.</returns>
        PriceBreakdown FilterInverse(PriceComponentFilter filter);
    }
}
