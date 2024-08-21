// <copyright file="UpdateSystemEventsExpiryTimeStampCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Commands.Migration
{
    using System.Data;
    using System.Data.SqlClient;
    using System.Threading;
    using System.Threading.Tasks;
    using Dapper;
    using Humanizer;
    using MediatR;
    using Microsoft.Extensions.Logging;
    using NodaTime;
    using UBind.Domain.Events;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Services.Maintenance;
    using UBind.Persistence;

    /// <summary>
    /// This handler updates the system events expiry timestamp.
    /// This is ran in migration to make sure that system events
    /// follows the documented persistence.
    /// </summary>
    public class UpdateSystemEventsExpiryTimeStampCommandHandler
        : ICommandHandler<UpdateSystemEventsExpiryTimeStampCommand, Unit>
    {
        /// <summary>
        /// Aproximately 1 second
        /// </summary>
        private const int BatchSize = 1000;

        private const int SqlTimeoutCountBeforeRebuildingIndex = 10;

        /// <summary>
        /// Every 10 batches
        /// </summary>
        private const int ExpectedEventsUpdatedBeforeCheckingDBLog = BatchSize * 10;
        private readonly IConnectionConfiguration connection;
        private readonly IDbLogFileMaintenanceService dbLogFileMaintenanceService;
        private readonly ILogger<UpdateSystemEventsExpiryTimeStampCommandHandler> logger;
        private readonly IClock clock;

        public UpdateSystemEventsExpiryTimeStampCommandHandler(
            IConnectionConfiguration connection,
            IDbLogFileMaintenanceService dbLogFileMaintenanceService,
            IClock clock,
            ILogger<UpdateSystemEventsExpiryTimeStampCommandHandler> logger)
        {
            this.connection = connection;
            this.dbLogFileMaintenanceService = dbLogFileMaintenanceService;
            this.logger = logger;
            this.clock = clock;
        }

        public async Task<Unit> Handle(UpdateSystemEventsExpiryTimeStampCommand request, CancellationToken cancellationToken)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                await this.UpdateSystemEventsWithWithZeroOrMaxExpiryToNull(cancellationToken);
                await this.UpdateSystemEventsWithExpiryNull(cancellationToken);
                await this.RebuildIndexForExpiryTicksSinceEpochAndEventType(cancellationToken);
                await this.UpdateSystemEventsWithExpiryNotNull(cancellationToken);
                await this.RebuildIndexForExpiryTicksSinceEpochAndEventType(cancellationToken);
                this.logger.LogInformation("Operation completed.");
            }
            catch (OperationCanceledException)
            {
                this.logger.LogError("The operation was cancelled.");
                throw;
            }
            finally
            {
                await this.dbLogFileMaintenanceService.ShrinkLogFileOrCleanIfNeeded(cancellationToken);
            }

            return Unit.Value;
        }

        /// <summary>
        /// This is to update system events that has
        /// expiry timestamp of 0 and 2534023007999999999 to null.
        /// Old records have used 0 and 2534023007999999999 to indicate
        /// events persisted indefinitely.
        /// This is required so that system events that SHOULD HAVE expiry can be
        /// caught on UpdateSystemEventsWithExpiryNotNull as well.
        /// </summary>
        private async Task UpdateSystemEventsWithWithZeroOrMaxExpiryToNull(CancellationToken cancellationToken)
        {
            this.logger.LogInformation(
                "Starting to update system event records that have an expiry timestamp " +
                "of 0 or 2534023007999999999 to NULL.");
            var sql = "UPDATE TOP (@batchSize) dbo.SystemEvents" +
                " WITH (ROWLOCK, READPAST)" +
                " SET ExpiryTicksSinceEpoch = NULL" +
                " WHERE ExpiryTicksSinceEpoch in (0, 2534023007999999999);";
            var parameters = new DynamicParameters();
            var totalUpdatedCount = await this.UpdateSystemEventsInBatches(sql, parameters, cancellationToken);
            this.logger.LogInformation($"Finished updating {totalUpdatedCount} records.");
        }

        /// <summary>
        /// Update SystemEvents that should have NULL (indefinite) expiry timestamp
        /// according to the uBind reference but has an expiry.
        /// </summary>
        private async Task UpdateSystemEventsWithExpiryNull(CancellationToken cancellationToken)
        {
            this.logger.LogInformation(
                "Starting to update system event records that should be persisted indefinitely" +
                " (expiry timestamp set to NULL).");
            var listSystemEventTypes = SystemEventTypeExtensions.GetSystemEventTypeValuesToPersistIndefinitely();
            var sql = "UPDATE TOP (@batchSize) dbo.SystemEvents" +
                " WITH (ROWLOCK, READPAST)" +
                " SET ExpiryTicksSinceEpoch = NULL" +
                " WHERE EventType in @systemEventTypes" +
                " AND ExpiryTicksSinceEpoch IS NOT NULL;";
            var parameters = new DynamicParameters();
            parameters.Add("@systemEventTypes", listSystemEventTypes);
            var totalUpdatedCount = await this.UpdateSystemEventsInBatches(sql, parameters, cancellationToken);
            this.logger.LogInformation($"Finished updating {totalUpdatedCount} records.");
        }

        /// <summary>
        /// Update SystemEvents that dont have expiry where it should have,
        /// so we set the expiry to its created timestamp plus its documented persistence.
        /// </summary>
        private async Task UpdateSystemEventsWithExpiryNotNull(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var totalUpdatedCount = 0;
            this.logger.LogInformation("Starting to update system event records that" +
                " should have an expiry timestamp (instead of NULL).");
            var listSystemEventTypes = SystemEventTypeExtensions.GetSystemEventTypesWithExpiry();
            foreach (var systemEventType in listSystemEventTypes)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var hoursToPersist = systemEventType.GetPersistenceInHoursOrNull();
                if (hoursToPersist == null)
                {
                    continue;
                }

                var ticksToAddInCreated = Duration.FromHours(hoursToPersist.Value).ToTimeSpan().Ticks;
                totalUpdatedCount += await this.UpdateExpiryBySystemEventType((int)systemEventType, ticksToAddInCreated, cancellationToken);
            }

            this.logger.LogInformation($"Finished updating {totalUpdatedCount} records.");
        }

        private async Task<int> UpdateExpiryBySystemEventType(
            int systemEventType, long ticksToAddInCreated, CancellationToken cancellationToken)
        {
            this.logger.LogInformation($"Starting to update system event records with eventType {systemEventType}.");
            var sql = "UPDATE TOP (@batchSize) dbo.SystemEvents" +
                " WITH (ROWLOCK, READPAST)" +
                " SET ExpiryTicksSinceEpoch = CreatedTicksSinceEpoch + @ticksToAdd" +
                " WHERE" +
                " EventType = @systemEventType" +
                " AND ExpiryTicksSinceEpoch IS NULL;";
            var parameters = new DynamicParameters();
            parameters.Add("@systemEventType", systemEventType);
            parameters.Add("@ticksToAdd", ticksToAddInCreated);
            return await this.UpdateSystemEventsInBatches(sql, parameters, cancellationToken);
        }

        private async Task RebuildIndexForExpiryTicksSinceEpochAndEventType(CancellationToken cancellationToken)
        {
            async Task RebuildIndex(CancellationToken cancellationToken)
            {
                await this.dbLogFileMaintenanceService.ShrinkLogFileOrCleanIfNeeded(cancellationToken);
                this.logger.LogInformation("Rebuilding index for ExpiryTicksSinceEpoch and EventType.");
                const string indexName = "IX_SystemEvents_ExpiryTicksSinceEpoch_EventType";
                using (var connection = new SqlConnection(this.connection.UBind))
                {
                    await connection.OpenAsync(cancellationToken);
                    string sql = $@"
                        DECLARE @online VARCHAR(3) = CASE
	                        WHEN (CAST(SERVERPROPERTY ('edition') AS NVARCHAR(128)) LIKE 'Enterprise Edition%') THEN 'ON'
	                        ELSE 'OFF'
                        END;

                        IF EXISTS (
	                        SELECT *
	                        FROM sys.indexes
	                        WHERE name = '{indexName}'
		                        AND object_id = OBJECT_ID('dbo.SystemEvents')
                        )
                        BEGIN
	                        DECLARE @rebuildIndex NVARCHAR(1000) =
		                        'ALTER INDEX {indexName} ON dbo.SystemEvents REBUILD
		                        WITH (ONLINE = ' + @online + ')';
	                        EXEC(@rebuildIndex);
                        END";
                    cancellationToken.ThrowIfCancellationRequested();
                    var commandDefinition = new CommandDefinition(
                        sql,
                        commandTimeout: 180,
                        cancellationToken: cancellationToken);
                    await connection.ExecuteAsync(commandDefinition);
                }
                await Task.Delay(5000, cancellationToken);
            }

            await RetryPolicyHelper.ExecuteAsync<SqlException>((ct) => RebuildIndex(ct), maxJitter: 1500, cancellationToken: cancellationToken);
        }

        private async Task<int> UpdateSystemEventsInBatches(string sql, DynamicParameters parameters, CancellationToken cancellationToken)
        {
            var totalUpdatedCount = 0;
            using (var connection = new SqlConnection(this.connection.UBind))
            {
                await connection.OpenAsync(cancellationToken);
                var updatedCount = BatchSize;
                bool isIndicesAlreadyRebuilt = false;
                int timeoutCount = 0;
                var startTime = this.clock.GetCurrentInstant();
                var maximumSecondsItTakes = 0;
                while (updatedCount == BatchSize)
                {
                    try
                    {
                        startTime = this.clock.GetCurrentInstant();
                        cancellationToken.ThrowIfCancellationRequested();
                        using (var transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted))
                        {
                            parameters.Add("@batchSize", BatchSize);
                            var commandDefinition = new CommandDefinition(
                                sql,
                                parameters,
                                transaction,
                                commandTimeout: 180,
                                cancellationToken: cancellationToken);
                            updatedCount = await connection.ExecuteAsync(commandDefinition);
                            totalUpdatedCount += updatedCount;
                            transaction.Commit();
                        }
                        timeoutCount = 0;
                        isIndicesAlreadyRebuilt = false;

                        // log every 5 batches
                        if (totalUpdatedCount > 0 && totalUpdatedCount % (BatchSize * 5) == 0)
                        {
                            this.logger.LogInformation($"Updated {BatchSize * 5} rows, total update is now {totalUpdatedCount} rows." +
                                $" Maximum time an SQL command takes for updating a batch of {BatchSize} rows is {TimeSpan.FromSeconds(maximumSecondsItTakes).Humanize()}");
                        }

                        var duration = this.clock.GetCurrentInstant() - startTime;
                        maximumSecondsItTakes = Math.Max(maximumSecondsItTakes, (int)duration.TotalSeconds);
                    }
                    catch (SqlException ex) when (ex.Message.Contains("Timeout expired"))
                    {
                        if (isIndicesAlreadyRebuilt)
                        {
                            // if indices are already rebuilt and still we are getting timeout,
                            // then we simply throw so it can be investigated
                            throw;
                        }

                        timeoutCount++;
                        if (timeoutCount > SqlTimeoutCountBeforeRebuildingIndex)
                        {
                            this.logger.LogInformation($"Rebuilding index for ExpiryTicksSinceEpoch and EventType due to timeout count of {timeoutCount}");
                            timeoutCount = 0;
                            isIndicesAlreadyRebuilt = true;
                            await this.RebuildIndexForExpiryTicksSinceEpochAndEventType(cancellationToken);
                        }

                        // we let it run the query again on timeout
                        updatedCount = BatchSize;
                        this.logger.LogInformation($"SQL timeout. Retry count: {timeoutCount}. Retrying after {timeoutCount} seconds.");

                        // add increasing delay on retry
                        await Task.Delay(1000 * timeoutCount, cancellationToken);
                    }

                    await Task.Delay(4000, cancellationToken);

                    if (totalUpdatedCount > 0 && totalUpdatedCount % ExpectedEventsUpdatedBeforeCheckingDBLog == 0)
                    {
                        await this.dbLogFileMaintenanceService.ShrinkLogFileOrCleanIfNeeded(cancellationToken);
                    }
                }
            }

            return totalUpdatedCount;
        }
    }
}