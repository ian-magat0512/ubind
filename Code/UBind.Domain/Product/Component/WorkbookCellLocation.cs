// <copyright file="WorkbookCellLocation.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Product.Component
{
    /// <summary>
    /// Reperesents the location of a cell in a spreadsheet workkbook.
    /// </summary>
    public struct WorkbookCellLocation
    {
        /// <summary>
        /// Gets or sets the sheet index.
        /// </summary>
        public int SheetIndex { get; set; }

        /// <summary>
        /// Gets or sets the row Index.
        /// </summary>
        public int RowIndex { get; set; }

        /// <summary>
        /// Gets or sets the column Index.
        /// </summary>
        public int ColIndex { get; set; }
    }
}
