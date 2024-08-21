// <copyright file="UpdateCustomerOwnerUserIdFromDefaultGuidToNullCommandHandler.cs" company="uBind">
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

    public class UpdateCustomerOwnerUserIdFromDefaultGuidToNullCommandHandler : ICommandHandler<UpdateCustomerOwnerUserIdFromDefaultGuidToNullCommand, Unit>
    {
        private readonly ICustomerUserOwnerIdMigration customerMigration;
        private readonly ILogger<UpdateCustomerOwnerUserIdFromDefaultGuidToNullCommand> logger;

        public UpdateCustomerOwnerUserIdFromDefaultGuidToNullCommandHandler(
            ICustomerUserOwnerIdMigration customerMigration,
            ILogger<UpdateCustomerOwnerUserIdFromDefaultGuidToNullCommand> logger)
        {
            this.customerMigration = customerMigration;
            this.logger = logger;
        }

        public Task<Unit> Handle(UpdateCustomerOwnerUserIdFromDefaultGuidToNullCommand request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            this.logger.LogInformation("Starting migration of customer UserOwnerId from default guid to null..");
            this.customerMigration.ProcessUpdatingCustomerUserOwnerId();
            return Task.FromResult(Unit.Value);
        }
    }
}
