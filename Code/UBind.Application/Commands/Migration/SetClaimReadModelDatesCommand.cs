// <copyright file="SetClaimReadModelDatesCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Commands.Migration
{
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// Command for setting the claim lodge, settled and declined dates.
    /// This is a migration command called on startup.
    /// </summary>
    public class SetClaimReadModelDatesCommand : ICommand
    {
    }
}
