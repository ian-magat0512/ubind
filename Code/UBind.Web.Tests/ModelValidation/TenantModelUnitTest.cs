// <copyright file="TenantModelUnitTest.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Tests.ModelValidation
{
    using UBind.Web.ResourceModels;
    using UBind.Web.ResourceModels.ModelHelpers;
    using Xunit;

    /// <summary>
    /// Defines the <see cref="TenantModelUnitTest" />.
    /// </summary>
    public class TenantModelUnitTest
    {
        /// <summary>
        /// The TenantModelHasValidNameAndAlias.
        /// </summary>
        [Fact]
        public void TenantModelHasValidNameAndAlias()
        {
            // Arrange
            var model = new TenantModel()
            {
                Name = "test",
                Alias = "test",
            };

            // Act
            var results = ResourceModelTestHelper.Validate(model);

            // Assert
            Assert.Equal(0, results.Count);
        }

        /// <summary>
        /// The TenantModelHasMissingNameAndValidAlias.
        /// </summary>
        [Fact]
        public void TenantModelHasMissingNameAndValidAlias()
        {
            // Arrange
            var model = new TenantModel()
            {
                Name = string.Empty,
                Alias = "test",
            };

            // Act
            var results = ResourceModelTestHelper.Validate(model);

            // Assert
            Assert.Equal(1, results.Count);
            Assert.Equal("Tenant name is required.", results[0].ErrorMessage);
        }

        /// <summary>
        /// The TenantModelHas_InvalidNameWithNoAlphanumeric_AndInvalidAliasWithUppercaseLetters.
        /// </summary>
        [Fact]
        public void TenantModelHas_InvalidNameWithNoAlphanumeric_AndInvalidAliasWithUppercaseLetters()
        {
            // Arrange
            var model = new TenantModel()
            {
                Name = " !",
                Alias = "T",
            };

            // Act
            var results = ResourceModelTestHelper.Validate(model);

            // Assert
            Assert.Equal(2, results.Count);
            Assert.Equal("Name must start with a letter or digit, and may only contain letters, digits, spaces, hyphens, apostrophes, commas and period characters.", results[0].ErrorMessage);
            Assert.Equal("Alias must only contain lowercase English letters, digits, and hyphens; and must not start or end with a hyphen.", results[1].ErrorMessage);
        }

        /// <summary>
        /// The TenantModelHas_ValidName_AndInvalidAliasWithBeginningHyphens.
        /// </summary>
        [Fact]
        public void TenantModelHas_ValidName_AndInvalidAliasWithBeginningHyphens()
        {
            // Arrange
            var model = new TenantModel()
            {
                Name = "test",
                Alias = "-test-alias",
            };

            // Act
            var results = ResourceModelTestHelper.Validate(model);

            // Assert
            Assert.Equal(1, results.Count);
            Assert.Equal("Alias must only contain lowercase English letters, digits, and hyphens; and must not start or end with a hyphen.", results[0].ErrorMessage);
        }

        /// <summary>
        /// The TenantModelHas_ValidName_AndInvalidAliasWithEndingHyphen.
        /// </summary>
        [Fact]
        public void TenantModelHas_ValidName_AndInvalidAliasWithEndingHyphen()
        {
            // Arrange
            var model = new TenantModel()
            {
                Name = "test",
                Alias = "test-alias-",
            };

            // Act
            var results = ResourceModelTestHelper.Validate(model);

            // Assert
            Assert.Equal(1, results.Count);
            Assert.Equal("Alias must only contain lowercase English letters, digits, and hyphens; and must not start or end with a hyphen.", results[0].ErrorMessage);
        }
    }
}
