// <copyright file="AggregateSnapshotResult.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.Models;

/// <summary>
/// This the model for the result of the aggregate snapshot.
/// The aggregate was the deserialize aggregate and the version is the version of the aggregate was being stored.
/// </summary>
/// <typeparam name="TAggregate">The generic type of the aggregate entity. [QuoteAggregate etc.]</typeparam>
public class AggregateSnapshotResult<TAggregate>
{
    public AggregateSnapshotResult(TAggregate aggregate, int version)
    {
        this.Aggregate = aggregate;
        this.Version = version;
    }

    public TAggregate Aggregate { get; private set; }

    public int Version { get; private set; }
}
