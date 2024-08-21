// <copyright file="IQueryableExtensions.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Extensions
{
    using System.Data.Entity.Core.Objects;
    using System.Linq;
    using UBind.Domain.Enums;
    using UBind.Domain.Extensions.Implementations;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// Custom methods for collections.
    /// </summary>
    public static class IQueryableExtensions
    {
#pragma warning disable SA1401 // Fields should be private
        // Move the implementation logic to a separate non-static class so we can test it.
        public static IQueryableExtensionsImplementation Implementation = new QueryableExtensionsImplementation();
#pragma warning restore SA1401 // Fields should be private

        /// <summary>
        /// Generates the raw SQL for a query, for debugging purposes.
        /// </summary>
        /// <typeparam name="T">The type the IQueryable returns.</typeparam>
        /// <param name="t">The query.</param>
        /// <returns>The raw SQL string.</returns>
        public static string ToTraceString<T>(this IQueryable<T> t)
        {
            string sql = string.Empty;
            ObjectQuery<T> oqt = t as ObjectQuery<T>;
            if (oqt != null)
            {
                sql = oqt.ToTraceString();
            }

            return sql;
        }

        /// <summary>
        /// Gets page items for the given ReadModelFilters ,page and size.
        /// </summary>
        /// <param name="list">The full list of items you would like to paginate.</param>
        /// <param name="filter">filters.</param>
        /// <typeparam name="T">The type of each collection element.</typeparam>
        /// <returns>Items for the given page.</returns>
        public static IQueryable<T> Paginate<T>(this IQueryable<T> list, EntityListFilters filter)
        {
            return Implementation.Paginate(list, filter);
        }

        /// <summary>
        /// Gets ordered list for the given ReadModelFilters.
        /// </summary>
        /// <param name="list">The full list of items you would like to order.</param>
        /// <param name="propertyName">The Property Name to be order.</param>
        /// <param name="sortDirection">Sort Order to be use.</param>
        /// <typeparam name="T">The type of each collection element.</typeparam>
        /// <returns>Items that is ordered by.</returns>
        public static IQueryable<T> Order<T>(this IQueryable<T> list, string propertyName, SortDirection sortDirection)
        {
            return Implementation.Order(list, propertyName, sortDirection);
        }
    }
}
