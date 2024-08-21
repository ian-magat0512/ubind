// <copyright file="EventTrigger.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Triggers
{
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;
    using UBind.Application.Automation.Data;
    using UBind.Application.Automation.Enums;
    using UBind.Application.Automation.Providers;
    using UBind.Domain.Events;
    using UBind.Domain.Extensions;

    /// <summary>
    /// Defines a trigger that will be invoked when a specific event is raised within the system.
    /// </summary>
    public class EventTrigger : ConditionalTrigger
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventTrigger"/> class.
        /// </summary>
        /// <param name="name">The name of the trigger.</param>
        /// <param name="alias">The alias of the trigger.</param>
        /// <param name="description">The trigger description.</param>
        /// <param name="runConditionProvider">The run condition, if any.</param>
        /// <param name="customEventAlias">The custom event alias.</param>
        /// <param name="eventType">The event type.</param>
        [JsonConstructor]
        public EventTrigger(
            string name,
            string alias,
            string description,
            IProvider<Data<bool>> runConditionProvider,
            string customEventAlias,
            SystemEventType eventType)
            : base(name, alias, description, runConditionProvider)
        {
            this.CustomEventAlias = customEventAlias;
            this.EventType = eventType;
        }

        /// <summary>
        /// Gets the event alias.
        /// </summary>
        public string CustomEventAlias { get; }

        /// <summary>
        /// Gets the event type.
        /// </summary>
        [JsonProperty(PropertyName = "eventType")]
        [JsonConverter(typeof(SystemEventTypeConverter), converterParameters: typeof(CamelCaseNamingStrategy))]
        public SystemEventType EventType { get; }

        /// <inheritdoc/>
        public override async Task<bool> DoesMatch(AutomationData dataContext)
        {
            if (dataContext.Trigger.Type != TriggerType.EventTrigger)
            {
                return false;
            }

            var providerContext = new ProviderContext(dataContext);
            string dataEventTypestr = await dataContext.GetValue<string>("/trigger/eventType", providerContext);
            string configEventTypeStr = this.EventType.ToString().ToCamelCase();
            string customEventTypeStr = SystemEventType.Custom.ToString().ToCamelCase();
            string dataCustomEventAlias = null;
            if (dataEventTypestr.Equals(customEventTypeStr))
            {
                dataCustomEventAlias = await dataContext.GetValue<string>("/trigger/customEventAlias", providerContext);
            }

            if (configEventTypeStr != dataEventTypestr)
            {
                return false;
            }
            else if (this.EventType == SystemEventType.Custom && this.CustomEventAlias != dataCustomEventAlias)
            {
                return false;
            }

            return true;
        }
    }
}
