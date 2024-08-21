// <copyright file="ObjectPathLookupConditionFilterProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Filters
{
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Threading.Tasks;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Expression;
    using UBind.Application.Automation.Providers.Object;

    public class ObjectPathLookupConditionFilterProvider : IFilterProvider
    {

        private readonly IProvider<Data<string>> pathProvider;
        private readonly IProvider<IData> defaultValueProvider;
        private readonly IObjectProvider objectProvider;

        public ObjectPathLookupConditionFilterProvider(
            IProvider<Data<string>> pathProvider,
            IProvider<IData> defaultValueProvider = null,
            IObjectProvider objectProvider = null)
        {
            this.pathProvider = pathProvider;
            this.defaultValueProvider = defaultValueProvider;
            this.objectProvider = objectProvider;
        }

        public string SchemaReferenceKey => "objectPathLookupCondition";

        public async Task<Expression> Resolve(IProviderContext providerContext, ExpressionScope scope = null)
        {
            providerContext.CancellationToken.ThrowIfCancellationRequested();
            var expressionProvider = new ObjectPathLookupExpressionProvider<bool>(this.pathProvider, this.defaultValueProvider, this.objectProvider);
            var expression = await expressionProvider.Invoke(providerContext, scope);
            if (expression.Type != typeof(bool))
            {
                var methodInfo = typeof(ObjectPathLookupConditionFilterProvider)
                    .GetMethod(nameof(this.TryParseWrapper), BindingFlags.NonPublic | BindingFlags.Instance);
                var methodCall = Expression.Call(
                    Expression.Constant(this),
                    methodInfo,
                    expression);
                return Expression.Lambda(methodCall, scope.CurrentParameter);
            }

            return Expression.Lambda(expression, scope.CurrentParameter);
        }

        private bool TryParseWrapper(string value)
        {
            bool success = bool.TryParse(value, out bool result);
            return success ? result : false;
        }
    }
}
