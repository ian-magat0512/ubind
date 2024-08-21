// <copyright file="Base64TextBinaryProviderConfigModelConverter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Binary
{
    using System;
    using Newtonsoft.Json;

    public class Base64TextBinaryProviderConfigModelConverter : DeserializationConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(Base64TextBinaryProviderConfigModel) == objectType;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var textProvider = serializer.Deserialize<IBuilder<IProvider<Data<string>>>>(reader);
            var base64BinaryModel = new Base64TextBinaryProviderConfigModel() { TextProvider = textProvider };
            return base64BinaryModel;
        }
    }
}
