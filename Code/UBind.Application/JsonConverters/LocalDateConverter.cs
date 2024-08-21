// <copyright file="LocalDateConverter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.JsonConverters;

using NodaTime.Text;
using NodaTime;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// This custom converter is used to serialize and deserialize NodaTime.LocalDate objects.
/// Since the System.Text.Json library does not have built-in support for NodaTime types, this converter is necessary.
/// </summary>
public class LocalDateConverter : JsonConverter<LocalDate>
{
    private readonly LocalDatePattern pattern = LocalDatePattern.Iso;

    public override LocalDate Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        if (string.IsNullOrEmpty(value))
        {
            return default;
        }

        return this.pattern.Parse(value).GetValueOrThrow();
    }

    public override void Write(Utf8JsonWriter writer, LocalDate value, JsonSerializerOptions options)
    {
        string formatted = this.pattern.Format(value);
        writer.WriteStringValue(formatted);
    }
}

/// <summary>
/// This custom converter is used to serialize and deserialize nullable NodaTime.LocalDate objects.
/// Since the System.Text.Json library does not have built-in support for NodaTime types, this converter is necessary.
/// </summary>
public class NullableLocalDateConverter : JsonConverter<LocalDate?>
{
    private readonly LocalDatePattern pattern = LocalDatePattern.Iso;

    public override LocalDate? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();

        if (reader.TokenType == JsonTokenType.Null || string.IsNullOrEmpty(value))
        {
            return null;
        }

        return this.pattern.Parse(value).GetValueOrThrow();
    }

    public override void Write(Utf8JsonWriter writer, LocalDate? value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
        }
        else
        {
            string formatted = this.pattern.Format(value.Value);
            writer.WriteStringValue(formatted);
        }
    }
}
