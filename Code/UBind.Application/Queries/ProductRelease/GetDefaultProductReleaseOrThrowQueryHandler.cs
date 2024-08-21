// <copyright file="GetDefaultProductReleaseOrThrowQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.ProductRelease;

using System.Threading;
using System.Threading.Tasks;
using UBind.Application.Releases;

using UBind.Domain.Patterns.Cqrs;

public class GetDefaultProductReleaseIdOrThrowQueryHandler : IQueryHandler<GetDefaultProductReleaseIdOrThrowQuery, Guid>
{
    private readonly IReleaseQueryService releaseQueryService;

    public GetDefaultProductReleaseIdOrThrowQueryHandler(IReleaseQueryService releaseQueryService)
    {
        this.releaseQueryService = releaseQueryService;
    }

    public Task<Guid> Handle(GetDefaultProductReleaseIdOrThrowQuery request, CancellationToken cancellationToken)
    {
        Guid releaseId = this.releaseQueryService.GetDefaultProductReleaseIdOrThrow(
            request.TenantId,
            request.ProductId,
            request.Environment);
        return Task.FromResult(releaseId);
    }
}
