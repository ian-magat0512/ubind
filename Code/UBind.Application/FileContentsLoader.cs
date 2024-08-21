// <copyright file="FileContentsLoader.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application
{
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json.Linq;
    using UBind.Application.Queries.AssetFile;
    using UBind.Application.Releases;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Product;

    /// <inheritdoc/>
    public class FileContentsLoader : IFileContentsLoader
    {
        private readonly ICachingResolver cachingResolver;
        private readonly IReleaseQueryService releaseQueryService;
        private readonly ILogger logger;
        private readonly ICqrsMediator mediator;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileContentsLoader"/> class.
        /// </summary>
        /// <param name="releaseQueryService">Service for creating releases.</param>
        /// <param name="logger">A logger.</param>
        public FileContentsLoader(
            IReleaseQueryService releaseQueryService,
            ILogger<FileContentsLoader> logger,
            ICachingResolver cachingResolver,
            ICqrsMediator mediator)
        {
            this.cachingResolver = cachingResolver;
            this.releaseQueryService = releaseQueryService;
            this.logger = logger;
            this.mediator = mediator;
        }

        /// <inheritdoc/>
        public string Load(
            ReleaseContext releaseContext,
            string filename)
        {
            var release = this.releaseQueryService.GetRelease(releaseContext);
            ProductComponentConfiguration componentConfig =
                release.GetProductComponentConfigurationOrThrow(WebFormAppType.Quote);

            // Old releases may still be storing file contents as strings and use this old property.
            var file = componentConfig.Files
                .Where(f => f.Name == filename)
                .OrderByDescending(f => f.FileModifiedTicksSinceEpoch)
                .FirstOrDefault();
            if (file != null)
            {
                var tenantAlias = this.cachingResolver.GetTenantAliasOrThrow(releaseContext.TenantId);
                var productAlias = this.cachingResolver.GetProductAliasOrThrow(releaseContext.TenantId, releaseContext.ProductId);

                // Log so we can track if any deployed releases are still using old storage.
                this.logger.LogInformation(
                    "Product {tenantAlias}/{productAlias} release in {environment} (id: {releaseId}) is still using old file storage.",
                    tenantAlias,
                    productAlias,
                    releaseContext.Environment,
                    release.ReleaseId);
                return file.Name ?? string.Empty;
            }

            // All files are loaded from quote configuration currently. This will change in the future so quote and claim
            // apps can have different file sets.
            var file2 = componentConfig.Files
                .Where(f => f.Name == filename)
                .OrderByDescending(f => f.FileModifiedTicksSinceEpoch)
                .FirstOrDefault();
            var content = file2?.FileContent?.Content;
            return file2 != null ? (System.Text.Encoding.UTF8.GetString(content) ?? string.Empty) : string.Empty;
        }

        /// <inheritdoc/>
        public async Task<byte[]> LoadData(
            ReleaseContext releaseContext,
            string filename)
        {
            ActiveDeployedRelease release = this.releaseQueryService.GetRelease(releaseContext);
            ProductComponentConfiguration componentConfig =
                release.GetProductComponentConfigurationOrThrow(WebFormAppType.Quote);
            return await this.GetFileBytes(releaseContext, filename, componentConfig);
        }

        private async Task<byte[]> GetFileBytes(
            ReleaseContext releaseContext,
            string filename,
            ProductComponentConfiguration componentConfig,
            WebFormAppType webFormAppType = WebFormAppType.Quote,
            FileVisibility fileVisibility = FileVisibility.Private)
        {
            // All files are loaded from quote configuration currently. This will change in the future so quote and claim
            // apps can have different file sets.
            var file2 = componentConfig.Files
                .Where(f => f.Name == filename)
                .OrderByDescending(f => f.FileModifiedTicksSinceEpoch)
                .FirstOrDefault();
            if (file2 == null)
            {
                var errorData = new JObject
                {
                    { "tenantId", releaseContext.TenantId },
                    { "productId", releaseContext.ProductId },
                    { "environment", releaseContext.Environment.ToString() },
                    { "productReleaseId", releaseContext.ProductReleaseId },
                    { "filename", filename },
                };
                throw new ErrorException(Errors.Automation.AssetNotFound(filename, errorData));
            }

            var fileContent = file2?.FileContent?.Content;
            if (fileContent == null)
            {
                fileContent = await this.mediator.Send(new GetProductFileContentsByFileNameQuery(
                    releaseContext,
                    webFormAppType,
                    fileVisibility,
                    filename));
            }

            return fileContent;
        }
    }
}
