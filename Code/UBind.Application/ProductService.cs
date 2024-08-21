// <copyright file="ProductService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using NodaTime;
    using UBind.Application.Automation;
    using UBind.Application.Commands.Product;
    using UBind.Application.MicrosoftGraph;
    using UBind.Application.Services;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Notifications;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Processing;
    using UBind.Domain.Product;
    using UBind.Domain.ReadWriteModel;
    using UBind.Domain.ReferenceNumbers;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services;

    /// <inheritdoc/>
    public class ProductService : IProductService
    {
        private readonly IProductPortalSettingRepository productPortalSettingRepository;
        private readonly ICachingResolver cachingResolver;
        private readonly ITenantRepository tenantRepository;
        private readonly IQuoteService quoteService;
        private readonly ICreditNoteNumberRepository creditNoteNumberRepository;
        private readonly IInvoiceNumberRepository invoiceNumberRepository;
        private readonly IClaimNumberRepository claimNumberRepository;
        private readonly IPolicyNumberRepository policyNumberRepository;
        private readonly IProductRepository productRepository;
        private readonly ICachingAuthenticationTokenProvider authenticator;
        private readonly IFilesystemFileRepository fileRepository;
        private readonly IFilesystemStoragePathService pathService;
        private readonly IJobClient jobClient;
        private readonly IClock clock;
        private readonly ILogger<ProductService> logger;
        private readonly IProductFeatureSettingService productFeatureService;
        private readonly IAutomationPeriodicTriggerScheduler periodicTriggerScheduler;
        private readonly ICqrsMediator mediator;
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;

        private ProductDeploymentSetting defaultDeploymentSetting = new ProductDeploymentSetting()
        {
            Development = new List<string>()
                {
                        "*.ubind.io",
                        "*.ubind.com.au",
                        "localhost:*",
                        "bs-local.com",
                },
        };

        public ProductService(
            ITenantRepository tenantRepository,
            IProductRepository productRepository,
            ICachingAuthenticationTokenProvider authenticator,
            IFilesystemFileRepository fileRepository,
            IFilesystemStoragePathService pathService,
            IJobClient jobClient,
            IClock clock,
            IInvoiceNumberRepository invoiceNumberRepository,
            IClaimNumberRepository claimNumberRepository,
            IPolicyNumberRepository policyNumberRepository,
            ICreditNoteNumberRepository creditNoteNumberRepository,
            IQuoteService quoteService,
            IProductFeatureSettingService productFeatureService,
            IAutomationPeriodicTriggerScheduler periodicTriggerScheduler,
            ILogger<ProductService> logger,
            ICqrsMediator mediator,
            IHttpContextPropertiesResolver httpContextPropertiesResolver,
            IProductPortalSettingRepository productPortalSettingRepository,
            ICachingResolver cachingResolver)
        {
            Contract.Assert(productRepository != null);
            Contract.Assert(authenticator != null);
            Contract.Assert(fileRepository != null);
            Contract.Assert(jobClient != null);

            this.productPortalSettingRepository = productPortalSettingRepository;
            this.cachingResolver = cachingResolver;
            this.tenantRepository = tenantRepository;
            this.quoteService = quoteService;
            this.creditNoteNumberRepository = creditNoteNumberRepository;
            this.invoiceNumberRepository = invoiceNumberRepository;
            this.claimNumberRepository = claimNumberRepository;
            this.policyNumberRepository = policyNumberRepository;
            this.productRepository = productRepository;
            this.authenticator = authenticator;
            this.fileRepository = fileRepository;
            this.pathService = pathService;
            this.jobClient = jobClient;
            this.clock = clock;
            this.logger = logger;
            this.productFeatureService = productFeatureService;
            this.periodicTriggerScheduler = periodicTriggerScheduler;
            this.mediator = mediator;
            this.httpContextPropertiesResolver = httpContextPropertiesResolver;
        }

        /// <inheritdoc/>
        public async Task<Domain.Product.Product> CreateOrUpdateProduct(Guid tenantId, string productAlias, string name, bool disabled, bool deleted)
        {
            this.ThrowIfProductAliasIsNull(productAlias);
            var product = this.productRepository.GetProductByAlias(tenantId, productAlias);

            if (product != null
                && this.productRepository.ProductIdWasDeletedInTenant(tenantId, product.Id))
            {
                if (product != null
                    && !this.productRepository.ProductNameIsAvailableInTenant(product.TenantId, name, product.Id))
                {
                    throw new DuplicateProductNameException(
                        Errors.General.DuplicatePropertyValue("product", "name", name));
                }

                product = await this.mediator.Send(new UpdateProductCommand(
                    product.TenantId,
                    product.Id,
                    name,
                    productAlias,
                    product.Details.Disabled,
                    false,
                    null,
                    ProductQuoteExpirySettingUpdateType.UpdateNone));
                this.InitializeDeploymentSettings(product.TenantId, product.Id);

                var tenant = this.tenantRepository.GetTenantById(tenantId);
                if (this.fileRepository is GraphClient)
                {
                    var jobId = this.jobClient.Enqueue<ProductService>(
                        service => service.InitializeProduct(product.TenantId, product.Id, false),
                        tenant.Details.Alias,
                        product.Details.Alias);
                }
                else
                {
                    await this.InitializeProduct(product.TenantId, product.Id, false);
                }

                return product;
            }

            return await this.CreateProduct(tenantId, productAlias, name);
        }

        /// <inheritdoc/>
        public async Task<Domain.Product.Product> CreateProduct(Guid tenantId, string productAlias, string name, Guid? productId = null)
        {
            var now = this.clock.GetCurrentInstant();
            var tenant = this.tenantRepository.GetTenantById(tenantId);
            var product = new Domain.Product.Product(tenant.Id, productId ?? Guid.NewGuid(), name, productAlias, now);

            this.ValidateProduct(product);
            this.productRepository.Insert(product);
            this.productRepository.SaveChanges();
            this.productFeatureService.CreateProductFeatures(tenant.Id, product.Id);
            this.productPortalSettingRepository.CreateSettingsForAllDefaultOrganisationPortals(tenant.Id, product.Id);
            this.InitializeDeploymentSettings(tenant.Id, product.Id);

            // publish an event so that other parts of the system can respond to the creation of this product.
            var productCreatedEvent = new ProductCreatedEvent(
                product,
                this.httpContextPropertiesResolver.PerformingUserId,
                this.clock.Now());
            await this.mediator.Publish(productCreatedEvent);

            if (this.fileRepository is GraphClient)
            {
                this.jobClient.Enqueue<ProductService>(
                    service => service.InitializeProduct(product.TenantId, product.Id, false),
                    tenant.Details.Alias,
                    product.Details.Alias);
            }
            else
            {
                await this.InitializeProduct(product.TenantId, product.Id);
            }

            return product;
        }

        /// <inheritdoc/>
        public async Task<Domain.Product.Product> UpdateProduct(
            Guid tenantId,
            Guid productId,
            string name,
            string newAlias,
            bool disabled,
            bool deleted,
            CancellationToken cancellationToken,
            QuoteExpirySettings productQuoteExpirySetting = null,
            ProductQuoteExpirySettingUpdateType updateType = ProductQuoteExpirySettingUpdateType.UpdateNone)
        {
            this.ThrowIfProductAliasIsNull(newAlias);
            var product = this.productRepository.GetProductById(tenantId, productId);
            var oldAlias = product.Details.Alias;
            var aliasChanged = oldAlias != newAlias;
            if (product == null)
            {
                throw new ErrorException(Errors.General.NotFound("product", productId));
            }

            // change in alias.
            if (aliasChanged)
            {
                var checkIfExistsQuery =
                    new UBind.Application.Queries.Product.ThrowIfHasNewProductDirectoryExistsQuery(
                        tenantId,
                        productId,
                        newAlias,
                        this.httpContextPropertiesResolver.PerformingUserId);
                await this.mediator.Send(checkIfExistsQuery);
            }

            var oldQuoteExpiry = product.Details.QuoteExpirySetting;
            bool quoteExpiryChanged = oldQuoteExpiry != null
                ? !oldQuoteExpiry.Equals(productQuoteExpirySetting)
                : false;
            var details = new ProductDetails(
                name,
                newAlias,
                disabled,
                deleted,
                this.clock.Now(),
                product.Details.DeploymentSetting,
                quoteExpiryChanged ? productQuoteExpirySetting : product.Details.QuoteExpirySetting);
            this.ValidateProductDetails(product.TenantId, details, product.Id);
            product.Update(details);
            this.productRepository.SaveChanges();

            if (details.Disabled || details.Deleted)
            {
                this.periodicTriggerScheduler.RemovePeriodicTriggerJobs(product);
            }

            if (!details.Disabled && !details.Deleted)
            {
                await this.periodicTriggerScheduler.RegisterPeriodicTriggerJobs(product.TenantId, product.Id);
            }

            this.cachingResolver.RemoveCachedProducts(
                product.TenantId,
                product.Id,
                new List<string> { oldAlias, newAlias });
            if (aliasChanged)
            {
                var onAliasChange =
                    new ProductAliasChangeDomainEvent(
                        product.TenantId,
                        product.Id,
                        oldAlias,
                        newAlias,
                        this.httpContextPropertiesResolver.PerformingUserId,
                        this.clock.Now());
                await this.mediator.Publish(onAliasChange);
            }

            if (productQuoteExpirySetting != null
                && quoteExpiryChanged)
            {
                this.UpdateQuoteExpirySettings(tenantId, productId, productQuoteExpirySetting, updateType, cancellationToken);
            }

            return product;
        }

        /// <inheritdoc/>
        public Domain.Product.Product UpdateDeploymentSettings(Guid tenantId, Guid productId, ProductDeploymentSetting deploymentSettings)
        {
            var product = this.productRepository.GetProductById(tenantId, productId);
            var details = new ProductDetails(
                product.Details.Name,
                product.Details.Alias,
                product.Details.Disabled,
                product.Details.Deleted,
                this.clock.Now(),
                deploymentSettings,
                product.Details.QuoteExpirySetting);
            product.Update(details);
            this.productRepository.SaveChanges();
            return product;
        }

        /// <inheritdoc/>
        public Domain.Product.Product UpdateQuoteExpirySettings(
            Guid tenantId,
            Guid productId,
            QuoteExpirySettings expirySettings,
            ProductQuoteExpirySettingUpdateType updateType,
            CancellationToken cancellationToken)
        {
            if (expirySettings == null)
            {
                throw new ArgumentNullException("Expiry settings should have value.");
            }

            var product = this.productRepository.GetProductById(tenantId, productId);
            var oldQuoteExpiry = product.Details.QuoteExpirySetting;
            bool quoteExpiryChanged = oldQuoteExpiry != null
                ? !oldQuoteExpiry.Equals(expirySettings)
                : false;
            if (quoteExpiryChanged)
            {
                var details = new ProductDetails(
                product.Details.Name,
                product.Details.Alias,
                product.Details.Disabled,
                product.Details.Deleted,
                this.clock.Now(),
                product.Details.DeploymentSetting,
                expirySettings);
                product.Update(details);
                this.productRepository.SaveChanges();
            }

            // do the background job here regarding quote expiry update.
            if (!expirySettings.Enabled)
            {
                this.jobClient.Enqueue<IQuoteService>(
                    service => service.RemoveExpiryDates(product.TenantId, product.Id, cancellationToken),
                    JobParameter.Tenant(product.Details.Alias),
                    JobParameter.Product(product.Details.Alias));
            }
            else if (updateType != ProductQuoteExpirySettingUpdateType.UpdateNone)
            {
                var tenant = this.tenantRepository.GetTenantById(tenantId);
                this.jobClient.Enqueue<IQuoteService>(
                    service => service.UpdateExpiryDates(product.TenantId, product.Id, expirySettings, updateType, cancellationToken),
                    JobParameter.Tenant(tenant.Details.Alias),
                    JobParameter.Product(product.Details.Alias));
            }

            return product;
        }

        /// <inheritdoc/>
        public ProductDeploymentSetting GetDeploymentSettings(Guid tenantId, Guid productId)
        {
            var product = this.productRepository.GetProductById(tenantId, productId);

            return product.Details.DeploymentSetting ?? this.defaultDeploymentSetting;
        }

        /// <inheritdoc/>
        public async Task InitializeProduct(Guid tenantId, Guid productId, bool createComponentFiles = false)
        {
            var product = this.productRepository.GetProductById(tenantId, productId);
            var tenant = this.tenantRepository.GetTenantById(tenantId);
            var tenantAlias = tenant.Details.Alias;
            var productAlias = product.Details.Alias;

            try
            {
                string bearerToken = await this.GetBearerToken();

                // Create tenant development folder
                await this.fileRepository.CreateFolder(this.pathService.DevelopmentFolderPath, tenantAlias, bearerToken);

                // Create product development folder
                await this.fileRepository.CreateFolder(this.pathService.GetTenantDevelopmentFolder(tenantAlias), productAlias, bearerToken);

                var productDevelopmentFolder = this.pathService.GetTenantProductDevelopmentFolder(tenantAlias, productAlias);
                bool folderIsEmpty = !(await this.fileRepository.ListItemsInFolder(productDevelopmentFolder, bearerToken)).Any();

                if (folderIsEmpty && createComponentFiles)
                {
                    // Create Quotes development folder
                    await this.fileRepository.CreateFolder(
                        this.pathService.GetTenantProductDevelopmentFolder(tenantAlias, productAlias),
                        this.pathService.GetWebFormAppFolderName(WebFormAppType.Quote),
                        bearerToken);

                    // Create Claims development folder
                    await this.fileRepository.CreateFolder(
                        this.pathService.GetTenantProductDevelopmentFolder(tenantAlias, productAlias),
                        this.pathService.GetWebFormAppFolderName(WebFormAppType.Claim),
                        bearerToken);

                    // Copy default workbook
                    var quoteFolder = this.pathService.GetProductDevelopmentAppFolder(tenantAlias, productAlias, WebFormAppType.Quote);
                    await this.fileRepository.WriteFileContents(
                        this.GetDefaultQuoteWorkbookBytes(),
                        Path.Combine(quoteFolder, this.pathService.GetProductWorkbookName(tenantAlias, productAlias)),
                        bearerToken);

                    var claimFolder = this.pathService.GetProductDevelopmentAppFolder(tenantAlias, productAlias, WebFormAppType.Claim);
                    await this.fileRepository.WriteFileContents(
                       this.GetDefaultQuoteWorkbookBytes(),
                       Path.Combine(claimFolder, this.pathService.GetProductWorkbookName(tenantAlias, productAlias)),
                       bearerToken);

                    // Copy default workflow file
                    await this.fileRepository.WriteFileContents(
                        this.GetDefaultWorkflowFileBytes(),
                        Path.Combine(quoteFolder, this.pathService.WorkflowFileName),
                        bearerToken);

                    await this.fileRepository.WriteFileContents(
                        this.GetDefaultWorkflowFileBytes(),
                        Path.Combine(claimFolder, this.pathService.WorkflowFileName),
                        bearerToken);

                    // Create files folder
                    await this.fileRepository.CreateFolder(
                        quoteFolder,
                        this.pathService.MiscFilesFolderName,
                        bearerToken);

                    await this.fileRepository.CreateFolder(
                        claimFolder,
                        this.pathService.MiscFilesFolderName,
                        bearerToken);

                    // Creates assets folder
                    await this.fileRepository.CreateFolder(
                       quoteFolder,
                       this.pathService.AssetFilesFolderName,
                       bearerToken);

                    await this.fileRepository.CreateFolder(
                        claimFolder,
                        this.pathService.AssetFilesFolderName,
                        bearerToken);
                }

                List<DeploymentEnvironment> environments = new List<DeploymentEnvironment>
                {
                    DeploymentEnvironment.Development,
                    DeploymentEnvironment.Staging,
                };

                // initialize invoice, claim, policy
                foreach (var environment in environments)
                {
                    this.invoiceNumberRepository.Seed(
                        product.TenantId,
                        product.Id,
                        environment);
                    this.claimNumberRepository.Seed(
                        product.TenantId,
                        product.Id,
                        environment);
                    this.policyNumberRepository.Seed(
                        product.TenantId,
                        product.Id,
                        environment);
                    this.creditNoteNumberRepository.Seed(
                        product.TenantId,
                        product.Id,
                        environment);
                }

                product.OnInitized(this.clock.GetCurrentInstant());
                this.productRepository.SaveChanges();
            }
            catch (Exception ex)
            {
                // Log the error for trouble shooting why the product is not initialised.
                string messageFormat = $"Exception thrown in {nameof(ProductService)}.{{0}}: {{1}}";
                this.logger.LogError(ex, messageFormat, messageFormat, "InitializeProduct", ex.Message);

                product.OnInitizationFailed(this.clock.GetCurrentInstant());
                this.productRepository.SaveChanges();
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task SeedFilesAsync(Guid tenantId, Guid productId, string environment, FileModel file, string folder)
        {
            var tenant = this.tenantRepository.GetTenantById(tenantId);
            var product = this.productRepository.GetProductById(tenantId, productId);
            string bearerToken = await this.GetBearerToken();
            var path = this.pathService.GetProductDevelopmentAppFolder(tenant.Details.Alias, product.Details.Alias, WebFormAppType.Quote);

            if (folder != null)
            {
                await this.fileRepository.CreateFolder(path, folder, bearerToken);
            }

            path = Path.Combine(path, folder ?? string.Empty, file.FileName);

            await this.fileRepository.WriteFileContents(file.Bytes, path, bearerToken);
        }

        /// <inheritdoc/>
        public Domain.Product.Product GetProductById(Guid tenantId, Guid productId)
        {
            var product = this.productRepository.GetProductById(tenantId, productId);
            return product;
        }

        private async Task MoveFolder(string oldPath, string newPath, string bearerToken)
        {
            if (await this.fileRepository.FolderExists(oldPath, bearerToken))
            {
                await this.fileRepository.MoveFolder(oldPath, newPath, true, bearerToken);
            }
            else
            {
                await this.fileRepository.CreateFolder(newPath, bearerToken);
            }
        }

        // Throw error message if the alias value is null
        private void ThrowIfProductAliasIsNull(string alias)
        {
            if (!string.IsNullOrEmpty(alias) && alias.ToLower() == "null")
            {
                throw new ErrorException(
                    Errors.Product.AliasIsNull(alias));
            }
        }

        /// <summary>
        /// Validate the product name if system already has existing name or alias of the same object.
        /// </summary>
        /// <param name="product">The object to be validated.</param>
        private void ValidateProduct(Domain.Product.Product product)
        {
            if (!this.productRepository.ProductAliasIsAvailableInTenant(product.TenantId, product.Details.Alias, product.Id))
            {
                throw new DuplicateProductNameException(
                    Errors.General.DuplicatePropertyValue("product", "alias", product.Details.Alias));
            }

            if (!this.productRepository.ProductNameIsAvailableInTenant(product.TenantId, product.Details.Name, product.Id))
            {
                throw new DuplicateProductNameException(
                    Errors.General.DuplicatePropertyValue("product", "name", product.Details.Name));
            }
        }

        /// <summary>
        /// Validate the product details if system already has existing name or alias of the same object.
        /// </summary>
        /// <param name="tenantId">The guid tenant id.</param>
        /// <param name="details">The object to be validated.</param>
        /// <param name="productId">The guid product id.</param>
        private void ValidateProductDetails(Guid tenantId, ProductDetails details, Guid productId)
        {
            if (!this.productRepository.ProductAliasIsAvailableInTenant(tenantId, details.Alias, productId))
            {
                throw new DuplicateProductNameException(
                    Errors.General.DuplicatePropertyValue("product", "alias", details.Alias));
            }

            if (!this.productRepository.ProductNameIsAvailableInTenant(tenantId, details.Name, productId))
            {
                throw new DuplicateProductNameException(
                    Errors.General.DuplicatePropertyValue("product", "name", details.Name));
            }
        }

        /// <summary>
        /// initializes the deployment settings.
        /// </summary>
        /// <param name="tenantId">tenant id.</param>
        /// <param name="productId">product id.</param>
        private Domain.Product.Product InitializeDeploymentSettings(Guid tenantId, Guid productId)
        {
            // initiate deployment settings.
            this.defaultDeploymentSetting.DevelopmentIsActive =
            this.defaultDeploymentSetting.StagingIsActive =
            this.defaultDeploymentSetting.ProductionIsActive = true;
            return this.UpdateDeploymentSettings(tenantId, productId, this.defaultDeploymentSetting);
        }

        /// <summary>
        /// Gets the default quote workbook as a byte array.
        /// </summary>
        /// <returns>The default quote workbook as a byte array.</returns>
        private byte[] GetDefaultQuoteWorkbookBytes()
        {
            var assembly = System.Reflection.Assembly.GetEntryAssembly();

            // when running unit tests, GetEntryAssembly() will return null
            if (assembly == null)
            {
                return Array.Empty<byte>();
            }

            var location = assembly.Location;
            var directory = Path.GetDirectoryName(location);
            var localDefaultWorkbookPath = Path.Combine(directory, "Templates", this.pathService.DefaultWorkbookName);
            return System.IO.File.ReadAllBytes(localDefaultWorkbookPath);
        }

        private byte[] GetDefaultWorkflowFileBytes()
        {
            var assembly = System.Reflection.Assembly.GetEntryAssembly();

            // when running unit tests, GetEntryAssembly() will return null
            if (assembly == null)
            {
                return Array.Empty<byte>();
            }

            var location = assembly.Location;
            var directory = Path.GetDirectoryName(location);
            var localDefaultWorkflowPath = Path.Combine(directory, "Templates", this.pathService.WorkflowFileName);
            return System.IO.File.ReadAllBytes(localDefaultWorkflowPath);
        }

        private async Task<string> GetBearerToken()
        {
            return await this.fileRepository.GetAuthenticationToken();
        }
    }
}
