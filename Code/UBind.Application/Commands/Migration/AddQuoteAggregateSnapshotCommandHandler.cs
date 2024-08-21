// <copyright file="AddQuoteAggregateSnapshotCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Commands.Migration;

using MediatR;
using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.Services.Migration;

/// <summary>
/// This command is used for migration purposes to add a quote aggregate snapshot
/// for existing policy records that do not have a snapshot.
/// It will serialize the latest quote aggregate and add it to the database.
/// This allows the snapshot to be used to rehydrate the aggregate during a query (GetById),
/// improving performance.
/// </summary>
public class AddQuoteAggregateSnapshotCommandHandler : ICommandHandler<AddQuoteAggregateSnapshotCommand, Unit>
{
    private readonly IAddQuoteAggregateSnapshotMigration quoteAggregateSnapshotMigration;

    public AddQuoteAggregateSnapshotCommandHandler(IAddQuoteAggregateSnapshotMigration quoteAggregateSnapshotMigration)
    {
        this.quoteAggregateSnapshotMigration = quoteAggregateSnapshotMigration;
    }

    public async Task<Unit> Handle(AddQuoteAggregateSnapshotCommand command, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await this.quoteAggregateSnapshotMigration.ProcessQuoteAggregateSnapshotForExistingRecords(cancellationToken);
        return Unit.Value;
    }
}
