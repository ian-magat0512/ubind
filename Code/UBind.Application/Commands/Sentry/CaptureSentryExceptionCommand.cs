// <copyright file="CaptureSentryExceptionCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Sentry
{
    using System;
    using UBind.Domain;
    using UBind.Domain.Attributes;
    using UBind.Domain.Patterns.Cqrs;

    // Normally a command is assumed to have a RequestIntent of ReadWrite, but this command doesn't
    // write to the database, so it can be marked as ReadOnly.
    [RequestIntent(RequestIntent.ReadOnly)]
    public class CaptureSentryExceptionCommand : ICommand
    {
        public CaptureSentryExceptionCommand(
            Exception exception,
            DeploymentEnvironment? environment = null,
            object? additionalContext = null)
        {
            this.Exception = exception;
            this.Environment = environment;
            this.AdditionalContext = additionalContext;
        }

        public Exception Exception { get; set; }

        public DeploymentEnvironment? Environment { get; set; }

        public object? AdditionalContext { get; set; }
    }
}
