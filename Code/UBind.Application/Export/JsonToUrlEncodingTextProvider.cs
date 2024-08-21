// <copyright file="JsonToUrlEncodingTextProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Export
{
    using System.Net;
    using System.Threading.Tasks;
    using UBind.Domain;

    /// <summary>
    /// For converting a JSON string from a text provider to a URL encoded string.
    /// </summary>
    public class JsonToUrlEncodingTextProvider : ITextProvider
    {
        private readonly ITextProvider jsonProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonToUrlEncodingTextProvider"/> class.
        /// </summary>
        /// <param name="jsonProvider">Text provider that will provider string containing JSON.</param>
        public JsonToUrlEncodingTextProvider(ITextProvider jsonProvider)
        {
            this.jsonProvider = jsonProvider;
        }

        /// <inheritdoc/>
        public async Task<string> Invoke(ApplicationEvent applicationEvent)
        {
            var json = await this.jsonProvider.Invoke(applicationEvent);
            var urlEncodedJson = WebUtility.UrlEncode(json);
            return urlEncodedJson;
        }
    }
}
