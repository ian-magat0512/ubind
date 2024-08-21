// <copyright file="IDataList.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Providers.List
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using MorseCode.ITask;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Filters;

    /// <summary>
    /// Interface for data collections that supports basic linq-style operations.
    /// </summary>
    /// <typeparam name="TData">The type of data stored in the collection.</typeparam>
    public interface IDataList<out TData> : IEnumerable<TData>, IData
    {
        /// <summary>
        /// Gets the type of the items in the collection (used in query expressions).
        /// </summary>
        Type DataType { get; }

        /// <summary>
        /// Gets the query used to generate the items in the collection.
        /// </summary>
        IQueryable<TData> Query { get; }

        /// <summary>
        /// Filters the collection.
        /// </summary>
        /// <param name="itemAlias">An alias for accessing collection items in filters. If no item alias is provided, a default one will be generated.</param>
        /// <param name="filterProvider">A factory for creating the predicate used to filter the collection.</param>
        /// <param name="providerContext">The automation data context.</param>
        /// <returns>An <see cref="IDataList{TData}"/> that contains elements from the collection that
        /// satisfy the condition specified by the predicate.</returns>
        ITask<IDataList<TData>> Where(
            string? itemAlias,
            IFilterProvider filterProvider,
            IProviderContext providerContext);

        /// <summary>
        /// Returns the first element of the collection that satisfies a specified condition or a default value if no
        /// such element is found.
        /// </summary>
        /// <param name="itemAlias">An alias for accessing collection items in filters.</param>
        /// <param name="filterProvider">A factory for creating the predicate used to test the elements.</param>
        /// <param name="providerContext">The automation data context.</param>
        /// <returns>The first element from the collection that satisfies the condition specified by the predicate, or
        /// default(TData) if none is found.</returns>
        ITask<TData> FirstOrDefault(string itemAlias, IFilterProvider? filterProvider, IProviderContext providerContext);

        /// <summary>
        /// Counts the elements in the collection.
        /// </summary>
        /// <returns>The count.</returns>
        Data<int> Count();
    }
}
