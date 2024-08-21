// <copyright file="ExpressionExtensions.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Extensions
{
    using System;
    using System.Linq.Expressions;
    using NodaTime;
    using UBind.Domain.Exceptions;

    /// <summary>
    /// Extension methods for System.Linq.Expression.
    /// </summary>
    public static class ExpressionExtensions
    {
        /// <summary>
        /// Negates a predicate expression.
        /// </summary>
        /// <typeparam name="T">The return type of the predicate.</typeparam>
        /// <param name="predicate">The predicate expression.</param>
        /// <returns>A negated predicate expression.</returns>
        public static Expression<Func<T, bool>> Not<T>(this Expression<Func<T, bool>> predicate)
        {
            var candidateExpr = predicate.Parameters[0];
            var body = Expression.Not(predicate.Body);

            return Expression.Lambda<Func<T, bool>>(body, candidateExpr);
        }

        /// <summary>
        /// This method ensures that a value of date or datetime, e.g. '2023-03-09T13:09:18.2951381+11' is parsed to date with time set to midnight
        /// can be parsed to an Instant and then into ticks which is used for date comparisons.
        /// </summary>
        /// <param name="inputExpression">An input expression whose value should be a date or datetime.</param>
        /// <returns>An expression of type long representing the ticks for the input's date value at midnight.</returns>
        /// <remarks>This method is only used if the passed input is from an objectified list to be filtered, rather than
        /// a query for the database.</remarks>
        public static Expression ToTicksAtMidnight(this Expression inputExpression)
        {
            if (inputExpression.Type != typeof(LocalDate))
            {
                throw new ErrorException(Domain.Errors.General.Unexpected($"Expected an expression of type DateTime when converting to ticks but got {inputExpression.Type}"));
            }

            Expression dateTimeExpression = inputExpression;
            var atMidnightMethod = typeof(LocalDate).GetMethod(nameof(LocalDate.AtMidnight));
            var atMidnightExpression = Expression.Call(dateTimeExpression, atMidnightMethod);
            var atUtcMethod = typeof(LocalDateTime).GetMethod(nameof(LocalDateTime.InUtc));
            var atUtcExpression = Expression.Call(atMidnightExpression, atUtcMethod);

            var toInstantMethod = typeof(ZonedDateTime).GetMethod(nameof(ZonedDateTime.ToInstant));
            var toInstantExpr = Expression.Call(atUtcExpression, toInstantMethod);

            var toUnixTimeMethod = typeof(Instant).GetMethod(nameof(Instant.ToUnixTimeTicks));
            var toTicksExpression = Expression.Call(toInstantExpr, toUnixTimeMethod);
            return toTicksExpression;
        }
    }
}
