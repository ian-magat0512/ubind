// <copyright file="FormDataCurrencyFieldFormatterTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests.FormDataFieldFormatters
{
    using FluentAssertions;
    using UBind.Domain.FormDataFieldFormatters;
    using UBind.Domain.Product.Component.Form;
    using Xunit;

    public class FormDataCurrencyFieldFormatterTests
    {
        [Theory]
        [InlineData("123.12", "AUD", "$123.12")]
        [InlineData("123456.12", "AUD", "$123,456.12")]
        [InlineData("123456.12", "NZD", "$123,456.12")]
        [InlineData("123456.12", "USD", "$123,456.12")]
        [InlineData("123456.12", "GBP", "£123,456.12")]
        [InlineData("123456.12", "EUR", "€123,456.12")]
        [InlineData("123456.12", "PGK", "K123,456.12")]
        [InlineData("123,456.12", "USD", "$123,456.12")]
        [InlineData("12,3456.12", "GBP", "£123,456.12")]
        [InlineData(" 123456.12", "EUR", "€123,456.12")]
        [InlineData("K123456.12", "PGK", "K123,456.12")]
        [InlineData("$123456.12", "AUD", "$123,456.12")]
        [InlineData("$123456.005", "USD", "$123,456.01")]
        [InlineData("AUD 123456.12", "AUD", "$123,456.12")]
        [InlineData("USD123456.12", "USD", "$123,456.12")]
        [InlineData("1000.00", "USD", "$1,000")]
        [InlineData("1234.00", "AUD", "$1,234")]
        [InlineData("", "AUD", "")]
        [InlineData(" ", "AUD", "")]
        [InlineData(null, "AUD", null)]
        [InlineData("\t", "AUD", "")]
        [InlineData("\n", "AUD", "")]
        public void FormDataCurrencyFieldFormatter_Format_CorrectlyFormatsCurrency(string value, string currencyCode, string expected)
        {
            // Arrange
            var formatter = new FormDataCurrencyFieldFormatter();
            var metaData = new QuestionMetaData(null, false, false, false, false, false, false, DataType.Currency, "Currency", null, false, currencyCode);

            // Act
            var result = formatter.Format(value, metaData);

            // Assert
            result.Should().Be(expected);
        }
    }
}
