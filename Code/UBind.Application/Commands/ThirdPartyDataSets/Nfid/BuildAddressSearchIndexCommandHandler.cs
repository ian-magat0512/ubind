// <copyright file="BuildAddressSearchIndexCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Commands.ThirdPartyDataSets.Nfid
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
    using UBind.Application.ThirdPartyDataSets.NfidUpdaterJob;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Repositories;
    using UBind.Domain.Search.ThirdPartyDataSets;
    using UBind.Domain.ThirdPartyDataSets;
    using UBind.Domain.ThirdPartyDataSets.Nfid;

    public class BuildAddressSearchIndexCommandHandler : ICommandHandler<BuildAddressSearchIndexCommand, Unit>
    {
        private readonly INfidRepository nfidRepository;
        private readonly IThirdPartyDataSetsSearchService thirdPartyDataSetsSearchService;
        private readonly IThirdPartyDataSetsConfiguration thirdPartyDataSetsConfiguration;
        private readonly Action<IndexWriter, Detail> addDoc = (indexWriter, nfid) =>
        {
            var searchQuery = new TermQuery(new Term(AddressConstants.GnafAddressId, nfid.GnafAddressId));
            indexWriter.DeleteDocuments(searchQuery);
            var doc = new Document();

            doc.Add(CreateField(AddressConstants.GnafAddressId, nfid.GnafAddressId));
            doc.Add(CreateField(AddressConstants.Elevation, nfid.Elevation.ToString()));
            doc.Add(CreateField(AddressConstants.FloodDepth20, nfid.FloodDepth20.ToString()));
            doc.Add(CreateField(AddressConstants.FloodDepth50, nfid.FloodDepth50.ToString()));
            doc.Add(CreateField(AddressConstants.FloodDepth100, nfid.FloodDepth100.ToString()));
            doc.Add(CreateField(AddressConstants.FloodDepthExtreme, nfid.FloodDepthExtreme.ToString()));
            doc.Add(CreateField(AddressConstants.FloodAriGl, nfid.FloodAriGl.ToString()));
            doc.Add(CreateField(AddressConstants.FloodAriGl1M, nfid.FloodAriGl1M.ToString()));
            doc.Add(CreateField(AddressConstants.FloodAriGl2M, nfid.FloodAriGl2M.ToString()));
            doc.Add(CreateField(AddressConstants.NotesId, nfid.NotesId.ToString()));
            doc.Add(CreateField(AddressConstants.LevelNfidId, nfid.LevelNfidId.ToString()));
            doc.Add(CreateField(AddressConstants.LevelFezId, nfid.LevelFezId.ToString()));
            doc.Add(CreateField(AddressConstants.FloodCode, nfid.FloodCode));
            doc.Add(CreateField(AddressConstants.Latitude, nfid.Latitude.ToString()));
            doc.Add(CreateField(AddressConstants.Longitude, nfid.Longitude.ToString()));

            indexWriter.AddDocument(doc);
        };

        public BuildAddressSearchIndexCommandHandler(
            INfidRepository nfidRepository,
            IThirdPartyDataSetsSearchService thirdPartyDataSetsSearchService,
            IThirdPartyDataSetsConfiguration thirdPartyDataSetsConfiguration)
        {
            this.nfidRepository = nfidRepository;
            this.thirdPartyDataSetsSearchService = thirdPartyDataSetsSearchService;
            this.thirdPartyDataSetsConfiguration = thirdPartyDataSetsConfiguration;
        }

        public async Task<Unit> Handle(BuildAddressSearchIndexCommand request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var indexConfiguration = this.thirdPartyDataSetsConfiguration.IndexNames[UpdaterJobType.Nfid.ToString()];
            var temporaryIndexName = indexConfiguration.TemporaryIndexName;
            var defaultIndex = indexConfiguration.DefaultIndexName;

            var pageNumber = request.PageNumber;
            var pageSize = request.PageSize;

            while (true)
            {
                var nfidList =
                    await this.nfidRepository.GetPaginatedNfid(pageNumber, pageSize);

                if (!nfidList.Any())
                {
                    break;
                }

                this.thirdPartyDataSetsSearchService.IndexItemsToTemporaryLocation(
                    temporaryIndexName,
                    nfidList,
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
