// <copyright file="FormDataNumberFieldFormatterTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests.FormDataFieldFormatters
{
    using FluentAssertions;
    using UBind.Domain.FormDataFieldFormatters;
    using Xunit;

    public class FormDataNumberFieldFormatterTests
    {
        [Theory]
        [InlineData("123123", "123,123")]
        [InlineData("123123.45", "123,123.45")]
        [InlineData("12,3123.45", "123,123.45")]
        [InlineData(".45", "0.45")]
        [InlineData("1000", "1,000")]
        [InlineData("1000.001", "1,000.001")]
        [InlineData("1000.5", "1,000.50")]
        [InlineData("1000.123456789", "1,000.123456789")]
        [InlineData("", "")]
        [InlineData(" ", "")]
        [InlineData(null, null)]
        [InlineData("\t", "")]
        [InlineData("\n", "")]
        [InlineData("9223372036854775807", "9,223,372,036,854,775,807")]
        [InlineData("411111111.1111111111111111111", "411,111,111.11111111111111")]
        [InlineData("4111111111111111111111111111", "4111111111111111111111111111")]
        [InlineData("41111111111111111111111111.11111", "41,111,111,111,111,111,111,111,111.111")]
        public void FormDataNumberFieldFormatter_Format_CorrectlyFormatsNumbers(string value, string expected)
        {
            // Arrange
            var formatter = new FormDataNumberFieldFormatter();

            // Act
            var result = formatter.Format(value, null);

            // Assert
            result.Should().Be(expected);
        }
    }
}
