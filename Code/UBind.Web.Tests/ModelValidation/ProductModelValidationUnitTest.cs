// <copyright file="ProductModelValidationUnitTest.cs" company="uBind">
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
    /// Defines the <see cref="ProductModelValidationUnitTest" />.
    /// </summary>
    public class ProductModelValidationUnitTest
    {
        /// <summary>
        /// The ProductModel_Validation_Success_IfValidInput.
        /// </summary>
        /// <param name="id">The id<see cref="string"/>.</param>
        /// <param name="name">The name<see cref="string"/>.</param>
        /// <param name="alias">The alias<see cref="string"/>.</param>
        [InlineData(
        "product-id",
        "This is some name",
        "product-alias")]
        [InlineData(
        "d",
        "name-Sake",
        "a")]
        [InlineData(
        "pro-duct-id",
        "boot boot -'. ,",
        "product-alias")]
        [InlineData(
        "product-001",
        "Product 001",
        "product-001")]
        [InlineData(
        "product-001-product",
        "Product 001 product",
        "product-001-product")]
        [InlineData(
        "product001-product",
        "Product001 product",
        "product001-product")]
        [InlineData(
        "001-product",
        "001-product",
        "001-product")]
        [InlineData(
        "1234a",
        "1234a",
        "1234a")]
        [InlineData(
        "1234-a",
        "1234-a",
        "1234-a")]
        [InlineData(
        "Product123",
        "Product123",
        "product123")]
        [InlineData(
        "Product123",
        "Product.'123",
        "product-123")]
        [InlineData(
        "Product123",
        "1a",
        "1a")]
        [Theory]
        public void ProductModel_Validation_Success_IfValidInput(
            string id,
            string name,
            string alias)
        {
            // Arrange
            var model = new ProductCreateRequestModel()
            {
                Id = id,
                Tenant = System.Guid.Empty.ToString(),
                Name = name,
                Alias = alias,
            };

            // Act
            var results = ResourceModelTestHelper.Validate(model);

            // Assert
            Assert.Equal(0, results.Count);
        }

        /// <summary>
        /// The ProductModel_Validation_Fail_IfInvalidInput.
        /// </summary>
        /// <param name="id">The id<see cref="string"/>.</param>
        /// <param name="name">The name<see cref="string"/>.</param>
        /// <param name="alias">The alias<see cref="string"/>.</param>
        [InlineData(
        "",
        "",
        "")]
        [InlineData(
        "-w-",
        "()()",
        "F a")]
        [InlineData(
        "Abrev",
        "!!!!!!!!",
        "!()")]
        [InlineData(
        "001product",
        "001 Tenant-",
        "001")]
        [InlineData(
        "null",
        "null",
        "null")]
        [InlineData(
        "product@123",
        "product@123",
        "product@123")]
        [InlineData(
        "12345",
        "12345",
        "12345")]
        [InlineData(
        "12345",
        "12345-",
        "12345-")]
        [Theory]
        public void ProductModel_Validation_Fail_IfInvalidInput(
           string id,
           string name,
           string alias)
        {
            // Arrange
            var model = new ProductCreateRequestModel()
            {
                Id = id,
                Tenant = System.Guid.Empty.ToString(),
                Name = name,
                Alias = alias,
            };

            // Act
            var results = ResourceModelTestHelper.Validate(model);

            // Assert
            Assert.Equal(2, results.Count);
        }
    }
}
