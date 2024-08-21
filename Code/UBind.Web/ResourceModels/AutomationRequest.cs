// <copyright file="AutomationRequest.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Primitives;
    using MoreLinq;
    using MoreLinq.Extensions;
    using Newtonsoft.Json.Linq;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Http;
    using UBind.Application.ExtensionMethods;
    using UBind.Domain.Extensions;

    /// <summary>
    /// Contains the http request obtained from the current http context.
    /// </summary>
    public class AutomationRequest
    {
        private readonly HttpRequest request;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutomationRequest"/> class.
        /// </summary>
        /// <param name="context">The controller context.</param>
        public AutomationRequest(HttpContext context)
        {
            this.request = context.Request;
        }

        /// <summary>
        /// Creates an instance of <see cref="TriggerRequest"/> out of this model.
        /// </summary>
        /// <param name="clientIpHeaderCode">The code used in the client IP header name.</param>
        /// <returns>A request.</returns>
        public async Task<TriggerRequest> ToTriggerRequest(string clientIpHeaderCode)
        {
            var url = this.request.GetFullUrl();
            var remoteAddress = this.request.HttpContext.GetClientIPAddress(clientIpHeaderCode).ToString();
            this.request.Headers.TryGetValue("Referrer", out StringValues referrer);
            ContentPart content = await this.ReadRequestContent();
            Dictionary<string, StringValues> headerDictionary = new Dictionary<string, StringValues>();
            foreach (var header in this.request.Headers)
            {
                string key = header.Key.ToLower();
                if (!headerDictionary.ContainsKey(key))
                {
                    var valueList = header.Value.ToString().Split(',');
                    if (valueList.Length > 1)
                    {
                        headerDictionary.Add(key, valueList);
                    }
                    else
                    {
                        headerDictionary.Add(key, header.Value);
                    }
                }
                else
                {
                    StringValues existingValue = headerDictionary[key];
                    StringValues valueList = StringValues.Concat(existingValue, header.Value);
                    headerDictionary[key] = valueList;
                }
            }

            return new TriggerRequest(
                url,
                this.request.Method,
                remoteAddress,
                headerDictionary,
                referrer,
                content?.ContentType,
                content?.CharacterSet,
                content?.Content);
        }

        private static byte[] HexStringToByteArray(string hex)
        {
            int numberChars = hex.Length;
            byte[] bytes = new byte[numberChars / 2];
            for (int i = 0; i < numberChars; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }

            return bytes;
        }

        private async Task<ContentPart> ReadRequestContent()
        {
            this.request.EnableBuffering();
            var httpContent = (HttpContent)new StreamContent(this.request.Body);

            string contentTypeString = this.request.Headers.Where(h => h.Key == "Content-Type").LastOrDefault().Value;
            if (contentTypeString != null)
            {
                httpContent.Headers.ContentType = MediaTypeHeaderValue.Parse(contentTypeString);
            }

            string contentDispositionString
                = this.request.Headers.Where(h => h.Key == "Content-Disposition").LastOrDefault().Value;
            if (contentDispositionString != null)
            {
                httpContent.Headers.ContentDisposition = ContentDispositionHeaderValue.Parse(contentDispositionString);
            }

            string contentLengthString
                = this.request.Headers.Where(h => h.Key == "Content-Length").LastOrDefault().Value;
            if (contentLengthString != null)
            {
                httpContent.Headers.ContentLength = long.Parse(contentLengthString);
            }

            string contentLocationString
                = this.request.Headers.Where(h => h.Key == "Content-Location").LastOrDefault().Value;
            if (contentLocationString != null)
            {
                httpContent.Headers.ContentLocation = new Uri(contentLocationString);
            }

            string contentMd5String
                = this.request.Headers.Where(h => h.Key == "Content-MD5").LastOrDefault().Value;
            if (contentMd5String != null)
            {
                httpContent.Headers.ContentMD5 = HexStringToByteArray(contentMd5String);
            }

            string contentRangeString
                = this.request.Headers.Where(h => h.Key == "Content-Range").LastOrDefault().Value;
            if (contentRangeString != null)
            {
                httpContent.Headers.ContentRange = ContentRangeHeaderValue.Parse(contentRangeString);
            }

            var contentPart = await ContentPart.FromHttpContent(httpContent);
            return contentPart;
        }

        private JObject ParseQueryParameters() =>
            new JObject(this.request.Query.Select(kvp => new JProperty(kvp.Key, kvp.Value.ToString())));
    }
}
