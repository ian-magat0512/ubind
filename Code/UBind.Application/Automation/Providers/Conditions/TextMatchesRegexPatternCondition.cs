// <copyright file="TextMatchesRegexPatternCondition.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Conditions
{
    using System.Text.RegularExpressions;

    /// <summary>
    /// Returns a boolean true if the text is a regex match for the regex pattern.
    /// </summary>
    public class TextMatchesRegexPatternCondition : ComparisonCondition<string>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TextMatchesRegexPatternCondition"/> class.
        /// </summary>
        /// <param name="textProvider">The text provider.</param>
        /// <param name="regexPatternProvider">The regex expression provider.</param>
        public TextMatchesRegexPatternCondition(IProvider<Data<string>> textProvider, IProvider<Data<string>> regexPatternProvider)
            : base(textProvider, regexPatternProvider, (text, regexPattern) => { return new Regex(regexPattern).IsMatch(text); }, "textMatchesRegexPatternCondition")
        {
        }
    }
}
