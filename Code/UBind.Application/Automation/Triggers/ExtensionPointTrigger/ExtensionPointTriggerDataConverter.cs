// <copyright file="ExtensionPointTriggerDataConverter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Triggers.ExtensionPointTrigger;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// Converter for deserializing JSON data into specific derived classes based on the "extensionPoint" type.
/// </summary>
public class ExtensionPointTriggerDataConverter : DeserializationConverter
{
    public ExtensionPointTriggerDataConverter()
    {
    }

    public override bool CanConvert(Type objectType)
    {
        return typeof(ExtensionPointTriggerData) == objectType;
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        var jObject = JObject.Load(reader);
        var extensionPoint = jObject["extensionPoint"]?.Value<string>();
        if (Enum.TryParse(extensionPoint, true, out ExtensionPointType extensionPointType))
        {
            switch (extensionPointType)
            {
                case ExtensionPointType.BeforeQuoteCalculation:
                    return jObject.ToObject<BeforeQuoteCalculationExtensionPointTriggerData>();
                case ExtensionPointType.AfterQuoteCalculation:
                    return jObject.ToObject<AfterQuoteCalculationExtensionPointTriggerData>();
            }
        }

        throw new JsonException("The 'extensionPoint' field is missing or invalid.");
    }
}
