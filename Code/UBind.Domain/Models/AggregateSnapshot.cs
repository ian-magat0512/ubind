// <copyright file="AggregateSnapshot.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.Models;

using NodaTime;

/// <summary>
/// Represents a snapshot of an aggregate entity.
/// It should be used for performance optimization.
/// Every 100 time an aggregate is saved, a snapshot should be created.
/// The Json property should contain the serialized aggregate.
/// The version was the last version of the aggregate when the snapshot was created.
/// </summary>
public class AggregateSnapshot : Entity<Guid>
{
    public AggregateSnapshot(
        Guid tenantId,
        Guid aggregateId,
        AggregateType aggregateType,
        int version,
        string json,
        Instant createdTimestamp)
        : base(Guid.NewGuid(), createdTimestamp)
    {
        this.TenantId = tenantId;
        this.AggregateId = aggregateId;
        this.AggregateType = aggregateType;
        this.Version = version;
        this.Json = json;
    }

    /// <summary>
    /// Parameterless constructor for Entity Framework.
    /// </summary>
    private AggregateSnapshot()
        : base(default, default)
    {
        this.Json = string.Empty;
    }

    public Guid TenantId { get; private set; }

    public Guid AggregateId { get; private set; }

    public AggregateType AggregateType { get; private set; }

    public int Version { get; private set; }

    public string Json { get; private set; }
}
