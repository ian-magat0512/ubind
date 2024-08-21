// <copyright file="WebServiceIntegrationModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Export
{
    using System.Collections.Generic;
    using System.Linq;
    using UBind.Application.Export.WebServiceTextProviders;
    using UBind.Domain.Configuration;

    /// <summary>
    /// For building service integration handlers from deserialized json.
    /// </summary>
    public class WebServiceIntegrationModel : IExporterModel<WebServiceIntegration>
    {
        /// <summary>
        /// Gets or sets the identifier for the integration that is unique within the current config.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the type of request to be made for the web integration.
        /// </summary>
        public IExporterModel<IWebServiceTextProvider> RequestType { get; set; }

        /// <summary>
        /// Gets or sets the text provider for obtaining the request URL.
        /// </summary>
        public IExporterModel<IWebServiceTextProvider> Url { get; set; }

        /// <summary>
        /// Gets or sets the text provider for obtaining the authentication method to be used.
        /// </summary>
        public AuthMethod AuthMethod { get; set; }

        /// <summary>
        /// Gets or sets a collection of headers for the request.
        /// </summary>
        public IEnumerable<IExporterModel<IWebServiceTextProvider>> Headers { get; set; }

        /// <summary>
        /// Gets or  sets the DotLiquid template provider for forming the request body.
        /// </summary>
        public IExporterModel<IWebServiceTextProvider> RequestTemplate { get; set; }

        /// <summary>
        /// Gets or sets the DotLiquid template provider for parsing the response content.
        /// </summary>
        public IExporterModel<IWebServiceTextProvider> ResponseTemplate { get; set; }

        /// <summary>
        /// Creates an instance of a <see cref="WebServiceIntegration"/>.
        /// </summary>
        /// <param name="dependencyProvider">Container for dependencies required for exporter building.</param>
        /// <param name="productConfiguration">The product configuration.</param>
        /// <returns>The web service integration.</returns>
        public WebServiceIntegration Build(
            IExporterDependencyProvider dependencyProvider,
            IProductConfiguration productConfiguration)
        {
            IList<IWebServiceTextProvider> headers = new List<IWebServiceTextProvider>();

            foreach (IExporterModel<IWebServiceTextProvider> header in this.Headers
                ?? Enumerable.Empty<IExporterModel<IWebServiceTextProvider>>())
            {
                IWebServiceTextProvider textProvider = header?.Build(dependencyProvider, productConfiguration);
                if (textProvider != null)
                {
                    headers.Add(textProvider);
                }
            }

            return new WebServiceIntegration(
                this.Id,
                this.Url.Build(dependencyProvider, productConfiguration),
                this.RequestType.Build(dependencyProvider, productConfiguration),
                headers,
                this.AuthMethod,
                this.RequestTemplate?.Build(dependencyProvider, productConfiguration),
                this.ResponseTemplate?.Build(dependencyProvider, productConfiguration));
        }
    }
}
