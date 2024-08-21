// <copyright file="ImportDelimiterSeparatedValuesCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Commands.ThirdPartyDataSets.RedBook
{
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using UBind.Application.Helpers;
    using UBind.Application.Services.DelimiterSeparatedValues;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Repositories;
    using UBind.Domain.ThirdPartyDataSets;

    /// <summary>
    /// Represents the handler to import delimiter separated values files into Gnaf database.
    /// </summary>
    public class ImportDelimiterSeparatedValuesCommandHandler : ICommandHandler<ImportDelimiterSeparatedValuesCommand, Unit>
    {
        private const string ExtractedFolder = "Extracted";
        private readonly IThirdPartyDataSetsConfiguration thirdPartyDataSetsConfiguration;
        private readonly IRedBookRepository redBookRepository;

        private readonly IDelimiterSeparatedValuesService delimiterSeparatedValuesService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImportDelimiterSeparatedValuesCommandHandler"/> class.
        /// </summary>
        /// <param name="thirdPartyDataSetsConfiguration">The third party data sets configuration.</param>
        /// <param name="redBookRepository">The RedBook repository.</param>
        /// <param name="delimiterSeparatedValuesService">The data separated value service.</param>
        public ImportDelimiterSeparatedValuesCommandHandler(
            IThirdPartyDataSetsConfiguration thirdPartyDataSetsConfiguration,
            IRedBookRepository redBookRepository,
            IDelimiterSeparatedValuesService delimiterSeparatedValuesService)
        {
            this.thirdPartyDataSetsConfiguration = thirdPartyDataSetsConfiguration;
            this.redBookRepository = redBookRepository;
            this.delimiterSeparatedValuesService = delimiterSeparatedValuesService;
        }

        /// <inheritdoc/>
        public async Task<Unit> Handle(ImportDelimiterSeparatedValuesCommand request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var basePath = this.thirdPartyDataSetsConfiguration.UpdaterJobsPath;
            var extractPath = Path.Combine(basePath, request.UpdaterJobId.ToString(), ExtractedFolder);

            var filesWithGroup = this.delimiterSeparatedValuesService.GetDsvFilesWithGroup(DelimiterSeparatedValuesFileTypes.Csv, extractPath, Path.GetFileNameWithoutExtension);
            var currentSuffix = await this.redBookRepository.GetExistingTableIndex();
            if (filesWithGroup != null)
            {
                foreach (var file in filesWithGroup)
                {
                    var newTable = file.GroupName + (string.IsNullOrEmpty(currentSuffix) ? string.Empty : "_" + currentSuffix);
                    var dataTableWithSchema = this.redBookRepository.GetDataTableWithSchema(newTable);

                    var dataTableFromDsv = this.delimiterSeparatedValuesService.ConvertDelimiterSeparatedValuesToDataTable(
                        file.FileName, ",", dataTableWithSchema);

                    await this.redBookRepository.BulkCopyAsync(dataTableFromDsv);
                }
            }

            await this.redBookRepository.CreateOrUpdateSchemaBoundView(currentSuffix);
            var rollingNumber = new RollingNumber(99, currentSuffix, "D2");
            string previousIndex = rollingNumber.GetPrevious();
            await this.redBookRepository.DropAllTablesByIndex(previousIndex);
            return await Task.FromResult(Unit.Value);
        }
    }
}
