// <copyright file="DurationProviderConfigModelConverter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Duration
{
    using System;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Application.Export;

    /// <summary>
    /// This class is responsible for the base converion of a duration provider if a static value is provided.
    /// </summary>
    public class DurationProviderConfigModelConverter : PropertyDiscriminatorConverter<IBuilder<IProvider<Data<Period>>>>
    {
        public DurationProviderConfigModelConverter(TypeMap typeMap)
            : base(typeMap)
        {
        }

        /// <inheritdoc/>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.Value != null)
            {
                var textProviderBuilder = new StaticBuilder<Data<string>> { Value = reader.Value.ToString() };
                var textDurationProviderModel = new StaticDurationProviderConfigModel() { Value = textProviderBuilder };
                return textDurationProviderModel;
            }

            return base.ReadJson(reader, objectType, existingValue, serializer);
        }
    }
}
