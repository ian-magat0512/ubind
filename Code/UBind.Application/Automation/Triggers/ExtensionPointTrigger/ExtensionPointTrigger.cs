// <copyright file="ExtensionPointTrigger.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Triggers.ExtensionPointTrigger
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using Newtonsoft.Json.Serialization;
    using UBind.Application.Automation.Providers;

    /// <summary>
    /// Defines a trigger that will be on a specific point of code to extend functionality.
    /// </summary>
    public abstract class ExtensionPointTrigger : ConditionalTrigger
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExtensionPointTrigger"/> class.
        /// </summary>
        /// <param name="name">The name of the trigger.</param>
        /// <param name="alias">The alias of the trigger.</param>
        /// <param name="description">The trigger description.</param>
        /// <param name="extensionPointType">The extension point type.</param>
        /// <param name="runCondition">The run condition.</param>
        [JsonConstructor]
        public ExtensionPointTrigger(
            string name,
            string alias,
            string description,
            ExtensionPointType extensionPointType,
            IProvider<Data<bool>>? runCondition)
            : base(name, alias, description, runCondition)
        {
            this.ExtensionPoint = extensionPointType;
        }

        /// <summary>
        /// Gets the extension point type.
        /// </summary>
        [JsonProperty(PropertyName = "extensionPoint")]
        [JsonConverter(typeof(StringEnumConverter), converterParameters: typeof(CamelCaseNamingStrategy))]
        public ExtensionPointType ExtensionPoint { get; }
    }
}
