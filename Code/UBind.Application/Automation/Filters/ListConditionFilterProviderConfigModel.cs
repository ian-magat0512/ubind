// <copyright file="ListConditionFilterProviderConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Filters
{
    using System;
    using Newtonsoft.Json;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Expression;

    /// <summary>
    /// Model for building a list condition filter provider.
    /// </summary>
    /// <remarks>
    /// A list condition filter is a condition for filtering a collection, that bases the condition on a different
    /// collection, that could be a sub-list of the first collection, or could be an independent list.
    /// </remarks>
    public class ListConditionFilterProviderConfigModel : IBuilder<IFilterProvider>
    {
        /// <summary>
        /// Gets the model for building the provider for the list the condition should be applied to.
        /// </summary>
        [JsonProperty]
        public IBuilder<IExpressionProvider> List { get; private set; }

        /// <summary>
        /// Gets the model for building the provider of the alias used to access list items in filters.
        /// </summary>
        [JsonProperty]
        public IBuilder<IProvider<Data<string>>>? ItemAlias { get; private set; }

        /// <summary>
        /// Gets the model for building the provider for the condition to apply to the list items.
        /// </summary>
        [JsonProperty]
        public IBuilder<IFilterProvider> Condition { get; private set; }

        /// <summary>
        /// Gets the type of operation that should be used when applying the condition to the list items (any, all, etc.).
        /// </summary>
        [JsonProperty]
        public ListConditionMatchType MatchType { get; private set; }

        /// <inheritdoc/>
        IFilterProvider IBuilder<IFilterProvider>.Build(IServiceProvider dependencyProvider)
        {
            return new ListConditionFilterProvider(
                this.List.Build(dependencyProvider),
                this.ItemAlias?.Build(dependencyProvider) ?? new StaticProvider<Data<string>>("item"),
                this.Condition.Build(dependencyProvider),
                this.MatchType);
        }
    }
}
