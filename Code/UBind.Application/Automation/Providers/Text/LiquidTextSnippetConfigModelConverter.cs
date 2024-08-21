// <copyright file="LiquidTextSnippetConfigModelConverter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Text
{
    using System;
    using System.Linq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using UBind.Application.Automation.Providers;

    /// <summary>
    /// Converter for liquid snippets for liquid text provider.
    /// </summary>
    public class LiquidTextSnippetConfigModelConverter : DeserializationConverter
    {
        /// <inheritdoc/>
        public override bool CanConvert(Type objectType)
        {
            return typeof(IBuilder<IProvider<LiquidTextSnippet>>) == objectType;
        }

        /// <inheritdoc/>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jObject = JObject.Load(reader);
            var propertyKeys = jObject.Properties().Select(n => n.Name);
            var configModel = new LiquidTextSnippetConfigModel();
            if (propertyKeys.First().Equals("snippetAlias"))
            {
                var propertyNameTokenReader = jObject.SelectToken(propertyKeys.First()).CreateReader();
                configModel.SnippetAlias = serializer.Deserialize<IBuilder<IProvider<Data<string>>>>(propertyNameTokenReader);
            }
            else
            {
                configModel.SnippetAlias = new StaticBuilder<Data<string>>() { Value = propertyKeys.First().ToString() };
            }

            var item = this.ReadValueBuilders(jObject.SelectToken(propertyKeys.LastOrDefault()).CreateReader(), serializer);
            configModel.LiquidTemplate = item;
            return configModel;
        }

        private IBuilder<IProvider<Data<string>>> ReadValueBuilders(JsonReader reader, JsonSerializer serializer)
        {
            reader.Read();
            switch (reader.TokenType)
            {
                case JsonToken.String:
                    var staticBuilder = new StaticBuilder<Data<string>>();
                    staticBuilder.Value = reader.Value.ToString();
                    return staticBuilder;
                case JsonToken.StartObject:
                    return serializer.Deserialize<IBuilder<IProvider<Data<string>>>>(reader);
                default:
                    throw new JsonSerializationException("Unexpected token '" + reader.TokenType.ToString() + "'");
            }
        }
    }
}
