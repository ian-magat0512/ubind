// <copyright file="UpdatePolicyStateFromPendingToIssuedCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Migration
{
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using Microsoft.Extensions.Logging;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Services.Migration;

    /// <summary>
    /// This is to update all policies that are pending state to issued state.
    /// </summary>
    public class UpdatePolicyStateFromPendingToIssuedCommandHandler : ICommandHandler<UpdatePolicyStateFromPendingToIssuedCommand, Unit>
    {
        private readonly IPolicyStateMigration policyMigration;
        private readonly ILogger<UpdatePolicyStateFromPendingToIssuedCommand> logger;

        public UpdatePolicyStateFromPendingToIssuedCommandHandler(
            IPolicyStateMigration policyMigration,
            ILogger<UpdatePolicyStateFromPendingToIssuedCommand> logger)
        {
            this.policyMigration = policyMigration;
            this.logger = logger;
        }

        public Task<Unit> Handle(UpdatePolicyStateFromPendingToIssuedCommand command, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            this.logger.LogInformation("Starting migration of policy state from pending to issued..");
            this.policyMigration.ProcessUpdatePolicyStateFromPendingToIssued();
            return Task.FromResult(Unit.Value);
        }
    }
}
