// <copyright file="WorkbookOptionsTableParser.cs" company="uBind">
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
    /// Reads and parses the options table from a standard uBind workbook.
    /// </summary>
    public class WorkbookOptionsTableParser : WorkbookTableParser
    {
        private Dictionary<string, PropertyInfo> columnToPropertyMap;

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkbookOptionsTableParser"/> class.
        /// </summary>
        /// <param name="workbook">The workbook.</param>
        /// <param name="registry">The registry of maps from table names to object properties.</param>
        public WorkbookOptionsTableParser(
            FlexCelWorkbook workbook,
            IAttributeObjectPropertyMapRegistry<WorkbookTableColumnNameAttribute> registry)
            : base(workbook, "Table_Question_Options")
        {
            this.columnToPropertyMap = registry.GetAttributeToPropertyMap(typeof(Option))
                .ToDictionary(kvp => kvp.Key.ColumnName, kvp => kvp.Value);
        }

        /// <summary>
        /// Parses the Options table from a standard UBind workbook.
        /// </summary>
        /// <returns>A list of OptionSets.</returns>
        public List<OptionSet> Parse()
        {
            return this.ParseGroupedByHeader<OptionSet, Option>(
                (headerValue, rowIndex) => new OptionSet { Name = headerValue },
                (optionSet, option) => optionSet.Options.Add(option),
                this.PopulateProperty);
        }

        private void PopulateProperty(string columnName, Option option, int rowIndex, int colIndex)
        {
            this.PopulateProperty(this.columnToPropertyMap, columnName, option, rowIndex, colIndex);
            switch (columnName)
            {
                case "Icon" when !string.IsNullOrEmpty(option.Icon):
                    option.Icon = WorkbookParserHelper.AddIconSetClasses(option.Icon);
                    break;
                case "Display" when !string.IsNullOrEmpty(option.Label):
                    // Replace slash quote with just a quote because in html fields in the workbook, that's what people have been entering.
                    option.Label = WorkbookParserHelper.ReplaceSlashQuoteWithQuoteInHtml(option.Label);
                    break;
            }
        }
    }
}
