// <copyright file="GlobalReleaseCacheTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Releases
{
    using System;
    using System.Collections.Generic;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using UBind.Application.FlexCel;
    using UBind.Application.Releases;
    using UBind.Application.Tests.Automations.Fakes;
    using UBind.Domain;
    using UBind.Domain.Extensions;
    using UBind.Domain.Product;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services;
    using UBind.Domain.Tests.Fakes;
    using Xunit;

    public class GlobalReleaseCacheTests
    {
        private GlobalReleaseCache sut;
        private IServiceProvider serviceProvider;
        private Mock<IProductReleaseService> productReleaseService = new Mock<IProductReleaseService>();
        private Mock<IServiceScopeFactory> serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        private Mock<IServiceScope> serviceScopeMock = new Mock<IServiceScope>();

        public GlobalReleaseCacheTests()
        {
            var devReleaseRepositoryMock = new Mock<IDevReleaseRepository>();
            var deploymentRepositoryMock = new Mock<IDeploymentRepository>();
            var releaseRepositoryMock = new Mock<IReleaseRepository>();

            var services = new ServiceCollection()
                .AddScoped<IDevReleaseRepository>(_ => devReleaseRepositoryMock.Object)
                .AddScoped<IDeploymentRepository>(_ => deploymentRepositoryMock.Object)
                .AddScoped<IReleaseRepository>(_ => releaseRepositoryMock.Object)
                .AddScoped<IProductReleaseService, ProductReleaseService>();
            this.serviceProvider = services.BuildServiceProvider();

            this.serviceScopeFactoryMock.Setup(x => x.CreateScope()).Returns(this.serviceScopeMock.Object);
            this.serviceScopeMock.Setup(x => x.ServiceProvider).Returns(this.serviceProvider);
            this.sut = new GlobalReleaseCache(null, this.serviceProvider);
        }

        [Fact]
        public void GetCurrentRelease_ReturnsLatestDevReleaseFromRepository_WhenContextSpecifiesDevelopment()
        {
            // Arrange
            var fakeDevRelease = FakeReleaseBuilder.CreateForProduct(TenantFactory.DefaultId, ProductFactory.DefaultId).BuildDevRelease();
            var releaseContext = new ReleaseContext(
                TenantFactory.DefaultId,
                ProductFactory.DefaultId,
                DeploymentEnvironment.Development,
                fakeDevRelease.Id);
            this.productReleaseService
                .Setup(r => r.GetReleaseFromDatabaseWithoutAssetFileContents(releaseContext.TenantId, releaseContext.ProductReleaseId))
                .Returns(fakeDevRelease);

            // Act
            var release = this.sut.GetRelease(
                releaseContext,
                this.productReleaseService.Object);

            // Assert
            release.ReleaseId.Should().Be(fakeDevRelease.Id);
        }

        [Fact]
        public void GetCurrentRelease_DoesNotLoadFromRepository_WhenDevReleaseIsAlreadyCached()
        {
            // Arrange
            var fakeDevRelease = FakeReleaseBuilder.CreateForProduct(TenantFactory.DefaultId, ProductFactory.DefaultId).BuildDevRelease();
            var releaseContext = new ReleaseContext(
                TenantFactory.DefaultId,
                ProductFactory.DefaultId,
                DeploymentEnvironment.Development,
                fakeDevRelease.Id);
            this.productReleaseService
                .Setup(r => r.GetReleaseFromDatabaseWithoutAssetFileContents(releaseContext.TenantId, releaseContext.ProductReleaseId))
                .Returns(fakeDevRelease);
            var release = this.sut.GetRelease(
                releaseContext,
                this.productReleaseService.Object);

            // Act
            var release2 = this.sut.GetRelease(
                releaseContext,
                this.productReleaseService.Object);

            // Assert
            release.ReleaseId.Should().Be(fakeDevRelease.Id);
            release2.ReleaseId.Should().Be(fakeDevRelease.Id);
            this.productReleaseService.Verify(
                r => r.GetReleaseFromDatabaseWithoutAssetFileContents(releaseContext.TenantId, releaseContext.ProductReleaseId),
                Times.AtLeastOnce());
        }

        [Fact]
        public void GetCurrentRelease_LoadsNewDevReleaseFromRepository_WhenCachedReleaseIsNotLatest()
        {
            // Arrange
            var oldFakeDevRelease = FakeReleaseBuilder.CreateForProduct(TenantFactory.DefaultId, ProductFactory.DefaultId).BuildDevRelease();
            var oldFakeDevReleaseContext = new ReleaseContext(
                TenantFactory.DefaultId,
                ProductFactory.DefaultId,
                DeploymentEnvironment.Development,
                oldFakeDevRelease.Id);
            this.productReleaseService
                .Setup(r => r.GetReleaseFromDatabaseWithoutAssetFileContents(oldFakeDevReleaseContext.TenantId, oldFakeDevReleaseContext.ProductReleaseId))
                .Returns(oldFakeDevRelease);
            var oldReleaseContext = new ReleaseContext(
                TenantFactory.DefaultId,
                ProductFactory.DefaultId,
                DeploymentEnvironment.Development,
                oldFakeDevRelease.Id);
            var release = this.sut.GetRelease(
                oldReleaseContext,
                this.productReleaseService.Object);

            var newFakeDevRelease = FakeReleaseBuilder.CreateForProduct(TenantFactory.DefaultId, ProductFactory.DefaultId).BuildDevRelease();
            var newReleaseContext = new ReleaseContext(
                TenantFactory.DefaultId,
                ProductFactory.DefaultId,
                DeploymentEnvironment.Development,
                newFakeDevRelease.Id);
            this.productReleaseService
                .Setup(r => r.GetReleaseFromDatabaseWithoutAssetFileContents(newReleaseContext.TenantId, newReleaseContext.ProductReleaseId))
                .Returns(newFakeDevRelease);

            // Act
            var release2 = this.sut.GetRelease(
                newReleaseContext,
                this.productReleaseService.Object);

            // Assert
            release2.ReleaseId.Should().Be(newFakeDevRelease.Id);
        }

        [Fact]
        public void GetRelease_DoesNotLoadFromRepository_WhenReleaseIsAlreadyCached()
        {
            // Arrange
            var fakeRelease = FakeReleaseBuilder.CreateForProduct(TenantFactory.DefaultId, ProductFactory.DefaultId).BuildRelease();
            var releaseContext = new ReleaseContext(TenantFactory.DefaultId, ProductFactory.DefaultId, DeploymentEnvironment.Staging, fakeRelease.Id);
            this.productReleaseService
                .Setup(r => r.GetReleaseFromDatabaseWithoutAssetFileContents(releaseContext.TenantId, releaseContext.ProductReleaseId))
                .Returns(fakeRelease);
            var release = this.sut.GetRelease(
                releaseContext,
                this.productReleaseService.Object);

            // Act
            var release2 = this.sut.GetRelease(
                releaseContext,
                this.productReleaseService.Object);

            // Assert
            release.ReleaseId.Should().Be(fakeRelease.Id);
            release2.ReleaseId.Should().Be(fakeRelease.Id);
            this.productReleaseService.Verify(
                r => r.GetReleaseFromDatabaseWithoutAssetFileContents(releaseContext.TenantId, releaseContext.ProductReleaseId),
                Times.AtLeastOnce());
        }

        [Fact]
        public void GetCurrentRelease_ReturnsCorrectRelease_WhenMultipleReleasesAreCached()
        {
            // Arrange - setup a release for each product / environment combination.
            var tenantIds = new List<Guid>
            {
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
            };
            var productIds = new List<Guid>
            {
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
            };

            var environments = new List<DeploymentEnvironment>
            {
                DeploymentEnvironment.Development,
                DeploymentEnvironment.Staging,
                DeploymentEnvironment.Production,
            };

            var releases = new Dictionary<Guid, Dictionary<Guid, Dictionary<DeploymentEnvironment, ReleaseBase>>>();
            foreach (var tenantId in tenantIds)
            {
                releases[tenantId] = new Dictionary<Guid, Dictionary<DeploymentEnvironment, ReleaseBase>>();
                foreach (var productId in productIds)
                {
                    releases[tenantId][productId] = new Dictionary<DeploymentEnvironment, ReleaseBase>();
                    foreach (var environment in environments)
                    {
                        this.SetupReleaseForProductContext(
                            tenantId,
                            productId,
                            environment,
                            releases);
                    }
                }
            }

            foreach (var tenantId in tenantIds)
            {
                foreach (var productId in productIds)
                {
                    foreach (var environment in environments)
                    {
                        var releaseId = releases[tenantId][productId][environment].Id;
                        var releaseContext = new ReleaseContext(
                            tenantId,
                            productId,
                            environment,
                            releaseId);

                        // Act - fetch each release twice to test 2nd call uses cached release.

                        // Fetch uncached
                        var firstRelease = this.sut.GetRelease(
                            releaseContext,
                            this.productReleaseService.Object);

                        // Fetch cached
                        var secondRelease = this.sut.GetRelease(
                            releaseContext,
                            this.productReleaseService.Object);

                        // Assert - verify correct releases returned, and database is only hit once for each
                        firstRelease.ReleaseId.Should().Be(releaseId);
                        secondRelease.ReleaseId.Should().Be(firstRelease.ReleaseId);
                        this.productReleaseService.Verify(
                            r => r.GetReleaseFromDatabaseWithoutAssetFileContents(releaseContext.TenantId, releaseContext.ProductReleaseId),
                            Times.AtLeastOnce());

                        this.productReleaseService.Invocations.Clear();
                    }
                }
            }
        }

        [Fact]
        public void ReleaseCached_IsInvoked_WhenAReleaseIsAddedToTheCache()
        {
            // Arrange
            var fakeDevRelease = FakeReleaseBuilder.CreateForProduct(TenantFactory.DefaultId, ProductFactory.DefaultId).BuildDevRelease();
            var releaseContext = new ReleaseContext(
                TenantFactory.DefaultId,
                ProductFactory.DefaultId,
                DeploymentEnvironment.Development,
                fakeDevRelease.Id);
            this.productReleaseService
                .Setup(r => r.GetReleaseFromDatabaseWithoutAssetFileContents(releaseContext.TenantId, releaseContext.ProductReleaseId))
                .Returns(fakeDevRelease);
            var releasesThatCachedEventsWereFiredFor = new List<ActiveDeployedRelease>();
            this.sut.ReleaseCached += (s, e) => releasesThatCachedEventsWereFiredFor.Add(e.Release);

            // Act
            var release = this.sut.GetRelease(
                releaseContext,
                this.productReleaseService.Object);

            // Assert
            releasesThatCachedEventsWereFiredFor.Should().HaveCount(1);
            releasesThatCachedEventsWereFiredFor.Should().Contain(release);
        }

        [Fact]
        public void ReleaseCached_IsNotInvoked_WhenAlreadyCachedReleaseIsReturned()
        {
            // Arrange
            var fakeDevRelease = FakeReleaseBuilder.CreateForProduct(TenantFactory.DefaultId, ProductFactory.DefaultId).BuildDevRelease();
            var releaseContext = new ReleaseContext(
                TenantFactory.DefaultId,
                ProductFactory.DefaultId,
                DeploymentEnvironment.Development,
                fakeDevRelease.Id);

            this.productReleaseService
                .Setup(r => r.GetReleaseFromDatabaseWithoutAssetFileContents(releaseContext.TenantId, releaseContext.ProductReleaseId))
                .Returns(fakeDevRelease);
            var release = this.sut.GetRelease(
                releaseContext,
                this.productReleaseService.Object);
            var releasesThatCachedEventsWereFiredFor = new List<ActiveDeployedRelease>();
            this.sut.ReleaseCached += (s, e) => releasesThatCachedEventsWereFiredFor.Add(e.Release);

            // Act
            var release2 = this.sut.GetRelease(
                releaseContext,
                this.productReleaseService.Object);

            // Assert
            releasesThatCachedEventsWereFiredFor.Should().BeEmpty();
        }

        [Fact]
        public void CacheNewDevRelease_IncludesWorkbooksInEmittedEvents()
        {
            // Arrange
            var fakeDevRelease = FakeReleaseBuilder.CreateForProduct().BuildDevRelease();
            var quoteWorkbook = new Mock<IFlexCelWorkbook>().Object;
            var claimWorkbook = new Mock<IFlexCelWorkbook>().Object;
            var fakeInitializationArtefacts = new DevReleaseInitializationArtefacts(
                fakeDevRelease,
                quoteWorkbook,
                claimWorkbook);
            ReleaseCachingArgs capturedEventArgs = null;
            this.sut.ReleaseCached += (s, e) => capturedEventArgs = e;

            // Act
            this.sut.CacheNewDevRelease(fakeInitializationArtefacts);

            // Assert
            capturedEventArgs.QuoteWorkbook.Should().Be(quoteWorkbook);
            capturedEventArgs.ClaimWorkbook.Should().Be(claimWorkbook);
        }

        private void SetupReleaseForProductContext(
           Guid tenantId,
           Guid productId,
           DeploymentEnvironment environment,
           Dictionary<Guid, Dictionary<Guid, Dictionary<DeploymentEnvironment, ReleaseBase>>> releases)
        {
            var release = FakeReleaseBuilder.CreateForProduct(TenantFactory.DefaultId, ProductFactory.DefaultId).BuildDevRelease();
            releases[tenantId][productId][environment] = release;
            var releaseContext = new ReleaseContext(
                tenantId,
                productId,
                environment,
                release.Id);
            this.productReleaseService
                .Setup(r => r.GetReleaseFromDatabaseWithoutAssetFileContents(releaseContext.TenantId, releaseContext.ProductReleaseId))
                .Returns(release);
        }
    }
}
