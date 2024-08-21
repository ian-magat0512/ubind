// <copyright file="CachingResolverTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading;
    using FluentAssertions;
    using Humanizer;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using UBind.Application.Queries.Services;
    using UBind.Application.Services.Imports;
    using UBind.Domain;
    using UBind.Domain.Enums;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Product;
    using UBind.Domain.Repositories;
    using UBind.Domain.Tests.Fakes;
    using Xunit;

    public class CachingResolverTests
    {
        private ICachingResolver cachingResolver;
        private Mock<ICqrsMediator> mockMediator;
        private Mock<ITenantRepository> mockTenantRepository;
        private Mock<IProductRepository> mockProductRepository;
        private Mock<IFeatureSettingRepository> mockFeatureSettingRepository;
        private Mock<IProductFeatureSettingRepository> mockProductFeatureSettingRepository;

        public CachingResolverTests()
        {
            var services = new ServiceCollection();
            var internalUrlConfiguration = new InternalUrlConfiguration() { BaseApi = "https://localhost:44301" };
            services.AddTransient<ICachingResolver>(c => this.cachingResolver);
            services.AddTransient<ITenantRepository>(c => this.mockTenantRepository.Object);
            services.AddTransient<ICqrsMediator>(c => this.mockMediator.Object);

            this.mockMediator = new Mock<ICqrsMediator>();
            this.mockTenantRepository = new Mock<ITenantRepository>();
            this.mockProductRepository = new Mock<IProductRepository>();
            this.mockFeatureSettingRepository = new Mock<IFeatureSettingRepository>();
            this.mockProductFeatureSettingRepository = new Mock<IProductFeatureSettingRepository>();

            var now = new TestClock().GetCurrentInstant();
            var settings = new List<Setting>();
            settings.Add(new Setting("customer-mgmt", Feature.CustomerManagement.Humanize(), "contact", 1, now, IconLibrary.IonicV4));
            settings.Add(new Setting("policy-mgmt", Feature.PolicyManagement.Humanize(), "shield", 2, now, IconLibrary.IonicV4));
            settings.Add(new Setting("user-mgmt", Feature.UserManagement.Humanize(), "people", 3, now, IconLibrary.IonicV4));

            var tenant = new Tenant(Guid.NewGuid(), "test tenant", "alias", null, default, default, now);

            this.mockTenantRepository.Setup(t => t.GetTenantById(It.IsAny<Guid>())).Returns(tenant);
            this.mockFeatureSettingRepository
                .Setup(x => x.GetSettings(It.IsAny<Guid>()))
                .Returns(settings);

            this.cachingResolver = new CachingResolver(
                this.mockMediator.Object,
                this.mockTenantRepository.Object,
                this.mockProductRepository.Object,
                this.mockFeatureSettingRepository.Object,
                this.mockProductFeatureSettingRepository.Object);
        }

        [Fact]
        public void GetSettingsOrThrow_ShouldAccessDbAndCreateCacheEntry_WhenFeatureSettingsNotInCache()
        {
            // Arrange
            var tenantId = Guid.NewGuid();

            // Act
            this.cachingResolver.GetSettingsOrThrow(tenantId);

            // Assert
            this.mockFeatureSettingRepository
                .Verify(x => x.GetSettings(tenantId), Times.Once());
        }

        [Fact]
        public void GetSettingsOrNull_ShouldAccessDbAndCreateCacheEntry_WhenFeatureSettingsNotInCache()
        {
            // Arrange
            var tenantId = Guid.NewGuid();

            // Act
            this.cachingResolver.GetSettingsOrNull(tenantId);

            // Assert
            this.mockFeatureSettingRepository
                .Verify(x => x.GetSettings(tenantId), Times.Once());
        }

        [Fact]
        public void GetActiveSettingsOrThrow_ShouldAccessDbAndCreateCacheEntry_WhenFeatureSettingsNotInCache()
        {
            // Arrange
            var tenantId = Guid.NewGuid();

            // Act
            this.cachingResolver.GetActiveSettingsOrThrow(tenantId);

            // Assert
            this.mockFeatureSettingRepository
                .Verify(x => x.GetActiveSettings(tenantId), Times.Once());
        }

        [Fact]
        public void GetActiveSettingsOrNull_ShouldAccessDbAndCreateCacheEntry_WhenFeatureSettingsNotInCache()
        {
            // Arrange
            var tenantId = Guid.NewGuid();

            // Act
            this.cachingResolver.GetActiveSettingsOrNull(tenantId);

            // Assert
            this.mockFeatureSettingRepository
                .Verify(x => x.GetActiveSettings(tenantId), Times.Once());
        }

        [Fact]
        public void GetSettingsOrThrow_ShouldAccessCache_WhenFeatureSettingsAreInCache()
        {
            // Arrange
            var tenantId = Guid.NewGuid();

            // Act
            this.cachingResolver.GetSettingsOrThrow(tenantId);
            Thread.Sleep(TimeSpan.FromSeconds(5));

            this.cachingResolver.GetSettingsOrThrow(tenantId);

            // Assert
            this.mockFeatureSettingRepository
                .Verify(x => x.GetSettings(tenantId), Times.Once());
        }

        [Fact]
        public void GetSettingsOrNull_ShouldAccessCache_WhenFeatureSettingsAreInCache()
        {
            // Arrange
            var tenantId = Guid.NewGuid();

            // Act
            this.cachingResolver.GetSettingsOrNull(tenantId);
            Thread.Sleep(TimeSpan.FromSeconds(5));

            this.cachingResolver.GetSettingsOrNull(tenantId);

            // Assert
            this.mockFeatureSettingRepository
                .Verify(x => x.GetSettings(tenantId), Times.Once());
        }

        [Fact]
        public void GetActiveSettingsOrThrow_ShouldAccessCache_WhenFeatureSettingsAreInCache()
        {
            // Arrange
            var tenantId = Guid.NewGuid();

            // Act
            this.cachingResolver.GetActiveSettingsOrThrow(tenantId);
            Thread.Sleep(TimeSpan.FromSeconds(5));

            this.cachingResolver.GetActiveSettingsOrThrow(tenantId);

            // Assert
            this.mockFeatureSettingRepository
                .Verify(x => x.GetActiveSettings(tenantId), Times.Once());
        }

        [Fact]
        public void GetActiveSettingsOrNull_ShouldAccessCache_WhenFeatureSettingsAreInCache()
        {
            // Arrange
            var tenantId = Guid.NewGuid();

            // Act
            this.cachingResolver.GetActiveSettingsOrNull(tenantId);
            Thread.Sleep(TimeSpan.FromSeconds(5));

            this.cachingResolver.GetActiveSettingsOrNull(tenantId);

            // Assert
            this.mockFeatureSettingRepository
                .Verify(x => x.GetActiveSettings(tenantId), Times.Once());
        }

        [Fact]
        public void GetSettingsOrThrow_ShouldThrowError_WhenTenantIdIsEmptyOrInvalid()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            this.mockTenantRepository.Setup(t => t.GetTenantById(tenantId)).Returns((Tenant)null);

            // Act & Assert
            var result = this.cachingResolver
                            .Invoking(c => c.GetSettingsOrThrow(tenantId))
                            .Should().Throw<ErrorException>().And
                            .Error.HttpStatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public void GetActiveSettingsOrThrow_ShouldThrowError_WhenTenantIdIsEmptyOrInvalid()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            this.mockTenantRepository.Setup(t => t.GetTenantById(tenantId)).Returns((Tenant)null);

            // Act & Assert
            var result = this.cachingResolver
                            .Invoking(c => c.GetActiveSettingsOrThrow(tenantId))
                            .Should().Throw<ErrorException>().And
                            .Error.HttpStatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public void GetSettingsOrNull_ShouldReturnNull_WhenTenantIdIsEmptyOrInvalid()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            this.mockTenantRepository.Setup(t => t.GetTenantById(tenantId)).Returns((Tenant)null);

            // Act
            var result = this.cachingResolver.GetSettingsOrNull(tenantId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void GetActiveSettingsOrNull_ShouldReturnNull_WhenTenantIdIsEmptyOrInvalid()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            this.mockTenantRepository.Setup(t => t.GetTenantById(tenantId)).Returns((Tenant)null);

            // Act
            var result = this.cachingResolver.GetActiveSettingsOrNull(tenantId);

            // Assert
            result.Should().BeNull();
        }
    }
}
