// <copyright file="RecreateModelsOfEventsCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Migration
{
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// Command for recreating read models for aggregates.
    /// This is important because some aggregates does not have read models for whatever reason,
    /// as those readmodels are expected. This command will replay the events associated with an aggregate,
    /// sending them to the read model writer, which will ultimately regenerate the read model
    /// up to the current point in time.
    /// </summary>
    public class RecreateModelsOfEventsCommand : ICommand
    {
    }
}
