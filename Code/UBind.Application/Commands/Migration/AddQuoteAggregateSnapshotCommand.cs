// <copyright file="AddQuoteAggregateSnapshotCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Commands.Migration;

using UBind.Domain.Patterns.Cqrs;

/// <summary>
/// This command is used for migration purposes to add a quote aggregate snapshot
/// for existing policy records that do not have a snapshot.
/// It will serialize the latest quote aggregate and add it to the database.
/// This allows the snapshot to be used to rehydrate the aggregate during a query (GetById),
/// improving performance.
/// </summary>
public class AddQuoteAggregateSnapshotCommand : ICommand
{
}
