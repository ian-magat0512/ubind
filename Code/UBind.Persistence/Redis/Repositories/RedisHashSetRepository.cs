// <copyright file="RedisHashSetRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Persistence.Redis.Repositories;

using StackExchange.Redis;
using System;
using System.Text.Json;
using UBind.Domain.Repositories.Redis;
using UBind.Persistence.Configuration;

/// <summary>
/// Repository for hashset redis values.
/// </summary>
public class RedisHashSetRepository<T> : RedisRepository, IRedisHashSetRepository<T>
{
    private readonly IDatabase redisDatabase;
    private readonly string setKey;
    private readonly TimeSpan? expiryTime;

    public RedisHashSetRepository(
        IRedisConfiguration redisConfiguration,
        IConnectionMultiplexer connectionMultiplexer,
        Guid tenantId,
        string setKey,
        TimeSpan? expiryTime = null)
        : base(redisConfiguration, connectionMultiplexer)
    {
        this.redisDatabase = this.connectionMultiplexer.GetDatabase();
        this.setKey = this.GetKey(tenantId, setKey);
        this.expiryTime = expiryTime;
    }

    protected override string Prefix => "product:";

    /// <inheritdoc/>
    public async Task Add(T value)
    {
        await this.redisDatabase.SetAddAsync(this.setKey, this.Serialize(value));
        if (this.expiryTime.HasValue)
        {
            await this.redisDatabase.KeyExpireAsync(this.setKey, this.expiryTime);
        }
    }

    /// <inheritdoc/>
    public async Task<bool> Contains(T value)
    {
        return await this.redisDatabase.SetContainsAsync(this.setKey, this.Serialize(value));
    }

    /// <inheritdoc/>
    public async Task DeleteSet()
    {
        await this.redisDatabase.KeyDeleteAsync(this.setKey);
    }

    /// <inheritdoc/>
    public async Task<long> GetCount()
    {
        return await this.redisDatabase.SetLengthAsync(this.setKey);
    }

    private RedisValue Serialize(T value)
    {
        return value is string str
            ? str
            : JsonSerializer.Serialize(value);
    }
}