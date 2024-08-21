// <copyright file="QuoteConverter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.JsonConverters;

using System.Text.Json;
using UBind.Domain;
using UBind.Domain.Aggregates.Quote;

/// <summary>
/// This custom converter is to handle the deserialization of the Quote object.
/// Since we have different types of quotes, we need to determine the type of quote before deserializing it.
/// System.Text.Json does not support polymorphic deserialization out of the box.
/// </summary>
public class QuoteConverter : ConditionalIgnorePropertiesConverter<Quote>
{
    public override Quote Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using (var doc = JsonDocument.ParseValue(ref reader))
        {
            JsonElement element = doc.RootElement;
            var type = element.GetProperty(nameof(Quote.Type).ToLower()).GetInt32();
            QuoteType quoteType = Enum.Parse<QuoteType>(type.ToString());

            Quote quote = quoteType switch
            {
                QuoteType.NewBusiness => JsonSerializer.Deserialize<NewBusinessQuote>(element.GetRawText(), options) ?? throw new JsonException("Cannot deserialize NewBusinessQuote."),
                QuoteType.Adjustment => JsonSerializer.Deserialize<AdjustmentQuote>(element.GetRawText(), options) ?? throw new JsonException("Cannot deserialize AdjustmentQuote."),
                QuoteType.Renewal => JsonSerializer.Deserialize<RenewalQuote>(element.GetRawText(), options) ?? throw new JsonException("Cannot deserialize RenewalQuote."),
                QuoteType.Cancellation => JsonSerializer.Deserialize<CancellationQuote>(element.GetRawText(), options) ?? throw new JsonException("Cannot deserialize CancellationQuote."),
                _ => throw new JsonException($"Unknown quote type: {type}")
            };

            return quote;
        }
    }
}
