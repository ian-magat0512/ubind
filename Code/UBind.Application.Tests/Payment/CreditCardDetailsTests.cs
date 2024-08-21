// <copyright file="CreditCardDetailsTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Payment
{
    using UBind.Domain.Aggregates.Quote.Payment;
    using Xunit;

    /// <summary>
    /// Defines the <see cref="CreditCardDetailsTests" />.
    /// </summary>
    public class CreditCardDetailsTests
    {
        /// <summary>
        /// The ExpiryYearyyyy_Assumes21stCentury_For2DigitYears.
        /// </summary>
        [Fact]
        public void ExpiryYearyyyy_Assumes21stCentury_For2DigitYears()
        {
            // Arrange
            var sut = new CreditCardDetails("4444333322221111", "Foo", "3", 23, "123");

            // Act
            var yyyy = sut.ExpiryYearyyyy;

            // Assert
            Assert.Equal("2023", yyyy);
        }

        /// <summary>
        /// The ExpiryYearyy_TrimsCorrectly_For4DigitYears.
        /// </summary>
        [Fact]
        public void ExpiryYearyy_TrimsCorrectly_For4DigitYears()
        {
            // Arrange
            var sut = new CreditCardDetails("4444333322221111", "Foo", "3", 2023, "123");

            // Act
            var yy = sut.ExpiryYearyy;

            // Assert
            Assert.Equal("23", yy);
        }

        /// <summary>
        /// The ExpiryDateMMyy_TrimsCorrectly_For4DigitYears.
        /// </summary>
        [Fact]
        public void ExpiryDateMMyy_TrimsCorrectly_For4DigitYears()
        {
            // Arrange
            var sut = new CreditCardDetails("4444333322221111", "Foo", "03", 2023, "123");

            // Act
            var monthYear = sut.ExpiryMMyy;

            // Assert
            Assert.Equal("0323", monthYear);
        }
    }
}
