// <copyright file="CustomerEventPayload.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.Events.Payload;

using Newtonsoft.Json;
using UBind.Domain.Events.Models;

/// <summary>
/// The payload that comes with the user event.
/// </summary>
public class CustomerEventPayload : BaseEventPayload, IEventPayload
{
    [JsonProperty(PropertyName = "organisation", NullValueHandling = NullValueHandling.Ignore, Order = 2)]
    public Organisation? Organisation { get; set; }

    [JsonProperty(PropertyName = "customer", NullValueHandling = NullValueHandling.Ignore, Order = 3)]
    public Customer? Customer { get; set; }
}
