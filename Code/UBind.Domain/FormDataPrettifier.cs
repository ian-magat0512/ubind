// <copyright file="FormDataPrettifier.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Extensions.Logging;
    using MoreLinq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using UBind.Domain.Product;
    using UBind.Domain.Product.Component.Form;

    /// <summary>
    /// A class responsible to prettify FormData.
    /// </summary>
    public class FormDataPrettifier : IFormDataPrettifier
    {
        private IFormDataFieldFormatterFactory formatterFactory;
        private ILogger<FormDataPrettifier> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="FormDataPrettifier"/> class.
        /// </summary>
        /// <param name="formatterFactory">a factory which creates form data field formatters.</param>
        /// <param name="logger">A logger for debugging purposes.</param>
        public FormDataPrettifier(IFormDataFieldFormatterFactory formatterFactory, ILogger<FormDataPrettifier> logger)
        {
            this.formatterFactory = formatterFactory;
            this.logger = logger;
        }

        /// <summary>
        /// Get prettified form data.
        /// </summary>
        /// <param name="questionsMetaData">the form's questions metadata.</param>
        /// <param name="jsonFormData">The raw json.</param>
        /// <param name="entityId">For debugging purposes, the entity id the form data is for.</param>
        /// <param name="entityType">For debugging purposes, the entity type the form data is for, e.g. Quote or Claim.</param>
        /// <param name="productContext">For debugging purposes, the product context.</param>
        /// <returns>return the updated form data.</returns>
        public JObject GetPrettifiedFormData(
            IEnumerable<IQuestionMetaData> questionsMetaData,
            string jsonFormData,
            Guid entityId,
            string entityType,
            IProductContext productContext)
        {
            JObject formDataJObject = JObject.Parse(jsonFormData);
            JObject formModelJObject = (JObject)formDataJObject["formModel"];
            this.Prettify(questionsMetaData, formModelJObject, entityId, entityType, productContext);
            return formDataJObject;
        }

        /// <summary>
        /// Gets the prettified Calculation Result json.
        /// </summary>
        /// <param name="questionsMetaData">the form's questions metadata.</param>
        /// <param name="jsonCalculationResult">The raw Calculation Result json.</param>
        /// <param name="entityId">For debugging purposes, the entity id the form data is for.</param>
        /// <param name="entityType">For debugging purposes, the entity type the form data is for, e.g. Quote or Claim.</param>
        /// <param name="productContext">For debugging purposes, the product context.</param>
        /// <returns>return the updated calculation result json string.</returns>
        public JObject GetPrettifiedCalculationResult(
            IEnumerable<IQuestionMetaData> questionsMetaData,
            string jsonCalculationResult,
            Guid entityId,
            string entityType,
            IProductContext productContext)
        {
            JObject calculationResultJObject = JObject.Parse(jsonCalculationResult);

            foreach (var parent in calculationResultJObject.Children())
            {
                var parentKey = parent.Path;

                var questionSets = calculationResultJObject[parentKey].Children()
                                                                      .OfType<JProperty>()
                                                                      .Select(x => x.Name)
                                                                      .ToList();

                foreach (var question in questionSets)
                {
                    var questionMeta = questionsMetaData.FirstOrDefault(x => x.Key == question);

                    if (questionMeta != null)
                    {
                        if (questionMeta.DataType == DataType.Repeating)
                        {
                            foreach (var child in calculationResultJObject[parentKey][question].Children())
                            {
                                var repeatingField = calculationResultJObject[parentKey][question];

                                if (repeatingField.Type == JTokenType.Array)
                                {
                                    repeatingField.Children().SelectMany(jObject => jObject.Children())
                                        .Where(jProperty => ((JProperty)jProperty).Value != null)
                                        .Select(jProperty => new KeyValuePair<string, JToken>(((JProperty)jProperty).Name, ((JProperty)jProperty).Value))
                                        .ForEach(x => x.Value.Replace(this.PrettifyFieldData(x.Value.ToString(), questionsMetaData.First(y => y.Key == x.Key))));
                                }
                            }
                        }
                        else
                        {
                            JToken value = calculationResultJObject[parentKey][question];
                            value.Replace(this.PrettifyFieldData(value.ToString(), questionMeta));
                        }
                    }
                }
            }

            return calculationResultJObject;
        }

        /// <summary>
        /// Prettifies form data.
        /// </summary>
        /// <param name="questionsMetaData">the form's questions metadata.</param>
        /// <param name="formModelJObject">The json object to be updated.</param>
        /// <param name="entityId">For debugging purposes, the entity id the form data is for.</param>
        /// <param name="entityType">For debugging purposes, the entity type the form data is for, e.g. Quote or Claim.</param>
        /// <param name="productContext">For debugging purposes, the product context.</param>
        public void Prettify(
            IEnumerable<IQuestionMetaData> questionsMetaData,
            JObject formModelJObject,
            Guid entityId,
            string entityType,
            IProductContext productContext)
        {
            if (questionsMetaData == null || formModelJObject == null)
            {
                // Nothing we can do. We can't prettify the form data unless we have the metadata about the fields.
                // Some older form configurations didn't provide metadata so this can happen sometimes with older data.
                return;
            }

            string selectedCurrencyCode = this.GetSelectedCurrencyCode(formModelJObject);

            try
            {
                foreach (var questionMetaData in questionsMetaData)
                {
                    questionMetaData.CurrencyCode = selectedCurrencyCode ?? questionMetaData.CurrencyCode;

                    if (questionMetaData.DataType == DataType.Repeating)
                    {
                        // Escape question meta data with repeating the data type,
                        // only the field inside a repeating question set will be processed.
                        continue;
                    }
                    else if (questionMetaData.IsWithinRepeatingQuestionSet)
                    {
                        if (questionMetaData.ParentQuestionSetDataType != null
                            && questionMetaData.ParentQuestionSetDataType != DataType.Repeating)
                        {
                            continue;
                        }

                        var repeatingField = formModelJObject[questionMetaData.ParentQuestionSetKey];
                        this.PrettifyMatchingRepeatingFields(questionMetaData, repeatingField, entityId, entityType, productContext);
                    }
                    else
                    {
                        JToken value = formModelJObject[questionMetaData.Key];
                        if (value != null)
                        {
                            value.Replace(this.PrettifyFieldData(value.ToString(), questionMetaData));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Error prettifying form data.", e);
            }
        }

        private void PrettifyMatchingRepeatingFields(
            IQuestionMetaData questionMetaData,
            JToken repeatingField,
            Guid entityId,
            string entityType,
            IProductContext productContext)
        {
            JToken parent = repeatingField;
            if (parent == null)
            {
                return;
            }

            try
            {
                JArray repeatingJArray;
                if (parent.Type == JTokenType.String)
                {
                    var repeatingQuestionSetAsJsonString = parent.ToString();
                    if (repeatingQuestionSetAsJsonString == string.Empty)
                    {
                        // nothing to prettify.
                        return;
                    }

                    repeatingJArray = JArray.Parse(repeatingQuestionSetAsJsonString);
                    parent = repeatingJArray;
                }

                if (parent.Type != JTokenType.Array)
                {
                    // It's an unexpected situation to have any other type, however it's not fatal, since all it will result in is the data being unformatted.
                    // So we will log this error and continue.
                    this.logger.LogError(
                        $"When trying to prettify form data for a {entityType} with ID {entityId}, "
                        + $"we came across a field which is supposed to be a repeating question set field, and so we "
                        + $"were expecting it to contain an array of data, however the JTokenType was {parent.Type}. ",
                        productContext,
                        entityId,
                        entityType);
                    return;
                }
            }

            // ignores the issue with parsing a string to a JArray.
            catch (JsonReaderException)
            {
            }

            parent.Children().SelectMany(jObject => jObject.Children())
                .Where(jProperty => ((JProperty)jProperty).Name == questionMetaData.Key && ((JProperty)jProperty).Value != null)
                .Select(jProperty => ((JProperty)jProperty).Value)
                .ForEach(value => value.Replace(this.PrettifyFieldData(value.ToString(), questionMetaData)));
        }

        private string PrettifyFieldData(string value, IQuestionMetaData questionMetaData)
        {
            var result = this.formatterFactory.Create(questionMetaData.DataType);
            return result.IsSuccess ? result.Value.Format(value, questionMetaData) : value;
        }

        /// <summary>
        /// Gets the selected currency code from a form data field with key "currencyCode", if it exists, otherwise, returns null.
        /// </summary>
        /// <param name="formData">The submitted form data.</param>
        /// <returns>The currency code or null if not found.</returns>
        private string GetSelectedCurrencyCode(JObject formData)
        {
            JToken currencyCode = formData.SelectToken("currencyCode");
            if (currencyCode != null)
            {
                return currencyCode.Value<string>();
            }

            return null;
        }
    }
}
