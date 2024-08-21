// <copyright file="NumberValidationTest.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Tests.Validation
{
    using UBind.Web.Validation;
    using Xunit;

    /// <summary>
    /// Defines the <see cref="NumberValidationTest" />.
    /// </summary>
    public class NumberValidationTest
    {
        /// <summary>
        /// The MobileNumberValidation_Accepts_Valid.
        /// </summary>
        /// <param name="value">The value<see cref="string"/>.</param>
        [Theory]
        [InlineData("0412345678")]
        [InlineData("")]
        [InlineData("04 0000 0000")]
        [InlineData("+61 4 0000 0000")]
        [InlineData("05 0000 0000")]
        [InlineData("+61 5 0000 0000")]
        [InlineData("+61 5 0000 0010")]
        public void MobileNumberValidation_Accepts_Valid(string value)
        {
            // Arrange
            var attribute = new AustralianMobileNumberAttribute();

            // Act + Assert
            Assert.True(attribute.IsValid(value));
        }

        /// <summary>
        /// The MobileNumberValidation_Rejects_Invalid.
        /// </summary>
        /// <param name="value">The value<see cref="string"/>.</param>
        [Theory]
        [InlineData("0412345678a")]
        [InlineData(" ")]
        [InlineData("dsadswqw-w")]
        [InlineData("04 0000 0a000")]
        [InlineData("05-0000-0000")]
        public void MobileNumberValidation_Rejects_Invalid(string value)
        {
            // Arrange
            var attribute = new AustralianMobileNumberAttribute();

            // Act + Assert
            Assert.False(attribute.IsValid(value));
        }

        /// <summary>
        /// The PhoneNumberValidation_Accepts_Valid.
        /// </summary>
        /// <param name="value">The value<see cref="string"/>.</param>
        [Theory]
        [InlineData("0412345678")]
        [InlineData("")]
        [InlineData("04 0000 0000")]
        [InlineData("+61 4 0000 0000")]
        [InlineData("05 0000 0000")]
        [InlineData("+61 5 0000 0000")]
        [InlineData("+61 0 0000 0000")]
        [InlineData("0400000000")]
        [InlineData("+61400000000")]
        public void PhoneNumberValidation_Accepts_Valid(string value)
        {
            // Arrange
            var attribute = new AustralianPhoneNumberAttribute();

            // Act + Assert
            Assert.True(attribute.IsValid(value));
        }

        /// <summary>
        /// The PhoneNumberValidation_Rejects_Invalid.
        /// </summary>
        /// <param name="value">The value<see cref="string"/>.</param>
        [Theory]
        [InlineData("0412345678a")]
        [InlineData(" ")]
        [InlineData("dsadswqw-w")]
        [InlineData("04 0000 0a000")]
        [InlineData("05-0000-0000")]
        [InlineData("     s     ")]
        [InlineData("04 0000 0a00")]
        [InlineData("+61 4 0000-0000")]
        [InlineData("05 0000 0s00")]
        [InlineData("+61 5 @000 0000")]
        [InlineData("+61 0 00() 0000")]
        public void PhoneNumberValidation_Rejects_Invalid(string value)
        {
            // Arrange
            var attribute = new AustralianPhoneNumberAttribute();

            // Act + Assert
            Assert.False(attribute.IsValid(value));
        }
    }
}
