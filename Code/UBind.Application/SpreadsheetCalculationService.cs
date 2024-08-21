// <copyright file="SpreadsheetCalculationService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using StackExchange.Profiling;
    using UBind.Application.Export;
    using UBind.Application.FlexCel;
    using UBind.Application.Releases;
    using UBind.Application.ResourcePool;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Product;
    using UBind.Domain.Product.Component.Form;

    /// <inheritdoc/>
    public class SpreadsheetCalculationService : ICalculationService
    {
        /// <summary>
        /// Gets or sets the workbook heading row index.
        /// </summary>
        public const int StandardWorkbookHeadingRowIndex = 4;

        private const string OutputWorksheetName = "Outputs";
        private const string QuestionSetsWorksheetName = "Question Sets";
        private const string RepeatingQuestionSetsWorksheetName = "Repeating Question Sets";

        private const string QuestionSetsTableName = "Table_Question_Sets";
        private const string QuestionSetsAnswerColumnName = "Value";

        private const string RepeatingQuestionSetsAnswerRange = "Repeating_Question_Sets_Values";

        private const string QuoteTableName = "Table_Quote";

        private readonly ISpreadsheetPoolService spreadsheetPoolService;
        private readonly IReleaseQueryService releaseQueryService;
        private readonly ILogger<SpreadsheetCalculationService> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SpreadsheetCalculationService"/> class.
        /// </summary>
        /// <param name="spreadsheetPoolService">A service which provides pools of spreadsheet instances.</param>
        /// <param name="releaseQueryService">Service for obtaining current release (including workbook data).</param>
        /// <param name="logger">The logger.</param>
        public SpreadsheetCalculationService(
            ISpreadsheetPoolService spreadsheetPoolService,
            IReleaseQueryService releaseQueryService,
            ILogger<SpreadsheetCalculationService> logger)
        {
            this.spreadsheetPoolService = spreadsheetPoolService;
            this.releaseQueryService = releaseQueryService;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public ReleaseCalculationOutput GetQuoteCalculation(
            ReleaseContext releaseContext,
            SpreadsheetCalculationDataModel spreadsheetCalculationDataModel,
            IAdditionalRatingFactors additionalRatingFactors = null)
        {
            using (MiniProfiler.Current.Step(nameof(SpreadsheetCalculationService) + "." + nameof(this.GetQuoteCalculation)))
            {
                FlexCelWorkbookPool pool = this.GetSpreadsheetPool(releaseContext, WebFormAppType.Quote) as FlexCelWorkbookPool;
                var instance = pool.AcquireResource();

                spreadsheetCalculationDataModel = this.PatchSpreadsheetCalculationDataModel(releaseContext, spreadsheetCalculationDataModel, WebFormAppType.Quote, instance as IFlexCelWorkbook);
                var questionAnswers = spreadsheetCalculationDataModel.QuestionAnswers;
                var repeatingQuestionAnswers = spreadsheetCalculationDataModel.RepeatingQuestionAnswers;
                try
                {
                    string calculationJson = this.PatchAndReadResult(
                        instance as IFlexCelWorkbook,
                        questionAnswers,
                        repeatingQuestionAnswers,
                        releaseContext.Environment,
                        additionalRatingFactors);

                    return new ReleaseCalculationOutput
                    {
                        CalculationJson = calculationJson,
                        ReleaseId = pool.ReleaseId,
                    };
                }
                finally
                {
                    pool.ReleaseResource(instance);
                }
            }
        }

        /// <inheritdoc/>
        public string GetClaimCalculation(ReleaseContext releaseContext, string formData)
        {
            using (MiniProfiler.Current.Step(nameof(SpreadsheetCalculationService) + "." + nameof(this.GetClaimCalculation)))
            {
                var pool = this.GetSpreadsheetPool(releaseContext, WebFormAppType.Claim);
                var instance = pool.AcquireResource();

                var spreadsheetCalculationDataModel = JsonConvert.DeserializeObject<SpreadsheetCalculationDataModel>(formData);
                spreadsheetCalculationDataModel = this.PatchSpreadsheetCalculationDataModel(releaseContext, spreadsheetCalculationDataModel, WebFormAppType.Claim, instance as IFlexCelWorkbook);
                var questionAnswers = spreadsheetCalculationDataModel.QuestionAnswers;
                var repeatingQuestionAnswers = spreadsheetCalculationDataModel.RepeatingQuestionAnswers;
                try
                {
                    var result = this.PatchAndReadResult(
                        instance as IFlexCelWorkbook,
                        questionAnswers,
                        repeatingQuestionAnswers,
                        releaseContext.Environment);

                    return this.RemoveTrailingCommas(result);
                }
                finally
                {
                    pool.ReleaseResource(instance);
                }
            }
        }

        /// <summary>
        /// Note: this is a temporary solution. We need to find a better way to remove trailing commas.
        /// The issue happens when the workbook causes an invalid json with trailing commas,
        /// which the backend uses to send to the frontend, and the frontend cannot parse it.
        /// Previously fixed passively when querying the aggregate, and since the invalid json was saved as a string,
        /// and now as you build the aggregate, the json string will now be converted to objects, which fixes the issue passively.
        /// But now it is showing because we do not query the aggregate, we build it once and when we update it with new calculations,
        /// we dont query it again, the the invalid json will not be fixed passively by because the json conversion is absent.
        /// </summary>
        /// <param name="json">The calculation result json string.</param>
        /// <returns>The calculation result but with no trailing commas.</returns>
        private string RemoveTrailingCommas(string json)
        {
            // Define a regular expression pattern to match trailing commas in objects and arrays
            string pattern = @"(,)(\s*[\]\}])";

            // Use Regex.Replace to remove trailing commas
            string result = Regex.Replace(json, pattern, "$2");

            return result;
        }

        private IResourcePool GetSpreadsheetPool(ReleaseContext releaseContext, WebFormAppType webFormAppType)
        {
            var release = this.releaseQueryService.GetRelease(releaseContext);
            var pool = this.spreadsheetPoolService.GetSpreadsheetPool(releaseContext, webFormAppType);
            if (pool == null)
            {
                // This should never happen, but somehow it has happened, where a release was cached but the
                // workbook pool wasn't created. We'll log this rare occcurance and create it now.
                this.logger.LogDebug(
                    "When trying to perform a calculation for the {webFormAppType} product component of "
                    + "{productContext}, the spreadsheet pool was found to not exist. By this stage, the "
                    + "release should have been retrieved, cached and the spreadsheet pool created.",
                    webFormAppType,
                    releaseContext);
                this.spreadsheetPoolService.CreateSpreadsheetPool(releaseContext, webFormAppType, release[webFormAppType].WorkbookData, release.ReleaseId);
                pool = this.spreadsheetPoolService.GetSpreadsheetPool(releaseContext, webFormAppType);
            }

            if (pool == null)
            {
                throw new InvalidOperationException(
                    $"When trying to perform a calculation for the {webFormAppType} product component of "
                    + $"{releaseContext}, the spreadsheet pool was found to not exist. By this stage, the "
                    + "release should have been retrieved, cached and the spreadsheet pool created. "
                    + "However it wasn't so we even tried to create the spreadsheet pool from the release data "
                    + "and then retreive it again, but it still was not found. ");
            }

            return pool;
        }

        private void HandleMissingWorkbookData(ProductContext productContext, WebFormAppType webFormAppType)
        {
            if (productContext.Environment == DeploymentEnvironment.Development)
            {
                throw new ProductConfigurationException(
                    Errors.Product.MisConfiguration(
                    $"The latest initialized dev release for {productContext.TenantId}/{productContext.ProductId} does not contain workbook data, " +
                    "and so cannot be used with the FlexCel calculation service. " +
                    "Please create and deploy a new release."));
            }

            throw new ProductConfigurationException(
                Errors.Product.MisConfiguration(
                    $"The currently deployed release for {productContext.TenantId}/{productContext.ProductId} in {productContext.Environment} does not contain workbook data, " +
                    "and so cannot be used with the FlexCel calculation service." +
                    "Please create and deploy a new release."));
        }

        private T DeepClone<T>(T obj)
        {
            var serialized = JsonConvert.SerializeObject(obj);
            return JsonConvert.DeserializeObject<T>(serialized);
        }

        private string PatchAndReadResult(
            IFlexCelWorkbook workbookInstance,
            JArray questionAnswers,
            JArray repeatingQuestionAnswers,
            DeploymentEnvironment environment,
            IAdditionalRatingFactors additionalRatingFactors = null)
        {
            using (MiniProfiler.Current.Step(nameof(SpreadsheetCalculationService) + "." + nameof(this.PatchAndReadResult) + "*"))
            {
                string result = "{}";
                using (MiniProfiler.Current.Step(nameof(FlexCelWorkbook)))
                {
                    using (MiniProfiler.Current.Step("Write Question Sets answers"))
                    {
                        workbookInstance.PatchData(
                            QuestionSetsWorksheetName,
                            QuestionSetsTableName,
                            QuestionSetsAnswerColumnName,
                            questionAnswers);
                    }

                    using (MiniProfiler.Current.Step("Write Repeating Question Sets answers"))
                    {
                        workbookInstance.PatchData(
                            RepeatingQuestionSetsWorksheetName,
                            RepeatingQuestionSetsAnswerRange,
                            repeatingQuestionAnswers);
                    }

                    using (MiniProfiler.Current.Step("Update Additional Ratings Factors"))
                    {
                        workbookInstance.UpdateAdditionalRatingFactors(additionalRatingFactors);
                    }

                    using (MiniProfiler.Current.Step("Read Calculation result"))
                    {
                        try
                        {
                            result = workbookInstance.ReadTableTextAsString(
                                OutputWorksheetName,
                                QuoteTableName);
                        }
                        catch (ErrorException ex)
                        {
                            // enrich the data with the questionAnswers and repeatingQuestionAnswers, for debugging
                            ex.Error.Data.Add("questionAnswersForPastingIntoWorkbook", JsonTabulator.TabulateTabDelimited(questionAnswers.ToObject<JArray[]>()));
                            ex.Error.Data.Add("repeatingQuestionAnswersForPastingIntoWorkbook", JsonTabulator.TabulateTabDelimited(repeatingQuestionAnswers.ToObject<JArray[]>()));
                            throw ex;
                        }
                    }
                }

                // we only do JSON validity checking in the development environment because it's slow
                if (environment == DeploymentEnvironment.Development)
                {
                    try
                    {
                        result.CheckJsonIsValid("Excel workbook calculation output");
                    }
                    catch (ErrorException ex)
                    {
                        // enrich the data with the questionAnswers and repeatingQuestionAnswers, for debugging
                        var questionAnswersForPastingIntoWorkbook = JsonTabulator.TabulateTabDelimited(questionAnswers.ToObject<JArray[]>());
                        var repeatingQuestionAnswersForPastingIntoWorkbook = JsonTabulator.TabulateTabDelimited(repeatingQuestionAnswers.ToObject<JArray[]>());
                        if (ex.Error.Code == "json.invalid.missing.property.value")
                        {
                            throw new ErrorException(Errors.Calculation.Spreadsheet.CalculationOutputJsonInvalidMissingPropertyValue(
                                ex.Error.Data["jsonPath"].ToString(),
                                ex.Error.Data["sourceJson"].ToString(),
                                questionAnswersForPastingIntoWorkbook,
                                repeatingQuestionAnswersForPastingIntoWorkbook));
                        }
                        else
                        {
                            ex.Error.Data.Add("questionAnswersForPastingIntoWorkbook", JsonTabulator.TabulateTabDelimited(questionAnswers.ToObject<JArray[]>()));
                            ex.Error.Data.Add("repeatingQuestionAnswersForPastingIntoWorkbook", JsonTabulator.TabulateTabDelimited(repeatingQuestionAnswers.ToObject<JArray[]>()));
                            throw ex;
                        }
                    }
                }

                return result;
            }
        }

        private SpreadsheetCalculationDataModel PatchSpreadsheetCalculationDataModel(
            ReleaseContext releaseContext,
            SpreadsheetCalculationDataModel spreadsheetCalculationDataModel,
            WebFormAppType webFormAppType,
            IFlexCelWorkbook instance)
        {
            ActiveDeployedRelease activeDeployedRelease =
                this.releaseQueryService.GetRelease(releaseContext);
            ProductComponentConfiguration productComponentConfig = activeDeployedRelease?[webFormAppType];
            JArray repeatingQuestionAnswerArray = this.GenerateRepeatingQuestionAnswerArray(
                spreadsheetCalculationDataModel.FormModel,
                productComponentConfig?.Component?.Form?.QuestionSets,
                productComponentConfig?.Component?.Form?.RepeatingQuestionSets,
                productComponentConfig?.Component?.Form?.RepeatingInstanceMaxQuantity,
                instance);
            JArray questionAnswerArray = this.GenerateQuestionAnswerArray(
                spreadsheetCalculationDataModel.FormModel,
                productComponentConfig?.Component?.Form?.QuestionSets);

            if (spreadsheetCalculationDataModel.QuestionAnswers.IsNullOrEmpty())
            {
                spreadsheetCalculationDataModel.QuestionAnswers = questionAnswerArray;
            }

            if (spreadsheetCalculationDataModel.RepeatingQuestionAnswers.IsNullOrEmpty())
            {
                spreadsheetCalculationDataModel.RepeatingQuestionAnswers = repeatingQuestionAnswerArray;
            }

            return spreadsheetCalculationDataModel;
        }

        private JArray GenerateQuestionAnswerArray(
            JObject formModel,
            List<QuestionSet> questionSets)
        {
            using (MiniProfiler.Current.Step(nameof(SpreadsheetCalculationService) + "." + nameof(this.GenerateQuestionAnswerArray)))
            {
                if (questionSets == null)
                {
                    return null;
                }

                List<Field> orderedFields = this.OrderFields(questionSets);
                var rowsCount = this.GetRowIndexOfLastFieldInQuestionSetsTable(orderedFields);
                var table = new object[rowsCount][];
                var row = new object[] { "Value" };
                table[0] = row;
                foreach (var field in orderedFields)
                {
                    if (field is IDataStoringField dataStoringField)
                    {
                        int fieldRowIndex = dataStoringField.CalculationWorkbookCellLocation.Value.RowIndex;
                        JToken fieldValue = formModel[field.Key];
                        object objFieldValue = fieldValue != null && fieldValue.Type == JTokenType.Array ? string.Empty : (fieldValue ?? string.Empty);
                        objFieldValue = objFieldValue != null && objFieldValue.ToString() != string.Empty ? objFieldValue : this.GetDefaultValue(field);
                        row = new object[] { objFieldValue };
                        table[fieldRowIndex - StandardWorkbookHeadingRowIndex] = row;
                    }

                    if (field is RepeatingField repeatingField)
                    {
                        if (!repeatingField.CalculationWorkbookCellLocation.HasValue)
                        {
                            continue;
                        }

                        int fieldRowIndex = repeatingField.CalculationWorkbookCellLocation.Value.RowIndex;
                        JToken repeatingQuestionSetFieldValue = formModel[field.Key];
                        var repeatingQuestionSetFields = repeatingField.QuestionSetToRepeat.Fields;
                        var repeatingQuestiongSetIsComplete =
                            this.RepeatingQuestionSetIsComplete(repeatingQuestionSetFieldValue, repeatingQuestionSetFields);
                        object objFieldValue =
                            repeatingQuestionSetFieldValue == null || (repeatingQuestionSetFieldValue != null && repeatingQuestiongSetIsComplete)
                                ? "complete" : "incomplete";
                        objFieldValue = objFieldValue != null && objFieldValue.ToString() != string.Empty ? objFieldValue : this.GetDefaultValue(field);
                        row = new object[] { objFieldValue };
                        table[fieldRowIndex - StandardWorkbookHeadingRowIndex] = row;
                    }
                }

                for (int i = 0; i < table.Length; i++)
                {
                    if (table[i] == null)
                    {
                        table[i] = new object[] { string.Empty };
                    }
                }

                return JArray.FromObject(table);
            }
        }

        private bool RepeatingQuestionSetIsComplete(
            JToken repeatingQuestionSetFieldValue,
            List<Field> repeatingQuestionSetFields)
        {
            bool repeatingFieldIsComplete = true;
            if (repeatingQuestionSetFieldValue?.Type != JTokenType.Array)
            {
                return repeatingFieldIsComplete;
            }

            foreach (var repeatingFieldArrayItem in JArray.FromObject(repeatingQuestionSetFieldValue))
            {
                if (repeatingFieldArrayItem.IsNullOrEmpty())
                {
                    continue;
                }

                foreach (var repeatingQuestionField in repeatingQuestionSetFields)
                {
                    if (repeatingQuestionField is InteractiveField interactiveField)
                    {
                        var isRequired = interactiveField.Required;
                        var repeatingFieldValue = repeatingFieldArrayItem[repeatingQuestionField.Key];
                        if (isRequired.HasValue && isRequired.Value && repeatingFieldValue.IsNullOrEmpty())
                        {
                            return false;
                        }
                    }
                }
            }

            // if there are no repeating instances then we'll consider it complete.
            return true;
        }

        private JArray GenerateRepeatingQuestionAnswerArray(
            JObject formModel,
            List<QuestionSet> questionSets,
            List<RepeatingQuestionSet> repeatingQuestionSets,
            int? repeatingInstanceMaxQuantity,
            IFlexCelWorkbook instance)
        {
            using (MiniProfiler.Current.Step(nameof(SpreadsheetCalculationService) + "." + nameof(this.GenerateRepeatingQuestionAnswerArray)))
            {
                if (repeatingQuestionSets == null)
                {
                    return null;
                }

                var repeatingFields = questionSets.SelectMany(x => x.Fields).Where(f => f is RepeatingField);
                List<List<object>> table = new List<List<object>>();
                List<Field> orderedFields = this.OrderRepeatingFields(repeatingQuestionSets);
                int rowIndex = SpreadsheetCalculationService.StandardWorkbookHeadingRowIndex;
                List<object> row = new List<object>();

                // first row is empty.
                table.Add(row);
                rowIndex++;
                foreach (var field in orderedFields)
                {
                    if (field is IDataStoringField dataStoringField)
                    {
                        int fieldRowIndex = dataStoringField.CalculationWorkbookCellLocation.Value.RowIndex;
                        string repeatingQuestionSetKey = field.QuestionSetKey;
                        while (rowIndex < fieldRowIndex)
                        {
                            row = new List<object>() { this.GetDefaultValue(field) };
                            table.Add(row);
                            rowIndex++;
                        }

                        row = new List<object>();
                        var repeatingField = repeatingFields
                            .Where(f => f.Key == repeatingQuestionSetKey)
                            .FirstOrDefault() as RepeatingField;
                        if (formModel[repeatingQuestionSetKey] != null &&
                           formModel[repeatingQuestionSetKey].Type == JTokenType.Array)
                        {
                            if (formModel[repeatingQuestionSetKey].ToArray().Length > repeatingInstanceMaxQuantity)
                            {
                                throw new ProductConfigurationException(
                                    Errors.Product.MisConfiguration(
                                        $"The number of repeating instances for \"{repeatingQuestionSetKey}\" is " +
                                        $"{formModel[repeatingQuestionSetKey].ToArray().Length} which is greater than {repeatingInstanceMaxQuantity}. " +
                                        $"Please ensure that the \"Maximum Quantity Expression\" does not exceed {repeatingInstanceMaxQuantity} or " +
                                        $"update the \"Repeating Question Sets\" configuration to handle {formModel[repeatingQuestionSetKey].ToArray().Length} repeating instances."));
                            }

                            foreach (var repeatingQuestionSetFields in formModel[repeatingQuestionSetKey].ToArray())
                            {
                                if (repeatingQuestionSetFields.IsNullOrEmpty())
                                {
                                    continue;
                                }

                                if (repeatingQuestionSetFields[field.Key] != null)
                                {
                                    row.Add(repeatingQuestionSetFields[field.Key]);
                                }
                                else
                                {
                                    row.Add(this.GetDefaultValue(field));
                                }
                            }
                        }

                        table.Add(row);
                        rowIndex++;
                    }
                }

                // override first row.
                List<object> firstRow = table[0];
                for (int i = 0; i < repeatingInstanceMaxQuantity; i++)
                {
                    firstRow.Add("Value " + (i + 1));
                }

                foreach (var tableRow in table)
                {
                    while (tableRow.Count < repeatingInstanceMaxQuantity)
                    {
                        tableRow.Add(string.Empty);
                    }
                }

                return JArray.FromObject(table);
            }
        }

        /// <summary>
        /// Gets the default value of the field.
        /// </summary>
        private object GetDefaultValue(Field field)
        {
            if (field.DataType == DataType.Boolean)
            {
                return false;
            }

            return string.Empty;
        }

        /// <summary>
        /// Orders question sets to the proper order of the workbook.
        /// </summary>
        private List<Field> OrderFields(List<QuestionSet> questionSets)
        {
            var fields = questionSets.SelectMany(x => x.Fields).ToList();
            fields.Sort((x, y) => this.CompareByCalculationWorkbookRow(x, y));
            return fields;
        }

        /// <summary>
        /// Orders repeating question sets to the proper order of the workbook.
        /// </summary>
        private List<Field> OrderRepeatingFields(List<RepeatingQuestionSet> repeatingQuestionSets)
        {
            var fields = repeatingQuestionSets.SelectMany(x => x.Fields).ToList();
            fields.Sort((x, y) => this.CompareByCalculationWorkbookRow(x, y));
            return fields;
        }

        /// <summary>
        /// Used for sorting workbook rows.
        /// </summary>
        private int CompareByCalculationWorkbookRow(Field first, Field second)
        {
            var firstScore = 0;
            var secondScore = 0;
            if (first is IDataStoringField firstDataStoringField)
            {
                if (firstDataStoringField.CalculationWorkbookCellLocation.HasValue)
                {
                    firstScore += firstDataStoringField.CalculationWorkbookCellLocation.Value.SheetIndex * 100000;
                    firstScore += firstDataStoringField.CalculationWorkbookCellLocation.Value.RowIndex;
                }
            }

            if (first is RepeatingField firstRepeatingField)
            {
                if (firstRepeatingField.CalculationWorkbookCellLocation.HasValue)
                {
                    firstScore += firstRepeatingField.CalculationWorkbookCellLocation.Value.SheetIndex * 100000;
                    firstScore += firstRepeatingField.CalculationWorkbookCellLocation.Value.RowIndex;
                }
            }

            if (second is IDataStoringField secondDataStoringField)
            {
                if (secondDataStoringField.CalculationWorkbookCellLocation.HasValue)
                {
                    secondScore += secondDataStoringField.CalculationWorkbookCellLocation.Value.SheetIndex * 100000;
                    secondScore += secondDataStoringField.CalculationWorkbookCellLocation.Value.RowIndex;
                }
            }

            if (second is RepeatingField secondRepeatingField)
            {
                if (secondRepeatingField.CalculationWorkbookCellLocation.HasValue)
                {
                    secondScore += secondRepeatingField.CalculationWorkbookCellLocation.Value.SheetIndex * 100000;
                    secondScore += secondRepeatingField.CalculationWorkbookCellLocation.Value.RowIndex;
                }
            }

            return firstScore - secondScore;
        }

        /// <summary>
        /// The method will iterate through the ordered list in reverse and
        /// return the row index of the first datastoring or repeating field.
        /// </summary>
        private int GetRowIndexOfLastFieldInQuestionSetsTable(List<Field> orderedFields)
        {
            for (int i = orderedFields.Count - 1; i >= 0; i--)
            {
                if (orderedFields[i] is IDataStoringField dataStoringField)
                {
                    return dataStoringField.CalculationWorkbookCellLocation.Value.RowIndex;
                }

                if (orderedFields[i] is RepeatingField repeatingField)
                {
                    return repeatingField.CalculationWorkbookCellLocation.Value.RowIndex;
                }
            }

            return orderedFields.Count;
        }
    }
}
