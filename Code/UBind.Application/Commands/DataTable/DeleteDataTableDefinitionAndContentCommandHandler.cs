// <copyright file="DeleteDataTableDefinitionAndContentCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.DataTable
{
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using UBind.Domain;
    using UBind.Domain.Aggregates;
    using UBind.Domain.Events;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Repositories;

    /// <summary>
    /// Flagged the data table definition entry as deleted and drop its associated database table.
    /// </summary>
    public class DeleteDataTableDefinitionAndContentCommandHandler
        : ICommandHandler<DeleteDataTableDefinitionAndContentCommand, Unit>
    {
        private readonly IDataTableDefinitionRepository dataTableDefinitionRepository;
        private readonly IDataTableContentRepository dataTableContentRepository;
        private readonly IUBindDbContext dbContext;
        private readonly ITenantSystemEventEmitter tenantSystemEventEmitter;
        private readonly IOrganisationSystemEventEmitter organisationSystemEventEmitter;

        public DeleteDataTableDefinitionAndContentCommandHandler(
            IDataTableDefinitionRepository dataTableDefinitionRepository,
            IDataTableContentRepository dataTableContentRepository,
            IUBindDbContext dbContext,
            ITenantSystemEventEmitter tenantSystemEventEmitter,
            IOrganisationSystemEventEmitter organisationSystemEventEmitter)
        {
            this.dataTableDefinitionRepository = dataTableDefinitionRepository;
            this.dataTableContentRepository = dataTableContentRepository;
            this.dbContext = dbContext;
            this.tenantSystemEventEmitter = tenantSystemEventEmitter;
            this.organisationSystemEventEmitter = organisationSystemEventEmitter;
        }

        public async Task<Unit> Handle(DeleteDataTableDefinitionAndContentCommand request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var dataTableDefinition = this.dataTableDefinitionRepository.GetDataTableDefinitionById(request.TenantId, request.DefinitionId);
            if (dataTableDefinition == null)
            {
                throw new ErrorException(Domain.Errors.DataTableDefinition.NotFound(request.DefinitionId));
            }

            await this.dataTableContentRepository.DropDataTableContent(
                dataTableDefinition.Id, dataTableDefinition.DatabaseTableName);

            await this.dataTableDefinitionRepository.RemoveDataTableDefinition(dataTableDefinition);

            if (dataTableDefinition.EntityType == EntityType.Tenant)
            {
                await this.tenantSystemEventEmitter.CreateAndEmitSystemEvent(dataTableDefinition.TenantId, SystemEventType.TenantModified);
            }
            else if (dataTableDefinition.EntityType == EntityType.Organisation)
            {
                await this.organisationSystemEventEmitter.CreateAndEmitModifiedSystemEvent(dataTableDefinition.TenantId, dataTableDefinition.EntityId);
            }

            return Unit.Value;
        }
    }
}
