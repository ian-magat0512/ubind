// <copyright file="ImportDelimiterSeparatedValuesCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Commands.ThirdPartyDataSets.Nfid
{
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using UBind.Application.Helpers;
    using UBind.Application.Services.DelimiterSeparatedValues;
    using UBind.Application.ThirdPartyDataSets.NfidUpdaterJob;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Repositories;
    using UBind.Domain.ThirdPartyDataSets;

    /// <summary>
    /// Handler for command to command to import delimiter separated values files into NFID database.
    /// </summary>
    public class ImportDelimiterSeparatedValuesCommandHandler : ICommandHandler<ImportDelimiterSeparatedValuesCommand, Unit>
    {
        private const string ExtractedFolder = "Extracted";
        private const string TableName = "Stage6";

        private readonly IThirdPartyDataSetsConfiguration thirdPartyDataSetsConfiguration;
        private readonly INfidConfiguration nfidConfiguration;
        private readonly INfidRepository nfidRepository;
        private readonly IDelimiterSeparatedValuesService delimiterSeparatedValuesService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImportDelimiterSeparatedValuesCommandHandler"/> class.
        /// </summary>
        /// <param name="thirdPartyDataSetsConfiguration">The third party data sets configuration.</param>
        /// <param name="nfidConfiguration">The nfid configuration.</param>
        /// <param name="nfidRepository">The NFID repository.</param>
        /// <param name="delimiterSeparatedValuesService">The data separated value service.</param>
        public ImportDelimiterSeparatedValuesCommandHandler(
            IThirdPartyDataSetsConfiguration thirdPartyDataSetsConfiguration,
            INfidConfiguration nfidConfiguration,
            INfidRepository nfidRepository,
            IDelimiterSeparatedValuesService delimiterSeparatedValuesService)
        {
            this.thirdPartyDataSetsConfiguration = thirdPartyDataSetsConfiguration;
            this.nfidConfiguration = nfidConfiguration;
            this.nfidRepository = nfidRepository;
            this.delimiterSeparatedValuesService = delimiterSeparatedValuesService;
        }

        /// <inheritdoc/>
        public async Task<Unit> Handle(ImportDelimiterSeparatedValuesCommand request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var basePath = this.thirdPartyDataSetsConfiguration.UpdaterJobsPath;
            var extractPath = Path.Combine(basePath, request.UpdaterJobId.ToString(), ExtractedFolder);
            var currentSuffix = await this.nfidRepository.GetExistingTableIndex();
            var newTable = TableName + (string.IsNullOrEmpty(currentSuffix) ? string.Empty : "_" + currentSuffix);
            var dataTableWithSchema = this.nfidRepository.GetDataTableWithSchema(newTable);

            var hasDatasetFiles = this.nfidConfiguration.TsvFiles.Select(f => File.Exists(Path.Combine(extractPath, f))).Any(e => e == true);

            if (!hasDatasetFiles)
            {
                throw new ErrorException(Domain.Errors.ThirdPartyDataSets.Nfid.NoValidDataSetFileFound(request.UpdaterJobManifest.DownloadUrls.First().Url));
            }

            foreach (var file in this.nfidConfiguration.TsvFiles)
            {
                var targetFile = Path.Combine(extractPath, file);

                if (File.Exists(targetFile))
                {
                    var dataTableFromTsv = this.delimiterSeparatedValuesService.ConvertDelimiterSeparatedValuesToDataTable(
                        targetFile, "\t", dataTableWithSchema);
                    await this.nfidRepository.BulkCopyAsync(dataTableFromTsv);
                }
            }

            await this.nfidRepository.CreateOrUpdateSchemaBoundView(currentSuffix);
            var rollingNumber = new RollingNumber(99, currentSuffix, "D2");
            string previousIndex = rollingNumber.GetPrevious();
            await this.nfidRepository.DropAllTablesByIndex(previousIndex);
            return await Task.FromResult(Unit.Value);
        }
    }
}
