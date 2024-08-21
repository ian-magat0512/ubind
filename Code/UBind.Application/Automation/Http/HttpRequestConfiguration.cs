// <copyright file="HttpRequestConfiguration.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Http
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Primitives;
    using StackExchange.Profiling;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers;

    /// <summary>
    /// Represents the HTTP request.
    /// </summary>
    public class HttpRequestConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HttpRequestConfiguration"/> class.
        /// </summary>
        /// <param name="url">The url path of the request.</param>
        /// <param name="httpVerb">The http verb.</param>
        /// <param name="headers">The headers of the http request.</param>
        /// <param name="contentType">The content type of http request.</param>
        /// <param name="content">The content of the http request.</param>
        public HttpRequestConfiguration(
            IProvider<Data<string>> url,
            IProvider<Data<string>>? httpVerb,
            IEnumerable<IProvider<KeyValuePair<string, IEnumerable<string>>>>? headers,
            IProvider<Data<string>>? contentType,
            IProvider<Data<string>>? characterSet,
            ContentProvider? content,
            ClientCertificate? clientCertificate)
        {
            this.Url = url;
            this.HttpVerb = httpVerb;
            this.Headers = headers;
            this.Content = content;
            this.ContentType = contentType;
            this.CharacterSet = characterSet;
            this.ClientCertificate = clientCertificate;
        }

        /// <summary>
        /// Gets the URL of http request.
        /// </summary>
        /// <remarks>This value must be unique per tenant and per HTTP verb.</remarks>
        public IProvider<Data<string>> Url { get; }

        /// <summary>
        /// Gets the HTTP verb that the http request will respond to. Defaults to GET.
        /// </summary>
        public IProvider<Data<string>>? HttpVerb { get; }

        /// <summary>
        /// Gets the headers in the HTTP request.
        /// </summary>
        public IEnumerable<IProvider<KeyValuePair<string, IEnumerable<string>>>>? Headers { get; }

        /// <summary>
        /// Gets the content-type of the HTTP request.
        /// </summary>
        public IProvider<Data<string>>? ContentType { get; }

        /// <summary>
        /// Gets the character set for the content, if applicable.
        /// </summary>
        public IProvider<Data<string>>? CharacterSet { get; }

        /// <summary>
        /// Gets the content of the HTTP request. It should match the content-type.
        /// </summary>
        /// <remarks>For MVP this is made to be only a text provider to match required scope.</remarks>
        public ContentProvider? Content { get; }

        /// <summary>
        /// Gets the client certificate of the request.
        /// </summary>
        /// <remarks>For MVP this is made to be only a text provider to match required scope.</remarks>
        public ClientCertificate? ClientCertificate { get; }

        /// <summary>
        /// Generates a request from the configured properties.
        /// </summary>
        /// <param name="providerContext">The data and path to perform resolutions with.</param>
        /// <returns>A resolved request.</returns>
        public async Task<Request> GenerateRequest(IProviderContext providerContext)
        {
            using (MiniProfiler.Current.Step(nameof(HttpRequestConfiguration) + "." + nameof(this.GenerateRequest)))
            {
                var url = (await this.Url.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue;
                var headers = await this.ResolveHeaders(providerContext);
                string? contentType = (await this.ContentType.ResolveValueIfNotNull(providerContext))?.DataValue;
                string? characterSet = (await this.CharacterSet.ResolveValueIfNotNull(providerContext))?.DataValue;
                if (contentType == null || characterSet == null)
                {
                    (string? contentTypeFromHeaders, string? characterSetFromHeaders) =
                        HttpContentBuilder.GetContentTypeAndCharacterSetFromHeaders(headers) ?? default;
                    contentType = contentType ?? contentTypeFromHeaders;
                    characterSet = characterSet ?? characterSetFromHeaders;
                }

                var content = (await this.Content.ResolveValueIfNotNull(providerContext))?.GetValueFromGeneric();
                ClientCertificateData? clientCertificate = this.ClientCertificate != null
                   ? await this.ClientCertificate.Resolve(providerContext)
                   : null;
                var httpVerb = (await this.HttpVerb.ResolveValueIfNotNull(providerContext))?.DataValue ?? "GET";
                return new Request(
                    url,
                    httpVerb,
                    headers,
                    contentType,
                    characterSet,
                    content,
                    clientCertificate);
            }
        }

        private async Task<Dictionary<string, StringValues>> ResolveHeaders(IProviderContext providerContext)
        {
            Dictionary<string, StringValues> headers = new Dictionary<string, StringValues>();
            if (this.Headers != null)
            {
                foreach (var pair in this.Headers)
                {
                    var header = (await pair.Resolve(providerContext)).GetValueOrThrowIfFailed();
                    if (header.Value.Count() > 1)
                    {
                        headers.Add(header.Key, new StringValues(header.Value.ToArray<string>()));
                    }
                    else
                    {
                        headers.Add(header.Key, header.Value.First());
                    }
                }
            }

            return headers;
        }
    }
}
