// <copyright file="PatchRulesExtensions.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Json
{
    /// <summary>
    /// Extension methods for <see cref="Json.PatchRules"/>.
    /// </summary>
    public static class PatchRulesExtensions
    {
        /// <summary>
        /// Returns a value indicating whether an instance of <see cref="PatchRules"/> represents a valid combination of rules.
        /// </summary>
        /// <param name="rules">The rules to test.</param>
        /// <returns>true if the rules are a valid combination, otherwise false.</returns>
        public static bool IsValidCombination(this PatchRules rules)
        {
            if (rules.HasFlag(PatchRules.PropertyExists))
            {
                if (rules.HasFlag(PatchRules.PropertyDoesNotExist)
                    || rules.HasFlag(PatchRules.PropertyIsMissingOrNullOrEmpty))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
