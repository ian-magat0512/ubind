// <copyright file="DateTimeComparisonFilterProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Filters
{
    using System.Linq.Expressions;
    using NodaTime;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Expression;
    using UBind.Domain;
    using UBind.Domain.Exceptions;

    public class DateTimeComparisonFilterProvider : IFilterProvider
    {
        private readonly IExpressionProvider firstExpression;
        private readonly IExpressionProvider secondExpression;
        private readonly Func<Expression, Expression, Expression> comparisonProviderFactory;

        public DateTimeComparisonFilterProvider(
            Func<Expression, Expression, Expression> comparisonProviderFactory,
            IExpressionProvider first,
            IExpressionProvider second,
            string schemaReferenceKey)
        {
            this.comparisonProviderFactory = comparisonProviderFactory;
            this.firstExpression = first;
            this.secondExpression = second;
            this.SchemaReferenceKey = schemaReferenceKey;
        }

        public string SchemaReferenceKey { get; }

        public async Task<Expression> Resolve(IProviderContext providerContext, ExpressionScope scope = null)
        {
            providerContext.CancellationToken.ThrowIfCancellationRequested();
            var firstExpression = await this.firstExpression.Invoke(providerContext, scope);
            var secondExpression = await this.secondExpression.Invoke(providerContext, scope);
            if (firstExpression.Type != typeof(object) && firstExpression.Type != typeof(long))
            {
                throw new ErrorException(Errors.Automation.ParameterValueTypeInvalid(
                    this.SchemaReferenceKey,
                    "dateTime"));
            }

            var leftExpression = firstExpression.Type != typeof(long)
                ? this.ConvertInputToTicks(firstExpression)
                : firstExpression;
            var rightExpression = secondExpression.Type != typeof(long)
                ? this.ConvertInputToTicks(secondExpression)
                : secondExpression;
            var comparisonExpresion = this.comparisonProviderFactory.Invoke(leftExpression, rightExpression);
            return Expression.Lambda(comparisonExpresion, scope?.CurrentParameter);
        }

        /// <summary>
        /// This method ensures that a value of datetime, e.g. '2023-03-09T13:09:18.2951381+11'
        /// can be parsed to an Instant and then into Ticks which is the one used for datetime comparisons.
        /// </summary>
        /// <param name="inputExpression">An input expression whose value should be a datetime object.</param>
        /// <returns>An expression of type long representing the ticks for the input passed.</returns>
        /// <remarks>This method is only used if the passed input is from an objectified list to be filtered, rather than
        /// a query for the database.</remarks>
        private Expression ConvertInputToTicks(Expression inputExpression)
        {
            var objectToStringMethod = typeof(object).GetMethod("ToString");
            var toStringExpression = Expression.Call(inputExpression, objectToStringMethod);
            var dateTimeOffsetParseMethod = typeof(DateTimeOffset).GetMethod("Parse", new[] { typeof(string) });
            var dateTimeOffsetParseExpr = Expression.Call(dateTimeOffsetParseMethod, toStringExpression);
            var instantConversionMethod = typeof(Instant).GetMethod("FromDateTimeOffset");
            var instantConversionExpression = Expression.Call(instantConversionMethod, dateTimeOffsetParseExpr);

            var toUnixTimeMethod = typeof(Instant).GetMethod("ToUnixTimeTicks");
            var toTicksExpression = Expression.Call(instantConversionExpression, toUnixTimeMethod);
            return toTicksExpression;
        }
    }
}
