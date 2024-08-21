// <copyright file="PasswordComplexityValidator.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Authentication
{
    using System.Collections.Generic;
    using System.Linq;
    using CSharpFunctionalExtensions;
    using UBind.Domain.Helpers;

    /// <inheritdoc/>
    public class PasswordComplexityValidator : IPasswordComplexityValidator
    {
        private readonly IEnumerable<PasswordComplexityRuleSet> complexityRuleSets;

        /// <summary>
        /// Initializes a new instance of the <see cref="PasswordComplexityValidator"/> class.
        /// </summary>
        /// <param name="complexityRuleSets">Sets of password complexity rules.</param>
        public PasswordComplexityValidator(params PasswordComplexityRuleSet[] complexityRuleSets)
        {
            this.complexityRuleSets = complexityRuleSets;
        }

        /// <summary>
        /// Gets a default validator to use when specific validation rules are not specified for a tenant.
        /// </summary>
        public static IPasswordComplexityValidator Default
        {
            get
            {
                var shortPasswordComplexityRuleSet = new PasswordComplexityRuleSet(
                        "Password must be 12 or more characters including at least one letter, digit and non-alphanumeric character",
                        StandardPasswordRules.MinimumLength(12),
                        StandardPasswordRules.MustContainLetter,
                        StandardPasswordRules.MustContainDigit,
                        StandardPasswordRules.MustContainNonAlphaNumeric);
                return new PasswordComplexityValidator(shortPasswordComplexityRuleSet);
            }
        }

        /// <inheritdoc/>
        public Result<Void, IEnumerable<string>> Validate(string cleartextPassword)
        {
            var complexityResults = this.complexityRuleSets.Select(set => set.Invoke(cleartextPassword));
            if (!complexityResults.Any(result => result.Succeeded))
            {
                var errors = complexityResults.SelectMany(result => result.Error);
                return Result.Failure<Void, IEnumerable<string>>(errors);
            }

            return Result.Success<Void, IEnumerable<string>>(default);
        }
    }
}
