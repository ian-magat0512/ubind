// <copyright file="ImportDelimiterSeparatedValuesCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Commands.ThirdPartyDataSets.GlassGuide;

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using UBind.Application.Helpers;
using UBind.Application.ThirdPartyDataSets.GlassGuideUpdaterJob;
using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.Repositories;
using UBind.Domain.ThirdPartyDataSets;

/// <summary>
/// Represents the handler to import fixed-width value files into Glass's Guide database.
/// </summary>
public class ImportDelimiterSeparatedValuesCommandHandler : ICommandHandler<ImportDelimiterSeparatedValuesCommand, Unit>
{
    private const string ExtractedFolder = "Extracted";
    private readonly IThirdPartyDataSetsConfiguration thirdPartyDataSetsConfiguration;
    private readonly IGlassGuideRepository glassGuideRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="ImportDelimiterSeparatedValuesCommandHandler"/> class.
    /// </summary>
    /// <param name="thirdPartyDataSetsConfiguration">The third party data sets configuration.</param>
    /// <param name="glassGuideRepository">The Glass's Guide repository.</param>
    public ImportDelimiterSeparatedValuesCommandHandler(
        IThirdPartyDataSetsConfiguration thirdPartyDataSetsConfiguration,
        IGlassGuideRepository glassGuideRepository)
    {
        this.thirdPartyDataSetsConfiguration = thirdPartyDataSetsConfiguration;
        this.glassGuideRepository = glassGuideRepository;
    }

    /// <inheritdoc/>
    public async Task<Unit> Handle(ImportDelimiterSeparatedValuesCommand command, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var basePath = this.thirdPartyDataSetsConfiguration.UpdaterJobsPath;
        var extractPath = Path.Combine(basePath, command.UpdaterJobId.ToString(), ExtractedFolder);

        var dataImportCodeDescriptions = new DataImportCodeDescriptions(this.glassGuideRepository.CreateCodeDescriptionTable(), extractPath);
        var makesTable = await dataImportCodeDescriptions.ImportMakes();
        var bodiesTable = await dataImportCodeDescriptions.ImportBodies();
        var enginesTable = await dataImportCodeDescriptions.ImportEngines();
        var transmissionsTable = await dataImportCodeDescriptions.ImportTransmissions();

        var dataImportRecodes = new DataImportRecodes(this.glassGuideRepository.CreateRecodeTable(), extractPath);
        var recodesTable = await dataImportRecodes.ImportRecodes();

        var currentSuffix = await this.glassGuideRepository.GetExistingTableIndex();
        var dataTableWithSchema = this.glassGuideRepository.GetVehiclesDataTable(currentSuffix);
        var dataImportVehicle = new DataImportVehicle(
            dataTableWithSchema,
            extractPath,
            recodesTable,
            makesTable,
            bodiesTable,
            enginesTable,
            transmissionsTable);
        dataTableWithSchema = await dataImportVehicle.ImportVehicles();
        await this.glassGuideRepository.BulkCopyAsync(dataTableWithSchema);
        await this.glassGuideRepository.GenerateMakesFamiliesAndYearsTableFromVehicles(dataTableWithSchema, currentSuffix);
        await this.glassGuideRepository.CreateOrUpdateSchemaBoundView(currentSuffix);
        var rollingNumber = new RollingNumber(99, currentSuffix, "D2");
        string previousIndex = rollingNumber.GetPrevious();
        await this.glassGuideRepository.DropAllTablesByIndex(previousIndex);
        return await Task.FromResult(Unit.Value);
    }
}