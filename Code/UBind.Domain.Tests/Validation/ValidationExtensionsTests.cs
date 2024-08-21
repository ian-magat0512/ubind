// <copyright file="ValidationExtensionsTests.cs" company="uBind">
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

    public class ValidationExtensionsTests
    {
        [Fact]
        public void FlattenCompositeResults_FlattensCompositeResults_WhenThereAreSome()
        {
            // Arrange
            var results = new List<ValidationResult>();
            var composite = new CompositeValidationResult("Object validation failed.", this.GetType());
            composite.AddResult(new ValidationResult("Abc is required."));
            composite.AddResult(new ValidationResult("Cde is required."));
            results.Add(composite);
            results.Add(new ValidationResult("Xyz is required"));
            results.Should().HaveCount(2);

            // Act
            var flatList = results.FlattenCompositeResults();

            // Assert
            flatList.Should().HaveCount(3);
        }

        [Fact]
        public void FlattenCompositeResults_FlattensCompositeResults_WhenThereIsMultipleLevelsOfNesting()
        {
            // Arrange
            var results = new List<ValidationResult>();
            var composite1 = new CompositeValidationResult("Object validation failed.", this.GetType());
            composite1.AddResult(new ValidationResult("Abc is required."));
            composite1.AddResult(new ValidationResult("Cde is required."));
            results.Add(composite1);
            results.Add(new ValidationResult("Xyz is required"));
            var composite2 = new CompositeValidationResult("Object validation failed.", this.GetType());
            composite2.AddResult(new ValidationResult("AAA is required."));
            composite2.AddResult(new ValidationResult("BBB is required."));
            composite1.AddResult(composite2);
            results.Should().HaveCount(2);

            // Act
            var flatList = results.FlattenCompositeResults();

            // Assert
            flatList.Should().HaveCount(5);
        }
    }
}
