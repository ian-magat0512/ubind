// <copyright file="UpdatePolicyReadModelLatestExpiryDateTimeCommandHandler.cs" company="uBind">
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
    using UBind.Domain.Services;

    public class UpdatePolicyReadModelLatestExpiryDateTimeCommandHandler
        : ICommandHandler<UpdatePolicyReadModelLatestExpiryDateTimeCommand, Unit>
    {
        private readonly IPolicyLatestExpiryDateTimeMigration policyLatestExpiryDateTimeMigration;
        private readonly ILogger<UpdatePolicyReadModelLatestExpiryDateTimeCommandHandler> logger;

        public UpdatePolicyReadModelLatestExpiryDateTimeCommandHandler(
            IPolicyLatestExpiryDateTimeMigration policyLatestExpiryDateTimeMigration,
            ILogger<UpdatePolicyReadModelLatestExpiryDateTimeCommandHandler> logger)
        {
            this.policyLatestExpiryDateTimeMigration = policyLatestExpiryDateTimeMigration;
            this.logger = logger;
        }

        public async Task<Unit> Handle(UpdatePolicyReadModelLatestExpiryDateTimeCommand request, CancellationToken cancellationToken)
        {
            this.logger.LogInformation("Starting migration..");
            await this.policyLatestExpiryDateTimeMigration.ProcessUpdatingPolicyLatestExpiryDateTime(cancellationToken);
            return Unit.Value;
        }
    }
}
