// <copyright file="LiteralConstantExpressionProviderConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Expression
{
    using System;
    using System.Linq.Expressions;
    using UBind.Application.Automation;

    /// <summary>
    /// Model for building constant expressions for use in filters from literals.
    /// </summary>
    public class LiteralConstantExpressionProviderConfigModel : IBuilder<IExpressionProvider>
    {
        private readonly ConstantExpression expression;

        /// <summary>
        /// Initializes a new instance of the <see cref="LiteralConstantExpressionProviderConfigModel"/> class.
        /// </summary>
        /// <param name="expression">The constant expression to use.</param>
        public LiteralConstantExpressionProviderConfigModel(ConstantExpression expression) =>
            this.expression = expression;

        /// <inheritdoc/>
        public IExpressionProvider Build(IServiceProvider serviceProvider)
        {
            return new LiteralConstantExpressionProvider(this.expression);
        }
    }
}
