// <copyright file="ValidateObjectAttribute.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Validation
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Attribute to put on a property to tell it to validate the sub properties of that object.
    /// </summary>
    public class ValidateObjectAttribute : ValidationAttribute
    {
        /// <inheritdoc />
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                // nothing to validate. We will not create a failed validation result, instead
                // that can be done by the [Required] attribute.
                return ValidationResult.Success;
            }

            var results = new List<ValidationResult>();
            var context = new ValidationContext(value, null, null);

            Validator.TryValidateObject(value, context, results, true);
            if (results.Count != 0)
            {
                string subjectName = value is INamed ? (value as INamed).Name : null;
                var compositeResults = new CompositeValidationResult(
                    string.Format("Validation for {0} failed!", validationContext.DisplayName),
                    value.GetType(),
                    subjectName);
                results.ForEach(compositeResults.AddResult);
                return compositeResults;
            }

            return ValidationResult.Success;
        }
    }
}
