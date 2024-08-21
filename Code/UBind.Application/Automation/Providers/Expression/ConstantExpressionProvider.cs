// <copyright file="ConstantExpressionProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Expression
{
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers;

    /// <summary>
    /// For providing constant expressions for use in filters.
    /// </summary>
    public class ConstantExpressionProvider : IExpressionProvider
    {
        private IProvider<IData> dataProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConstantExpressionProvider"/> class.
        /// </summary>
        /// <param name="dataProvider">A provider for the value to use in the expression.</param>
        public ConstantExpressionProvider(IProvider<IData> dataProvider) =>
            this.dataProvider = dataProvider;

        public string SchemaReferenceKey => "expression";

        /// <inheritdoc/>
        public async Task<Expression> Invoke(IProviderContext providerContext, ExpressionScope scope = null)
        {
            providerContext.CancellationToken.ThrowIfCancellationRequested();
            var datum = (await this.dataProvider.Resolve(providerContext)).GetValueOrThrowIfFailed();
            var wrappedDatum = datum as IDataWrapper;
            if (wrappedDatum != null)
            {
                return Expression.Constant(wrappedDatum.Data);
            }

            return Expression.Constant(datum);
        }
    }
}
