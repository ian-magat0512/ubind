// <copyright file="SearchAustralianBusinessRegisterByNameQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.AbnLookup
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using CSharpFunctionalExtensions;
    using UBind.Application.ConnectedServices.AbrXmlSearchRpc;
    using UBind.Domain;
    using UBind.Domain.AbnLookup;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// This class is needed because we need a query handler for searching Australian business register by name.
    /// </summary>
    public class SearchAustralianBusinessRegisterByNameQueryHandler :
        IQueryHandler<SearchAustralianBusinessRegisterByNameQuery, Result<AbnNameSearchResponse, Error>>
    {
        private const string NoRecordsFound = "No records found";
        private readonly IAbnLookupConfiguration abnLookupConfiguration;

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchAustralianBusinessRegisterByNameQueryHandler"/> class.
        /// </summary>
        /// <param name="abnLookupConfiguration">The ABN lookup configuration.</param>
        public SearchAustralianBusinessRegisterByNameQueryHandler(IAbnLookupConfiguration abnLookupConfiguration)
        {
            this.abnLookupConfiguration = abnLookupConfiguration;
        }

        /// <inheritdoc/>
        public async Task<Result<AbnNameSearchResponse, Error>> Handle(
            SearchAustralianBusinessRegisterByNameQuery request, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!request.IncludeEntityNames && !request.IncludeBusinessNames && !request.IncludeTradingNames)
            {
                return Result.Failure<AbnNameSearchResponse, Error>(Errors.AbnLookup.NoNametypesIncludedInSearch());
            }

            // Checking the search value if it is an integer and longer than 9 digits
            // then will return an empty result instead of making a request to the ABR API
            if (!request.IsValid)
            {
                return Result.Success<AbnNameSearchResponse, Error>(new AbnNameSearchResponse(new List<AbnRegistration>()));
            }

            var searchResult = await this.SearchAbnRegisterByName(request, cancellationToken);
            var abnRegistrations = new List<AbnRegistration>();

            var list = searchResult as ResponseSearchResultsList;
            if (list == null)
            {
                var responseException = (ResponseException)searchResult;
                if (responseException.Description.Equals(NoRecordsFound, StringComparison.OrdinalIgnoreCase))
                {
                    return Result.Success<AbnNameSearchResponse, Error>(new AbnNameSearchResponse(abnRegistrations));
                }

                var error = Errors.General.BadRequest($"{responseException.Code}: {responseException.Description}");
                return Result.Failure<AbnNameSearchResponse, Error>(error);
            }

            foreach (var record in list.SearchResultsRecord)
            {
                var item = this.GetAbnRegistrationDetail(record);
                abnRegistrations.Add(item);
            }

            var response = new AbnNameSearchResponse(abnRegistrations.OrderByDescending(n => n.Score));
            return Result.Success<AbnNameSearchResponse, Error>(response);
        }

        private async Task<object> SearchAbnRegisterByName(SearchAustralianBusinessRegisterByNameQuery request, CancellationToken cancellationToken)
        {
            var client = new ABRXMLSearchRPCSoapClient(
                Enum.Parse<ABRXMLSearchRPCSoapClient.EndpointConfiguration>(this.abnLookupConfiguration.EndpointConfigurationNameRpc));

            var searchCriteria = new ExternalRequestNameSearchAdvanced2017 { Name = request.Search };

            searchCriteria.MaxSearchResults = request.MaxResults.ToString();

            // This doesn't have a 2017 version, TradingName is not available in 2018
            var filterNameType = new ExternalRequestFilterNameType2012
            {
                BusinessName = request.IncludeBusinessNames ? "Y" : "N",
                LegalName = request.IncludeEntityNames ? "Y" : "N",
                TradingName = request.IncludeTradingNames ? "Y" : "N",
            };

            // ActiveABNsOnly filter is only available in 2017
            var filters = new ExternalRequestFilters2017
            {
                NameType = filterNameType,
                ActiveABNsOnly = request.IncludeCancelledRegistrations ? "N" : "Y",
            };
            searchCriteria.Filters = filters;
            var searchTask = client.ABRSearchByNameAdvanced2017Async(searchCriteria, this.abnLookupConfiguration.UBindGuid);
            var taskCompletionSource = new TaskCompletionSource<object>();
            cancellationToken.Register(() =>
            {
                taskCompletionSource.TrySetCanceled();
            });
            await Task.WhenAny(searchTask, taskCompletionSource.Task);
            Payload? searchResult = null;
            if (searchTask.IsCompleted)
            {
                taskCompletionSource.TrySetResult(null);
                searchResult = await searchTask;
            }
            else if (cancellationToken.IsCancellationRequested)
            {
                throw new OperationCanceledException();
            }

            return searchResult.Response.ResponseBody;
        }

        private AbnRegistration GetAbnRegistrationDetail(SearchResultsRecord row)
        {
            var abn = row.ABN.FirstOrDefault();
            var name = row.Name.FirstOrDefault();
            var nameType = row.NameType.FirstOrDefault();
            var mainBusinessPhysicalAddress = row.MainBusinessPhysicalAddress.FirstOrDefault();

            var (fullName, score) = nameType == NameType.legalName
                ? (((Individual)name).FullName, ((IndividualSimpleName)name).Score)
                : (((Organisation)name).OrganisationName, ((OrganisationSimpleName)name).Score);

            return new AbnRegistration(
                abn.IdentifierValue,
                abn.IdentifierStatus.ToLower(),
                fullName,
                this.GetNameTypeDisplay(nameType.ToString()),
                int.TryParse(mainBusinessPhysicalAddress.Postcode, out int intCode) ? (int?)intCode : null,
                mainBusinessPhysicalAddress.StateCode,
                score);
        }

        private string GetNameTypeDisplay(string nameType)
        {
            var nameTypeMapping = new Dictionary<string, string>
            {
                { "mainName", "Entity Name" },
                { "legalName", "Entity Name" },
                { "businessName", "Business Name" },
                { "mainTradingName", "Trading Name" },
                { "otherTradingName", "Trading Name" },
            };

            return nameTypeMapping.ContainsKey(nameType) ? nameTypeMapping[nameType] : nameType;
        }
    }
}
