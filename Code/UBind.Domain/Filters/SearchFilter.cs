// <copyright file="SearchFilter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Filters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Filter for filtering resources based on wether a set of search terms occur in a given set of properties.
    /// </summary>
    /// <typeparam name="TResource">The type of the resource being filtered.</typeparam>
    public class SearchFilter<TResource> : IResourceFilter<TResource>
    {
        private readonly IEnumerable<string> searchTerms;
        private readonly Func<TResource, IEnumerable<string>> searchPropertySelector;

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchFilter{TResource}"/> class.
        /// </summary>
        /// <param name="searchTerms">The set terms to search for.</param>
        /// <param name="propertySetSelector">A function for selecting the set of properties to search in.</param>
        public SearchFilter(IEnumerable<string> searchTerms, Func<TResource, IEnumerable<string>> propertySetSelector)
        {
            this.searchTerms = searchTerms;
            this.searchPropertySelector = propertySetSelector;
        }

        /// <inheritdoc/>
        public IEnumerable<TResource> Apply(IEnumerable<TResource> resources)
        {
            // Note this does not support non-English corner cases such as ß = ss, etc.
            // We could support that in future if required, but it will be slower.
            return resources.Where(r =>
                this.searchTerms.All(term => this.searchPropertySelector(r).Any(field => field.Contains(term, StringComparison.OrdinalIgnoreCase))));
        }
    }
}
