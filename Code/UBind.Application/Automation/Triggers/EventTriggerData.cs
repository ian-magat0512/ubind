// <copyright file="EventTriggerData.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Triggers
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using UBind.Application.Automation.Enums;
    using UBind.Domain.Events;
    using UBind.Domain.Extensions;

    /// <summary>
    /// Represents the data associated with a event trigger.
    /// </summary>
    public class EventTriggerData : TriggerData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventTriggerData"/> class.
        /// </summary>
        /// <param name="event">The event.</param>
        public EventTriggerData(SystemEvent @event)
            : base(TriggerType.EventTrigger)
        {
            this.CustomEventAlias = @event.CustomEventAlias;
            this.EventData = JObject.Parse(@event.PayloadJson ?? "{}");
            this.EventType = @event.EventType.ToString().ToCamelCase();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventTriggerData"/> class.
        /// </summary>
        [JsonConstructor]
        public EventTriggerData()
            : base(TriggerType.EventTrigger)
        {
        }

        /// <summary>
        /// Gets the custom event alias.
        /// </summary>
        [JsonProperty("customEventAlias")]
        public string CustomEventAlias { get; private set; }

        /// <summary>
        /// Gets the event type.
        /// </summary>
        [JsonProperty("eventType")]
        public string EventType { get; private set; }

        /// <summary>
        /// Gets the event request received by the event trigger.
        /// </summary>
        [JsonProperty("eventData")]
        public JObject EventData { get; private set; }
    }
}
