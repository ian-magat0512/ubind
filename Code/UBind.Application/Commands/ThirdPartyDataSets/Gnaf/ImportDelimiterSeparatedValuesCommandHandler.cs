// <copyright file="ImportDelimiterSeparatedValuesCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Commands.ThirdPartyDataSets.Gnaf
{
    using System;
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

        private readonly IGnafRepository gnafRepository;

        private readonly IDelimiterSeparatedValuesService delimiterSeparatedValuesService;

        private readonly Func<string, string> funcExtractFileGroupName = (fileName) =>
            {
                fileName = Path.GetFileNameWithoutExtension(fileName);
                fileName = fileName.Replace("Authority_", string.Empty).Replace("_psv", string.Empty);

                if (fileName.IndexOf('_') >= 0)
                {
                    fileName = fileName.Substring(
                        fileName.IndexOf('_') + 1,
                        fileName.Length - fileName.IndexOf('_') - 1);
                }

                return fileName;
            };

        /// <summary>
        /// Initializes a new instance of the <see cref="ImportDelimiterSeparatedValuesCommandHandler"/> class.
        /// </summary>
        /// <param name="thirdPartyDataSetsConfiguration">The third party data sets configuration.</param>
        /// <param name="gnafRepository">The Gnaf repository.</param>
        /// <param name="delimiterSeparatedValuesService">The data separated value service.</param>
        public ImportDelimiterSeparatedValuesCommandHandler(
            IThirdPartyDataSetsConfiguration thirdPartyDataSetsConfiguration,
            IGnafRepository gnafRepository,
            IDelimiterSeparatedValuesService delimiterSeparatedValuesService)
        {
            this.thirdPartyDataSetsConfiguration = thirdPartyDataSetsConfiguration;
            this.gnafRepository = gnafRepository;
            this.delimiterSeparatedValuesService = delimiterSeparatedValuesService;
        }

        /// <inheritdoc/>
        public async Task<Unit> Handle(ImportDelimiterSeparatedValuesCommand request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var basePath = this.thirdPartyDataSetsConfiguration.UpdaterJobsPath;
            var extractPath = Path.Combine(basePath, request.UpdaterJobId.ToString(), ExtractedFolder);

            var filesWithGroup = this.delimiterSeparatedValuesService.GetDsvFilesWithGroup(DelimiterSeparatedValuesFileTypes.Psv, extractPath, this.funcExtractFileGroupName);
            var currentSuffix = await this.gnafRepository.GetExistingTableIndex();
            if (filesWithGroup != null)
            {
                foreach (var file in filesWithGroup)
                {
                    // Extras doesnt need to be imported and causes issues with created tables.
                    if (!file.FileName.Contains("\\Extras\\"))
                    {
                        var newTable = file.GroupName + (string.IsNullOrEmpty(currentSuffix) ? string.Empty : "_" + currentSuffix);
                        var dataTableWithSchema = this.gnafRepository.GetDataTableWithSchema(newTable);

                        var dataTableFromDsv = this.delimiterSeparatedValuesService.ConvertDelimiterSeparatedValuesToDataTable(
                                file.FileName,
                                "|",
                                dataTableWithSchema);

                        await this.gnafRepository.BulkCopyAsync(dataTableFromDsv);
                    }
                }
            }

            await this.gnafRepository.CreateOrUpdateSchemaBoundView(currentSuffix);
            await this.gnafRepository.CreateOrUpdateAddressView();

            var rollingNumber = new RollingNumber(99, currentSuffix);
            string previousIndex = rollingNumber.GetPrevious();
            await this.gnafRepository.DropAllTablesByIndex(previousIndex);
            return await Task.FromResult(Unit.Value);
        }
    }
}
