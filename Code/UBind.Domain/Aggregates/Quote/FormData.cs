// <copyright file="FormData.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote
{
    using System.Collections.Generic;
    using System.Linq;
    using CSharpFunctionalExtensions;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Newtonsoft.Json.Serialization;
    using NodaTime;
    using UBind.Domain.Dto.FormData;
    using UBind.Domain.Extensions;
    using UBind.Domain.Json;
    using UBind.Domain.ReadModel.Claim;

    /// <summary>
    /// Stores a snapshot of the form data for an application.
    /// </summary>
    public class FormData : CachingJObjectWrapper
    {
        private const string FormModelKey = "formModel";
        private const string PastClaimsKey = "pastClaims";
        private const string EmptyFormDataJsonString = "{\"formModel\":{}}";

        /// <summary>
        /// Initializes a new instance of the <see cref="FormData"/> class from a string of json.
        /// </summary>
        /// <param name="json">String JSON representation of form data.</param>
        public FormData(string json)
            : base(json)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FormData"/> class from an existing JObject.
        /// </summary>
        /// <param name="formModel">JObject representation of form data.</param>
        [System.Text.Json.Serialization.JsonConstructor]
        public FormData(JObject formModel)
        {
            if (formModel == null)
            {
                return;
            }

            bool hasFormModelKey = formModel.SelectToken(FormModelKey) != null;
            if (hasFormModelKey)
            {
                this.internalJObject = formModel;
            }
            else
            {
                this.internalJObject = new JObject();
                this.internalJObject[FormModelKey] = formModel;
            }
        }

        [JsonConstructor]
        private FormData()
            : base()
        {
        }

        /// <summary>
        /// Gets a string containing the form model JSON.
        /// </summary>
        public JObject FormModel => (JObject)this.JObject[FormModelKey];

        /// <summary>
        /// Create formdata with empty form model.
        /// </summary>
        /// <returns>A form data.</returns>
        public static FormData CreateEmpty()
        {
            return new FormData(EmptyFormDataJsonString);
        }

        /// <summary>
        /// Clones the form data.
        /// </summary>
        /// <returns>A new instance of <see cref="FormData"/> identical to this one.</returns>
        public FormData Clone()
        {
            return new FormData(this.Json);
        }

        /// <summary>
        /// Gets form data by key.
        /// </summary>
        /// <param name="fieldPath">The key.</param>
        /// <returns>The value of the form field for the given key.</returns>
        public string GetValue(string fieldPath)
        {
            JToken jToken = this.FormModel.SelectToken(fieldPath);
            if (jToken != null)
            {
                return jToken.ToString();
            }
            else
            {
                return null;
            }

            /*return this.FormModel[fieldPath]?.ToString();*/
        }

        /// <summary>
        /// Returns a value indicating whether the form model contains a given property.
        /// </summary>
        /// <param name="path">The json path specifying the property.</param>
        /// <returns><c>true</c> if the form model contains the given property, otherwise <c>false</c>.</returns>
        public bool HasProperty(string path)
        {
            return this.FormModel.SelectToken(path) != null;
        }

        /// <summary>
        /// Returns a value indicating whether the form model contains a given property and the property has data.
        /// </summary>
        /// <param name="path">The json path specifying the property.</param>
        /// <returns><c>true</c> if the form model contains the given propertyand the property has data, otherwise <c>false</c>.</returns>
        public bool PropertyIsNullOrEmpty(string path)
        {
            return this.FormModel.SelectToken(path).IsNullOrEmpty();
        }

        /// <summary>
        /// Create a new copy of the form data with updated dates.
        /// </summary>
        /// <param name="newEffectiveDate">The new effective date.</param>
        /// <param name="newExpiryDate">The new expiry date.</param>
        /// <param name="formDataPaths">Specification for where to locate dates in the json.</param>
        /// <returns>A new instance of <see cref="FormData"/> with a copy of this instance's data but with updated dates.</returns>
        public FormData UpdateDates(LocalDate newEffectiveDate, LocalDate newExpiryDate, IFormDataPaths formDataPaths)
        {
            JObject obj = this.JObject;
            var effectiveDateToken = obj.SelectToken(FormModelKey + "." + formDataPaths.EffectiveDatePath);
            effectiveDateToken.Replace(newEffectiveDate.ToMMDDYYYWithSlashes());
            var expiryDateToken = obj.SelectToken(FormModelKey + "." + formDataPaths.ExpiryDatePath);
            expiryDateToken.Replace(newExpiryDate.ToMMDDYYYWithSlashes());
            return new FormData(new JObject(obj));
        }

        /// <summary>
        /// Checks if a given property of formModel can be patched according to given rules.
        /// </summary>
        /// <param name="path">The path specifying the property.</param>
        /// <param name="rules">The rules.</param>
        /// <returns>A result indicating whether the patch can be applied, or the reason why not.</returns>
        public Result CanPatchFormModelProperty(JsonPath path, PatchRules rules)
        {
            return this.FormModel.CanPatchProperty(path, rules);
        }

        /// <summary>
        /// Patch a given property.
        /// </summary>
        /// <param name="path">The path of the property to patch.</param>
        /// <param name="newValue">The new value for the property.</param>
        public void PatchProperty(JsonPath path, JToken newValue)
        {
            if (newValue == null || newValue?.Type == JTokenType.Null)
            {
                return;
            }

            if (this.internalJObject == null)
            {
                this.internalJObject = JObject.Parse(this.internalJson);
            }

            this.internalJObject.PatchProperty(path, newValue);
            this.internalJson = null;
        }

        /// <summary>
        /// Patch a given property inside the formModel.
        /// </summary>
        /// <param name="path">The path of the property to patch.</param>
        /// <param name="newValue">The new value for the property.</param>
        public void PatchFormModelProperty(JsonPath path, JToken newValue)
        {
            this.FormModel.PatchProperty(path, newValue);
            this.internalJson = null;
        }

        /// <summary>
        /// Create a new copy of the form data with the list value.
        /// </summary>
        /// <param name="propertyName">The Property Name.</param>
        /// <param name="list">The Property Value.</param>
        public void InjectFormDataListValue(string propertyName, dynamic list)
        {
            var serializer = new JsonSerializer()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
            };

            JArray array = JArray.FromObject(list, serializer);
            this.JObject[FormModelKey][propertyName] = array;
            this.internalJson = null;
        }

        /// <summary>
        /// Remove form data propery.
        /// </summary>
        /// <param name="propertyName">The Property Name.</param>
        public void RemoveFormDataProperty(string propertyName)
        {
            this.JObject.Descendants()
                .OfType<JProperty>()
                .Where(attr => attr.Name.Contains(propertyName))
                .ToList()
                .ForEach(attr => attr.Remove());
            this.internalJson = null;
        }

        /// <summary>
        /// Insert claims data into the form at well-known location.
        /// </summary>
        /// <param name="recentClaimsUnderPolicy">The set of recent claims to be inserted.</param>
        public void AddRecentClaims(IEnumerable<IClaimReadModel> recentClaimsUnderPolicy)
        {
            if (recentClaimsUnderPolicy != null && recentClaimsUnderPolicy.Any())
            {
                var claimPresentationModel = recentClaimsUnderPolicy.Select(claim => new HistoricalClaimPresentationModel(claim)).ToList();
                var claimsArray = JArray.FromObject(
                    claimPresentationModel,
                    new JsonSerializer { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                this.PatchFormModelProperty(new JsonPath(PastClaimsKey), claimsArray);
            }
            else
            {
                this.PatchFormModelProperty(new JsonPath(PastClaimsKey), new JArray());
            }
        }
    }
}
