// <copyright file="ConfiguredError.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Error
{
    using System.Collections.Generic;
    using System.Net;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using UBind.Domain;

    /// <summary>
    /// Represents the details of a configured error from automations.
    /// </summary>
    public class ConfiguredError
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfiguredError"/> class.
        /// </summary>
        /// <param name="code">The error code to be used.</param>
        /// <param name="title">A title for the error message to be presented.</param>
        /// <param name="message">The error message raised/returned.</param>
        /// <param name="httpStatusCode">The http status code to be used for presenting the error.</param>
        /// <param name="additionalProperties">Addiional properties to be returned for error presentation, if any.</param>
        /// <param name="data">Additional data to be used for error presentation, if any.</param>
        public ConfiguredError(string code, string title, string message, int httpStatusCode, List<string> additionalProperties = null, JObject data = null)
        {
            this.Code = code;
            this.Title = title;
            this.Message = message;
            this.HttpStatusCode = httpStatusCode;
            this.AdditionalProperties = additionalProperties;
            this.Data = data;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfiguredError"/> class.
        /// </summary>
        [JsonConstructor]
        public ConfiguredError()
        {
        }

        /// <summary>
        /// Gets or sets the error code.
        /// </summary>
        [JsonProperty("code")]
        public string Code { get; set; }

        /// <summary>
        /// Gets or sets the title for the error message.
        /// </summary>
        [JsonProperty("title")]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the message of the error.
        /// </summary>
        [JsonProperty("message")]
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the http status code to be returned as part of the response.
        /// </summary>
        [JsonProperty("httpStatusCode")]
        public int HttpStatusCode { get; set; }

        /// <summary>
        /// Gets or sets a collection of additional details, if any, regarding the error.
        /// </summary>
        [JsonProperty("additionalDetails", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> AdditionalProperties { get; set; }

        /// <summary>
        /// Gets or sets an optional data object.
        /// </summary>
        [JsonProperty("data", NullValueHandling = NullValueHandling.Ignore)]
        public JObject Data { get; set; }

        /// <summary>
        ///  Returns an instance of <see cref="Error"/> from this automation error.
        /// </summary>
        /// <returns>An instance of error from this automation error.</returns>
        public Error ToError()
        {
            return new Error(this.Code, this.Title, this.Message, (HttpStatusCode)this.HttpStatusCode, this.AdditionalProperties, this.Data);
        }
    }
}
