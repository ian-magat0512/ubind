// <copyright file="Error.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using CSharpFunctionalExtensions;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Represents an error which has occurred within the application.
    /// </summary>
    public class Error : ValueObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Error"/> class.
        /// </summary>
        /// <param name="code">The error code, e.g. "record.not.found" - we are using a string rather than an int as the error code can then convey some meaning in and of itself, yet it still must be unique.</param>
        /// <param name="title">A title for the message. This is used when the message is presented to the user in a dialog.</param>
        /// <param name="message">The error message.</param>
        /// <param name="httpStatusCode">The System.Net.HttpStatusCode statuscode, e.g. BadRequest.</param>
        /// <param name="additionalDetails">If there are additional details, they can be passed here.</param>
        /// <param name="data">the data you would want to pass.</param>
        public Error(
            string code,
            string title,
            string message,
            HttpStatusCode httpStatusCode = HttpStatusCode.BadRequest,
            IEnumerable<string>? additionalDetails = null,
            JObject? data = null)
        {
            this.Code = code;
            this.Title = title;
            this.Message = message;
            this.HttpStatusCode = httpStatusCode;
            this.AdditionalDetails = additionalDetails != null
                ? additionalDetails.ToList()
                : new List<string>();
            this.Data = data;
        }

        /// <summary>
        /// Gets the error code, e.g. "record.not.found" - we are using a string rather than an int as the error code can then convey some meaning in and of itself, yet it still must be unique.
        /// </summary>
        [JsonProperty]
        public string Code { get; }

        /// <summary>
        /// Gets a title for the error.
        /// </summary>
        [JsonProperty]
        public string Title { get; }

        /// <summary>
        /// Gets the error message.
        /// </summary>
        [JsonProperty]
        public string Message { get; }

        /// <summary>
        /// Gets the http code as HttpStatusCode, e.g. BadRequest = 400.
        /// </summary>
        public HttpStatusCode HttpStatusCode { get; }

        /// <summary>
        /// Gets or sets the list of error messasges.
        /// </summary>
        [JsonProperty]
        public List<string>? AdditionalDetails { get; set; }

        /// <summary>
        /// Gets or sets the contextual data.
        /// </summary>
        [JsonProperty]
        public JObject? Data { get; set; }

        /// <summary>
        /// Converts the error to a single string.
        /// </summary>
        /// <returns>a string representation of the error.</returns>
        public override string ToString()
        {
            var @string = $"{this.Title}: {this.Message}.";
            if (this.AdditionalDetails != null && this.AdditionalDetails.Any())
            {
                @string += $" Additional Details: [ {string.Join("; ", this.AdditionalDetails)} ].";
            }

            if (this.Data != null)
            {
                @string += $" Data: [ {this.Data} ].";
            }

            @string += $" Code: {this.Code}.";
            return @string;
        }

        /// <summary>
        /// returns the components needed for equality tests.
        /// </summary>
        /// <returns>the components needed for equality tests.</returns>
        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return this.Code;
        }
    }
}
