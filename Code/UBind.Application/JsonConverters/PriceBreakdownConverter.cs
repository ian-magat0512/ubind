// <copyright file="PriceBreakdownConverter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.JsonConverters;

using UBind.Domain.ReadWriteModel;

/// <summary>
/// This custom converter handles the serialization of the PriceBreakdown object.
/// If a property value is null, 0, or 0.0, it will not be serialized.
/// This helps to reduce the size of the JSON payload.
/// </summary>
public class PriceBreakdownConverter : ConditionalIgnorePropertiesConverter<PriceBreakdown>
{
    protected override bool ShouldSerializeProperty(object? propertyValue)
    {
        if (propertyValue == null)
        {
            return false;
        }

        if (propertyValue is decimal decimalValue)
        {
            return decimalValue > 0m;
        }
        else if (propertyValue is int intValue)
        {
            return intValue > 0;
        }
        else
        {
            return true;
        }
    }
}
