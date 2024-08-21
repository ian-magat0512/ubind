// <copyright file="ModelValidationHelperTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Tests.Validation
{
    using UBind.Web.Validation;
    using Xunit;

    /// <summary>
    /// Defines the <see cref="ModelValidationHelperTests" />.
    /// </summary>
    public class ModelValidationHelperTests
    {
        /// <summary>
        /// The EmailListValidationAttribute_Matched_ValidEmailLists.
        /// </summary>
        /// <param name="validEmailList">The validEmailList<see cref="string"/>.</param>
        [Theory]
        [InlineData("a@a.com")]
        [InlineData("a@a.com, a@a.com")]
        [InlineData("a@a")]
        [InlineData("a@a, a@a")]
        [InlineData("a@a, a@a, a@a")]
        [InlineData("a@a; a@a")]
        public void EmailListValidationAttribute_Matched_ValidEmailLists(string validEmailList)
        {
            // Arrange
            var emailListValidation = new EmailListValidationAttribute();

            // Act + Assert
            Assert.True(emailListValidation.IsValid(validEmailList));
        }

        /// <summary>
        /// The EmailListValidationAttribute_DoesNotMatch_InvalidEmailLists.
        /// </summary>
        /// <param name="invalidEmailList">The invalidEmailList<see cref="string"/>.</param>
        [Theory]
        [InlineData("a@")]
        [InlineData("a@a, a@")]
        [InlineData("a@a, a@a, a@")]
        [InlineData("a@a, a@a, 1")]
        public void EmailListValidationAttribute_DoesNotMatch_InvalidEmailLists(string invalidEmailList)
        {
            ////// Arrange
            var emailListValidation = new EmailListValidationAttribute();

            // Act + Assert
            Assert.False(emailListValidation.IsValid(invalidEmailList));
        }
    }
}
