// <copyright file="AddressSearchBySearchStringQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Queries.ThirdPartyDataSets.Gnaf
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using UBind.Application.ThirdPartyDataSets.Query.QueryFactories.Gnaf;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Search.ThirdPartyDataSets;
    using UBind.Domain.ThirdPartyDataSets.Gnaf;

    /// <summary>
    /// Represents the handler for obtaining the Gnaf addresses by search string and maximum result.
    /// </summary>
    public class AddressSearchBySearchStringQueryHandler : IQueryHandler<AddressSearchBySearchStringQuery, IReadOnlyList<MaterializedAddressView>>
    {
        private const string DefaultIndex = "GnafAddressLuceneIndex";
        private readonly IThirdPartyDataSetsSearchService thirdPartyDataSetsSearchService;
        private readonly IQueryFactory gnafQueryFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddressSearchBySearchStringQueryHandler "/> class.
        /// </summary>
        /// <param name="thirdPartyDataSetsSearchService">The third party data sets search service.</param>
        /// <param name="gnafQueryFactory">The Gnaf query factory.</param>
        public AddressSearchBySearchStringQueryHandler(
            IThirdPartyDataSetsSearchService thirdPartyDataSetsSearchService,
            IQueryFactory gnafQueryFactory)
        {
            this.thirdPartyDataSetsSearchService = thirdPartyDataSetsSearchService;
            this.gnafQueryFactory = gnafQueryFactory;
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<MaterializedAddressView>> Handle(AddressSearchBySearchStringQuery request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await Task.FromResult(
                this.thirdPartyDataSetsSearchService.Search<MaterializedAddressView>(
                    DefaultIndex,
                    this.gnafQueryFactory.CreateQueryFilterFields(this.gnafQueryFactory.CreateQueryTerms(request.SearchString)),
                    this.gnafQueryFactory.CreateQueryTerms(request.SearchString),
                    request.MaxResults,
                    cancellationToken: cancellationToken));
        }
    }
}
