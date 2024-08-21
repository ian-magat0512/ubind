// <copyright file="FlexCelWorkbook.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.FlexCel
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime;
    using CSharpFunctionalExtensions;
    using DotLiquid.Util;
    using global::FlexCel.Core;
    using global::FlexCel.XlsAdapter;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using NodaTime;
    using StackExchange.Profiling;
    using UBind.Application.Exceptions;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;

    /// <summary>
    /// A FlexCel workbook session for managing a series of writes and reads to a workbook.
    /// </summary>
    [Serializable]
    public class FlexCelWorkbook : IFlexCelWorkbook
    {
        private const string DataTypeColumnName = "Data Type";
        private const string CountColumnName = "Count ";
        private const string InvalidColumnName = "Invalid ";
        private const string FlexCelErrValue = "errvalue";
        private const string RepeatingQuestionSetsAnswerTableName = "Table_Repeating_Question_Sets";
        private const string RepeatingQuestionSetsWorksheetName = "Repeating Question Sets";
        private const string StyleWorksheetName = "Styles";
        private const string RatingFactorSheetName = "Rating Factors";
        private const string RatingFactorTableName = "Table_Rating_Factors";
        private const string StylesTableName = "Table_Styles";
        private const string ValueToReplace = "VALUE_TO_REPLACE";
        private const string CountColumnForumulaTemplate = "=IF(Table_Repeating_Question_Sets[Header]=\"Yes\",\"\",IF(ISNUMBER(INDIRECT(SUBSTITUTE(ADDRESS(1,COLUMN(Table_Repeating_Question_Sets[Count VALUE_TO_REPLACE]),4),\"1\",\"\")&ROW()+1)),VALUE(INDIRECT(SUBSTITUTE(ADDRESS(1,COLUMN(Table_Repeating_Question_Sets[Count VALUE_TO_REPLACE]),4),\"1\",\"\")&ROW()+1)),0)+IF(Table_Repeating_Question_Sets[Value VALUE_TO_REPLACE]<>\"\",1,0))";
        private const string InvalidColumnFormulaTemplate = "=IF(Table_Repeating_Question_Sets[Header]=\"Yes\",IF(AND(INDIRECT(SUBSTITUTE(ADDRESS(1,COLUMN(Table_Repeating_Question_Sets[Count VALUE_TO_REPLACE]),4),\"1\",\"\")&ROW()+1)>0,INDIRECT(SUBSTITUTE(ADDRESS(1,COLUMN(),4),\"1\",\"\")&ROW()+1)>0),\"yes\",\"\"),IF(ISNUMBER(VALUE(INDIRECT(SUBSTITUTE(ADDRESS(1,COLUMN(Table_Repeating_Question_Sets[Invalid VALUE_TO_REPLACE]),4),\"1\",\"\")&ROW()+1))),  VALUE(INDIRECT(SUBSTITUTE(ADDRESS(1,COLUMN(Table_Repeating_Question_Sets[Invalid VALUE_TO_REPLACE]),4),\"1\",\"\")&ROW()+1)),0)+IF(AND(Table_Repeating_Question_Sets[Property]<>\"\",Table_Repeating_Question_Sets[Required]=\"Yes\",Table_Repeating_Question_Sets[Value VALUE_TO_REPLACE]=\"\"),1,0))";

        private readonly List<string> numberTypes = new List<string>() { "Currency", "Percent", "Number" };
        private readonly WebFormAppType webFormAppType;
        private readonly IClock clock;
        private readonly byte[] spreadsheetBytes;
        private XlsFile xlsxFile;

        /// <summary>
        /// Initializes a new instance of the <see cref="FlexCelWorkbook"/> class.
        /// </summary>
        /// <param name="spreadsheetBytes">The workbook binary data.</param>
        /// <param name="webFormAppType">The app the workbook is for.</param>
        /// <param name="clock">It's a clock.</param>
        public FlexCelWorkbook(
            byte[] spreadsheetBytes,
            WebFormAppType webFormAppType,
            IClock clock)
        {
            using (MiniProfiler.Current.Step(nameof(FlexCelWorkbook) + "." + nameof(FlexCelWorkbook)))
            {
                this.spreadsheetBytes = spreadsheetBytes;
                this.clock = clock;
                this.webFormAppType = webFormAppType;
                this.CreatedTimestamp = this.clock.GetCurrentInstant();
                using (MiniProfiler.Current.Step("Creating XlsFile instance"))
                {
                    this.xlsxFile = new XlsFile();
                }

                MemoryStream memoryStream = null;
                try
                {
                    using (MiniProfiler.Current.Step("Creating memory stream"))
                    {
                        memoryStream = new MemoryStream(this.spreadsheetBytes);
                    }

                    using (MiniProfiler.Current.Step("FlexCel.XlsFile.Open"))
                    {
                        // Try to pause gen 2 garbage collection during opening of the spreadsheet
                        // so that it doesn't get held up
                        GCLatencyMode oldGCLatencyMode = GCSettings.LatencyMode;
                        try
                        {
                            GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
                            var watch = new System.Diagnostics.Stopwatch();
                            watch.Start();
                            this.xlsxFile.Open(memoryStream);
                            watch.Stop();
                        }
                        finally
                        {
                            GCSettings.LatencyMode = oldGCLatencyMode;
                        }
                    }

                    this.PatchRepeatingQuestionSetsFormulas();

                    // By default we'll choose to not read formula text from here on, since it's slower and usually not needed.
                    this.xlsxFile.IgnoreFormulaText = true;
                }
                finally
                {
                    if (memoryStream != null)
                    {
                        memoryStream.Dispose();
                    }
                }
            }
        }

        /// <summary>
        /// Gets the XlsFile object for direct access to the workbook.
        /// </summary>
        public XlsFile XlsFile => this.xlsxFile;

        /// <summary>
        /// Gets or sets the Instant this workbook instance was created.
        /// </summary>
        public Instant CreatedTimestamp { get; set; }

        /// <summary>
        /// Gets or sets the Instant this workbook was last used.
        /// </summary>
        public Instant LastUsedTimestamp { get; set; }

        /// <summary>
        /// Gets a dictionary of column names for column indexes for
        /// the table at the location.
        /// </summary>
        /// <param name="sheet">The index for the sheet which the table is in.</param>
        /// <param name="row">The index for the header row of the table.</param>
        /// <param name="startColumn">The starting column for the table.</param>
        /// <param name="endColumn">The ending column for the table.</param>
        /// <param name="tableName">THe name of the table (for debugging purposes).</param>
        /// <returns>A dictionary of column indexes.</returns>
        public Dictionary<int, string> GetTableColumnNames(
            int sheet,
            int row,
            int startColumn,
            int endColumn,
            string tableName)
        {
            // The xf is an ID of the excel formatting for a cell. It's passed in to receive the cell
            // formatting ID, which we don't care about, so we'll just ignore the result.
            int xF = -1;
            Dictionary<int, string> columnNames = new Dictionary<int, string>();
            for (var columnIndex = startColumn; columnIndex <= endColumn; columnIndex++)
            {
                object value = this.xlsxFile.GetCellValue(sheet, row, columnIndex, ref xF);
                if (!(value is string))
                {
                    throw new ErrorException(
                        Errors.Product.WorkbookParseFailure(
                            $"When reading the header row of {tableName}, we came across a header "
                            + "which was not a string. This is unexpected."));
                }

                columnNames.Add(columnIndex, value as string);
            }

            return columnNames;
        }

        /// <summary>
        /// Gets the cell value at the given coordinates.
        /// </summary>
        /// <typeparam name="TReturn">The type of value to be returned.</typeparam>
        /// <param name="sheetIndex">The sheet index.</param>
        /// <param name="rowIndex">The row index.</param>
        /// <param name="colIndex">The column index.</param>
        /// <returns>The value, if it was found and of the expected type.</returns>
        public Maybe<TReturn> GetCellValue<TReturn>(int sheetIndex, int rowIndex, int colIndex)
        {
            // The xf is an ID of the excel formatting for a cell. It's passed in to receive the cell
            // formatting ID, which we don't care about, so we'll just ignore the result.
            int xF = -1;
            object value = this.xlsxFile.GetCellValue(sheetIndex, rowIndex, colIndex, ref xF);
            switch (value)
            {
                case null:
                    return Maybe<TReturn>.None;
                case TFormula formula:
                    value = formula.Result;
                    break;
            }

            // perform type conversions
            Type returnType = typeof(TReturn);
            if (returnType == typeof(bool?))
            {
                return Maybe<TReturn>.From((TReturn)(object)this.CellValueToNullableBool(value));
            }
            else if (returnType == typeof(bool))
            {
                return Maybe<TReturn>.From((TReturn)(object)this.CellValueToBool(value));
            }
            else if (returnType == typeof(JObject))
            {
                return Maybe<TReturn>.From((TReturn)(object)this.CellValueToJObject(value));
            }
            else if (returnType.IsEnum)
            {
                return this.GetCellEnumValue<TReturn>(sheetIndex, rowIndex, colIndex);
            }
            else if ((returnType == typeof(int) || returnType == typeof(int?)) && value is string stringValue)
            {
                return string.IsNullOrWhiteSpace(stringValue)
                    ? Maybe<TReturn>.None
                    : Maybe<TReturn>.From((TReturn)(object)int.Parse(stringValue));
            }
            else if ((returnType == typeof(int) || returnType == typeof(int?)) && value is double)
            {
                return Maybe<TReturn>.From((TReturn)(object)Convert.ToInt32(value));
            }
            else if (returnType == typeof(string) && !(value is string))
            {
                if (value is double)
                {
                    return Maybe<TReturn>.From((TReturn)(object)value.ToString());
                }
            }

            return (value is TReturn && value != default)
                ? Maybe<TReturn>.From((TReturn)value)
                : Maybe<TReturn>.None;
        }

        /// <summary>
        /// Gets the value of a cell and parses it to an enum.
        /// </summary>
        /// <typeparam name="TEnum">The enum type.</typeparam>
        /// <param name="sheetIndex">The sheet index.</param>
        /// <param name="rowIndex">The row index.</param>
        /// <param name="colIndex">The column index.</param>
        /// <returns>The enum value.</returns>
        public Maybe<TEnum> GetCellEnumValue<TEnum>(int sheetIndex, int rowIndex, int colIndex)
        {
            var result = this.GetCellValue<string>(sheetIndex, rowIndex, colIndex);
            if (result.HasNoValue)
            {
                return Maybe<TEnum>.None;
            }

            // Use Humanizer to convert the string to the enum type
            var stringValue = result.Value;
            var enumType = typeof(TEnum);
            var enumValue = stringValue.ToEnumOrNull(typeof(TEnum));
            return Maybe<TEnum>.From((TEnum)(object)enumValue);
        }

        /// <summary>
        /// Gets the name of the style applied to the cell.
        /// </summary>
        /// <param name="sheetIndex">The sheet index.</param>
        /// <param name="rowIndex">The row index.</param>
        /// <param name="colIndex">The column index.</param>
        /// <returns>The name of the style, or null.</returns>
        public string GetCellStyleName(int sheetIndex, int rowIndex, int colIndex)
        {
            // The xf is an ID of the excel formatting for a cell.
            int formattingId = this.xlsxFile.GetCellFormat(sheetIndex, rowIndex, colIndex);
            TFlxFormat format = this.xlsxFile.GetFormat(formattingId);
            return format.ParentStyle;
        }

        /// <inheritdoc/>
        public string ReadTableTextAsString(string worksheetName, string tableName)
        {
            bool existingIgnoreFormulaText = this.xlsxFile.IgnoreFormulaText;
            this.xlsxFile.IgnoreFormulaText = true;

            try
            {
                this.LastUsedTimestamp = this.clock.GetCurrentInstant();
                string resultString = string.Empty;
                var sheetIndex = this.xlsxFile.GetSheetIndex(worksheetName);
                var range = this.xlsxFile.GetTable(tableName).Range;
                using (MiniProfiler.Current.Step("Read Output Cells"))
                {
                    int xF = 0;
                    for (var row = range.Top + 1; row <= range.Bottom; row++)
                    {
                        for (var col = range.Left; col <= range.Right; col++)
                        {
                            this.xlsxFile.RecalcCell(sheetIndex, row, col, true);
                            object value = this.xlsxFile.GetCellValue(sheetIndex, row, col, ref xF);
                            ThrowIfFormulaError(value, worksheetName, tableName, sheetIndex, row, col);
                            if (value != null)
                            {
                                resultString += value.ToString();
                            }
                        }
                    }
                }

                return resultString;
            }
            finally
            {
                this.xlsxFile.IgnoreFormulaText = existingIgnoreFormulaText;
            }
        }

        /// <inheritdoc/>
        public TableTextModel ReadTableText(string worksheetName, string tableName)
        {
            bool existingIgnoreFormulaText = this.xlsxFile.IgnoreFormulaText;
            this.xlsxFile.IgnoreFormulaText = true;

            try
            {
                this.LastUsedTimestamp = this.clock.GetCurrentInstant();
                TableTextModel tableModel = new TableTextModel();
                string resultString = string.Empty;
                var sheetIndex = this.xlsxFile.GetSheetIndex(worksheetName);
                var range = this.xlsxFile.GetTable(tableName).Range;
                int endColumn = range.Right;
                List<List<string>> rows = new List<List<string>>();
                this.xlsxFile.SetSheetSelected(sheetIndex, true);

                // this.XlsxFile.Recalc();
                for (int row = range.Top; row <= range.Bottom; row++)
                {
                    List<string> columnTextList = new List<string>();
                    for (int column = range.Left; column <= endColumn; column++)
                    {
                        this.xlsxFile.RecalcCell(sheetIndex, row, column, true);
                        var columnIndex = this.ColumnIndexToColumnLetter(column);
                        var value = this.xlsxFile.GetCellValue(worksheetName + "!" + columnIndex + row);
                        var firstColumnValue = this.xlsxFile.GetCellValue(worksheetName + "!" + this.ColumnIndexToColumnLetter(range.Left) + row);

                        // flexcell gets the blank cell of Styles as ErrValue, used our own textjoin function insteaad
                        if ((value + string.Empty).ToLower() == FlexCelErrValue && (firstColumnValue + string.Empty).ToLower().IndexOf("styles") > -1)
                        {
                            this.xlsxFile.RecalcCell(sheetIndex, row, column, true);
                            value = "\"fonts\":[" + this.GetColumnTextJoin(StyleWorksheetName, StylesTableName, UbindTableColumns.StylesConfigFont) + "]";
                        }

                        columnTextList.Add(value != null ? value + string.Empty : null);
                    }

                    rows.Add(columnTextList);
                }

                tableModel.Rows = rows;
                return tableModel;
            }
            finally
            {
                this.xlsxFile.IgnoreFormulaText = existingIgnoreFormulaText;
            }
        }

        /// <inheritdoc/>
        public string ReadTableColumn(string worksheetName, string tableName, string columnName)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void PatchData(string worksheetName, string tableName, string columnName, JArray jsonData)
        {
            this.LastUsedTimestamp = this.clock.GetCurrentInstant();
            var range = this.xlsxFile.GetTable(tableName).Range;
            int startColumnIndex = this.GetColumnIndex(tableName, columnName);
            string dataTypeColumn = this.GetColumnLetter(tableName, DataTypeColumnName);
            int row = range.Top;

            foreach (JArray innerArray in jsonData)
            {
                int column = startColumnIndex;
                foreach (JToken item in innerArray)
                {
                    dynamic dataType = this.xlsxFile.GetCellValue(worksheetName + "!" + dataTypeColumn + row);
                    var columnLetter = this.ColumnIndexToColumnLetter(column);
                    this.SetCellValue(worksheetName, item, columnLetter, row, dataType);
                    column++;
                }

                row++;
            }
        }

        /// <inheritdoc/>
        public void PatchData(string worksheetName, string address, JArray jsonData)
        {
            this.LastUsedTimestamp = this.clock.GetCurrentInstant();
            TXlsNamedRange namedRange = this.GetNamedRange(worksheetName, address);
            int startColumnIndex = namedRange.Left;
            string dataTypeColumn = this.GetColumnLetter(RepeatingQuestionSetsAnswerTableName, DataTypeColumnName);
            int row = namedRange.Top;

            foreach (JArray innerArray in jsonData)
            {
                var column = startColumnIndex;
                foreach (JToken item in innerArray)
                {
                    dynamic dataType = this.xlsxFile.GetCellValue(worksheetName + "!" + dataTypeColumn + row);
                    var columnLetter = this.ColumnIndexToColumnLetter(column);
                    this.SetCellValue(worksheetName, item, columnLetter, row, dataType);
                    column++;
                }

                row++;
            }
        }

        /// <inheritdoc/>
        public void UpdateAdditionalRatingFactors(IAdditionalRatingFactors additionalRatingFactors)
        {
            this.LastUsedTimestamp = this.clock.GetCurrentInstant();
            using (MiniProfiler.Current.Step(nameof(FlexCelWorkbook) + "." + nameof(this.UpdateAdditionalRatingFactors)))
            {
                if (additionalRatingFactors != null)
                {
                    using (MiniProfiler.Current.Step(nameof(FlexCelWorkbook) + "." + nameof(this.UpdateAdditionalRatingFactors)))
                    {
                        var table = this.xlsxFile.GetTable(RatingFactorTableName);
                        if (table != null)
                        {
                            foreach (var ratingFactor in additionalRatingFactors.AdditionalRatingFactorsMap)
                            {
                                if (ratingFactor.Value is List<object>)
                                {
                                    this.WriteToAdditionalRatingFactorTables(ratingFactor.Key, ratingFactor.Value);
                                }
                                else
                                {
                                    this.WriteTablePropertyValue(
                                        RatingFactorSheetName,
                                        RatingFactorTableName,
                                        ratingFactor.Key,
                                        ratingFactor.Value);
                                }
                            }
                        }
                    }
                }
                else
                {
                    this.ClearAdditionalRatingFactors();
                }
            }
        }

        /// <inheritdoc/>
        public dynamic ReadTablePropertyValue(string sheetName, string tableName, string propertyName, bool ignoreFormulaText = true)
        {
            bool existingIgnoreFormulaText = this.xlsxFile.IgnoreFormulaText;
            this.xlsxFile.IgnoreFormulaText = ignoreFormulaText;

            try
            {
                this.LastUsedTimestamp = this.clock.GetCurrentInstant();
                var range = this.xlsxFile.GetTable(tableName).Range;
                string propertyColumnLetter = this.GetColumnLetter(tableName, UbindTableColumns.Property);
                string valueColumnLetter = this.GetColumnLetter(tableName, UbindTableColumns.Value);
                for (var row = range.Top + 1; row <= range.Bottom; row++)
                {
                    var nameValue = this.xlsxFile.GetCellValue(sheetName + "!" + propertyColumnLetter + row);

                    if (propertyName == (nameValue + string.Empty))
                    {
                        return this.xlsxFile.GetCellValue(sheetName + "!" + valueColumnLetter + row);
                    }
                }

                return null;
            }
            finally
            {
                this.xlsxFile.IgnoreFormulaText = existingIgnoreFormulaText;
            }
        }

        /// <inheritdoc/>
        public TXlsNamedRange GetNamedRange(string worksheetName, string namedRange)
        {
            var sheetIndex = this.xlsxFile.GetSheetIndex(worksheetName);
            return this.xlsxFile.GetNamedRange(namedRange, sheetIndex);
        }

        private static void ThrowIfFormulaError(object value, string worksheetName, string tableName, int sheetIndex, int row, int column)
        {
            if (value is TFlxFormulaErrorValue error)
            {
                ThrowFormulaError(error, worksheetName, tableName, sheetIndex, row, column);
            }
            else if (value is TFormula formula)
            {
                // sometimes the string "ErrNA" will come through as a normal value instead of a TFlxFormulaErrorValue
                string result = formula.Result.ToString();

                switch (result)
                {
                    case "ErrRef":
                        ThrowFormulaError(TFlxFormulaErrorValue.ErrRef, worksheetName, tableName, sheetIndex, row, column);
                        break;
                    case "ErrNA":
                        ThrowFormulaError(TFlxFormulaErrorValue.ErrNA, worksheetName, tableName, sheetIndex, row, column);
                        break;
                    case "ErrValue":
                        ThrowFormulaError(TFlxFormulaErrorValue.ErrValue, worksheetName, tableName, sheetIndex, row, column);
                        break;
                    case "ErrName":
                        ThrowFormulaError(TFlxFormulaErrorValue.ErrName, worksheetName, tableName, sheetIndex, row, column);
                        break;
                }
            }
        }

        private static void ThrowFormulaError(TFlxFormulaErrorValue errorValue, string worksheetName, string tableName, int sheetIndex, int row, int column)
        {
            Error error = null;
            var additionalDetails = new List<string>()
            {
                $"Worksheet name: {worksheetName}",
                $"Table name: {tableName}",
                $"Excel error code: {errorValue.ToString()}",
            };
            if (row != 0 || column != 0)
            {
                var cellAddress = new TCellAddress(row, column);
                additionalDetails.Add($"Cell reference: {cellAddress.CellRef}");
            }

            switch (errorValue)
            {
                case TFlxFormulaErrorValue.ErrNull:
                    error = Errors.Calculation.Spreadsheet.FormulaError(
                        "A cell has a null value (ErrNull). Please review all inputs to ensure nulls are never passed in.",
                        worksheetName,
                        tableName,
                        sheetIndex,
                        row,
                        column,
                        additionalDetails);
                    break;
                case TFlxFormulaErrorValue.ErrDiv0:
                    error = Errors.Calculation.Spreadsheet.FormulaError(
                        "A cell is attempting to divide by zero (ErrDiv0). Please review all inputs and formulas to ensure that "
                        + "a value can never be divided by zero. ",
                        worksheetName,
                        tableName,
                        sheetIndex,
                        row,
                        column,
                        additionalDetails);
                    break;
                case TFlxFormulaErrorValue.ErrValue:
                    error = Errors.Calculation.Spreadsheet.FormulaError(
                        "A cell has an invalid value (ErrValue). Please ensure all values input can be handled by Excel.",
                        worksheetName,
                        tableName,
                        sheetIndex,
                        row,
                        column,
                        additionalDetails);
                    break;
                case TFlxFormulaErrorValue.ErrRef:
                    error = Errors.Calculation.Spreadsheet.FormulaError(
                        "A cell was referencing a deleted or invalid cell (ErrRef). Please review formulas to ensure they are referencing valid cells.",
                        worksheetName,
                        tableName,
                        sheetIndex,
                        row,
                        column,
                        additionalDetails);
                    break;
                case TFlxFormulaErrorValue.ErrName:
                    error = Errors.Calculation.Spreadsheet.FormulaError(
                        "A cell has an invalid name (ErrName). Please ensure when you name a cell, it's a valid and allowed name.",
                        worksheetName,
                        tableName,
                        sheetIndex,
                        row,
                        column,
                        additionalDetails);
                    break;
                case TFlxFormulaErrorValue.ErrNum:
                    error = Errors.Calculation.Spreadsheet.FormulaError(
                        "A cell has an invalid number (ErrNum). Please ensure when you write a number into a cell, it's a valid number.",
                        worksheetName,
                        tableName,
                        sheetIndex,
                        row,
                        column,
                        additionalDetails);
                    break;
                case TFlxFormulaErrorValue.ErrNA:
                    error = Errors.Calculation.Spreadsheet.FormulaError(
                        "An input to the formula was found to not be available (ErrNA). Please review all formulas for unavailable inputs.",
                        worksheetName,
                        tableName,
                        sheetIndex,
                        row,
                        column,
                        additionalDetails);
                    break;
                case TFlxFormulaErrorValue.ErrGettingData:
                    error = Errors.Calculation.Spreadsheet.FormulaError(
                        "A cell reference cannot be evaluated because the value for the cell has not been retrieved or calculated (ErrGettingData).",
                        worksheetName,
                        tableName,
                        sheetIndex,
                        row,
                        column,
                        additionalDetails);
                    break;
                default:
                    error = Errors.Calculation.Spreadsheet.FormulaError(
                        "An unknown or general error has occurred. Please raise a support ticket.",
                        worksheetName,
                        tableName,
                        sheetIndex,
                        row,
                        column,
                        additionalDetails);
                    break;
            }

            throw new ErrorException(error);
        }

        private static void ThrowIfResultIsError(string flexCelResult)
        {
            try
            {
                var parsedJson = JObject.Parse(flexCelResult);
            }
            catch (Exception ex)
            {
                // TODO: Review error message in the flexCelResult, and if it is not clear enough, extend
                // this method to take a descripton of the operation being attempted, and double wrap the
                // underlying exception, first with the passed in details, before secondly with the error
                // message specified in the global error message set.
                var hasConfigError = flexCelResult.ToLower().Contains(FlexCelErrValue);
                var additionalDetails = new List<string>
                {
                    ex.Message,
                };
                var data = new JObject
                {
                    { "rawCalculationOutput", flexCelResult },
                };

                throw new UBindFlexCelException(
                    Errors.Product.MisConfiguration(ErrorMessage.FlexCel.GetCalculationResultErrorMessage(hasConfigError), additionalDetails, data), ex);
            }
        }

        private bool CellValueToBool(object value)
        {
            if (value is bool boolValue)
            {
                return boolValue;
            }
            else if (value is string stringValue)
            {
                return stringValue.EqualsIgnoreCase("true") || stringValue.EqualsIgnoreCase("yes");
            }

            return false;
        }

        private bool? CellValueToNullableBool(object value)
        {
            if (value is bool boolValue)
            {
                return boolValue;
            }
            else if (value is string stringValue)
            {
                if (stringValue.EqualsIgnoreCase("true") || stringValue.EqualsIgnoreCase("yes"))
                {
                    return true;
                }
                else if (stringValue.EqualsIgnoreCase("false") || stringValue.EqualsIgnoreCase("no"))
                {
                    return false;
                }
            }

            return null;
        }

        private JObject CellValueToJObject(object value)
        {
            string json = value as string;
            if (json != null)
            {
                try
                {
                    return JObject.Parse(json);
                }
                catch (JsonReaderException ex)
                {
                    var error = Errors.JsonDocument.JsonInvalid(
                        "workbook cell value",
                        ex.Message,
                        ex.LineNumber,
                        ex.LinePosition,
                        ex.Path,
                        json);
                    throw new ErrorException(error);
                }
            }

            return null;
        }

        /// <summary>
        /// Find the cell within the range that has an error and then throws an exception with that cell's details.
        /// </summary>
        private void ThrowErrorForCellInRange(TFlxFormulaErrorValue errorValue, string worksheetName, string tableName, int sheetIndex, TXlsCellRange range)
        {
            for (var row = range.Top; row <= range.Bottom; row++)
            {
                for (var column = range.Left; column <= range.Right; column++)
                {
                    // TODO: work out what this XF cell format thing is for
                    /*
                    int cellFormat = 0;
                    object value = this.XlsxFile.GetCellValue(sheetIndex, row, column, ref cellFormat);
                    */
                    object value = this.xlsxFile.RecalcCell(sheetIndex, row, column, true);
                    if (value is TFlxFormulaErrorValue)
                    {
                        ThrowFormulaError((TFlxFormulaErrorValue)value, worksheetName, tableName, sheetIndex, row, column);
                    }
                }
            }

            ThrowFormulaError(errorValue, worksheetName, tableName, sheetIndex, range.Top - 1, range.Left);
        }

        private void ClearAdditionalRatingFactors()
        {
            using (MiniProfiler.Current.Step(nameof(FlexCelWorkbook) + "." + nameof(this.ClearAdditionalRatingFactors)))
            {
                var table = this.xlsxFile.GetTable(RatingFactorTableName);
                if (table != null)
                {
                    this.WriteTablePropertyValue(
                                                    RatingFactorSheetName,
                                                    RatingFactorTableName,
                                                    UbindTableProperty.LastPremium,
                                                    null);

                    this.WriteTablePropertyValue(
                                                 RatingFactorSheetName,
                                                 RatingFactorTableName,
                                                 UbindTableProperty.TotalClaimsPremium,
                                                 null);
                }
            }
        }

        private void PatchRepeatingQuestionSetsFormulas()
        {
            bool existingIgnoreFormulaText = this.xlsxFile.IgnoreFormulaText;

            // we need to ensure we can read the formulas for this operation
            this.xlsxFile.IgnoreFormulaText = false;

            try
            {
                using (MiniProfiler.Current.Step(nameof(FlexCelWorkbook) + "." + nameof(this.PatchRepeatingQuestionSetsFormulas)))
                {
                    var sheetIndex = this.xlsxFile.GetSheetIndex(RepeatingQuestionSetsWorksheetName);
                    var range = this.xlsxFile.GetTable(RepeatingQuestionSetsAnswerTableName).Range;
                    this.xlsxFile.SetSheetSelected(sheetIndex, true);
                    for (var row = range.Top; row <= range.Bottom; row++)
                    {
                        for (int count = 1; count <= 10; count++)
                        {
                            string countColumnLetter = this.GetColumnLetter(RepeatingQuestionSetsAnswerTableName, CountColumnName + count);
                            string invalidColumnLetter = this.GetColumnLetter(RepeatingQuestionSetsAnswerTableName, InvalidColumnName + count);

                            // escape for new workbook where Count{count} and Invalid{count} was no longer used.
                            if (string.IsNullOrEmpty(countColumnLetter) || string.IsNullOrEmpty(invalidColumnLetter))
                            {
                                continue;
                            }

                            var countColumnValue = this.xlsxFile.GetCellValue(RepeatingQuestionSetsWorksheetName + "!" + countColumnLetter + row);
                            var invalidColumnValue = this.xlsxFile.GetCellValue(RepeatingQuestionSetsWorksheetName + "!" + invalidColumnLetter + row);

                            if (countColumnValue != null && countColumnValue.GetType() == typeof(TFormula))
                            {
                                TFormula formulaCell = (TFormula)countColumnValue;
                                if (formulaCell.Text.ToLower().IndexOf(ExcelFunctions.NumberValue) > -1)
                                {
                                    formulaCell.Text = CountColumnForumulaTemplate.Replace(ValueToReplace, count + string.Empty);
                                    this.xlsxFile.SetCellValue(RepeatingQuestionSetsWorksheetName + "!" + countColumnLetter + row, formulaCell);
                                }
                                else
                                {
                                    break;
                                }
                            }

                            if (invalidColumnValue != null && invalidColumnValue.GetType() == typeof(TFormula))
                            {
                                TFormula formulaCell = (TFormula)invalidColumnValue;
                                if (formulaCell.Text.ToLower().IndexOf(ExcelFunctions.NumberValue) > -1)
                                {
                                    formulaCell.Text = InvalidColumnFormulaTemplate.Replace(ValueToReplace, count + string.Empty);
                                    this.xlsxFile.SetCellValue(RepeatingQuestionSetsWorksheetName + "!" + invalidColumnLetter + row, formulaCell);
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            finally
            {
                this.xlsxFile.IgnoreFormulaText = existingIgnoreFormulaText;
            }
        }

        private void WriteTablePropertyValue(string sheetName, string tableName, string propertyName, dynamic value)
        {
            var table = this.xlsxFile.GetTable(tableName);
            if (table != null)
            {
                var range = this.xlsxFile.GetTable(tableName).Range;
                string nameColumnLetter = this.GetColumnLetter(tableName, UbindTableColumns.Property);
                string valueColumnLetter = this.GetColumnLetter(tableName, UbindTableColumns.Value);
                for (var row = range.Top + 1; row <= range.Bottom; row++)
                {
                    var nameValue = this.xlsxFile.GetCellValue(sheetName + "!" + nameColumnLetter + row);

                    if (propertyName == (nameValue + string.Empty))
                    {
                        this.xlsxFile.SetCellValue(sheetName + "!" + valueColumnLetter + row, value);
                    }
                }
            }
        }

        private string GetColumnTextJoin(string sheetName, string tableName, string columnName)
        {
            string joinedString = string.Empty;
            var sheetIndex = this.xlsxFile.GetSheetIndex(sheetName);
            var range = this.xlsxFile.GetTable(tableName).Range;
            for (var row = range.Top + 1; row <= range.Bottom; row++)
            {
                string columnLetter = this.GetColumnLetter(tableName, columnName);
                var columnValue = this.xlsxFile.GetCellValue(sheetName + "!" + columnLetter + row);

                if (!string.IsNullOrEmpty(columnValue + string.Empty) && (columnValue + string.Empty).ToLower() != "errvalue")
                {
                    joinedString += columnValue + ",";
                }
            }

            return joinedString;
        }

        private void SetCellValue(string worksheetName, dynamic item, string column, int row, dynamic dataType)
        {
            if (item == null)
            {
                this.xlsxFile.SetCellValue(worksheetName + "!" + column + row, string.Empty);
                return;
            }
            else if (this.numberTypes.Contains(dataType + string.Empty))
            {
                var isNumeric = decimal.TryParse((item.Value + string.Empty).Replace("$", string.Empty).Replace(",", string.Empty), out decimal n);
                this.xlsxFile.SetCellValue(worksheetName + "!" + column + row, isNumeric ? n : item);
            }
            else if ((dataType + string.Empty).ToLower() == "boolean")
            {
                string stringVal = item.Value + string.Empty;
                stringVal = stringVal.ToLower();
                this.xlsxFile.SetCellValue(worksheetName + "!" + column + row, stringVal);
            }
            else
            {
                var itemTemp = item;
                var itemValue = item.Value ?? string.Empty;
                itemTemp.Value = this.EscapeBackSlashAndDoubleQuote(itemValue.ToString());
                this.xlsxFile.SetCellValue(worksheetName + "!" + column + row, itemTemp);
            }
        }

        private string EscapeBackSlashAndDoubleQuote(string input)
        {
            return input.Replace("\\", "\\\\")
                        .Replace("\"", "\\\"")
                        .Replace(Environment.NewLine, "\\r\\n")
                        .Replace("\t", "\\t");
        }

        private void Recalculate(ref XlsFile xls, string worksheetName, string tableName)
        {
            var sheetIndex = this.xlsxFile.GetSheetIndex(worksheetName);
            var range = this.xlsxFile.GetTable(tableName).Range;
            int endColumn = range.Right;
            string resultString = string.Empty;
            for (int row = range.Top + 1; row <= range.Bottom; row++)
            {
                for (int column = range.Left; column <= endColumn; column++)
                {
                    this.xlsxFile.RecalcCell(sheetIndex, row, column, true);
                }
            }
        }

        private string GetColumnLetter(string tableName, string columnName)
        {
            var tableSheet = this.xlsxFile.GetTableSheet(tableName);
            var column = this.xlsxFile.GetTable(tableName).ColumnFromName(columnName, this.xlsxFile, tableSheet);
            var columnNumber = this.xlsxFile.GetTable(tableName).ColumnInGridFromId(column.Id);
            return this.ColumnIndexToColumnLetter(columnNumber);
        }

        private int GetColumnIndex(string tableName, string columnName)
        {
            var tableSheet = this.xlsxFile.GetTableSheet(tableName);
            var column = this.xlsxFile.GetTable(tableName).ColumnFromName(columnName, this.xlsxFile, tableSheet);
            return this.xlsxFile.GetTable(tableName).ColumnInGridFromId(column.Id);
        }

        private string ColumnIndexToColumnLetter(int colIndex)
        {
            int div = colIndex;
            string colLetter = string.Empty;
            int mod = 0;

            while (div > 0)
            {
                mod = (div - 1) % 26;
                colLetter = (char)(65 + mod) + colLetter;
                div = (int)((div - mod) / 26);
            }

            return colLetter;
        }

        /// <summary>
        /// Writes to additonal rating factor tables if there has.
        /// </summary>
        /// <param name="propertyName">The property name.</param>
        /// <param name="values">The array values.</param>
        private void WriteToAdditionalRatingFactorTables(string propertyName, List<object> values)
        {
            if (values == null)
            {
                return;
            }

            var tableName = RatingFactorTableName + "_" + propertyName;
            var hasTable = this.xlsxFile.HasTable(tableName);
            if (hasTable)
            {
                var range = this.xlsxFile.GetTable(tableName).Range;
                Dictionary<string, char> propertyColumnPair = new Dictionary<string, char>();

                // get columns of the table
                for (var i = 0; i < range.ColCount; i++)
                {
                    char col = (char)((int)range.CellRef.First() + i);
                    var headerValue = this.xlsxFile.GetCellValue(RatingFactorSheetName + "!" + col + range.Top).ToString();
                    propertyColumnPair.Add(headerValue.ToCamelCase(), col);
                }

                // add it to the table.
                if (values.Any())
                {
                    bool rowsInsufficient = values.Count - (range.RowCount - 1) < values.Count;

                    if (rowsInsufficient)
                    {
                        var firstCol = propertyColumnPair.OrderBy(x => x.Value).First().Value;
                        var lastCol = propertyColumnPair.OrderBy(x => x.Value).Last().Value;
                        var topRow = range.Top + 1;

                        // deleting all rows leaving only 1 row left.
                        this.ResetAdditionalRatingFactorTables(tableName, range, propertyColumnPair);

                        // add more rows if its insufficient.
                        var insertCellRange = new TXlsCellRange(firstCol + $"{topRow}:" + lastCol + topRow);
                        var tableSheet = this.xlsxFile.GetTableSheet(tableName);
                        var previousActiveSheet = this.xlsxFile.ActiveSheet;
                        this.xlsxFile.ActiveSheet = tableSheet;
                        this.xlsxFile.InsertAndCopyRange(insertCellRange, topRow, firstCol, values.Count - 1, TFlxInsertMode.ShiftRowDown, TRangeCopyMode.None, this.XlsFile, tableSheet);
                        this.XlsFile.ActiveSheet = previousActiveSheet;
                        range = this.xlsxFile.GetTable(tableName).Range;
                    }

                    var currentItemIndex = 0;

                    // place values to the tables.
                    for (var row = range.Top + 1; row <= range.Bottom; row++)
                    {
                        if (values.Count == currentItemIndex)
                        {
                            continue;
                        }

                        var value = values[currentItemIndex];
                        foreach (var propertyColumn in propertyColumnPair)
                        {
                            object val = (value is JObject ?
                            ((JValue)(value as JObject)[propertyColumn.Key])?.Value
                            : value.GetPropertyValue(propertyColumn.Key)) ?? string.Empty;
                            this.xlsxFile.SetCellValue(RatingFactorSheetName + "!" + propertyColumn.Value + row, val);
                        }

                        currentItemIndex++;
                    }
                }

                // if there is no values, clear the table.
                else
                {
                    this.ResetAdditionalRatingFactorTables(tableName, range, propertyColumnPair);
                }
            }
        }

        /// <summary>
        /// Resets additional rating factor tables, and leaves 1 row with all cells default value to empty string.
        /// </summary>
        private void ResetAdditionalRatingFactorTables(
            string tableName,
            TXlsCellRange range,
            Dictionary<string, char> propertyColumnPair)
        {
            var topRowToDelete = range.Top + 1;
            var bottomRowToDelete = range.Bottom - 1;
            if (bottomRowToDelete >= topRowToDelete)
            {
                var firstCol = propertyColumnPair.OrderBy(x => x.Value).First().Value;
                var lastCol = propertyColumnPair.OrderBy(x => x.Value).Last().Value;
                TXlsCellRange deleteCellRange = new TXlsCellRange(firstCol + $"{topRowToDelete}:" + lastCol + bottomRowToDelete);
                var tableSheet = this.xlsxFile.GetTableSheet(tableName);
                this.xlsxFile.DeleteRange(tableSheet, tableSheet, deleteCellRange, TFlxInsertMode.ShiftRowDown);

                // Last row should be set with default values.
                foreach (var prop in propertyColumnPair)
                {
                    this.xlsxFile.SetCellValue(RatingFactorSheetName + "!" + prop.Value + topRowToDelete, string.Empty);
                }
            }
        }

        /// <summary>
        /// Model for response from table text request.
        /// </summary>
        public class TableTextModel
        {
            /// <summary>
            /// Gets or sets the data in the response.
            /// </summary>
            [JsonProperty(PropertyName = "Text")]
            public IEnumerable<IEnumerable<string>> Rows { get; set; }
        }

        private static class ExcelFunctions
        {
            public const string NumberValue = "numbervalue";
        }

        private static class UbindTableColumns
        {
            public const string ConfigCountFwd = "Config Count Fwd";
            public const string ConfigCountBack = "Config Count Back";
            public const string UsedValue = "Used Value";
            public const string CalculationCallBack = "Calculation Count Back";
            public const string OutputConfig = "Output Config";
            public const string StylesConfigFont = "Config Font";
            public const string Property = "Property";
            public const string Value = "Value";
        }

        private static class UbindTableProperty
        {
            public const string LastPremium = "Last Premium";
            public const string TotalClaimsPremium = "Total Claims Amount";
        }
    }
}
