// <copyright file="GetReleaseContextForReleaseOrDefaultReleaseQuery.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.ProductRelease;

using UBind.Domain;
using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.Product;

public class GetReleaseContextForReleaseOrDefaultReleaseQuery : IQuery<ReleaseContext>
{
    public GetReleaseContextForReleaseOrDefaultReleaseQuery(
            Guid tenantId,
            Guid productId,
            DeploymentEnvironment environment,
            Guid? productReleaseId)
    {
        this.TenantId = tenantId;
        this.ProductId = productId;
        this.Environment = environment;
        this.ProductReleaseId = productReleaseId;
    }

    public Guid TenantId { get; }

    public Guid ProductId { get; }

    public DeploymentEnvironment Environment { get; }

    public Guid? ProductReleaseId { get; }
}
