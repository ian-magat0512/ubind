// <copyright file="ObjectToJsonTextProviderConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Text
{
    using System;
    using UBind.Application.Automation.Providers.Object;

    /// <summary>
    /// Model for building an instance of <see cref="ObjectToJsonTextProvider"/>.
    /// </summary>
    public class ObjectToJsonTextProviderConfigModel : IBuilder<IProvider<Data<string>>>
    {
        /// <summary>
        /// Gets or sets the data object to be converted to json string.
        /// </summary>
        public IBuilder<IObjectProvider> DataObject { get; set; }

        /// <inheritdoc/>
        public IProvider<Data<string>> Build(IServiceProvider dependencyProvider)
        {
            return new ObjectToJsonTextProvider(this.DataObject.Build(dependencyProvider));
        }
    }
}
