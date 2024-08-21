// <copyright file="IAggregateSnapshotRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.Aggregates;

using UBind.Domain.Models;

/// <summary>
/// This interface defines the contract for the repository that stores and retrieves aggregate snapshots.
/// It also retrieves aggregate snapshots by version.
/// </summary>
public interface IAggregateSnapshotRepository
{
    AggregateSnapshot? GetAggregateSnapshot(
        Guid tenantId,
        Guid aggregateId,
        AggregateType aggregateEntityType);

    Task<AggregateSnapshot?> GetAggregateSnapshotAsync(
        Guid tenantId,
        Guid aggregateId,
        AggregateType aggregateEntityType);

    void AddAggregateSnapshot(AggregateSnapshot aggregateSnapshot);

    Task<AggregateSnapshot?> GetAggregateSnapshotByVersion(
       Guid tenantId,
       Guid aggregateId,
       int version,
       AggregateType aggregateEntityType);

    Task DeleteOlderAggregateSnapshots(
        Guid tenantId,
        Guid id,
        Guid aggregateId,
        AggregateType aggregateType);

    void SaveChanges();

    Task SaveChangesAsync();
}
