// <copyright file="QuoteOperationEventPayload.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.Events.Payload;

using Newtonsoft.Json;
using UBind.Domain.Events.Models;

/// <summary>
/// Gets or sets the payload related to a quote event.
/// </summary>
public class QuoteOperationEventPayload : BaseEventPayload, IEventPayload
{
    [JsonProperty(PropertyName = "organisation", Order = 2)]
    public Organisation? Organisation { get; set; }

    [JsonProperty(PropertyName = "product", Order = 3)]
    public Product? Product { get; set; }

    [JsonProperty(PropertyName = "quote", NullValueHandling = NullValueHandling.Ignore, Order = 4)]
    public Quote? Quote { get; set; }

    [JsonProperty(PropertyName = "customer", NullValueHandling = NullValueHandling.Ignore, Order = 5)]
    public Customer? Customer { get; set; }

    [JsonProperty(PropertyName = "file", NullValueHandling = NullValueHandling.Ignore, Order = 6)]
    public File? File { get; set; }

    public QuoteOperationEventPayload SetFile(string name)
    {
        this.File = new File
        {
            Name = name,
        };
        return this;
    }

    public QuoteOperationEventPayload SetQuote(Aggregates.Quote.Quote quote)
    {
        this.Quote = EventPayloadHelper.GetQuote(quote);
        return this;
    }
}
