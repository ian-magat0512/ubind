// <copyright file="INumberPoolCountLastCheckedTimestampRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>
namespace UBind.Domain.Repositories.Redis
/// <summary>
/// Provides methods for getting and setting values in Redis.
/// </summary>
{
    using NodaTime;

    public interface INumberPoolCountLastCheckedTimestampRepository
    {
        Task UpsertLastCheckedTimestamp(Guid tenantId, string productAlias, string numberPoolName, Instant timestamp);

        Instant? GetLastCheckedTimestamp(Guid tenantId, string productAlias, string numberPoolName);
    }
}
