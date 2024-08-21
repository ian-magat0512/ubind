// <copyright file="ValidateItemsAttribute.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Validation
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Attribute to put on a property which is a collection to tell it to validate items in the collection.
    /// </summary>
    public class ValidateItemsAttribute : ValidationAttribute
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

            if (!(value is IEnumerable))
            {
                throw new InvalidOperationException(
                    "You cannot add the ValidateItems attribute to a property which is not IEnumerable.");
            }

            IEnumerable items = (IEnumerable)value;
            var results = new List<ValidationResult>();
            foreach (var item in items)
            {
                var itemResults = new List<ValidationResult>();
                var context = new ValidationContext(item, null, null);
                Validator.TryValidateObject(item, context, itemResults, true);

                if (itemResults.Count != 0)
                {
                    var subjectName = item is INamed ? (item as INamed).Name : null;
                    var compositeResults = new CompositeValidationResult(
                        string.Format("Validation for {0} failed!", context.DisplayName), item.GetType(), subjectName);
                    itemResults.ForEach(compositeResults.AddResult);
                    results.Add(compositeResults);
                }
            }

            if (results.Count != 0)
            {
                var subjectName = value is INamed ? (value as INamed).Name : null;
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
