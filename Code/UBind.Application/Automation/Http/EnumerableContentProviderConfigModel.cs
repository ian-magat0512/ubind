// <copyright file="EnumerableContentProviderConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Http
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Builder model for multipart content.
    /// </summary>
    public class EnumerableContentProviderConfigModel : IBuilder<ContentProvider>
    {
        /// <summary>
        /// Gets or sets the content.
        /// </summary>
        public IEnumerable<MultipartContentPropertyConfigModel> Content { get; set; } = Enumerable.Empty<MultipartContentPropertyConfigModel>();

        /// <inheritdoc/>
        public ContentProvider Build(IServiceProvider dependencyProvider)
        {
            var content = this.Content.Select(ct => ct.Build(dependencyProvider));
            return new EnumerableContentProvider(content);
        }
    }
}
