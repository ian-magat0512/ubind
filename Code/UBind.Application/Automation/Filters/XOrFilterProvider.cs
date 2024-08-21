// <copyright file="XOrFilterProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Filters
{
    using System.Collections.Generic;
    using System.Linq.Expressions;

    /// <summary>
    /// Returns a boolean true expression if one of the input conditions is true and the other is false, otherwise it returns boolean false expression.
    /// </summary>
    public class XOrFilterProvider : AggregateFilterProvider
    {
        public XOrFilterProvider(IEnumerable<IFilterProvider> filterProviders)
            : base(filterProviders, Expression.ExclusiveOr, false, "xorCondition")
        {
        }
    }
}
