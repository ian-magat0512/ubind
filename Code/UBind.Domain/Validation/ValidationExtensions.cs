// <copyright file="ValidationExtensions.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Validation
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Extension methods relating to Validation classes.
    /// </summary>
    public static class ValidationExtensions
    {
        /// <summary>
        /// Extension method to flatten composite results into a single list of results.
        /// It also adds the given context to the error message so you know which nested object it was for.
        /// </summary>
        /// <param name="results">The list of results containing instances of CompositeValidationResult.</param>
        /// <param name="context">The context to include in messaging for the flattened results.</param>
        /// <returns>A flattened list of ValidationResults.</returns>
        public static IReadOnlyList<ValidationResult> FlattenCompositeResults(
            this IEnumerable<ValidationResult> results,
            string context = null)
        {
            var flatResults = new List<ValidationResult>();
            foreach (var result in results)
            {
                if (result is CompositeValidationResult)
                {
                    var compositeResult = (CompositeValidationResult)result;
                    var compositeContext = compositeResult.SubjectType.Name
                        + (string.IsNullOrEmpty(compositeResult.SubjectName)
                            ? string.Empty
                            : $" \"{compositeResult.SubjectName}\"");
                    IEnumerable<ValidationResult> subResults = compositeResult.Results;
                    IEnumerable<ValidationResult> flatSubResults
                        = subResults.FlattenCompositeResults(compositeContext);
                    foreach (var flatSubResult in flatSubResults)
                    {
                        PrefixErrorMessageWithContext(flatSubResult, context);
                        flatResults.Add(flatSubResult);
                    }
                }
                else
                {
                    PrefixErrorMessageWithContext(result, context);
                    flatResults.Add(result);
                }
            }

            return flatResults;
        }

        private static void PrefixErrorMessageWithContext(ValidationResult result, string context)
        {
            if (!string.IsNullOrEmpty(context))
            {
                result.ErrorMessage = $"[{context}] {result.ErrorMessage}";
            }
        }
    }
}
