// <copyright file="BuildAddressSearchIndexCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Commands.ThirdPartyDataSets.Gnaf
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using global::Lucene.Net.Documents;
    using global::Lucene.Net.Index;
    using global::Lucene.Net.Search;
    using MediatR;
    using UBind.Application.ThirdPartyDataSets;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Repositories;
    using UBind.Domain.Search.ThirdPartyDataSets;
    using UBind.Domain.ThirdPartyDataSets;
    using UBind.Domain.ThirdPartyDataSets.Gnaf;

    /// <summary>
    /// Represents the handler to build an address search index from the Gnaf database.
    /// </summary>
    public class BuildAddressSearchIndexCommandHandler : ICommandHandler<BuildAddressSearchIndexCommand, Unit>
    {
        private readonly IGnafRepository gnafRepository;
        private readonly IThirdPartyDataSetsSearchService thirdPartyDataSetsSearchService;
        private readonly IThirdPartyDataSetsConfiguration thirdPartyDataSetsConfiguration;

        private readonly Action<IndexWriter, MaterializedAddressView> addDoc = (indexWriter, address) =>
        {
            var searchQuery = new TermQuery(new Term(AddressConstants.Id, address.Id));
            indexWriter.DeleteDocuments(searchQuery);
            var doc = new Document();

            doc.Add(CreateField(AddressConstants.Id, address.Id));
            doc.Add(CreateField(AddressConstants.NumberFirst, address.NumberFirst));
            doc.Add(CreateField(AddressConstants.LotNumber, address.LotNumber));
            doc.Add(CreateField(AddressConstants.FlatNumber, address.FlatNumber));
            doc.Add(CreateField(AddressConstants.StreetName, address.StreetName));
            doc.Add(CreateField(AddressConstants.StreetTypeCode, address.StreetTypeCode));
            doc.Add(CreateField(AddressConstants.LocalityName, address.LocalityName));
            doc.Add(CreateField(AddressConstants.StateAbbreviation, address.StateAbbreviation));
            doc.Add(CreateField(AddressConstants.PostCode, address.PostCode));
            doc.Add(CreateField(AddressConstants.FullAddress, address.FullAddress));
            doc.Add(CreateField(AddressConstants.FlatType, address.FlatType));
            doc.Add(CreateField(AddressConstants.LevelNumber, address.LevelNumber));
            doc.Add(CreateField(AddressConstants.LevelType, address.LevelType));
            doc.Add(CreateField(AddressConstants.StreetTypeShortName, address.StreetTypeShortName));
            doc.Add(CreateField(AddressConstants.Latitude, address.Latitude.ToString()));
            doc.Add(CreateField(AddressConstants.Longitude, address.Longitude.ToString()));

            indexWriter.AddDocument(doc);
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="BuildAddressSearchIndexCommandHandler"/> class.
        /// </summary>
        /// <param name="gnafRepository">The gnaf repository.</param>
        /// <param name="thirdPartyDataSetsSearchService">The third party data sets search service. </param>
        /// <param name="thirdPartyDataSetsConfiguration">The third party data sets configuration.</param>
        public BuildAddressSearchIndexCommandHandler(
            IGnafRepository gnafRepository,
            IThirdPartyDataSetsSearchService thirdPartyDataSetsSearchService,
            IThirdPartyDataSetsConfiguration thirdPartyDataSetsConfiguration)
        {
            this.gnafRepository = gnafRepository;
            this.thirdPartyDataSetsSearchService = thirdPartyDataSetsSearchService;
            this.thirdPartyDataSetsConfiguration = thirdPartyDataSetsConfiguration;
        }

        /// <inheritdoc/>
        public async Task<Unit> Handle(BuildAddressSearchIndexCommand request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var indexConfiguration = this.thirdPartyDataSetsConfiguration.IndexNames[UpdaterJobType.Gnaf.ToString()];
            var temporaryIndexName = indexConfiguration.TemporaryIndexName;
            var defaultIndex = indexConfiguration.DefaultIndexName;

            var pageNumber = request.PageNumber;
            var pageSize = request.PageSize;

            while (true)
            {
                var materializedAddressList =
                    await this.gnafRepository.GetGnafMaterializedAddressView(pageNumber, pageSize);

                if (!materializedAddressList.Any())
                {
                    break;
                }

                this.thirdPartyDataSetsSearchService.IndexItemsToTemporaryLocation(
                    temporaryIndexName,
                    materializedAddressList,
                    this.addDoc);

                pageNumber++;
            }

            this.thirdPartyDataSetsSearchService.SwitchIndexFromTemporaryLocationToTargetIndexBasePath(temporaryIndexName, defaultIndex);

            return await Task.FromResult(Unit.Value);
        }

        private static TextField CreateField(string name, string value)
        {
            return new TextField(name, value ?? string.Empty, Field.Store.YES);
        }
    }
}
