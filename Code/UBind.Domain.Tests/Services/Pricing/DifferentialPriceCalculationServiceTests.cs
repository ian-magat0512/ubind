// <copyright file="DifferentialPriceCalculationServiceTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765


////namespace UBind.Domain.Tests.Services.Pricing
////{
////    using Xunit;
////    using NodaTime;
////    using UBind.Domain.ReadWriteModel;
////    using UBind.Domain.Services.Pricing;

////
////    public class DifferentialPriceCalculationServiceTests
////    {
////        [Fact]
////        public void CalculateDifferentialPrice_RefundsOnlyRefundablePart()
////        {
////            // Arrange
////            var originalPrice = new PriceBreakdown(100m, 0m, 0m, 0m, 10m, 0m, 0m, 0m, 100);
////            var originalRefundableFilter = new PriceComponentFilter(true, true, true, true, false, true, true, true);
////            var newPrice = new PriceBreakdown(200m, 0m, 0m, 0m, 10m, 0m, 0m, 0m, 100);
////            var newRefundableFilter = new PriceComponentFilter(true, true, true, true, false, true, true, true);
////            var sut = new DifferentialPriceCalculationService();
////            var inceptionDate = new LocalDate(2019, 1, 1);
////            var expiryDate = inceptionDate.PlusDays(100);
////            var adjustmentDate = inceptionDate.PlusDays(50);

////            // Act
////            var priceAdjustment = sut.CalculateDifferentialPrice(
////                originalPrice, originalRefundableFilter, newPrice, newRefundableFilter, inceptionDate, expiryDate, adjustmentDate);

////            // Assert
////            Assert.Equal(priceAdjustment.Difference.Payable, 60m);
////        }
////    }
////}
