// <copyright file="ObjectPathLookupExpressionDateProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Expression
{
    using System.Linq.Expressions;
    using UBind.Application.Automation.Providers.Object;
    using UBind.Domain.Extensions;

    public class ObjectPathLookupExpressionDateProvider<TData> : ObjectPathLookupExpressionProvider<TData>, IExpressionProvider
    {
        public ObjectPathLookupExpressionDateProvider(IProvider<Data<string>> pathProvider, IProvider<IData> defaultValueProvider = null, IObjectProvider objectProvider = null)
            : base(pathProvider, defaultValueProvider, objectProvider)
        {
        }

        public string SchemaReferenceKey => "objectPathLookupDateExpression";

        public async Task<Expression> Invoke(IProviderContext providerContext, ExpressionScope scope = null)
        {
            providerContext.CancellationToken.ThrowIfCancellationRequested();
            var lookupDataExpression = await base.Invoke(providerContext, scope);
            if (lookupDataExpression.Type == typeof(object))
            {
                var objectToStringMethod = typeof(object).GetMethod(nameof(object.ToString));
                var toStringCall = Expression.Call(lookupDataExpression, objectToStringMethod);
                var toLocalDateMethodInfo = typeof(StringExtensions).GetMethod(nameof(StringExtensions.ToLocalDateFromIso8601OrDateTimeIso8601));
                return Expression.Call(toLocalDateMethodInfo, toStringCall);
            }

            return lookupDataExpression;
        }
    }
}
