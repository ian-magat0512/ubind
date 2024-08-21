// <copyright file="HttpEventExporterAction.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Export
{
    using System;
    using System.Threading.Tasks;
    using Flurl;
    using Flurl.Http;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Exporter that exports via HTTP post.
    /// </summary>
    public class HttpEventExporterAction : EventExporterAction
    {
        private readonly ILogger logger;
        private readonly ITextProvider urlProvider;
        private readonly ITextProvider payloadProvider;
        private readonly ITextProvider contentTypeHeaderProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpEventExporterAction"/> class.
        /// </summary>
        /// <param name="urlProvider">For providing the URL to post to.</param>
        /// <param name="payloadProvider">For providing the post request body.</param>
        /// <param name="contentTypeHeaderProvider">For providing the post request content type header.</param>
        /// <param name="logger">Logger.</param>
        public HttpEventExporterAction(
            ITextProvider urlProvider,
            ITextProvider payloadProvider,
            ITextProvider contentTypeHeaderProvider,
            ILogger logger)
        {
            this.urlProvider = urlProvider;
            this.payloadProvider = payloadProvider;
            this.contentTypeHeaderProvider = contentTypeHeaderProvider;
            this.logger = logger;
        }

        /// <summary>
        /// Handle the event by sending an HTTP post.
        /// </summary>
        /// <param name="applicationEvent">The event to handle.</param>
        /// <returns>An awaitable task.</returns>
        public override async Task HandleEvent(Domain.ApplicationEvent applicationEvent)
        {
            var urlString = await this.urlProvider.Invoke(applicationEvent);
            var url = new Url(urlString);
            var payload = await this.payloadProvider.Invoke(applicationEvent);
            var contentType = await this.contentTypeHeaderProvider.Invoke(applicationEvent);
            this.logger.LogInformation("Http integration:");
            this.logger.LogInformation($" - URL: {url}");
            this.logger.LogInformation($" - content type: {contentType}");
            this.logger.LogInformation($" - payload: {payload}");

            if (contentType == ContentTypes.Json)
            {
                var response = await url.PostJsonAsync(payload);
            }
            else if (contentType == ContentTypes.Urlencoded)
            {
                var response = await url.PostUrlEncodedAsync(payload);
            }
            else if (contentType == ContentTypes.PlainText)
            {
                var response = await url.PostStringAsync(payload);
            }
            else
            {
                throw new NotSupportedException("Unsupported content-type for HTTP export: " + contentType);
            }
        }
    }
}
