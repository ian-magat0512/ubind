// <copyright file="EventExporterModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Export
{
    using System.Collections.Generic;
    using UBind.Domain;
    using UBind.Domain.Configuration;

    /// <summary>
    /// For building event handlers from deserialized json.
    /// </summary>
    public class EventExporterModel : IExporterModel<EventExporter>
    {
        /// <summary>
        /// Gets or sets an identifier for the exporter that is unique within the current config for a product.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the types of event that should be handled.
        /// </summary>
        public IEnumerable<ApplicationEventType> Events { get; set; }

        /// <summary>
        /// Gets or sets the action that should be triggered in response to a relevant event.
        /// </summary>
        public IExporterModel<EventExporterAction> Action { get; set; }

        /// <summary>
        /// Gets or sets the condition that should be triggered in response to a relevant event.
        /// </summary>
        public IExporterModel<EventExporterCondition> Condition { get; set; }

        /// <summary>
        /// Instantiates a new event handler from the deserialized json.
        /// </summary>
        /// <param name="dependencyProvider">Container providing dependencies required in exporter building.</param>
        /// <param name="productConfiguration">Contains the per-product configuration.</param>
        /// <returns>A new event handler.</returns>
        public EventExporter Build(IExporterDependencyProvider dependencyProvider, IProductConfiguration productConfiguration)
        {
            return new EventExporter(
                this.Id, this.Events, this.Action.Build(dependencyProvider, productConfiguration), this.Condition?.Build(dependencyProvider, productConfiguration), dependencyProvider.Logger);
        }
    }
}
