// <copyright file="IFilterProvider.cs" company="uBind">
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
    /// Provides a predicate used to filter items in a collection.
    /// </summary>
    public interface IFilterProvider
    {
        /// <summary>
        /// Gets the reference key for the given provider within the automation schema.
        /// </summary>
        string SchemaReferenceKey { get; }

        /// <summary>
        /// Resolve a predicate for filtering.
        /// </summary>
        /// <param name="providerContext">The data and path to perform resolutions with.</param>
        /// <param name="scope">The scope the filer will execute in.</param>
        /// <returns>An expression for the predicate for filtering.</returns>
        Task<Expression> Resolve(IProviderContext providerContext, ExpressionScope scope = null);
    }

    /// <summary>
    /// Extenion methods for the <see cref="IFilterProvider"/> interface.
    /// </summary>
    public static class IFilterProviderExtensions
    {
        /// <summary>
        /// Resolve a predicate for filtering as a typed lambda function so it can be used in where clauses etc.
        /// </summary>
        /// <typeparam name="TData">The type of item the predicate applies to.</typeparam>
        /// <param name="provider">The provider the extension method is acting upon.</param>
        /// <param name="providerContext">The data and path to perform resolutions with.</param>
        /// <param name="scope">The scope the filer will execute in.</param>
        /// <returns>An expression for the predicate for filtering.</returns>
        public static async Task<Expression<Func<TData, bool>>> Resolve<TData>(
            this IFilterProvider provider,
            IProviderContext providerContext,
            ExpressionScope scope = null) =>
            (Expression<Func<TData, bool>>)await provider.Resolve(providerContext, scope);
    }
}
