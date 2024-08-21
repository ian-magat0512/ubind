// <copyright file="ReleaseModelValidationUnitTest.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Tests.ModelValidation
{
    using UBind.Web.ResourceModels;
    using UBind.Web.ResourceModels.ModelHelpers;
    using UBind.Web.ResourceModels.ProductRelease;
    using Xunit;

    /// <summary>
    /// Defines the <see cref="ReleaseModelValidationUnitTest" />.
    /// </summary>
    public class ReleaseModelValidationUnitTest
    {
        /// <summary>
        /// The ReleaseModel_Validation_Success_IfValidInput.
        /// </summary>
        /// <param name="label">The label<see cref="string"/>.</param>
        /// <param name="type">The type<see cref="ReleaseType"/>.</param>
        [InlineData(
        "This is some name",
        ReleaseType.Minor)]
        [InlineData(
        "2pack",
        ReleaseType.Major)]
        [InlineData(
        "@boot @boot",
        ReleaseType.Major)]
        [Theory]
        public void ReleaseModel_Validation_Success_IfValidInput(
            string label,
            ReleaseType type)
        {
            // Arrange
            var model = new ReleaseUpsertModel()
            {
                Product = System.Guid.Empty.ToString(),
                Tenant = System.Guid.Empty.ToString(),
                Description = label,
                Type = type,
            };

            // Act
            var results = ResourceModelTestHelper.Validate(model);

            // Assert
            Assert.Equal(0, results.Count);
        }

        /// <summary>
        /// The ReleaseModel_Validation_Fail_IfInvalidInput.
        /// </summary>
        /// <param name="label">The label<see cref="string"/>.</param>
        /// <param name="type">The type<see cref="ReleaseType"/>.</param>
        [InlineData(
        "",
        ReleaseType.Minor)]
        [InlineData(
        " ",
        ReleaseType.Minor)]
        [InlineData(
        " ",
        ReleaseType.Major)]
        [Theory]
        public void ReleaseModel_Validation_Fail_IfInvalidInput(
            string label,
            ReleaseType type)
        {
            // Arrange
            var model = new ReleaseUpsertModel()
            {
                Product = System.Guid.Empty.ToString(),
                Tenant = System.Guid.Empty.ToString(),
                Description = label,
                Type = type,
            };

            // Act
            var results = ResourceModelTestHelper.Validate(model);

            // Assert
            Assert.True(results.Count >= 1);
        }
    }
}
