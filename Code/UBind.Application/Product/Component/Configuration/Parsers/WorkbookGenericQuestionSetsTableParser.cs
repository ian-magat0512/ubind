// <copyright file="WorkbookGenericQuestionSetsTableParser.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Product.Component.Configuration.Parsers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Newtonsoft.Json;
    using UBind.Application.FlexCel;
    using UBind.Domain;
    using UBind.Domain.Attributes;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Product.Component;
    using UBind.Domain.Product.Component.Form;

    /// <summary>
    /// Reads and parses the question sets table from a standard uBind workbook.
    /// </summary>
    /// <typeparam name="TQuestionSet">The type of QuestionSet, either QuestionSet or RepeatingQuestionSet.</typeparam>
    public class WorkbookGenericQuestionSetsTableParser<TQuestionSet> : WorkbookTableParser
        where TQuestionSet : QuestionSet, new()
    {
        private IWorkbookFieldFactory fieldFactory;
        private IAttributeObjectPropertyMapRegistry<WorkbookTableColumnNameAttribute> registry;
        private IEnumerable<OptionSet> optionSets;
        private Dictionary<Type, Dictionary<string, PropertyInfo>> columnToPropertyMapByType
            = new Dictionary<Type, Dictionary<string, PropertyInfo>>();

        /// <summary>
        /// The name of the first column where values will be injected into when performing a calculation using the
        /// workbook.
        /// </summary>
        private string firstValueColumnName;

        /// <summary>
        /// The index of the column with the name firstValueColumnName.
        /// </summary>
        private int? firstValueColumnIndex;

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkbookGenericQuestionSetsTableParser{TQuestionSet}"/> class.
        /// </summary>
        /// <param name="workbook">The workbook.</param>
        /// <param name="fieldFactory">The workbook field factory for creating fields that match the specified field
        /// type in the workbook.</param>
        /// <param name="optionSets">The option sets, so that options fields can be properly populated.</param>
        /// <param name="registry">The registry of maps from table names to object properties.</param>
        /// <param name="tableName">The table name which holds the question sets.
        /// Defaults to "Table_Question_Sets".</param>
        /// <param name="firstValueColumnName">The name of the first column where values will be injected into
        /// when performing a calculation using the workbook.</param>
        public WorkbookGenericQuestionSetsTableParser(
            FlexCelWorkbook workbook,
            IWorkbookFieldFactory fieldFactory,
            IEnumerable<OptionSet> optionSets,
            IAttributeObjectPropertyMapRegistry<WorkbookTableColumnNameAttribute> registry,
            string tableName,
            string firstValueColumnName)
            : base(workbook, tableName)
        {
            this.fieldFactory = fieldFactory;
            this.optionSets = optionSets;
            this.registry = registry;
            this.firstValueColumnName = firstValueColumnName;
        }

        /// <summary>
        /// Gets the index of the column named "Value".
        /// </summary>
        protected int FirstValueColumn
        {
            get
            {
                if (this.firstValueColumnIndex == null)
                {
                    var valueColumns = this.ColumnIndexes.Where(c => c.Value == this.firstValueColumnName);
                    if (!valueColumns.Any())
                    {
                        throw new ErrorException(Errors.Product.WorkbookParseFailure(
                            $"Could not find the \"{this.firstValueColumnName}\" column in the table {this.TableName}"));
                    }

                    this.firstValueColumnIndex = valueColumns.First().Key;
                }

                return (int)this.firstValueColumnIndex;
            }
        }

        /// <summary>
        /// Parses the question sets table from a standuard UBind workbook.
        /// </summary>
        /// <returns>A list of OptionSets.</returns>
        public List<TQuestionSet> Parse()
        {
            return this.ParseGroupedByHeaderWithItemFactory<TQuestionSet, Field>(
                this.CreateQuestionSet,
                "Field Type",
                this.fieldFactory.Create,
                (questionSet, field) =>
                {
                    questionSet.Fields.Add(field);
                    field.QuestionSet = questionSet;
                },
                this.PopulateProperty,
                this.ItemFinished);
        }

        /// <summary>
        /// Does post processing on a single field once all properties have been populated.
        /// </summary>
        /// <param name="field">The field which was populated.</param>
        /// <param name="rowIndex">The row of the field in the table.</param>
        protected virtual void ItemFinished(Field field, int rowIndex)
        {
            this.PopulateCalculationWorkbookCellLocation(field, rowIndex);
        }

        private TQuestionSet CreateQuestionSet(string name, int rowIndex)
        {
            string key = this.GetKeyValue(rowIndex);
            var questionSet = new TQuestionSet { Name = name, Key = key };
            return questionSet;
        }

        private void PopulateProperty(string columnName, Field field, int rowIndex, int colIndex)
        {
            // Process tags manually
            if (columnName == "Tags")
            {
                string tagsValue = this.GetCellValueOrDefault<string>(rowIndex, colIndex);
                if (!string.IsNullOrEmpty(tagsValue))
                {
                    // TODO: handle parsing tags when they are quoted.
                    /*bool tagsAreQuoted = tagsValue.Contains("\"");*/
                    field.Tags = new HashSet<string>(tagsValue.Split(' '));
                }

                return;
            }

            // automatic field mappings:
            var columnToPropertyMap = this.GetColumnToPropertyMap(field.GetType());
            try
            {
                this.PopulateProperty(columnToPropertyMap, columnName, field, rowIndex, colIndex);
            }
            catch (ErrorException ex)
            {
                if (!string.IsNullOrEmpty(field.Name))
                {
                    ex.Error.AdditionalDetails.Add($"Field name: {field.Name}");
                }

                throw ex;
            }

            // manual field mappings:
            if (field is ToggleField toggleField)
            {
                this.ProcessToggleFieldProperties(toggleField, columnName, rowIndex, colIndex);
            }

            if (field is VisibleField visibleField)
            {
                this.ProcessVisibleFieldProperties(visibleField, columnName, rowIndex, colIndex);
            }

            if (field is OptionsField optionsField)
            {
                this.ProcessOptionsFieldProperties(optionsField, columnName, rowIndex, colIndex);
            }

            if (columnName == "Custom Properties")
            {
                this.PopulateJsonProperties(field, rowIndex, colIndex);
            }
        }

        private void ProcessToggleFieldProperties(ToggleField field, string columnName, int rowIndex, int colIndex)
        {
            if (columnName == "Icon")
            {
                if (!string.IsNullOrEmpty(field.Icon))
                {
                    field.Icon = WorkbookParserHelper.AddIconSetClasses(field.Icon);
                }
            }
        }

        private void ProcessVisibleFieldProperties(VisibleField field, string columnName, int rowIndex, int colIndex)
        {
            switch (columnName)
            {
                case "HTML":
                    if (!string.IsNullOrEmpty(field.Html))
                    {
                        // Replace slash quote with just a quote because in html fields in the workbook, that's what people have been entering.
                        field.Html = WorkbookParserHelper.ReplaceSlashQuoteWithQuoteInHtml(field.Html);
                    }

                    break;
                case "Terms":
                    if (!string.IsNullOrEmpty(field.HtmlTermsAndConditions))
                    {
                        field.HtmlTermsAndConditions = WorkbookParserHelper.ReplaceSlashQuoteWithQuoteInHtml(field.HtmlTermsAndConditions);
                    }

                    break;
                case "Paragraph":
                    if (!string.IsNullOrEmpty(field.Paragraph))
                    {
                        field.Paragraph = WorkbookParserHelper.ReplaceSlashQuoteWithQuoteInHtml(field.Paragraph);
                    }

                    break;
                case "Summary Label Expression":
                    // backwards compatibility for Summary Label Expression (deprecated)
                    if (string.IsNullOrEmpty(field.SummaryLabel))
                    {
                        string summaryLabelExpression = this.GetCellValueOrDefault<string>(rowIndex, colIndex);
                        if (!string.IsNullOrEmpty(summaryLabelExpression))
                        {
                            field.SummaryLabel = "%{ " + summaryLabelExpression + " }%";
                        }
                    }

                    break;
            }
        }

        private void ProcessOptionsFieldProperties(OptionsField field, string columnName, int rowIndex, int colIndex)
        {
            if (columnName == "Option Set")
            {
                string optionSetName = this.GetCellValueOrDefault<string>(rowIndex, colIndex);
                if (!string.IsNullOrEmpty(optionSetName))
                {
                    var optionSet = this.optionSets.Where(os => os.Name == optionSetName).FirstOrDefault();
                    if (optionSet == null)
                    {
                        throw new ErrorException(
                            Errors.Product.WorkbookParseFailure($"When parsing the field \"{field.Name}\", we could not "
                                + $"find the option set \"{optionSetName}\"."));
                    }

                    field.OptionSet = optionSet;
                    field.OptionSetKey = optionSet.Key;
                }
            }
        }

        private void PopulateJsonProperties(Field field, int rowIndex, int colIndex)
        {
            string json = this.GetCellValueOrDefault<string>(rowIndex, colIndex);
            if (json != null)
            {
                JsonConvert.PopulateObject(json, field);
            }
        }

        private Dictionary<string, PropertyInfo> GetColumnToPropertyMap(Type type)
        {
            Dictionary<string, PropertyInfo> map = null;
            if (!this.columnToPropertyMapByType.TryGetValue(type, out map))
            {
                map = this.registry.GetAttributeToPropertyMap(type)
                    .ToDictionary(kvp => kvp.Key.ColumnName, kvp => kvp.Value);
                this.columnToPropertyMapByType.Add(type, map);
            }

            return map;
        }

        private void PopulateCalculationWorkbookCellLocation(Field field, int rowIndex)
        {
            if (field is IDataStoringField dataStoringField)
            {
                dataStoringField.CalculationWorkbookCellLocation = new WorkbookCellLocation
                {
                    SheetIndex = this.SheetIndex,
                    RowIndex = rowIndex,
                    ColIndex = this.FirstValueColumn,
                };
            }

            if (field is RepeatingField repeatingField)
            {
                repeatingField.CalculationWorkbookCellLocation = new WorkbookCellLocation
                {
                    SheetIndex = this.SheetIndex,
                    RowIndex = rowIndex,
                    ColIndex = this.FirstValueColumn,
                };
            }
        }
    }
}
