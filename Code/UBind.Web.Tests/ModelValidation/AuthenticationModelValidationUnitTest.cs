// <copyright file="AuthenticationModelValidationUnitTest.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Tests.ModelValidation
{
    using System;
    using UBind.Web.ResourceModels;
    using UBind.Web.ResourceModels.ModelHelpers;
    using Xunit;

    /// <summary>
    /// Defines the <see cref="AuthenticationModelValidationUnitTest" />.
    /// </summary>
    public class AuthenticationModelValidationUnitTest
    {
        /// <summary>
        /// The AuthenticationModel_Validation_Success_IfValidInput.
        /// </summary>
        /// <param name="emailAddress">The emailAddress<see cref="string"/>.</param>
        /// <param name="password">The password<see cref="string"/>.</param>
        [InlineData(
            "john.talavera@ubind.io",
            "ubindTest123*")]
        [InlineData(
            "ewq@gmail.com",
            "Twow232*****")]
        [InlineData(
            "jeo.talavera@gmail.com",
            "dkw2O*ddowow")]
        [Theory]
        public void AuthenticationModel_Validation_Success_IfValidInput(
           string emailAddress,
           string password)
        {
            // Arrange
            var model = new AuthenticationModel()
            {
                EmailAddress = emailAddress,
                PlaintextPassword = password,
                Tenant = Guid.Empty.ToString(),
            };

            // Act
            var results = ResourceModelTestHelper.Validate(model);

            // Assert
            Assert.Equal(0, results.Count);
        }

        /// <summary>
        /// The AuthenticationModel_Validation_Fail_IfInvalidInput.
        /// </summary>
        /// <param name="emailAddress">The emailAddress<see cref="string"/>.</param>
        /// <param name="password">The password<see cref="string"/>.</param>
        [InlineData(
            "",
            "")]
        [InlineData(
            "ewq@gma@il.com",
            "Tw**")]
        [InlineData(
            "jeo..talavera@gm@ail.com",
            "somepassword01")]
        [InlineData(
            "jeo..talavera@gm@ail.com",
            "ww@")]
        [Theory]
        public void AuthenticationModel_Validation_Fail_IfInvalidInput(
          string emailAddress,
          string password)
        {
            // Arrange
            var model = new AuthenticationModel()
            {
                EmailAddress = emailAddress,
                PlaintextPassword = password,
            };

            // Act
            var results = ResourceModelTestHelper.Validate(model);

            // Assert
            Assert.True(results.Count >= 1);
        }
    }
}
