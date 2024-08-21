// <copyright file="AggregateJsonSerializerSetting.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Json;

using System.Text.Json;
using UBind.Application.JsonConverters;
using UBind.Domain.Aggregates.Quote;

/// <summary>
/// Custom JSON serializer settings used in the application during serailization and deserialization
/// Some types require custom converters to be able to serialize and deserialize them correctly.
/// Since the System.Text.Json can't handle some types out of the box, we need to provide custom converters.
/// </summary>
public static class AggregateJsonSerializerSetting
{
    public static JsonSerializerOptions SerializerSetting
    {
        get
        {
            var options = DefaultGenerationOptions;
            options.Converters.Add(new ConditionalIgnorePropertiesConverter<QuoteAggregate>());
            options.Converters.Add(new PriceBreakdownConverter());
            return options;
        }
    }

    public static JsonSerializerOptions DeserializerSetting
    {
        get
        {
            var options = DefaultGenerationOptions;
            return options;
        }
    }

    private static JsonSerializerOptions DefaultGenerationOptions
    {
        get
        {
            var options = AggregateJsonContext.SourceGenerationOptions;
            options.Converters.Add(new QuoteConverter());
            options.Converters.Add(new PolicyTransactionConverter());
            options.Converters.Add(new JObjectConverter());
            options.Converters.Add(new DateTimeZoneConverter());
            options.Converters.Add(new LocalDateConverter());
            options.Converters.Add(new NullableLocalDateConverter());
            options.Converters.Add(new LocalDateTimeConverter());
            options.Converters.Add(new NullableLocalDateTimeConverter());
            options.Converters.Add(new InstantConverter());
            options.Converters.Add(new NullableInstantConverter());
            options.Converters.Add(new PersonalDetailConverter());
            return options;
        }
    }
}
