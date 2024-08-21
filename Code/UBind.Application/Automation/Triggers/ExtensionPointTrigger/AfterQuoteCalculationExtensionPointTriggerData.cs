// <copyright file="AfterQuoteCalculationExtensionPointTriggerData.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Triggers.ExtensionPointTrigger
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Represents the after quote calculation input data associated with a event trigger.
    /// </summary>
    public class AfterQuoteCalculationExtensionPointTriggerData : ExtensionPointTriggerData
    {
        [JsonConstructor]
        public AfterQuoteCalculationExtensionPointTriggerData(
            ExtensionPointType extensionPoint,
            JObject sourceInputData,
            JObject sourceCalculationResult)
            : base()
        {
            this.ExtensionPoint = extensionPoint;
            this.SourceInputData = sourceInputData;
            this.SourceCalculationResult = sourceCalculationResult;
        }

        [JsonProperty("extensionPoint")]
        public ExtensionPointType ExtensionPoint { get; private set; }

        [JsonProperty("sourceCalculationResult")]
        public JObject SourceCalculationResult { get; set; }

        [JsonProperty("returnCalculationResult")]
        public JObject? ReturnCalculationResult { get; set; }

        [JsonProperty("sourceInputData")]
        public JObject SourceInputData { get; set; }

        [JsonProperty("returnInputData")]
        public JObject? ReturnInputData { get; set; }
    }
}
