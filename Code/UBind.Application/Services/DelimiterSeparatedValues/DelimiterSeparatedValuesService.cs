// <copyright file="DelimiterSeparatedValuesService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Services.DelimiterSeparatedValues
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Dynamic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using CsvHelper;
    using CsvHelper.Configuration;
    using Humanizer;

    /// <inheritdoc/>
    public class DelimiterSeparatedValuesService : IDelimiterSeparatedValuesService
    {
        private readonly IDelimiterSeparatedValuesFileProvider delimiterSeparatedValuesFileProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelimiterSeparatedValuesService"/> class.
        /// </summary>
        /// <param name="delimiterSeparatedValuesFileProvider">The delimiter separated values file provider.</param>
        public DelimiterSeparatedValuesService(IDelimiterSeparatedValuesFileProvider delimiterSeparatedValuesFileProvider)
        {
            this.delimiterSeparatedValuesFileProvider = delimiterSeparatedValuesFileProvider;
        }

        /// <inheritdoc/>
        public DataTable ConvertDelimiterSeparatedValuesToDataTable(
            string fileFullPath,
            string delimiter,
            DataTable dataTableContainerAndDefinition)
        {
            using (var reader = new StreamReader(fileFullPath))
            {
                var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                    Delimiter = delimiter,
                    MissingFieldFound = null,
                    IgnoreBlankLines = false,
                    TrimOptions = TrimOptions.Trim,
                };

                using (var csv = new CsvReader(reader, csvConfig))
                {
                    csv.Context.TypeConverterOptionsCache.GetOptions<string>().NullValues.Add(string.Empty);

                    if (csv.Configuration.HasHeaderRecord)
                    {
                        csv.Read();
                        csv.ReadHeader();
                    }

                    var valueTypes = this.GetDataTableValueTypes(dataTableContainerAndDefinition);

                    var valueBuffer = new object[valueTypes.Length];

                    dataTableContainerAndDefinition.BeginLoadData();
                    while (csv.Read())
                    {
                        for (var i = 0; i < valueBuffer.Length; i++)
                        {
                            valueBuffer[i] = csv.GetField(valueTypes[i], i);
                        }

                        dataTableContainerAndDefinition.LoadDataRow(valueBuffer, true);
                    }

                    dataTableContainerAndDefinition.EndLoadData();
                }
            }

            return dataTableContainerAndDefinition;
        }

        /// <inheritdoc/>
        public DataTable ConvertCsvStringToDataTable(string csvString)
        {
            System.Data.DataTable dataTable = new System.Data.DataTable();

            using (var reader = new StringReader(csvString))
            using (var csv = new CsvHelper.CsvReader(reader, CultureInfo.InvariantCulture))
            {
                csv.Read();
                csv.ReadHeader();
                var records = csv.GetRecords<dynamic>().ToList();

                var col = csv.HeaderRecord.Select(h => new DataColumn(h.Trim().Replace('-', '_').Camelize()));
                dataTable.Columns.AddRange(col.ToArray());

                foreach (var record in records)
                {
                    dataTable.Rows.Add((record as ExpandoObject).Select(r => r.Value?.ToString().Trim()).ToArray());
                }
            }

            return dataTable;
        }

        /// <inheritdoc/>
        public IReadOnlyList<(string FileName, string GroupName)> GetDsvFilesWithGroup(DelimiterSeparatedValuesFileTypes delimiterSeparatedValuesFileType, string basePath, Func<string, string> funcConstructGroupName)
        {
            var files = this.delimiterSeparatedValuesFileProvider.GetDelimiterSeparatedValuesFiles(basePath, delimiterSeparatedValuesFileType);

            return files.Select(fileNames => (fileNames, funcConstructGroupName(fileNames))).ToArray();
        }

        private Type[] GetDataTableValueTypes(DataTable dataTableDefinition)
        {
            var valueTypes = new Type[dataTableDefinition.Columns.Count];

            for (var i = 0; i < valueTypes.Length; i++)
            {
                var dataColumn = dataTableDefinition.Columns[i];
                var type = dataColumn.DataType;
                if (dataColumn.AllowDBNull && type.IsValueType)
                {
                    type = typeof(Nullable<>).MakeGenericType(type);
                }

                valueTypes[i] = type;
            }

            return valueTypes;
        }
    }
}
