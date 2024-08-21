// <copyright file="GetProductReleaseWithoutAssetsQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.ProductRelease;

using System.Threading;
using System.Threading.Tasks;
using UBind.Domain;
using UBind.Domain.Helpers;
using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.Services;

public class GetProductReleaseWithoutAssetsQueryHandler : IQueryHandler<GetProductReleaseWithoutAssetsQuery, ReleaseBase>
{
    private readonly IProductReleaseService productReleaseService;

    public GetProductReleaseWithoutAssetsQueryHandler(IProductReleaseService productReleaseService)
    {
        this.productReleaseService = productReleaseService;
    }

    public Task<ReleaseBase?> Handle(GetProductReleaseWithoutAssetsQuery query, CancellationToken cancellationToken)
    {
        var release = this.productReleaseService.GetReleaseFromDatabaseWithoutAssets(
            query.ReleaseContext.TenantId,
            query.ReleaseContext.ProductReleaseId);
        EntityHelper.ThrowIfNotFound(release, query.ReleaseContext.ProductReleaseId, "release");
        return Task.FromResult(release);
    }
}
