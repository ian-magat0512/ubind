// <copyright file="GetProductFileContentsByFileNameQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.AssetFile;

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UBind.Application.Releases;
using UBind.Domain;
using UBind.Domain.Exceptions;
using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.Product;
using UBind.Domain.Repositories;

public class GetProductFileContentsByFileNameQueryHandler : IQueryHandler<GetProductFileContentsByFileNameQuery, byte[]>
{
    private readonly ICachingResolver cachingResolver;
    private readonly IReleaseQueryService releaseQueryService;
    private readonly IFileContentRepository fileContentRepository;

    public GetProductFileContentsByFileNameQueryHandler(
        IReleaseQueryService releaseQueryService,
        ICachingResolver cachingResolver,
        IFileContentRepository fileContentRepository)
    {
        this.cachingResolver = cachingResolver;
        this.releaseQueryService = releaseQueryService;
        this.fileContentRepository = fileContentRepository;
    }

    public async Task<byte[]> Handle(GetProductFileContentsByFileNameQuery query, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var release = this.releaseQueryService.GetRelease(query.ReleaseContext);

        ProductComponentConfiguration componentConfig
            = release.GetProductComponentConfigurationOrThrow(query.WebformAppType);
        var file = query.Visibility == FileVisibility.Private
            ? componentConfig.Files
                .Where(af => af.Name == query.FileName)
                .OrderByDescending(af => af.FileModifiedTicksSinceEpoch)
                .FirstOrDefault()
            : componentConfig.Assets
                .Where(af => af.Name == query.FileName)
                .OrderByDescending(af => af.FileModifiedTicksSinceEpoch)
                .FirstOrDefault();
        if (file == null)
        {
            var tenantAlias = await this.cachingResolver.GetTenantAliasOrThrowAsync(query.ReleaseContext.TenantId);
            var productAlias = await this.cachingResolver.GetProductAliasOrThrowAsync(query.ReleaseContext.TenantId, query.ReleaseContext.ProductId);

            throw new ErrorException(Errors.Product.File.NotFound(
                tenantAlias,
                productAlias,
                query.ReleaseContext.Environment,
                query.WebformAppType,
                query.Visibility,
                query.FileName));
        }

        this.LoadFileContentsIfNull(query.ReleaseContext.TenantId, file);
        return await Task.FromResult(file.FileContent.Content);
    }

    private void LoadFileContentsIfNull(Guid tenantId, Asset file)
    {
        if (file.FileContent == null)
        {
            // load the file content into memory - it will remain and be ready for next time,
            // until the release is removed from memory
            file.FileContent = this.fileContentRepository.GetFileContent(tenantId, file.FileContentId);
        }
    }
}
