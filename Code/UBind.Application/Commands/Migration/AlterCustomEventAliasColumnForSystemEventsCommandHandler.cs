// <copyright file="AlterCustomEventAliasColumnForSystemEventsCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Commands.Migration
{
    using System.Data;
    using System.Data.SqlClient;
    using System.Threading;
    using System.Threading.Tasks;
    using Dapper;
    using MediatR;
    using Microsoft.Extensions.Logging;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Persistence;

    /// <summary>
    /// Command handler for altering the CustomEventAlias column in SystemEvents table
    /// to use a maximum length of 255.
    /// </summary>
    public class AlterCustomEventAliasColumnForSystemEventsCommandHandler
        : ICommandHandler<AlterCustomEventAliasColumnForSystemEventsCommand, Unit>
    {
        private const int BatchesPerCopy = 10;
        private const int BatchSize = 100;

        private readonly IConnectionConfiguration connection;
        private readonly ILogger<AlterCustomEventAliasColumnForSystemEventsCommandHandler> logger;

        public AlterCustomEventAliasColumnForSystemEventsCommandHandler(
            IConnectionConfiguration connection,
            ILogger<AlterCustomEventAliasColumnForSystemEventsCommandHandler> logger)
        {
            this.connection = connection;
            this.logger = logger;
        }

        public async Task<Unit> Handle(
            AlterCustomEventAliasColumnForSystemEventsCommand request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (await this.IsJobRequiredToRun(cancellationToken))
            {
                await this.Execute(cancellationToken);
            }
            else
            {
                this.logger.LogInformation("Not executing this modification any more as CustomEventAlias column is already of max length 255.");
            }
            return Unit.Value;
        }

        private async Task Execute(CancellationToken cancellationToken)
        {
            this.logger.LogInformation("Starting to alter CustomEventAlias column");

            this.logger.LogInformation("Adding new column CustomEventAlias_new");
            await this.ExecuteQuery(this.AddColumnQuery(), cancellationToken);

            this.logger.LogInformation("Copying CustomEventAlias to CustomEventAlias_new");
            var noOfRowsToCopy = await this.CountAffected(cancellationToken);
            this.logger.LogInformation($"Total rows to copy: {noOfRowsToCopy}");
            await this.ExecuteCopyDataAndRetryOnSqlTimeout(cancellationToken);

            this.logger.LogInformation("Swapping the columns");
            await this.ExecuteQuery(this.SwapColumnQuery(), cancellationToken);

            this.logger.LogInformation("Dropping the old CustomEventAlias column");
            await this.ExecuteQuery(this.DropColumnQuery(), cancellationToken);
        }

        private async Task ExecuteCopyDataAndRetryOnSqlTimeout(CancellationToken cancellationToken)
        {
            int batchCount = 0;
            int timeoutCount = 0;
            int totalUpdated = 0;
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    var paramaters = new DynamicParameters();
                    paramaters.Add("@MaxBatches", BatchesPerCopy, DbType.Int32);
                    paramaters.Add("@BatchSize", BatchSize, DbType.Int32);
                    batchCount = await this.ExecuteQuery(this.CopyDataQuery(), cancellationToken, paramaters);

                    totalUpdated += batchCount;
                    this.logger.LogInformation($"Copied {totalUpdated} batches in batches of {BatchSize}.");
                    if (batchCount < BatchesPerCopy)
                    {
                        break;
                    }
                    timeoutCount = 0;
                }
                catch (SqlException ex) when (ex.Message.Contains("Timeout expired"))
                {
                    timeoutCount++;
                    this.logger.LogWarning($"Timeout occurred while copying data. Retrying {timeoutCount} for up to 3 times...");
                    if (timeoutCount > 3)
                    {
                        this.logger.LogWarning("Timeout occurred while copying data. Retried 3 times. Aborting...");
                        throw;
                    }
                    await Task.Delay(1000 * timeoutCount, cancellationToken);
                }

                await Task.Delay(5000, cancellationToken);
            }
        }

        private async Task<int> ExecuteQuery(string sql, CancellationToken cancellationToken, DynamicParameters? parameters = null)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (var connection = new SqlConnection(this.connection.UBind))
            {
                await connection.OpenAsync(cancellationToken);
                var commandDefinition = new CommandDefinition(
                    sql,
                    parameters: parameters,
                    commandTimeout: 180,
                    cancellationToken: cancellationToken);
                return await connection.ExecuteAsync(commandDefinition);
            }
        }

        private async Task<int> CountAffected(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var sql = this.CountAffectedQuery();
            return await this.QuerySingle(sql, cancellationToken);
        }

        private async Task<int> QuerySingle(string sql, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (var connection = new SqlConnection(this.connection.UBind))
            {
                await connection.OpenAsync(cancellationToken);
                return await connection.QuerySingleAsync<int>(sql, commandTimeout: 180);
            }
        }

        private async Task<bool> IsJobRequiredToRun(CancellationToken cancellationToken)
        {
            string sql = @"
                SELECT CHARACTER_MAXIMUM_LENGTH
                FROM INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_NAME = 'SystemEvents' AND COLUMN_NAME = 'CustomEventAlias';
            ";
            var maxCharacterLength = await this.QuerySingle(sql, cancellationToken);
            return maxCharacterLength != 255;
        }

        private string AddColumnQuery()
        {
            return @"
                IF NOT EXISTS (SELECT 1 FROM sys.columns 
                    WHERE Name = N'CustomEventAlias_New' AND Object_ID = Object_ID(N'SystemEvents'))
                BEGIN
                    ALTER TABLE SystemEvents
                    ADD CustomEventAlias_New NVARCHAR(255) NULL;
                END     
                ";
        }

        private string CountAffectedQuery()
        {
            return @"
                SELECT COUNT(*) from dbo.SystemEvents
                WHERE EventType = 0  
                ";
        }

        private string SwapColumnQuery()
        {
            return @"
                BEGIN TRANSACTION;

                BEGIN TRY
                    IF EXISTS (SELECT 1 FROM sys.columns 
                        WHERE Name = N'CustomEventAlias' AND Object_ID = Object_ID(N'SystemEvents'))
                    BEGIN
                        EXEC sp_rename 'SystemEvents.CustomEventAlias', 'CustomEventAlias_Old', 'COLUMN';
                    END
                    IF EXISTS (SELECT 1 FROM sys.columns 
                        WHERE Name = N'CustomEventAlias_New' AND Object_ID = Object_ID(N'SystemEvents'))
                    BEGIN
                        EXEC sp_rename 'SystemEvents.CustomEventAlias_New', 'CustomEventAlias', 'COLUMN';
                    END

                    COMMIT TRANSACTION;
                END TRY
                BEGIN CATCH
                    ROLLBACK TRANSACTION;
                    THROW;
                END CATCH;
                ";
        }

        private string CopyDataQuery()
        {
            return @"
                DECLARE @LastID UNIQUEIDENTIFIER = '00000000-0000-0000-0000-000000000000';
                DEClARE @BatchCounter INT = 0;
                DECLARE @RowCount INT;

                WHILE (1=1)
                BEGIN
                    UPDATE TOP (@BatchSize) SystemEvents WITH (ROWLOCK)
                    SET CustomEventAlias_New = CustomEventAlias
                    WHERE CustomEventAlias_New IS NULL AND EventType = 0;

                    SET @RowCount = @@ROWCOUNT;
                    IF @RowCount < @BatchSize BREAK;

                    SET @BatchCounter = @BatchCounter + 1;
                    IF @BatchCounter >= @MaxBatches BREAK;
                END

                SELECT @BatchCounter AS BatchCounter;
                ";
        }

        private string DropColumnQuery()
        {
            return @"
                IF EXISTS (SELECT 1 FROM sys.columns 
                           WHERE Name = N'CustomEventAlias_Old' AND Object_ID = Object_ID(N'SystemEvents'))
                BEGIN
                    ALTER TABLE SystemEvents
                    DROP COLUMN CustomEventAlias_Old;
                END
                ";
        }
    }
}
