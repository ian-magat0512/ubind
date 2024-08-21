// <copyright file="GetReleaseFileWithoutCachingQuery.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.ProductRelease;

using UBind.Domain;
using UBind.Domain.Patterns.Cqrs;
using UBind.Application.Models;

/// <summary>
/// Gets a release file for inspection.
/// Warning: This is slow. It does not use caching. Do not use this for normal operations.
/// </summary>
public class GetReleaseFileWithoutCachingQuery : IQuery<FileContentsDto>
{
    public GetReleaseFileWithoutCachingQuery(
        Guid tenantId,
        Guid productReleaseId,
        WebFormAppType webFormAppType,
        string path)
    {
        this.TenantId = tenantId;
        this.ProductReleaseId = productReleaseId;
        this.WebformAppType = webFormAppType;
        this.Path = path;
    }

    public Guid TenantId { get; }

    public Guid ProductReleaseId { get; }

    public WebFormAppType WebformAppType { get; }

    public string Path { get; }
}
