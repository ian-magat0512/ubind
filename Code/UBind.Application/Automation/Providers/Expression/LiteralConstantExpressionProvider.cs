// <copyright file="LiteralConstantExpressionProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Expression
{
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    /// <summary>
    /// Provider for constant expressions for use in filters.
    /// </summary>
    public class LiteralConstantExpressionProvider : IExpressionProvider
    {
        private readonly ConstantExpression expression;

        /// <summary>
        /// Initializes a new instance of the <see cref="LiteralConstantExpressionProvider"/> class.
        /// </summary>
        /// <param name="expression">The expression to provide.</param>
        public LiteralConstantExpressionProvider(ConstantExpression expression) =>
            this.expression = expression;

        public string SchemaReferenceKey => "expression";

        /// <inheritdoc/>
        public Task<Expression> Invoke(IProviderContext providerContext, ExpressionScope scope = null) =>
            Task.FromResult((Expression)this.expression);
    }
}
