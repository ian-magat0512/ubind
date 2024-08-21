// <copyright file="PortNumberValidationTest.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Tests.Validation
{
    using UBind.Web.Validation;
    using Xunit;

    /// <summary>
    /// Defines the <see cref="PortNumberValidationTest" />.
    /// </summary>
    public class PortNumberValidationTest
    {
        /// <summary>
        /// The PortNumberValidation_Accepts_ValidPortNumber.
        /// </summary>
        /// <param name="portNumber">The portNumber<see cref="int"/>.</param>
        [Theory]
        [InlineData(1)]
        [InlineData(65535)]
        [InlineData(80)]
        [InlineData(8080)]
        [InlineData(81)]
        public void PortNumberValidation_Accepts_ValidPortNumber(int portNumber)
        {
            // Arrange
            var portNumberValidationAttribute = new PortNumberValidationAttribute();

            // Act + Assert
            Assert.True(portNumberValidationAttribute.IsValid(portNumber));
        }

        /// <summary>
        /// The PortNumberValidation_Rejects_InValidPortNumber.
        /// </summary>
        /// <param name="portNumber">The portNumber<see cref="int"/>.</param>
        [Theory]
        [InlineData(0)]
        [InlineData(65536)]
        [InlineData(-12)]
        public void PortNumberValidation_Rejects_InValidPortNumber(int portNumber)
        {
            // Arrange
            var portNumberValidationAttribute = new PortNumberValidationAttribute();

            // Act + Assert
            Assert.False(portNumberValidationAttribute.IsValid(portNumber));
        }
    }
}
