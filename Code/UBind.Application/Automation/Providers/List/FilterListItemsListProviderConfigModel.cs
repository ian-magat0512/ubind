// <copyright file="FilterListItemsListProviderConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.List
{
    using System;
    using Newtonsoft.Json;
    using UBind.Application.Automation.Filters;
    using UBind.Application.Automation.Providers;

    /// <summary>
    /// Model for building a filter list items list provider.
    /// </summary>
    public class FilterListItemsListProviderConfigModel : IBuilder<IDataListProvider<object>>
    {
        /// <summary>
        /// Gets the model for building the provider for the collection to be filtered.
        /// </summary>
        [JsonProperty]
        public IBuilder<IDataListProvider<object>> List { get; private set; }

        /// <summary>
        /// Gets the models for building the filter providers.
        /// </summary>
        [JsonProperty]
        public IBuilder<IFilterProvider> Condition { get; private set; }

        /// <summary>
        /// Gets the model for building the provider for the item alias.
        /// An optional alias that will be assigned to the list item while evaluating the condition. Can be used by
        /// path lookups within the condition using hashtag prefix, e.g. #myAlias.propertyName. If none is specified,
        /// and the item is of a known entity type, the name of that entity type will be used as the alias,
        /// e.g. 'event' or 'quote'. If the item is not of a known entity type, then the alias 'item' will be used.
        /// If another item alias within the same context has the same auto-generated name, then an integer suffix
        /// will be added such the alias is unique, e.g. 'item1' or 'item2'.
        /// </summary>
        [JsonProperty]
        public IBuilder<IProvider<Data<string>>> ItemAlias { get; private set; }

        /// <inheritdoc/>
        public IDataListProvider<object> Build(IServiceProvider dependencyProvider)
        {
            var collectionProvider = this.List.Build(dependencyProvider);
            var filterProvider = this.Condition.Build(dependencyProvider);
            var itemAliasProvider = this.ItemAlias?.Build(dependencyProvider);
            return new FilterListItemsListProvider(collectionProvider, filterProvider, itemAliasProvider);
        }
    }
}
