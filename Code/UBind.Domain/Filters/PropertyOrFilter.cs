// <copyright file="PropertyOrFilter.cs" company="uBind">
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
    /// Filter for filtering resources based on a property matching any of a set of values.
    /// </summary>
    /// <typeparam name="TResource">The type of the resource being filtered.</typeparam>
    /// <typeparam name="TProperty">The type of the property being filtered on.</typeparam>
    public class PropertyOrFilter<TResource, TProperty> : IResourceFilter<TResource>
    {
        private readonly IEnumerable<TProperty> values;
        private readonly Func<TResource, TProperty> propertySelector;
        private readonly Func<TProperty, TProperty, bool> comparisonOfResoucePropertyToValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyOrFilter{TResource, TProperty}"/> class.
        /// </summary>
        /// <param name="values">The values to filter for.</param>
        /// <param name="propertySelector">A function for selecting the property to filter on.</param>
        /// <param name="comparisonOfResourcePropertyToValue">A method for comparing the property value with the filter value.</param>
        public PropertyOrFilter(
            IEnumerable<TProperty> values,
            Func<TResource, TProperty> propertySelector,
            Func<TProperty, TProperty, bool> comparisonOfResourcePropertyToValue)
        {
            this.values = values;
            this.propertySelector = propertySelector;
            this.comparisonOfResoucePropertyToValue = comparisonOfResourcePropertyToValue;
        }

        /// <inheritdoc/>
        public IEnumerable<TResource> Apply(IEnumerable<TResource> resources)
        {
            return resources.Where(r => this.values.Any(value => this.comparisonOfResoucePropertyToValue(this.propertySelector(r), value)));
        }
    }
}
