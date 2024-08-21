// <copyright file="ExtensionPointTriggerConverter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Triggers.ExtensionPointTrigger
{
    using System;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Converter ExtensionPointTrigger to a proper type depending on the ExtensionPointType.
    /// </summary>
    public class ExtensionPointTriggerConverter : DeserializationConverter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExtensionPointTriggerConverter"/> class.
        /// </summary>
        public ExtensionPointTriggerConverter()
        {
        }

        /// <inheritdoc/>
        public override bool CanConvert(Type objectType)
        {
            return typeof(ExtensionPointTriggerConfigModel) == objectType;
        }

        /// <inheritdoc/>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jObject = JObject.Load(reader);
            var extensionPoint = jObject["extensionPoint"].ToString();
            Enum.TryParse(extensionPoint, true, out ExtensionPointType extensionPointType);

            switch (extensionPointType)
            {
                case ExtensionPointType.AfterQuoteCalculation:
                    return serializer.Deserialize<AfterQuoteCalculationExtensionPointTriggerConfigModel>(jObject.CreateReader());
                case ExtensionPointType.BeforeQuoteCalculation:
                default:
                    return serializer.Deserialize<BeforeQuoteCalculationExtensionPointTriggerConfigModel>(jObject.CreateReader());
            }
        }
    }
}
