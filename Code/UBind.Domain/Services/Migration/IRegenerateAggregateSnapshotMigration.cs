// <copyright file="IRegenerateAggregateSnapshotMigration.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.Services.Migration;

/// <summary>
/// This service is used for migration purposes to regenerate aggregate snapshots.
/// Since some property was being stored twice in the database, we need to regenerate the snapshots.
/// This will free up space in the database and improve performance.
/// </summary>
public interface IRegenerateAggregateSnapshotMigration
{
    Task RegenerateAggregateSnapshots(CancellationToken cancellationToken);
}
