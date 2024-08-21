// <copyright file="IProductReleaseService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Services;

using NodaTime;

public interface IProductReleaseService
{
    Guid? GetDefaultProductReleaseId(Guid tenantId, Guid productId, DeploymentEnvironment environment);

    ReleaseBase? GetReleaseFromDatabaseWithoutAssets(Guid tenantId, Guid productReleaseId);

    /// <summary>
    /// Not having the environment here makes it slower, as it requires 2 lookups.
    /// </summary>
    ReleaseBase? GetReleaseFromDatabaseWithoutAssetFileContents(Guid tenantId, Guid productReleaseId);

    Instant? GetLastModifiedTimestamp(Guid tenantId, Guid productReleaseId);
}
