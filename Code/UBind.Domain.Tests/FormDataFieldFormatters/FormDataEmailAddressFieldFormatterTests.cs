// <copyright file="FormDataEmailAddressFieldFormatterTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests.FormDataFieldFormatters
{
    using FluentAssertions;
    using UBind.Domain.FormDataFieldFormatters;
    using Xunit;

    public class FormDataEmailAddressFieldFormatterTests
    {
        [Theory]
        [InlineData("John.Doe@email.com", "john.doe@email.com")]
        [InlineData("Johnny145@EMAIL.com", "johnny145@email.com")]
        [InlineData(" asdf@asdf.com ", "asdf@asdf.com")]
        [InlineData("", "")]
        [InlineData(" ", "")]
        [InlineData(null, null)]
        [InlineData("\t", "")]
        [InlineData("\n", "")]
        public void FormDataEmailAddressFieldFormatter_Format_CorrectlyFormatsEmailAddresses(string value, string expected)
        {
            // Arrange
            var formatter = new FormDataEmailAddressFieldFormatter();

            // Act
            var result = formatter.Format(value, null);

            // Assert
            result.Should().Be(expected);
        }
    }
}
