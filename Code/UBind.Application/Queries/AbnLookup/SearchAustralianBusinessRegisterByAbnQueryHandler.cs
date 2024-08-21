// <copyright file="SearchAustralianBusinessRegisterByAbnQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.AbnLookup
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using CSharpFunctionalExtensions;
    using MoreLinq;
    using UBind.Application.ConnectedServices.AbrXmlSearch;
    using UBind.Domain;
    using UBind.Domain.AbnLookup;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// This class is needed because we need a query handler for searching Australian business register by ABN.
    /// </summary>
    public class SearchAustralianBusinessRegisterByAbnQueryHandler :
        IQueryHandler<SearchAustralianBusinessRegisterByAbnQuery, Result<AbnSearchResponse, Error>>
    {
        private readonly IAbnLookupConfiguration abnLookupConfiguration;

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchAustralianBusinessRegisterByAbnQueryHandler"/> class.
        /// </summary>
        /// <param name="abnLookupConfiguration">The ABN lookup configuration.</param>
        public SearchAustralianBusinessRegisterByAbnQueryHandler(IAbnLookupConfiguration abnLookupConfiguration)
        {
            this.abnLookupConfiguration = abnLookupConfiguration;
        }

        /// <inheritdoc/>
        public async Task<Result<AbnSearchResponse, Error>> Handle(
            SearchAustralianBusinessRegisterByAbnQuery request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var abn = request.Abn.Replace(" ", string.Empty);
            if (abn.StartsWith("0") || abn.Length != 11 || !long.TryParse(abn, out _))
            {
                return Result.Failure<AbnSearchResponse, Error>(Errors.AbnLookup.InvalidAbn(request.Abn));
            }

            var searchResult = await this.SearchAbnRegisterByNumber(abn, cancellationToken);
            var entity = searchResult as ResponseBusinessEntity202001;

            if (entity == null)
            {
                var responseException = (ResponseException)searchResult;
                var exDescription = responseException.exceptionDescription;

                var error
                    = exDescription == "Search text is not a valid ABN or ACN" || exDescription == "No records found"
                    ? Errors.AbnLookup.AbnNotFound(request.Abn)
                    : Errors.AbnLookup.InvalidSearch(request.Abn, responseException.exceptionDescription);
                return Result.Failure<AbnSearchResponse, Error>(error);
            }

            var response = CreateAbnSearchResponse(entity);

            return Result.Success<AbnSearchResponse, Error>(response);
        }

        private static AbnSearchResponse CreateAbnSearchResponse(ResponseBusinessEntity202001 entity)
        {
            var abnStatus = entity.entityStatus.FirstOrDefault();

            var businessNames = new List<string>();
            entity.businessName?.ForEach((o) =>
            {
                businessNames.Add(o.organisationName);
            });

            var otherTradingNames = new List<string>();
            entity.otherTradingName?.ForEach((o) =>
            {
                otherTradingNames.Add(o.organisationName);
            });

            var individual = entity.Items[0] as Individual;
            var entityName =
                (entity.Items[0] as Organisation)?.organisationName ??
                $"{individual?.familyName}, {individual?.givenName}";

            var address = entity.mainBusinessPhysicalAddress.FirstOrDefault();
            var response = new AbnSearchResponse(
               entity.ABN?[0].identifierValue,
               abnStatus.entityStatusCode,
               abnStatus.effectiveFrom,
               entity.ASICNumber,
               address.effectiveFrom,
               address.postcode,
               address.stateCode,
               businessNames,
               entityName,
               entity.entityType.entityDescription,
               entity.entityType.entityTypeCode,
               entity.mainTradingName?[0].organisationName,
               otherTradingNames,
               entity.goodsAndServicesTax?[0].effectiveFrom);
            return response;
        }

        private async Task<ResponseBody> SearchAbnRegisterByNumber(string abn, CancellationToken cancellationToken)
        {
            var client = new ABRXMLSearchSoapClient(
                Enum.Parse<ABRXMLSearchSoapClient.EndpointConfiguration>(this.abnLookupConfiguration.EndpointConfigurationName));
            var searchTask = client.SearchByABNv202001Async(abn, "N", this.abnLookupConfiguration.UBindGuid);
            var taskCompletionSource = new TaskCompletionSource<object>();
            cancellationToken.Register(() =>
            {
                taskCompletionSource.TrySetCanceled();
            });
            await Task.WhenAny(searchTask, taskCompletionSource.Task);
            SearchByABNv202001Response? payload = null;
            if (searchTask.IsCompleted)
            {
                taskCompletionSource.TrySetResult(null);
                payload = await searchTask;
            }
            else if (cancellationToken.IsCancellationRequested)
            {
                throw new OperationCanceledException();
            }

            return payload.ABRPayloadSearchResults.response.Item;
        }
    }
}
