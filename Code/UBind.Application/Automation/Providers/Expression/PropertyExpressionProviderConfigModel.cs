// <copyright file="PropertyExpressionProviderConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Expression
{
    using System;
    using Newtonsoft.Json;
    using UBind.Application.Automation.Providers;

    /// <summary>
    /// For providing an expression specifying a property on an entity, for use in entity collection filters.
    /// </summary>
    public class PropertyExpressionProviderConfigModel : IBuilder<IExpressionProvider>
    {
        /// <summary>
        /// Gets or sets the model for building the provider for the path to the property to use.
        /// </summary>
        [JsonProperty]
        public IBuilder<IProvider<Data<string>>> Property { get; set; }

        /// <inheritdoc/>
        public IExpressionProvider Build(IServiceProvider serviceProvider)
        {
            return new PropertyExpressionProvider(this.Property.Build(serviceProvider));
        }
    }
}
