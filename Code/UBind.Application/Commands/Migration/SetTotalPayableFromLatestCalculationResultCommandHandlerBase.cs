// <copyright file="SetTotalPayableFromLatestCalculationResultCommandHandlerBase.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Migration
{
    using System.Data.SqlClient;
    using System.Threading;
    using System.Threading.Tasks;
    using Dapper;
    using Microsoft.Extensions.Logging;
    using UBind.Domain.Enums;
    using UBind.Domain.Helpers;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services.Maintenance;
    using UBind.Persistence;

    /// <summary>
    ///  Handler for setting the TotalPayable of existing quotes from LatestCalculationResult.
    ///  This is a migration command called on startup.
    /// </summary>
    public abstract class SetTotalPayableFromLatestCalculationResultCommandHandlerBase<T>
        where T : class
    {
        protected IConnectionConfiguration Connection { get; set; }

        protected IUBindDbContext DbContext { get; set; }

        protected ITenantRepository TenantRepository { get; set; }

        protected IDbLogFileMaintenanceService DbLogFileMaintenanceService { get; set; }

        protected string TemporaryTableName { get; set; }

        protected ILogger<SetTotalPayableFromLatestCalculationResultCommandHandlerBase<T>> Logger { get; set; }

        protected abstract Task<List<Guid>> SetTotalPayableAndReturnExclusions(List<T> records, CancellationToken cancellationToken);

        protected abstract Task<List<T>> GetRecordsWithTotalPayableNotSet(Guid tenantId, int batch, CancellationToken cancellationToken);

        protected async Task Execute(string entity, CancellationToken cancellationToken)
        {
            await this.CreateTemporaryTable(cancellationToken);
            try
            {
                var tenants = this.TenantRepository.GetTenants();
                foreach (var tenant in tenants)
                {
                    this.Logger.LogInformation($"Set TotalPayable for {entity} of tenant {tenant.Id}");
                    await this.SetTotalPayableByTenant(entity, tenant, cancellationToken);
                    cancellationToken.ThrowIfCancellationRequested();
                }
            }
            catch (OperationCanceledException)
            {
                // catch here so it will not retry
                this.Logger.LogInformation("Cancellation requested, exiting.");
                throw;
            }
            finally
            {
                await this.DropTemporaryTable(cancellationToken);
            }

            this.Logger.LogInformation("Completed.");
        }

        protected async Task CreateTemporaryTable(CancellationToken cancellationToken)
        {
            using (var connection = new SqlConnection(this.Connection.UBind))
            {
                connection.Open();
                string createTableQuery = @$"
                    IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'{this.TemporaryTableName}') AND type = 'U')
                    BEGIN
                        CREATE TABLE {this.TemporaryTableName} (Id UNIQUEIDENTIFIER PRIMARY KEY);
                    END";
                var command = new CommandDefinition(createTableQuery, cancellationToken: cancellationToken);
                await connection.ExecuteAsync(command);
            }
        }

        protected async Task InsertRecords(List<Guid> quotes, CancellationToken cancellationToken)
        {
            using (var connection = new SqlConnection(this.Connection.UBind))
            {
                connection.Open();
                string insertRecordQuery = $"INSERT INTO {this.TemporaryTableName} (Id) VALUES (@RecordId)";
                var parameters = quotes.Select(pId => new { RecordId = pId });
                var command = new CommandDefinition(insertRecordQuery, parameters: parameters, cancellationToken: cancellationToken);
                await connection.ExecuteAsync(command);
            }
        }

        protected async Task DropTemporaryTable(CancellationToken cancellationToken)
        {
            using (var connection = new SqlConnection(this.Connection.UBind))
            {
                connection.Open();
                string dropTableQuery = @$"
                    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'{this.TemporaryTableName}') AND type = 'U')
                    BEGIN
                        DROP TABLE {this.TemporaryTableName}
                    END";
                var command = new CommandDefinition(dropTableQuery, cancellationToken: cancellationToken);
                await connection.ExecuteAsync(command);
            }
        }

        protected async Task SaveChangesForUnsavedRecords(CancellationToken cancellationToken)
        {
            async Task Save(CancellationToken cancellationToken)
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    await this.DbContext.SaveChangesAsync();
                }
            }

            await RetryPolicyHelper.ExecuteAsync<Exception>((cancellationToken) => Save(cancellationToken), maxJitter: 1500, cancellationToken: cancellationToken);
        }

        protected async Task SetTotalPayableByTenant(string entity, Domain.Tenant tenant, CancellationToken cancellationToken)
        {
            var pageSize = (int)PageSize.Default;
            var numberOfRecordsProcessed = 0;
            while (true)
            {
                var records = await this.GetRecordsWithTotalPayableNotSet(tenant.Id, pageSize, cancellationToken);
                var recordIdsToExclude = await this.SetTotalPayableAndReturnExclusions(records, cancellationToken);
                if (recordIdsToExclude.Count > 0)
                {
                    await this.InsertRecords(recordIdsToExclude, cancellationToken);
                }

                numberOfRecordsProcessed += records.Count;
                this.Logger.LogInformation($"Processed {entity}: {records.Count}");
                if (records.Count < pageSize)
                {
                    break;
                }

                cancellationToken.ThrowIfCancellationRequested();
            }
        }
    }
}