// <copyright file="RedisRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Redis.Repositories
{
    using StackExchange.Redis;
    using UBind.Persistence.Configuration;

    public abstract class RedisRepository
    {
        protected readonly IConnectionMultiplexer connectionMultiplexer;
        protected readonly IRedisConfiguration redisConfiguration;

        protected RedisRepository(
            IRedisConfiguration redisConfiguration,
            IConnectionMultiplexer connectionMultiplexer)
        {
            this.redisConfiguration = redisConfiguration;
            this.connectionMultiplexer = connectionMultiplexer;
        }

        protected abstract string Prefix { get; }

        protected string GetKey(Guid tenantId, string key)
        {
            return this.redisConfiguration.Prefix + "{" + tenantId.ToString() + "}:" + this.Prefix + key;
        }
    }
}
