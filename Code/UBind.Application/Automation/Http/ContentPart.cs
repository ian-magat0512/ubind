// <copyright file="ContentPart.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Http
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using UBind.Application.Automation.Helper;
    using UBind.Domain.Extensions;

    /// <summary>
    /// Represents the details of an <see cref="HttpContent"/>.
    /// </summary>
    public class ContentPart
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContentPart"/> class.
        /// </summary>
        /// <param name="contentType">The content type.</param>
        /// <param name="content">The body content, as either a string, byte[] or a list of <see cref="ContentPart"/>.</param>
        /// <param name="characterSet">The character set, e.g. "UTF-8". Only required for text media types.</param>
        public ContentPart(object content, string contentType, string characterSet = null)
        {
            this.Content = content;
            this.ContentType = contentType;
            this.CharacterSet = characterSet;
        }

        /// <summary>
        /// Gets the mime-type to be used for the <see cref="HttpContent"/>.
        /// </summary>
        [JsonProperty("contentType")]
        public string ContentType { get; }

        /// <summary>
        /// Gets the character set of the response (e.g. "UTF-8").
        /// </summary>
        [JsonProperty("characterSet")]
        public string CharacterSet { get; private set; }

        /// <summary>
        /// Gets the body of the <see cref="HttpContent"/>.
        /// </summary>
        [JsonProperty("content")]
        [JsonConverter(typeof(HttpContentJsonConverter))]
        public object Content { get; }

        public static async Task<ContentPart> FromHttpContent(HttpContent content)
        {
            string mediaType = content.Headers.ContentType?.MediaType?.ToString();
            if (HttpHelper.IsJsonMediaType(content.Headers.ContentType))
            {
                string charset = content.Headers.ContentType.CharSet;
                JToken jToken = await ParseJsonResponseContent(content);
                if (jToken != null)
                {
                    return new ContentPart(jToken, mediaType, charset);
                }
            }
            else if (HttpHelper.IsMultipartMediaType(content.Headers.ContentType))
            {
                List<ContentPart> parts
                    = await ParseMultipartResponseContent(content as MultipartContent);
                return new ContentPart(parts, mediaType);
            }
            else if (HttpHelper.IsTextMediaType(content.Headers.ContentType))
            {
                string charset = content.Headers.ContentType.CharSet;
                string contentString = await content.ReadAsStringAsync();
                return new ContentPart(contentString, mediaType, charset);
            }

            // Treat it as binary data
            byte[] contentBytes = await content.ReadAsByteArrayAsync();
            if (contentBytes == null || contentBytes.Length == 0)
            {
                return null;
            }

            return new ContentPart(contentBytes, mediaType);
        }

        private static async Task<List<ContentPart>> ParseMultipartResponseContent(
            MultipartContent content)
        {
            return (await content.SelectAsync(async c => await FromHttpContent(c))).ToList();
        }

        private static async Task<JToken> ParseJsonResponseContent(HttpContent content)
        {
            string contentString = await content.ReadAsStringAsync();
            try
            {
                return JToken.Parse(contentString);
            }
            catch
            {
                return null;
            }
        }
    }
}
