// <copyright file="RollingNumberTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Tests.Helpers
{
    using UBind.Application.Helpers;
    using Xunit;

    /// <summary>
    /// Defines the <see cref="RollingNumberTests" />.
    /// </summary>
    public class RollingNumberTests
    {
        [Theory]
        [InlineData(99, "00", "D2", "01")]
        [InlineData(99, "98", "D2", "99")]
        [InlineData(99, "99", "D2", "01")]
        public void GetNext_ReturnsCorrectValue_WhenValuesAreValid(int maxValue, string value, string format, string? expectedValue)
        {
            // Arrange
            var rollingNumber = new RollingNumber(maxValue, value, format, false);

            // Act
            var result = rollingNumber.GetNext();

            // Assert
            Assert.Equal(expectedValue, result);
        }

        [Theory]
        [InlineData(99, "00", "D2", "99")]
        [InlineData(99, "01", "D2", "99")]
        [InlineData(99, "99", "D2", "98")]
        public void GetPrevious_ReturnsCorrectValue_WhenValuesAreValid(int maxValue, string value, string format, string? expectedValue)
        {
            // Arrange
            var rollingNumber = new RollingNumber(maxValue, value, format, false);

            // Act
            var result = rollingNumber.GetPrevious();

            // Assert
            Assert.Equal(expectedValue, result);
        }

        [Theory]
        [InlineData(999, "000", "D3", "001")]
        [InlineData(999, "999", "D3", "000")]
        public void GetNext_ReturnsCorrectZeroBasedValue_WhenValuesAreValid(int maxValue, string value, string format, string? expectedValue)
        {
            // Arrange
            var rollingNumber = new RollingNumber(maxValue, value, format, true);

            // Act
            var result = rollingNumber.GetNext();

            // Assert
            Assert.Equal(expectedValue, result);
        }

        [Theory]
        [InlineData(999, "000", "D3", "999")]
        [InlineData(999, "001", "D3", "000")]
        public void GetPrevious_ReturnsCorrectZeroBasedValue_WhenValuesAreValid(int maxValue, string value, string format, string? expectedValue)
        {
            // Arrange
            var rollingNumber = new RollingNumber(maxValue, value, format, true);

            // Act
            var result = rollingNumber.GetPrevious();

            // Assert
            Assert.Equal(expectedValue, result);
        }
    }
}
