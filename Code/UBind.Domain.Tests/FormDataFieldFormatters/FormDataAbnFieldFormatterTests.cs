// <copyright file="FormDataAbnFieldFormatterTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests.FormDataFieldFormatters
{
    using FluentAssertions;
    using UBind.Domain.FormDataFieldFormatters;
    using Xunit;

    public class FormDataAbnFieldFormatterTests
    {
        [Theory]
        [InlineData("12312312311", "12 312 312 311")]
        [InlineData("1 231 231 2311", "12 312 312 311")]
        [InlineData("1 2 3 1 2 3 1 2 3 1 1", "12 312 312 311")]
        [InlineData("1 2 3 123 1 2 3 1 1", "12 312 312 311")]
        [InlineData("11-222-333-444", "11 222 333 444")]
        [InlineData("01234567890", "01 234 567 890")]
        [InlineData("01234567890 ", "01 234 567 890")]
        [InlineData("", "")]
        [InlineData(" ", "")]
        [InlineData(null, null)]
        [InlineData("\t", "")]
        [InlineData("\n", "")]
        public void FormDataAbnFieldFormatter_Format_AddsSpacesInTheCorrectPosition(string value, string expected)
        {
            // Arrange
            var formatter = new FormDataAbnFieldFormatter();

            // Act
            var result = formatter.Format(value, null);

            // Assert
            result.Should().Be(expected);
        }
    }
}
