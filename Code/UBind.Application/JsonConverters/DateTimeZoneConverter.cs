// <copyright file="DateTimeZoneConverter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.JsonConverters;

using NodaTime;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// This custom converter is used to serialize and deserialize <see cref="DateTimeZone"/> objects.
/// Since the System.Text.Json library does not have built-in support for NodaTime types, this converter is necessary.
/// </summary>
public class DateTimeZoneConverter : JsonConverter<DateTimeZone>
{
    public override DateTimeZone Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using (var element = JsonDocument.ParseValue(ref reader))
        {
            // for backward compatibility, we need to check if the timezone ID is stored as a string or as an object
            if (element.RootElement.ValueKind == JsonValueKind.String)
            {
                return this.GetDateTimeZone(element.RootElement.GetString());
            }
            else if (element.RootElement.ValueKind == JsonValueKind.Object)
            {
                if (element.RootElement.TryGetProperty("id", out JsonElement idElement))
                {
                    return this.GetDateTimeZone(idElement.GetString());
                }

                throw new JsonException("Invalid or missing timezone ID.");
            }
            else
            {
                throw new JsonException("Expected string or object for timezone ID.");
            }
        }
    }

    public override void Write(Utf8JsonWriter writer, DateTimeZone value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Id);
    }

    private DateTimeZone GetDateTimeZone(string? timezoneId)
    {
        if (string.IsNullOrWhiteSpace(timezoneId))
        {
            throw new JsonException("Invalid or missing timezone ID.");
        }
        return DateTimeZoneProviders.Tzdb[timezoneId];
    }
}
