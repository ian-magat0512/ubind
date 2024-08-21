// <copyright file="TinyUrlRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Repositories;

using System.Data.SqlClient;
using Dapper;
using UBind.Domain.Models;
using UBind.Domain.Repositories;

/// <summary>
/// The repository for storing URL tokens and destination URL
/// </summary>
public class TinyUrlRepository : ITinyUrlRepository
{
    private readonly IUBindDbContext dbContext;
    private readonly IConnectionConfiguration connection;

    public TinyUrlRepository(IUBindDbContext dbContext, IConnectionConfiguration connection)
    {
        this.dbContext = dbContext;
        this.connection = connection;
    }

    /// <inheritdoc/>
    public async Task<TinyUrl?> GetByToken(string token)
    {
        using (var connection = new SqlConnection(this.connection.UBind))
        {
            const string query = "SELECT * FROM dbo.TinyUrls WHERE Token = @Token";
            return await connection.QueryFirstOrDefaultAsync<TinyUrl>(query, new { Token = token });
        }
    }

    public void Insert(TinyUrl tinyUrl)
    {
        this.dbContext.TinyUrls.Add(tinyUrl);
    }

    public void SaveChanges()
    {
        this.dbContext.SaveChanges();
    }

    public async Task<long?> GetMaxSequenceNumber()
    {
        using (var connection = new SqlConnection(this.connection.UBind))
        {
            const string query = "SELECT TOP 1 SequenceNumber FROM dbo.TinyUrls ORDER BY SequenceNumber DESC";
            return await connection.QueryFirstOrDefaultAsync<long>(query);
        }
    }
}
