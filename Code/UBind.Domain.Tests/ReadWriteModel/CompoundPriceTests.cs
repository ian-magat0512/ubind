// <copyright file="CompoundPriceTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests.ReadWriteModel
{
    using NodaTime;
    using UBind.Domain.ReadWriteModel;
    using Xunit;

    public class CompoundPriceTests
    {
        [Fact]
        public void Update_CorrectlyAppends_WhenNewIntervalStartsAtEnd()
        {
            // Arrange
            var priceBreakdown = PriceBreakdown.CreateFromCalculationFormatV1(
                 new CalculatedPaymentTotal(100m, 0m, 10m, 11m, 20m, 0m, 2m, 0m, 143m, "AUD"));
            var fixedAndScalablePrice = new FixedAndScalablePrice(priceBreakdown, PriceComponentFilter.DefaultFixedComponentFilter);
            var t0 = new LocalDate(2019, 7, 1);
            var t1 = t0.PlusDays(100);
            var intervalPrice = new IntervalPrice(fixedAndScalablePrice, t0, t1);
            var compoundPrice = new CompoundPrice(intervalPrice);
            var t2 = t1.PlusDays(100);
            var secondIntervalPrice = new IntervalPrice(fixedAndScalablePrice, t1, t2);

            // Act
            var newCompoundPrice = compoundPrice.Update(secondIntervalPrice);

            // Assert
            Assert.Equal(2, newCompoundPrice.IntervalPrices.Count);
        }

        [Fact]
        public void CalculateRefund_CorrectlyRefunds_PartialAmountFromSingleInterval()
        {
            // Arrange
            var priceBreakdown = PriceBreakdown.CreateFromCalculationFormatV1(
                new CalculatedPaymentTotal(100m, 0m, 10m, 11m, 20m, 0m, 2m, 0m, 143m, "AUD"));
            var fixedAndScalablePrice = new FixedAndScalablePrice(priceBreakdown, PriceComponentFilter.DefaultFixedComponentFilter);
            var startDate = new LocalDate(2019, 7, 1);
            var endDate = startDate.PlusDays(100);
            var intervalPrice = new IntervalPrice(fixedAndScalablePrice, startDate, endDate);
            var compoundPrice = new CompoundPrice(intervalPrice);
            var refundDate = startDate.PlusDays(50);

            // Act
            var refund = compoundPrice.CalculateRefund(refundDate);

            // Assert
            var expectedRefund = 60.5m; // (100 + 10 + 11) / 2
            Assert.Equal(expectedRefund, refund.TotalPayable);
        }

        [Fact]
        public void CalculateRefund_CorrectlyRefunds_PartialAmountFromSingleIntervalNewFormat()
        {
            // Arrange
            var priceBreakdown = PriceBreakdown.CreateFromCalculationFormatV2(
                new CalculatedPriceComponents(
                 117m, 0m, 0m, 0m, 0m, 11.7m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 12.87m, 23.40m, 2.34m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 1.93m, 0.19m, 1.64m, 0.16m, 12.87m, 141.57m, 11.7m, 145.49m, "AUD"));
            var fixedAndScalablePrice = new FixedAndScalablePrice(priceBreakdown, PriceComponentFilter.DefaultFixedComponentFilter);
            var startDate = new LocalDate(2019, 7, 1);
            var endDate = startDate.PlusDays(100);
            var intervalPrice = new IntervalPrice(fixedAndScalablePrice, startDate, endDate);
            var compoundPrice = new CompoundPrice(intervalPrice);
            var refundDate = startDate.PlusDays(55);

            // Act
            var refund = compoundPrice.CalculateRefund(refundDate);

            // Assert
            var expectedRefund = 63.71m; // (117 + 11.7 + 12.87) * (45m / 100m)
            Assert.Equal(expectedRefund, refund.TotalPayable);
        }

        [Fact]
        public void CalculateRefund_CorrectlyRefunds_PartialAmountFromSingleInterval_UsingCalculatedPriceComponents()
        {
            // Arrange
            var priceComponents = new CalculatedPriceComponents(
                    117m, 0m, 0m, 0m, 0m, 11.7m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 12.87m, 23.40m, 2.34m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 1.93m, 0.19m, 1.64m, 0.16m, 12.87m, 141.57m, 11.7m, 145.49m, "AUD");

            var priceBreakdown = PriceBreakdown.CreateFromCalculationFormatV2(priceComponents);
            var fixedAndScalablePrice = new FixedAndScalablePrice(priceBreakdown, PriceComponentFilter.DefaultFixedComponentFilter);
            var startDate = new LocalDate(2019, 7, 1);
            var endDate = startDate.PlusDays(100);
            var intervalPrice = new IntervalPrice(fixedAndScalablePrice, startDate, endDate);
            var compoundPrice = new CompoundPrice(intervalPrice);
            var refundDate = startDate.PlusDays(75);

            // Act
            var refund = compoundPrice.CalculateRefund(refundDate);

            // Assert
            var expectedRefund = 35.39m; // (117 + 11.7 + 12.87) * (25m / 100m)
            Assert.Equal(expectedRefund, refund.TotalPayable);
        }

        [Fact]
        public void CalculateRefund_CorrectlyRefunds_AmountFromPartialLastInterval()
        {
            // Arrange
            var priceBreakdown = PriceBreakdown.CreateFromCalculationFormatV1(
                new CalculatedPaymentTotal(100m, 0m, 10m, 11m, 20m, 0m, 2m, 0m, 143m, "AUD"));
            var fixedAndScalablePrice = new FixedAndScalablePrice(priceBreakdown, PriceComponentFilter.DefaultFixedComponentFilter);
            var startDate = new LocalDate(2019, 7, 1);
            var endDate = startDate.PlusDays(100);
            var intervalPrice = new IntervalPrice(fixedAndScalablePrice, startDate, endDate);
            var compoundPrice = new CompoundPrice(intervalPrice);
            var secondInterval = new IntervalPrice(fixedAndScalablePrice, endDate, endDate.PlusDays(100));
            var newCompoundPrice = compoundPrice.Update(secondInterval);
            var refundDate = startDate.PlusDays(150);

            // Act
            var refund = newCompoundPrice.CalculateRefund(refundDate);

            // Assert
            var expectedRefund = 60.5m; // (100 + 10 + 11 ) / 2
            Assert.Equal(expectedRefund, refund.TotalPayable);
        }

        [Fact]
        public void CalculateRefund_CorrectlyRefunds_AmountFromPartialLastIntervalNewFormat()
        {
            // Arrange
            var priceBreakdown = PriceBreakdown.CreateFromCalculationFormatV2(
                new CalculatedPriceComponents(
                    117m, 0m, 0m, 0m, 0m, 11.7m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 12.87m, 23.40m, 2.34m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 1.93m, 0.19m, 1.64m, 0.16m, 12.87m, 141.57m, 11.7m, 145.49m, "AUD"));
            var fixedAndScalablePrice = new FixedAndScalablePrice(priceBreakdown, PriceComponentFilter.DefaultFixedComponentFilter);
            var startDate = new LocalDate(2019, 7, 1);
            var endDate = startDate.PlusDays(100);
            var intervalPrice = new IntervalPrice(fixedAndScalablePrice, startDate, endDate);
            var compoundPrice = new CompoundPrice(intervalPrice);
            var secondInterval = new IntervalPrice(fixedAndScalablePrice, endDate, endDate.PlusDays(100));
            var newCompoundPrice = compoundPrice.Update(secondInterval);
            var refundDate = startDate.PlusDays(150);

            // Act
            var refund = newCompoundPrice.CalculateRefund(refundDate);

            // Assert
            var expectedRefund = 70.78m; // (117 + 11.7 + 12.87) / 2
            Assert.Equal(expectedRefund, refund.TotalPayable);
        }

        [Fact]
        public void CalculateRefund_CorrectlyRefunds_AmountFromWholeLastInterval()
        {
            // Arrange
            var priceBreakdown = PriceBreakdown.CreateFromCalculationFormatV1(
                new CalculatedPaymentTotal(100m, 0m, 10m, 11m, 20m, 0m, 2m, 0m, 143m, "AUD"));
            var fixedAndScalablePrice = new FixedAndScalablePrice(priceBreakdown, PriceComponentFilter.DefaultFixedComponentFilter);
            var startDate = new LocalDate(2019, 7, 1);
            var endDate = startDate.PlusDays(100);
            var intervalPrice = new IntervalPrice(fixedAndScalablePrice, startDate, endDate);
            var compoundPrice = new CompoundPrice(intervalPrice);
            var secondInterval = new IntervalPrice(fixedAndScalablePrice, endDate, endDate.PlusDays(100));
            var newCompoundPrice = compoundPrice.Update(secondInterval);
            var refundDate = endDate;

            // Act
            var refund = newCompoundPrice.CalculateRefund(refundDate);

            // Assert
            var expectedRefund = 121m; // (100 + 10 + 11 )
            Assert.Equal(expectedRefund, refund.TotalPayable);
        }

        [Fact]
        public void CalculateRefund_CorrectlyRefunds_AmountFromWholeLastIntervalNewFormat()
        {
            // Arrange
            var priceBreakdown = PriceBreakdown.CreateFromCalculationFormatV2(
                new CalculatedPriceComponents(
                    117m, 0m, 0m, 0m, 0m, 11.7m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 12.87m, 23.40m, 2.34m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 1.93m, 0.19m, 1.64m, 0.16m, 12.87m, 141.57m, 11.7m, 145.49m, "AUD"));
            var fixedAndScalablePrice = new FixedAndScalablePrice(priceBreakdown, PriceComponentFilter.DefaultFixedComponentFilter);
            var startDate = new LocalDate(2019, 7, 1);
            var endDate = startDate.PlusDays(100);
            var intervalPrice = new IntervalPrice(fixedAndScalablePrice, startDate, endDate);
            var compoundPrice = new CompoundPrice(intervalPrice);
            var secondInterval = new IntervalPrice(fixedAndScalablePrice, endDate, endDate.PlusDays(100));
            var newCompoundPrice = compoundPrice.Update(secondInterval);
            var refundDate = endDate;

            // Act
            var refund = newCompoundPrice.CalculateRefund(refundDate);

            // Assert
            var expectedRefund = 141.57m; // (117 + 11.7 + 12.87)
            Assert.Equal(expectedRefund, refund.TotalPayable);
        }

        [Fact]
        public void CalculateRefund_CorrectlyRefunds_CancelledTest()
        {
            // Arrange
            var priceBreakdown = PriceBreakdown.CreateFromCalculationFormatV2(
                new CalculatedPriceComponents(
                    117m, 0m, 0m, 0m, 0m, 11.7m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 12.87m, 23.40m, 2.34m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 1.93m, 0.19m, 1.64m, 0.16m, 12.87m, 141.57m, 11.7m, 145.49m, "AUD"));
            var fixedAndScalablePrice = new FixedAndScalablePrice(priceBreakdown, PriceComponentFilter.DefaultFixedComponentFilter);
            var startDate = new LocalDate(2019, 7, 1);
            var endDate = startDate.PlusDays(100);
            var intervalPrice = new IntervalPrice(fixedAndScalablePrice, startDate, endDate);
            var compoundPrice = new CompoundPrice(intervalPrice);
            var refundDate = endDate.PlusDays(-30);

            // Act
            var refund = compoundPrice.CalculateRefund(refundDate);

            // Assert
            decimal expectedRefund = 42.47m; // ((117m + 11.7m + 12.87m) * (30m / 100m)
            Assert.Equal(expectedRefund, refund.TotalPayable);
        }
    }
}
