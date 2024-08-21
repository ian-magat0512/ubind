// <copyright file="HttpResponse.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Http
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Primitives;
    using MoreLinq;
    using StackExchange.Profiling;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers;

    /// <summary>
    /// Represents the response to an inbound HTTP request.
    /// </summary>
    public class HttpResponse
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HttpResponse"/> class.
        /// </summary>
        /// <param name="httpStatusCodeProvider">The http status code provider.</param>
        /// <param name="headersProvider">The headers provider.</param>
        /// <param name="contentTypeProvider">The content type provider.</param>
        /// <param name="contentProvider">The content provider.</param>
        public HttpResponse(
            IProvider<Data<long>>? httpStatusCodeProvider,
            IEnumerable<IProvider<KeyValuePair<string, IEnumerable<string>>>>? headersProvider,
            IProvider<Data<string>>? contentTypeProvider,
            ContentProvider? contentProvider)
        {
            this.HttpStatusCode = httpStatusCodeProvider;
            this.Headers = headersProvider;
            this.ContentType = contentTypeProvider;
            this.Content = contentProvider;
        }

        /// <summary>
        /// Gets the HTTP status code to be set for the HTTP response.
        /// </summary>
        public IProvider<Data<long>>? HttpStatusCode { get; }

        /// <summary>
        /// Gets the headers in the HTTP response.
        /// </summary>
        public IEnumerable<IProvider<KeyValuePair<string, IEnumerable<string>>>>? Headers { get; }

        /// <summary>
        /// Gets the content-type of the HTTP response.
        /// </summary>
        public IProvider<Data<string>>? ContentType { get; }

        /// <summary>
        /// Gets the content of the HTTP response. It should match the content-type.
        /// </summary>
        public ContentProvider? Content { get; }

        /// <summary>
        /// Generate the response based on the configuration and current context.
        /// </summary>
        /// <param name="providerContext">The data and path to perform resolutions with.</param>
        /// <returns>An object containing the values of the invoked providers.</returns>
        public async Task<Response> GenerateResponseAsync(IProviderContext providerContext)
        {
            using (MiniProfiler.Current.Step(nameof(HttpResponse) + "." + nameof(this.GenerateResponseAsync)))
            {
                var headers = await this.ResolveHeaders(providerContext);
                var content = (await this.Content.ResolveValueIfNotNull(providerContext))?.GetValueFromGeneric();
                var characterSet = content is string
                    ? Encoding.UTF8.WebName
                    : null;
                int? statusCode = (int?)(await this.HttpStatusCode.ResolveValueIfNotNull(providerContext))?.DataValue;
                var contentType = (await this.ContentType.ResolveValueIfNotNull(providerContext))?.DataValue;
                return new Response(
                    statusCode ?? 200,
                    null,
                    headers,
                    contentType,
                    characterSet,
                    content);
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
