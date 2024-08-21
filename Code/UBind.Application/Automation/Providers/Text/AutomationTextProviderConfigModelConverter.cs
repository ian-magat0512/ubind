// <copyright file="AutomationTextProviderConfigModelConverter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Text
{
    using System;
    using Newtonsoft.Json;
    using UBind.Application.Export;

    /// <summary>
    /// Converter for deserializing text provider models from automation configuration json.
    /// </summary>
    /// <remarks>
    /// Text fields are interpreted as fixed automation text providers.
    /// </remarks>
    public class AutomationTextProviderConfigModelConverter : PropertyDiscriminatorConverter<IBuilder<IProvider<Data<string>>>>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AutomationTextProviderConfigModelConverter"/> class.
        /// </summary>
        /// <param name="typeMap">A map for the property field values to concrete child types.</param>
        public AutomationTextProviderConfigModelConverter(TypeMap typeMap)
            : base(typeMap)
        {
        }

        /// <inheritdoc/>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return new StaticBuilder<Data<string>> { Value = null };
            }

            var value = reader.Value;
            if (value != null)
            {
                return new StaticBuilder<Data<string>> { Value = value.ToString() };
            }

            return base.ReadJson(reader, objectType, existingValue, serializer);
        }
    }
}
