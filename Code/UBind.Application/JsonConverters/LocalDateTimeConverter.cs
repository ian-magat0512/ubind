// <copyright file="LocalDateTimeConverter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.JsonConverters;

using NodaTime.Text;
using NodaTime;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// This custom converter is used to serialize and deserialize NodaTime.LocalDateTime objects.
/// Since the System.Text.Json library does not have built-in support for NodaTime types, this converter is necessary.
/// </summary>
public class LocalDateTimeConverter : JsonConverter<LocalDateTime>
{
    private readonly LocalDateTimePattern pattern =
            LocalDateTimePattern.CreateWithInvariantCulture("yyyy-MM-dd'T'HH:mm:ss");

    public override LocalDateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        if (string.IsNullOrEmpty(value))
        {
            return default;
        }

        var parseResult = this.pattern.Parse(value);
        if (!parseResult.Success)
        {
            throw new JsonException($"Failed to parse LocalDateTime: {parseResult.Exception.Message}");
        }

        return parseResult.Value;
    }

    public override void Write(Utf8JsonWriter writer, LocalDateTime value, JsonSerializerOptions options)
    {
        var formatted = this.pattern.Format(value);
        writer.WriteStringValue(formatted);
    }
}

/// <summary>
/// This custom converter is used to serialize and deserialize nullable NodaTime.LocalDateTime objects.
/// Since the System.Text.Json library does not have built-in support for NodaTime types, this converter is necessary.
/// </summary>
public class NullableLocalDateTimeConverter : JsonConverter<LocalDateTime?>
{
    private readonly LocalDateTimePattern pattern =
            LocalDateTimePattern.CreateWithInvariantCulture("yyyy-MM-dd'T'HH:mm:ss");

    public override LocalDateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        if (reader.TokenType == JsonTokenType.Null || string.IsNullOrEmpty(value))
        {
            return null;
        }

        var parseResult = this.pattern.Parse(value);
        if (!parseResult.Success)
        {
            throw new JsonException($"Failed to parse LocalDateTime: {parseResult.Exception.Message}");
        }

        return parseResult.Value;
    }

    public override void Write(Utf8JsonWriter writer, LocalDateTime? value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
        }
        else
        {
            var formatted = this.pattern.Format(value.Value);
            writer.WriteStringValue(formatted);
        }
    }
}
