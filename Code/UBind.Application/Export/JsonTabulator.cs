// <copyright file="JsonTabulator.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Export
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using MoreLinq;
    using Newtonsoft.Json.Linq;
    using UBind.Domain.Extensions;

    /// <summary>
    /// For writing JSON as a text table, e.g. CSV or Tab delimited.
    /// </summary>
    public static class JsonTabulator
    {
        /// <summary>
        /// Write a collection of objects as a text table.
        /// </summary>
        /// <param name="objects">The objects.</param>
        /// <param name="delimiter">The delimiter to use (defaults to ",").</param>
        /// <param name="decorator">A decorator, which by default quotes each string with double quotes (e.g for CSV).</param>
        /// <typeparam name="T">The type of the object, where T is not a JObject.</typeparam>
        /// <returns>A string containing the data in the specified format.</returns>
        public static string Tabulate<T>(
            IEnumerable<T> objects,
            string delimiter = ",",
            Func<string, string> decorator = null)
        {
            if (decorator == null)
            {
                decorator = (val) => val;
            }

            var jObjects = objects.Select(o => JObject.FromObject(o));
            var flattenedRows = jObjects.Select(r => r.FlattenToDictionary());
            var columns = flattenedRows
                .SelectMany(r => r.Select(kvp => kvp.Key))
                .Distinct()
                .OrderBy(c => c).ToList();
            var builder = new StringBuilder();
            builder.AppendLine(
                string.Join(delimiter, columns.Select(decorator)));
            foreach (var row in flattenedRows)
            {
                for (int index = 0; index < columns.Count; index++)
                {
                    var value = string.Empty;
                    row.TryGetValue(columns[index], out value);
                    builder.Append(!string.IsNullOrEmpty(value) ? decorator(value) : value);

                    if (index < columns.Count - 1)
                    {
                        builder.Append(delimiter);
                    }
                }

                builder.AppendLine();
            }

            return builder.ToString();
        }

        /// <summary>
        /// Write a collection of Json objects as a CSV table.
        /// </summary>
        /// <param name="objects">The objects.</param>
        /// <typeparam name="T">The type of the object, where T is not a JObject.</typeparam>
        /// <returns>A string containing the data in CSV format.</returns>
        public static string TabulateCsv<T>(IEnumerable<T> objects)
        {
            return Tabulate(objects, ",", StringExtensions.FormatForCsv);
        }

        /// <summary>
        /// Write a collection of Json arrays as a CSV table.
        /// </summary>
        /// <param name="jArrayList">The JArray objects.</param>
        /// <returns>A string containing the data in CSV format.</returns>
        public static string TabulateTabDelimited(IEnumerable<JArray> jArrayList)
        {
            string delimiter = "\t";
            var builder = new StringBuilder();
            jArrayList.ForEach(jArray =>
                builder.AppendLine(string.Join(delimiter, jArray.Select(item => item.ToString()))));
            return builder.ToString();
        }
    }
}
