// <copyright file="GetDefaultProductReleaseContextOrNullQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.ProductRelease;

using System.Threading;
using System.Threading.Tasks;
using UBind.Application.Releases;
using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.Product;

public class GetDefaultProductReleaseContextOrNullQueryHandler
    : IQueryHandler<GetDefaultProductReleaseContextOrNullQuery, ReleaseContext?>
{
    private readonly IReleaseQueryService releaseQueryService;

    public GetDefaultProductReleaseContextOrNullQueryHandler(IReleaseQueryService releaseQueryService)
    {
        this.releaseQueryService = releaseQueryService;
    }

    public Task<ReleaseContext?> Handle(GetDefaultProductReleaseContextOrNullQuery query, CancellationToken cancellationToken)
    {
        var releaseContext = this.releaseQueryService.GetDefaultReleaseContextOrNull(
            query.TenantId,
            query.ProductId,
            query.Environment);
        return Task.FromResult(releaseContext);
    }
}
