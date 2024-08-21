// <copyright file="PropertyExpressionProviderConfigModelConverter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Expression
{
    using System;
    using Newtonsoft.Json;
    using UBind.Application.Automation.Providers;

    /// <summary>
    /// Converter for deserializing property expresion provider config models.
    /// </summary>
    public class PropertyExpressionProviderConfigModelConverter : DeserializationConverter
    {
        /// <inheritdoc/>
        public override bool CanConvert(Type objectType)
        {
            return typeof(PropertyExpressionProviderConfigModel) == objectType;
        }

        /// <inheritdoc/>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var propertyNameProviderModel = serializer.Deserialize(reader, typeof(IBuilder<IProvider<Data<string>>>)) as IBuilder<IProvider<Data<string>>>;
            return new PropertyExpressionProviderConfigModel { Property = propertyNameProviderModel };
        }
    }
}
