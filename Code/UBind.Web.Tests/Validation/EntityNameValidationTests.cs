// <copyright file="EntityNameValidationTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Tests.Validation
{
    using FluentAssertions;
    using UBind.Web.Validation;
    using Xunit;

    /// <summary>
    /// Defines the <see cref="EntityNameValidationTests" />.
    /// </summary>
    public class EntityNameValidationTests
    {
        /// <summary>
        /// The AliasValidation_Accepts_ValidHostNames.
        /// </summary>
        /// <param name="entityName">The alias<see cref="string"/>.</param>
        [Theory]
        [InlineData("Brokers Pty Ltd")]
        [InlineData("You, Me and Dupreee")]
        [InlineData("ABC Brokers Pty. Ltd.")]
        [InlineData("Mick's Flicks")]
        [InlineData("Baden-Powell")]
        [InlineData("Broker123")]
        [InlineData("Broker123broker")]
        public void EntityNameValidation_Accepts_ValidEntityNames(string entityName)
        {
            // Arrange
            var entityNameAttribute = new EntityNameAttribute();

            // Act
            var isValid = entityNameAttribute.IsValid(entityName);

            // Assert
            isValid.Should().BeTrue($"\"{entityName}\"" + " should have been considered valid but it wasn't.");
        }

        /// <summary>
        /// The AliasValidation_Rejects_InvalidHostNames.
        /// </summary>
        /// <param name="entityName">The alias<see cref="string"/>.</param>
        [Theory]
        [InlineData(" wo-2123")]
        [InlineData("-Craig")]
        [InlineData("'Shaz")]
        [InlineData(",Foo")]
        [InlineData(".Bar")]
        [InlineData("*")]
        [InlineData("A!")]
        [InlineData("A@")]
        [InlineData("B#")]
        [InlineData("C$")]
        [InlineData("D%")]
        [InlineData("E^")]
        [InlineData("F&")]
        [InlineData("G*")]
        [InlineData("H(")]
        [InlineData("I)*")]
        [InlineData("J+")]
        [InlineData("K=")]
        [InlineData("L{")]
        [InlineData("M}")]
        [InlineData("N|")]
        [InlineData("O:")]
        [InlineData("P;")]
        [InlineData("Q\"")]
        [InlineData("R?")]
        [InlineData("S/")]
        [InlineData("T\\")]
        [InlineData("U~")]
        public void EntityNameValidation_Rejects_InvalidEntityNames(string entityName)
        {
            // Arrange
            var entityNameAttribute = new EntityNameAttribute();

            // Act
            var isValid = entityNameAttribute.IsValid(entityName);

            // Assert
            isValid.Should().BeFalse($"\"{entityName}\"" + " should have been considered invalid but it wasn't.");
        }
    }
}
