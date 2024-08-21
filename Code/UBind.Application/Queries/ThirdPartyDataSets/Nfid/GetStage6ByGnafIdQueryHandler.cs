// <copyright file="GetStage6ByGnafIdQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Queries.ThirdPartyDataSets.Nfid
{
    using System.Threading;
    using System.Threading.Tasks;
    using StackExchange.Profiling;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Search.ThirdPartyDataSets;
    using UBind.Domain.ThirdPartyDataSets.Nfid;

    /// <summary>
    /// Handler for obtaining NFID details provided by GNAF address id.
    /// </summary>
    public class GetStage6ByGnafIdQueryHandler : IQueryHandler<GetStage6ByGnafIdQuery, Detail>
    {
        private const string DefaultIndex = "NfidLuceneIndex";
        private readonly IThirdPartyDataSetsSearchService thirdPartyDataSetsSearchService;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetStage6ByGnafIdQueryHandler"/> class.
        /// </summary>
        /// <param name="thirdPartyDataSetsSearchService">The third party data set search service.</param>
        public GetStage6ByGnafIdQueryHandler(IThirdPartyDataSetsSearchService thirdPartyDataSetsSearchService)
        {
            this.thirdPartyDataSetsSearchService = thirdPartyDataSetsSearchService;
        }

        /// <inheritdoc/>
        public Task<Detail> Handle(GetStage6ByGnafIdQuery request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using (MiniProfiler.Current.Step(nameof(GetStage6ByGnafIdQueryHandler) + "." + nameof(this.Handle)))
            {
                var result = this.thirdPartyDataSetsSearchService
                    .SearchSingleOrDefault<Detail>(DefaultIndex, AddressConstants.GnafAddressId, request.GnafAddressId);

                if (result == null)
                {
                    throw new ErrorException(Errors.ThirdPartyDataSets.Nfid.GnafAddressIdNotFound(request.GnafAddressId));
                }

                return Task.FromResult(result);
            }
        }
    }
}
