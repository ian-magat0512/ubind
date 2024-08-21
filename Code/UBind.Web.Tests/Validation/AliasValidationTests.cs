// <copyright file="AliasValidationTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Tests.Validation
{
    using UBind.Web.Validation;
    using Xunit;

    /// <summary>
    /// Defines the <see cref="AliasValidationTests" />.
    /// </summary>
    public class AliasValidationTests
    {
        /// <summary>
        /// The AliasValidation_Accepts_ValidHostNames.
        /// </summary>
        /// <param name="alias">The alias<see cref="string"/>.</param>
        [Theory]
        [InlineData("foo")]
        [InlineData("foo-bar")]
        [InlineData("qoweoi-ewqij")]
        [InlineData("ewq-qwe")]
        [InlineData("tow2-qwe")]
        public void AliasValidation_Accepts_ValidHostNames(string alias)
        {
            // Arrange
            var aliasAttribute = new AliasAttribute();

            // Act + Assert
            Assert.True(aliasAttribute.IsValid(alias));
        }

        /// <summary>
        /// The AliasValidation_Rejects_InvalidHostNames.
        /// </summary>
        /// <param name="alias">The alias<see cref="string"/>.</param>
        [Theory]
        [InlineData(" ")]
        [InlineData("@")]
        [InlineData(" wo-2123")]
        [InlineData("foo.bar")]
        [InlineData("127.0.0.1")]
        [InlineData("::1")]
        [InlineData("[::1]")]
        [InlineData("2001:db8:85a3:8d3:1319:8a2e:370:7348")]
        [InlineData("[2001:db8:85a3:8d3:1319:8a2e:370:7348]")]
        public void AliasValidation_Rejects_InvalidHostNames(string alias)
        {
            // Arrange
            var aliasAttribute = new AliasAttribute();
            var valid = aliasAttribute.IsValid(alias);

            // Act + Assert
            Assert.False(valid);
        }
    }
}
