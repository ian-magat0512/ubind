// <copyright file="IDbaRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>
namespace UBind.Domain.Repositories;
using System.Data.Entity;
using UBind.Domain.ReadModels.Dba;

/// <summary>
/// Defines the repository for DBA operations.
/// </summary>
public interface IDbaRepository<T> where T : DbContext
{
    /// <summary>
    /// Gets all active connections of the current DB.
    /// </summary>
    /// <returns>Details of each connection related to the DB.</returns>
    List<SqlServerSysProcessViewModel> GetActiveConnections();

    /// <summary>
    /// Gets the maximum connection pool size.
    /// </summary>
    /// <returns>Maximum connection pool size.</returns>
    int GetMaxConnectionPool();

    /// <summary>
    /// Gets the name of the database for the current context.
    /// </summary>
    /// <returns>Name of the database.</returns>
    string GetDbName();
}