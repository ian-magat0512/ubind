// <copyright file="DataTypeSqlProperties.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.Models.DataTable;

public class DataTypeSqlProperties
{
    public DataTypeSqlProperties(string sqlDataType, int? maxLength = null, int? decimalsLength = null, Type? dataTableDataType = null)
    {
        this.SqlDataType = sqlDataType;
        this.MaxLength = maxLength;
        this.DecimalsLength = decimalsLength;
        this.DataTableDataType = dataTableDataType ?? typeof(string);
        this.SqlColumnSize = this.GetSqlColumnSize();
    }

    public string SqlDataType { get; }

    public int? MaxLength { get; }

    public int? DecimalsLength { get; }

    public Type DataTableDataType { get; }

    public string SqlColumnSize { get; }

    public string SqlDataTypeAndSize => this.SqlDataType + this.SqlColumnSize;

    public string GetSqlDefaultValue(object defaultValue)
    {
        var stringValue = defaultValue.ToString();
        if (string.IsNullOrEmpty(stringValue))
        {
            return string.Empty;
        }

        switch (this.SqlDataType)
        {
            case "bigint":
            case "decimal":
                return stringValue;
            case "bit":
                return Convert.ToBoolean(stringValue) ? "1" : "0";
            default:
                return $"'{stringValue}'";
        }
    }

    private string GetSqlColumnSize()
    {
        var typesWithColumnSize = new List<string> { "nvarchar", "varchar", "decimal" };
        if (!typesWithColumnSize.Contains(this.SqlDataType))
        {
            return string.Empty;
        }

        string maxValue = this.MaxLength == int.MaxValue
            || this.MaxLength >= 8000
            ? "max" : $"{this.MaxLength}";

        if (this.DecimalsLength == -1 && this.MaxLength == -1)
        {
            return string.Empty;
        }

        if (this.DecimalsLength != -1 && this.MaxLength != -1)
        {
            return $"({maxValue},{this.DecimalsLength})";
        }

        if (this.MaxLength != -1)
        {
            return $"({maxValue})";
        }
        return string.Empty;
    }
}