// <copyright file="InstantConverter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.JsonConverters;

using NodaTime;
using NodaTime.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// This custom converter is used to serialize and deserialize NodaTime.Instant objects.
/// Since the System.Text.Json library does not have built-in support for NodaTime types, this converter is necessary.
/// </summary>
public class InstantConverter : JsonConverter<Instant>
{
    public override Instant Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.None:
            case JsonTokenType.Null:
                return default;
            case JsonTokenType.String:
                var value = reader.GetString();
                if (string.IsNullOrEmpty(value))
                {
                    return default;
                }

                var parseResult = InstantPattern.ExtendedIso.Parse(value);
                if (!parseResult.Success)
                {
                    throw new JsonException("Invalid datetime format.");
                }
                return parseResult.Value;

            default:
                throw new JsonException($"Unexpected JSON token {reader.TokenType} when parsing an Instant.");
        }
    }

    public override void Write(Utf8JsonWriter writer, Instant value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(InstantPattern.ExtendedIso.Format(value));
    }
}

/// <summary>
/// This custom converter is used to serialize and deserialize nullable NodaTime.Instant objects.
/// Since the System.Text.Json library does not have built-in support for NodaTime types, this converter is necessary.
/// </summary>
public class NullableInstantConverter : JsonConverter<Instant?>
{
    public override Instant? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.None:
            case JsonTokenType.Null:
                return null;
            case JsonTokenType.String:
                var value = reader.GetString();
                if (string.IsNullOrEmpty(value))
                {
                    return null;
                }

                var parseResult = InstantPattern.ExtendedIso.Parse(value);
                if (!parseResult.Success)
                {
                    throw new JsonException("Invalid date format.");
                }
                return parseResult.Value;

            default:
                throw new JsonException($"Unexpected JSON token {reader.TokenType} when parsing an Instant.");
        }
    }

    public override void Write(Utf8JsonWriter writer, Instant? value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
        }
        else
        {
            writer.WriteStringValue(InstantPattern.ExtendedIso.Format(value.Value));
        }
    }
}
