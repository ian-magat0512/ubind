// <copyright file="WorkbookRepeatingQuestionSetsTableParser.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Product.Component.Configuration.Parsers
{
    using System.Collections.Generic;
    using UBind.Application.FlexCel;
    using UBind.Domain.Attributes;
    using UBind.Domain.Product.Component;
    using UBind.Domain.Product.Component.Form;

    /// <summary>
    /// Reads and parses the repeating question sets table from a standard uBind workbook.
    /// </summary>
    public class WorkbookRepeatingQuestionSetsTableParser
        : WorkbookGenericQuestionSetsTableParser<RepeatingQuestionSet>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WorkbookRepeatingQuestionSetsTableParser"/> class.
        /// </summary>
        /// <param name="workbook">The workbook.</param>
        /// <param name="fieldFactory">The workbook field factory for creating fields that match the specified field
        /// type in the workbook.</param>
        /// <param name="optionSets">The option sets, so that options fields can be properly populated.</param>
        /// <param name="registry">The registry of maps from table names to object properties.</param>
        public WorkbookRepeatingQuestionSetsTableParser(
            FlexCelWorkbook workbook,
            IWorkbookFieldFactory fieldFactory,
            IEnumerable<OptionSet> optionSets,
            IAttributeObjectPropertyMapRegistry<WorkbookTableColumnNameAttribute> registry)
            : base(workbook, fieldFactory, optionSets, registry, "Table_Repeating_Question_Sets", "Value 1")
        {
        }
    }
}
