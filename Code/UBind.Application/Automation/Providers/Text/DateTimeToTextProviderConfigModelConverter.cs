// <copyright file="DateTimeToTextProviderConfigModelConverter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Text
{
    using System;
    using Newtonsoft.Json;
    using NodaTime;
    using NodaTime.Text;
    using UBind.Application.Automation.Providers;

    public class DateTimeToTextProviderConfigModelConverter : JsonConverter
    {
        /// <inheritdoc/>
        public override bool CanConvert(Type objectType)
        {
            return typeof(DateTimeToTextProviderConfigModel) == objectType;
        }

        /// <inheritdoc/>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var dateTimeValue = default(IBuilder<IProvider<Data<Instant>>>);
            if (reader.TokenType == JsonToken.String)
            {
                dateTimeValue = new StaticBuilder<Data<Instant>>() { Value = InstantPattern.ExtendedIso.Parse(reader.Value.ToString()).Value };
            }
            else
            {
                dateTimeValue = serializer.Deserialize<IBuilder<IProvider<Data<Instant>>>>(reader);
            }

            return new DateTimeToTextProviderConfigModel() { DateTimeToText = dateTimeValue };
        }

        /// <inheritdoc/>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
