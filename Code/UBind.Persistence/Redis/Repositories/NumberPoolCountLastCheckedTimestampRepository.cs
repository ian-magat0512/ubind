// <copyright file="NumberPoolCountLastCheckedTimestampRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Persistence.Redis.Repositories;

using NodaTime;
using StackExchange.Redis;
using UBind.Domain.Configuration;
using UBind.Domain.Repositories.Redis;
using UBind.Persistence.Configuration;

/// <summary>
/// Repository class for managing system alerts in the Redis cache.
/// It provides methods to retrieve the last checked timestamp from the Redis cache
/// and to update the last checked timestamp in the Redis cache.
/// </summary>
public class NumberPoolCountLastCheckedTimestampRepository : RedisRepository, INumberPoolCountLastCheckedTimestampRepository
{
    private readonly TimeSpan maxDuration = TimeSpan.FromMinutes(30);

    public NumberPoolCountLastCheckedTimestampRepository(
        IRedisConfiguration redisConfiguration,
        IConnectionMultiplexer connectionMultiplexer)
        : base(redisConfiguration, connectionMultiplexer)
    {
    }

    protected override string Prefix => "numberPoolCountLastChecked:";

    public async Task UpsertLastCheckedTimestamp(Guid tenantId, string productAlias, string numberPoolName, Instant timestamp)
    {
        var ticksSinceEpoch = timestamp.ToUnixTimeTicks();
        var db = this.connectionMultiplexer.GetDatabase();
        string key = this.GetKey(tenantId, $"{productAlias}:{numberPoolName}");

        await db.StringSetAsync(key, ticksSinceEpoch, this.maxDuration);
    }

    public Instant? GetLastCheckedTimestamp(Guid tenantId, string productAlias, string numberPoolName)
    {
        var db = this.connectionMultiplexer.GetDatabase();
        string key = this.GetKey(tenantId, $"{productAlias}:{numberPoolName}");
        RedisValue ticksSinceEpoch = db.StringGet(key);

        return ticksSinceEpoch.IsNullOrEmpty ? null : Instant.FromUnixTimeTicks((long)ticksSinceEpoch);
    }
}
