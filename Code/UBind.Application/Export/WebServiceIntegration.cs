// <copyright file="WebServiceIntegration.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Export
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Flurl.Http;
    using UBind.Application.Export.Enums;
    using UBind.Application.Export.WebServiceTextProviders;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Configuration;

    /// <inheritdoc/>
    public class WebServiceIntegration : IWebServiceIntegration
    {
        private readonly IWebServiceTextProvider urlProvider;
        private readonly IWebServiceTextProvider requestTypeProvider;
        private readonly IEnumerable<IWebServiceTextProvider> headersProvider;
        private readonly IAuthMethod authMethodProvider;
        private readonly IWebServiceTextProvider requestBodyProvider;
        private readonly IWebServiceTextProvider responsePayloadProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebServiceIntegration"/> class.
        /// </summary>
        /// <param name="id">For providing the web service integration's unique id.</param>
        /// <param name="urlProvider">For providing the URL to request on.</param>
        /// <param name="requestTypeProvider">For providing the type of request to be made.</param>
        /// <param name="headerProvider">For providing the arbitrary headers of the request.</param>
        /// <param name="authMethodProvider">For providing the method and token to be used for authentication.</param>
        /// <param name="requestBodyProvider">For providing the templated request body.</param>
        /// <param name="responsePayloadProvider">For providing the parsed response payload.</param>
        public WebServiceIntegration(
            string id,
            IWebServiceTextProvider urlProvider,
            IWebServiceTextProvider requestTypeProvider,
            IEnumerable<IWebServiceTextProvider> headerProvider,
            IAuthMethod authMethodProvider,
            IWebServiceTextProvider requestBodyProvider,
            IWebServiceTextProvider responsePayloadProvider)
        {
            Contract.Assert(id != null);
            Contract.Assert(requestTypeProvider != null);

            this.Id = id;
            this.urlProvider = urlProvider;
            this.requestTypeProvider = requestTypeProvider;
            this.headersProvider = headerProvider;
            this.authMethodProvider = authMethodProvider;
            this.requestBodyProvider = requestBodyProvider;
            this.responsePayloadProvider = responsePayloadProvider;
        }

        /// <summary>
        /// Gets the unique integration ID for the current product configuration.
        /// </summary>
        public string Id { get; }

        /// <inheritdoc/>
        public async Task<WebServiceIntegrationResponse> Execute(string payloadJson, QuoteAggregate quoteAggregate, IProductConfiguration productConfiguration, Guid quoteId)
        {
            var requestType = this.requestTypeProvider.Invoke(payloadJson, quoteAggregate, productConfiguration, quoteId);
            Enum.TryParse<HttpRequestType>(requestType, true, out HttpRequestType requestTypeOut);

            switch (requestTypeOut)
            {
                case HttpRequestType.Get:
                    {
                        return await this.GetRequestIntegration(payloadJson, quoteAggregate, productConfiguration, quoteId);
                    }

                case HttpRequestType.Post:
                    {
                        return await this.PostRequestIntegration(payloadJson, quoteAggregate, productConfiguration, quoteId);
                    }

                default: throw new NotImplementedException();
            }
        }

        private async Task<WebServiceIntegrationResponse> GetRequestIntegration(string payloadJson, QuoteAggregate quoteAggregate, IProductConfiguration productConfiguration, Guid quoteId)
        {
            var headers = this.ParseHeaders(payloadJson, quoteAggregate, productConfiguration, quoteId);
            var urlString = this.urlProvider.Invoke(payloadJson, quoteAggregate, productConfiguration, quoteId);
            HttpResponseMessage response = null;
            try
            {
                var request = this.authMethodProvider != null && this.authMethodProvider.AuthenticationType.Equals("Bearer")
                    ? urlString.WithOAuthBearerToken(this.authMethodProvider.AuthenticationToken).WithHeaders(headers)
                    : urlString.WithHeaders(headers);
                IFlurlResponse flurlResponse = await request.GetAsync();
                response = flurlResponse.ResponseMessage;

                var content = await response.Content.ReadAsStringAsync();
                var responseContent = this.responsePayloadProvider?.Invoke(
                    content, quoteAggregate, productConfiguration, quoteId);
                return new WebServiceIntegrationResponse()
                {
                    Code = response.StatusCode,
                    ResponseJson = responseContent ?? content,
                };
            }
            catch (FlurlHttpException ex)
            {
                return new WebServiceIntegrationResponse()
                {
                    Code = ex.Call.HttpResponseMessage.StatusCode,
                    ResponseJson = ex.Message,
                };
            }
        }

        private async Task<WebServiceIntegrationResponse> PostRequestIntegration(string payloadJson, QuoteAggregate quoteAggregate, IProductConfiguration productConfiguration, Guid quoteId)
        {
            var headers = this.ParseHeaders(payloadJson, quoteAggregate, productConfiguration, quoteId);
            var urlString = this.urlProvider.Invoke(payloadJson, quoteAggregate, productConfiguration, quoteId);
            var requestPayload = this.requestBodyProvider?.Invoke(payloadJson, quoteAggregate, productConfiguration, quoteId);
            HttpResponseMessage response = null;
            try
            {
                var request = this.authMethodProvider != null && this.authMethodProvider.AuthenticationType.Equals("Bearer")
                   ? urlString.WithOAuthBearerToken(this.authMethodProvider.AuthenticationToken).WithHeaders(headers)
                   : urlString.WithHeaders(headers);
                IFlurlResponse flurlResponse = await request.PostJsonAsync(requestPayload);
                response = flurlResponse.ResponseMessage;

                var content = await response.Content.ReadAsStringAsync();
                var responseContent = this.responsePayloadProvider?.Invoke(
                    content, quoteAggregate, productConfiguration, quoteId);
                return new WebServiceIntegrationResponse()
                {
                    Code = response.StatusCode,
                    ResponseJson = responseContent ?? content,
                };
            }
            catch (FlurlHttpException ex)
            {
                return new WebServiceIntegrationResponse()
                {
                    Code = ex.Call.HttpResponseMessage.StatusCode,
                    ResponseJson = ex.Message,
                };
            }
        }

        /// <summary>
        /// Parse headers to key-value pairs.
        /// </summary>
        /// <param name="payloadJson">The JSON payload.</param>
        /// <param name="quoteAggregate">The quote aggregate.</param>
        /// <param name="productConfiguration">The product configuration.</param>
        /// /// <param name="quoteId">The ID of the quote.</param>
        /// <returns>A dictionary of headers.</returns>
        private Dictionary<string, string> ParseHeaders(string payloadJson, QuoteAggregate quoteAggregate, IProductConfiguration productConfiguration, Guid quoteId)
        {
            Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();
            foreach (var headerItem in this.headersProvider)
            {
                var headerPair = headerItem.Invoke(payloadJson, quoteAggregate, productConfiguration, quoteId);
                var key = headerPair.Substring(0, headerPair.IndexOf(':'));
                keyValuePairs[key] = headerPair.Substring(headerPair.IndexOf(':') + 1);
            }

            return keyValuePairs;
        }
    }
}
