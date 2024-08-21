// <copyright file="ListConditionFilterProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Filters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Expression;
    using UBind.Application.Automation.Providers.List;

    /// <summary>
    /// Filter for applying "Any" conditions to lists.
    /// </summary>
    public class ListConditionFilterProvider : IFilterProvider
    {
        private readonly IExpressionProvider listProvider;
        private readonly IProvider<Data<string>>? itemAliasProvider;
        private readonly IFilterProvider listFilterProvider;
        private readonly ListConditionMatchType matchType;

        /// <summary>
        /// Initializes a new instance of the <see cref="ListConditionFilterProvider"/> class.
        /// </summary>
        /// <param name="listProvider">A provider for the list to apply the conditions to.</param>
        /// <param name="itemAliasProvider">A provider for the alias by which list items can be accessed in filters.</param>
        /// <param name="listFilterProvider">The condition to apply to list items.</param>
        /// <param name="matchType">A value indicating how the condition should be applied to the list.</param>
        public ListConditionFilterProvider(
            IExpressionProvider listProvider,
            IProvider<Data<string>>? itemAliasProvider,
            IFilterProvider listFilterProvider,
            ListConditionMatchType matchType)
        {
            this.listProvider = listProvider;
            this.itemAliasProvider = itemAliasProvider;
            this.listFilterProvider = listFilterProvider;
            this.matchType = matchType;
        }

        public string SchemaReferenceKey => "listCondition";

        /// <inheritdoc/>
        public async Task<Expression> Resolve(IProviderContext providerContext, ExpressionScope scope)
        {
            providerContext.CancellationToken.ThrowIfCancellationRequested();
            var outerParameter = scope.CurrentParameter;
            var listExpression = await this.listProvider.Invoke(providerContext, scope);
            var elementType = this.GetElementType(listExpression);
            var itemAlias = (await this.itemAliasProvider.ResolveValueIfNotNull(providerContext))?.DataValue;
            Func<ExpressionScope.INestedScope>? scopeFactory = null;
            if (itemAlias != null)
            {
                scopeFactory = () => scope.Push(itemAlias, Expression.Parameter(elementType), nameof(ListConditionFilterProvider));
            }
            else
            {
                scopeFactory = () => scope.PushWithGeneratedAlias(
                    AliasFactory.Generate(elementType), Expression.Parameter(elementType), nameof(ListConditionFilterProvider));
            }

            using (var nestedScope = scopeFactory())
            {
                var predicate = await this.listFilterProvider.Resolve(providerContext, scope);
                var methodName = this.matchType == ListConditionMatchType.Any
                    ? nameof(Enumerable.Any)
                    : this.matchType == ListConditionMatchType.All
                        ? nameof(Enumerable.All)
                        : throw new NotSupportedException($"Unsupported list condition match type: {this.matchType}.");
                var anyExpression = Expression.Call(
                        typeof(Enumerable),
                        methodName,
                        new[] { elementType },
                        listExpression,
                        predicate);
                return Expression.Lambda(anyExpression, outerParameter);
            }
        }

        private Type GetElementType(Expression expression)
        {
            IEnumerable<Type> interfaces = expression.Type.GetInterfaces();
            if (expression.Type.IsInterface)
            {
                interfaces = interfaces.Prepend(expression.Type);
            }

            var isEnumerable = interfaces
                .Where(i => i.IsGenericType)
                .Any(i => i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
            if (!isEnumerable)
            {
                throw new InvalidOperationException($"Expected collection, but got: {expression.Type}");
            }

            return expression.Type.GetGenericArguments().First();
        }
    }
}
