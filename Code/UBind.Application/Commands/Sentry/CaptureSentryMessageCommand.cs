// <copyright file="CaptureSentryMessageCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Sentry
{
    using UBind.Domain;
    using UBind.Domain.Patterns.Cqrs;

    public class CaptureSentryMessageCommand : ICommand
    {
        public CaptureSentryMessageCommand(
            string message,
            DeploymentEnvironment? environment = null,
            Dictionary<string, string>? tags = null)
        {
            this.Message = message;
            this.Environment = environment;
            this.Tags = tags;
        }

        public string Message { get; }

        public DeploymentEnvironment? Environment { get; }

        public Dictionary<string, string>? Tags { get; }
    }
}
