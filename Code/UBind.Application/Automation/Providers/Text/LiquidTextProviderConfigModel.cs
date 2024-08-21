// <copyright file="LiquidTextProviderConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Text
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UBind.Application.Automation.Providers.Object;

    /// <summary>
    /// Model for building an instance of <see cref="LiquidTextProvider"/>.
    /// </summary>
    public class LiquidTextProviderConfigModel : IBuilder<IProvider<Data<string>>>
    {
        /// <summary>
        /// Gets or sets the liquid template string.
        /// </summary>
        public IBuilder<IProvider<Data<string>>> LiquidTemplate { get; set; }

        /// <summary>
        /// Gets or sets the data object to be exposed to the template for parsing.
        /// </summary>
        public IBuilder<IObjectProvider> DataObject { get; set; }

        /// <summary>
        /// Gets or sets the snippet of the template.
        /// </summary>
        public IEnumerable<IBuilder<IProvider<LiquidTextSnippet>>> Snippets { get; set; } = Enumerable.Empty<IBuilder<IProvider<LiquidTextSnippet>>>();

        /// <inheritdoc/>
        public IProvider<Data<string>> Build(IServiceProvider dependencyProvider)
        {
            return new LiquidTextProvider(
                this.LiquidTemplate.Build(dependencyProvider),
                this.DataObject.Build(dependencyProvider),
                this.Snippets?.Select(snippet => snippet.Build(dependencyProvider)));
        }
    }
}
