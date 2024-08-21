// <copyright file="WorkbookStylesTableParser.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Product.Component.Configuration.Parsers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using UBind.Application.FlexCel;
    using UBind.Domain;
    using UBind.Domain.Attributes;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Product.Component;
    using UBind.Domain.Product.Component.Form;

    /// <summary>
    /// Parses the styles table in the standard uBind workbook to generate css.
    /// </summary>
    public class WorkbookStylesTableParser : WorkbookTableParser
    {
        private Dictionary<string, PropertyInfo> columnToPropertyMap;
        private int? selectorColumnIndex;

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkbookStylesTableParser"/> class.
        /// </summary>
        /// <param name="workbook">The workbook.</param>
        /// <param name="registry">The registry of maps from table column names to object properties.</param>
        public WorkbookStylesTableParser(
            FlexCelWorkbook workbook,
            IAttributeObjectPropertyMapRegistry<WorkbookTableColumnNameAttribute> registry)
            : base(workbook, "Table_Styles")
        {
            this.columnToPropertyMap = registry.GetAttributeToPropertyMap(typeof(Style))
                .ToDictionary(kvp => kvp.Key.ColumnName, kvp => kvp.Value);
        }

        /// <summary>
        /// Gets the index of the column named "Selector".
        /// </summary>
        private int SelectorColumn
        {
            get
            {
                if (this.selectorColumnIndex == null)
                {
                    var selectorColumns = this.ColumnIndexes.Where(c => c.Value == "Selector");
                    if (!selectorColumns.Any())
                    {
                        throw new ErrorException(Errors.Product.WorkbookParseFailure(
                            $"Could not find the header column in the table {this.TableName}"));
                    }

                    this.selectorColumnIndex = selectorColumns.First().Key;
                }

                return (int)this.selectorColumnIndex;
            }
        }

        /// <summary>
        /// Parses and populates the component with styles.
        /// </summary>
        /// <param name="component">The component to populate.</param>
        public void Parse(Component component)
        {
            if (component.Form.Theme == null)
            {
                component.Form.Theme = new Theme();
            }

            component.Form.Theme.Styles = this.ParseForStyles();
        }

        /// <summary>
        /// Parse the styles from the workbook.
        /// </summary>
        /// <returns>A list of styles.</returns>
        public List<Style> ParseForStyles()
        {
            return this.ParseTypedByHeader((type) => new Style { Category = type }, this.PopulateProperty);
        }

        /// <summary>
        /// determines whether the row at the specified index can be considered to be a header row.
        /// </summary>
        /// <param name="rowIndex">The index of the row.</param>
        /// <returns>True if this could be considered a header row, otherwise false.</returns>
        protected override bool IsHeaderRow(int rowIndex)
        {
            return string.IsNullOrEmpty(this.GetCellValueOrDefault<string>(rowIndex, this.SelectorColumn));
        }

        private void PopulateProperty(
            string columnName,
            Style style,
            int rowIndex,
            int colIndex)
        {
            this.PopulateProperty(this.columnToPropertyMap, columnName, style, rowIndex, colIndex);
            if (columnName == "Custom" && !string.IsNullOrEmpty(style.CustomCss))
            {
                // This is needed because the old workbooks have their backslashes escaped for things
                // like the folowing: content: '\\e803';
                // It should actually just be: content: '\e803';
                // So we are converting the double backslashes into a single.
                // At some point we should go and edit all workbooks to fix this, then we can remove this code.
                style.CustomCss = style.CustomCss.Replace("\\\\", "\\");
            }
        }
    }
}
