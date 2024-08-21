// <copyright file="UserEventPayload.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.Events.Payload;

using Newtonsoft.Json;
using UBind.Domain.Events.Models;
using UBind.Domain.Helpers;

/// <summary>
/// The payload that comes with the user event.
/// </summary>
public class UserEventPayload : BaseEventPayload, IEventPayload
{
    [JsonProperty(PropertyName = "organisation", NullValueHandling = NullValueHandling.Ignore, Order = 2)]
    public Organisation? Organisation { get; set; }

    [JsonProperty(PropertyName = "user", NullValueHandling = NullValueHandling.Ignore, Order = 3)]
    public User? User { get; set; }

    [JsonProperty(PropertyName = "role", NullValueHandling = NullValueHandling.Ignore, Order = 4)]
    public Role? Role { get; set; }

    [JsonProperty(PropertyName = "emailAddress", NullValueHandling = NullValueHandling.Ignore, Order = 5)]
    public string? EmailAddress { get; set; }

    public UserEventPayload SetRole(Entities.Role role)
    {
        if (role == null)
        {
            return this;
        }

        this.Role = new Role
        {
            Id = role.Id,
            Name = role.Name,
        };
        return this;
    }

    public UserEventPayload SetEmailAddress(string emailAddress)
    {
        this.EmailAddress = PersonInformationHelper.GetMaskedEmailWithHashing(emailAddress);
        return this;
    }

    public UserEventPayload SetUser(Guid id, string displayName, string accountEmailAddress)
    {
        this.User = new User
        {
            Id = id,
            DisplayName = PersonInformationHelper.GetMaskedNameWithHashing(displayName),
            AccountEmailAddress = PersonInformationHelper.GetMaskedEmailWithHashing(accountEmailAddress),
        };
        return this;
    }
}
