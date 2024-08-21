// <copyright file="ValidateObjectAttributeTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests.Validation
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using FluentAssertions;
    using UBind.Domain.Validation;
    using Xunit;

    public class ValidateObjectAttributeTests
    {
        [Fact]
        public void IsValid_ReturnsTrue_WhenSubObjectIsNull()
        {
            // Arrange
            var testClass = new ValidationStub.WithValidateObject
            {
                Property1 = "asdf",
            };
            var validationContext = new ValidationContext(testClass);
            var results = new List<ValidationResult>();

            // Act
            bool isValid = Validator.TryValidateObject(testClass, validationContext, results, true);

            // Assert
            isValid.Should().BeTrue();
            results.Should().HaveCount(0);
        }

        [Fact]
        public void IsValid_ReturnsCompositeResults_WhenSubPropertiesAreInvalid()
        {
            // Arrange
            var testClass = new ValidationStub.WithValidateObject
            {
                Object1 = new ValidationStub.WithRequiredProperties(),
            };
            var validationContext = new ValidationContext(testClass);
            var results = new List<ValidationResult>();

            // Act
            bool isValid = Validator.TryValidateObject(testClass, validationContext, results, true);

            // Assert
            isValid.Should().BeFalse();
            results.Should().HaveCount(2);
            results[1].Should().BeOfType<CompositeValidationResult>();
            CompositeValidationResult innerResult = (CompositeValidationResult)results[1];
            innerResult.Results.Should().HaveCount(2);
        }
    }
}
