// <copyright file="HeadersConverter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Http
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Extensions.Primitives;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class HeadersConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Dictionary<string, StringValues>);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            var jObject = JObject.Load(reader);
            var value = new Dictionary<string, StringValues>();
            foreach (var property in jObject)
            {
                StringValues headerValue = property.Value is JArray ?
                    new StringValues(property.Value.Select(x => x.ToString()).ToArray()) :
                    new StringValues(property.Value.ToString());
                value.Add(property.Key, headerValue);
            }

            return value;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var dictionary = value as Dictionary<string, StringValues>;
            var headerObject = new JObject();
            foreach (var item in dictionary)
            {
                JToken headerValue;
                if (item.Value.Count > 1)
                {
                    headerValue = new JArray(item.Value);
                }
                else
                {
                    headerValue = new JValue(item.Value);
                }

                headerObject.Add(item.Key, headerValue);
            }

            headerObject.WriteTo(writer);
        }
    }
}
