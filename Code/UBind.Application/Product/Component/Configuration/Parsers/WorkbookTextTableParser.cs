// <copyright file="WorkbookTextTableParser.cs" company="uBind">
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
    using UBind.Domain.Attributes;
    using UBind.Domain.Product.Component;
    using UBind.Domain.Product.Component.Form;

    /// <summary>
    /// Parses the text table in the standard ubind workbook.
    /// </summary>
    public class WorkbookTextTableParser : WorkbookTableParser
    {
        private Dictionary<string, PropertyInfo> columnToPropertyMap;

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkbookTextTableParser"/> class.
        /// </summary>
        /// <param name="workbook">The workbook.</param>
        /// <param name="registry">The registry of maps from table column names to object properties.</param>
        public WorkbookTextTableParser(
            FlexCelWorkbook workbook,
            IAttributeObjectPropertyMapRegistry<WorkbookTableColumnNameAttribute> registry)
            : base(workbook, "Table_Text")
        {
            this.columnToPropertyMap = registry.GetAttributeToPropertyMap(typeof(TextElement))
                .ToDictionary(kvp => kvp.Key.ColumnName, kvp => kvp.Value);
        }

        /// <summary>
        /// Parses the workbook for text elements and populates the component.
        /// </summary>
        /// <param name="component">The component to populate.</param>
        public void Parse(Component component)
        {
            component.Form.TextElements = this.ParseForTextElements();
        }

        /// <summary>
        /// Parses text elements from the workbook.
        /// </summary>
        /// <returns>A list of text elements.</returns>
        public List<TextElement> ParseForTextElements()
        {
            return this.ParseItemsByCategoryAndSubCategory(
                (category, subcategory) => new TextElement { Category = category, Subcategory = subcategory },
                (columnName, trigger, rowIndex, colIndex) =>
                    this.PopulateProperty(columnName, trigger, rowIndex, colIndex));
        }

        private void PopulateProperty(string columnName, TextElement textElement, int rowIndex, int colIndex)
        {
            this.PopulateProperty(this.columnToPropertyMap, columnName, textElement, rowIndex, colIndex);
            if (columnName == "Icon" && !string.IsNullOrEmpty(textElement.Icon))
            {
                textElement.Icon = WorkbookParserHelper.AddIconSetClasses(textElement.Icon);
            }
        }
    }
}
