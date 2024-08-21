// <copyright file="EfundExpressInstalmentCalculatorTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Funding.EFundExpress
{
    using UBind.Application.Funding.EFundExpress;
    using Xunit;

    public class EfundExpressInstalmentCalculatorTests
    {
        [Theory]
        [InlineData(588.082, 62.93)]
        [InlineData(588.06, 62.92)]
        [InlineData(200.02, 21.4)]
        [InlineData(500.00, 53.5)]
        public void CalculateRegularInstalmentAmount_MatchesExampleFromPrincipalFinance(decimal premium, decimal expected)
        {
            // Arrange
            var interestRate = 1.07m;
            var numberOfInstalments = 10;

            // Act
            var instalmentAmount = EfundExpressInstalmentCalculator.CalculateRegularInstalmentAmount(premium, interestRate, numberOfInstalments);

            // Assert
            Assert.Equal(expected, instalmentAmount);
        }

        [Theory]
        [InlineData(588.082, 107.93)]
        [InlineData(588.06, 107.92)]
        [InlineData(200.02, 66.4)]
        [InlineData(500.00, 98.5)]
        public void CalculateFirstInstalmentAmount_MatchesExampleFromPrincipalFinance(decimal premium, decimal expected)
        {
            // Arrange
            var interestRate = 1.07m;
            var numberOfInstalments = 10;
            var fee = 45m;

            // Act
            var instalmentAmount = EfundExpressInstalmentCalculator.CalculateFirstInstalmentAmount(premium, interestRate, numberOfInstalments, fee);

            // Assert
            Assert.Equal(expected, instalmentAmount);
        }
    }
}
