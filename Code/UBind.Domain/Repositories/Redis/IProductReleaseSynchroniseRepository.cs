// <copyright file="IProductReleaseSynchroniseRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Repositories.Redis;

/// <summary>
/// This is a repository for syncing product releases.
/// It is used to keep track of which product have been sync is ongoing.
/// so that we can avoid the same product being synced with same times.
/// </summary>
public interface IProductReleaseSynchroniseRepository
{
    Task Upsert(Guid tenantId, Guid productId);

    Task<bool> Exists(Guid tenantId, Guid productId);

    Task Delete(Guid tenantId, Guid productId);
}
