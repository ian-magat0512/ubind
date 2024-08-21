// <copyright file="FormDataPhoneNumberFieldFormatterTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests.FormDataFieldFormatters
{
    using FluentAssertions;
    using UBind.Domain.FormDataFieldFormatters;
    using Xunit;

    public class FormDataPhoneNumberFieldFormatterTests
    {
        [Theory]
        [InlineData("0400123 123", "0400 123 123")]
        [InlineData("+613880-11234", "+61 (3) 8801-1234")]
        [InlineData("+6140-0123123", "+61 400 123 123")]
        [InlineData("039(123) 1234", "(03) 9123-1234")]
        [InlineData("1800 123123", "1800 123 123")]
        [InlineData("1300123123", "1300 123 123")]
        [InlineData("139876", "13 98 76")]
        [InlineData("", "")]
        [InlineData(" ", "")]
        [InlineData(null, null)]
        [InlineData("\t", "")]
        [InlineData("\n", "")]
        public void FormDataPhoneFieldFormatter_Format_CorrectlyFormatsPhoneNumbers(string value, string expected)
        {
            // Arrange
            var formatter = new FormDataPhoneNumberFieldFormatter();

            // Act
            var result = formatter.Format(value, null);

            // Assert
            result.Should().Be(expected);
        }
    }
}
