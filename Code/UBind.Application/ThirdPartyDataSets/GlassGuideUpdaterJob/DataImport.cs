// <copyright file="DataImport.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.ThirdPartyDataSets.GlassGuideUpdaterJob;

using System;
using System.Data;

public abstract class DataImport : IDataImport
{
    protected const int MissingColumn = -1;
    protected DataTable dataTable;
    private string extractPath;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataImport"/> class.
    /// </summary>
    /// <param name="dataTable">The data table import target.</param>
    /// <param name="extractPath">The extract folder location of Glass's Guide data.</param>
    public DataImport(DataTable dataTable, string extractPath)
    {
        this.dataTable = dataTable;
        this.extractPath = extractPath;
    }

    /// <inheritdoc/>
    public async Task<DataTable> Import(string segment, DataComponentType type, bool canAddValues = false, bool canValidateValues = false, bool canRecode = false)
    {
        var filter = $"{segment}*.{type}";
        string[]? files = null;
        try
        {
            files = Directory.GetFiles(this.extractPath, filter, SearchOption.AllDirectories);
        }
        catch (DirectoryNotFoundException)
        {
            return this.dataTable;
        }

        if (files == null || files.Length <= 0)
        {
            return this.dataTable;
        }

        Array.Sort(files); // from earliest to newest based on file name
        foreach (var file in files)
        {
            using (var reader = new StreamReader(file))
            {
                string? row = reader.ReadLine(); // header
                if (row == null)
                {
                    throw new InvalidDataException("Data component file has invalid content.");
                }
                var dataSegment = new DataSegment(file);
                int[] rawIndices = this.GetDataComponentIndices(row, type);
                if (rawIndices.Length == 0)
                {
                    throw new NotSupportedException("Data component file has no matching vehicle category.");
                }
                reader.ReadLine(); // column indices
                string? delimiterRow = reader.ReadLine(); // header delimiter

                var valueTypes = this.GetDataTableValueTypes(this.dataTable);
                object?[] valueBuffer = new object[valueTypes.Length];
                int[] columnIndices = this.GetColumnMapping(type, dataSegment.DataCode);
                if (columnIndices.Length == 0)
                {
                    throw new NotSupportedException("Cannot map columns with unknown data segment file.");
                }
                if (valueBuffer.Length != columnIndices.Length)
                {
                    throw new ArgumentOutOfRangeException("Table columns didn't match the number of column indices provided.");
                }
                this.dataTable.BeginLoadData();
                row = reader.ReadLine();
                while (row != null)
                {
                    var values = new string[rawIndices.Length];
                    for (var i = 0; i < rawIndices.Length; i++)
                    {
                        int index = rawIndices[i];
                        if (index == MissingColumn)
                        {
                            values[i] = string.Empty;
                        }
                        else
                        {
                            int length = this.GetRawColumnLength(rawIndices, i, delimiterRow);
                            string value = (index < row.Length && length < row.Length) ?
                                row.Substring(index, length) :
                                string.Empty;
                            values[i] = value.Trim();
                        }
                    }

                    if (canRecode)
                    {
                        bool shouldDelete = this.RecodeValues(dataSegment, values);
                        if (shouldDelete)
                        {
                            row = reader.ReadLine();
                            continue;
                        }
                    }

                    for (var i = 0; i < valueBuffer.Length; i++)
                    {
                        string? value = null;
                        int column = columnIndices[i];
                        if (column == MissingColumn)
                        {
                            if (canAddValues)
                            {
                                value = this.AddValue(dataSegment, i, values);
                            }
                            else
                            {
                                continue;
                            }
                        }
                        else
                        {
                            value = canValidateValues ? this.ValidateValue(type, dataSegment, i, column, values) : values[column];
                        }

                        if (string.IsNullOrEmpty(value))
                        {
                            valueBuffer[i] = null;
                            continue;
                        }

                        try
                        {
                            Type t = Nullable.GetUnderlyingType(valueTypes[i]) ?? valueTypes[i];
                            valueBuffer[i] = Convert.ChangeType(value, t);
                        }
                        catch (Exception ex) when (
                            ex is InvalidCastException ||
                            ex is FormatException ||
                            ex is ArgumentNullException)
                        {
                            valueBuffer[i] = null;
                        }
                    }
                    this.dataTable.LoadDataRow(valueBuffer, LoadOption.Upsert);
                    row = reader.ReadLine();
                }
                this.dataTable.EndLoadData();
            }
        }

        return await Task.FromResult(this.dataTable);
    }

    /// <summary>
    /// Get the component index list based on given component and segment type.
    /// </summary>
    /// <param name="type">The data component type (e.g. N12).</param>
    /// <returns>The raw indices of data segment columns.</returns>
    public int[] GetDataComponentIndices(string headerRow, DataComponentType type)
    {
        if (type.Equals(DataComponentType.N12))
        {
            return this.GetHeaderIndices(headerRow, new string[]
{
                "CODE", "NVIC", "YEAR", "MTH", "MAKE", "FAMILY", "VARIANT", "SERIES", "STYLE",
                "ENGINE", "CC", "SIZE", "TRANS", "CYL", "NEW",
                "BT", "ET", "TT",
            });
        }
        else if (type.Equals(DataComponentType.U12))
        {
            return this.GetHeaderIndices(headerRow, new string[]
            {
                "CODE", "NVIC", "YEAR", "MTH", "MAKE", "FAMILY", "VARIANT", "SERIES", "STYLE",
                "ENGINE", "CC", "SIZE", "TRANS", "CYL", "NEW", "$", "$$", "$$$", "B/AV.", "AV.", "A/AV.",
                "BT", "ET", "TT",
            });
        }
        else if (type.Equals(DataComponentType.Mak) ||
            type.Equals(DataComponentType.Bdy) ||
            type.Equals(DataComponentType.Eng) ||
            type.Equals(DataComponentType.Trn))
        {
            return this.GetHeaderIndices(headerRow, new string[]
            {
                "CD", "DESCRIPTION", "SEGMENT",
            });
        }
        else if (type.Equals(DataComponentType.Rec))
        {
            return this.GetHeaderIndices(headerRow, new string[]
            {
                "OLD GLASS CODE", "NEW GLASS CODE", "DATE", "NVIC", "SEGMENT",
            });
        }
        else
        {
            return Array.Empty<int>();
        }
    }

    /// <summary>
    /// Validates values of each of the data component types.
    /// </summary>
    /// <param name="type">The data component type (e.g. N12).</param>
    /// <param name="dataSegment">Vehicle information based on data segment.</param>
    /// <param name="columnIndex">The column index from dataset.</param>
    /// <param name="rawColumnIndex">The column index from data component file.</param>
    /// <param name="values">Cell values from the data component file.</param>
    /// <returns>The validated value.</returns>
    public abstract string ValidateValue(
        DataComponentType type,
        DataSegment dataSegment,
        int columnIndex,
        int rawColumnIndex,
        string[] values);

    /// <summary>
    /// Recodes (change glass code or delete) a row from data segment data.
    /// </summary>
    /// <param name="dataSegment">Vehicle information based on data segment.</param>
    /// <param name="values">The pre-loaded values from data segment file.</param>
    /// <returns>Returns true if this row should be deleted, false otherwise.</returns>
    protected abstract bool RecodeValues(DataSegment dataSegment, string[] values);

    /// <summary>
    /// Adds value to non-existing column in data segment file.
    /// </summary>
    /// <param name="dataSegment">Vehicle information based on data segment.</param>
    /// <param name="columnIndex">The column index from dataset.</param>
    /// <param name="values">The pre-loaded values from data segment file.</param>
    /// <returns>The new value.</returns>
    protected abstract string? AddValue(DataSegment dataSegment, int columnIndex, string[] values);

    /// <summary>
    /// Get dataset to component file column mapping based on vehicle category.
    /// </summary>
    /// <param name="type">The data component type (e.g. N12).</param>
    /// <param name="category">The vehicle category (e.g. PVG).</param>
    /// <returns>The column mapping.</returns>
    protected abstract int[] GetColumnMapping(DataComponentType type, VehicleSegmentType category);

    private Type[] GetDataTableValueTypes(DataTable dataTable)
    {
        var valueTypes = new Type[dataTable.Columns.Count];
        for (var i = 0; i < valueTypes.Length; i++)
        {
            var dataColumn = dataTable.Columns[i];
            var type = dataColumn.DataType;
            if (dataColumn.AllowDBNull && type.IsValueType)
            {
                type = typeof(Nullable<>).MakeGenericType(type);
            }

            valueTypes[i] = type;
        }

        return valueTypes;
    }

    private int GetRawColumnLength(int[] rawIndices, int index, string? delimiterRow)
    {
        if (string.IsNullOrEmpty(delimiterRow))
        {
            throw new InvalidDataException("Data component file has invalid content.");
        }
        int lastExistingIndex = rawIndices.Length - 1;
        for (; rawIndices[lastExistingIndex] == MissingColumn && lastExistingIndex >= 0; --lastExistingIndex)
        {
        }
        bool isLast = index == lastExistingIndex;
        int value = rawIndices[index];
        return (isLast ? delimiterRow.Length : delimiterRow.IndexOf(' ', value)) - value;
    }

    private int[] GetHeaderIndices(string headerRow, string[] headerKeys)
    {
        var indices = new List<int>();
        foreach (string header in headerKeys)
        {
            indices.Add(headerRow.IndexOf(header));
        }
        return indices.ToArray();
    }
}
