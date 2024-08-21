// <copyright file="IDataTableDataTypeSqlSettingsRegistry.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.Models.DataTable;

using UBind.Domain.Enums;

public interface IDataTableDataTypeSqlSettingsRegistry
{
    DataTypeSqlProperties GetDataTableDataTypeProperties(DataTableDataType dataTableDataType);
}
