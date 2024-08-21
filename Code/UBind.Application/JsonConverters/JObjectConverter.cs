// <copyright file="JObjectConverter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.JsonConverters;

using Newtonsoft.Json.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// This custom converter is important to convert a JObject to a string and vice versa.
/// Since the System.Text.Json does not have a built-in converter for JObject, we need to create one.
/// JObject was a Newtownsoft.Json type that we are still using in some parts of the application.
/// So System.Text.Json needs to know how to convert it.
/// </summary>
public class JObjectConverter : JsonConverter<JObject>
{
    public override JObject Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using (var jsonDocument = JsonDocument.ParseValue(ref reader))
        {
            string rawJson = jsonDocument.RootElement.GetRawText();
            return JObject.Parse(rawJson);
        }
    }

    public override void Write(Utf8JsonWriter writer, JObject value, JsonSerializerOptions options)
    {
        using (var document = JsonDocument.Parse(value.ToString()))
        {
            document.WriteTo(writer);
        }
    }
}
