// <copyright file="FormDataBooleanFieldFormatterTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests.FormDataFieldFormatters
{
    using FluentAssertions;
    using UBind.Domain.FormDataFieldFormatters;
    using Xunit;

    public class FormDataBooleanFieldFormatterTests
    {
        [Theory]
        [InlineData("True", "Yes")]
        [InlineData("false", "No")]
        [InlineData("yes", "Yes")]
        [InlineData("No", "No")]
        [InlineData("True ", "Yes")]
        [InlineData(" false", "No")]
        [InlineData("yes ", "Yes")]
        [InlineData(" No", "No")]
        [InlineData(" ", "")]
        [InlineData("", "")]
        [InlineData(null, null)]
        [InlineData("\t", "")]
        [InlineData("\n", "")]
        public void FormDataBooleanFieldFormatter_Format_CorrectlyInterpretsBooleanValue(string value, string expected)
        {
            // Arrange
            var formatter = new FormDataBooleanFieldFormatter();

            // Act
            var result = formatter.Format(value, null);

            // Assert
            result.Should().Be(expected);
        }
    }
}
