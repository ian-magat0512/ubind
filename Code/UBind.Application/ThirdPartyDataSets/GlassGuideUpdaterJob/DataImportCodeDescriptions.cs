// <copyright file="DataImportCodeDescriptions.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.ThirdPartyDataSets.GlassGuideUpdaterJob;

using System.Data;
using UBind.Application.ThirdPartyDataSets.GlassGuideUpdaterJob.ColumnMaps;

public class DataImportCodeDescriptions : DataImport
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataImportCodeDescriptions"/> class.
    /// </summary>
    /// <param name="dataTable">The data table import target.</param>
    /// <param name="extractPath">The extract folder location of Glass's Guide data.</param>
    public DataImportCodeDescriptions(DataTable dataTable, string extractPath)
        : base(dataTable, extractPath)
    {
    }

    /// <summary>
    /// Import from MAK data component files.
    /// </summary>
    /// <returns>The data table containing code-description information.</returns>
    public async Task<DataTable> ImportMakes()
    {
        return await this.Import(string.Empty, DataComponentType.Mak, true);
    }

    /// <summary>
    /// Import from BDY data component files.
    /// </summary>
    /// <returns>The data table containing code-description information.</returns>
    public async Task<DataTable> ImportBodies()
    {
        return await this.Import(string.Empty, DataComponentType.Bdy, true);
    }

    /// <summary>
    /// Import from ENG data component files.
    /// </summary>
    /// <returns>The data table containing code-description information.</returns>
    public async Task<DataTable> ImportEngines()
    {
        return await this.Import(string.Empty, DataComponentType.Eng, true);
    }

    /// <summary>
    /// Import from TRN data component files.
    /// </summary>
    /// <returns>The data table containing code-description information.</returns>
    public async Task<DataTable> ImportTransmissions()
    {
        return await this.Import(string.Empty, DataComponentType.Trn, true);
    }

    /// <inheritdoc/>
    public override string ValidateValue(
        DataComponentType type,
        DataSegment dataSegment,
        int columnIndex,
        int rawColumnIndex,
        string[] valuess)
    {
        return string.Empty;
    }

    protected override bool RecodeValues(DataSegment dataSegment, string[] values)
    {
        return false;
    }

    protected override string? AddValue(DataSegment dataSegment, int columnIndex, string[] values)
    {
        if (columnIndex == (int)RawColumnsCodeDescription.Segment)
        {
            return dataSegment.DataCode.ToString();
        }
        return null;
    }

    protected override int[] GetColumnMapping(DataComponentType type, VehicleSegmentType category)
    {
        return new int[]
        {
            (int)RawColumnsCodeDescription.Code,
            (int)RawColumnsCodeDescription.Description,
            MissingColumn,
        };
    }
}
