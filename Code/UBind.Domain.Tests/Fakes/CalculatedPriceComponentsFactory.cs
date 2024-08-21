// <copyright file="CalculatedPriceComponentsFactory.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests.Fakes
{
    using UBind.Domain.Extensions;

    /// <summary>
    /// Factory for price component data for tests.
    /// </summary>
    public static class CalculatedPriceComponentsFactory
    {
        /// <summary>
        /// Create a sample price from base premium, broker fee, and 10% tax and duty rates.
        /// </summary>
        /// <param name="basePremium">The base premium.</param>
        /// <param name="brokerFee">The broker fee.</param>
        /// <returns> new instance of <see cref="CalculatedPriceComponents"/>.</returns>
        public static CalculatedPriceComponents Create(decimal basePremium, decimal brokerFee)
        {
            var gst = (basePremium * 0.1m).RoundToWholeCents();
            var stampDuty = ((basePremium + gst) * 0.1m).RoundToWholeCents();
            var brokerFeeGst = (brokerFee * 0.1m).RoundToWholeCents();
            return new CalculatedPriceComponents(
                basePremium: basePremium,
                terrorismPremium: 0m,
                esl: 0m,
                eslNsw: 0m,
                eslTas: 0m,
                premiumGst: gst,
                stampDutyAct: 0m,
                stampDutyNsw: 0m,
                stampDutyNt: 0m,
                stampDutyQld: 0m,
                stampDutySa: 0m,
                stampDutyTas: 0m,
                stampDutyWa: 0m,
                stampDutyVic: stampDuty,
                commission: 0m,
                commissionGst: 0m,
                brokerFee: brokerFee,
                brokerFeeGst: brokerFeeGst,
                underwriterFee: 0m,
                underwriterFeeGst: 0m,
                roadsideAssistanceFee: 0m,
                roadsideAssistanceFeeGst: 0m,
                policyFee: 0m,
                policyFeeGst: 0m,
                partnerFee: 0m,
                partnerFeeGst: 0m,
                administrationFee: 0m,
                administrationFeeGst: 0m,
                establishmentFee: 0m,
                establishmentFeeGst: 0m,
                interest: 0m,
                interestGst: 0m,
                merchantFees: 0m,
                merchantFeesGst: 0m,
                transactionCosts: 0m,
                transactionCostsGst: 0m,
                stampDutyTotal: stampDuty,
                totalPremium: basePremium + gst + stampDuty,
                totalGst: gst + brokerFeeGst,
                totalPayable: basePremium + gst + stampDuty + brokerFee + brokerFeeGst,
                currencyCode: "AUD");
        }
    }
}
