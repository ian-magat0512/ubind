// <copyright file="GetReleaseContextForReleaseOrDefaultReleaseQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.ProductRelease;

using UBind.Application.Releases;
using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.Product;

public class GetReleaseContextForReleaseOrDefaultReleaseQueryHandler
    : IQueryHandler<GetReleaseContextForReleaseOrDefaultReleaseQuery, ReleaseContext>
{
    private readonly IReleaseQueryService releaseQueryService;

    public GetReleaseContextForReleaseOrDefaultReleaseQueryHandler(
        IReleaseQueryService releaseQueryService)
    {
        this.releaseQueryService = releaseQueryService;
    }

    public Task<ReleaseContext> Handle(
        GetReleaseContextForReleaseOrDefaultReleaseQuery query,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var releaseContext = this.releaseQueryService.GetReleaseContextForReleaseOrDefaultRelease(
            query.TenantId,
            query.ProductId,
            query.Environment,
            query.ProductReleaseId);
        return Task.FromResult(releaseContext);
    }
}
