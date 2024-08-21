// <copyright file="SearchAustralianBusinessRegisterByNameQuery.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.AbnLookup
{
    using CSharpFunctionalExtensions;
    using UBind.Domain;
    using UBind.Domain.AbnLookup;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// This class is needed because we need a query to search Australian business register by name.
    /// </summary>
    public class SearchAustralianBusinessRegisterByNameQuery : IQuery<Result<AbnNameSearchResponse, Error>>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SearchAustralianBusinessRegisterByNameQuery"/> class.
        /// </summary>
        /// <param name="search">Used to perform a search using the ABR API.</param>
        /// <param name="maxResults">The max number of results, default is 10.</param>
        /// <param name="includeEntityNames">Specifies whether results matched against entityNames should be included in the response.</param>
        /// <param name="includeBusinessNames">Specifies whether results matched against businessNames should be included in the response.</param>
        /// <param name="includeTradingNames">Specifies whether results matched against tradingNames should be included in the response.</param>
        /// <param name="includeCancelledRegistrations">Specifies whether results matched against cancelled registrations should be included in the response.</param>
        public SearchAustralianBusinessRegisterByNameQuery(
            string search,
            int maxResults,
            bool includeEntityNames,
            bool includeBusinessNames,
            bool includeTradingNames,
            bool includeCancelledRegistrations)
        {
            this.Search = search;
            this.MaxResults = maxResults;
            this.IncludeEntityNames = includeEntityNames;
            this.IncludeBusinessNames = includeBusinessNames;
            this.IncludeTradingNames = includeTradingNames;
            this.IncludeCancelledRegistrations = includeCancelledRegistrations;
        }

        /// <summary>
        /// Gets a value used to perform a search using the ABR API..
        /// </summary>
        public string Search { get; private set; }

        /// <summary>
        /// Gets the max number of results, default is 10.
        /// </summary>
        public int MaxResults { get; private set; }

        /// <summary>
        /// Gets a value indicating whether results matched against entity names should be included in the response.
        /// </summary>
        public bool IncludeEntityNames { get; private set; }

        /// <summary>
        /// Gets a value indicating whether results matched against business names should be included in the response.
        /// </summary>
        public bool IncludeBusinessNames { get; private set; }

        /// <summary>
        /// Gets a value indicating whether results matched against trading names should be included in the response.
        /// </summary>
        public bool IncludeTradingNames { get; private set; }

        /// <summary>
        /// Gets a value indicating whether results matched against cancelled registrations should be included in the response.
        /// </summary>
        public bool IncludeCancelledRegistrations { get; private set; }

        public bool IsValid
        {
            get
            {
                if (long.TryParse(this.Search.Replace(" ", string.Empty), out long abn) && abn.ToString().Length >= 9)
                {
                    return false;
                }

                return true;
            }
        }
    }
}
