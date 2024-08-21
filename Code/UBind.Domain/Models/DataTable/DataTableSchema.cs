// <copyright file="DataTableSchema.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Models.DataTable;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

public class DataTableSchema
{
    [JsonProperty("columns")]
    public IEnumerable<DataTableModel.Column> Columns { get; set; }

    [JsonProperty("clusteredIndex")]
    public DataTableModel.ClusteredIndex? ClusteredIndex { get; set; }

    [JsonProperty("unclusteredIndexes")]
    public IEnumerable<DataTableModel.UnclusteredIndex>? UnclusteredIndexes { get; set; }

    [JsonIgnore]
    public IEnumerable<string> ColumnNames => this.Columns.Select(x => x.Name);

    [JsonIgnore]
    public IEnumerable<string> PascalizedColumnAliases => this.Columns.Select(x => x.PascalizedAlias);

    public override string ToString()
    {
        return JsonConvert.SerializeObject(this, new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.Indented,
        });
    }
}