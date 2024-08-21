// <copyright file="CreateDataTableFromCsvDataCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Commands.DataTable
{
    using System.Threading;
    using System.Threading.Tasks;
    using NodaTime;
    using UBind.Application.Services;
    using UBind.Domain;
    using UBind.Domain.Aggregates;
    using UBind.Domain.Entities;
    using UBind.Domain.Events;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Models.DataTable;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Repositories;

    /// <summary>
    /// Handler for creating data table definition and content command.
    /// Creates a data table definition entry, and then create a new database table
    /// and load the contents of the CSV data into the new database table.
    /// The first row of the CSV data will be used to determine the column names of the new database table,
    /// and the rest of the CSV data will be loaded in as the data table content.
    /// </summary>
    public class CreateDataTableFromCsvDataCommandHandler
        : ICommandHandler<CreateDataTableFromCsvDataCommand, DataTableDefinition>
    {
        private const int MaxCsvDataSize = 20971520; // 20MB
        private readonly IDataTableDefinitionRepository dataTableDefinitionRepository;
        private readonly IDataTableContentRepository dataTableContentRepository;
        private readonly IDataTableDefinitionService dataTableDefinitionService;
        private readonly IClock clock;
        private readonly ITenantSystemEventEmitter tenantSystemEventEmitter;
        private readonly IOrganisationSystemEventEmitter organisationSystemEventEmitter;
        private readonly IUBindDbContext context;

        public CreateDataTableFromCsvDataCommandHandler(
            IDataTableDefinitionRepository dataTableDefinitionRepository,
            IDataTableContentRepository dataTableContentRepository,
            IDataTableDefinitionService dataTableDefinitionService,
            IClock clock,
            ITenantSystemEventEmitter tenantSystemEventEmitter,
            IOrganisationSystemEventEmitter organisationSystemEventEmitter,
            IUBindDbContext context)
        {
            this.dataTableDefinitionRepository = dataTableDefinitionRepository;
            this.dataTableContentRepository = dataTableContentRepository;
            this.dataTableDefinitionService = dataTableDefinitionService;
            this.clock = clock;
            this.tenantSystemEventEmitter = tenantSystemEventEmitter;
            this.organisationSystemEventEmitter = organisationSystemEventEmitter;
            this.context = context;
        }

        public async Task<DataTableDefinition> Handle(CreateDataTableFromCsvDataCommand request, CancellationToken cancellationToken)
        {
            DataTableDefinition dataTableDefinition;

            this.dataTableDefinitionService.ValidateTableSchema(request.TableSchema.ToString());
            var tableSchema = request.TableSchema;

            this.dataTableDefinitionService.ValidateColumns(tableSchema);
            this.dataTableDefinitionService.ValidateTableSchemaIndexes(tableSchema);

            System.Data.DataTable csvDataTable = this.dataTableDefinitionService.CreateDataTableFromCsv(tableSchema, request.CsvData);

            cancellationToken.ThrowIfCancellationRequested();

            // We need transaction to revert any created table/views when an exceptions occurs and also revert any data created in DataTableDefinition.
            // It is initiated here due to needing a granular approach on managing it.
            // Currently, the way to initialize this is by adding an attribute to command class which will
            // encompass the handler to be executed in a TransactionScope.
            // Because we're handling an unknown size of data, it is safer to not include the population of data inside a transaction.
            // If there's a reference/solution of a better approach in doing this, a refactor would be necessary.
            using var transaction = this.context.Database.BeginTransaction();
            try
            {
                dataTableDefinition = await this.CreateDataTableDefinition(request, tableSchema, csvDataTable);
                await this.dataTableContentRepository.CreateDataTableContentSchema(
                     dataTableDefinition, tableSchema, transaction);
                await this.dataTableContentRepository.CreateOrAlterSchemaBoundView(
                    dataTableDefinition.Id,
                    dataTableDefinition.DatabaseTableName,
                    tableSchema.Columns, transaction);
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }

            // Currently, there's no limit to how much data can be passed besides the request content limit.
            // Since the size of data is unknown, we cannot include the Population of datatable in a transaction due to performance concerns.
            // Still we must handle data cleanup on exceptions.
            try
            {
                await this.dataTableContentRepository.PopulateDataTable(
                    dataTableDefinition.DatabaseTableName, csvDataTable);
                await this.dataTableContentRepository.ReseedDataTableContentToCache(dataTableDefinition);
            }
            catch
            {
                await this.dataTableContentRepository.DropDataTableContent(
                    dataTableDefinition.Id, dataTableDefinition.DatabaseTableName);
                await this.HandleRollbackOfCreatedDataTableDefinition(dataTableDefinition.TenantId, dataTableDefinition.Id);
                throw;
            }

            return dataTableDefinition;
        }

        private async Task<DataTableDefinition> CreateDataTableDefinition(
            CreateDataTableFromCsvDataCommand request,
            DataTableSchema dataTableSchema,
            System.Data.DataTable dataTableContent)
        {
            if (this.dataTableDefinitionRepository.IsNameIsInUse(request.TenantId, request.EntityId, request.DataTableName))
            {
                throw new ErrorException(Errors.DataTableDefinition.NameInUse(request.DataTableName));
            }

            if (this.dataTableDefinitionRepository.IsAliasIsInUse(request.TenantId, request.EntityId, request.DataTableAlias))
            {
                throw new ErrorException(Errors.DataTableDefinition.AliasInUse(request.DataTableAlias));
            }

            if (request.CsvData.Length > MaxCsvDataSize && request.MemoryCachingEnabled) // 20MB
            {
                throw new ErrorException(Errors.DataTableDefinition.CsvDataTooLarge());
            }

            var dataTableDefinition = DataTableDefinition.Create(
                request.TenantId,
                request.EntityType,
                request.EntityId,
                request.DataTableName,
                request.DataTableAlias,
                request.MemoryCachingEnabled,
                request.CacheExpiryInSeconds,
                request.TableSchema.ToString(),
                dataTableSchema.Columns.Count(),
                dataTableContent.Rows.Count,
                this.clock.Now());
            var finalDatabaseTableName = await this.dataTableDefinitionService.GenerateDatabaseTableName(dataTableDefinition);
            dataTableDefinition.UpdateDatabaseTableName(finalDatabaseTableName);
            this.dataTableDefinitionRepository.CreateDataTableDefinition(dataTableDefinition);

            if (request.EntityType == EntityType.Tenant)
            {
                await this.tenantSystemEventEmitter.CreateAndEmitSystemEvent(request.TenantId, SystemEventType.TenantModified);
            }
            else if (request.EntityType == EntityType.Organisation)
            {
                await this.organisationSystemEventEmitter.CreateAndEmitModifiedSystemEvent(request.TenantId, request.EntityId);
            }

            return dataTableDefinition;
        }

        /// <summary>
        /// Handle the rollback of datatable definition record, created tables and views.
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="datatableDefinitionId"></param>
        /// <returns></returns>
        /// <exception cref="ErrorException"></exception>
        private async Task HandleRollbackOfCreatedDataTableDefinition(Guid tenantId, Guid datatableDefinitionId)
        {
            var dataTableDefinition = this.dataTableDefinitionRepository.GetDataTableDefinitionById(tenantId, datatableDefinitionId);
            if (dataTableDefinition == null)
            {
                throw new ErrorException(Domain.Errors.DataTableDefinition.NotFound(datatableDefinitionId));
            }

            await this.dataTableContentRepository.DropDataTableContent(
                dataTableDefinition.Id, dataTableDefinition.DatabaseTableName);

            await this.dataTableDefinitionRepository.RemoveDataTableDefinition(dataTableDefinition);
        }
    }
}
