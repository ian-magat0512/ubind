// <copyright file="IUserSystemEventEmitter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates;

using NodaTime;
using UBind.Domain.Aggregates.User;
using UBind.Domain.Events;
using UBind.Domain.ReadModel.User;

/// <summary>
/// Interface for user system event emitters, monitors events then trigger system event.
/// </summary>
public interface IUserSystemEventEmitter : IUserEventObserver, ISystemEventEmitter
{
    /// <summary>
    /// Create and emit user system events not related to
    /// an aggregate event from a userId
    /// for the automation to listen to.
    /// </summary>
    Task CreateAndEmitSystemEvents(Guid tenantId, Guid userId, List<SystemEventType> eventTypes, Guid? performingUserId = null, Instant? timestamp = null);

    /// <summary>
    /// Create and emit login system events from a userId
    /// for the automation to listen to.
    /// </summary>
    Task CreateAndEmitLoginSystemEvents(Guid tenantId, Guid userId, List<SystemEventType> eventTypes, Instant? timestamp = null);

    /// <summary>
    /// Create and emit login system events from a UserReadModel
    /// for the automation to listen to.
    /// </summary>
    Task CreateAndEmitLoginSystemEvents(UserReadModel user, List<SystemEventType> eventTypes, Instant? timestamp = null);
}
