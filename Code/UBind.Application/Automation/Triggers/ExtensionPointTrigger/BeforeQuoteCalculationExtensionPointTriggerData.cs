// <copyright file="BeforeQuoteCalculationExtensionPointTriggerData.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Triggers.ExtensionPointTrigger
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Represents the before quote calculation data associated with a event trigger.
    /// </summary>
    public class BeforeQuoteCalculationExtensionPointTriggerData : ExtensionPointTriggerData
    {
        [JsonConstructor]
        public BeforeQuoteCalculationExtensionPointTriggerData(
            ExtensionPointType extensionPoint,
            JObject sourceInputData)
            : base()
        {
            this.ExtensionPoint = extensionPoint;
            this.SourceInputData = sourceInputData;
        }

        /// <summary>
        /// Gets the extension point type.
        /// </summary>
        [JsonProperty("extensionPoint")]
        public ExtensionPointType ExtensionPoint { get; private set; }

        /// <summary>
        /// Gets or sets the source input data which is an object.
        /// </summary>
        [JsonProperty("sourceInputData")]
        public JObject SourceInputData { get; set; }

        /// <summary>
        /// Gets or sets the return input data which is an object.
        /// </summary>
        [JsonProperty("returnInputData")]
        public JObject? ReturnInputData { get; set; }
    }
}
