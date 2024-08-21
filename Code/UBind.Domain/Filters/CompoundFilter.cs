// <copyright file="CompoundFilter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Filters
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Compound filter for applying a set of filters to a resource.
    /// </summary>
    /// <typeparam name="TResource">The type of the resource being filtered.</typeparam>
    public class CompoundFilter<TResource> : IResourceFilter<TResource>
    {
        private readonly IList<IResourceFilter<TResource>> filters;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompoundFilter{TResource}"/> class.
        /// </summary>
        /// <param name="filters">The set of filters to apply.</param>
        public CompoundFilter(params IResourceFilter<TResource>[] filters)
        {
            this.filters = filters.ToList();
        }

        /// <inheritdoc/>
        public IEnumerable<TResource> Apply(IEnumerable<TResource> resources)
        {
            foreach (var filter in this.filters)
            {
                resources = filter.Apply(resources);
            }

            return resources;
        }

        /// <summary>
        /// Add a new filter to the compound filter.
        /// </summary>
        /// <param name="filter">The filter to add.</param>
        public void AddFilter(IResourceFilter<TResource> filter)
        {
            this.filters.Add(filter);
        }
    }
}
