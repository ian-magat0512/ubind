// <copyright file="ContentProviderConfigModelConverter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Http
{
    using System;
    using System.Linq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using UBind.Application.Automation.Providers;

    /// <summary>
    /// Model converter for objects of type <see cref="ContentProvider"/>.
    /// </summary>
    public class ContentProviderConfigModelConverter : DeserializationConverter
    {
        /// <inheritdoc/>
        public override bool CanConvert(Type objectType)
        {
            return typeof(IBuilder<ContentProvider>) == objectType;
        }

        /// <inheritdoc/>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                var textContentModel = new TextContentProviderConfigModel() { Content = null };
                return textContentModel;
            }
            else if (reader.TokenType == JsonToken.String)
            {
                var value = reader.Value;
                var textProvider = new StaticBuilder<Data<string>> { Value = value.ToString() };
                var textContentModel = new TextContentProviderConfigModel() { Content = textProvider };
                return textContentModel;
            }
            else if (reader.TokenType == JsonToken.StartArray)
            {
                var contentListJson = JArray.Load(reader);
                var contentList = contentListJson.Select(x => serializer.Deserialize<MultipartContentPropertyConfigModel>(x.CreateReader()));
                var enumerableContentModel = new EnumerableContentProviderConfigModel() { Content = contentList };
                return enumerableContentModel;
            }
            else
            {
                var obj = JObject.Load(reader);
                var providerName = obj.Properties().First().Name;
                var binaryProviders = AutomationDeserializationConfiguration.BinaryProviderTypeMap;
                if (binaryProviders.ContainsKey(providerName))
                {
                    var binaryProvider = serializer.Deserialize<IBuilder<IProvider<Data<byte[]>>>>(obj.CreateReader());
                    var binaryContentModel = new BinaryContentProviderConfigModel() { Content = binaryProvider };
                    return binaryContentModel;
                }

                var textProvider = serializer.Deserialize<IBuilder<IProvider<Data<string>>>>(obj.CreateReader());
                var textContentModel = new TextContentProviderConfigModel() { Content = textProvider };
                return textContentModel;
            }
        }
    }
}
