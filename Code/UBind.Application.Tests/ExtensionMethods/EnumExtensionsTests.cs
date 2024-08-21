// <copyright file="EnumExtensionsTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

#pragma warning disable SA1602 // Enumeration items should be documented

namespace UBind.Application.Tests.ExtensionMethods
{
    using FluentAssertions;
    using UBind.Application.ExtensionMethods;
    using UBind.Domain;
    using UBind.Domain.Extensions;
    using Xunit;

    public class EnumExtensionsTests
    {
        internal enum Numbers
        {
            One = 1,
            Two = 2,
            Three = 3,
        }

        internal enum Nombres
        {
            Un = 1,
            Deux = 2,
            Trois = 3,
        }

        internal enum Hundreds
        {
            One = 100,
            Two = 200,
            Three = 300,
        }

        internal enum LargeNumbers : ulong
        {
            One = 1L,
            Two = 2L,
            Three = 3L,
        }

        [Fact]
        public void ConvertByName_ConvertsIntEnumsCorrectly()
        {
            // Arrange
            var input = Numbers.Two;

            // Act
            var output = input.ConvertByName<Numbers, Hundreds>();

            // Assert
            Assert.Equal(Hundreds.Two, output);
        }

        [Fact]
        public void ConvertByName_ConvertsIntEnumsToLongEnumsCorrectly()
        {
            // Arrange
            var input = Numbers.Two;

            // Act
            var output = input.ConvertByName<Numbers, LargeNumbers>();

            // Assert
            Assert.Equal(LargeNumbers.Two, output);
        }

        [Fact]
        public void ConvertByValue_ConvertsIntEnumsCorrectly()
        {
            // Arrange
            var input = Numbers.Two;

            // Act
            var output = input.ConvertByValue<Numbers, Nombres>();

            // Assert
            Assert.Equal(Nombres.Deux, output);
        }

        [Theory]
        [InlineData(EntityType.ClaimVersion, "claimVersion")]
        [InlineData(EntityType.Claim, "claim")]
        [InlineData(EntityType.PolicyTransaction, "policyTransaction")]
        [InlineData(EntityType.Portal, "portal")]
        public void ToCamelCaseString_ConvertsCorrectly_IfPascalCaseInput(EntityType entityType, string expectedValue)
        {
            // Arrange
            var input = entityType;

            // Act
            var output = input.ToCamelCaseString();

            // Assert
            output.Should().Be(expectedValue);
        }
    }
}
