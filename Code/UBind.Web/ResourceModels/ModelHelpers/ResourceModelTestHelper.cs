// <copyright file="ResourceModelTestHelper.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels.ModelHelpers
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Resource model test helper for validating resource models in unit tests.
    /// </summary>
    public class ResourceModelTestHelper
    {
        /// <summary>
        /// Validates the resource model.
        /// </summary>
        /// <param name="model">The model to be validated.</param>
        /// <returns>A collection of validation results.</returns>
        public static IList<ValidationResult> Validate(object model)
        {
            var results = new List<ValidationResult>();
            var validationContext = new ValidationContext(model, null, null);
            Validator.TryValidateObject(model, validationContext, results, true);
            if (model is IValidatableObject)
            {
                (model as IValidatableObject).Validate(validationContext);
            }

            return results;
        }
    }
}
