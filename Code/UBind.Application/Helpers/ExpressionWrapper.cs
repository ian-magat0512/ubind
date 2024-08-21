// <copyright file="ExpressionWrapper.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Helper
{
    using System.Linq;
    using System.Linq.Expressions;
    using Newtonsoft.Json.Linq;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.PathLookup;

    /// <summary>
    /// This class is needed because we need a class that will extend the existing System.Linq.Expression class
    /// because System.Linq.Expression does not support nested properties.
    /// </summary>
    public static class ExpressionWrapper
    {
        /// <summary>
        /// Method for retrieving the absolute member expression from an object.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="path">The path of the property to get.</param>
        /// <returns>The member expression that will be used in the query.</returns>
        public static Expression GetProperty(Expression expression, string path, string providerName, JObject debugContext)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return expression;
            }

            var typeName = expression.Type.Name;
            if (path.StartsWith(typeName))
            {
                var propertyPath = path.Replace($"{typeName}.", string.Empty);
                return Expression.Property(expression, propertyPath);
            }
            else if (typeName.Equals(typeof(object).Name))
            {
                // 1. create expressions for each parameter of Resolve method from pocoPathLookupResolver
                // 2. get the method info for PocoPathLookupResolver.Resolve
                // 3. Evaluate the provider Result from pocoPathLookupResolver return and IData.GetValueFromGeneric
                var pathExpr = Expression.Constant($"/{path}");
                var providerNameExpr = Expression.Constant(providerName);
                var debugContextExpr = Expression.Constant(debugContext);
                var resolverMethodInfo = typeof(PocoPathLookupResolver).GetMethod(nameof(PocoPathLookupResolver.Resolve));
                var providerResultMethodInfo = typeof(ProviderResultExtensions).GetMethod(nameof(ProviderResultExtensions.GetValueOrThrowIfFailed))
                    .MakeGenericMethod(new Type[] { typeof(IData) });
                var iDataGetValueMethodInfo = typeof(IData).GetMethod(nameof(IData.GetValueFromGeneric));

                // 4. create the Expression.Call for the Resolve method
                // then pass its output to the Expression.Call for the GetValueFromGeneric method
                var staticCallExpr = Expression.Call(resolverMethodInfo, new Expression[] { expression, pathExpr, providerNameExpr, debugContextExpr });
                var getProviderResultExpr = Expression.Call(null, providerResultMethodInfo, staticCallExpr);
                var getInnerValueCallExpr = Expression.Call(getProviderResultExpr, iDataGetValueMethodInfo);
                return getInnerValueCallExpr;
            }

            var exp = NestedExpressionProperty(expression, path);
            return exp;
        }

        /// <summary>
        /// TODO: This logic does not query or create an expression properly for nested properties with arrays.
        /// Might need a separate ticket for this, but for now.
        /// </summary>
        private static MemberExpression NestedExpressionProperty(Expression expression, string propertyName)
        {
            string[] parts = propertyName.Split('.');
            int partsL = parts.Length;

            return (partsL > 1)
                ?
                Expression.Property(
                    NestedExpressionProperty(
                        expression,
                        parts.Take(partsL - 1).Aggregate((a, i) => a + "." + i)),
                    parts[partsL - 1])
                :
                Expression.Property(expression, propertyName);
        }
    }
}
