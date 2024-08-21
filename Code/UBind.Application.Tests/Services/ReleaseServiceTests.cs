// <copyright file="ReleaseServiceTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Services
{
    using Hangfire;
    using Moq;
    using NodaTime;
    using UBind.Application.FlexCel;
    using UBind.Application.MicrosoftGraph;
    using UBind.Application.Releases;
    using UBind.Domain;
    using UBind.Domain.Product;
    using UBind.Domain.Repositories;

    public class ReleaseServiceTests
    {
        protected const string TestConnectionStringName = "UBindTestDatabase";
        private readonly Mock<IReleaseRepository> releaseRepository;
        private readonly Mock<IDevReleaseRepository> devReleaseRepository;
        private readonly Mock<IProductRepository> productRepository;
        private readonly Mock<IProductFeatureSettingRepository> productFeatureSettingRepository;
        private readonly Mock<ICachingAuthenticationTokenProvider> authenticator;
        private readonly Mock<IFilesystemFileRepository> fileRepository;
        private readonly Mock<IFilesystemStoragePathService> pathService;
        private readonly Mock<IFormConfigurationGenerator> devConfigurationReader;
        private readonly Mock<IBackgroundJobClient> jobClient;
        private readonly Mock<ISpreadsheetPoolService> spreadsheetPoolService;
        private readonly Mock<IReleaseValidator> releaseValidator;
        private readonly Mock<IGlobalReleaseCache> globalReleaseCache;
        private readonly Mock<ICachingResolver> cachingResolver;
        private readonly Mock<IClock> clock;
        private readonly Mock<IFileContentRepository> fileContentRepository;
        private readonly Mock<IFilesystemStorageConfiguration> filesystemStorageConfiguration;

        public ReleaseServiceTests()
        {
            this.releaseRepository = new Mock<IReleaseRepository>();
            this.devReleaseRepository = new Mock<IDevReleaseRepository>();
            this.productRepository = new Mock<IProductRepository>();
            this.authenticator = new Mock<ICachingAuthenticationTokenProvider>();
            this.fileRepository = new Mock<IFilesystemFileRepository>();
            this.pathService = new Mock<IFilesystemStoragePathService>();
            this.devConfigurationReader = new Mock<IFormConfigurationGenerator>();
            this.jobClient = new Mock<IBackgroundJobClient>();
            this.spreadsheetPoolService = new Mock<ISpreadsheetPoolService>();
            this.releaseValidator = new Mock<IReleaseValidator>();
            this.globalReleaseCache = new Mock<IGlobalReleaseCache>();
            this.cachingResolver = new Mock<ICachingResolver>();
            this.clock = new Mock<IClock>();
            this.productFeatureSettingRepository = new Mock<IProductFeatureSettingRepository>();
            this.fileContentRepository = new Mock<IFileContentRepository>();
            this.filesystemStorageConfiguration = new Mock<IFilesystemStorageConfiguration>();
        }

        /*
        [Fact]
        public async void CreateDevRelease_InsertsNewFileContent_WhenSynchronised()
        {
            // Arrange
            var tenant = new Tenant(
                TenantFactory.DefaultId, "Leon", "leon", null, default, default, SystemClock.Instance.Now());
            var product = new Product(
                TenantFactory.DefaultId, ProductFactory.DefaultId, "Dev", "dev", SystemClock.Instance.Now());

            var authenticationToken = new AuthenticationToken("bearerToken", SystemClock.Instance.GetCurrentInstant());
            this.authenticator.Setup(auth => auth.GetAuthenticationTokenAsync())
                .Returns(Task.FromResult(authenticationToken));

            var fileInBytes = Fakes.Workbook.Standard;
            var workbook = FileContent.CreateFromBytes(tenant.Id, Guid.NewGuid(), fileInBytes);

            var devHashString = "Empty File".GetHashString();
            var oldDevRelease = new DevRelease(TenantFactory.DefaultId, ProductFactory.DefaultId, SystemClock.Instance.GetCurrentInstant());
            var details = new ReleaseDetails(
                WebFormAppType.Quote,
                "{ \"configuration\": \"json\" }",
                "{ \"workflow\": \"json\" }",
                "{ \"exports\": \"json\" }",
                "{ \"automations\": \"json\" }",
                "{ \"paymentForm\": \"json\" }",
                "{ \"payment\": \"json\" }",
                "{ \"funding\": \"json\" }",
                "{ \"product\": \"json\" }",
                Enumerable.Empty<Asset>(),
                Enumerable.Empty<Asset>(),
                fileInBytes,
                SystemClock.Instance.GetCurrentInstant());
            var latestDevRelease = new DevRelease(TenantFactory.DefaultId, ProductFactory.DefaultId, SystemClock.Instance.GetCurrentInstant());

            this.devReleaseRepository.Setup(drr => drr.GetDevReleaseForProduct(TenantFactory.DefaultId, ProductFactory.DefaultId))
                .Returns(latestDevRelease);

            var folderName = "UBindStorage";
            var targetPaths = new List<string>
            {
                $@"{folderName}\{DeploymentEnvironment.Development}\{tenant.Details.Alias}\{product.Details.Alias}\{0}\assets",
                $@"{folderName}\{DeploymentEnvironment.Development}\{tenant.Details.Alias}\{product.Details.Alias}\{0}\files",
                $@"{folderName}\{DeploymentEnvironment.Development}\{tenant.Details.Alias}\{product.Details.Alias}\{0}",
            };

            this.fileRepository
                .Setup(hs => hs.GetFilesHashStringInFolders(It.IsAny<List<string>>(), authenticationToken.BearerToken, default))
                .Returns(Task.FromResult(devHashString));
            this.filesystemStorageConfiguration.Setup(f => f.UBindFolderName).Returns(folderName);

            var releaseService = new ReleaseService(
                new Mock<IInternalUrlConfiguration>().Object,
                this.releaseRepository.Object,
                this.devReleaseRepository.Object,
                this.productRepository.Object,
                this.productFeatureSettingRepository.Object,
                this.fileRepository.Object,
                new FilesystemStoragePathService(this.filesystemStorageConfiguration.Object),
                this.devConfigurationReader.Object,
                this.jobClient.Object,
                this.spreadsheetPoolService.Object,
                this.releaseValidator.Object,
                this.globalReleaseCache.Object,
                this.cachingResolver.Object,
                this.clock.Object,
                new Mock<IAutomationPeriodicTriggerScheduler>().Object,
                this.fileContentRepository.Object,
                NullLogger<ReleaseService>.Instance);

            this.productFeatureSettingRepository
                .Setup(p => p.GetDeployedProductFeatureSettings(
                    TenantFactory.DefaultId, It.IsAny<DeploymentEnvironment>()))
                .Returns(new List<ProductFeatureSetting>());

            this.cachingResolver
                .Setup(c => c.GetProductAliasOrThrowAsync(TenantFactory.DefaultId, ProductFactory.DefaultId))
                .Returns(Task.FromResult(product.Details.Alias));
            this.cachingResolver
                .Setup(c => c.GetProductOrThrow(TenantFactory.DefaultId, ProductFactory.DefaultId))
                .Returns(Task.FromResult(product));

            this.cachingResolver
                .Setup(c => c.GetTenantAliasOrThrowAsync(TenantFactory.DefaultId))
                .Returns(Task.FromResult(tenant.Details.Alias));
            this.cachingResolver
                .Setup(c => c.GetTenantOrThrow(TenantFactory.DefaultId))
                .Returns(Task.FromResult(tenant));

            this.fileRepository
                .Setup(f => f.FolderExists(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>()))
                .Returns(Task.FromResult(true));

            var workbookName = $"{tenant.Details.Alias}-{product.Details.Alias}-Workbook.xlsx";
            var quoteAssetsFolder =
                $@"{folderName}\{DeploymentEnvironment.Development}\{tenant.Details.Alias}\{product.Details.Alias}\Quote\{workbookName}";
            var claimAssetsFolder =
                $@"{folderName}\{DeploymentEnvironment.Development}\{tenant.Details.Alias}\{product.Details.Alias}\Claim\{workbookName}";

            this.fileRepository
                .Setup(gc => gc.GetFileContents(quoteAssetsFolder, It.IsAny<string>(), default))
                .Returns(Task.FromResult(fileInBytes));

            this.fileRepository
                .Setup(gc => gc.GetFileContents(claimAssetsFolder, It.IsAny<string>(), default))
                .Returns(Task.FromResult(fileInBytes));

            IEnumerable<string> filesInFolder = new List<string> { "File1.txt" };
            this.fileRepository
               .Setup(f => f.ListFilesInFolder(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>()))
               .Returns(Task.FromResult(filesInFolder));

            // File not found in DB
            this.fileContentRepository
                .Setup(f => f.GetFileContentIdOrNullForHashCode(It.IsAny<Guid>(), It.IsAny<string>())).Returns((Guid?)null);

            // Act
            var devRelease = await releaseService.CreateDevRelease(tenant.Id, product.Id);

            // Assert

            // The first workbook contents is unique so will be inserted
            devRelease.QuoteDetails.Assets.First().FileContent.Should().NotBeNull();

            // Since the claim workbook uses the same bytes it's hashcode will be identical so it won't be inserted
            devRelease.ClaimDetails.Files.First().FileContent.Should().BeNull();
        }

        [Fact]
        public async void CreateDevRelease_DoesNotInsertExistingFileContent_WhenSynchronised()
        {
            // Arrange
            var tenant = new Tenant(
                TenantFactory.DefaultId, "Leon", "leon", null, default, default, SystemClock.Instance.Now());
            var product = new Product(
                TenantFactory.DefaultId, ProductFactory.DefaultId, "Dev", "dev", SystemClock.Instance.Now());

            var authenticationToken = new AuthenticationToken("bearerToken", SystemClock.Instance.GetCurrentInstant());
            this.authenticator.Setup(auth => auth.GetAuthenticationTokenAsync())
                .Returns(Task.FromResult(authenticationToken));

            var fileInBytes = Fakes.Workbook.Standard;
            var workbook = FileContent.CreateFromBytes(tenant.Id, Guid.NewGuid(), fileInBytes);

            var devHashString = "Empty File".GetHashString();
            var oldDevRelease = new DevRelease(TenantFactory.DefaultId, ProductFactory.DefaultId, SystemClock.Instance.GetCurrentInstant());
            var details = new ReleaseDetails(
                WebFormAppType.Quote,
                "{ \"workflow\": \"json\" }",
                "{ \"exports\": \"json\" }",
                "{ \"automations\": \"json\" }",
                "{ \"outbound-email-servers\": \"json\" }",
                "{ \"payment\": \"json\" }",
                "{ \"funding\": \"json\" }",
                "{ \"product\": \"json\" }",
                Enumerable.Empty<Asset>(),
                Enumerable.Empty<Asset>(),
                fileInBytes,
                SystemClock.Instance.GetCurrentInstant());
            var latestDevRelease = new DevRelease(TenantFactory.DefaultId, ProductFactory.DefaultId, SystemClock.Instance.GetCurrentInstant());
            latestDevRelease.OnInitialized(details, SystemClock.Instance.GetCurrentInstant());

            this.devReleaseRepository.Setup(drr => drr.GetDevReleaseForProduct(TenantFactory.DefaultId, ProductFactory.DefaultId))
                .Returns(latestDevRelease);

            var folderName = "UBindStorage";
            var targetPaths = new List<string>
            {
                $@"{folderName}\{DeploymentEnvironment.Development}\{tenant.Details.Alias}\{product.Details.Alias}\{0}\assets",
                $@"{folderName}\{DeploymentEnvironment.Development}\{tenant.Details.Alias}\{product.Details.Alias}\{0}\files",
                $@"{folderName}\{DeploymentEnvironment.Development}\{tenant.Details.Alias}\{product.Details.Alias}\{0}",
            };

            this.fileRepository
                .Setup(hs => hs.GetFilesHashStringInFolders(It.IsAny<List<string>>(), authenticationToken.BearerToken, default))
                .Returns(Task.FromResult(devHashString));
            this.filesystemStorageConfiguration.Setup(f => f.UBindFolderName).Returns(folderName);

            var releaseService = new ReleaseService(
                new Mock<IInternalUrlConfiguration>().Object,
                this.releaseRepository.Object,
                this.devReleaseRepository.Object,
                this.productRepository.Object,
                this.productFeatureSettingRepository.Object,
                this.fileRepository.Object,
                new FilesystemStoragePathService(this.filesystemStorageConfiguration.Object),
                this.devConfigurationReader.Object,
                this.jobClient.Object,
                this.spreadsheetPoolService.Object,
                this.releaseValidator.Object,
                this.globalReleaseCache.Object,
                this.cachingResolver.Object,
                this.clock.Object,
                new Mock<IAutomationPeriodicTriggerScheduler>().Object,
                this.fileContentRepository.Object,
                NullLogger<ReleaseService>.Instance);

            this.productFeatureSettingRepository
                .Setup(p => p.GetDeployedProductFeatureSettings(
                    TenantFactory.DefaultId, It.IsAny<DeploymentEnvironment>()))
                .Returns(new List<ProductFeatureSetting>());

            this.cachingResolver
                .Setup(c => c.GetProductAliasOrThrowAsync(TenantFactory.DefaultId, ProductFactory.DefaultId))
                .Returns(Task.FromResult(product.Details.Alias));
            this.cachingResolver
                .Setup(c => c.GetProductOrThrow(TenantFactory.DefaultId, ProductFactory.DefaultId))
                .Returns(Task.FromResult(product));

            this.cachingResolver
                .Setup(c => c.GetTenantAliasOrThrowAsync(TenantFactory.DefaultId))
                .Returns(Task.FromResult(tenant.Details.Alias));
            this.cachingResolver
                .Setup(c => c.GetTenantOrThrow(TenantFactory.DefaultId))
                .Returns(Task.FromResult(tenant));

            this.fileRepository
                .Setup(f => f.FolderExists(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>()))
                .Returns(Task.FromResult(true));

            var workbookName = $"{tenant.Details.Alias}-{product.Details.Alias}-Workbook.xlsx";
            var quoteAssetsFolder =
                $@"{folderName}\{DeploymentEnvironment.Development}\{tenant.Details.Alias}\{product.Details.Alias}\Quote\{workbookName}";
            var claimAssetsFolder =
                $@"{folderName}\{DeploymentEnvironment.Development}\{tenant.Details.Alias}\{product.Details.Alias}\Claim\{workbookName}";

            this.fileRepository
                .Setup(gc => gc.GetFileContents(quoteAssetsFolder, It.IsAny<string>(), default))
                .Returns(Task.FromResult(fileInBytes));

            this.fileRepository
                .Setup(gc => gc.GetFileContents(claimAssetsFolder, It.IsAny<string>(), default))
                .Returns(Task.FromResult(fileInBytes));

            IEnumerable<string> filesInFolder = new List<string> { "File1.txt" };
            this.fileRepository
                .Setup(f => f.ListFilesInFolder(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>()))
                .Returns(Task.FromResult(filesInFolder));

            // Return an existing file from DB with different Id
            var existingFileInDb = FileContent.CreateFromBytes(tenant.Id, Guid.NewGuid(), fileInBytes);

            // Existing file found in DB
            this.fileContentRepository
                .Setup(f => f.GetFileContentIdOrNullForHashCode(It.IsAny<Guid>(), It.IsAny<string>())).Returns(existingFileInDb.Id);

            // Act
            var devRelease = await releaseService.CreateDevRelease(tenant.Id, product.Id);

            // Assert
            // Null filecontent won't insert anything in the DB
            devRelease.QuoteDetails.Assets.First().FileContent.Should().BeNull();
            devRelease.ClaimDetails.Files.First().FileContent.Should().BeNull();
        }
        */
    }
}
