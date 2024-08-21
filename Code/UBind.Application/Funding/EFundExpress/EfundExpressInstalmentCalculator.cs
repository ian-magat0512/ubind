// <copyright file="EfundExpressInstalmentCalculator.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Funding.EFundExpress
{
    using UBind.Domain.Extensions;

    /// <summary>
    /// Helper for calculating instalments for Principal Finance's premium funding.
    /// </summary>
    public static class EfundExpressInstalmentCalculator
    {
        /// <summary>
        /// Calculate the amount of the regular instalments.
        /// </summary>
        /// <param name="premium">The premium being funded.</param>
        /// <param name="interestRate">The interest rate.</param>
        /// <param name="numberOfInstalments">The total number of instalments (including the first instalment).</param>
        /// <returns>The amount payable in each instalment after the first one.(exluding card surcharges).</returns>
        public static decimal CalculateRegularInstalmentAmount(decimal premium, decimal interestRate, int numberOfInstalments)
        {
            var premiumAndInterest = (premium * interestRate).RoundTo2DecimalPlacesAwayFromZero();
            var instalmentAmount = (premiumAndInterest / numberOfInstalments).RoundTo2DecimalPlacesAwayFromZero();
            return instalmentAmount;
        }

        /// <summary>
        /// Calculate the amount of the first instalment.
        /// </summary>
        /// <param name="premium">The premium being funded.</param>
        /// <param name="interestRate">The interest rate.</param>
        /// <param name="numberOfInstalments">The total number of instalments (including the first instalment).</param>
        /// <param name="fee">The fee included in the first instalment.</param>
        /// <returns>The amount payable in the first instalment.(exluding card surcharges).</returns>
        public static decimal CalculateFirstInstalmentAmount(decimal premium, decimal interestRate, int numberOfInstalments, decimal fee)
        {
            var regularInstalmentAmount = CalculateRegularInstalmentAmount(premium, interestRate, numberOfInstalments);
            var firstInstalmentAmount = regularInstalmentAmount + fee;
            return firstInstalmentAmount;
        }

        /// <summary>
        /// Calculate the cost to fund.
        /// </summary>
        /// <param name="initialInstallment">The initial installment amount.</param>
        /// <param name="regularInstallment">The regular installment amount.</param>
        /// <param name="numberOfInstallments">The total number of instalments (including the first instalment).</param>
        /// <returns>The total funding amount.</returns>
        public static decimal CalculateAmountFunded(
            decimal initialInstallment,
            decimal regularInstallment,
            decimal numberOfInstallments)
        {
            decimal fundingAmount = initialInstallment + (regularInstallment * (numberOfInstallments - 1));
            return fundingAmount.RoundTo2DecimalPlacesAwayFromZero();
        }
    }
}
