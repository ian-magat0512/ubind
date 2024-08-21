// <copyright file="UBindProblemDetails.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Infrastructure
{
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// For returning error responses from the Web API.
    /// </summary>
    public class UBindProblemDetails : ProblemDetails
    {
        public UBindProblemDetails()
        {
        }

        public UBindProblemDetails(UBind.Domain.Error error)
        {
            this.Title = error.Title;
            this.Status = (int)error.HttpStatusCode;
            this.Detail = error.Message;
            this.Code = error.Code;
            this.AdditionalDetails = error.AdditionalDetails;
            this.Data = error.Data;
        }

        /// <summary>
        /// Gets or sets a code identifying the class of error.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "code")]
        public string Code { get; set; }

        /// <summary>
        /// Gets or sets additional error details.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "additionalDetails")]
        public IEnumerable<string> AdditionalDetails { get; set; }

        /// <summary>
        /// Gets or sets the contextual data.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "data")]
        public JObject Data { get; set; }

        /// <summary>
        /// Creates a new instance of the <see cref="UBindProblemDetails"></see> class.
        /// </summary>
        /// <param name="error">An error with details to populate the error response.</param>
        /// <returns>A new instancen of <see cref="UBindProblemDetails"/>.</returns>
        public static UBindProblemDetails FromError(UBind.Domain.Error error) =>
            new UBindProblemDetails(error);
    }
}
