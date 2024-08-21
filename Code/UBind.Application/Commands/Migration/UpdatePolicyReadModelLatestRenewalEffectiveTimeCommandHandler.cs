// <copyright file="UpdatePolicyReadModelLatestRenewalEffectiveTimeCommandHandler.cs" company="uBind">
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
    /// This is the handler to update the policy read model with latest renewal effective date.
    /// getting the latest creation date on policy transaction where the transaction type is renewal.
    /// </summary>
    public class UpdatePolicyReadModelLatestRenewalEffectiveTimeCommandHandler
        : ICommandHandler<UpdatePolicyReadModelLatestRenewalEffectiveTimeCommand, Unit>
    {
        private readonly IPolicyLatestRenewalEffectiveTimeMigration policyLatestRenewalMigraiton;
        private readonly ILogger<UpdatePolicyReadModelLatestRenewalEffectiveTimeCommandHandler> logger;

        public UpdatePolicyReadModelLatestRenewalEffectiveTimeCommandHandler(
            IPolicyLatestRenewalEffectiveTimeMigration policyLatestRenewalMigraiton,
            ILogger<UpdatePolicyReadModelLatestRenewalEffectiveTimeCommandHandler> logger)
        {
            this.policyLatestRenewalMigraiton = policyLatestRenewalMigraiton;
            this.logger = logger;
        }

        public Task<Unit> Handle(UpdatePolicyReadModelLatestRenewalEffectiveTimeCommand request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            this.logger.LogInformation("Starting migration..");
            this.policyLatestRenewalMigraiton.ProcessUpdatingPolicyRenewalEffectiveDate();
            return Task.FromResult(Unit.Value);
        }
    }
}
