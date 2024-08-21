// <copyright file="SetTenantIdOfRelationshipsCommandHandler.cs" company="uBind">
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

    public class SetTenantIdOfRelationshipsCommandHandler
        : ICommandHandler<SetTenantIdOfRelationshipsCommand, Unit>
    {
        private readonly ISetTenantIdOfRelationshipMigration migrationService;
        private readonly ILogger<ISetTenantIdOfRelationshipMigration> logger;

        public SetTenantIdOfRelationshipsCommandHandler(
            ISetTenantIdOfRelationshipMigration migrationService,
            ILogger<ISetTenantIdOfRelationshipMigration> logger)
        {
            this.migrationService = migrationService;
            this.logger = logger;
        }

        public Task<Unit> Handle(SetTenantIdOfRelationshipsCommand request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            this.logger.LogInformation("Starting migration..");
            this.migrationService.SetTenantIdfOfRelationships();
            return Task.FromResult(Unit.Value);
        }
    }
}
