// <copyright file="ObjectPathLookupExpressionTimeProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Expression
{
    using System.Globalization;
    using System.Linq.Expressions;
    using UBind.Application.Automation.Providers.Object;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;

    public class ObjectPathLookupExpressionTimeProvider<TData> : ObjectPathLookupExpressionProvider<TData>, IExpressionProvider
    {
        public ObjectPathLookupExpressionTimeProvider(
            IProvider<Data<string>> pathProvider,
            IProvider<IData> defaultValueProvider = null,
            IObjectProvider objectProvider = null)
            : base(pathProvider, defaultValueProvider, objectProvider)
        {
        }

        public override string SchemaReferenceKey => "objectPathLookupTimeExpression";

        public async Task<Expression> Invoke(IProviderContext providerContext, ExpressionScope scope)
        {
            providerContext.CancellationToken.ThrowIfCancellationRequested();
            var lookupDataExpression = await base.Invoke(providerContext, scope);
            if (lookupDataExpression.Type == typeof(object))
            {
                var objectToString = typeof(object).GetMethod(nameof(object.ToString));
                var toStringCall = Expression.Call(lookupDataExpression, objectToString);
                var toTimeMethodInfo = typeof(StringExtensions).GetMethod(nameof(StringExtensions.ToLocalTimeFromIso8601OrhmmttOrhhmmttOrhhmmssttWithCulture));
                return Expression.Call(toTimeMethodInfo, toStringCall, Expression.Constant(CultureInfo.GetCultureInfo(Locales.en_AU)));
            }

            return lookupDataExpression;
        }
    }
}
