// <copyright file="GetProductReleaseSourceFilesQuery.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.Product;

using UBind.Domain.Dto;
using UBind.Domain.Patterns.Cqrs;

public class GetProductReleaseSourceFilesQuery : IQuery<List<ConfigurationFileDto>>
{
    /// <param name="releaseId">The release ID. If not passed, the latest dev release is used.</param>
    private GetProductReleaseSourceFilesQuery(Guid tenantId, Guid? productId, Guid? releaseId)
    {
        this.TenantId = tenantId;
        this.ProductId = productId;
        this.ReleaseId = releaseId;
    }

    public Guid TenantId { get; }

    public Guid? ProductId { get; }

    public Guid? ReleaseId { get; }

    public static GetProductReleaseSourceFilesQuery CreateForDevRelease(Guid tenantId, Guid productId)
    {
        return new GetProductReleaseSourceFilesQuery(tenantId, productId, null);
    }

    public static GetProductReleaseSourceFilesQuery CreateForRelease(Guid tenantId, Guid releaseId)
    {
        return new GetProductReleaseSourceFilesQuery(tenantId, null, releaseId);
    }
}
