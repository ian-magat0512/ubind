// <copyright file="PasswordComplexityRuleSet.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Authentication
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Class defining a set of complexity rules that must be satisfied together.
    /// </summary>
    public class PasswordComplexityRuleSet
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PasswordComplexityRuleSet"/> class.
        /// </summary>
        /// <param name="requirementDescription">A description of the rule set requirements.</param>
        /// <param name="rules">The actual rules.</param>
        public PasswordComplexityRuleSet(string requirementDescription, params PasswordComplexityRule[] rules)
        {
            this.RequirementDescription = requirementDescription;
            this.Rules = rules;
        }

        /// <summary>
        /// Gets the description of the rule set requirements.
        /// </summary>
        public string RequirementDescription { get; }

        /// <summary>
        /// Gets the actual rules.
        /// </summary>
        public IEnumerable<PasswordComplexityRule> Rules { get; }

        /// <summary>
        /// Apply the rules in the set to a given password.
        /// </summary>
        /// <param name="cleartextPassword">The cleartext password.</param>
        /// <returns>A result indicating sucess or failure, and in the case of failure, the set of errors encountered.</returns>
        public ResultOld<IEnumerable<string>> Invoke(string cleartextPassword)
        {
            var errors = this.Rules
                .Select(rule => rule.Invoke(cleartextPassword))
                .Where(result => !result.Succeeded)
                .Select(result => result.Error);
            return errors.Any()
                ? ResultOld<IEnumerable<string>>.Failure(errors)
                : ResultOld<IEnumerable<string>>.Success();
        }
    }
}
