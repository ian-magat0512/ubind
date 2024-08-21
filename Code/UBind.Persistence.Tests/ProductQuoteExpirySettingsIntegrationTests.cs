// <copyright file="ProductQuoteExpirySettingsIntegrationTests.cs" company="uBind">
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
    /// tests product quote expiry settings.
    /// </summary>
    [Collection(DatabaseCollection.Name)]
    public class ProductQuoteExpirySettingsIntegrationTests
    {
        /// <summary>
        /// test UpdateDeploymentSettings function expect it to success if deployment settings change.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task UpdateQuoteExpirySettings_Success_IfChange()
        {
            var tenantId = Guid.NewGuid();
            var productAlias = Guid.NewGuid().ToString();
            var originalName = "My test product";
            var tenant = new Tenant(tenantId, "foofoo", productAlias, null, default, default, SystemClock.Instance.GetCurrentInstant());
            var settings = new QuoteExpirySettings(20, true);

            // Arrange
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
                stack.Clock.Increment(Duration.FromSeconds(2)); // needed to have higher created time.
                stack.ProductService.UpdateQuoteExpirySettings(
                    tenant.Id,
                    product.Id,
                    settings,
                    ProductQuoteExpirySettingUpdateType.UpdateNone,
                    CancellationToken.None);
            }

            // Request 3
            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                product = stack.ProductRepository.GetProductById(tenant.Id, product.Id);

                var storedSettings = product.Details.QuoteExpirySetting;

                // Assert
                productAlias.Should().Be(product.Details.Alias);
                settings.Enabled.Should().Be(storedSettings.Enabled);
                settings.ExpiryDays.Should().Be(storedSettings.ExpiryDays);
            }
        }

        /// <summary>
        /// test UpdateDeploymentSettings function expect it to success if deployment settings change.
        /// </summary>
        [Fact]
        public void ProductQuoteExpirySetting_ArgumentException_IfExpiryDateIsLessThanZero()
        {
            var tenant = new Tenant(Guid.NewGuid(), "foofoo", "ff", null, default, default, SystemClock.Instance.GetCurrentInstant());
            QuoteExpirySettings settings = null;

            Assert.Throws<ArgumentException>(() =>
            {
                settings = new QuoteExpirySettings(-1, true);
            });
        }

        /// <summary>
        /// test UpdateQuoteExpirySettings function expect it to success if settings change and try to override.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task UpdateQuoteExpirySettings_Success_IfSettingsChangeAndTryToOverride()
        {
            var tenantId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var originalName = "My test product";
            var tenant = new Tenant(
                    tenantId,
                    "foofoo",
                    tenantId.ToString(),
                    null,
                    default,
                    default,
                    SystemClock.Instance.GetCurrentInstant());
            var settings = new QuoteExpirySettings(20, true);

            // Arrange
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
                product = await stack.ProductService.CreateProduct(tenant.Id, productId.ToString(), originalName, productId);
            }

            // Request 2
            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                stack.Clock.Increment(Duration.FromSeconds(2)); // needed to have created time greater than previous value.
                product = stack.ProductService.UpdateQuoteExpirySettings(
                    tenant.Id,
                    product.Id,
                    settings,
                    ProductQuoteExpirySettingUpdateType.UpdateNone,
                    CancellationToken.None);
            }

            // Request 3
            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                product = stack.ProductRepository.GetProductById(tenant.Id, product.Id);
            }

            // Request 4
            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                stack.Clock.Increment(Duration.FromSeconds(4)); // needed to have created time greater than previous value.
                product = await stack.ProductService.UpdateProduct(
                    tenant.Id,
                    product.Id,
                    originalName,
                    product.Details.Alias,
                    false,
                    false,
                    CancellationToken.None,
                    product.Details.QuoteExpirySetting);
            }

            // Request 5
            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                product = stack.ProductRepository.GetProductById(tenant.Id, product.Id);
            }

            var storedSettings = product.Details.QuoteExpirySetting;

            // Assert
            productId.Should().Be(product.Id);
            settings.Enabled.Should().Be(storedSettings.Enabled);
            settings.ExpiryDays.Should().Be(storedSettings.ExpiryDays);
        }

        /// <summary>
        /// test GetQuoteExpireSettings function expect it to Success by simple retrieval.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetProductById_Success_SimpleRetrieval()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var productName = "My test product";
            var tenant = new Tenant(tenantId, "foofoo", tenantId.ToString(), null, default, default, SystemClock.Instance.GetCurrentInstant());
            var settings = new QuoteExpirySettings(20, true);
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
                product = await stack.ProductService.CreateProduct(tenant.Id, Guid.NewGuid().ToString(), productName);
            }

            // Request 2
            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                stack.Clock.Increment(Duration.FromSeconds(2)); // needed to have created time greater than previous value.
                product = stack.ProductService.UpdateQuoteExpirySettings(
                    tenant.Id,
                    product.Id,
                    settings,
                    ProductQuoteExpirySettingUpdateType.UpdateNone,
                    CancellationToken.None);
            }

            // Request 3
            QuoteExpirySettings storedSettings = null;
            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                storedSettings = stack.ProductRepository.GetProductById(tenant.Id, product.Id).Details.QuoteExpirySetting;
            }

            // Assert
            settings.Enabled.Should().Be(storedSettings.Enabled);
            settings.ExpiryDays.Should().Be(storedSettings.ExpiryDays);
        }
    }
}
