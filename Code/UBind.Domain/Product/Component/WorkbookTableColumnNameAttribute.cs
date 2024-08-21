// <copyright file="WorkbookTableColumnNameAttribute.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Product.Component
{
    using System;

    /// <summary>
    /// Allows us to specify which workbook table columns map to fields for the purpose of
    /// reading a spreadsheet table into C# objects.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class WorkbookTableColumnNameAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WorkbookTableColumnNameAttribute"/> class.
        /// </summary>
        /// <param name="columnName">The name of the column in the workbook.</param>
        public WorkbookTableColumnNameAttribute(string columnName)
        {
            this.ColumnName = columnName;
        }

        /// <summary>
        /// Gets the name of the column in the workbook.
        /// </summary>
        public string ColumnName { get; private set; }
    }
}
