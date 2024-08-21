// <copyright file="ProductFeatureSettingServiceTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Services
{
    using System;
    using System.Collections.Generic;
    using FluentAssertions;
    using Moq;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Authentication;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Services;
    using UBind.Domain.Tests.Fakes;
    using Xunit;

    /// <summary>
    /// Tests for Product feature Service.
    /// </summary>
    public class ProductFeatureSettingServiceTests
    {
        private Mock<IUserAuthenticationData> userAuthenticationData = new Mock<IUserAuthenticationData>();
        private Mock<IProductFeatureSettingRepository> productFeatureRepository = new Mock<IProductFeatureSettingRepository>();
        private Mock<IProductRepository> productRepository = new Mock<IProductRepository>();
        private Mock<IClock> clock = new Mock<IClock>();
        private Mock<ICachingResolver> cacheResolver = new Mock<ICachingResolver>();

        /// <summary>
        /// Test enable product feature should enable product feature.
        /// </summary>
        [Fact]
        public void EnableProductFeatures_ShouldEnable_ProductFeature()
        {
            // Arrange
            var productFeatureService = this.GetProductFeatureService();
            var productFeature = new ProductFeatureSetting(TenantFactory.DefaultId, ProductFactory.DefaultId, this.clock.Object.Now());
            this.cacheResolver
                .Setup(p => p.GetProductSettingOrThrow(TenantFactory.DefaultId, ProductFactory.DefaultId))
                .Returns(productFeature);

            // Act
            productFeatureService.EnableProductFeature(
                TenantFactory.DefaultId,
                ProductFactory.DefaultId,
                ProductFeatureSettingItem.AdjustmentQuotes);

            // Assert
            this.productFeatureRepository.Verify(e => e.EnableProductFeature(
                TenantFactory.DefaultId,
                ProductFactory.DefaultId,
                ProductFeatureSettingItem.AdjustmentQuotes));
        }

        /// <summary>
        /// Test enable product feature should throw ErrorException when product feature already enabled.
        /// </summary>
        [Fact]
        public void EnableProducFeature_WithEnabledProductFeature_ThrowsErrorException()
        {
            // Arrange
            var productFeatureService = this.GetProductFeatureService();
            var productFeature = new ProductFeatureSetting(TenantFactory.DefaultId, ProductFactory.DefaultId, this.clock.Object.Now());
            this.cacheResolver
                .Setup(p => p.GetProductSettingOrThrow(TenantFactory.DefaultId, ProductFactory.DefaultId))
                .Returns(productFeature);

            // Act
            Func<ProductFeatureSetting> act = () => productFeatureService.EnableProductFeature(
              TenantFactory.DefaultId,
              ProductFactory.DefaultId,
              ProductFeatureSettingItem.NewBusinessQuotes);

            // Assert
            act.Should().Throw<ErrorException>();
        }

        /// <summary>
        /// Test disable product feature should disable product feature.
        /// </summary>
        [Fact]
        public void DisableProducFeature_ShouldDisableProductFeature()
        {
            var productFeatureService = this.GetProductFeatureService();
            this.userAuthenticationData.Setup(t => t.TenantId).Returns(Tenant.MasterTenantId);
            var productFeature = new ProductFeatureSetting(TenantFactory.DefaultId, ProductFactory.DefaultId, this.clock.Object.Now());
            this.cacheResolver
                .Setup(p => p.GetProductSettingOrThrow(TenantFactory.DefaultId, ProductFactory.DefaultId))
                .Returns(productFeature);

            // Act
            productFeatureService.DisableProductFeature(
                TenantFactory.DefaultId,
                ProductFactory.DefaultId,
                ProductFeatureSettingItem.NewBusinessQuotes);

            // Assert
            this.productFeatureRepository.Verify(e => e.DisableProductFeature(
                TenantFactory.DefaultId,
                ProductFactory.DefaultId,
                ProductFeatureSettingItem.NewBusinessQuotes));
        }

        /// <summary>
        /// Test disable product feature with already disabled product feature should trow exception.
        /// </summary>
        [Fact]
        public void DisableProducFeatures_WithDisabledProductFeature_ThrowsErrorException()
        {
            var productFeatureService = this.GetProductFeatureService();
            this.userAuthenticationData.Setup(t => t.TenantId).Returns(Tenant.MasterTenantId);
            var productFeature = new ProductFeatureSetting(TenantFactory.DefaultId, ProductFactory.DefaultId, this.clock.Object.Now());
            this.cacheResolver
                .Setup(p => p.GetProductSettingOrThrow(TenantFactory.DefaultId, ProductFactory.DefaultId))
                .Returns(productFeature);

            // Act
            Func<ProductFeatureSetting> act = () => productFeatureService.DisableProductFeature(
              TenantFactory.DefaultId,
              ProductFactory.DefaultId,
              ProductFeatureSettingItem.AdjustmentQuotes);

            // Assert
            act.Should().Throw<ErrorException>();
        }

        private List<ProductFeatureSetting> GetProductFeatures()
        {
            List<ProductFeatureSetting> productFeatures = new List<ProductFeatureSetting>()
            {
                new ProductFeatureSetting(TenantFactory.DefaultId, ProductFactory.DefaultId, this.clock.Object.Now()),
            };
            return productFeatures;
        }

        private ProductFeatureSettingService GetProductFeatureService()
        {
            var tenant = TenantFactory.Create();
            var product = ProductFactory.Create();
            Mock<DevRelease> devRelease = new Mock<DevRelease>();

            var productSummary = new Mock<IProductSummary>();
            var deploymentSetting = new ProductDeploymentSetting();
            deploymentSetting.Development = new List<string> { DeploymentEnvironment.Development.ToString() };
            var productDetails = new ProductDetails("name", product.Details.Alias, false, false, this.clock.Object.Now(), deploymentSetting);
            productSummary.Setup(p => p.Details).Returns(productDetails);
            productSummary.Setup(p => p.Id).Returns(ProductFactory.DefaultId);
            IEnumerable<IProductSummary> productSummaries = new List<IProductSummary>() { productSummary.Object };

            this.productRepository.Setup(p => p.GetAllActiveProductSummariesForTenant(tenant.Id)).Returns(productSummaries);
            var productFeatureService = new ProductFeatureSettingService(
                this.productFeatureRepository.Object,
                this.clock.Object,
                this.cacheResolver.Object);
            return productFeatureService;
        }
    }
}
