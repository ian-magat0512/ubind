// <copyright file="DataTypeSqlSettingsAttribute.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Attributes;

/// <summary>
/// Data Type SQL Settings.
/// Stores details of a data types settings for SQL
/// </summary>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class DataTypeSqlSettingsAttribute : Attribute
{

    /// <summary>
    /// The SQL Data type to use for generating a data table.
    /// </summary>
    public string SqlDataType { get; set; }

    /// <summary>
    /// Maximum character length of the data if applicable. Use int.MaxValue for 'max' value.
    /// </summary>
    public int MaxLength { get; set; } = -1;

    /// <summary>
    /// The decimal number of digits allowed for the data type.
    /// </summary>
    public int DecimalsLength { get; set; } = -1;

    /// <summary>
    /// Type declaration to use for data type
    /// </summary>
    public Type DataTableDataType { get; set; }

    public string SqlColumnSize
    {
        get
        {
            if (this.DecimalsLength == null && this.MaxLength == null)
            {
                return string.Empty;
            }

            if (this.DecimalsLength != null && this.MaxLength != null)
            {
                return $"({this.MaxLength},{this.DecimalsLength})";
            }

            if (this.MaxLength != null)
            {
                return $"({this.MaxLength})";
            }
            return string.Empty;
        }
    }
}
