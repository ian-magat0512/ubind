// <copyright file="EventExporter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Export
{
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using UBind.Domain;
    using UBind.Domain.Extensions;

    /// <summary>
    /// An exporter that will trigger an export action in response to given application events.
    /// </summary>
    public class EventExporter
    {
        private readonly IEnumerable<ApplicationEventType> eventTypes;
        private readonly EventExporterAction action;
        private readonly EventExporterCondition condition;
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventExporter"/> class.
        /// </summary>
        /// <param name="id">An ID to uniquely identify this exporter amongst the product's current export config.</param>
        /// <param name="eventTypes">The application events to handle.</param>
        /// <param name="action">The export action to trigger.</param>
        /// <param name="condition">The export condition to trigger.</param>
        /// <param name="logger">The logger.</param>
        public EventExporter(
            string id,
            IEnumerable<ApplicationEventType> eventTypes,
            EventExporterAction action,
            EventExporterCondition condition,
            ILogger logger)
        {
            Contract.Assert(id.IsNotNullOrWhitespace());
            this.Id = id;

            // add the ability to replay all events
            this.eventTypes = eventTypes.Append(ApplicationEventType.ReplayEvent);
            this.action = action;
            this.condition = condition;
            this.logger = logger;
        }

        /// <summary>
        /// Gets the exporter's ID (unique for current product config).
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Triggers an export action in response to an application event if it matches trigger lists.
        /// </summary>
        /// <param name="applicationEvent">The application event to handle.</param>
        /// <returns>Returns an awaitable task.</returns>
        public async Task HandleEvent(ApplicationEvent applicationEvent)
        {
            if (!this.CanHandleEvent(applicationEvent.EventType))
            {
                this.logger.LogInformation("{ApplicationEventType} not handled because it doesn't match one of the event types.", applicationEvent.EventType);
                return;
            }

            if (this.condition == null)
            {
                this.logger.LogInformation("Executing {ApplicationEventType} because there were no conditions to stop it from executing.", applicationEvent.EventType);
                await this.action.HandleEvent(applicationEvent);
                return;
            }

            bool conditionMet = await this.condition.Evaluate(applicationEvent);
            this.logger.LogInformation(this.condition.DebugInfo);
            if (conditionMet)
            {
                this.logger.LogInformation("Executing {ApplicationEventType} because the condition was met.", applicationEvent.EventType);
                await this.action.HandleEvent(applicationEvent);
            }
            else
            {
                this.logger.LogInformation("Not executing {ApplicationEventType} because the condition was not met.", applicationEvent.EventType);
            }
        }

        /// <summary>
        /// Returns a value indicating whether a particular event type can be handled by this exporter.
        /// </summary>
        /// <param name="eventType">The event type to test.</param>
        /// <returns><c>true</c> if the event can be handled, oitherwise <c>false</c>.</returns>
        public bool CanHandleEvent(ApplicationEventType eventType)
        {
            return this.eventTypes.Contains(eventType);
        }
    }
}
