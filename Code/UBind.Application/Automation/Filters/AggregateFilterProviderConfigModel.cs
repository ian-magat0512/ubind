// <copyright file="AggregateFilterProviderConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Filters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Base class for config models for filter provider that aggregate a filters from other providers.
    /// </summary>
    public abstract class AggregateFilterProviderConfigModel : IBuilder<IFilterProvider>
    {
        /// <summary>
        /// Gets or sets the conditions that should be ANDed together.
        /// </summary>
        public IEnumerable<IBuilder<IFilterProvider>> Conditions { get; set; }

        /// <inheritdoc/>
        public IFilterProvider Build(IServiceProvider dependencyProvider)
        {
            var filterProviders = this.Conditions.Select(c => c.Build(dependencyProvider));
            return this.AggregateFilterProviders(filterProviders);
        }

        /// <summary>
        /// Method for aggregating a collection of filter providers to create single filter provider.
        /// </summary>
        /// <param name="filterProviders">The filter providers to aggregate.</param>
        /// <returns>A new aggregate filter provider.</returns>
        protected abstract IFilterProvider AggregateFilterProviders(IEnumerable<IFilterProvider> filterProviders);
    }
}
