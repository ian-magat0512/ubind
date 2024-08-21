// <copyright file="DateTimeExpressionProviderConfigModel.cs" company="uBind">
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
    /// Model for building instant expressions for use in filters.
    /// </summary>
    public class DateTimeExpressionProviderConfigModel : IDataExpressionProviderConfigModel<long>
    {
        /// <summary>
        /// Gets or sets the model for building the provider for the instant.
        /// </summary>
        [JsonProperty]
        public IBuilder<IProvider<Data<string>>> DateTime { get; set; }

        /// <inheritdoc/>
        public IExpressionProvider Build(IServiceProvider dependencyProvider)
        {
            return new DateTimeExpressionProvider(this.DateTime.Build(dependencyProvider));
        }
    }
}
