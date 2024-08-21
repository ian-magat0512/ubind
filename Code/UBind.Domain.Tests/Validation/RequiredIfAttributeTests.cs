// <copyright file="RequiredIfAttributeTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests.Validation
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using FluentAssertions;
    using UBind.Domain.Validation;
    using Xunit;

    public class RequiredIfAttributeTests
    {
        [Fact]
        public void IsValid_ReturnsSuccess_WhenEqualsConditionIsMetOnString()
        {
            // Arrange
            var modelStub = new ModelStubForStringEquals
            {
                StringA = "Has A Value",
                StringB = "TestString1",
            };
            var validationContext = new ValidationContext(modelStub);
            var results = new List<ValidationResult>();

            // Act
            bool isValid = Validator.TryValidateObject(modelStub, validationContext, results, true);

            // Assert
            isValid.Should().BeTrue(this.GetReasons(results));
        }

        [Fact]
        public void IsValid_ReturnsFailure_WhenEqualsConditionIsNotMetOnString()
        {
            // Arrange
            var modelStub = new ModelStubForStringEquals
            {
                StringB = "TestString1",
            };
            var validationContext = new ValidationContext(modelStub);
            var results = new List<ValidationResult>();

            // Act
            bool isValid = Validator.TryValidateObject(modelStub, validationContext, results, true);

            // Assert
            isValid.Should().BeFalse(this.GetReasons(results));
        }

        [Fact]
        public void IsValid_ReturnsSuccess_WhenNotEqualConditionIsMetOnString()
        {
            // Arrange
            var modelStub = new ModelStubForStringNotEquals
            {
                StringA = "Has A Value",
                StringB = "Something Not Equal",
            };
            var validationContext = new ValidationContext(modelStub);
            var results = new List<ValidationResult>();

            // Act
            bool isValid = Validator.TryValidateObject(modelStub, validationContext, results, true);

            // Assert
            isValid.Should().BeTrue(this.GetReasons(results));
        }

        [Fact]
        public void IsValid_ReturnsFailure_WhenNotEqualConditionIsNotMetOnString()
        {
            // Arrange
            var modelStub = new ModelStubForStringNotEquals
            {
                StringB = "Something Not Equal",
            };
            var validationContext = new ValidationContext(modelStub);
            var results = new List<ValidationResult>();

            // Act
            bool isValid = Validator.TryValidateObject(modelStub, validationContext, results, true);

            // Assert
            isValid.Should().BeFalse(this.GetReasons(results));
        }

        [Fact]
        public void IsValid_ReturnsSuccess_WhenEqualsConditionIsMetOnInt()
        {
            // Arrange
            var modelStub = new ModelStubForIntEquals
            {
                StringA = "Something",
                IntB = 1,
            };
            var validationContext = new ValidationContext(modelStub);
            var results = new List<ValidationResult>();

            // Act
            bool isValid = Validator.TryValidateObject(modelStub, validationContext, results, true);

            // Assert
            isValid.Should().BeTrue(this.GetReasons(results));
        }

        [Fact]
        public void IsValid_ReturnsFailure_WhenEqualsConditionIsNotMetOnInt()
        {
            // Arrange
            var modelStub = new ModelStubForIntEquals
            {
                IntB = 1,
            };
            var validationContext = new ValidationContext(modelStub);
            var results = new List<ValidationResult>();

            // Act
            bool isValid = Validator.TryValidateObject(modelStub, validationContext, results, true);

            // Assert
            isValid.Should().BeFalse(this.GetReasons(results));
        }

        [Fact]
        public void IsValid_ReturnsSuccess_WhenNotEqualConditionIsMetOnInt()
        {
            // Arrange
            var modelStub = new ModelStubForIntNotEquals
            {
                StringA = "Something",
                IntB = 99,
            };
            var validationContext = new ValidationContext(modelStub);
            var results = new List<ValidationResult>();

            // Act
            bool isValid = Validator.TryValidateObject(modelStub, validationContext, results, true);

            // Assert
            isValid.Should().BeTrue(this.GetReasons(results));
        }

        [Fact]
        public void IsValid_ReturnsFailure_WhenNotEqualConditionIsNotMetOnInt()
        {
            // Arrange
            var modelStub = new ModelStubForIntNotEquals
            {
                IntB = 99,
            };
            var validationContext = new ValidationContext(modelStub);
            var results = new List<ValidationResult>();

            // Act
            bool isValid = Validator.TryValidateObject(modelStub, validationContext, results, true);

            // Assert
            isValid.Should().BeFalse(this.GetReasons(results));
        }

        [Fact]
        public void IsValid_ReturnsSuccess_WhenEqualsConditionIsMetOnBool()
        {
            // Arrange
            var modelStub = new ModelStubForBooleanEquals
            {
                StringA = "Something",
                BoolB = true,
            };
            var validationContext = new ValidationContext(modelStub);
            var results = new List<ValidationResult>();

            // Act
            bool isValid = Validator.TryValidateObject(modelStub, validationContext, results, true);

            // Assert
            isValid.Should().BeTrue(this.GetReasons(results));
        }

        [Fact]
        public void IsValid_ReturnsFailure_WhenEqualsConditionIsNotMetOnBool()
        {
            // Arrange
            var modelStub = new ModelStubForBooleanEquals
            {
                BoolB = true,
            };
            var validationContext = new ValidationContext(modelStub);
            var results = new List<ValidationResult>();

            // Act
            bool isValid = Validator.TryValidateObject(modelStub, validationContext, results, true);

            // Assert
            isValid.Should().BeFalse(this.GetReasons(results));
        }

        [Fact]
        public void IsValid_ReturnsSuccess_WhenNotEqualConditionIsMetOnBool()
        {
            // Arrange
            var modelStub = new ModelStubForBooleanNotEquals
            {
                StringA = "Something",
                BoolB = false,
            };
            var validationContext = new ValidationContext(modelStub);
            var results = new List<ValidationResult>();

            // Act
            bool isValid = Validator.TryValidateObject(modelStub, validationContext, results, true);

            // Assert
            isValid.Should().BeTrue(this.GetReasons(results));
        }

        [Fact]
        public void IsValid_ReturnsFailure_WhenNotEqualConditionIsNotMetOnBool()
        {
            // Arrange
            var modelStub = new ModelStubForBooleanNotEquals
            {
                BoolB = false,
            };
            var validationContext = new ValidationContext(modelStub);
            var results = new List<ValidationResult>();

            // Act
            bool isValid = Validator.TryValidateObject(modelStub, validationContext, results, true);

            // Assert
            isValid.Should().BeFalse(this.GetReasons(results));
        }

        private string GetReasons(List<ValidationResult> results)
        {
            return string.Join(". ", results.Select(r => r.ErrorMessage));
        }

        public class ModelStubForStringEquals
        {
            [RequiredIf("StringB", "TestString1")]
            public string StringA { get; set; }

            public string StringB { get; set; }
        }

        public class ModelStubForStringNotEquals
        {
            [RequiredIf("StringB", "TestString1", ComparisonOperator.NotEqual)]
            public string StringA { get; set; }

            public string StringB { get; set; }
        }

        public class ModelStubForIntEquals
        {
            [RequiredIf("IntB", 1)]
            public string StringA { get; set; }

            public int IntB { get; set; }
        }

        public class ModelStubForIntNotEquals
        {
            [RequiredIf("IntB", 1, ComparisonOperator.NotEqual)]
            public string StringA { get; set; }

            public int IntB { get; set; }
        }

        public class ModelStubForBooleanEquals
        {
            [RequiredIf("BoolB", true)]
            public string StringA { get; set; }

            public bool BoolB { get; set; }
        }

        public class ModelStubForBooleanNotEquals
        {
            [RequiredIf("BoolB", true, ComparisonOperator.NotEqual)]
            public string StringA { get; set; }

            public bool BoolB { get; set; }
        }
    }
}
