// <copyright file="ProductReleaseSynchroniseRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Redis.Repositories;

using StackExchange.Redis;
using UBind.Domain.Repositories.Redis;
using UBind.Persistence.Configuration;

/// <summary>
/// This is a repository for synchronise product releases.
/// It is used to keep track of which product have been synchronise is in progress.
/// so that we can avoid the same product being synced with same times.
/// </summary>
public class ProductReleaseSynchroniseRepository : RedisRepository, IProductReleaseSynchroniseRepository
{
    private readonly TimeSpan maxDuration = TimeSpan.FromMinutes(2);

    public ProductReleaseSynchroniseRepository(
        IRedisConfiguration redisConfiguration,
        IConnectionMultiplexer connectionMultiplexer)
        : base(redisConfiguration, connectionMultiplexer)
    {
    }

    protected override string Prefix => "productReleaseSync:";

    public async Task Upsert(Guid tenantId, Guid productId)
    {
        var db = this.connectionMultiplexer.GetDatabase();
        string productKey = productId.ToString();
        string key = this.GetKey(tenantId, productKey);

        await db.StringSetAsync(key, productKey, this.maxDuration);
    }

    public async Task<bool> Exists(Guid tenantId, Guid productId)
    {
        var db = this.connectionMultiplexer.GetDatabase();
        string productKey = productId.ToString();
        string key = this.GetKey(tenantId, productKey);

        return await db.KeyExistsAsync(key);
    }

    public async Task Delete(Guid tenantId, Guid productId)
    {
        var db = this.connectionMultiplexer.GetDatabase();
        string productKey = productId.ToString();
        string key = this.GetKey(tenantId, productKey);

        await db.KeyDeleteAsync(key);
    }
}
