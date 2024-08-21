// <copyright file="WorkbookQuestionSetsTableParser.cs" company="uBind">
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
    /// Reads and parses the question sets table from a standard uBind workbook.
    /// </summary>
    public class WorkbookQuestionSetsTableParser : WorkbookGenericQuestionSetsTableParser<QuestionSet>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WorkbookQuestionSetsTableParser"/> class.
        /// </summary>
        /// <param name="workbook">The workbook.</param>
        /// <param name="fieldFactory">The workbook field factory for creating fields that match the specified field
        /// type in the workbook.</param>
        /// <param name="optionSets">The option sets, so that options fields can be properly populated.</param>
        /// <param name="registry">The registry of maps from table names to object properties.</param>
        public WorkbookQuestionSetsTableParser(
            FlexCelWorkbook workbook,
            IWorkbookFieldFactory fieldFactory,
            IEnumerable<OptionSet> optionSets,
            IAttributeObjectPropertyMapRegistry<WorkbookTableColumnNameAttribute> registry)
            : base(workbook, fieldFactory, optionSets, registry, "Table_Question_Sets", "Value")
        {
        }

        /// <summary>
        /// Does post processing on a single field once all properties have been populated.
        /// </summary>
        /// <param name="field">The field which was populated.</param>
        /// <param name="rowIndex">The row of the field in the table.</param>
        protected override void ItemFinished(Field field, int rowIndex)
        {
            base.ItemFinished(field, rowIndex);
            if (field is RepeatingField repeatingField)
            {
                this.DetermineRepeatingInstanceHeadingType(repeatingField);
                this.DetermineRepeatingInstanceName(repeatingField);
            }
        }

        private void DetermineRepeatingInstanceName(RepeatingField field)
        {
            bool isOnlyHeading3Used = string.IsNullOrEmpty(field.Heading2)
                && !string.IsNullOrEmpty(field.Heading3)
                && string.IsNullOrEmpty(field.Heading4);
            bool isOnlyHeading4Used = string.IsNullOrEmpty(field.Heading2)
                && string.IsNullOrEmpty(field.Heading3)
                && !string.IsNullOrEmpty(field.Heading4);
            if (isOnlyHeading3Used)
            {
                field.RepeatingInstanceName = field.Heading3;
                field.Heading3 = null;
            }
            else if (isOnlyHeading4Used)
            {
                field.RepeatingInstanceName = field.Heading4;
                field.Heading4 = null;
            }
            else if (!string.IsNullOrEmpty(field.Heading2)
                && !string.IsNullOrEmpty(field.Heading3))
            {
                field.RepeatingInstanceName = field.Heading3;
                field.Heading3 = null;
            }
            else if (!string.IsNullOrEmpty(field.Heading4))
            {
                field.RepeatingInstanceName = field.Heading4;
                field.Heading4 = null;
            }
            else
            {
                if (field.Name.EndsWith("s"))
                {
                    int length = field.Name.Length;
                    field.RepeatingInstanceName = field.Name.Substring(0, length - 1);
                }
                else
                {
                    field.RepeatingInstanceName = field.Name;
                }
            }
        }

        private void DetermineRepeatingInstanceHeadingType(RepeatingField field)
        {
            field.RepeatingInstanceHeadingLevel = !string.IsNullOrEmpty(field.Heading4) ? 4 : 3;
        }
    }
}
