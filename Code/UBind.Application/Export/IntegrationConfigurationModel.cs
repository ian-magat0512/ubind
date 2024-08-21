// <copyright file="IntegrationConfigurationModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Export
{
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;
    using UBind.Domain;
    using UBind.Domain.Configuration;

    /// <summary>
    /// Configuration for integrations.
    /// </summary>
    public class IntegrationConfigurationModel : IExporterModel<IntegrationConfiguration>
    {
        /// <summary>
        /// Gets or sets configuration for Event handlers.
        /// </summary>
        [JsonProperty(PropertyName = "EventExporters")]
        public IEnumerable<EventExporterModel> EventExporterModels { get; set; } = Enumerable.Empty<EventExporterModel>();

        /// <summary>
        /// Gets or sets configuration for web service integration request handlers.
        /// </summary>
        [JsonProperty(PropertyName = "WebServiceIntegrations")]
        public IEnumerable<WebServiceIntegrationModel> WebServiceIntegrationModels { get; set; } = Enumerable.Empty<WebServiceIntegrationModel>();

        /// <summary>
        /// Create a new instance of the <see cref="IntegrationConfiguration" /> class from the model.
        /// </summary>
        /// <param name="dependencyProvider">Container providing dependencies required in exporter building.</param>
        /// <param name="productConfiguration">The product configuation.</param>
        /// <returns>A new instance of the <see cref="IntegrationConfiguration" /> class.</returns>
        public IntegrationConfiguration Build(IExporterDependencyProvider dependencyProvider, IProductConfiguration productConfiguration = null)
        {
            var config = new IntegrationConfiguration(
                this.EventExporterModels.Select(model => model.Build(dependencyProvider, productConfiguration)),
                this.WebServiceIntegrationModels.Select(model => model.Build(dependencyProvider, productConfiguration)));
            return config;
        }

        /// <summary>
        /// Get exporters for the event type.
        /// </summary>
        /// <param name="eventType">The event type.</param>
        /// <returns>The list of exporters for the event type.</returns>
        public IEnumerable<EventExporterModel> GetExportersForEvent(ApplicationEventType eventType)
        {
            return this.EventExporterModels?
                .Where(e => e.Events.Contains(eventType));
        }
    }
}
