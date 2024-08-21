// <copyright file="BaseEventPayload.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.Events.Payload;

using Newtonsoft.Json;
using UBind.Domain.Events.Models;

public abstract class BaseEventPayload : IEventPayload
{
    [JsonProperty(PropertyName = "tenant", Order = 1)]
    public Tenant? Tenant { get; set; }

    [JsonProperty(PropertyName = "performingUser", NullValueHandling = NullValueHandling.Ignore, Order = 20)]
    public User? PerformingUser { get; set; }
}
