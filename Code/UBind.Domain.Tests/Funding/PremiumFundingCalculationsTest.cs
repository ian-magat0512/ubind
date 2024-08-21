// <copyright file="PremiumFundingCalculationsTest.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.Tests.Funding;

using FluentAssertions;
using UBind.Domain.Funding;
using Xunit;

public class PremiumFundingCalculationsTest
{

    [Fact]
    public void PremiumFundingCalculations_ShouldCalculateCorrectAmounts()
    {
        // Arrange
        int numberOfInstalments = 12;
        decimal amountToFund = 2440m;
        decimal applicationFee = 60.0m;
        decimal instalment = 2440m / numberOfInstalments;
        decimal initialInstalment = instalment + applicationFee;
        decimal multiplier = 0.0079m;
        decimal interestRate = 0.15m;

        // Act
        var calculations = new PremiumFundingCalculation(
            numberOfInstalments,
            amountToFund,
            instalment,
            initialInstalment,
            multiplier,
            interestRate);

        // Assert
        var expectedInstalmentWithMultiplier = instalment * (1 + multiplier);
        calculations.RegularInstalment.Should().Be(expectedInstalmentWithMultiplier);

        var expectedInitialInstalmentWithMultiplier = initialInstalment * (1 + multiplier);
        calculations.InitialInstalment.Should().Be(expectedInitialInstalmentWithMultiplier);

        var expectedInterest = amountToFund * interestRate;
        calculations.Interest.Should().Be(expectedInterest);
    }
}
