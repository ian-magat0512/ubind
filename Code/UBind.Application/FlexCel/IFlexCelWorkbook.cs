// <copyright file="IFlexCelWorkbook.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.FlexCel
{
    using global::FlexCel.Core;
    using Newtonsoft.Json.Linq;
    using UBind.Application.ResourcePool;
    using UBind.Domain;
    using static UBind.Application.FlexCel.FlexCelWorkbook;

    /// <summary>
    /// A FlexCel workbook session for managing a series of writes and reads to a workbook.
    /// </summary>
    public interface IFlexCelWorkbook : IResourcePoolMember
    {
        /// <summary>
        /// Write data to a cell range.
        /// </summary>
        /// <param name="worksheetName">The name of the worksheet the table is in.</param>
        /// <param name="address">The address of the cell range to write to.</param>
        /// <param name="jsonData">JSON containing the data to write.</param>
        void PatchData(string worksheetName, string address, JArray jsonData);

        /// <summary>
        /// Write data to a table column.
        /// </summary>
        /// <param name="worksheetName">The name of the worksheet the table is in.</param>
        /// <param name="tableName">The table name.</param>
        /// <param name="columnName">The column name.</param>
        /// <param name="jsonData">JSON containing the data to write.</param>
        void PatchData(string worksheetName, string tableName, string columnName, JArray jsonData);

        /// <summary>
        /// Read data from a table column.
        /// </summary>
        /// <param name="worksheetName">The name of the worksheet the table is in.</param>
        /// <param name="tableName">The table name.</param>
        /// <param name="columnName">The column name.</param>
        /// <returns>A string containing JSON for the data read or any errror.</returns>
        string ReadTableColumn(string worksheetName, string tableName, string columnName);

        /// <summary>
        /// Read text from a table.
        /// </summary>
        /// <param name="worksheetName">The name of the worksheet the table is in.</param>
        /// <param name="tableName">The table name.</param>
        /// <returns>A string containing JSON for the table text read or any errror.</returns>
        string ReadTableTextAsString(string worksheetName, string tableName);

        /// <summary>
        /// Read text from a table.
        /// </summary>
        /// <param name="worksheetName">The name of the worksheet the table is in.</param>
        /// <param name="tableName">The table name.</param>
        /// <returns>A string containing JSON for the table text read or any errror.</returns>
        TableTextModel ReadTableText(string worksheetName, string tableName);

        /// <summary>
        /// Reads Table Property value.
        /// </summary>
        /// <param name="sheetName">The name of the worksheet the table is in.</param>
        /// <param name="tableName">The table name.</param>
        /// <param name="propertyName">The property name.</param>
        /// <param name="ignoreFormulaText">A value that determins whether formula text will be returned along with the value.</param>
        /// <returns>A dynamic type.</returns>
        dynamic ReadTablePropertyValue(string sheetName, string tableName, string propertyName, bool ignoreFormulaText = true);

        /// <summary>
        /// Update additional rating factors.
        /// </summary>
        /// <param name="additionalRatingFactors">Addtional rating factors.</param>
        void UpdateAdditionalRatingFactors(IAdditionalRatingFactors additionalRatingFactors);

        /// <summary>
        /// Get the named range.
        /// </summary>
        /// <param name="worksheetName">The worksheet name.</param>
        /// <param name="name">The name of the range.</param>
        /// <returns>The named range object.</returns>
        TXlsNamedRange GetNamedRange(string worksheetName, string name);
    }
}
