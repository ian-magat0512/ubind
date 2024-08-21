// <copyright file="XmlTextToObjectProviderConfigModelConverter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers
{
    using System;
    using Newtonsoft.Json;
    using UBind.Application.Automation.Providers.Object;

    /// <summary>
    /// Converter for XML object provider expecting an object of type text.
    /// </summary>
    public class XmlTextToObjectProviderConfigModelConverter : JsonConverter
    {
        /// <inheritdoc/>
        public override bool CanConvert(Type objectType)
        {
            return typeof(XmlTextToObjectProviderConfigModel) == objectType;
        }

        /// <inheritdoc/>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var textProvider = serializer.Deserialize<IBuilder<IProvider<Data<string>>>>(reader);
            var xmlObjectModel = new XmlTextToObjectProviderConfigModel() { TextProvider = textProvider };
            return xmlObjectModel;
        }

        /// <inheritdoc/>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
