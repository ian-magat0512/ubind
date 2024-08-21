// <copyright file="IAggregateLockingService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.Services;

using RedLockNet;
using UBind.Domain;

/// <summary>
/// This service is used to create locks across all instances of the application for the aggregate.
/// </summary>
public interface IAggregateLockingService : IDisposable
{
    /// <summary>
    /// Creates a lock across all instances of the application for the given tenant, aggregate Id and resource type.
    /// To ensure that the operation is executing on the latest version of the aggregate,
    /// the lock needs to be acquired before querying the aggregate from the db.
    /// To release the lock, the lock instance should be disposed.
    /// The lock as well has an expiry time of 30 seconds.
    /// <returns>The instance of the lock</returns>
    Task<IRedLock> CreateLockOrThrow(Guid tenantId, Guid aggregateId, AggregateType aggregateType);
}