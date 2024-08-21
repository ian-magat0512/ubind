// <copyright file="UrlWebServiceTextProviderModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Export.WebServiceTextProviders
{
    using System.Collections.Generic;
    using System.Linq;
    using UBind.Domain.Configuration;

    /// <summary>
    /// Model for Url text provider.
    /// </summary>
    public class UrlWebServiceTextProviderModel : IExporterModel<IWebServiceTextProvider>
    {
        /// <summary>
        /// Gets or sets the url text provider.
        /// </summary>
        public IExporterModel<IWebServiceTextProvider> UrlString { get; set; }

        /// <summary>
        /// Gets or sets the GET request path parameter provider, if any.
        /// </summary>
        public IExporterModel<IWebServiceTextProvider> PathParameter { get; set; }

        /// <summary>
        /// Gets or sets the GET request query parameter providers, if any.
        /// </summary>
        public IEnumerable<IExporterModel<IWebServiceTextProvider>> QueryParameters { get; set; }

        /// <inheritdoc />
        public IWebServiceTextProvider Build(IExporterDependencyProvider dependencyProvider, IProductConfiguration productConfiguration)
        {
            IList<IWebServiceTextProvider> queryParams = new List<IWebServiceTextProvider>();

            foreach (IExporterModel<IWebServiceTextProvider> queryParam in this.QueryParameters
                ?? Enumerable.Empty<IExporterModel<IWebServiceTextProvider>>())
            {
                IWebServiceTextProvider textProvider = queryParam?.Build(dependencyProvider, productConfiguration);
                if (textProvider != null)
                {
                    queryParams.Add(textProvider);
                }
            }

            return new UrlWebServiceTextProvider(
                this.UrlString.Build(dependencyProvider, productConfiguration),
                this.PathParameter?.Build(dependencyProvider, productConfiguration),
                queryParams);
        }
    }
}
