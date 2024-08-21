// <copyright file="BaseDbaRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Persistence;

using Dapper;
using Microsoft.Extensions.Logging;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using UBind.Domain.ReadModels.Dba;

public abstract class BaseDbaRepository
{
    protected readonly DbContext dbContext;
    protected readonly IServiceProvider serviceProvider;
    protected readonly ILogger<BaseDbaRepository> logger;

    private static readonly Regex ConnectionStringPattern = new Regex("Max Pool Size=(\\d+)", RegexOptions.Compiled);

    protected BaseDbaRepository(DbContext dbContext, IServiceProvider serviceProvider, ILogger<BaseDbaRepository> logger)
    {
        this.dbContext = dbContext;
        this.serviceProvider = serviceProvider;
        this.logger = logger;
    }

    /// <inheritdoc/>
    public List<SqlServerSysProcessViewModel> GetActiveConnections()
    {
        try
        {
            List<SqlServerSysProcessViewModel> connections = new List<SqlServerSysProcessViewModel>();
            string query = $@"SELECT
                       dbid,
                       DB_NAME(dbid) as DBName,
                       COUNT(dbid) as NumberOfConnections,
                       loginame as LoginName,
                       status
                   FROM
                       sys.sysprocesses
                   WHERE
                       dbid > 0
                   GROUP BY
                       dbid, loginame, status
                   HAVING
                       DB_NAME(dbid) = @dbName";

            using (var connection = new SqlConnection(this.dbContext.Database.Connection.ConnectionString))
            {
                var parameters = new DynamicParameters();
                parameters.Add("dbName", this.dbContext.Database.Connection.Database);
                var sysViewResult = connection.Query<SqlServerSysProcessViewModel>(query, parameters);
                connections = sysViewResult.ToList();
            }

            return connections;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, $"An error occurred while retrieving active connections.");
            throw;
        }
    }

    /// <inheritdoc/>
    public int GetMaxConnectionPool()
    {
        Match match = BaseDbaRepository.ConnectionStringPattern.Match(this.dbContext.Database.Connection.ConnectionString);
        if (!match.Success)
        {
            return 0;
        }

        int maxPoolSize = 0;
        int.TryParse(match.Groups[1].Value, out maxPoolSize);
        return maxPoolSize;
    }

    /// <inheritdoc/>
    public string GetDbName()
    {
        return this.dbContext.Database.Connection.Database;
    }
}