// <copyright file="PersonalDetailConverter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.JsonConverters;

using System.Text.Json;
using UBind.Domain.Aggregates.Person;

/// <summary>
/// This custom converter for PersonalDetails is needed because the IPersonalDetails interface is not a concrete type.
/// System.Text.Json does not know how to deserialize an interface.
/// So we need to provide a custom converter to tell System.Text.Json how to deserialize IPersonalDetails.
/// </summary>
public class PersonalDetailConverter : ConditionalIgnorePropertiesConverter<IPersonalDetails>
{
    public override IPersonalDetails Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        JsonDocument jsonDoc = JsonDocument.ParseValue(ref reader);
        return JsonSerializer.Deserialize<PersonalDetails>(jsonDoc.RootElement.GetRawText(), options) ?? throw new JsonException("Cannot deserialize PersonalDetails.");
    }
}
