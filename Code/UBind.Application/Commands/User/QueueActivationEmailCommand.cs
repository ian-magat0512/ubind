// <copyright file="QueueActivationEmailCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.User
{
    using System;
    using UBind.Domain;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// The command for queueing activation email.
    /// </summary>
    public class QueueActivationEmailCommand : ICommand
    {
        public QueueActivationEmailCommand(Guid tenantId, Guid userId, DeploymentEnvironment environment)
        {
            this.TenantId = tenantId;
            this.UserId = userId;
            this.Environment = environment;
        }

        public Guid TenantId { get; private set; }

        /// <summary>
        /// Gets the Id of the user to where the system will send the activation email.
        /// </summary>
        public Guid UserId { get; private set; }

        public DeploymentEnvironment Environment { get; }
    }
}
