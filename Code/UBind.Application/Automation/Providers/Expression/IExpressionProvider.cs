// <copyright file="IExpressionProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Expression
{
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    /// <summary>
    /// For providing expressions for use in filters.
    /// </summary>
    public interface IExpressionProvider
    {
        /// <summary>
        /// Gets the reference key for the given provider within the automation schema.
        /// </summary>
        string SchemaReferenceKey { get; }

        /// <summary>
        /// Provide an expression for use in a filter.
        /// </summary>
        /// <param name="providerContext">The data and path to perform resolutions with.</param>
        /// <param name="scope">The scope in which the expression will be evaluated (can include parameter expressions).</param>
        /// <returns>An expression that can be used in a filter.</returns>
        Task<Expression> Invoke(IProviderContext providerContext, ExpressionScope scope);
    }
}
