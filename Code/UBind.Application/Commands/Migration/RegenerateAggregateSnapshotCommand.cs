// <copyright file="RegenerateAggregateSnapshotCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Commands.Migration;

using UBind.Domain.Patterns.Cqrs;

/// <summary>
/// This command is used for migration purposes to regenerate aggregate snapshots.
/// Since some property was being stored twice in the database, we need to regenerate the snapshots.
/// This will free up space in the database and improve performance.
/// </summary>
public class RegenerateAggregateSnapshotCommand : ICommand
{
}
