// <copyright file="RebuildSystemEventsAndRelationshipsIndexCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Commands.SystemEvents;

using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Humanizer;
using MediatR;
using Microsoft.Extensions.Logging;
using NodaTime;
using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.Services.Maintenance;
using UBind.Persistence;

/// <summary>
/// Handler for rebuilding fragmented SystemEvents and Relationships Indices.
/// Overtime, the system events and relationships indices may become fragmented
/// due to the deletion of expired system events and associated relationship.
/// </summary>
public class RebuildSystemEventsAndRelationshipsIndexCommandHandler : ICommandHandler<RebuildSystemEventsAndRelationshipsIndexCommand, Unit>
{
    private const string SystemEventsTable = "dbo.SystemEvents";
    private const string RelationshipsTable = "dbo.Relationships";
    private const double PercentageFragmentationThreshold = 30.0;

    private readonly ILogger<RebuildSystemEventsAndRelationshipsIndexCommandHandler> logger;
    private readonly IDbLogFileMaintenanceService dbLogFileMaintenanceService;
    private readonly IConnectionConfiguration connectionConfiguration;
    private readonly IClock clock;

    public RebuildSystemEventsAndRelationshipsIndexCommandHandler(
            ILogger<RebuildSystemEventsAndRelationshipsIndexCommandHandler> logger,
            IDbLogFileMaintenanceService dbLogFileMaintenanceService,
            IConnectionConfiguration connectionConfiguration,
            IClock clock)
    {
        this.logger = logger;
        this.dbLogFileMaintenanceService = dbLogFileMaintenanceService;
        this.connectionConfiguration = connectionConfiguration;
        this.clock = clock;
    }

    public async Task<Unit> Handle(RebuildSystemEventsAndRelationshipsIndexCommand request, CancellationToken cancellationToken)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            this.logger.LogInformation($"Starting to execute index rebuilding for tables {SystemEventsTable} and {RelationshipsTable}.");
            await this.RebuildIndexIfNecessary(SystemEventsTable, this.GetListOfIndicesForSystemEventsTable(), cancellationToken);
            await this.RebuildIndexIfNecessary(RelationshipsTable, this.GetListOfIndicesForRelationshipsTable(), cancellationToken);
            this.logger.LogInformation($"Operation completed.");
        }
        catch (OperationCanceledException)
        {
            this.logger.LogInformation("Operation was cancelled.");
            throw;
        }

        return Unit.Value;
    }

    private async Task RebuildIndexIfNecessary(string tableName, List<string> indices, CancellationToken cancellationToken)
    {
        var indexList = await this.GetIndicesThatRequiresRebuilding(tableName, indices, cancellationToken);
        if (!indexList.Any())
        {
            this.logger.LogInformation($"No indices require rebuilding for {tableName} Table");
            return;
        }
        foreach (var index in indexList)
        {
            await this.RebuildIndex(tableName, index, cancellationToken);
        }
    }

    private async Task<IEnumerable<string>> GetIndicesThatRequiresRebuilding(string tableName, List<string> indices, CancellationToken cancellationToken)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@TableName", tableName);
        parameters.Add("@IndexName", indices);
        string query = @"
            SELECT
                i.name AS IndexName,
                avg_fragmentation_in_percent AS AverageFragmentation
            FROM sys.dm_db_index_physical_stats(DB_ID(), OBJECT_ID(@TableName), NULL, NULL, 'LIMITED') AS ps
            INNER JOIN sys.indexes AS i ON ps.object_id = i.object_id AND ps.index_id = i.index_id
            WHERE (i.name in @IndexName)
                  AND i.type_desc <> 'HEAP'            -- Exclude heap as it doesn't benefit from reorganization or rebuild
            ORDER BY avg_fragmentation_in_percent DESC;
        ";

        using (var connection = new SqlConnection(this.connectionConfiguration.UBind))
        {
            await connection.OpenAsync(cancellationToken);
            var commandDefinition = new CommandDefinition(
                query,
                parameters,
                commandTimeout: 180,
                cancellationToken: cancellationToken);
            var results = await connection.QueryAsync<TableIndexInfo>(commandDefinition);
            if (results == null)
            {
                return Enumerable.Empty<string>();
            }
            foreach (var result in results)
            {
                this.logger.LogInformation($"Index {result.IndexName} has {Math.Round(result.AverageFragmentation, 2)}% fragmentation.");
            }
            return results.Where(x => x.AverageFragmentation > PercentageFragmentationThreshold).Select(x => x.IndexName);
        }
    }

    private async Task RebuildIndex(string tableName, string indexName, CancellationToken cancellationToken)
    {
        // SQL Server does not support the use of parameters directly in dynamic SQL for certain elements like table names and index names.
        // You need to integrate these names into the dynamic SQL string via concatenation before executing.
        // However, this needs to be done cautiously to avoid SQL injection risks, therefore we apply sanitation of indexName and tableName below.
        indexName = indexName.Replace("'", "''");
        tableName = tableName.Replace("'", "''");

        await this.dbLogFileMaintenanceService.ShrinkLogFileOrCleanIfNeeded(cancellationToken);

        var startInstant = this.clock.GetCurrentInstant();
        this.logger.LogInformation($"Rebuilding {indexName} on {tableName} Table");
        using (var connection = new SqlConnection(this.connectionConfiguration.UBind))
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
		                AND object_id = OBJECT_ID('{tableName}')
                )
                BEGIN
                    DECLARE @rebuildIndex NVARCHAR(1000) = 
		                'ALTER INDEX {indexName} ON {tableName} REBUILD
		                WITH (ONLINE = ' + @online + ')';
	                EXEC(@rebuildIndex);
                END";
            var commandDefinition = new CommandDefinition(
                sql,
                commandTimeout: 3600, // 1 hour, to allow it enough time to finish
                cancellationToken: cancellationToken);
            await connection.ExecuteAsync(commandDefinition);
        }
        var durationSecondsSinceStarted = Math.Round((this.clock.GetCurrentInstant() - startInstant).TotalSeconds, 2);
        this.logger.LogInformation($"It took {TimeSpan.FromSeconds(durationSecondsSinceStarted).Humanize()}");
        await Task.Delay(5000, cancellationToken);
    }

    private List<string> GetListOfIndicesForRelationshipsTable()
    {
        List<string> indices = new List<string>
        {
            "AK_RelationshipFromTypeIndex",
            "AK_RelationshipFromEntityIndex",
            "AK_RelationshipToTypeIndex",
            "AK_RelationshipToEntityIndex",
            "AK_RelationshipTenantFromEntityIndex",
            "AK_RelationshipTenantToEntityIndex",
            "AK_RelationshipTenantFromTypeIndex",
            "AK_RelationshipTenantToTypeIndex",
        };
        return indices;
    }

    private List<string> GetListOfIndicesForSystemEventsTable()
    {
        List<string> indices = new List<string>
        {
            "IX_SystemEvents_ExpiryTicksSinceEpoch_EventType",
        };
        return indices;
    }

    private class TableIndexInfo
    {
        public TableIndexInfo(string indexName, double averageFragmentation)
        {
            this.IndexName = indexName;
            this.AverageFragmentation = averageFragmentation;
        }

        public string IndexName { get; set; }

        public double AverageFragmentation { get; set; }
    }
}
