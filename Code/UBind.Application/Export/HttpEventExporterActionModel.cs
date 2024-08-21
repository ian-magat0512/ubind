// <copyright file="HttpEventExporterActionModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Export
{
    using UBind.Domain.Configuration;

    /// <summary>
    /// Model for email action.
    /// </summary>
    public class HttpEventExporterActionModel : IExporterModel<EventExporterAction>
    {
        /// <summary>
        /// Gets or sets the text provider for providing the URL.
        /// </summary>
        public IExporterModel<ITextProvider> Url { get; set; }

        /// <summary>
        /// Gets or sets the text provider for providing the HTTP post request body.
        /// </summary>
        public IExporterModel<ITextProvider> Body { get; set; }

        /// <summary>
        /// Gets or sets the text provider for providing the HTTP request content type header.
        /// </summary>
        public IExporterModel<ITextProvider> ContentType { get; set; }

        /// <summary>
        /// Build the HTTP action.
        /// </summary>
        /// <param name="dependencyProvider">Container for dependencies required for exporter building.</param>
        /// <param name="productConfiguration">Contains per-product configuration.</param>
        /// <returns>
        /// An HTTP action that can make an HTTP post in response to an application event.
        /// .</returns>
        public EventExporterAction Build(IExporterDependencyProvider dependencyProvider, IProductConfiguration productConfiguration)
        {
            var action = new HttpEventExporterAction(
                this.Url.Build(dependencyProvider, productConfiguration),
                this.Body.Build(dependencyProvider, productConfiguration),
                this.ContentType.Build(dependencyProvider, productConfiguration),
                dependencyProvider.Logger);
            return action;
        }
    }
}
