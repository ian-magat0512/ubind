// <copyright file="CustomSerializerSetting.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using Newtonsoft.Json;
    using NodaTime.Serialization.JsonNet;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Helpers;
    using UBind.Domain.Repositories;

    /// <summary>
    /// This class is used to create custom serializer settings.
    /// This is used to handle serialization of a new and old format of nodatime.
    /// </summary>
    public static class CustomSerializerSetting
    {
        static CustomSerializerSetting()
        {
            var jsonSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                ObjectCreationHandling = ObjectCreationHandling.Replace,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore,
                SerializationBinder = new EventSourcedAggregateStringToTypeBinder(),
            };

            jsonSettings.Converters.Add(new NodaLocalDateConverter());
            jsonSettings.Converters.Add(new NodaLocalDateTimeConverter());
            jsonSettings.Converters.Add(new InstantConverter());

            jsonSettings.DateParseHandling = DateParseHandling.None;
            jsonSettings.Converters.Add(NodaConverters.InstantConverter);
            jsonSettings.Converters.Add(NodaConverters.IntervalConverter);
            jsonSettings.Converters.Add(NodaConverters.LocalDateTimeConverter);
            jsonSettings.Converters.Add(NodaConverters.LocalTimeConverter);
            jsonSettings.Converters.Add(NodaConverters.OffsetConverter);
            jsonSettings.Converters.Add(NodaConverters.DurationConverter);
            jsonSettings.Converters.Add(NodaConverters.RoundtripPeriodConverter);
            jsonSettings.Converters.Add(NodaConverters.OffsetDateTimeConverter);
            JsonSerializerSettings = jsonSettings;

            AggregateEventSerializerSettings = GetAggregateEventSerializerSetting();
        }

        public static JsonSerializerSettings JsonSerializerSettings { get; private set; }

        public static JsonSerializerSettings AggregateEventSerializerSettings { get; }

        private static JsonSerializerSettings GetAggregateEventSerializerSetting()
        {
            var serializerSetting = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                ObjectCreationHandling = ObjectCreationHandling.Replace,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore,
                SerializationBinder = new EventSourcedAggregateStringToTypeBinder(),
            };

            serializerSetting.Converters.Add(new NodaLocalDateConverter());
            serializerSetting.Converters.Add(new NodaLocalDateTimeConverter());
            serializerSetting.Converters.Add(new InstantConverter());
            serializerSetting.Converters.Add(new PatchDocumentConverter());
            return serializerSetting;
        }
    }
}
