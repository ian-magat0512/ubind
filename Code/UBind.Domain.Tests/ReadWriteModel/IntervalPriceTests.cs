// <copyright file="IntervalPriceTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests.ReadWriteModel
{
    using FluentAssertions;
    using NodaTime;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadWriteModel;
    using UBind.Domain.Tests.Fakes;
    using Xunit;

    public class IntervalPriceTests
    {
        [Fact]
        public void Annualize_ProRatesPricesCorrecly()
        {
            // Arrange
            var calculatedPriceComponent = CalculatedPriceComponentsFactory.Create(100m, 100m);
            var fixedComponent = new FixedAndScalablePrice(
                PriceBreakdown.CreateFromCalculationFormatV2(calculatedPriceComponent),
                PriceComponentFilter.DefaultFixedComponentFilter);
            var sut = new IntervalPrice(fixedComponent, new LocalDate(2000, 01, 01), new LocalDate(2000, 01, 31));

            // Act
            var annualizedPrice = sut.Annualize();

            // Assert
            annualizedPrice.BasePremium.RoundToWholeCents().Should().Be(1216.67m);
            annualizedPrice.BrokerFee.RoundToWholeCents().Should().Be(100m);
        }
    }
}
