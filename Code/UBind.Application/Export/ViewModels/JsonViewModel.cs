// <copyright file="JsonViewModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Export.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json.Linq;
    using UBind.Application.Helpers;
    using UBind.Domain;
    using UBind.Domain.Dto;
    using UBind.Domain.Extensions;
    using UBind.Domain.Product;

    /// <summary>
    /// View model for presenting json data.
    /// </summary>
    public class JsonViewModel
    {
        private readonly JObject jObject;
        private readonly DisplayableFieldDto displayableFieldDto;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonViewModel"/> class.
        /// </summary>
        /// <param name="jObject">The JSON to present.</param>
        /// <param name="displayableFieldDto">The displayable fields.</param>
        public JsonViewModel(JObject jObject, DisplayableFieldDto displayableFieldDto)
        {
            this.jObject = jObject;
            this.displayableFieldDto = displayableFieldDto;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonViewModel"/> class.
        /// </summary>
        /// <param name="jObject">The JSON to present.</param>
        /// <param name="displayableFieldDto">The displayable fields.</param>
        /// <param name="formDataSchema">The form data schema.</param>
        /// <param name="formDataPrettifier">The form data prettifier.</param>
        /// <param name="entityId">For debugging purposes, the entity id the form data is for.</param>
        /// <param name="entityType">For debugging purposes, the entity type the form data is for, e.g. Quote or Claim.</param>
        /// <param name="productContext">For debugging purposes, the product context.</param>
        public JsonViewModel(
            IFormDataPrettifier formDataPrettifier,
            JObject jObject,
            DisplayableFieldDto displayableFieldDto,
            IFormDataSchema formDataSchema,
            Guid entityId,
            string entityType,
            IProductContext productContext)
        {
            this.displayableFieldDto = displayableFieldDto;
            if (formDataSchema != null)
            {
                // clone the JObject so we don't change the original data when we prettify it.
                this.jObject = new JObject(jObject);
                formDataPrettifier.Prettify(formDataSchema.GetQuestionMetaData(), this.jObject, entityId, entityType, productContext);
            }
            else
            {
                this.jObject = jObject;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonViewModel"/> class.
        /// </summary>
        /// <param name="jObject">The JSON to present.</param>
        public JsonViewModel(JObject jObject)
        {
            this.jObject = jObject;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonViewModel"/> class.
        /// </summary>
        /// <param name="json">The json to present.</param>
        public JsonViewModel(string json)
        {
            if (json.IsNotNullOrEmpty())
            {
                this.jObject = JObject.Parse(json);
            }
        }

        /// <summary>
        /// Gets a json property value as a string.
        /// </summary>
        /// <param name="propertyNameOrPath">The name of the property or a JsonPath.</param>
        /// <returns>A string representing the value of the property, or null if the property is not found.</returns>
        public dynamic this[string propertyNameOrPath]
        {
            get
            {
                JToken token = this.jObject?.SelectToken(propertyNameOrPath);

                if (token != null && token.Type == JTokenType.Array && token.Any())
                {
                    List<JsonViewModel> tokenItemList = new List<JsonViewModel>();
                    var tokensToAdd = token.Where(s => s.Type == JTokenType.Object).Select(s => new JsonViewModel((JObject)s));
                    tokenItemList.AddRange(tokensToAdd);
                    return tokenItemList;
                }

                if (token != null && token.Type == JTokenType.Object && token.Any())
                {
                    return new JsonViewModel(JObject.Parse(token.ToString()), this.displayableFieldDto);
                }

                return token?.ToString() ?? string.Empty;
            }
        }

        /// <summary>
        /// Merge another form model to your current form model.
        /// </summary>
        /// <param name="formModel">json to be merged.</param>
        public void Merge(JObject formModel)
        {
            this.jObject.Merge(formModel);
        }

        /// <summary>
        /// Gets a form property value with special string comparison option.
        /// </summary>
        /// <param name="propertyName">The name.</param>
        /// <param name="stringComparison">String comparison option.</param>
        /// <returns>A string representing the value of the property, or null if the property is not found.</returns>
        public string GetValue(
            string propertyName, StringComparison stringComparison = 0)
        {
            return this.jObject
                .GetValue(propertyName, stringComparison)
                ?.Value<string>();
        }

        /// <summary>
        /// Gets a value indicating whether a given property exists in the form.
        /// </summary>
        /// <param name="propertyName">The name of the property to test for.</param>
        /// <returns>true, if the property exists, otherwise false.</returns>
        public bool HasProperty(string propertyName)
        {
            IDictionary<string, JToken> dictionary = this.jObject;
            return dictionary.ContainsKey(propertyName);
        }

        /// <summary>
        /// Pretty print form data.
        /// </summary>
        /// <returns>A string containing the pretty-printed data.</returns>
        public string PrettyPrint()
        {
            return this.displayableFieldDto != null
                ? this.jObject.FilterDisplayableFieldKeys(this.displayableFieldDto).PrettyPrint()
                : this.jObject.PrettyPrint();
        }

        /// <summary>
        /// Pretty print form data as HTML.
        /// </summary>
        /// <returns>A string containing the HTML-formatted data.</returns>
        public string PrettyPrintHtml()
        {
            return this.displayableFieldDto != null
                ? this.jObject.FilterDisplayableFieldKeys(this.displayableFieldDto).PrettyPrintHtml()
                : this.jObject.PrettyPrintHtml();
        }

        /// <summary>
        /// Pretty print data as HTML Table.
        /// </summary>
        /// <param name="includeRepeatingQuestionSets">Optional to include Repeating Question Sets in the result.</param>
        /// <returns>A string containing the HTML-formatted data.</returns>
        public string PrettyPrintHtmlTables(bool includeRepeatingQuestionSets = true)
        {
            return this.displayableFieldDto != null && this.displayableFieldDto.DisplayableFields != null
                ? this.jObject.FilterDisplayableFieldKeys(this.displayableFieldDto).PrettyPrintHtmlTables(includeRepeatingQuestionSets)
                : this.jObject.PrettyPrintHtmlTables(includeRepeatingQuestionSets);
        }

        /// <summary>
        /// Print form data as URL encoded name value pairs.
        /// </summary>
        /// <returns>A string containing the HTML-formatted data.</returns>
        public string PrintAsURLParams()
        {
            return this.jObject.PrintAsURLParams();
        }

        /// <summary>
        /// Pretty print form data.
        /// </summary>
        /// <param name="excludedFieldKeys">Optional array of field keys to be excluded.</param>
        /// <returns>A string containing the pretty-printed data.</returns>
        public string PrettyPrintExcluding(params string[] excludedFieldKeys)
        {
            return this.jObject.PrettyPrint(excludedFieldKeys);
        }

        /// <summary>
        /// Pretty print form data as HTML.
        /// </summary>
        /// <param name="excludedFieldKeys">Optional array of field keys to be excluded.</param>
        /// <returns>A string containing the HTML-formatted data.</returns>
        public string PrettyPrintHtmlExcluding(params string[] excludedFieldKeys)
        {
            return this.jObject.PrettyPrintHtml(excludedFieldKeys);
        }

        /// <summary>
        /// Print form data as URL encoded name value pairs.
        /// </summary>
        /// <param name="excludedFieldKeys">Optional array of field keys to be excluded.</param>
        /// <returns>A string containing the URL encoded data.</returns>
        public string PrintAsURLParamsExcluding(params string[] excludedFieldKeys)
        {
            return this.jObject.PrintAsURLParams(excludedFieldKeys);
        }

        /// <summary>
        /// Gets the form model as Json.
        /// </summary>
        /// <returns>A string containing the form model as Json.</returns>
        public string AsJson()
        {
            return this.jObject.ToString();
        }
    }
}
