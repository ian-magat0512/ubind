// <copyright file="OrganisationEventPayload.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.Events.Payload;

using Newtonsoft.Json;
using UBind.Domain.Events.Models;

public class OrganisationEventPayload : BaseEventPayload, IEventPayload
{
    [JsonProperty(PropertyName = "organisation", Order = 2)]
    public Organisation? Organisation { get; set; }
}
