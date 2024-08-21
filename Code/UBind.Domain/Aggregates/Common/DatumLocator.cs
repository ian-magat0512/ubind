// <copyright file="DatumLocator.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Common
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using UBind.Domain.Extensions;
    using UBind.Domain.Json;

    /// <summary>
    /// Specifies the location of an item of data in form data or calculation result json.
    /// </summary>
    public class DatumLocator : IDatumLocator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DatumLocator"/> class.
        /// </summary>
        /// <param name="object">The object containing the quote datum.</param>
        /// <param name="path">THe path to the quote datum within the object.</param>
        public DatumLocator(DatumLocationObject @object, string path)
        {
            this.Object = @object;
            this.Path = path;
        }

        [JsonConstructor]
        private DatumLocator()
        {
        }

        /// <inheritdoc/>
        [JsonProperty]
        public DatumLocationObject Object { get; private set; }

        /// <inheritdoc/>
        [JsonProperty]
        public string Path { get; private set; }

        /// <inheritdoc/>
        public TDatum Invoke<TDatum>(JObject formData, JObject calculationResult)
        {
            formData.ThrowIfArgumentNull(nameof(formData));
            calculationResult.ThrowIfArgumentNull(nameof(calculationResult));
            var token = this.Object == DatumLocationObject.FormData
                ? formData.SelectToken("formModel").SelectToken(this.Path)
                : calculationResult.SelectToken(this.Path);
            return token != null && !token.IsNullOrEmpty()
                ? token.Value<TDatum>()
                : default;
        }

        /// <inheritdoc/>
        public void UpdateFormModel(JObject formData, JValue value)
        {
            if (this.Object == DatumLocationObject.FormData)
            {
                var jPath = new JsonPath(this.Path);
                formData.PatchProperty(jPath, value);
            }
        }
    }
}
