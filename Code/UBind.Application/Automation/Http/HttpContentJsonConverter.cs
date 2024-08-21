// <copyright file="HttpContentJsonConverter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Http
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class HttpContentJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(byte[])
                || objectType == typeof(ContentPart)
                || objectType == typeof(string);
        }

        public override object ReadJson(
            JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer)
        {
            switch (reader.TokenType)
            {
                case JsonToken.None:
                case JsonToken.Null:
                    return null;
                case JsonToken.String:
                    // it's just basic string content
                    return reader.Value;
                case JsonToken.StartObject:
                    JObject jObject = JObject.Load(reader);
                    string base64Value = jObject.GetValue("base64Value")?.Value<string>();
                    string binaryDataId = jObject.GetValue("binaryDataId")?.Value<string>();
                    if (base64Value == null && binaryDataId == null)
                    {
                        throw new JsonSerializationException("Could not deserialize HTTP Response \"content\" property. "
                            + $"An object was found but one of the required properties \"base64Value\" or "
                            + "\"binaryDataId\" was missing.");
                    }
                    else if (base64Value != null)
                    {
                        return Convert.FromBase64String(base64Value);
                    }

                    throw new NotImplementedException("When deserializing automation data binary content, we came "
                        + "across a binaryDataId property, however support for binary data stored separately has "
                        + "not yet been implemented");
                case JsonToken.StartArray:
                    JToken token = JToken.Load(reader);
                    return token.ToObject<List<ContentPart>>();
                default:
                    throw new JsonSerializationException("Could not deserialize HTTP Response \"content\" property. "
                        + $"Unexpected JsonToken {reader.TokenType}.");
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is string)
            {
                writer.WriteValue(value);
            }
            else if (value is byte[])
            {
                string base64String = Convert.ToBase64String(value as byte[]);
                JObject contentJObject = new JObject();
                contentJObject.Add("base64Value", base64String);
                contentJObject.WriteTo(writer);
            }
            else if (value is List<ContentPart>)
            {
                JArray.FromObject(value).WriteTo(writer);
            }
            else if (value is JToken)
            {
                writer.WriteValue(value.ToString());
            }
        }
    }
}
