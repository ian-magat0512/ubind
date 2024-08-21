// <copyright file="CreditCardConverterTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests.JsonConverters
{
    using Newtonsoft.Json;
    using UBind.Domain.JsonConverters;
    using Xunit;

    public class CreditCardConverterTests
    {
        [Fact]
        public void CardNumbersOf16CharactersAreMaskedCorrectly()
        {
            // Arrange
            var creditCardNumber = "4444333322221111";
            var testObject = new TestObject(creditCardNumber);

            // Act
            var json = JsonConvert.SerializeObject(testObject);

            // Assert
            Assert.DoesNotContain(creditCardNumber, json);
            Assert.Contains("************1111", json);
        }

        [Fact]
        public void CardNumbersOf13CharactersAreMaskedCorrectly()
        {
            // Arrange
            var creditCardNumber = "4444333322221";
            var testObject = new TestObject(creditCardNumber);

            // Act
            var json = JsonConvert.SerializeObject(testObject);

            // Assert
            Assert.DoesNotContain(creditCardNumber, json);
            Assert.Contains("*********2221", json);
        }

        [Fact]
        public void CardNumbersOf4CharactersAreNotMasked()
        {
            // Arrange
            var creditCardNumber = "4444";
            var testObject = new TestObject(creditCardNumber);

            // Act
            var json = JsonConvert.SerializeObject(testObject);

            // Assert
            Assert.Contains(creditCardNumber, json);
        }

        [Fact]
        public void CardNumbersOf2CharactersAreNotMasked()
        {
            // Arrange
            var creditCardNumber = "86";
            var testObject = new TestObject(creditCardNumber);

            // Act
            var json = JsonConvert.SerializeObject(testObject);

            // Assert
            Assert.Contains(creditCardNumber, json);
        }

        [Fact]
        public void CardNumbersOf0CharactersAreNotMasked()
        {
            // Arrange
            var creditCardNumber = string.Empty;
            var testObject = new TestObject(creditCardNumber);

            // Act
            var json = JsonConvert.SerializeObject(testObject);

            // Assert
            Assert.Contains("\"\"", json);
        }

        private class TestObject
        {
            public TestObject(string creditCardNumber)
            {
                this.CreditCard = creditCardNumber;
            }

            [JsonConverter(typeof(CreditCardConverter))]
            public string CreditCard { get; }
        }
    }
}
