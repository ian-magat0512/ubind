// <copyright file="AddressSearchBySearchStringQuery.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Queries.ThirdPartyDataSets.Gnaf
{
    using System.Collections.Generic;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ThirdPartyDataSets.Gnaf;

    /// <summary>
    /// Represents the Query for obtaining the Gnaf addresses by search string and maximum result.
    /// </summary>
    public class AddressSearchBySearchStringQuery : IQuery<IReadOnlyList<MaterializedAddressView>>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AddressSearchBySearchStringQuery"/> class.
        /// </summary>
        /// <param name="searchString">The search string value.</param>
        /// <param name="maxResults">The maximum results to be used to limit the search results.</param>
        public AddressSearchBySearchStringQuery(string searchString, int maxResults = 20)
        {
            this.SearchString = searchString;
            this.MaxResults = maxResults;
        }

        /// <summary>
        /// Gets the search string.
        /// </summary>
        public string SearchString { get; }

        /// <summary>
        /// Gets the maximum results.
        /// </summary>
        public int MaxResults { get; }
    }
}
