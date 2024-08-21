// <copyright file="LastPeriodProviderConfigModelConverter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Period
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// Model converter for period providers.
    /// </summary>
    public class LastPeriodProviderConfigModelConverter : DeserializationConverter
    {
        /// <inheritdoc/>
        public override bool CanConvert(Type objectType)
        {
            return typeof(LastPeriodProviderConfigModel) == objectType;
        }

        /// <inheritdoc/>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var durationProviderConfigModel = serializer.Deserialize<IBuilder<IProvider<Data<NodaTime.Period>>>>(reader);
            return new LastPeriodProviderConfigModel
            {
                Duration = durationProviderConfigModel,
            };
        }
    }
}
