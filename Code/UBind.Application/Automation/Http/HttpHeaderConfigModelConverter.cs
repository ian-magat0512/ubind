// <copyright file="HttpHeaderConfigModelConverter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Http
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using UBind.Application.Automation.Providers;

    /// <summary>
    /// Converter for http headers.
    /// </summary>
    public class HttpHeaderConfigModelConverter : DeserializationConverter
    {
        /// <inheritdoc/>
        public override bool CanConvert(Type objectType)
        {
            return typeof(IBuilder<IProvider<KeyValuePair<string, IEnumerable<string>>>>) == objectType;
        }

        /// <inheritdoc/>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jObject = JObject.Load(reader);
            var propertyKeys = jObject.Properties().Select(n => n.Name);
            var configModel = new HttpHeaderConfigModel();
            if (propertyKeys.First().Equals("name"))
            {
                var propertyNameTokenReader = jObject.SelectToken(propertyKeys.First()).CreateReader();
                configModel.Name = serializer.Deserialize<IBuilder<IProvider<Data<string>>>>(propertyNameTokenReader);
            }
            else
            {
                configModel.Name = new StaticBuilder<Data<string>>() { Value = propertyKeys.First().ToString() };
            }

            var items = this.ReadValueBuilders(jObject.SelectToken(propertyKeys.LastOrDefault()).CreateReader(), serializer);
            configModel.Values = items;
            return configModel;
        }

        private IReadOnlyList<IBuilder<IProvider<Data<string>>>> ReadValueBuilders(JsonReader reader, JsonSerializer serializer)
        {
            var items = new List<IBuilder<IProvider<Data<string>>>>();
            reader.Read();
            switch (reader.TokenType)
            {
                case JsonToken.String:
                    var staticBuilder = new StaticBuilder<Data<string>>();
                    staticBuilder.Value = reader.Value.ToString();
                    items.Add(staticBuilder);
                    break;
                case JsonToken.StartObject:
                    items.Add(serializer.Deserialize<IBuilder<IProvider<Data<string>>>>(reader));
                    break;
                case JsonToken.StartArray:
                    items.AddRange(this.ReadArray(reader, serializer));
                    break;
                default:
                    throw new JsonSerializationException("Unexpected token '" + reader.TokenType.ToString() + "'");
            }

            return items;
        }

        private IReadOnlyList<IBuilder<IProvider<Data<string>>>> ReadArray(JsonReader reader, JsonSerializer serializer)
        {
            var items = new List<IBuilder<IProvider<Data<string>>>>();
            while (reader.Read())
            {
                switch (reader.TokenType)
                {
                    case JsonToken.String:
                        var staticBuilder = new StaticBuilder<Data<string>>();
                        staticBuilder.Value = reader.Value.ToString();
                        items.Add(staticBuilder);
                        break;
                    case JsonToken.StartObject:
                        items.Add(serializer.Deserialize<IBuilder<IProvider<Data<string>>>>(reader));
                        break;
                    case JsonToken.EndArray:
                        return items;
                }
            }

            throw new JsonSerializationException("Unexpected end when trying to read array.");
        }
    }
}
