// <copyright file="AlterCustomEventAliasColumnForSystemEventsCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Commands.Migration
{
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// Command that alters the CustomEventAlias column for SystemEvents table
    /// to use a maximum length of 255.
    /// </summary>
    [RetryOnDbException(0)]
    public class AlterCustomEventAliasColumnForSystemEventsCommand : ICommand
    {
    }
}
