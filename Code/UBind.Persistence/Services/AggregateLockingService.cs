// <copyright file="AggregateLockingService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Persistence.Services;

using RedLockNet;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using StackExchange.Redis;
using UBind.Domain;
using UBind.Domain.Exceptions;
using UBind.Domain.Services;
using UBind.Persistence.Configuration;
using UBind.Persistence.Redis.Repositories;

public class AggregateLockingService : RedisRepository, IAggregateLockingService, IDisposable
{
    private readonly RedLockFactory redLockFactory;

    private TimeSpan keyExpiry = TimeSpan.FromSeconds(30);

    public AggregateLockingService(
        IConnectionMultiplexer connectionMultiplexer,
        IRedisConfiguration redisConfiguration)
        : base(redisConfiguration, connectionMultiplexer)
    {
        this.redLockFactory = RedLockFactory.Create(new List<RedLockMultiplexer>
        {
            new RedLockMultiplexer(connectionMultiplexer),
        });
    }

    protected override string Prefix => "aggregate";

    public async Task<IRedLock> CreateLockOrThrow(Guid tenantId, Guid aggregateId, AggregateType aggregateType)
    {
        var redlock = await this.CreateLock(tenantId, $"{aggregateId}", aggregateType.ToString());
        this.ThrowIfLockIsNotAcquired(redlock, aggregateId, aggregateType);
        return redlock;
    }

    public void Dispose()
    {
        if (this.redLockFactory != null)
        {
            this.redLockFactory.Dispose();
        }
    }

    private async Task<IRedLock> CreateLock(Guid tenantId, string aggregateId, string resourceType)
    {
        var resource = this.GetLockKey(tenantId, aggregateId, resourceType);
        return await this.redLockFactory.CreateLockAsync(resource, this.keyExpiry);
    }

    private string GetLockKey(Guid tenantId, string aggregateId, string resourceType)
    {
        return this.GetKey(tenantId, $"{resourceType}:{aggregateId}");
    }

    private void ThrowIfLockIsNotAcquired(IRedLock redlock, Guid aggregateId, AggregateType aggregateType)
    {
        if (!redlock.IsAcquired)
        {
            throw new ConcurrencyException($"Unable to acquire lock for aggregate '{aggregateType}' with aggregate Id '{aggregateId}'.");
        }
    }
}