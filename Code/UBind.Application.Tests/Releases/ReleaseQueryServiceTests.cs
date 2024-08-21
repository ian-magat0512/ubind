// <copyright file="ReleaseQueryServiceTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Releases
{
    using System;
    using System.Collections.Generic;
    using FluentAssertions;
    using Moq;
    using UBind.Application.Releases;
    using UBind.Application.Tests.Automations.Fakes;
    using UBind.Domain;
    using UBind.Domain.Extensions;
    using UBind.Domain.Product;
    using UBind.Domain.Product.Component;
    using UBind.Domain.Services;
    using UBind.Domain.Tests.Fakes;
    using Xunit;

    public class ReleaseQueryServiceTests
    {
        private readonly Mock<IGlobalReleaseCache> globalReleaseCache = new Mock<IGlobalReleaseCache>();
        private Mock<IProductReleaseService> productReleaseService = new Mock<IProductReleaseService>();
        private Mock<IFieldSerializationBinder> fieldSerialisationBinder = new Mock<IFieldSerializationBinder>();
        private Mock<IProductFeatureSettingRepository> productFeatureSettingRepository = new Mock<IProductFeatureSettingRepository>();
        private Mock<ICachingResolver> cachingResolver = new Mock<ICachingResolver>();
        private Mock<IReleaseRepository> mockReleaseRepository = new Mock<IReleaseRepository>();

        [Fact]
        public void GetRelease_ReturnsReleaseFromGlobalCache_WhenReleaseExists()
        {
            // Arrange
            var fakeRelease = FakeReleaseBuilder.CreateForProduct(TenantFactory.DefaultId, ProductFactory.DefaultId).BuildRelease();
            var releaseContext = new ReleaseContext(
                TenantFactory.DefaultId,
                ProductFactory.DefaultId,
                DeploymentEnvironment.Staging,
                fakeRelease.Id);
            var cachedRelease = new ActiveDeployedRelease(fakeRelease, DeploymentEnvironment.Staging, null);
            this.globalReleaseCache
                .Setup(c => c.GetRelease(
                    releaseContext,
                    this.productReleaseService.Object))
                .Returns(cachedRelease);
            var sut = new ReleaseQueryService(
                this.globalReleaseCache.Object,
                this.fieldSerialisationBinder.Object,
                this.cachingResolver.Object,
                this.productFeatureSettingRepository.Object,
                this.productReleaseService.Object,
                this.mockReleaseRepository.Object);

            // Act
            var release = sut.GetRelease(releaseContext);

            // Assert
            release.Should().NotBeNull();
            release.ReleaseId.Should().Be(fakeRelease.Id);
        }

        [Fact]
        public void GetRelease_DoesNotUseGlobalCache_WhenReleaseAlreadyLocallyCached()
        {
            // Arrange
            var release = FakeReleaseBuilder.CreateForProduct(TenantFactory.DefaultId, ProductFactory.DefaultId).BuildRelease();
            var releaseContext = new ReleaseContext(
                TenantFactory.DefaultId,
                ProductFactory.DefaultId,
                DeploymentEnvironment.Staging,
                release.Id);
            var cachedRelease = new ActiveDeployedRelease(release, DeploymentEnvironment.Staging, null);
            this.globalReleaseCache
                .Setup(c => c.GetRelease(
                    releaseContext,
                    this.productReleaseService.Object))
                .Returns(cachedRelease);
            var sut = new ReleaseQueryService(
                this.globalReleaseCache.Object,
                this.fieldSerialisationBinder.Object,
                this.cachingResolver.Object,
                this.productFeatureSettingRepository.Object,
                this.productReleaseService.Object,
                this.mockReleaseRepository.Object);
            var maybeRelease = sut.GetRelease(releaseContext);

            // Act
            var secondRelease = sut.GetRelease(releaseContext);

            // Assert
            secondRelease.Should().NotBeNull();
            secondRelease.ReleaseId.Should().Be(release.Id);
            this.globalReleaseCache.Verify(
                c => c.GetRelease(
                    releaseContext,
                    this.productReleaseService.Object),
                Times.Once);
        }

        [Fact]
        public void GetCurrentReleaseOrThrow_ReturnsCorrectRelease_WhenMultipleReleasesAreCached()
        {
            // Arrange
            var tenantIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
            var productIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
            var environments = new List<DeploymentEnvironment>
            {
                DeploymentEnvironment.Development,
                DeploymentEnvironment.Staging,
                DeploymentEnvironment.Production,
            };

            var releases = this.SetupReleasesForEachProductEnvironmentCombination(
                tenantIds, productIds, environments);

            var sut = new ReleaseQueryService(
                this.globalReleaseCache.Object,
                this.fieldSerialisationBinder.Object,
                this.cachingResolver.Object,
                this.productFeatureSettingRepository.Object,
                this.productReleaseService.Object,
                this.mockReleaseRepository.Object);

            foreach (var tenantId in tenantIds)
            {
                foreach (var productId in productIds)
                {
                    foreach (var environment in environments)
                    {
                        var releaseId = releases[tenantId][productId][environment].ReleaseId;
                        var releaseContext = new ReleaseContext(tenantId, productId, environment, releaseId);

                        // Act

                        // Fetch uncached
                        var firstRelease = sut.GetRelease(releaseContext);

                        // Fetch cached
                        var secondRelease = sut.GetRelease(releaseContext);

                        // Assert

                        // Verify that the correct release is returned.
                        firstRelease.ReleaseId.Should().Be(releaseId);
                        secondRelease.ReleaseId.Should().Be(firstRelease.ReleaseId);

                        // Verify that the global release cache is only hit once for each context.
                        this.globalReleaseCache.Verify(
                            c => c.GetRelease(
                                releaseContext,
                                this.productReleaseService.Object),
                            Times.Once);
                    }
                }
            }
        }

        private Dictionary<Guid, Dictionary<Guid, Dictionary<DeploymentEnvironment, ActiveDeployedRelease>>> SetupReleasesForEachProductEnvironmentCombination(
            IEnumerable<Guid> tenantIds,
            IEnumerable<Guid> productIds,
            IEnumerable<DeploymentEnvironment> environments)
        {
            var releases = new Dictionary<Guid, Dictionary<Guid, Dictionary<DeploymentEnvironment, ActiveDeployedRelease>>>();
            foreach (var tenantId in tenantIds)
            {
                releases[tenantId] = new Dictionary<Guid, Dictionary<DeploymentEnvironment, ActiveDeployedRelease>>();
                foreach (var productId in productIds)
                {
                    releases[tenantId][productId] = new Dictionary<DeploymentEnvironment, ActiveDeployedRelease>();
                    foreach (var environment in environments)
                    {
                        var release = FakeReleaseBuilder.CreateForProduct(TenantFactory.DefaultId, ProductFactory.DefaultId).BuildRelease();
                        var releaseContext = new ReleaseContext(tenantId, productId, environment, release.Id);
                        var cachedRelease = new ActiveDeployedRelease(release, environment, null);
                        releases[tenantId][productId][environment] = cachedRelease;
                        this.globalReleaseCache
                            .Setup(c => c.GetRelease(
                                releaseContext,
                                this.productReleaseService.Object))
                            .Returns(cachedRelease);
                    }
                }
            }

            return releases;
        }
    }
}
