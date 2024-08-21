// <copyright file="DeleteExpiredSystemEventsCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Commands.SystemEvents
{
    using MediatR;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// This command is used to delete expired system events.
    /// </summary>
    [RetryOnDbException(0)]
    public class DeleteExpiredSystemEventsCommand : ICommand<Unit>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteExpiredSystemEventsCommand"/> class.
        /// </summary>
        public DeleteExpiredSystemEventsCommand()
        {
        }
    }
}
