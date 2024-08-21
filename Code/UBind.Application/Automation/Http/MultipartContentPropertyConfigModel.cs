// <copyright file="MultipartContentPropertyConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Http
{
    using System;
    using UBind.Application.Automation.Providers;

    /// <summary>
    /// Builder model for multipart content properties.
    /// </summary>
    public class MultipartContentPropertyConfigModel : IBuilder<MultipartContentProperty>
    {
        /// <summary>
        /// Gets or sets the type of content.
        /// </summary>
        public IBuilder<IProvider<Data<string>>> ContentType { get; set; }

        /// <summary>
        /// Gets or sets the content.
        /// </summary>
        public IBuilder<ContentProvider> Content { get; set; }

        /// <inheritdoc/>
        public MultipartContentProperty Build(IServiceProvider dependencyProvider)
        {
            return new MultipartContentProperty() { ContentType = this.ContentType.Build(dependencyProvider), Content = this.Content.Build(dependencyProvider) };
        }
    }
}
