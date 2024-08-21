// <copyright file="AggregateSnapshotRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Persistence;

using Dapper;
using StackExchange.Profiling;
using System.Data.SqlClient;
using UBind.Domain;
using UBind.Domain.Aggregates;
using UBind.Domain.Models;
using UBind.Domain.Repositories;

/// <summary>
/// The repository for aggregate snapshots.
/// This repository is used to store and retrieve snapshots of aggregates.
/// It also retrieves the latest snapshot of an aggregate or it retrieves a snapshot by version.
/// </summary>
public class AggregateSnapshotRepository : IAggregateSnapshotRepository
{
    private readonly IUBindDbContext dbContext;
    private readonly IConnectionConfiguration connection;

    public AggregateSnapshotRepository(
        IUBindDbContext dbContext,
        IConnectionConfiguration connection)
    {
        this.dbContext = dbContext;
        this.connection = connection;
    }

    public AggregateSnapshot? GetAggregateSnapshot(
        Guid tenantId,
        Guid aggregateId,
        AggregateType aggregateType)
    {
        using (var connection = new SqlConnection(this.connection.UBind))
        {
            var parameters = new DynamicParameters();
            parameters.Add("@TenantId", tenantId);
            parameters.Add("@AggregateId", aggregateId);
            parameters.Add("@AggregateType", aggregateType);
            var query = this.GetAggregateSnapshotQuery();

            using (MiniProfiler.Current.Step(nameof(AggregateSnapshotRepository) + "." + nameof(this.GetAggregateSnapshot) + "/database snapshot repository query"))
            {
                return connection.QueryFirstOrDefault<AggregateSnapshot>(query, parameters, null, 180, System.Data.CommandType.Text);
            }
        }
    }

    public async Task<AggregateSnapshot?> GetAggregateSnapshotAsync(
        Guid tenantId,
        Guid aggregateId,
        AggregateType aggregateType)
    {
        using (var connection = new SqlConnection(this.connection.UBind))
        {
            var parameters = new DynamicParameters();
            parameters.Add("@TenantId", tenantId);
            parameters.Add("@AggregateId", aggregateId);
            parameters.Add("@AggregateType", aggregateType);
            var query = this.GetAggregateSnapshotQuery();

            using (MiniProfiler.Current.Step(nameof(AggregateSnapshotRepository) + "." + nameof(this.GetAggregateSnapshotAsync) + "/database snapshot repository query"))
            {
                return await connection.QueryFirstOrDefaultAsync<AggregateSnapshot>(query, parameters, null, 180, System.Data.CommandType.Text);
            }
        }
    }

    public void AddAggregateSnapshot(AggregateSnapshot aggregateSnapshot)
    {
        this.dbContext.AggregateSnapshots.Add(aggregateSnapshot);
    }

    public async Task<AggregateSnapshot?> GetAggregateSnapshotByVersion(
        Guid tenantId,
        Guid aggregateId,
        int version,
        AggregateType aggregateType)
    {
        using (var connection = new SqlConnection(this.connection.UBind))
        {
            var parameters = new DynamicParameters();
            parameters.Add("@TenantId", tenantId);
            parameters.Add("@AggregateId", aggregateId);
            parameters.Add("@AggregateType", aggregateType);
            parameters.Add("@Version", version);

            var query = $@"SELECT TOP 1   
                            Id, 
	                        TenantId,   
	                        AggregateId,    
	                        AggregateType,  
	                        Version,    
	                        Json,   
	                        CreatedTicksSinceEpoch      
                        FROM AggregateSnapshots 
                        WHERE TenantId = @TenantId  
                          AND AggregateId = @AggregateId    
                          AND AggregateType = @AggregateType  
                          AND Version <= @Version   
                        ORDER BY 
                          Version Desc,
                          CreatedTicksSinceEpoch DESC";

            return await connection.QueryFirstOrDefaultAsync<AggregateSnapshot>(query, parameters, null, 180, System.Data.CommandType.Text);
        }
    }

    public async Task DeleteOlderAggregateSnapshots(
        Guid tenantId,
        Guid idOfSnapshotToKeep,
        Guid aggregateId,
        AggregateType aggregateType)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@Id", idOfSnapshotToKeep);
        parameters.Add("@TenantId", tenantId);
        parameters.Add("@AggregateId", aggregateId);
        parameters.Add("@AggregateType", aggregateType);

        var sql = $"DELETE FROM AggregateSnapshots WHERE Id != @Id AND TenantId = @TenantId AND AggregateId = @AggregateId AND AggregateType = @AggregateType";
        using (var connection = new SqlConnection(this.connection.UBind))
        {
            await connection.ExecuteAsync(sql, parameters, null, 180, System.Data.CommandType.Text);
        }
    }

    public void SaveChanges()
    {
        this.dbContext.SaveChanges();
    }

    public async Task SaveChangesAsync()
    {
        await this.dbContext.SaveChangesAsync();
    }

    private string GetAggregateSnapshotQuery()
    {
        return @"SELECT TOP 1   
                Id, 
	            TenantId,   
	            AggregateId,    
	            AggregateType,  
	            Version,    
	            Json,   
	            CreatedTicksSinceEpoch      
            FROM AggregateSnapshots 
            WHERE TenantId = @TenantId  
                AND AggregateId = @AggregateId    
                AND AggregateType = @AggregateType    
            ORDER BY CreatedTicksSinceEpoch DESC";
    }
}
