// <copyright file="ListContainsValueFilterProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Filters
{
    using System.Linq.Expressions;
    using System.Reflection;
    using UBind.Application.Automation.Helper;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Expression;

    public class ListContainsValueFilterProvider : IFilterProvider
    {
        private readonly IExpressionProvider listProvider;
        private readonly IExpressionProvider valueProvider;

        public ListContainsValueFilterProvider(
            IExpressionProvider list,
            IExpressionProvider valueToFind)
        {
            this.listProvider = list;
            this.valueProvider = valueToFind;
        }

        public string SchemaReferenceKey => "listContainsValueCondition";

        public async Task<Expression> Resolve(IProviderContext providerContext, ExpressionScope scope = null)
        {
            providerContext.CancellationToken.ThrowIfCancellationRequested();
            var list = await this.listProvider.Invoke(providerContext, scope);
            var valueToCheck = await this.valueProvider.Invoke(providerContext, scope);

            var convertedList = Expression.TypeAs(list, typeof(IEnumerable<object>));
            var lambdaParameter = Expression.Parameter(typeof(object), "c");

            MethodInfo isEqualMethod = typeof(DataObjectHelper).GetMethod(nameof(DataObjectHelper.IsEqual));
            MethodCallExpression isEqualcall = Expression.Call(isEqualMethod, lambdaParameter, valueToCheck);
            LambdaExpression isEqualLambda = Expression.Lambda(isEqualcall, lambdaParameter);

            MethodInfo anyMethod = typeof(ListContainsValueFilterProvider).GetMethod(nameof(this.Any), BindingFlags.NonPublic | BindingFlags.Static);
            MethodCallExpression anyCall = Expression.Call(null, anyMethod, convertedList, isEqualLambda);
            Expression anyLambda = Expression.Lambda(anyCall, scope?.CurrentParameter);

            return anyLambda;
        }

        /// <summary>
        /// Wrapper method for Enumerable.Any. Needed to ensure that we are actually calling
        /// the method that we are looking for despite the numerous possible overloads.
        /// </summary>
        /// <param name="list">The list to be verified.</param>
        /// <param name="predicate">The function used for verification.</param>
        /// <returns>True if the list is confirmed to have the value needed, false otherwise.</returns>
        private static bool Any(IEnumerable<object> list, Func<object, bool> predicate)
        {
            return list.Any(x => predicate(x));
        }
    }
}
