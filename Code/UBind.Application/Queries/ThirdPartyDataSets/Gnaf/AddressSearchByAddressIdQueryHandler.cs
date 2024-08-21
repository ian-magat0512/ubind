// <copyright file="AddressSearchByAddressIdQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Queries.ThirdPartyDataSets.Gnaf
{
    using System.Threading;
    using System.Threading.Tasks;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Search.ThirdPartyDataSets;
    using UBind.Domain.ThirdPartyDataSets.Gnaf;

    /// <summary>
    /// Represents the handler for obtaining the Gnaf address by address id.
    /// </summary>
    public class AddressSearchByAddressIdQueryHandler : IQueryHandler<AddressSearchByAddressIdQuery, MaterializedAddressView>
    {
        private const string DefaultIndex = "GnafAddressLuceneIndex";
        private readonly IThirdPartyDataSetsSearchService thirdPartyDataSetsSearchService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddressSearchByAddressIdQueryHandler"/> class.
        /// </summary>
        /// <param name="thirdPartyDataSetsSearchService">The third party data sets search service.</param>
        public AddressSearchByAddressIdQueryHandler(IThirdPartyDataSetsSearchService thirdPartyDataSetsSearchService)
        {
            this.thirdPartyDataSetsSearchService = thirdPartyDataSetsSearchService;
        }

        /// <inheritdoc/>
        public async Task<MaterializedAddressView> Handle(AddressSearchByAddressIdQuery request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await Task.FromResult(this.thirdPartyDataSetsSearchService.SearchSingleOrDefault<MaterializedAddressView>(DefaultIndex, AddressConstants.Id, request.Id));
        }
    }
}
