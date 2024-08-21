// <copyright file="TriggerRequest.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation
{
    using System.Collections.Generic;
    using Microsoft.Extensions.Primitives;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using UBind.Application.Automation.Enums;
    using UBind.Application.Automation.Http;
    using Path = UBind.Domain.ValueTypes.Path;

    /// <summary>
    /// Represents the request property of a trigger when parsing from an HttpContext.
    /// </summary>
    /// <remarks>Only used for trigger of type <see cref="TriggerType.HttpTrigger"/>.</remarks>
    public class TriggerRequest : Request
    {
        private string[] actionPathSegments;

        /// <summary>
        /// Initializes a new instance of the <see cref="TriggerRequest"/> class.
        /// </summary>
        /// <param name="url">The url used to trigger this request.</param>
        /// <param name="httpVerb">The http method used to trigger this request.</param>
        /// <param name="remoteAddress">The remote IP address of this request.</param>
        /// <param name="headers">A collection of headers obtained for this request.</param>
        /// <param name="getParameters">A collection of get parameters, if any.</param>
        /// <param name="referrer">The referrer value of the HTTP request.</param>
        /// <param name="contentType">The type of content for this request, if any.</param>
        /// <param name="content">The content of this request, if any.</param>
        public TriggerRequest(
            string url,
            string httpVerb,
            string remoteAddress,
            Dictionary<string, StringValues> headers,
            string referrer = null,
            string contentType = null,
            string characterSet = null,
            object content = null)
            : base(url, httpVerb, headers, contentType, characterSet, content)
        {
            this.RemoteAddress = remoteAddress;
            this.Referrer = referrer;
        }

        [JsonConstructor]
        public TriggerRequest(string url, string httpVerb)
            : base(url, httpVerb)
        {
        }

        /// <summary>
        /// Gets the query parameters of the request, if any.
        /// </summary>
        [JsonProperty("getParameters", NullValueHandling = NullValueHandling.Ignore)]
        [Obsolete("Use QueryParameters instead.")]
        public JObject GetParameters => this.QueryParameters;

        /// <summary>
        /// Gets the path parameters of the request, if any.
        /// </summary>
        [JsonProperty("pathParameters", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, string> PathParameters { get; private set; }

        /// <summary>
        /// Gets the remote IP address of the HTTP request.
        /// </summary>
        [JsonProperty("remoteAddress", Required = Required.Always)]
        public string RemoteAddress { get; private set; }

        [JsonProperty("actionPathSegments", Required = Required.Always)]
        public string[] ActionPathSegments
        {
            get
            {
                if (this.actionPathSegments == null && this.PathSegments != null)
                {
                    this.actionPathSegments = this.GetActionPathSegments(this.PathSegments);
                }

                return this.actionPathSegments;
            }

            set
            {
                // this property doesn't need to be deserialized, but it's useful for object path lookups.
            }
        }

        [JsonProperty("actionPath", Required = Required.Always)]
        public string ActionPath
        {
            get => this.ActionPathSegments != null ? string.Join("/", this.ActionPathSegments) : null;
            set
            {
                // this property doesn't need to be deserialized, but it's useful for object path lookups.
            }
        }

        /// <summary>
        /// Gets the referrer value of the HTTP request.
        /// </summary>
        [JsonProperty("referrer", NullValueHandling = NullValueHandling.Ignore)]
        public string Referrer { get; private set; }

        public void DetectPathParameters(string endpointPath)
        {
            var pathParameters = new Dictionary<string, string>();
            var endpointSegments = new Path(endpointPath).Segments;
            if (endpointSegments.Length > 0)
            {
                for (int i = 0; i < endpointSegments.Length; i++)
                {
                    var endpointSegment = endpointSegments[i];

                    // check if the path segment is a token. Example: "{entityType}"
                    if (this.IsParameter(endpointSegment))
                    {
                        // remove the curly braces to get the token name. Exammple: "{entityType}" into "entityType"
                        var tokenName = endpointSegment.Substring(1, endpointSegment.Length - 2);
                        if (this.ActionPathSegments.Length > i)
                        {
                            pathParameters.Add(
                                tokenName,
                                this.ActionPathSegments[i].ToString());
                        }
                    }
                }
            }

            this.PathParameters = pathParameters;
        }

        private bool IsParameter(string segment)
        {
            return segment.StartsWith("{") && segment.EndsWith("}");
        }

        /// <summary>
        /// Gets the path segments after the "automations" or "automation" segment.
        /// </summary>
        /// <returns>An array of the path segments.</returns>
        private string[] GetActionPathSegments(string[] fullPathSegments)
        {
            int startIndex = Array.FindIndex(fullPathSegments, v => v == "automations" || v == "automation") + 1;
            return fullPathSegments.Skip(startIndex).ToArray();
        }
    }
}
