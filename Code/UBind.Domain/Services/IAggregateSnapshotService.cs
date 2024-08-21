// <copyright file="IAggregateSnapshotService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.Services;

using UBind.Domain.Models;

/// <summary>
/// This service is responsible for adding and retrieving aggregate snapshots.
/// We have a generic type TAggregate which is an aggregate entity.
/// This service is used to serialize and deserialize the aggregate entity.
/// It used by the aggregate repository to save and retrieve the aggregate snapshot.
/// </summary>
/// <typeparam name="TAggregate">The generic type of aggregate entity.</typeparam>
public interface IAggregateSnapshotService<TAggregate>
{
    Task AddAggregateSnapshot(
        Guid tenantId,
        TAggregate aggregate,
        int version);

    AggregateSnapshotResult<TAggregate>? GetAggregateSnapshot(
        Guid tenantId,
        Guid aggregateId,
        AggregateType aggregateType);

    Task<AggregateSnapshotResult<TAggregate>?> GetAggregateSnapshotAsync(
        Guid tenantId,
        Guid aggregateId,
        AggregateType aggregateType);

    Task<AggregateSnapshotResult<TAggregate>?> GetAggregateSnapshotByVersion(
        Guid tenantId,
        Guid aggregateId,
        int version,
        AggregateType aggregateType);

    string SerializeAggregate(TAggregate aggregate);
}
