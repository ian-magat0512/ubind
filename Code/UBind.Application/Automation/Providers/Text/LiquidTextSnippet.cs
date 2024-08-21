// <copyright file="LiquidTextSnippet.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Text
{
    /// <summary>
    /// Represents values of liquid text snippet.
    /// </summary>
    public class LiquidTextSnippet
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LiquidTextSnippet"/> class.
        /// </summary>
        public LiquidTextSnippet(
            string aliasProvider,
            string liquidTemplateProvider)
        {
            this.Alias = aliasProvider;
            this.Template = liquidTemplateProvider;
        }

        public string Alias { get; }

        public string Template { get; }
    }
}
