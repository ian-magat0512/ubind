// <copyright file="IntegrationConfiguration.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Export
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using CSharpFunctionalExtensions;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Configuration;
    using UBind.Domain.Exceptions;

    /// <summary>
    /// Configuration for integrations.
    /// </summary>
    public class IntegrationConfiguration
    {
        private readonly IEnumerable<EventExporter> eventExporters;
        private readonly IEnumerable<WebServiceIntegration> webServiceIntegrations;

        /// <summary>
        /// Initializes a new instance of the <see cref="IntegrationConfiguration"/> class.
        /// </summary>
        /// <param name="eventExporters">Event exporters.</param>
        /// <param name="webServiceIntegrations">Web Service Integrations.</param>
        public IntegrationConfiguration(IEnumerable<EventExporter> eventExporters, IEnumerable<WebServiceIntegration> webServiceIntegrations)
        {
            this.eventExporters = eventExporters;
            var duplicateIds = this.eventExporters
                .GroupBy(e => e.Id)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key);
            if (duplicateIds.Any())
            {
                throw new ErrorException(Errors.Integrations.DuplicateIds(duplicateIds, "event exporters"));
            }

            this.webServiceIntegrations = webServiceIntegrations;
            duplicateIds = this.webServiceIntegrations
                .GroupBy(e => e.Id)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key);
            if (duplicateIds.Any())
            {
                throw new ErrorException(Errors.Integrations.DuplicateIds(duplicateIds, "web service"));
            }
        }

        /// <summary>
        /// Get the IDs for any exporters that can handle a given event type.
        /// </summary>
        /// <param name="eventType">The type of the event.</param>
        /// <returns>The IDs of any exporters that can handle the event type.</returns>
        public IEnumerable<string> GetExportersForEvent(ApplicationEventType eventType)
        {
            return this.eventExporters
                .Where(exporter => exporter.CanHandleEvent(eventType))
                .Select(exporter => exporter.Id);
        }

        /// <summary>
        /// get the event exporter for operation id.
        /// </summary>
        /// <param name="operationId">operationid.</param>
        /// <returns>the result.</returns>
        public Result<EventExporter, Error> GetEventExporterForOperationId(string operationId)
        {
            var exporter = this.eventExporters.FirstOrDefault(exp => exp.Id == operationId);
            if (exporter == null)
            {
                return Result.Failure<EventExporter, Error>(Errors.Operations.NotFound(operationId));
            }

            return Result.Success<EventExporter, Error>(exporter);
        }

        /// <summary>
        /// Gets all the IDs of the available web integrations.
        /// </summary>
        /// <returns>The IDs of the web integrations for the given integration file.</returns>
        public IEnumerable<string> GetWebIntegrations()
        {
            return this.webServiceIntegrations
                .Select(integration => integration.Id);
        }

        /// <summary>
        /// Triggers a given exporter for a given application event.
        /// </summary>
        /// <param name="exporterId">The ID of the exporter to trigger.</param>
        /// <param name="applicationEvent">The application event.</param>
        /// <returns>An awaitable task.</returns>
        public async Task<EventExporter> ExecuteIntegration(string exporterId, ApplicationEvent applicationEvent)
        {
            var exporter = this.eventExporters.FirstOrDefault(exp => exp.Id == exporterId);
            if (exporter == null)
            {
                throw new ErrorException(Errors.General.NotFound("event exporter", exporterId));
            }

            await exporter.HandleEvent(applicationEvent);

            return exporter;
        }

        /// <summary>
        /// Executes a given web integration ID for a given quote.
        /// </summary>
        /// <param name="webIntegrationId">The ID of the web integration to execute.</param>
        /// <param name="payloadJson">The string payload.</param>
        /// <param name="quoteAggregate">The quote aggregate the integration is for.</param>
        /// <param name="productConfiguration">The product configuration for the given quote.</param>
        /// <param name="quoteId">The ID of the quote.</param>
        /// <returns>A web integration response.</returns>
        public async Task<WebServiceIntegrationResponse> ExecuteIntegration(
            string webIntegrationId,
            string payloadJson,
            QuoteAggregate quoteAggregate,
            IProductConfiguration productConfiguration,
            Guid quoteId)
        {
            var webIntegration = this.webServiceIntegrations.FirstOrDefault(wi => wi.Id == webIntegrationId);
            if (webIntegration == null)
            {
                throw new ErrorException(Errors.General.NotFound("web integration", webIntegrationId));
            }

            var response = await webIntegration.Execute(payloadJson, quoteAggregate, productConfiguration, quoteId);
            return response;
        }
    }
}
