// <copyright file="FormDataPercentFieldFormatterTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests.FormDataFieldFormatters
{
    using FluentAssertions;
    using UBind.Domain.FormDataFieldFormatters;
    using Xunit;

    public class FormDataPercentFieldFormatterTests
    {
        [Theory]
        [InlineData("100%", "100%")]
        [InlineData("100", "100%")]
        [InlineData("-10", "-10%")]
        [InlineData("99.5", "99.5%")]
        [InlineData(".45", "0.45%")]
        [InlineData("1000", "1,000%")]
        [InlineData("1000.44%", "1,000.44%")]
        [InlineData("", "")]
        [InlineData(" ", "")]
        [InlineData(null, null)]
        [InlineData("\t", "")]
        [InlineData("\n", "")]
        [InlineData("NIL", "0%")]
        [InlineData("nil", "0%")]
        [InlineData("invalid", "invalid")]
        public void FormDataPercentFieldFormatter_Format_CorrectlyFormatsPercentages(string value, string expected)
        {
            // Arrange
            var formatter = new FormDataPercentFieldFormatter();

            // Act
            var result = formatter.Format(value, null);

            // Assert
            result.Should().Be(expected);
        }
    }
}
