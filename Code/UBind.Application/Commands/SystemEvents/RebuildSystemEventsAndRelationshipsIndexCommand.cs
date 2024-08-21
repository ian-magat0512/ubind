// <copyright file="RebuildSystemEventsAndRelationshipsIndexCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Commands.SystemEvents
{
    using MediatR;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// Command for rebuilding fragmented SystemEvents and Relationships Indices.
    /// Overtime, the system events and relationships indices may become fragmented
    /// due to the deletion of expired system events and associated relationship.
    /// </summary>
    [RetryOnDbException(0)]
    public class RebuildSystemEventsAndRelationshipsIndexCommand : ICommand<Unit>
    {
        public RebuildSystemEventsAndRelationshipsIndexCommand()
        {
        }
    }
}
