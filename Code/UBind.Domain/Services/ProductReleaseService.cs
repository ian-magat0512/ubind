// <copyright file="ProductReleaseService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Services;

using NodaTime;
using UBind.Domain.Repositories;

public class ProductReleaseService : IProductReleaseService
{
    private readonly IDevReleaseRepository devReleaseRepository;
    private readonly IDeploymentRepository deploymentRepository;
    private readonly IReleaseRepository releaseRepository;

    public ProductReleaseService(
        IDevReleaseRepository devReleaseRepository,
        IDeploymentRepository deploymentRepository,
        IReleaseRepository releaseRepository)
    {
        this.devReleaseRepository = devReleaseRepository;
        this.deploymentRepository = deploymentRepository;
        this.releaseRepository = releaseRepository;
    }

    public Guid? GetDefaultProductReleaseId(Guid tenantId, Guid productId, DeploymentEnvironment environment)
    {
        Guid? defaultReleaseId = null;
        if (environment == DeploymentEnvironment.Development)
        {
            defaultReleaseId = this.devReleaseRepository.GetIdOfDevReleaseForProduct(tenantId, productId);
        }
        else
        {
            defaultReleaseId = this.deploymentRepository.GetDefaultReleaseId(
                tenantId,
                productId,
                environment);
        }

        return defaultReleaseId;
    }

    public ReleaseBase? GetReleaseFromDatabaseWithoutAssets(Guid tenantId, Guid productReleaseId)
    {
        ReleaseBase? release = this.releaseRepository.GetReleaseWithoutAssets(tenantId, productReleaseId);
        if (release == null)
        {
            release = this.devReleaseRepository.GetDevReleaseWithoutAssets(
                tenantId,
                productReleaseId);
        }

        return release;
    }

    /// <summary>
    /// Not having the environment here makes it slower, as it requires 2 lookups.
    /// </summary>
    public ReleaseBase? GetReleaseFromDatabaseWithoutAssetFileContents(Guid tenantId, Guid productReleaseId)
    {
        ReleaseBase? release = this.releaseRepository.GetReleaseWithoutAssetFileContents(tenantId, productReleaseId);
        if (release == null)
        {
            release = this.devReleaseRepository.GetDevReleaseWithoutAssetFileContents(
                tenantId,
                productReleaseId);
        }

        return release;
    }

    public Instant? GetLastModifiedTimestamp(Guid tenantId, Guid productReleaseId)
    {
        var lastModifiedTimestamp = this.devReleaseRepository.GetLastModifiedTimestamp(tenantId, productReleaseId);
        return lastModifiedTimestamp;
    }
}
