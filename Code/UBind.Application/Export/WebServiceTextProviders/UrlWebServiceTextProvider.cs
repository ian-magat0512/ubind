// <copyright file="UrlWebServiceTextProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Export.WebServiceTextProviders
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using Flurl;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Configuration;

    /// <summary>
    /// For providing the url to be used in the service.
    /// </summary>
    public class UrlWebServiceTextProvider : IWebServiceTextProvider
    {
        private IWebServiceTextProvider urlProvider;
        private IWebServiceTextProvider pathParameterProvider;
        private IEnumerable<IWebServiceTextProvider> queryParameterProviders;

        /// <summary>
        /// Initializes a new instance of the <see cref="UrlWebServiceTextProvider"/> class.
        /// </summary>
        /// <param name="urlProvider">The url text provider.</param>
        /// <param name="pathParameterProvider">The GET request path parameter text provider.</param>
        /// <param name="queryParameterProviders">The GET request query parameter text providers.</param>
        public UrlWebServiceTextProvider(
            IWebServiceTextProvider urlProvider,
            IWebServiceTextProvider pathParameterProvider,
            IEnumerable<IWebServiceTextProvider> queryParameterProviders)
        {
            Contract.Assert(urlProvider != null);
            this.urlProvider = urlProvider;
            this.pathParameterProvider = pathParameterProvider;
            this.queryParameterProviders = queryParameterProviders;
        }

        /// <inheritdoc/>
        public string Invoke(string payloadJson, QuoteAggregate quoteAggregate, IProductConfiguration productConfiguration, Guid quoteId)
        {
            var urlString = this.urlProvider.Invoke(payloadJson, quoteAggregate, productConfiguration, quoteId);
            if (this.pathParameterProvider == null && this.queryParameterProviders == null)
            {
                return new Url(urlString);
            }

            var pathParameter = this.pathParameterProvider?.Invoke(payloadJson, quoteAggregate, productConfiguration, quoteId);
            if (pathParameter != null)
            {
                urlString = Url.Combine(urlString, pathParameter);
            }

            string query = this.queryParameterProviders.FirstOrDefault()?.Invoke(payloadJson, quoteAggregate, productConfiguration, quoteId);
            foreach (var param in this.queryParameterProviders.Skip(1))
            {
                string paramResult = param?.Invoke(payloadJson, quoteAggregate, productConfiguration, quoteId);
                query = string.Concat(query, "&", paramResult);
            }

            return Url.Combine(urlString, "?", query);
        }
    }
}
