// <copyright file="AddCustomerRelationshipForQuayCommercialHullEmailsCommandHandler.cs" company="uBind">
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

    public class AddCustomerRelationshipForQuayCommercialHullEmailsCommandHandler : ICommandHandler<AddCustomerRelationshipForQuayCommercialHullEmailsCommand, Unit>
    {
        private readonly IMissingCustomerRelationshipMigration customerMigration;
        private readonly ILogger<AddCustomerRelationshipForQuayCommercialHullEmailsCommand> logger;

        public AddCustomerRelationshipForQuayCommercialHullEmailsCommandHandler(
            IMissingCustomerRelationshipMigration customerMigration,
            ILogger<AddCustomerRelationshipForQuayCommercialHullEmailsCommand> logger)
        {
            this.customerMigration = customerMigration;
            this.logger = logger;
        }

        public Task<Unit> Handle(AddCustomerRelationshipForQuayCommercialHullEmailsCommand request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            this.logger.LogInformation("Starting migration for quay commercial hull");
            this.customerMigration.AddMissingCustomerRelationshipForQuayCommercialHull();
            return Task.FromResult(Unit.Value);
        }
    }
}
