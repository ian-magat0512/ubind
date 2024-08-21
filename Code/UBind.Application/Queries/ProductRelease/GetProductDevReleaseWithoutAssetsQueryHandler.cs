// <copyright file="GetProductDevReleaseWithoutAssetsQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.ProductRelease;

using System.Threading;
using System.Threading.Tasks;
using UBind.Domain;
using UBind.Domain.Exceptions;
using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.Repositories;

public class GetProductDevReleaseWithoutAssetsQueryHandler : IQueryHandler<GetProductDevReleaseWithoutAssetsQuery, DevRelease>
{
    private readonly IDevReleaseRepository devReleaseRepository;
    private readonly ICachingResolver cachingResolver;

    public GetProductDevReleaseWithoutAssetsQueryHandler(
        IDevReleaseRepository devReleaseRepository,
        ICachingResolver cachingResolver)
    {
        this.devReleaseRepository = devReleaseRepository;
        this.cachingResolver = cachingResolver;
    }

    public Task<DevRelease> Handle(GetProductDevReleaseWithoutAssetsQuery query, CancellationToken cancellationToken)
    {
        var devRelease = this.devReleaseRepository.GetDevReleaseForProductWithoutAssets(query.TenantId, query.ProductId);
        if (devRelease == null)
        {
            var tenantAlias = this.cachingResolver.GetTenantAliasOrThrow(query.TenantId);
            var productAlias = this.cachingResolver.GetProductAliasOrThrow(query.TenantId, query.ProductId);
            throw new ErrorException(Errors.Release.AssetsNotSynchronised(tenantAlias, productAlias));
        }

        return Task.FromResult(devRelease);
    }
}
