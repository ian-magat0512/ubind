// <copyright file="CountListItemsIntegerProviderConfigModelConverter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Integer
{
    using System;
    using Newtonsoft.Json;
    using UBind.Application.Automation.Providers.List;

    /// <summary>
    /// Converter for deserializing collection count provider config models.
    /// </summary>
    public class CountListItemsIntegerProviderConfigModelConverter : DeserializationConverter
    {
        /// <inheritdoc/>
        public override bool CanConvert(Type objectType)
        {
            return typeof(CountListItemsIntegerProviderConfigModel) == objectType;
        }

        /// <inheritdoc/>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.StartObject)
            {
                throw new NotSupportedException("Unsupported list provider.");
            }

            var collectionProviderConfigModel = serializer.Deserialize<IBuilder<IDataListProvider<object>>>(reader);
            var collectionCountProviderConfigModel = new CountListItemsIntegerProviderConfigModel
            {
                List = collectionProviderConfigModel,
            };

            return collectionCountProviderConfigModel;
        }
    }
}
