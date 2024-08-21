// <copyright file="DatabasePropertyTranslator.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Expression
{
    using System.Linq;
    using System.Linq.Expressions;
    using UBind.Domain.Attributes;

    /// <summary>
    /// Helper for translating property expressions used in database queries when properties
    /// need to be mapped to other properties that correspond to database columns.
    /// </summary>
    public static class DatabasePropertyTranslator
    {
        /// <summary>
        /// Translates a property expression to use an alternative property on the same class
        /// if one is specified using the <see cref="DatabasePropertyAttribute"/>.
        /// </summary>
        /// <param name="expression">The expression to translate.</param>
        /// <returns>
        /// An expression for the alternative property if one is specified, otherwise the
        /// original property.
        /// </returns>
        public static Expression Translate(Expression expression)
        {
            MemberExpression memberExpression = expression as MemberExpression;
            var databasePropertyAttributes = memberExpression?.Member.GetCustomAttributes(
                typeof(DatabasePropertyAttribute), false);
            if (databasePropertyAttributes?.Any() ?? false)
            {
                var attribute = (DatabasePropertyAttribute)databasePropertyAttributes[0];
                return Expression.Property(memberExpression.Expression, attribute.PropertyName);
            }

            return expression;
        }
    }
}
