// <copyright file="DataTableDataTypeExtensions.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Extensions;

using UBind.Domain.Enums;
using UBind.Domain.Models.DataTable;

public static class DataTableDataTypeExtensions
{
    private static IDataTableDataTypeSqlSettingsRegistry dataTypeRegistry;

    public static void SetDataTypeSqlSettingsRegistry(IDataTableDataTypeSqlSettingsRegistry registry)
    {
        DataTableDataTypeExtensions.dataTypeRegistry = registry;
    }

    public static DataTypeSqlProperties? GetDataTypeSqlSettings(this DataTableDataType dataTableDataType)
    {
        return dataTypeRegistry.GetDataTableDataTypeProperties(dataTableDataType);
    }
}
