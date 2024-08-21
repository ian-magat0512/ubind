// <copyright file="ClaimFormData.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Claim
{
    using System.Dynamic;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Stores a snapshot of the form data for an application.
    /// </summary>
    public class ClaimFormData
    {
        private const string FormModelKey = "formModel";
        private JObject formModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClaimFormData"/> class.
        /// </summary>
        /// <param name="json">JSON representation of form data.</param>
        public ClaimFormData(string json)
        {
            this.Json = json;
        }

        [JsonConstructor]
        private ClaimFormData()
        {
        }

        /// <summary>
        /// Gets the form data as JSON.
        /// </summary>
        [JsonProperty]
        public string Json { get; private set; }

        /// <summary>
        /// Gets a string containing the form model JSON.
        /// </summary>
        public JObject FormModel
        {
            get
            {
                if (this.formModel == null)
                {
                    var data = JObject.Parse(this.Json);
                    this.formModel = data.Value<JObject>(FormModelKey);
                }

                return this.formModel;
            }
        }

        /// <summary>
        /// Gets form data by key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The value of the form field for the given key.</returns>
        public string GetValue(string key)
        {
            dynamic formUpdate = JsonConvert.DeserializeObject<ExpandoObject>(this.Json);
            var jobject = JObject.FromObject(formUpdate.formModel);
            return jobject?.SelectToken(key)?.ToString();
        }
    }
}
