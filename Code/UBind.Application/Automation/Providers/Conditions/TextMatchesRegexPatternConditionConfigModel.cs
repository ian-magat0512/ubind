// <copyright file="TextMatchesRegexPatternConditionConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Conditions
{
    using System;

    /// <summary>
    /// Model for building an instance of <see cref="TextMatchesRegexPatternCondition"/>.
    /// </summary>
    public class TextMatchesRegexPatternConditionConfigModel : IBuilder<IProvider<Data<bool>>>
    {
        /// <summary>
        /// Gets or sets the text that will be tested for matching the pattern.
        /// </summary>
        public IBuilder<IProvider<Data<string>>> Text { get; set; }

        /// <summary>
        /// Gets or sets the regex pattern that will be used to perform the test.
        /// </summary>
        public IBuilder<IProvider<Data<string>>> RegexPattern { get; set; }

        /// <inheritdoc/>
        public IProvider<Data<bool>> Build(IServiceProvider dependencyProvider)
        {
            return new TextMatchesRegexPatternCondition(
                this.Text.Build(dependencyProvider),
                this.RegexPattern.Build(dependencyProvider));
        }
    }
}
