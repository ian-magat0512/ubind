// <copyright file="DataImportVehicle.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.ThirdPartyDataSets.GlassGuideUpdaterJob;

using System.Data;
using UBind.Application.ExtensionMethods;
using UBind.Application.ThirdPartyDataSets.GlassGuideUpdaterJob.ColumnMaps;
using UBind.Domain.ThirdPartyDataSets.GlassGuide;

public class DataImportVehicle : DataImport
{
    private const string RecodeNew = "NEW";
    private const string RecodeDeleted = "DELETED";
    private readonly string[] months = { "jan", "feb", "mar", "apr", "may", "jun", "jul", "aug", "sep", "oct", "nov", "dec" };
    private DataTable recodesTable;
    private DataTable makesTable;
    private DataTable bodiesTable;
    private DataTable enginesTable;
    private DataTable transmissionsTable;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataImportVehicle"/> class.
    /// </summary>
    /// <param name="dataTable">The data table import target.</param>
    /// <param name="extractPath">The extract folder location of Glass's Guide data.</param>
    /// <param name="recodesTable">The data table for lookup of entry recodes.</param>
    /// <param name="makesTable">The data table for lookup of make descriptions.</param>
    /// <param name="bodiesTable">The data table for lookup of body descriptions.</param>
    /// <param name="enginesTable">The data table for lookup of engine descriptions.</param>
    /// <param name="transmissionsTable">The data table for lookup of transmission descriptions.</param>
    public DataImportVehicle(
        DataTable dataTable,
        string extractPath,
        DataTable recodesTable,
        DataTable makesTable,
        DataTable bodiesTable,
        DataTable enginesTable,
        DataTable transmissionsTable)
        : base(dataTable, extractPath)
    {
        this.recodesTable = recodesTable;
        this.makesTable = makesTable;
        this.bodiesTable = bodiesTable;
        this.enginesTable = enginesTable;
        this.transmissionsTable = transmissionsTable;
    }

    /// <summary>
    /// Import from U12 and N12 data component files.
    /// </summary>
    /// <returns>The data table containing vehicle information.</returns>
    public async Task<DataTable> ImportVehicles()
    {
        await this.Import(VehicleSegmentType.Pvg.ToString(), DataComponentType.U12, true, true, true);
        await this.Import(VehicleSegmentType.Ocg.ToString(), DataComponentType.U12, true, true, true);
        await this.Import(VehicleSegmentType.Cvg.ToString(), DataComponentType.U12, true, true, true);
        await this.Import(VehicleSegmentType.Olc.ToString(), DataComponentType.U12, true, true, true);
        await this.Import(VehicleSegmentType.Pvg.ToString(), DataComponentType.N12, true, true, true);
        await this.Import(VehicleSegmentType.Ocg.ToString(), DataComponentType.N12, true, true, true);
        await this.Import(VehicleSegmentType.Cvg.ToString(), DataComponentType.N12, true, true, true);
        await this.Import(VehicleSegmentType.Olc.ToString(), DataComponentType.N12, true, true, true);
        return this.dataTable;
    }

    /// <inheritdoc/>
    public override string ValidateValue(
        DataComponentType type,
        DataSegment dataSegment,
        int columnIndex,
        int rawColumnIndex,
        string[] values)
    {
        string value = values[rawColumnIndex];
        if (columnIndex == (int)VehicleColumns.Month)
        {
            int monthIndex = this.months.IndexOf(value.ToLower());
            if (monthIndex != -1)
            {
                value = (monthIndex + 1).ToString();
            }
        }
        else if (columnIndex == (int)VehicleColumns.MakeDescription)
        {
            int index = (int)RawColumnsVehicle.Code;
            string glassCode = values[index];
            string makeCode = glassCode.Substring(0, Math.Min(3, glassCode.Length));
            value = this.GetFullDescription(this.makesTable, makeCode, dataSegment) ?? value;
        }
        else if (columnIndex == (int)VehicleColumns.Body)
        {
            int index = type.Equals(DataComponentType.U12) ?
                (int)RawColumnsU12.Bt :
                (int)RawColumnsN12.Bt;
            value = this.GetFullDescription(this.bodiesTable, values[index], dataSegment) ?? value;
        }
        else if (columnIndex == (int)VehicleColumns.EngineType)
        {
            int index = type.Equals(DataComponentType.U12) ?
                (int)RawColumnsU12.Et :
                (int)RawColumnsN12.Et;
            value = this.GetFullDescription(this.enginesTable, values[index], dataSegment) ?? value;
        }
        else if (columnIndex == (int)VehicleColumns.Transmission)
        {
            int index = type.Equals(DataComponentType.U12) ?
                (int)RawColumnsU12.Tt :
                (int)RawColumnsN12.Tt;
            value = this.GetFullDescription(this.transmissionsTable, values[index], dataSegment) ?? value;
        }
        else if (columnIndex == (int)VehicleColumns.EngineVolume)
        {
            if (int.TryParse(value, out int engineVolume) && engineVolume == 0)
            {
                value = string.Empty;
            }
        }
        else if (columnIndex == (int)VehicleColumns.EngineSize)
        {
            if (double.TryParse(value, out double engineSize) && engineSize == 0.0)
            {
                value = string.Empty;
            }
        }
        else if (columnIndex == (int)VehicleColumns.Cylinders)
        {
            if ("NA".Equals(value))
            {
                value = string.Empty;
            }
        }

        return value;
    }

    protected override bool RecodeValues(DataSegment dataSegment, string[] values)
    {
        int codeIndex = (int)RawColumnsVehicle.Code;
        int nvicIndex = (int)RawColumnsVehicle.Nvic;
        var rows = this.recodesTable.Select(@$"
            {RecodeColumns.OldCode}='{values[codeIndex]}' AND 
            {RecodeColumns.Nvic}='{values[nvicIndex]}' AND 
            {RecodeColumns.Segment}='{dataSegment.DataCode}'");
        string? newCode = rows.TryGetValue(0, out DataRow? row) ? row?[(int)RecodeColumns.NewCode].ToString() : null;
        if (string.IsNullOrEmpty(newCode))
        {
            return false;
        }
        else if (newCode.Equals(RecodeDeleted))
        {
            return true;
        }
        else if (!newCode.Equals(RecodeNew))
        {
            values[codeIndex] = newCode;
        }
        return false;
    }

    protected override string? AddValue(DataSegment dataSegment, int columnIndex, string[] values)
    {
        string? value = null;
        if (columnIndex == (int)VehicleColumns.DataCode)
        {
            value = dataSegment.DataCode.ToString().ToUpper();
        }
        else if (columnIndex == (int)VehicleColumns.VehicleTypeCode)
        {
            value = dataSegment.VehicleTypeCode;
        }
        else if (columnIndex == (int)VehicleColumns.VehicleTypeDescription)
        {
            value = dataSegment.VehicleTypeDescription;
        }
        else if (columnIndex == (int)VehicleColumns.MakeCode)
        {
            int index = (int)RawColumnsVehicle.Code;
            string glassCode = values[index];
            value = glassCode.Substring(0, Math.Min(3, glassCode.Length));
        }
        else if (columnIndex == (int)VehicleColumns.FamilyCode)
        {
            int index = (int)RawColumnsVehicle.Code;
            string glassCode = values[index];
            if (glassCode.Length >= 6)
            {
                value = glassCode.Substring(3, 3).Replace("-", string.Empty);
            }
        }
        return value;
    }

    protected override int[] GetColumnMapping(DataComponentType type, VehicleSegmentType category)
    {
        int[] columnIndices =
        {
            (int)RawColumnsVehicle.Code,
            (int)RawColumnsVehicle.Nvic,
            MissingColumn,
            MissingColumn,
            MissingColumn,
            (int)RawColumnsVehicle.Year,
            (int)RawColumnsVehicle.Mth,
            MissingColumn,
            (int)RawColumnsVehicle.Make,
            MissingColumn,
            (int)RawColumnsVehicle.Family,
            (int)RawColumnsVehicle.Variant,
            (int)RawColumnsVehicle.Series,
            (int)RawColumnsVehicle.Style,
            (int)RawColumnsVehicle.Engine,
            (int)RawColumnsVehicle.Cc,
            (int)RawColumnsVehicle.Size,
            (int)RawColumnsVehicle.Transmission,
            (int)RawColumnsVehicle.Cyl,
        };
        int[] additionalColumnIndices = Array.Empty<int>();
        if (type.Equals(DataComponentType.N12))
        {
            additionalColumnIndices = new int[]
            {
                MissingColumn,
                MissingColumn,
                MissingColumn,
                MissingColumn,
                MissingColumn,
                MissingColumn,
                (int)RawColumnsN12.New_Pr,
                MissingColumn,
            };
        }
        else if (type.Equals(DataComponentType.U12))
        {
            bool isNewCategory = category.Equals(VehicleSegmentType.Pvg) || category.Equals(VehicleSegmentType.Cvg);
            additionalColumnIndices = new int[]
            {
                isNewCategory ? (int)RawColumnsU12.S : MissingColumn,
                isNewCategory ? (int)RawColumnsU12.Ss : MissingColumn,
                isNewCategory ? (int)RawColumnsU12.Sss : MissingColumn,
                isNewCategory ? MissingColumn : (int)RawColumnsU12.B_Av,
                isNewCategory ? MissingColumn : (int)RawColumnsU12.Av,
                isNewCategory ? MissingColumn : (int)RawColumnsU12.A_Av,
                (int)RawColumnsU12.New_Pr,
                isNewCategory ? (int)RawColumnsU12.Sss : (int)RawColumnsU12.Av,
            };
        }
        var allIndices = new int[columnIndices.Length + additionalColumnIndices.Length];
        columnIndices.CopyTo(allIndices, 0);
        additionalColumnIndices.CopyTo(allIndices, columnIndices.Length);
        return allIndices;
    }

    private string? GetFullDescription(DataTable table, string code, DataSegment dataSegment)
    {
        var rows = table.Select(@$"
            {CodeDescriptionColumns.Code}='{code}' AND 
            {CodeDescriptionColumns.Segment}='{dataSegment.DataCode}'");
        return rows.TryGetValue(0, out DataRow? row) ? row?[(int)CodeDescriptionColumns.Description].ToString() : null;
    }
}
