// <copyright file="TextToTimeProviderConfigModelConverter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Time
{
    using System;
    using Newtonsoft.Json;
    using UBind.Application.Automation.Providers;

    public class TextToTimeProviderConfigModelConverter : JsonConverter
    {
        /// <inheritdoc/>
        public override bool CanConvert(Type objectType)
        {
            return typeof(TextToTimeProviderConfigModel) == objectType;
        }

        /// <inheritdoc/>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var timeValue = default(IBuilder<IProvider<Data<string>>>);
            if (reader.TokenType == JsonToken.String)
            {
                timeValue = new StaticBuilder<Data<string>>() { Value = reader.Value.ToString() };
            }
            else
            {
                timeValue = serializer.Deserialize<IBuilder<IProvider<Data<string>>>>(reader);
            }

            return new TextToTimeProviderConfigModel() { TextToTime = timeValue };
        }

        /// <inheritdoc/>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
