// <copyright file="TriggerData.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Triggers
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using Newtonsoft.Json.Serialization;
    using UBind.Application.Automation.Enums;

    /// <summary>
    /// An object containing the data associated with the trigger which caused this automation to run.
    /// </summary>
    public abstract class TriggerData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TriggerData"/> class.
        /// </summary>
        /// <param name="type">The trigger type referenced.</param>
        public TriggerData(TriggerType type)
        {
            this.Type = type;
        }

        /// <summary>
        /// Gets the type of this trigger.
        /// </summary>
        [JsonProperty("type")]
        [JsonConverter(typeof(StringEnumConverter), converterParameters: typeof(CamelCaseNamingStrategy))]
        public TriggerType Type { get; }

        /// <summary>
        /// Gets or sets the alias of the trigger that started this automation.
        /// </summary>
        [JsonProperty("triggerAlias")]
        public string TriggerAlias { get; set; }
    }
}
