﻿// <copyright file="NumberGreaterThanOrEqualToFilterProviderConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Filters
{
    using System.Linq.Expressions;
    using Newtonsoft.Json;
    using UBind.Application.Automation.Providers.Expression;

    public class NumberGreaterThanOrEqualToFilterProviderConfigModel : IBuilder<IFilterProvider>
    {
        [JsonProperty]
        public IBuilder<IExpressionProvider> Number { get; private set; }

        [JsonProperty]
        public IBuilder<IExpressionProvider> IsGreaterThanOrEqualTo { get; private set; }

        public IFilterProvider Build(IServiceProvider dependencyProvider)
        {
            return new BinaryExpressionFilterProvider(
                (first, second) => Expression.GreaterThanOrEqual(first, second),
                this.Number.Build(dependencyProvider),
                this.IsGreaterThanOrEqualTo.Build(dependencyProvider),
                "numberIsGreaterThanOrEqualToCondition");
        }
    }
}
