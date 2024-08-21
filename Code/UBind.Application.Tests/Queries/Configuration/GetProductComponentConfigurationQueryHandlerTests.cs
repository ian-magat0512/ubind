// <copyright file="GetProductComponentConfigurationQueryHandlerTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace UBind.Application.Tests.Queries.ProductConfiguration
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Moq;
    using NodaTime;
    using UBind.Application.Queries.ProductConfiguration;
    using UBind.Application.Releases;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Product;
    using UBind.Domain.Product.Component;
    using UBind.Domain.Repositories;
    using UBind.Domain.Tests.Fakes;
    using Xunit;

    public class GetProductComponentConfigurationQueryHandlerTests
    {
        private IClock clock = new SequentialClock();
        private Mock<IReleaseQueryService> releaseQueryService;
        private Mock<ITenantRepository> tenantRepository;
        private Mock<IProductRepository> productRepository;
        private Mock<IConfigurationService> configurationService;
        private Guid tenantId = Guid.NewGuid();
        private Guid productId = Guid.NewGuid();
        private Mock<ICachingResolver> cachingResolver;
        private IFieldSerializationBinder fieldSerializationBinder;

        public GetProductComponentConfigurationQueryHandlerTests()
        {
            /// CAN THESE BE DELETED?
            // test it out
            /*
            var tenant = new Tenant(this.tenantId, "foo", this.tenantId.ToString(), null, default, default, SystemClock.Instance.GetCurrentInstant());
            var product = new Domain.Product.Product(this.tenantId, this.productId, "dummy", this.productId.ToString(), SystemClock.Instance.GetCurrentInstant());
            this.authenticator = new Mock<ICachingAuthenticationTokenProvider>();
            this.fileRepository = new Mock<IFilesystemFileRepository>();
            this.oneDriveFilePathService = new Mock<IFilesystemStoragePathService>();
            */
            ///////////////////////
            this.releaseQueryService = new Mock<IReleaseQueryService>();
            this.tenantRepository = new Mock<ITenantRepository>();
            this.productRepository = new Mock<IProductRepository>();
            this.configurationService = new Mock<IConfigurationService>();
            this.cachingResolver = new Mock<ICachingResolver>();
            this.fieldSerializationBinder = new FieldSerializationBinder();
        }

        [Fact]
        public async Task GetConfigurationAsync_DoesNotIncludePaymentFileItem_WhenNoFileExists()
        {
            // Arrange
            this.SetupTenant();
            this.SetupProduct();

            var devRelease = this.CreateDevReleaseWithConfig("{}");
            var cachedRelease = new ActiveDeployedRelease(devRelease, DeploymentEnvironment.Development, this.fieldSerializationBinder);
            var releaseContext = new ReleaseContext(
                this.tenantId,
                this.productId,
                DeploymentEnvironment.Development,
                cachedRelease.ReleaseId);
            this.releaseQueryService
                .Setup(c => c.GetRelease(releaseContext))
                .Returns(cachedRelease);

            // Initialize the system under test
            var sut = new GetProductComponentConfigurationQueryHandler(
                this.releaseQueryService.Object,
                this.configurationService.Object,
                this.tenantRepository.Object,
                this.productRepository.Object,
                this.cachingResolver.Object);

            var query = new GetProductComponentConfigurationQuery(releaseContext);

            // Act
            var config = await sut.Handle(query, CancellationToken.None);

            // Assert
            Assert.DoesNotContain("paymentForm", config.ConfigurationJson);
        }

        [Fact]
        public async Task GetConfigurationAsync_IncludesPaymentFileItem_WhenFileExists()
        {
            // Arrange
            this.SetupTenant();
            this.SetupProduct();
            var devRelease = this.CreateDevReleaseWithConfig("{ \"paymentFormJson\": \"json\" }");
            var cachedRelease = new ActiveDeployedRelease(devRelease, DeploymentEnvironment.Development, this.fieldSerializationBinder);
            var productContext = new ReleaseContext(
                this.tenantId,
                this.productId,
                DeploymentEnvironment.Development,
                cachedRelease.ReleaseId);
            this.releaseQueryService
                .Setup(c => c.GetRelease(It.IsAny<ReleaseContext>()))
                .Returns(cachedRelease);

            var query = new GetProductComponentConfigurationQuery(productContext);
            var sut = this.CreateSut();

            // Act
            var config = await sut.Handle(query, CancellationToken.None);

            // Assert
            Assert.Contains("paymentFormJson", config.ConfigurationJson);
        }

        [Fact]
        public async Task Handle_Should_ThrowException_When_TenantWasDisabled()
        {
            // Arrange
            this.SetupTenant(disabled: true);
            this.SetupProduct();
            var query = new GetProductComponentConfigurationQuery(this.GetReleaseContext());
            var sut = this.CreateSut();

            // Act
            Func<Task> act = async () => await sut.Handle(query, CancellationToken.None);

            // Assert
            (await act.Should().ThrowAsync<ErrorException>()).Which.Error.Code.Should().Be("tenant.disabled");
        }

        [Fact]
        public async Task Handle_Should_ThrowException_When_ProductWasDisabled()
        {
            // Arrange
            this.SetupTenant();
            this.SetupProduct(disabled: true);
            var query = new GetProductComponentConfigurationQuery(this.GetReleaseContext());
            var sut = this.CreateSut();

            // Act
            Func<Task> act = async () => await sut.Handle(query, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ErrorException>();
        }

        private GetProductComponentConfigurationQueryHandler CreateSut()
        {
            var sut = new GetProductComponentConfigurationQueryHandler(
            this.releaseQueryService.Object,
            this.configurationService.Object,
            this.tenantRepository.Object,
            this.productRepository.Object,
            this.cachingResolver.Object);
            return sut;
        }

        private void SetupTenant(bool disabled = false, bool deleted = false)
        {
            var tenant = new Tenant(
                this.tenantId,
                "foo",
                this.tenantId.ToString(),
                null,
                default,
                default,
                this.clock.Now());
            TenantDetails tenantDetail = new TenantDetails(
               this.tenantId.ToString(), this.tenantId.ToString(), null, disabled, deleted, default, default, this.clock.Now());
            tenant.Update(tenantDetail);
            this.tenantRepository
                .Setup(t => t.GetTenantById(this.tenantId))
                .Returns(tenant);
            this.tenantRepository
                .Setup(t => t.GetTenantById(this.tenantId))
                .Returns(tenant);

            this.cachingResolver.Setup(t => t.GetTenantOrThrow(It.IsAny<Guid>())).Returns(Task.FromResult(tenant));
        }

        private void SetupProduct(bool disabled = false, bool deleted = false)
        {
            Domain.Product.Product product = new Domain.Product.Product(
                this.tenantId,
                this.productId,
                "dummy product",
                this.productId.ToString(),
                this.clock.Now());
            product.Update(new ProductDetails("dummy product", product.Id.ToString(), disabled, deleted, this.clock.Now()));
            this.productRepository
                .Setup(pr => pr.GetProductById(this.tenantId, this.productId, false))
                .Returns(product);
            this.productRepository
               .Setup(pr => pr.GetProductById(this.tenantId, this.productId, false))
               .Returns(product);

            this.cachingResolver.Setup(e => e.GetProductOrThrow(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult(product));
        }

        private DevRelease CreateDevReleaseWithConfig(string config)
        {
            var devRelease = new DevRelease(this.tenantId, this.productId, this.clock.Now());
            var quoteReleaseDetails = new ReleaseDetails(
                WebFormAppType.Quote,
                $"{{ \"config\": {config} }}",
                "{ \"workflow\": \"json\" }",
                "{ \"exports\": \"json\" }",
                "{ \"automations\": \"json\" }",
                "{ \"outbound-email-servers\": \"json\" }",
                "{ \"payment\": \"json\" }",
                "{ \"funding\": \"json\" }",
                "{ \"product\": \"json\" }",
                Enumerable.Empty<Asset>(),
                Enumerable.Empty<Asset>(),
                new byte[] { 00, 00, 00, 00 },
                this.clock.Now());
            devRelease.QuoteDetails = quoteReleaseDetails;
            var claimReleaseDetails = new ReleaseDetails(
                WebFormAppType.Claim,
                "{ \"formConfiguration\": \"json\" }",
                "{ \"workflow\": \"json\" }",
                null,
                null,
                null,
                "{ \"payment\": \"json\" }",
                "{ \"funding\": \"json\" }",
                "{ \"product\": \"json\" }",
                Enumerable.Empty<Asset>(),
                Enumerable.Empty<Asset>(),
                new byte[] { 00, 00, 00, 00 },
                this.clock.Now());
            devRelease.ClaimDetails = claimReleaseDetails;

            return devRelease;
        }

        private ReleaseContext GetReleaseContext()
        {
            return new ReleaseContext(this.tenantId, this.productId, DeploymentEnvironment.Development, Guid.NewGuid());
        }

        // Helper methods like CreateDevReleaseWithConfig go here
    }
}
