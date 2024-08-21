// <copyright file="EventTriggerConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Triggers
{
    using System;
    using Newtonsoft.Json;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Providers;
    using UBind.Domain.Events;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;

    /// <summary>
    /// Model for event trigger.
    /// </summary>
    public class EventTriggerConfigModel : IBuilder<Trigger>
    {
        [JsonConstructor]
        public EventTriggerConfigModel(string name, string alias, string description, IBuilder<IProvider<Data<bool>>> runCondition, string customEventAlias, SystemEventType eventType)
        {
            if (string.IsNullOrEmpty(customEventAlias) && eventType == SystemEventType.Custom)
            {
                throw new ErrorException(Domain.Errors.Automation.Trigger.TriggerParameterMissing(
                    "customEventAlias",
                    startWithAn: true,
                    typeof(EventTrigger).Name.ToCamelCase(),
                    "The \"customEventAlias\" must be specified when the \"eventType\" parameter value is \"custom\"."));
            }

            this.Name = name;
            this.Alias = alias;
            this.Description = description;
            this.RunCondition = runCondition;
            this.CustomEventAlias = customEventAlias;
            this.EventType = eventType;
        }

        /// <summary>
        /// Gets the name of the trigger.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public string Name { get; private set; }

        /// <summary>
        /// Gets the unique trigger ID or alias.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public string Alias { get; private set; }

        /// <summary>
        /// Gets the description of the trigger.
        /// </summary>
        [JsonProperty]
        public string Description { get; private set; }

        /// <summary>
        /// Gets the run condition of the trigger.
        /// </summary>
        [JsonProperty]
        public IBuilder<IProvider<Data<bool>>> RunCondition { get; private set; }

        /// <summary>
        /// Gets the ID of the event that invokes this trigger.
        /// </summary>
        [JsonProperty]
        public string CustomEventAlias { get; private set; }

        /// <summary>
        /// Gets the type of the event that invokes this trigger.
        /// </summary>
        [JsonProperty]
        public SystemEventType EventType { get; private set; }

        /// <inheritdoc/>
        public Trigger Build(IServiceProvider dependencyProvider)
        {
            return new EventTrigger(
                this.Name,
                this.Alias,
                this.Description,
                this.RunCondition?.Build(dependencyProvider),
                this.CustomEventAlias,
                this.EventType);
        }
    }
}
