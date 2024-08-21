// <copyright file="HttpContentBuilder.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Http
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Primitives;
    using StackExchange.Profiling;
    using UBind.Application.Automation.Data;
    using UBind.Application.Automation.Enums;
    using UBind.Application.Automation.Providers;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;

    /// <summary>
    /// Builds an instance of <see cref="System.Net.Http.HttpContent">System.Net.Http.HttpContent</see> from a
    /// string, byte array, or list of <see cref="ContentPart">ContentPart</see>, so that it can be used in
    /// HTTP requests or HTTP resposnes.
    /// </summary>
    public static class HttpContentBuilder
    {
        /// <summary>
        /// Builds an instance of <see cref="System.Net.Http.HttpContent">System.Net.Http.HttpContent</see> from a
        /// string, byte array, or list of <see cref="ContentPart">ContentPart</see>, so that it can be used in
        /// HTTP requests or HTTP resposnes.
        /// </summary>
        /// <param name="contentProvided">A string, byte array of List of ContentPart.</param>
        /// <param name="contentType">The IANA content type, if any, of the response, e.g. "application/json".</param>
        /// <param name="characterSet">The IANA character set, e.g. "UTF-8". Only applicable if it's a text MIME type.
        /// </param>
        /// <param name="automationData">The automation data. This is needed in case there is an error, so that
        /// we can provide details.</param>
        /// <returns>An instance of HttpContent.</returns>
        public static async Task<HttpContent?> Build(
            object? contentProvided,
            string? contentType,
            string? characterSet,
            AutomationData automationData)
        {
            using (MiniProfiler.Current.Step(nameof(HttpContentBuilder) + "." + nameof(HttpContentBuilder.Build)))
            {
                if (contentProvided == null)
                {
                    // There's nothing to build.
                    return null;
                }

                var contentBody = contentProvided;
                if (contentBody is string textContent)
                {
                    var encoding = characterSet != null
                        ? Encoding.GetEncoding(characterSet)
                        : Encoding.UTF8;
                    return new StringContent(textContent, encoding, contentType);
                }
                else if (contentBody is IEnumerable<ContentPart> multiContent)
                {
                    MultipartContent content = new MultipartContent();
                    foreach (var contentPart in multiContent)
                    {
                        var partContent = await Build(
                            contentPart.Content,
                            contentPart.ContentType,
                            contentPart.CharacterSet,
                            automationData);
                        if (partContent != null)
                        {
                            content.Add(partContent);
                        }
                    }

                    return content;
                }
                else if (contentBody is byte[])
                {
                    var byteContent = new ByteArrayContent((contentBody as byte[])!);
                    return byteContent;
                }
                else
                {
                    var providerContext = new ProviderContext(automationData);
                    var contentBodyTypeName = contentBody.GetType().Name;
                    var errorData = await providerContext.GetDebugContext();
                    errorData.Add(ErrorDataKey.StackTrace, Environment.StackTrace);
                    throw new ErrorException(Errors.Automation.ContentBodyTypeNotSupported(contentBodyTypeName, errorData));
                }
            }
        }

        public static (string? contentType, string? characterSet)? GetContentTypeAndCharacterSetFromHeaders(
            IReadOnlyDictionary<string, StringValues> headers)
        {
            var contentTypeHeader = headers.FirstOrDefault(h => h.Key.EqualsIgnoreCase("Content-Type")).Value;
            if (contentTypeHeader.Count == 0)
            {
                return null;
            }

            string? contentTypeValue = contentTypeHeader.FirstOrDefault();
            string[]? parts = contentTypeValue == null ? null : contentTypeValue.Split(';');
            if (parts != null && parts.Length > 1)
            {
                return (parts[0], parts[1]);
            }
            else
            {
                return (contentTypeValue, null);
            }
        }
    }
}
