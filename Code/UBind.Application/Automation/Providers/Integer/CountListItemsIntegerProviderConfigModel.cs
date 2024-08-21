// <copyright file="CountListItemsIntegerProviderConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Integer
{
    using System;
    using Newtonsoft.Json;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.List;

    /// <summary>
    /// Model for building a list count provider.
    /// </summary>
    public class CountListItemsIntegerProviderConfigModel : IBuilder<IProvider<Data<long>>>
    {
        /// <summary>
        /// Gets or sets the model building the provider of the collection that is to be counted.
        /// </summary>
        [JsonProperty]
        public IBuilder<IDataListProvider<object>> List { get; set; }

        /// <inheritdoc/>
        public IProvider<Data<long>> Build(IServiceProvider dependencyProvider)
        {
            var collectionProvider = this.List.Build(dependencyProvider);

            return new CountListItemsIntegerProvider(collectionProvider);
        }
    }
}
