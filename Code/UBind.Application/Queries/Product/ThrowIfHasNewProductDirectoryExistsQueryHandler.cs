// <copyright file="ThrowIfHasNewProductDirectoryExistsQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.Product
{
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using UBind.Application.MicrosoftGraph;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Product;
    using UBind.Domain.Repositories;

    /// <summary>
    /// Check if new product directory exists.
    /// </summary>
    public class ThrowIfHasNewProductDirectoryExistsQueryHandler :
        IQueryHandler<ThrowIfHasNewProductDirectoryExistsQuery, Unit>
    {
        private readonly IFilesystemStoragePathService pathService;
        private readonly IProductRepository productRepository;
        private readonly ICachingAuthenticationTokenProvider authenticator;
        private readonly IFilesystemFileRepository fileRepository;
        private readonly ITenantRepository tenantRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="ThrowIfHasNewProductDirectoryExistsQueryHandler"/> class.
        /// </summary>
        public ThrowIfHasNewProductDirectoryExistsQueryHandler(
            ICachingAuthenticationTokenProvider authenticator,
            ITenantRepository tenantRepository,
            IProductRepository productRepository,
            IFilesystemStoragePathService pathService,
            IFilesystemFileRepository fileRepository)
        {
            this.productRepository = productRepository;
            this.authenticator = authenticator;
            this.fileRepository = fileRepository;
            this.tenantRepository = tenantRepository;
            this.pathService = pathService;
        }

        /// <inheritdoc/>
        public async Task<Unit> Handle(
            ThrowIfHasNewProductDirectoryExistsQuery command,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            string bearerToken = await this.GetBearerToken();
            var tenant = this.tenantRepository.GetTenantById(command.TenantId);
            var product = this.productRepository.GetProductById(command.TenantId, command.ProductId);
            string currentAlias = product.Details.Alias;

            if (command.NewProductAlias == currentAlias)
            {
                return Unit.Value;
            }

            var newDevelopmentPath = this.pathService.GetProductDevelopmentFolder(tenant.Details.Alias, command.NewProductAlias);
            var newReleasePath = this.pathService.GetProductReleasesFolder(tenant.Details.Alias, command.NewProductAlias);

            var developmentPathExists = await this.fileRepository.FolderExists(newDevelopmentPath, bearerToken);
            var releasePathExists = await this.fileRepository.FolderExists(newReleasePath, bearerToken);

            if (developmentPathExists)
            {
                // throw error
                throw new ErrorException(Errors.Product.ProductDirectoryAlreadyExists(
                    tenant.Details.Alias,
                    currentAlias,
                    command.NewProductAlias,
                    this.pathService.DevelopmentFolderName));
            }

            if (releasePathExists)
            {
                // throw error
                throw new ErrorException(Errors.Product.ProductDirectoryAlreadyExists(
                    tenant.Details.Alias,
                    currentAlias,
                    command.NewProductAlias,
                    this.pathService.ReleasesFolderName));
            }

            return Unit.Value;
        }

        private async Task<string> GetBearerToken()
        {
            return await this.fileRepository.GetAuthenticationToken();
        }
    }
}
