// <copyright file="RequiredIfAttribute.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Validation
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using Humanizer;

    public class RequiredIfAttribute : ValidationAttribute
    {
        public RequiredIfAttribute(
            string otherPropertyName,
            object otherPropertyValue,
            ComparisonOperator comparisonOperator = ComparisonOperator.Equal,
            string errorMessage = null)
        {
            this.OtherPropertyName = otherPropertyName;
            this.ErrorMessage = errorMessage;
            this.OtherPropertyExpectedValue = otherPropertyValue;
            this.ComparisonOperator = comparisonOperator;
        }

        public string OtherPropertyName { get; }

        public object OtherPropertyExpectedValue { get; }

        public ComparisonOperator ComparisonOperator { get; }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var instance = validationContext.ObjectInstance;
            var propertyInfo = validationContext.ObjectType.GetProperty(this.OtherPropertyName);
            if (propertyInfo == null)
            {
                throw new ArgumentException(
                    $"The property {this.OtherPropertyName} was not found when evaluating the RequiredIf attribute on "
                    + $"the {validationContext.DisplayName} property of {validationContext.ObjectType}.");
            }

            var otherPropertyActualValue
                = validationContext.ObjectType.GetProperty(this.OtherPropertyName).GetValue(instance, null);
            bool required;
            switch (this.ComparisonOperator)
            {
                case ComparisonOperator.Equal:
                    required = (otherPropertyActualValue == null && this.OtherPropertyExpectedValue == null)
                        || (otherPropertyActualValue != null && otherPropertyActualValue.Equals(this.OtherPropertyExpectedValue));
                    break;
                case ComparisonOperator.NotEqual:
                    required = (otherPropertyActualValue == null && this.OtherPropertyExpectedValue != null)
                        || (otherPropertyActualValue != null && this.OtherPropertyExpectedValue == null)
                        || !otherPropertyActualValue.Equals(this.OtherPropertyExpectedValue);
                    break;
                default:
                    throw new ArgumentException(
                        $"An unknown or unsupported comparison operator \"{this.ComparisonOperator.Humanize()}\" was used for the RequiredIf attribute when "
                        + $"evaluating the RequiredIf attribute on the {validationContext.DisplayName} property of "
                        + $"{validationContext.ObjectType}.");
            }

            if (required && value == null)
            {
                return new ValidationResult(
                    this.ErrorMessage ?? this.GenerateErrorMessage(validationContext, otherPropertyActualValue));
            }

            return ValidationResult.Success;
        }

        private string GenerateErrorMessage(ValidationContext validationContext, object propertyValue)
        {
            return $"When validating the {validationContext.ObjectType}, a value for the "
                + $"{validationContext.DisplayName} property was not provided, and it is required "
                + $"because the value of the {this.OtherPropertyName} property is {propertyValue}.";
        }
    }
}
