// <copyright file="WorkbookTriggersTableParser.cs" company="uBind">
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

    /// <summary>
    /// Reads and parses the triggers table from a standard uBind workbook.
    /// </summary>
    public class WorkbookTriggersTableParser : WorkbookTableParser
    {
        private Dictionary<string, PropertyInfo> columnToPropertyMap;

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkbookTriggersTableParser"/> class.
        /// </summary>
        /// <param name="workbook">The workbook.</param>
        /// <param name="registry">The registry of maps from table column names to object properties.</param>
        public WorkbookTriggersTableParser(
            FlexCelWorkbook workbook,
            IAttributeObjectPropertyMapRegistry<WorkbookTableColumnNameAttribute> registry)
            : base(workbook, "Table_Triggers")
        {
            this.columnToPropertyMap = registry.GetAttributeToPropertyMap(typeof(Trigger))
                .ToDictionary(kvp => kvp.Key.ColumnName, kvp => kvp.Value);
        }

        /// <summary>
        /// Parses the workbook and populates the component with the triggers from it.
        /// </summary>
        /// <param name="component">The component instance to populate.</param>
        public void Parse(Component component)
        {
            component.Triggers = this.ParseForTriggers();
        }

        /// <summary>
        /// Parse the triggers table from the workbook.
        /// </summary>
        /// <returns>A list of triggers.</returns>
        public List<Trigger> ParseForTriggers()
        {
            return this.ParseTypedByHeader(
                (type) => new Trigger { TypeName = type },
                (columnName, trigger, rowIndex, colIndex) =>
                    this.PopulateProperty(this.columnToPropertyMap, columnName, trigger, rowIndex, colIndex));
        }
    }
}
