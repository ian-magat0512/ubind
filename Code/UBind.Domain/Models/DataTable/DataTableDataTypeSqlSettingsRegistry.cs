// <copyright file="DataTableDataTypeSqlSettingsRegistry.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Models.DataTable;

using System.Reflection;
using UBind.Domain.Attributes;
using UBind.Domain.Enums;
using UBind.Domain.Extensions;

public class DataTableDataTypeSqlSettingsRegistry : IDataTableDataTypeSqlSettingsRegistry
{
    private Dictionary<DataTableDataType, DataTypeSqlProperties?>? dataTableDataTypesMap;
    private object map = new object();

    /// <summary>
    /// Initializes a new instance of the <see cref="DataTableDataTypeSqlSettingsRegistry"/> class.
    /// </summary>
    public DataTableDataTypeSqlSettingsRegistry()
    {
        DataTableDataTypeExtensions.SetDataTypeSqlSettingsRegistry(this);
    }

    public DataTypeSqlProperties? GetDataTableDataTypeProperties(DataTableDataType dataType)
    {
        lock (this.map)
        {
            if (this.dataTableDataTypesMap == null)
            {
                this.PopulateDataTableDataTypes();
            }
        }
        this.dataTableDataTypesMap.TryGetValue(dataType, out DataTypeSqlProperties? properties);
        return properties;
    }

    private void PopulateDataTableDataTypes()
    {
        this.dataTableDataTypesMap = new Dictionary<DataTableDataType, DataTypeSqlProperties?>();
        IEnumerable<FieldInfo> fieldInfos = typeof(DataTableDataType).GetFields();
        foreach (var fieldInfo in fieldInfos)
        {
            var attribute = fieldInfo
                .GetCustomAttributes(typeof(DataTypeSqlSettingsAttribute), false)
                .FirstOrDefault() as DataTypeSqlSettingsAttribute;
            if (Enum.TryParse<DataTableDataType>(fieldInfo.Name, true, out var dataTableDataType)
                && attribute != null)
            {
                this.AddDataTypeSqlPropertiesToMap(dataTableDataType, new DataTypeSqlProperties(
                    sqlDataType: attribute.SqlDataType,
                    maxLength: attribute.MaxLength,
                    decimalsLength: attribute.DecimalsLength,
                    dataTableDataType: attribute.DataTableDataType));
            }
        }
    }

    private void AddDataTypeSqlPropertiesToMap(DataTableDataType dataTableDataType, DataTypeSqlProperties? properties)
    {
        if (!this.dataTableDataTypesMap.ContainsKey(dataTableDataType))
        {
            this.dataTableDataTypesMap.Add(dataTableDataType, properties);
        }
    }
}
