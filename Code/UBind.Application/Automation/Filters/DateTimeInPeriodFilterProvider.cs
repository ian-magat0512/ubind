// <copyright file="DateTimeInPeriodFilterProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Filters
{
    using System;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using NodaTime;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Expression;

    /// <summary>
    /// Provider for an expression-based filter that tests whether a given date time in in a given period.
    /// </summary>
    public class DateTimeInPeriodFilterProvider : IFilterProvider
    {
        private readonly IExpressionProvider dateTimeExpresssionProvider;
        private readonly IProvider<Data<Interval>> intervalProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="DateTimeInPeriodFilterProvider"/> class.
        /// </summary>
        /// <param name="dateTimeExpresssionProvider">A provider for an expression for a date time represented as a number of ticks.</param>
        /// <param name="intervalProvider">A provider for the interval to test.</param>
        public DateTimeInPeriodFilterProvider(
            IExpressionProvider dateTimeExpresssionProvider,
            IProvider<Data<Interval>> intervalProvider)
        {
            this.dateTimeExpresssionProvider = dateTimeExpresssionProvider;
            this.intervalProvider = intervalProvider;
        }

        public string SchemaReferenceKey => "dateTimeIsInPeriodCondition";

        /// <inheritdoc/>
        public async Task<Expression> Resolve(IProviderContext providerContext, ExpressionScope scope = null)
        {
            var dateTimeExpression = await this.dateTimeExpresssionProvider.Invoke(providerContext, scope);
            var instantExpression = dateTimeExpression.Type == typeof(long)
                ? dateTimeExpression
                : dateTimeExpression.Type == typeof(long?)
                    ? this.ConvertNullableLongToLong(dateTimeExpression)
                    : this.ConvertInputToTicks(dateTimeExpression);
            Interval interval = (await this.intervalProvider.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue;
            var startExpression = Expression.Constant(interval.Start.ToUnixTimeTicks());
            var endExpression = Expression.Constant(interval.End.ToUnixTimeTicks());
            var inPeriodExpression = Expression.AndAlso(
                 Expression.GreaterThanOrEqual(instantExpression, startExpression),
                 Expression.LessThan(instantExpression, endExpression));
            var lambdaExpression = Expression.Lambda(inPeriodExpression, scope?.CurrentParameter);
            return lambdaExpression;
        }

        /// <summary>
        /// Converts a nullable long type to a long type.
        /// </summary>
        /// <returns>an expression</returns>
        private Expression ConvertNullableLongToLong(Expression expression)
        {
            // Check if the expression is null
            var isNull = Expression.Equal(expression, Expression.Constant(null, typeof(long?)));

            // Create an expression for '0' as long
            var zeroExpression = Expression.Constant(0L, typeof(long));

            // Use the original expression if it's not null, otherwise use zero
            var ifThenElse = Expression.Condition(
                isNull,
                zeroExpression,
                Expression.Convert(expression, typeof(long)) // Convert long? to long
            );

            return ifThenElse;
        }

        /// <summary>
        /// This method ensures that a value of datetime, e.g. '2023-03-09T13:09:18.2951381+11'
        /// can be parsed to an Instant and then into Ticks whcih can then be used to compare against
        /// the interval.
        /// </summary>
        /// <param name="inputExpression">An input expression whose value should be a datetime offset of any type.</param>
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
