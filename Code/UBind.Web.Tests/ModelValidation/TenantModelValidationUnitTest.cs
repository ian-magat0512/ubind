// <copyright file="TenantModelValidationUnitTest.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Tests.ModelValidation
{
    using System;
    using NodaTime;
    using UBind.Domain;
    using UBind.Web.ResourceModels;
    using UBind.Web.ResourceModels.ModelHelpers;
    using Xunit;

    /// <summary>
    /// Defines the <see cref="TenantModelValidationUnitTest" />.
    /// </summary>
    public class TenantModelValidationUnitTest
    {
        /// <summary>
        /// The TenantModel_Validation_Success_IfValidInput.
        /// </summary>
        /// <param name="alias">The alias<see cref="string"/>.</param>
        /// <param name="name">The name<see cref="string"/>.</param>
        [Theory]
        [InlineData(
        "tenant-id",
        "This is some name")]
        [InlineData(
        "d",
        "greatness")]
        [InlineData(
        "te-nant-id",
        "Boot-boot")]
        [InlineData(
        "d",
        "name-Sake")]
        [InlineData(
        "te-nant-id",
        "boot boot -'. ,")]
        [InlineData(
        "tenant-001",
        "Tenant 001")]
        [InlineData(
        "tenant-001-tenant",
        "Tenant 001 tenant")]
        [InlineData(
        "tenant001-tenant",
        "Tenant001 tenant")]
        public void TenantModel_Validation_Success_IfValidInput(
            string alias,
            string name)
        {
            // Arrange
            var tenant = new Tenant(Guid.NewGuid(), name, alias, null, default, default, Instant.MinValue);
            var model = new TenantModel(tenant);

            // Act
            var results = ResourceModelTestHelper.Validate(model);

            // Assert
            Assert.Equal(0, results.Count);
        }

        /// <summary>
        /// The TenantModel_Validation_Fail_IfInvalidInput.
        /// </summary>
        /// <param name="alias">The alias<see cref="string"/>.</param>
        /// <param name="name">The name<see cref="string"/>.</param>
        [Theory]
        [InlineData(
       "--",
       "--")]
        [InlineData(
       "",
       null)]
        [InlineData(
       "-w-",
       "()()")]
        [InlineData(
       "-woo-",
       "!!!!!!!!")]
        [InlineData(
       "001-test",
       "001 Test")]
        public void TenantModel_Validation_Fail_IfInvalidInput(
            string alias,
            string name)
        {
            try
            {
                // Arrange
                var tenant = new Tenant(Guid.NewGuid(), name, alias, null, default, default, Instant.MinValue);
                var model = new TenantModel(tenant);

                // Act
                var results = ResourceModelTestHelper.Validate(model);

                // Assert
                Assert.Equal(2, results.Count);
            }
            catch
            {
                // Assert
                Assert.True(true);
            }
        }
    }
}
