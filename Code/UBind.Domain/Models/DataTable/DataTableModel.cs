// <copyright file="DataTableModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Models.DataTable;

using Humanizer;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using UBind.Domain.Enums;
using UBind.Domain.Extensions;

public enum KeyColumnSortOrder
{
    Asc,
    Desc,
}

public class DataTableModel
{
    public class ClusteredIndex
    {
        public string Name { get; set; }

        public string Alias { get; set; }

        [JsonIgnore]
        public virtual string SqlIndexName => $"CI_{this.Alias.HyphenToUnderscore().Pascalize()}";

        public IEnumerable<KeyColumn> KeyColumns { get; set; }
    }

    public class Column
    {
        public string Name { get; set; } = string.Empty;

        public string Alias { get; set; } = string.Empty;

        [JsonIgnore]
        public string PascalizedAlias => GetNormalizedColumnAlias(this.Alias);

        [JsonConverter(typeof(StringEnumConverter), converterParameters: typeof(CamelCaseNamingStrategy))]
        public DataTableDataType DataType { get; set; }

        public string? DefaultValue { get; set; }

        public bool? Required { get; set; }

        public bool? Unique { get; set; }

        public static string GetNormalizedColumnAlias(string value)
        {
            return value.Trim().HyphenToUnderscore().Pascalize();
        }
    }

    public class UnclusteredIndex : ClusteredIndex
    {
        [JsonIgnore]
        public override string SqlIndexName => $"IX_{this.Alias.HyphenToUnderscore().Pascalize()}";

        public IEnumerable<string>? NonKeyColumns { get; set; }

        [JsonIgnore]
        public IEnumerable<string>? NormalizedNonKeyColumns =>
            this.NonKeyColumns != null
            ? this.NonKeyColumns.Select(alias => Column.GetNormalizedColumnAlias(alias)).ToList()
            : null;
    }

    public class KeyColumn
    {
        public string ColumnAlias { get; set; } = string.Empty;

        [JsonIgnore]
        public string NormalizedColumnAlias => Column.GetNormalizedColumnAlias(this.ColumnAlias);

        [JsonConverter(typeof(StringEnumConverter), converterParameters: typeof(CamelCaseNamingStrategy))]
        public KeyColumnSortOrder? SortOrder { get; set; }
    }
}
