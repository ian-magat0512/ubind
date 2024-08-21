// <copyright file="TextIsEqualToFilterProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Filters
{
    using System;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Expression;

    /// <summary>
    /// Model for building an condition provider for filtering a collection based on text equality.
    /// </summary>
    public class TextIsEqualToFilterProvider : IFilterProvider
    {
        private IExpressionProvider text;
        private IExpressionProvider isEqualTo;

        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateFilterProvider"/> class.
        /// </summary>
        public TextIsEqualToFilterProvider(IExpressionProvider text, IExpressionProvider isEqualTo)
        {
            this.text = text;
            this.isEqualTo = isEqualTo;
        }

        public virtual string SchemaReferenceKey { get; } = "textIsEqualToCondition";

        /// <inheritdoc/>
        public async Task<Expression> Resolve(IProviderContext providerContext, ExpressionScope scope)
        {
            providerContext.CancellationToken.ThrowIfCancellationRequested();
            var provider = new BinaryExpressionFilterProvider(
                (text, isEqualTo) =>
                {
                    return this.EvaluateResult(text, isEqualTo);
                },
                this.text,
                this.isEqualTo,
                this.SchemaReferenceKey);
            var exp = await provider.Resolve(providerContext, scope);
            return exp;
        }

        private Expression EvaluateResult(Expression text, Expression isEqualTo)
        {
            if (this.IsEnum(text))
            {
                return this.EquateEnum(text, isEqualTo);
            }
            else
            {
                return this.EquateString(text, isEqualTo);
            }
        }

        private Expression EquateString(Expression left, Expression right)
        {
            return Expression.Equal(left, right);
        }

        private Expression EquateEnum(Expression left, Expression right)
        {
            if (left is MethodCallExpression methodCall
                && methodCall.Object != null
                && methodCall.Object.NodeType == ExpressionType.Convert)
            {
                // Separating the expressions
                // This is the Convert expression
                UnaryExpression convertExpression = (UnaryExpression)methodCall.Object;

                // This is the original expression
                Expression originalExpression = convertExpression.Operand;

                Expression leftExpression = originalExpression;
                var leftType = originalExpression.Type;
                var leftAsInt = Expression.Convert(leftExpression, typeof(int));
                var rightConstant = (ConstantExpression)right;
                var rightValue = rightConstant.Value.ToString();
                if (Enum.TryParse(leftType, rightValue, true, out object? val))
                {
                    Expression rightExp = Expression.Constant((int)val);
                    return Expression.Equal(leftAsInt, rightExp);
                }
            }

            // equate it as string instead.
            return this.EquateString(left, right);
        }

        private bool IsEnum(Expression expression)
        {
            if (expression is MethodCallExpression methodCall
                && methodCall.Object != null
                && methodCall.Object.NodeType == ExpressionType.Convert)
            {
                // This is the Convert expression
                UnaryExpression convertExpression = (UnaryExpression)methodCall.Object;

                // This is the original expression
                Expression innerExpression = convertExpression.Operand;
                var innerExpressionType = innerExpression.Type;
                return innerExpressionType == null ? false : innerExpressionType.IsEnum;
            }

            return false;
        }
    }
}
