// <copyright file="RegenerateAggregateSnapshotCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Commands.Migration;

using MediatR;
using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.Services.Migration;

/// <summary>
/// This command is used for migration purposes to regenerate aggregate snapshots.
/// Since some property was being stored twice in the database, we need to regenerate the snapshots.
/// This will free up space in the database and improve performance.
/// </summary>
public class RegenerateAggregateSnapshotCommandHandler : ICommandHandler<RegenerateAggregateSnapshotCommand, Unit>
{
    private readonly IRegenerateAggregateSnapshotMigration regenerateAggregateSnapshotMigration;

    public RegenerateAggregateSnapshotCommandHandler(IRegenerateAggregateSnapshotMigration regenerateAggregateSnapshotMigration)
    {
        this.regenerateAggregateSnapshotMigration = regenerateAggregateSnapshotMigration;
    }

    public async Task<Unit> Handle(RegenerateAggregateSnapshotCommand request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await this.regenerateAggregateSnapshotMigration.RegenerateAggregateSnapshots(cancellationToken);
        return Unit.Value;
    }
}
