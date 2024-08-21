// <copyright file="ProductFileProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Providers.File
{
    using System;
    using System.Collections.Generic;
    using MorseCode.ITask;
    using UBind.Application.Automation.Data;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Queries.AssetFile;
    using UBind.Application.Releases;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Product;
    using UBind.Domain.ValueTypes;

    /// <summary>
    /// Text File Provider.
    /// </summary>
    public class ProductFileProvider : IProvider<Data<FileInfo>>
    {
        private readonly ICqrsMediator cqrsMediator;
        private readonly ICachingResolver cachingResolver;
        private readonly IReleaseQueryService releaseQueryService;
        private readonly string repository;
        private readonly string visibility;
        private readonly IProvider<Data<string>>? outputFileName;
        private readonly IProvider<Data<string>>? productAlias;
        private readonly IProvider<Data<string>>? environment;
        private readonly IProvider<Data<string>> filePath;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductFileProvider"/> class.
        /// </summary>
        /// <param name="releaseQueryService">Service for getting the current release for a given product and environment.</param>
        /// <param name="cachingResolver">The tenant, product and portal resolver.</param>
        /// <param name="repository">The folder where the asset folder resides (e.g. "quote", "claim", or "shared").</param>
        /// <param name="visibility">The folder where the asset folder resides (e.g. "private" or "public").</param>
        /// <param name="outputFileName">The file name.</param>
        /// <param name="productAlias">The product to get the file.</param>
        /// <param name="environment">The environment to get the file.</param>
        /// <param name="filePath">The path in the asset folder.</param>
        public ProductFileProvider(
            string repository,
            string visibility,
            IProvider<Data<string>> filePath,
            IProvider<Data<string>>? outputFileName,
            IProvider<Data<string>>? productAlias,
            IProvider<Data<string>>? environment,
            ICqrsMediator cqrsMediator,
            IReleaseQueryService releaseQueryService,
            ICachingResolver cachingResolver)
        {
            this.cqrsMediator = cqrsMediator;
            this.cachingResolver = cachingResolver;
            this.repository = repository;
            this.releaseQueryService = releaseQueryService;
            this.visibility = visibility;
            this.outputFileName = outputFileName;
            this.productAlias = productAlias;
            this.environment = environment;
            this.filePath = filePath;
        }

        public string SchemaReferenceKey => "productFile";

        /// <inheritdoc/>
        public async ITask<IProviderResult<Data<FileInfo>>> Resolve(IProviderContext providerContext)
        {
            var tenantId = providerContext.AutomationData.ContextManager.Tenant.Id;
            var productAlias = (await this.productAlias.ResolveValueIfNotNull(providerContext))?.DataValue;
            var filePath = (await this.filePath.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue;
            var fileName = (await this.outputFileName.ResolveValueIfNotNull(providerContext))?.DataValue;
            var env = (await this.environment.ResolveValueIfNotNull(providerContext))?.DataValue;
            DeploymentEnvironment environment = env.ToEnumOrNull<DeploymentEnvironment>()
                ?? providerContext.AutomationData.System.Environment;

            if (string.IsNullOrWhiteSpace(productAlias))
            {
                Guid? productId = providerContext.AutomationData.ContextManager.Product?.Id;
                if (productId == null)
                {
                    string? productIdString = providerContext.AutomationData.Automation[AutomationData.ProductId]?.ToString();
                    if (string.IsNullOrWhiteSpace(productIdString))
                    {
                        throw new ErrorException(Errors.Automation.ProviderParameterMissing(
                            "productAlias",
                            this.SchemaReferenceKey));
                    }
                    productId = Guid.Parse(productIdString);
                }

                productAlias = await this.cachingResolver.GetProductAliasOrThrowAsync(tenantId, (Guid)productId);
            }

            if (string.IsNullOrWhiteSpace(fileName))
            {
                fileName = System.IO.Path.GetFileName(filePath);
            }

            var errorData = await providerContext.GetDebugContextForProviders(this.SchemaReferenceKey);
            var additionalDetails = new List<string>();
            errorData.Add("filePath", filePath);
            errorData.Add("outputFilename", fileName);
            errorData.Add("repository", this.repository);
            errorData.Add("visibility", this.visibility);
            additionalDetails.Add("File path: " + filePath);
            additionalDetails.Add("Output file name: " + fileName);
            additionalDetails.Add("Repository: " + this.repository);
            additionalDetails.Add("Visibility: " + this.visibility);

            if (string.IsNullOrWhiteSpace(productAlias) || environment == DeploymentEnvironment.None)
            {
                throw new ErrorException(Errors.Automation.Provider.ProductIdBlank(errorData));
            }

            var product = await this.cachingResolver.GetProductByAliasOrNull(tenantId, productAlias);
            if (product == null || product.Details.Deleted)
            {
                throw new ErrorException(
                    Errors.Automation.Provider.ProductNotFound(
                        productAlias,
                        errorData));
            }

            var formType = default(WebFormAppType);
            if (this.repository.EqualsIgnoreCase("shared"))
            {
                formType = WebFormAppType.Quote;
            }
            else
            {
                if (!Enum.TryParse(this.repository, true, out formType))
                {
                    throw new ErrorException(
                        Errors.Automation.ParameterValueTypeInvalid(
                            this.SchemaReferenceKey,
                            "repository",
                            this.repository,
                            errorData,
                            null,
                            $"The {this.SchemaReferenceKey} only accepts \"quote\" or \"claim\" for repository value. "));
                }
            }

            var visibility = default(FileVisibility);
            if (!Enum.TryParse(this.visibility, true, out visibility))
            {
                throw new ErrorException(
                    Errors.Automation.ParameterValueTypeInvalid(
                        this.SchemaReferenceKey,
                        "visibility",
                        this.visibility,
                        errorData,
                        null,
                        $"The {this.SchemaReferenceKey} only accepts \"public\" or \"private\" for visibility value. "));
            }

            try
            {
                Guid? releaseId = providerContext.AutomationData.ContextManager.ProductRelease?.Id;
                var releaseContext = releaseId == null
                    ? this.releaseQueryService.GetDefaultReleaseContextOrThrow(tenantId, product.Id, environment)
                    : new ReleaseContext(tenantId, product.Id, environment, releaseId.Value);
                byte[] contents = await this.cqrsMediator.Send(new GetProductFileContentsByFileNameQuery(
                    releaseContext,
                    formType,
                    visibility,
                    filePath));
                var filename = new FileName(fileName);
                return ProviderResult<Data<FileInfo>>.Success(new FileInfo(filename.ToString(), contents));
            }
            catch (ErrorException ex)
            {
                ex.EnrichAndRethrow(errorData, additionalDetails);

                // This is unreachable code, but the compiler doesn't know that.
                return ProviderResult<Data<FileInfo>>.Success(null!);
            }
        }
    }
}
