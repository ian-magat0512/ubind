// <copyright file="MsExcelEngineService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.FileHandling
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.IO;
    using global::FlexCel.Core;
    using global::FlexCel.XlsAdapter;
    using Newtonsoft.Json.Linq;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;

    /// <inheritdoc />
    public class MsExcelEngineService : IMsExcelEngineService
    {
        /// <inheritdoc />
        public IMsExcelEngineDatasource Datasource { get; set; }

        /// <inheritdoc />
        public byte[] GenerateContent(string sourceFilename, string outputFilename)
        {
            Contract.Assert(this.Datasource != null);
            var details = new List<string>
                {
                    $"Source Filename: {sourceFilename}",
                    $"Output Filename: {outputFilename}",
                };

            try
            {
                XlsFile xls = new XlsFile(new MemoryStream(this.Datasource.Content), true);
                xls.ActiveFileName = outputFilename;

                bool isDev = this.Datasource.Environment == DeploymentEnvironment.Development;
                int emptyCellFormat = isDev ? this.CreateCellFormat(xls, Colors.Red) : 0;
                int valueCellFormat = isDev ? this.CreateCellFormat(xls, Colors.Yellow) : 0;

                for (int sheetIndex = 1; sheetIndex <= xls.SheetCount; sheetIndex++)
                {
                    xls.ActiveSheet = sheetIndex;

                    for (int tableIndex = 1; tableIndex <= xls.TableCountInSheet; tableIndex++)
                    {
                        var table = xls.GetTable(tableIndex);
                        var range = table.Range;
                        var row = range.Top + 1;

                        // Provider can handle both jsonPointer and jsonPath
                        var objectJsonPath = table.Comment;

                        // Only tables WITH comments that have a VALID json path/pointer, that we need to process
                        var dataObject = this.Datasource.GetObject(objectJsonPath);
                        if (dataObject == null)
                        {
                            continue;
                        }

                        var rowCount = ((JContainer)dataObject).Count;

                        if (rowCount > 0)
                        {
                            table.Range =
                            new TXlsCellRange(range.Top, range.Left, range.Top + rowCount, range.Right);
                            xls.SetTable(table);
                        }

                        var jsonSelectorsForColumns = this.GetJsonSelectorsForColumns(xls, range);
                        foreach (var data in dataObject)
                        {
                            if (data == null || data.Type == JTokenType.Null)
                            {
                                continue;
                            }

                            foreach (var jsonSelectorForColumn in jsonSelectorsForColumns)
                            {
                                var col = jsonSelectorForColumn.Key;
                                var jsonSelector = jsonSelectorForColumn.Value;
                                var propertyValue = ((JObject)data).GetToken(jsonSelector);
                                int xFormat = propertyValue == null ? emptyCellFormat : valueCellFormat;

                                xls.SetCellValue(row, col, propertyValue, xFormat);
                            }

                            row++;
                        }
                    }
                }

                return XlsToByteArray(xls);
            }
            catch (FlexCelXlsAdapterException ex)
            {
                throw new ErrorException(Errors.Automation.ExcelSourceCorruptedOrInvalidFormat(details), ex);
            }
            catch (Exception ex)
            {
                throw new ErrorException(Errors.Automation.ExcelGenerateContentFailed(details), ex);
            }
        }

        private static byte[] XlsToByteArray(XlsFile xls)
        {
            using (var ms = new MemoryStream())
            {
                var extension = Path.GetExtension(xls.ActiveFileName).ToLower();

                var tempFilename = Path.ChangeExtension(Path.GetTempFileName(), extension);
                xls.Save(tempFilename);
                return System.IO.File.ReadAllBytes(tempFilename);
            }
        }

        private int CreateCellFormat(XlsFile xls, TUIColor backgroundColor)
        {
            TFlxFormat format = xls.GetCellVisibleFormatDef(1, 1);
            format.FillPattern.Pattern = TFlxPatternStyle.Solid;
            format.FillPattern.FgColor = backgroundColor;
            return xls.AddFormat(format);
        }

        /// <summary>
        /// The 2nd row in the spreadsheet table will have a path into the data in JSON pointer format, e.g. "/CreatedDate".
        /// Here we get these paths so that we know which data value to insert.
        /// It's ok for some columns to not have Json Paths in the 2nd row, they will be ignored.
        /// At least one column must have a valid json path/pointer in it.
        /// </summary>
        private Dictionary<int, string> GetJsonSelectorsForColumns(XlsFile xls, TXlsCellRange range)
        {
            var jsonSelectorsForColumns = new Dictionary<int, string>();
            for (int col = 0; col < range.ColCount; col++)
            {
                var jsonPointer = xls.GetCellValue(range.Top + 1, range.Left + col);
                if (jsonPointer != null)
                {
                    jsonSelectorsForColumns.Add(range.Left + col, jsonPointer.ToString());
                }
            }

            if (jsonSelectorsForColumns.Count == 0)
            {
                throw new ErrorException(Errors.Automation.ExcelJsonPathNotFoundError());
            }

            return jsonSelectorsForColumns;
        }
    }
}
