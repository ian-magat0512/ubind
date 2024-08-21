// <copyright file="DataImportRecodes.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.ThirdPartyDataSets.GlassGuideUpdaterJob;

using System.Data;
using UBind.Application.ThirdPartyDataSets.GlassGuideUpdaterJob.ColumnMaps;

public class DataImportRecodes : DataImport
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataImportRecodes"/> class.
    /// </summary>
    /// <param name="dataTable">The data table import target.</param>
    /// <param name="extractPath">The extract folder location of Glass's Guide data.</param>
    public DataImportRecodes(DataTable dataTable, string extractPath)
        : base(dataTable, extractPath)
    {
    }

    /// <summary>
    /// Import from REC data component files.
    /// </summary>
    /// <returns>The data table containing vehicle recode information.</returns>
    public async Task<DataTable> ImportRecodes()
    {
        return await this.Import(string.Empty, DataComponentType.Rec, true);
    }

    /// <inheritdoc/>
    public override string ValidateValue(
        DataComponentType type,
        DataSegment dataSegment,
        int columnIndex,
        int rawColumnIndex,
        string[] values)
    {
        return string.Empty;
    }

    protected override bool RecodeValues(DataSegment dataSegment, string[] values)
    {
        return false;
    }

    protected override string? AddValue(DataSegment dataSegment, int columnIndex, string[] values)
    {
        if (columnIndex == (int)RawColumnsRecode.Segment)
        {
            return dataSegment.DataCode.ToString();
        }
        return null;
    }

    protected override int[] GetColumnMapping(DataComponentType type, VehicleSegmentType category)
    {
        return new int[]
        {
            (int)RawColumnsRecode.OldCode,
            (int)RawColumnsRecode.NewCode,
            (int)RawColumnsRecode.Date,
            (int)RawColumnsRecode.Nvic,
            MissingColumn,
        };
    }
}
