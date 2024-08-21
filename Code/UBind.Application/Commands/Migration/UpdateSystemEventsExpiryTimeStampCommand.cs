// <copyright file="UpdateSystemEventsExpiryTimeStampCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Commands.Migration
{
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// This command updates the system events expiry timestamp.
    /// This is ran in migration to make sure that system events
    /// follows the documented persistence.
    /// </summary>
    public class UpdateSystemEventsExpiryTimeStampCommand : ICommand
    {
    }
}
