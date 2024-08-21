// <copyright file="AggregateFilterProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Filters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Expression;

    /// <summary>
    /// For providing a filter that aggregates a set of other providers.
    /// </summary>
    public abstract class AggregateFilterProvider : IFilterProvider
    {
        private readonly IEnumerable<IFilterProvider> filterProviders;
        private readonly Expression seed;
        private readonly Func<Expression, Expression, Expression> accumulator;

        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateFilterProvider"/> class.
        /// </summary>
        /// <param name="filterProviders">A collection of filterProviders to be combined.</param>
        /// <param name="accumulator">Accumulator function for aggregating filters.</param>
        /// <param name="seed">Seed value for aggregation.</param>
        protected AggregateFilterProvider(
            IEnumerable<IFilterProvider> filterProviders,
            Func<Expression, Expression, Expression> accumulator,
            bool seed,
            string schemaReferenceKey)
        {
            this.filterProviders = filterProviders;
            this.accumulator = accumulator;
            this.seed = Expression.Constant(seed);
            this.SchemaReferenceKey = schemaReferenceKey;
        }

        public virtual string SchemaReferenceKey { get; }

        /// <inheritdoc/>
        public Task<Expression> Resolve(IProviderContext providerContext, ExpressionScope scope)
        {
            providerContext.CancellationToken.ThrowIfCancellationRequested();
            var lambda = Expression.Lambda(
                this.filterProviders.Aggregate(
                    this.seed,
                    (expression, provider) => this.accumulator(expression, ((LambdaExpression)provider.Resolve(providerContext, scope).Result).Body)),
                scope.CurrentParameter);
            return Task.FromResult((Expression)lambda);
        }
    }
}
