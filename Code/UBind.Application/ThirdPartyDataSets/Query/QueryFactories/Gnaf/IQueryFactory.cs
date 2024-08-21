// <copyright file="IQueryFactory.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.ThirdPartyDataSets.Query.QueryFactories.Gnaf
{
    /// <summary>
    /// Provides the contract to be used by Gnaf query factory.
    /// </summary>
    public interface IQueryFactory
    {
        /// <summary>
        /// Create a query filter fields.
        /// </summary>
        /// <param name="searchQuery">The search query.</param>
        /// <returns>Return the list of search fields.</returns>
        string[] CreateQueryFilterFields(string searchQuery);

        /// <summary>
        /// Create query terms from the raw search string.
        /// </summary>
        /// <param name="rawSearchString">The raw search string.</param>
        /// <returns>Return the query term from the raw search string.</returns>
        string CreateQueryTerms(string rawSearchString);
    }
}
