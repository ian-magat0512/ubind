// <copyright file="ProductDeploymentSettingsIntegrationTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Tests
{
    using System;
    using System.Threading.Tasks;
    using FluentAssertions;
    using NodaTime;
    using UBind.Application.Tests;
    using UBind.Domain;
    using UBind.Domain.Product;
    using UBind.Persistence.Tests.Fakes;
    using Xunit;

    /// <summary>
    /// tests product deployment settings.
    /// </summary>
    [Collection(DatabaseCollection.Name)]
    public class ProductDeploymentSettingsIntegrationTests
    {
        private ProductDeploymentSetting deploymentSettings;

        public ProductDeploymentSettingsIntegrationTests()
        {
            this.deploymentSettings = new ProductDeploymentSetting()
            {
                DevelopmentIsActive = true,
                Development = new System.Collections.Generic.List<string>
                {
                    "*.ubind.io",
                    "*.ubind.com.au",
                    "localhost:*",
                    "bs-local.com",
                },
                StagingIsActive = true,
                Staging = new System.Collections.Generic.List<string>
                {
                },
                ProductionIsActive = true,
                Production = new System.Collections.Generic.List<string>
                {
                },
            };
        }

        /// <summary>
        /// test UpdateDeploymentSettings function expect it to success if deployment settings change.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task UpdateDeploymentSettings_Success_IfDeploymentSettingsChange()
        {
            // Arrange
            var productAlias = Guid.NewGuid().ToString();
            var originalName = "My test product";
            var tenant = new Tenant(Guid.NewGuid(), "foofoo", productAlias, null, default, default, SystemClock.Instance.GetCurrentInstant());
            var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName);
            stack.TenantRepository.Insert(tenant);
            stack.TenantRepository.SaveChanges();

            // Act

            // Request 1
            stack.MockMediator.GetTenantByIdOrAliasQuery(tenant);
            var service = stack.ProductService;
            var product = await service.CreateProduct(tenant.Id, productAlias, originalName);
            var productId = product.Id;

            // Request 2
            service = new ApplicationStack(DatabaseFixture.TestConnectionStringName).ProductService;
            service.UpdateDeploymentSettings(tenant.Id, product.Id, this.deploymentSettings);

            // Request 3
            var repo = new ApplicationStack(DatabaseFixture.TestConnectionStringName).ProductRepository;
            product = repo.GetProductById(tenant.Id, productId);

            var storedDeploymentSettings = product.Details.DeploymentSetting;

            // Assert
            productAlias.Should().Be(product.Details.Alias);
            this.deploymentSettings.ProductionIsActive.Should().Be(storedDeploymentSettings.ProductionIsActive);
            this.deploymentSettings.StagingIsActive.Should().Be(storedDeploymentSettings.StagingIsActive);
            this.deploymentSettings.DevelopmentIsActive.Should().Be(storedDeploymentSettings.DevelopmentIsActive);
            this.deploymentSettings.Production.Count.Should().Be(storedDeploymentSettings.Production.Count);
            this.deploymentSettings.Staging.Count.Should().Be(storedDeploymentSettings.Staging.Count);
            this.deploymentSettings.Development.Count.Should().Be(storedDeploymentSettings.Development.Count);
        }

        /// <summary>
        /// test UpdateDeploymentSettings function expect it to success if deployment settings change and try to override.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task UpdateDeploymentSettings_Success_IfDeploymentSettingsChangeAndTryToOverride()
        {
            // Arrange
            var productAlias = Guid.NewGuid().ToString();
            var tenantId = Guid.NewGuid();
            var originalName = "My test product";
            var tenant = new Tenant(tenantId, tenantId.ToString(), tenantId.ToString(), null, default, default, SystemClock.Instance.GetCurrentInstant());

            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                var tenantRepo = stack.TenantRepository;
                tenantRepo.Insert(tenant);
                tenantRepo.SaveChanges();
            }

            // Act

            // Request 1
            Product product = null;
            Guid productId;
            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                stack.MockMediator.GetTenantByIdOrAliasQuery(tenant);
                product = await stack.ProductService.CreateProduct(tenant.Id, productAlias, originalName);
                productId = product.Id;
            }

            // Request 2
            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                stack.Clock.Increment(Duration.FromSeconds(1)); // needed to have higher created time to pass this unit test.
                product = stack.ProductService.UpdateDeploymentSettings(tenant.Id, product.Id, this.deploymentSettings);
            }

            // Request 3
            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                product = stack.ProductRepository.GetProductById(tenant.Id, productId);
            }

            // Request 4
            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                stack.Clock.Increment(Duration.FromSeconds(5)); // needed to have higher created time to pass this unit test.
                product = await stack.ProductService.UpdateProduct(tenant.Id, product.Id, originalName, productAlias, false, false, CancellationToken.None);
            }

            // Request 5
            // use for intermitent failing tests
            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                product = stack.ProductRepository.GetProductById(tenant.Id, productId);

                ProductDeploymentSetting storedDeploymentSettings = product?.Details?.DeploymentSetting;

                // re query if null.
                if (storedDeploymentSettings == null)
                {
                    product = stack.ProductRepository.GetProductById(tenant.Id, productId);
                    storedDeploymentSettings = product?.Details?.DeploymentSetting;
                }

                // Assert
                product.DetailsCollection.Count.Should().Be(4);
                this.deploymentSettings.ProductionIsActive.Should().Be(storedDeploymentSettings.ProductionIsActive);
                this.deploymentSettings.StagingIsActive.Should().Be(storedDeploymentSettings.StagingIsActive);
                this.deploymentSettings.DevelopmentIsActive.Should().Be(storedDeploymentSettings.DevelopmentIsActive);
                this.deploymentSettings.Production.Count.Should().Be(storedDeploymentSettings.Production.Count);
                this.deploymentSettings.Staging.Count.Should().Be(storedDeploymentSettings.Staging.Count);
                this.deploymentSettings.Development.Count.Should().Be(storedDeploymentSettings.Development.Count);
            }
        }

        /// <summary>
        /// test GetDeploymentSettings function expect it to Success by simple retrieval.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetDeploymentSettings_Success_SimpleRetrieval()
        {
            // Arrange
            var productAlias = "productAlias" + Guid.NewGuid();
            var tenantId = Guid.NewGuid();
            var originalName = "My test product";
            var tenant = new Tenant(tenantId, "foofoo", tenantId.ToString(), null, default, default, SystemClock.Instance.GetCurrentInstant());
            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                stack.TenantRepository.Insert(tenant);
                stack.TenantRepository.SaveChanges();
            }

            // Act

            // Request 1
            Product product = null;
            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                stack.MockMediator.GetTenantByIdOrAliasQuery(tenant);
                product = await stack.ProductService.CreateProduct(tenant.Id, productAlias, originalName);
            }

            // Request 2
            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                stack.Clock.Increment(Duration.FromSeconds(1)); // needed to have higher created time to pass this unit test.
                product = stack.ProductService.UpdateDeploymentSettings(tenant.Id, product.Id, this.deploymentSettings);
            }

            // Request 3
            // use for intermitent failing tests
            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                stack.MockMediator.GetTenantByIdOrAliasQuery(tenant);
                var savedDeploymentSettings = stack.ProductService.GetDeploymentSettings(tenant.Id, product.Id);

                // re query if null.
                if (savedDeploymentSettings == null)
                {
                    savedDeploymentSettings = stack.ProductService.GetDeploymentSettings(tenant.Id, product.Id);
                }

                // Assert
                var stringExpected = string.Join("---", this.deploymentSettings.Development);
                var stringActual = string.Join("---", savedDeploymentSettings.Development);
                stringExpected.Should().Be(stringActual);
                productAlias.Should().Be(product.Details.Alias);
                this.deploymentSettings.ProductionIsActive.Should().Be(savedDeploymentSettings.ProductionIsActive);
                this.deploymentSettings.StagingIsActive.Should().Be(savedDeploymentSettings.StagingIsActive);
                this.deploymentSettings.DevelopmentIsActive.Should().Be(savedDeploymentSettings.DevelopmentIsActive);
                this.deploymentSettings.Production.Count.Should().Be(savedDeploymentSettings.Production.Count);
                this.deploymentSettings.Staging.Count.Should().Be(savedDeploymentSettings.Staging.Count);
                this.deploymentSettings.Development.Count.Should().Be(savedDeploymentSettings.Development.Count);
            }
        }
    }
}
