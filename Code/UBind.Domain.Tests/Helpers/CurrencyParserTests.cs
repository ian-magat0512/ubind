// <copyright file="CurrencyParserTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests.Helpers
{
    using FluentAssertions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using Xunit;

    /// <summary>
    /// Tests the CurrencyParser.
    /// </summary>
    public class CurrencyParserTests
    {
        [Theory]
        [InlineData("USD 123.45", 123.45)]
        [InlineData("AUD123.45", 123.45)]
        [InlineData("K123.45", 123.45)]
        [InlineData("$123.45", 123.45)]
        [InlineData("AUD123,456.78", 123456.78)]
        [InlineData("-AUD123,456.78", -123456.78)]
        [InlineData("USD-123,456.78", -123456.78)]
        [InlineData("-K123.45", -123.45)]
        [InlineData("$ -123.45", -123.45)]
        public void TryParseToDecimalWithResult_CorrectlyParsesVariousFormatsToDecimal(string source, decimal expectedResult)
        {
            // Arange

            // Act
            var result = CurrencyParser.TryParseToDecimalWithResult(source);

            // Assert
            result.Should().Succeed();
            result.Value.Should().Be(expectedResult);
        }
    }
}
