// <copyright file="ProductIntegrationTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Tests
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.Extensions.Logging.Abstractions;
    using Moq;
    using NodaTime;
    using UBind.Application;
    using UBind.Application.Automation;
    using UBind.Application.MicrosoftGraph;
    using UBind.Application.Queries.Services;
    using UBind.Application.Services;
    using UBind.Application.Tests;
    using UBind.Domain;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Processing;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel.Product;
    using UBind.Domain.ReferenceNumbers;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services;
    using UBind.Domain.Tests.Fakes;
    using UBind.Persistence.Tests.Fakes;
    using Xunit;

    [Collection(DatabaseCollection.Name)]
    public class ProductIntegrationTests
    {
        public Mock<ICqrsMediator> MockMediator { get; private set; }

        [Fact(Skip = "Intermittently failing. To be fixed in UB-10266")]
        public async Task ProductsCanBeInsertedAndUpdated()
        {
            using (var appStack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                // Arrange
                var productAlias = "foo";
                var originalName = "My test product";
                var updatedName = "Foo product";
                var expirySettings = new QuoteExpirySettings(30, true);

                var tenant = TenantFactory.Create(Guid.NewGuid(), "pit-tenant-1");
                var tenantRepo = this.CreateTenantRepository();
                tenantRepo.Insert(tenant);
                tenantRepo.SaveChanges();

                // Act

                // Request 1
                TestClock testClock = new TestClock();
                testClock.Increment(Duration.FromMinutes(1));
                var productService = this.CreateService(testClock);
                this.MockMediator.GetTenantByIdOrAliasQuery(tenant);
                var product = await productService.CreateProduct(tenant.Id, productAlias, originalName);

                // ensure there is a difference in timestamps
                await Task.Delay(1);

                // Request 2
                testClock = new TestClock();
                testClock.Increment(Duration.FromMinutes(2));
                productService = this.CreateService(testClock);
                await productService.UpdateProduct(tenant.Id, product.Id, updatedName, "alias", false, false, CancellationToken.None, expirySettings);

                // Request 3
                var repo = this.CreateProductRepository();
                var updatedProduct = repo.GetProductById(tenant.Id, product.Id);

                // Assert
                productAlias.Should().NotBe(updatedProduct.Details.Alias);
                updatedName.Should().Be(updatedProduct.Details.Name);
                expirySettings.ExpiryDays.Should().Be(updatedProduct.Details.QuoteExpirySetting.ExpiryDays);
                expirySettings.Enabled.Should().Be(updatedProduct.Details.QuoteExpirySetting.Enabled);
                updatedProduct.History.Count().Should().Be(3);
            }
        }

        [Fact]
        public async Task IProductRepositoryGetProducts_IncludesProductDetails()
        {
            using (var appStack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                // Arrange
                var productId = Guid.NewGuid();
                var tenantId = Guid.NewGuid();
                var originalName = "My test product2";
                var tenant = new Tenant(
                    tenantId, "foofoo2", "pit-tenant-2", null, default, default, SystemClock.Instance.GetCurrentInstant());
                var tenantRepo = this.CreateTenantRepository();
                tenantRepo.Insert(tenant);
                tenantRepo.SaveChanges();
                var updatedName = "Foo product2";

                // Act

                // Request 1
                TestClock testClock = new TestClock();
                testClock.Increment(Duration.FromSeconds(1));
                var service = this.CreateService(testClock);
                this.MockMediator.GetTenantByIdOrAliasQuery(tenant);
                var product = await service.CreateProduct(tenant.Id, productId.ToString(), originalName, productId);

                // Request 2
                testClock.Increment(Duration.FromSeconds(2));
                service = this.CreateService(testClock);
                await service.UpdateProduct(tenant.Id, product.Id, updatedName, "alias", false, false, CancellationToken.None);

                // Request 3
                var repo = this.CreateProductRepository();
                var filters = new ProductReadModelFilters();
                filters.TenantId = tenantId;
                filters.ProductId = productId;
                var productSummaries = repo.GetProductSummaries(tenantId, filters).First();

                // Assert
                productSummaries.History.Count().Should().Be(3);
            }
        }

        [Fact]
        public void TestGetProductById()
        {
            using (var appStack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                // Arrange
                var tenantId = Guid.NewGuid();
                var productId = Guid.NewGuid();
                var productName = "foo foo product";
                var tenantRepo = this.CreateTenantRepository();
                var tenant = TenantFactory.Create(tenantId, "pit-tenant-3");
                tenantRepo.Insert(tenant);
                tenantRepo.SaveChanges();

                this.CreateProduct(tenant, productId, productName);

                // Act
                var service = this.CreateService();
                this.MockMediator.GetTenantByIdOrAliasQuery(tenant);
                var product = service.GetProductById(tenantId, productId);

                // Assert
                product.Should().NotBeNull();
            }
        }

        private IProductRepository CreateProductRepository()
        {
            var context = new UBindDbContext(DatabaseFixture.TestConnectionString);
            var repo = new ProductRepository(context);
            return repo;
        }

        private ITenantRepository CreateTenantRepository()
        {
            var context = new UBindDbContext(DatabaseFixture.TestConnectionString);
            var repo = new TenantRepository(context);
            return repo;
        }

        private UBind.Application.IProductService CreateService(IClock clock = null)
        {
            if (clock == null)
            {
                clock = new TestClock();
            }

            var context = new UBindDbContext(DatabaseFixture.TestConnectionString);
            var productRepo = new ProductRepository(context);
            var tenantRepo = new TenantRepository(context);
            var featureSettingsRepo = new FeatureSettingRepository(context, clock);
            var productSettingsRepo = new ProductFeatureSettingRepository(context);
            this.MockMediator = new Mock<ICqrsMediator>();
            var cachingResolver = new CachingResolver(
                this.MockMediator.Object, tenantRepo, productRepo, featureSettingsRepo, productSettingsRepo);
            var portalRepo = new PortalRepository(context, cachingResolver);
            var authenticator = new Mock<ICachingAuthenticationTokenProvider>();
            var graphClient = new Mock<IFilesystemFileRepository>();
            var pathService = new Mock<IFilesystemStoragePathService>();
            var jobClient = new Mock<IJobClient>();
            var invoiceNumberRepository = new Mock<IInvoiceNumberRepository>();
            var policyNumberRepository = new Mock<IPolicyNumberRepository>();
            var productFeatureService = new Mock<IProductFeatureSettingService>();
            var claimNumberRepository = new Mock<IClaimNumberRepository>();
            var creditNoteNumberRepository = new Mock<ICreditNoteNumberRepository>();
            var quoteService = new Mock<IQuoteService>();
            this.MockMediator = new Mock<ICqrsMediator>();
            var logger = NullLogger<UBind.Application.ProductService>.Instance;
            var httpContextPropertiesResolver = new Mock<IHttpContextPropertiesResolver>();
            var service = new UBind.Application.ProductService(
                tenantRepo,
                productRepo,
                authenticator.Object,
                graphClient.Object,
                pathService.Object,
                jobClient.Object,
                clock,
                invoiceNumberRepository.Object,
                claimNumberRepository.Object,
                policyNumberRepository.Object,
                creditNoteNumberRepository.Object,
                quoteService.Object,
                productFeatureService.Object,
                new Mock<IAutomationPeriodicTriggerScheduler>().Object,
                logger,
                this.MockMediator.Object,
                httpContextPropertiesResolver.Object,
                new Mock<IProductPortalSettingRepository>().Object,
                cachingResolver);

            return service;
        }

        private void CreateProduct(Tenant tenant, Guid productId, string productName)
        {
            var service = this.CreateService();
            this.MockMediator.GetTenantByIdOrAliasQuery(tenant);
            var product = service.GetProductById(tenant.Id, productId);
            if (product == null)
            {
                service.CreateProduct(tenant.Id, productId.ToString(), productName, productId);
            }
        }
    }
}
