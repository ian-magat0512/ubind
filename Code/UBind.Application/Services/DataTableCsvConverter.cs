// <copyright file="DataTableCsvConverter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Humanizer;
    using ServiceStack.Text;

    public class DataTableCsvConverter
    {
        public static Dictionary<Type, Func<object, string>> DataTableDefinitionCsvSerializers = new Dictionary<Type, Func<object, string>>()
        {
            {
                typeof(DateTime),
                (value) => ((DateTime?)value)?.ToString("s") ?? string.Empty
            },
            {
                typeof(decimal),
                (value) =>
                {
                    decimal decimalValue = (decimal)value;
                    var removedTrailingZeroes = decimalValue.ToString("G29");
                    if (decimalValue == Math.Truncate(decimalValue))
                    {
                        return removedTrailingZeroes + ".0";
                    }
                    return removedTrailingZeroes;
                }
            },
        };

        public static string ConvertToCsv(IEnumerable<dynamic> rows, Dictionary<Type, Func<object, string>>? customSerializers = null)
        {
            if (!rows.Any())
            {
                return string.Empty;
            }
            var csvBuilder = new StringBuilder();
            var properties = ((IDictionary<string, object>)rows.First()).Keys;
            csvBuilder.AppendLine(string.Join(",", properties.Select(SanitizeCsvValue).Select(x => x.Camelize())));
            foreach (var row in rows)
            {
                var rowDict = (IDictionary<string, object>)row;
                var values = new List<string>(properties.Count);
                foreach (var prop in properties)
                {
                    if (!rowDict.TryGetValue(prop, out var value))
                    {
                        values.Add(string.Empty);
                        continue;
                    }
                    string stringValue;
                    if (value != null && customSerializers?.TryGetValue(value.GetType(), out var serializer) == true)
                    {
                        stringValue = serializer(value);
                    }
                    else
                    {
                        stringValue = value?.ToString() ?? string.Empty;
                    }
                    values.Add(SanitizeCsvValue(stringValue));
                }
                csvBuilder.AppendLine(string.Join(",", values));
            }
            return csvBuilder.ToString();
        }

        public static string ConvertToCsv(IEnumerable<dynamic> rows)
        {

            if (!rows.Any())
            {
                return string.Empty;
            }

            return CsvSerializer.SerializeToString(rows);
        }

        private static string SanitizeCsvValue(string value)
        {
            if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            {
                // Escape double quotes and wrap in double quotes.
                value = $"\"{value.Replace("\"", "\"\"")}\"";
            }
            return value;
        }
    }
}
