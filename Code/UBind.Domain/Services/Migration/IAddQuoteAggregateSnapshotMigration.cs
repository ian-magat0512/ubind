// <copyright file="IAddQuoteAggregateSnapshotMigration.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.Services.Migration;

/// <summary>
/// This is the interface for the migration service to add quote aggregate snapshot for
/// existing policy records that do not have a snapshot.
/// </summary>
public interface IAddQuoteAggregateSnapshotMigration
{
    Task ProcessQuoteAggregateSnapshotForExistingRecords(CancellationToken cancellationToken);
}
