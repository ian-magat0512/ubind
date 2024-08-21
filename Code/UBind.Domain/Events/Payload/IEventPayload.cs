// <copyright file="IEventPayload.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.Events.Payload
{
    using UBind.Domain.Events.Models;

    /// <summary>
    /// Interface class for the event payload.
    /// </summary>
    public interface IEventPayload
    {
        User? PerformingUser { get; set; }

        Tenant? Tenant { get; set; }
    }
}
