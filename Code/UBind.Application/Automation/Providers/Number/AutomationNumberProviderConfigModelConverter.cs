// <copyright file="AutomationNumberProviderConfigModelConverter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Number
{
    using System;
    using Newtonsoft.Json;
    using UBind.Application.Export;

    /// <summary>
    /// Converter for deserializing number provider models from automation configuration json.
    /// </summary>
    /// <remarks>
    /// Number fields are interpreted as fixed automation number providers.
    /// </remarks>
    public class AutomationNumberProviderConfigModelConverter : PropertyDiscriminatorConverter<IBuilder<IProvider<Data<decimal>>>>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AutomationNumberProviderConfigModelConverter"/> class.
        /// </summary>
        /// <param name="typeMap">A map for the property field values to concrete child types.</param>
        public AutomationNumberProviderConfigModelConverter(TypeMap typeMap)
            : base(typeMap)
        {
        }

        /// <inheritdoc/>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var value = reader.Value;
            if (value != null)
            {
                return new TextToNumberProviderConfigModel
                {
                    TextToNumber = new StaticBuilder<Data<string>> { Value = value.ToString() },
                };
            }

            if (reader.TokenType == JsonToken.Null)
            {
                return new StaticBuilder<Data<decimal>> { Value = null };
            }

            return base.ReadJson(reader, objectType, existingValue, serializer);
        }
    }
}
