// <copyright file="TextContentProviderConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Http
{
    using System;
    using UBind.Application.Automation.Providers;

    /// <summary>
    /// Model for building a content object of type string.
    /// </summary>
    public class TextContentProviderConfigModel : IBuilder<ContentProvider>
    {
        /// <summary>
        /// Gets or sets the text defined by a text provider.
        /// </summary>
        public IBuilder<IProvider<Data<string?>>> Content { get; set; }

        /// <inheritdoc/>
        public ContentProvider Build(IServiceProvider dependencyProvider)
        {
            return new TextContentProvider(this.Content?.Build(dependencyProvider));
        }
    }
}
