// <copyright file="BinaryBase64TextProviderConfigModelConverter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Text
{
    using System;
    using Newtonsoft.Json;

    public class BinaryBase64TextProviderConfigModelConverter : DeserializationConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(BinaryBase64TextProviderConfigModel) == objectType;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var binaryProvider = serializer.Deserialize<IBuilder<IProvider<Data<byte[]>>>>(reader);
            var base64TextModel = new BinaryBase64TextProviderConfigModel() { BinaryProvider = binaryProvider };
            return base64TextModel;
        }
    }
}
