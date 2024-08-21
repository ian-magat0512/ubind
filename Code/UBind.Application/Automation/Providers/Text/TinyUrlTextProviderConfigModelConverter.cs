// <copyright file="TinyUrlTextProviderConfigModelConverter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Text;

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UBind.Application.Automation.Providers;

public class TinyUrlTextProviderConfigModelConverter : JsonConverter
{
    /// <inheritdoc/>
    public override bool CanConvert(Type objectType)
    {
        return typeof(TinyUrlTextProviderConfigModel) == objectType;
    }

    /// <inheritdoc/>
    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
        {
            return null;
        }

        var value = reader.Value;
        if (reader.TokenType == JsonToken.String)
        {
            var tinyUrlText = new StaticBuilder<Data<string>>() { Value = reader.Value.ToString() };
            return new TinyUrlTextProviderConfigModel() { RedirectUrl = tinyUrlText };
        }
        else
        {
            var obj = JObject.Load(reader);
            var redirectUrlProperty = obj.Properties().FirstOrDefault(c => c.Name.Equals("redirectUrl"));
            var redirectUrl = redirectUrlProperty != null
                ? serializer.Deserialize<IBuilder<IProvider<Data<string>>>>(redirectUrlProperty.Value.CreateReader())
                : serializer.Deserialize<IBuilder<IProvider<Data<string>>>>(obj.CreateReader());
            return new TinyUrlTextProviderConfigModel() { RedirectUrl = redirectUrl };
        }
    }

    /// <inheritdoc/>
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }
}
