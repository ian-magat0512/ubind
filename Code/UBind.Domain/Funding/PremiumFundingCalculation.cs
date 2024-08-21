// <copyright file="PremiumFundingCalculation.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.Funding;

/// <summary>
/// Keeps all the calculation related to premium funding instalment amount that will be charge to the customer.
/// This means the amount to be returned should include all charges/fees
/// that may not be initially part of the original amount
/// </summary>
public class PremiumFundingCalculation
{
    /// <summary>
    /// Creates a new instance of <see cref="PremiumFundingCalculation"/>
    /// </summary>
    /// <param name="numberOfInstalments">The number of instalments configured for the premium funding provider</param>
    /// <param name="amountFunded">The total amount funded by the premium funding provider. This excludes any additional charges/fee from the premium funding provider</param>
    /// <param name="regularInstalmentAmount">The regular instalment amount for each payment frequency</param>
    /// <param name="initialInstalmentAmount">The initial instalment that includes regular amount and any admin fee</param>
    /// <param name="instalmentMerchantFeeMultiplier">The merchant fee multiplier that applies to each instalment charges</param>
    /// <param name="interestRate">The interest rate applied by the funding provider for instalments</param>
    public PremiumFundingCalculation(
        int numberOfInstalments,
        decimal amountFunded,
        decimal regularInstalmentAmount,
        decimal initialInstalmentAmount,
        decimal instalmentMerchantFeeMultiplier,
        decimal interestRate)
    {
        var multiplier = 1 + instalmentMerchantFeeMultiplier;
        this.InitialInstalment = initialInstalmentAmount * multiplier;
        this.RegularInstalment = regularInstalmentAmount * multiplier;
        this.TotalInstalment = this.InitialInstalment + ((numberOfInstalments - 1) * this.RegularInstalment);
        this.Interest = amountFunded * interestRate;
    }

    public decimal InitialInstalment { get; }

    public decimal RegularInstalment { get; }

    public decimal TotalInstalment { get; }

    public decimal Interest { get; }
}
