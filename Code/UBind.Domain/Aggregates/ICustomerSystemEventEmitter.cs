// <copyright file="ICustomerSystemEventEmitter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates;

using NodaTime;
using UBind.Domain.Aggregates.Customer;
using UBind.Domain.Events;

/// <summary>
/// Interface for customer system event emitters, monitors events then trigger system event.
/// </summary>
public interface ICustomerSystemEventEmitter : ICustomerEventObserver, ISystemEventEmitter
{
    /// <summary>
    /// Create and emit system events not related to an aggregate event
    /// for the automation to listen to that does not relate to the aggregate.
    /// </summary>
    Task CreateAndEmitSystemEvents(Guid tenantId, Guid customerId, List<SystemEventType> eventTypes, Guid? performingUserId, Instant? timestamp = null);
}
