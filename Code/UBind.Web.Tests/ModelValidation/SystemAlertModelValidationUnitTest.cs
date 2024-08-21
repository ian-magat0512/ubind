// <copyright file="SystemAlertModelValidationUnitTest.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

// CS1591 Missing XML comment for publicly visible type or member
// Suppress CS1591. The unit test method must be named correctly instead of adding a comment in it.
#pragma warning disable CS1591

namespace UBind.Web.Tests.ModelValidation
{
    using System;
    using NodaTime;
    using UBind.Domain;
    using UBind.Web.ResourceModels;
    using UBind.Web.ResourceModels.ModelHelpers;
    using Xunit;

    /// <summary>
    /// Defines the <see cref="SystemAlertModelValidationUnitTest" />.
    /// </summary>
    public class SystemAlertModelValidationUnitTest
    {
        /// <summary>
        /// The SystemAlertModel_Validation_Success_IfValidInput.
        /// </summary>
        /// <param name="criticalNumber">The criticalNumber.</param>
        /// <param name="warningNumber">The warningNumber.</param>
        [InlineData(
            9999,
            9999)]
        [InlineData(
            123,
            3221)]
        [InlineData(
            0,
            0)]
        [InlineData(
            null,
            null)]
        [Theory]
        public void SystemAlertModel_Validation_Success_IfValidInput(
           int? criticalNumber,
           int? warningNumber)
        {
            // Arrange
            var systemAlert = new SystemAlert(Guid.NewGuid(), SystemAlertType.PolicyNumbers, Instant.MaxValue);
            var model = new SystemAlertModel(systemAlert);
            model.WarningThreshold = warningNumber;
            model.CriticalThreshold = criticalNumber;

            // Act
            var results = ResourceModelTestHelper.Validate(model);

            // Assert
            Assert.Equal(0, results.Count);
        }

        [InlineData(
             10001111,
             -99999999)]
        [InlineData(
            10001,
            100000)]
        [InlineData(
            -1,
            -6)]
        [Theory]
        public void SystemAlertModel_Validation_Fail_IfInvalidInput(
            int? criticalNumber,
            int? warningNumber)
        {
            // Arrange
            var systemAlert = new SystemAlert(Guid.NewGuid(), SystemAlertType.PolicyNumbers, Instant.MaxValue);
            var model = new SystemAlertModel(systemAlert);
            model.WarningThreshold = warningNumber;
            model.CriticalThreshold = criticalNumber;

            // Act
            var results = ResourceModelTestHelper.Validate(model);

            // Assert
            Assert.Equal(2, results.Count);
        }
    }
}
