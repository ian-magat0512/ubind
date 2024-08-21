// <copyright file="Response.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Http
{
    using System.Collections.Generic;
    using Microsoft.Extensions.Primitives;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the response to a HTTP request or the resolved values for <see cref="HttpResponse"/>.
    /// </summary>
    public class Response
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Response"/> class.
        /// </summary>
        /// <param name="httpStatusCode">The status code returned by the response.</param>
        /// <param name="reasonPhrase">The text that comes along with the status code.</param>
        /// <param name="headers">The headers that are part of the response message.</param>
        /// <param name="contentType">The IANA content type, if any, of the response, e.g. "application/json".</param>
        /// <param name="characterSet">The IANA character set, e.g. "UTF-8". Only applicable if it's a text MIME type.
        /// </param>
        /// <param name="content">The body content, as either a string, byte[] or a list of <see cref="ContentPart"/>.</param>
        public Response(
            long httpStatusCode,
            string? reasonPhrase,
            Dictionary<string, StringValues>? headers,
            string? contentType,
            string? characterSet,
            object? content)
        {
            this.HttpStatusCode = httpStatusCode;
            this.ReasonPhrase = reasonPhrase;
            this.Headers = headers;
            this.ContentType = contentType;
            this.CharacterSet = characterSet;
            this.Content = content;
        }

        [JsonConstructor]
        private Response()
        {
        }

        /// <summary>
        /// Gets the HTTP status code in the response.
        /// </summary>
        [JsonProperty(PropertyName = "httpStatusCode")]
        public long HttpStatusCode { get; private set; }

        /// <summary>
        /// Gets the text which comes along with the status code.
        /// </summary>
        [JsonProperty(PropertyName = "reasonPhrase")]
        public string? ReasonPhrase { get; private set; }

        /// <summary>
        /// Gets the headers in the response.
        /// </summary>
        [JsonProperty(PropertyName = "headers")]
        [JsonConverter(typeof(HeadersConverter))]
        public Dictionary<string, StringValues>? Headers { get; private set; }

        /// <summary>
        /// Gets the content type of the response.
        /// </summary>
        [JsonProperty(PropertyName = "contentType")]
        public string? ContentType { get; private set; }

        /// <summary>
        /// Gets the character set of the response (e.g. "UTF-8").
        /// This is only set for text based content types.
        /// </summary>
        [JsonProperty(PropertyName = "characterSet")]
        public string? CharacterSet { get; private set; }

        /// <summary>
        /// Gets the content of the response. This could be either a string, a byte array,
        /// or a list of MIME multipart as instances of <see cref="ContentPart"/>.
        /// If the content type is a text MIME type, then this will be a string. If it's binary it
        /// will be a byte array. If it's a multi-part response then this will be an list of parts
        /// with each part having a ContentType, CharacterSet, and Content property.
        /// </summary>
        [JsonProperty(PropertyName = "content")]
        [JsonConverter(typeof(HttpContentJsonConverter))]
        public object? Content { get; private set; }
    }
}
