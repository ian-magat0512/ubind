// <copyright file="IntegerGreaterThanOrEqualToFilterProviderConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Filters
{
    using System;
    using System.Linq.Expressions;
    using Newtonsoft.Json;
    using UBind.Application.Automation.Providers.Expression;

    /// <summary>
    /// Model for building an filter based on "greater than or equal to" comparison of integers.
    /// </summary>
    public class IntegerGreaterThanOrEqualToFilterProviderConfigModel : IBuilder<IFilterProvider>
    {
        /// <summary>
        /// Gets the model for building the provider for the first operand of the "greater than or equal to" filter.
        /// </summary>
        [JsonProperty]
        public IBuilder<IExpressionProvider> Integer { get; private set; }

        /// <summary>
        /// Gets the model for building the provider for the second operand of the "greater than or equal to" filter.
        /// </summary>
        [JsonProperty]
        public IBuilder<IExpressionProvider> IsGreaterThanOrEqualTo { get; private set; }

        /// <inheritdoc/>
        IFilterProvider IBuilder<IFilterProvider>.Build(IServiceProvider dependencyProvider)
        {
            return new BinaryExpressionFilterProvider(
                (first, second) => Expression.GreaterThanOrEqual(first, second),
                this.Integer.Build(dependencyProvider),
                this.IsGreaterThanOrEqualTo.Build(dependencyProvider),
                "integerIsGreaterThanOrEqualToCondition");
        }
    }
}
