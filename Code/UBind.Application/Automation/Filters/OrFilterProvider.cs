// <copyright file="OrFilterProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Filters
{
    using System.Collections.Generic;
    using System.Linq.Expressions;

    /// <summary>
    /// For providing a filters that performs a logical OR of a collection of other filers.
    /// </summary>
    public class OrFilterProvider : AggregateFilterProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OrFilterProvider"/> class.
        /// </summary>
        /// <param name="filterProviders">A collection of providers for filters to be ORed.</param>
        public OrFilterProvider(IEnumerable<IFilterProvider> filterProviders)
            : base(filterProviders, Expression.Or, false, "orCondition")
        {
        }
    }
}
