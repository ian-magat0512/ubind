// <copyright file="PropertyEqualityFilter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Filters
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Filter for filtering resources based on the value of a property.
    /// </summary>
    /// <typeparam name="TResource">The type of the resource being filtered.</typeparam>
    /// <typeparam name="TProperty">The type of the property being filtered on.</typeparam>
    public class PropertyEqualityFilter<TResource, TProperty> : PropertyFilter<TResource, TProperty>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyEqualityFilter{TResource, TProperty}"/> class.
        /// </summary>
        /// <param name="value">The value to filter for.</param>
        /// <param name="propertySelector">A function for selecting the property to filter on.</param>
        public PropertyEqualityFilter(TProperty value, Func<TResource, TProperty> propertySelector)
            : base(value, propertySelector, (x, y) => EqualityComparer<TProperty>.Default.Equals(x, y))
        {
        }
    }
}
