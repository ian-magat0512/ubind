// <copyright file="UpdateDataTableFromCsvDataCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Commands.DataTable
{
    using System.Threading;
    using System.Threading.Tasks;
    using UBind.Application.Services;
    using UBind.Domain;
    using UBind.Domain.Aggregates;
    using UBind.Domain.Entities;
    using UBind.Domain.Events;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Models.DataTable;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Repositories;

    /// <summary>
    /// Handler for updating data table definition and content command.
    /// Updates a data table definition entry, and then recreate the database table
    /// and load the contents of the CSV data into the it.
    /// </summary>
    public class UpdateDataTableFromCsvDataCommandHandler
        : ICommandHandler<UpdateDataTableFromCsvDataCommand, DataTableDefinition>
    {
        private const int MaxCsvDataSize = 20971520; // 20MB
        private readonly IDataTableDefinitionRepository dataTableDefinitionRepository;
        private readonly IDataTableContentRepository dataTableContentRepository;
        private readonly IDataTableDefinitionService dataTableDefinitionService;
        private readonly IUBindDbContext uBindDbContext;
        private readonly ITenantSystemEventEmitter tenantSystemEventEmitter;
        private readonly IOrganisationSystemEventEmitter organisationSystemEventEmitter;

        public UpdateDataTableFromCsvDataCommandHandler(
            IDataTableDefinitionRepository dataTableDefinitionRepository,
            IDataTableContentRepository dataTableContentRepository,
            IDataTableDefinitionService dataTableDefinitionService,
            IUBindDbContext uBindDbContext,
            ITenantSystemEventEmitter tenantSystemEventEmitter,
            IOrganisationSystemEventEmitter organisationSystemEventEmitter)
        {
            this.dataTableDefinitionRepository = dataTableDefinitionRepository;
            this.dataTableContentRepository = dataTableContentRepository;
            this.dataTableDefinitionService = dataTableDefinitionService;
            this.uBindDbContext = uBindDbContext;
            this.tenantSystemEventEmitter = tenantSystemEventEmitter;
            this.organisationSystemEventEmitter = organisationSystemEventEmitter;
        }

        public async Task<Domain.Entities.DataTableDefinition> Handle(UpdateDataTableFromCsvDataCommand request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (request.CsvData.Length > MaxCsvDataSize && request.MemoryCachingEnabled) // 20MB
            {
                throw new ErrorException(Errors.DataTableDefinition.CsvDataTooLarge());
            }

            var dataTableDefinition = this.dataTableDefinitionRepository.GetDataTableDefinitionById(request.TenantId, request.DefinitionId);
            if (dataTableDefinition == null)
            {
                throw new ErrorException(Domain.Errors.DataTableDefinition.NotFound(request.DefinitionId));
            }

            DataTableSchema tableSchema = request.TableSchema;
            string tableSchemaJson = request.TableSchema.ToString();

            var oldDataTableDefinition = this.CopyDataTableDefinition(dataTableDefinition);
            this.dataTableDefinitionService.ValidateTableSchema(tableSchemaJson);
            this.dataTableDefinitionService.ValidateColumns(tableSchema);
            this.dataTableDefinitionService.ValidateTableSchemaIndexes(tableSchema);

            System.Data.DataTable dataTableContent = this.dataTableDefinitionService.CreateDataTableFromCsv(tableSchema, request.CsvData);

            // We need transaction to revert any created table/views when an exceptions occurs and also revert any data changes in DataTableDefinition.
            // It is initiated here due to needing a granular approach on managing it.
            // Currently, the way to initialize this is by adding an attribute to command class which will
            // encompass the handler to be executed in a TransactionScope.
            // Because we're handling an unknown size of data, it is safer to not include the population of data inside a transaction.
            // If there's a reference/solution of a better approach in doing this, a refactor would be necessary.
            using var transaction = this.uBindDbContext.Database.BeginTransaction();
            try
            {
                await this.UpdateDataTableDefinition(
                    dataTableDefinition,
                    request.DataTableName,
                    request.DataTableAlias,
                    request.MemoryCachingEnabled,
                    request.CacheExpiryInSeconds,
                    tableSchemaJson,
                    dataTableContent.Rows.Count,
                    dataTableContent.Columns.Count);
                await this.UpdateDataTableDatabaseName(dataTableDefinition);

                await this.dataTableContentRepository.CreateDataTableContentSchema(
                    dataTableDefinition, tableSchema, transaction);
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
                    dataTableDefinition.DatabaseTableName, dataTableContent);
                await this.dataTableContentRepository.ReseedDataTableContentToCache(dataTableDefinition);
                await this.dataTableContentRepository.CreateOrAlterSchemaBoundView(
                    dataTableDefinition.Id, dataTableDefinition.DatabaseTableName, tableSchema.Columns);
                await this.dataTableContentRepository.DropDataTableIfExists(oldDataTableDefinition.DatabaseTableName);
                await this.EmitSystemEvent(dataTableDefinition);
            }
            catch
            {
                await this.HandleRollbackOfUpdatedDataTableDefinition(dataTableDefinition.TenantId, dataTableDefinition.Id, oldDataTableDefinition);
                throw;
            }

            return dataTableDefinition;
        }

        private async Task EmitSystemEvent(DataTableDefinition dataTableDefinition)
        {
            if (dataTableDefinition.EntityType == EntityType.Tenant)
            {
                await this.tenantSystemEventEmitter.CreateAndEmitSystemEvent(dataTableDefinition.TenantId, SystemEventType.TenantModified);
            }
            else if (dataTableDefinition.EntityType == EntityType.Organisation)
            {
                await this.organisationSystemEventEmitter.CreateAndEmitModifiedSystemEvent(dataTableDefinition.TenantId, dataTableDefinition.EntityId);
            }
        }

        private Domain.Entities.DataTableDefinition CopyDataTableDefinition(Domain.Entities.DataTableDefinition dataTableDefinition)
        {
            var oldDataTableDefinition = Domain.Entities.DataTableDefinition.Create(
                dataTableDefinition.TenantId,
                dataTableDefinition.EntityType,
                dataTableDefinition.EntityId,
                dataTableDefinition.Name,
                dataTableDefinition.Alias,
                dataTableDefinition.MemoryCachingEnabled,
                dataTableDefinition.CacheExpiryInSeconds,
                dataTableDefinition.TableSchemaJson,
                dataTableDefinition.ColumnCount,
                dataTableDefinition.RecordCount,
                dataTableDefinition.LastModifiedTimestamp);

            oldDataTableDefinition.UpdateDatabaseTableName(dataTableDefinition.DatabaseTableName);
            oldDataTableDefinition.UpdateRecordAndColumnCount(dataTableDefinition.RecordCount, dataTableDefinition.ColumnCount);

            return oldDataTableDefinition;
        }

        private async Task UpdateDataTableDatabaseName(DataTableDefinition dataTableDefinition)
        {
            var finalDatabaseTableName = await this.dataTableDefinitionService.GenerateDatabaseTableName(dataTableDefinition);
            dataTableDefinition.UpdateDatabaseTableName(finalDatabaseTableName);
            await this.uBindDbContext.SaveChangesAsync();
        }

        private async Task HandleRollbackOfUpdatedDataTableDefinition(Guid tenantId, Guid datatableDefinitionId, Domain.Entities.DataTableDefinition oldDataTableDefinition)
        {
            var dataTableDefinition = this.dataTableDefinitionRepository.GetDataTableDefinitionById(tenantId, datatableDefinitionId);
            if (dataTableDefinition == null)
            {
                throw new ErrorException(Domain.Errors.DataTableDefinition.NotFound(datatableDefinitionId));
            }

            await this.dataTableContentRepository.DropDataTableContent(
                dataTableDefinition.Id, dataTableDefinition.DatabaseTableName);

            await this.UpdateDataTableDefinition(
                dataTableDefinition,
                oldDataTableDefinition.Name,
                oldDataTableDefinition.Alias,
                oldDataTableDefinition.MemoryCachingEnabled,
                oldDataTableDefinition.CacheExpiryInSeconds,
                oldDataTableDefinition.TableSchemaJson,
                oldDataTableDefinition.RecordCount,
                oldDataTableDefinition.ColumnCount);
            await this.UpdateDataTableDatabaseName(oldDataTableDefinition);
        }

        private async Task UpdateDataTableDefinition(
            Domain.Entities.DataTableDefinition dataTableDefinition,
            string name,
            string alias,
            bool memoryCachingEnabled,
            int cacheExpiryInSeconds,
            string json,
            long rows,
            int columns)
        {
            dataTableDefinition.UpdateName(name);
            dataTableDefinition.UpdateAlias(alias);
            dataTableDefinition.UpdateTableSchemaJson(json);
            dataTableDefinition.UpdateRecordAndColumnCount(rows, columns);
            dataTableDefinition.UpdateMemoryCachingStatus(memoryCachingEnabled, cacheExpiryInSeconds);
            await this.uBindDbContext.SaveChangesAsync();
        }
    }
}
