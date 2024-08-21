// <copyright file="ExpressionHelper.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Extensions
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;

    /// <summary>
    /// The expression helper.
    /// </summary>
    public static class ExpressionHelper
    {
        /// <summary>
        /// Appends two expressions with AND operator.
        /// </summary>
        public static Expression<Func<T, bool>> AndExpression<T>(
            Expression<Func<T, bool>> left,
            Expression<Func<T, bool>> right)
        {
            var visitor = new ParameterReplaceVisitor()
            {
                Target = right.Parameters[0],
                Replacement = left.Parameters[0],
            };

            var rewrittenRight = visitor.Visit(right.Body);
            var andExpression = Expression.AndAlso(left.Body, rewrittenRight);
            return Expression.Lambda<Func<T, bool>>(andExpression, left.Parameters);
        }

        public static Expression<Func<T, bool>> GreaterThanExpression<T>(string propertyName, object valueToCompare)
        {
            Type type = typeof(T);
            PropertyInfo property = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            var parameter = Expression.Parameter(type, "greaterThan");
            var lambdaExpression = Expression.Lambda<Func<T, bool>>(
                    Expression.GreaterThan(
                        Expression.Property(parameter, property),
                        Expression.Constant(valueToCompare)), parameter);

            return lambdaExpression;
        }

        public static Expression<Func<T, bool>> LessThanExpression<T>(string propertyName, object valueToCompare)
        {
            Type type = typeof(T);
            PropertyInfo property = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            var parameter = Expression.Parameter(type, "lessThan");
            var lambdaExpression = Expression.Lambda<Func<T, bool>>(
                    Expression.LessThan(
                        Expression.Property(parameter, property),
                        Expression.Constant(valueToCompare)), parameter);

            return lambdaExpression;
        }
    }

#pragma warning disable SA1402 // File may only contain a single type
    public class ParameterReplaceVisitor : ExpressionVisitor
#pragma warning restore SA1402 // File may only contain a single type
    {
        public ParameterExpression Target { get; set; }

        public ParameterExpression Replacement { get; set; }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return node == this.Target ? this.Replacement : base.VisitParameter(node);
        }
    }
}
