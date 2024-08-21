// <copyright file="JsonTextToObjectProviderConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Object
{
    using System;

    /// <summary>
    /// Configuration model for building an instance of <see cref="JsonTextToObjectProvider"/>.
    /// </summary>
    public class JsonTextToObjectProviderConfigModel : IBuilder<IObjectProvider>
    {
        /// <summary>
        /// Gets or sets the text provider to be used.
        /// </summary>
        public IBuilder<IProvider<Data<string>>> TextProvider { get; set; }

        /// <inheritdoc/>
        public IObjectProvider Build(IServiceProvider dependencyProvider)
        {
            return new JsonTextToObjectProvider(this.TextProvider.Build(dependencyProvider));
        }
    }
}
