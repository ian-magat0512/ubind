// <copyright file="IDomainEventHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Notifications
{
    using MediatR;

    /// <summary>
    /// Contract for classes that represents the domain event handler
    /// that reacts to a specific type of domain event.
    /// </summary>
    /// <typeparam name="TEvent">DomainEvent type.</typeparam>
    public interface IDomainEventHandler<TEvent> : INotificationHandler<TEvent>
        where TEvent : DomainEvent
    {
    }
}
