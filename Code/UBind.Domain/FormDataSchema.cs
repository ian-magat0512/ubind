// <copyright file="FormDataSchema.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using MoreLinq;
    using Newtonsoft.Json.Linq;
    using UBind.Domain.Extensions;
    using UBind.Domain.Json;
    using UBind.Domain.Product.Component;
    using UBind.Domain.Product.Component.Form;
    using UBind.Domain.ReadWriteModel;

    /// <inheritdoc />
    public class FormDataSchema : IFormDataSchema
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FormDataSchema"/> class.
        /// </summary>
        /// <param name="formConfiguration">the form configuration.</param>
        public FormDataSchema(JObject formConfiguration)
        {
            this.ParseForQuestionMetaData(formConfiguration);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FormDataSchema"/> class.
        /// </summary>
        /// <param name="component">The product component configuration.</param>
        public FormDataSchema(Component component)
        {
            // During unit test execution Form could be null
            if (component.Form != null)
            {
                this.QuestionsMetaData = QuestionMetadataGenerator.Generate(component.Form);
            }
        }

        private IReadOnlyList<IQuestionMetaData> QuestionsMetaData { get; set; }

        /// <inheritdoc/>
        public IReadOnlyList<IQuestionMetaData> GetQuestionMetaData()
        {
            return this.QuestionsMetaData != null ? this.QuestionsMetaData : new List<IQuestionMetaData>();
        }

        /// <inheritdoc/>
        public QuestionChangeEvaluationResult EvaluateQuestionChanges(CachingJObjectWrapper previousFormData, CachingJObjectWrapper newFormData)
        {
            var questionChangeResult = this.GetQuestionValueChanges(previousFormData, newFormData);
            var questionValueChangedWhenNotAllowed = questionChangeResult.Find(e => e.ValueChanged == true && e.ChangeAllowedAfterApproval == false) != null;
            if (questionValueChangedWhenNotAllowed)
            {
                return QuestionChangeEvaluationResult.InvalidQuestionChangeEvaluationResult(questionChangeResult);
            }
            else
            {
                return QuestionChangeEvaluationResult.ValidQuestionChangeEvaluationResult();
            }
        }

        private List<QuestionValueChange> GetQuestionValueChanges(CachingJObjectWrapper oldFormData, CachingJObjectWrapper newFormData)
        {
            List<QuestionValueChange> questionChangeResults = new List<QuestionValueChange> { };
            if (this.QuestionsMetaData == null)
            {
                return questionChangeResults;
            }

            var previousQuestions = this.GetQuestionData(oldFormData);
            var newQuestions = this.GetQuestionData(newFormData);
            var questionsMetadata = this.QuestionsMetaData.Where(q => q.IsWithinRepeatingQuestionSet == false);
            questionChangeResults = this.GetQuestionSetsValueChanged(previousQuestions, newQuestions);

            var previousRepeatingQuestionData = this.GetRepeatingQuestionData(oldFormData);
            var newRepeatingQuestionData = this.GetRepeatingQuestionData(newFormData);
            var repeatingQuestionChangeResults = this.GetRepeatingQuestionSetsValueChanged(previousRepeatingQuestionData, newRepeatingQuestionData);
            questionChangeResults.AddRange(repeatingQuestionChangeResults);
            return questionChangeResults;
        }

        private List<QuestionValueChange> GetQuestionSetsValueChanged(List<QuestionData> previousQuestions, List<QuestionData> newQuestions)
        {
            List<QuestionValueChange> questionChangeResults = new List<QuestionValueChange> { };
            if (this.QuestionsMetaData == null)
            {
                return questionChangeResults;
            }

            var repeatingQuestionSetsParentKeys = this.GetRepeatingQuestionSetsParentKeys();
            previousQuestions = previousQuestions.Where(q => !repeatingQuestionSetsParentKeys.Contains(q.Questionkey)).ToList();
            previousQuestions.ForEach(previousQuestion =>
            {
                var previousQuestionExist = newQuestions.Find(q => q.Questionkey == previousQuestion.Questionkey) != null;
                if (previousQuestionExist)
                {
                    var newValue = newQuestions.Find(q => q.Questionkey == previousQuestion.Questionkey).Value;
                    var questionValueChanged = newValue != previousQuestion.Value;
                    var questionMetaData = this.QuestionsMetaData.Where(q => q.Key == previousQuestion.Questionkey).FirstOrDefault();

                    var canChangeWhenApproved = questionMetaData != null ? questionMetaData.CanChangeWhenApproved : false;
                    var questionChangeResult = new QuestionValueChange(
                                                    previousQuestion.Questionkey,
                                                    previousQuestion.Value,
                                                    newValue,
                                                    questionValueChanged,
                                                    canChangeWhenApproved);
                    questionChangeResults.Add(questionChangeResult);
                }
            });
            return questionChangeResults;
        }

        private List<QuestionValueChange> GetRepeatingQuestionSetsValueChanged(List<QuestionData> previousQuestions, List<QuestionData> newQuestions)
        {
            List<QuestionValueChange> questionChangeResults = new List<QuestionValueChange> { };
            if (this.QuestionsMetaData == null)
            {
                return questionChangeResults;
            }

            var previousQuestionIndex = previousQuestions.Select(c => c.Index).ToList();
            List<QuestionData> questions = previousQuestions;
            questions.AddRange(newQuestions.Where(e => !previousQuestionIndex.Contains(e.Index)));

            questions.ForEach(question =>
            {
                var previousQuestion = previousQuestions.Where(q => q.Questionkey == question.Questionkey && q.Index == question.Index);
                var newQuestion = newQuestions.Where(q => q.Questionkey == question.Questionkey && q.Index == question.Index);

                var previousValue = previousQuestion.Any() ? previousQuestion.First().Value : string.Empty;
                var newValue = newQuestion.Any() ? newQuestion.First().Value : string.Empty;
                var questionValueChanged = previousValue != newValue;
                var questionMetaData = this.QuestionsMetaData.Where(q => q.IsWithinRepeatingQuestionSet == true).Where(q => q.Key == question.Questionkey).FirstOrDefault();

                var canChangeWhenApproved = questionMetaData != null ? questionMetaData.CanChangeWhenApproved : false;
                var questionChangeResult = new QuestionValueChange(
                                                question.Questionkey,
                                                previousValue,
                                                newValue,
                                                questionValueChanged,
                                                canChangeWhenApproved);
                questionChangeResults.Add(questionChangeResult);
            });
            return questionChangeResults;
        }

        private void ParseForQuestionMetaData(JObject formConfiguration)
        {
            const string questionMetaDataKey = "questionMetaData";
            const string questionSetKey = "questionSets";
            const string repeatingQuestionSetKey = "repeatingQuestionSets";
            const string canChangeWhenApprovedKey = "canChangeWhenApproved";
            const string resetForNewQuotesKey = "resetForNewQuotes";
            const string resetForNewRenewalQuotesKey = "resetForNewRenewalQuotes";
            const string resetForNewAdjustmentQuotesKey = "resetForNewAdjustmentQuotes";
            const string resetForNewCancellationQuotesKey = "resetForNewCancellationQuotes";
            const string resetForNewPurchaseQuotesKey = "resetForNewPurchaseQuotes";
            const string dataTypeKey = "dataType";
            const string currencyCodeKey = "currencyCode";
            string defaultCurrencyCode = this.GetDefaultCurrencyCode(formConfiguration);
            List<IQuestionMetaData> questionsMetaData = new List<IQuestionMetaData>();
            JToken questionMetaData = formConfiguration[questionMetaDataKey];
            if (questionMetaData == null)
            {
                return;
            }

            var questionSets = questionMetaData[questionSetKey];
            if (questionSets == null)
            {
                return;
            }

            var repeatingQuestionSets = questionMetaData[repeatingQuestionSetKey];
            Dictionary<string, JToken> allQuestions = new Dictionary<string, JToken>()
            {
                [questionSetKey] = questionSets,
                [repeatingQuestionSetKey] = repeatingQuestionSets,
            };

            Dictionary<string, DataType?> questionSetKeyToDataTypeMap = new Dictionary<string, DataType?>();
            foreach (var questionGroup in allQuestions)
            {
                if (questionGroup.Value == null)
                {
                    continue;
                }

                bool isFieldWithinRepeatingQuestionSet = questionGroup.Key == repeatingQuestionSetKey;

                foreach (var questionSet in questionGroup.Value)
                {
                    foreach (JToken questions in questionSet)
                    {
                        var parentProperty = (JProperty)questions.Parent;
                        var parentName = parentProperty.Name;

                        foreach (JToken question in questions)
                        {
                            if (question.Type == JTokenType.Property)
                            {
                                JProperty questionProperty = (JProperty)question;
                                string key = questionProperty.Name;
                                if (questionProperty.Value.Type == JTokenType.Object)
                                {
                                    JObject questionObject = (JObject)questionProperty.Value;
                                    JToken canChangeWhenApprovedToken = questionObject[canChangeWhenApprovedKey];
                                    bool canChangeWhenApproved = canChangeWhenApprovedToken != null ? canChangeWhenApprovedToken.Value<bool>() : false;
                                    JToken resetForNewQuotesToken = questionObject[resetForNewQuotesKey];
                                    bool resetForNewQuotes = resetForNewQuotesToken != null ? resetForNewQuotesToken.Value<bool>() : false;
                                    JToken resetForNewRenewalQuotesToken = questionObject[resetForNewRenewalQuotesKey];
                                    bool resetForNewRenewalQuotes = resetForNewRenewalQuotesToken != null ? resetForNewRenewalQuotesToken.Value<bool>() : false;
                                    JToken resetForNewAdjustmentQuotesToken = questionObject[resetForNewAdjustmentQuotesKey];
                                    bool resetForNewAdjustmentQuotes = resetForNewAdjustmentQuotesToken != null ? resetForNewAdjustmentQuotesToken.Value<bool>() : false;
                                    JToken resetForNewCancellationQuotesToken = questionObject[resetForNewCancellationQuotesKey];
                                    bool resetForNewCancellationQuotes = resetForNewCancellationQuotesToken != null ? resetForNewCancellationQuotesToken.Value<bool>() : false;
                                    JToken resetForNewPurchaseQuotesToken = questionObject[resetForNewPurchaseQuotesKey];
                                    bool resetForNewPurchaseQuotes = resetForNewPurchaseQuotesToken != null ? resetForNewPurchaseQuotesToken.Value<bool>() : false;
                                    JToken dataTypeToken = questionObject[dataTypeKey];
                                    string dataTypeString = dataTypeToken != null ? dataTypeToken.Value<string>() : string.Empty;
                                    DataType dataType = dataTypeString.ToEnumOrThrow<DataType>();
                                    JToken currencyCodeToken = questionObject[currencyCodeKey];
                                    string currencyCode = currencyCodeToken != null ? currencyCodeToken.Value<string>() : defaultCurrencyCode;

                                    // if from question set, add to the list its key and its data type.
                                    if (!isFieldWithinRepeatingQuestionSet
                                        && dataType == DataType.Repeating)
                                    {
                                        questionSetKeyToDataTypeMap.Add(key, dataType);
                                    }

                                    // if a repeating question set. check for parent data type.
                                    questionSetKeyToDataTypeMap.TryGetValue(parentName, out DataType? parentDataType);

                                    questionsMetaData.Add(new QuestionMetaData(
                                                        key,
                                                        canChangeWhenApproved,
                                                        resetForNewQuotes,
                                                        resetForNewRenewalQuotes,
                                                        resetForNewAdjustmentQuotes,
                                                        resetForNewCancellationQuotes,
                                                        resetForNewPurchaseQuotes,
                                                        dataType,
                                                        parentName,
                                                        isFieldWithinRepeatingQuestionSet ? parentDataType : null,
                                                        isFieldWithinRepeatingQuestionSet,
                                                        currencyCode));
                                }
                            }
                        }
                    }
                }

                this.QuestionsMetaData = questionsMetaData;
            }
        }

        private List<QuestionData> GetQuestionData(CachingJObjectWrapper formData)
        {
            JObject formModel = (JObject)formData.JObject["formModel"];
            var questionDataList = formModel.Properties().ToList()
                    .Where(p => !string.IsNullOrEmpty(p.Value.ToString()))
                    .Select(s => new QuestionData(s.Name.ToString(), s.Value.ToString()));

            return questionDataList.ToList();
        }

        private List<QuestionData> GetRepeatingQuestionData(CachingJObjectWrapper formData)
        {
            var repeatingQuestionSetsParentKeys = this.GetRepeatingQuestionSetsParentKeys();
            List<QuestionData> questionDataList = new List<QuestionData> { };
            List<JProperty> repeatingQuestionSets = new List<JProperty> { };
            JObject formModel = (JObject)formData.JObject["formModel"];
            repeatingQuestionSets.AddRange(
                formModel.Properties().Where(e => repeatingQuestionSetsParentKeys.Contains(e.Name.ToString())));

            foreach (var repeatingQuestionSet in repeatingQuestionSets)
            {
                int index = 0;
                foreach (var repeatingQuestions in repeatingQuestionSet.Value)
                {
                    JObject repeatingQuestion = (JObject)repeatingQuestions;
                    var approvedCalculationResultRepeatingQuestions = repeatingQuestion.Properties().ToList();
                    questionDataList.AddRange(
                        approvedCalculationResultRepeatingQuestions
                            .Where(p => !string.IsNullOrEmpty(p.Value.ToString()))
                            .Select(s => new QuestionData(s.Name.ToString(), s.Value.ToString(), index)));
                    index += 1;
                }
            }

            return questionDataList;
        }

        private string GetDefaultCurrencyCode(JObject formConfigurationJObject)
        {
            string defaultCurrencyCode = null;
            JToken defaultCurrency = formConfigurationJObject.SelectToken("$.settings.financial.defaultCurrency");
            if (defaultCurrency != null)
            {
                defaultCurrencyCode = defaultCurrency.Value<string>();
            }

            return defaultCurrencyCode ?? PriceBreakdown.DefaultCurrencyCode;
        }

        private IEnumerable<string> GetRepeatingQuestionSetsParentKeys()
        {
            return this.QuestionsMetaData.Where(q => q.IsWithinRepeatingQuestionSet == true).Select(e => e.ParentQuestionSetKey).Distinct();
        }
    }
}
