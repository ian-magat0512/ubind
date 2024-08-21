// <copyright file="UrlConverter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.JsonConverters;

using Flurl;
using Newtonsoft.Json;

/// <summary>
/// Ensures when using Flur.Url in a class that is serialized to JSON, the Url is serialized as a string.
/// Here how to use it: Decorate your property as follows:
///         [JsonConverter(typeof(UrlConverter))]
/// .
/// </summary>
public class UrlConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(Url);
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        if (value == null)
        {
            writer.WriteNull();
            return;
        }

        serializer.Serialize(writer, ((Url)value).ToString());
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
        {
            return null;
        }

        var url = serializer.Deserialize<string>(reader);

        return new Url(url);
    }
}
