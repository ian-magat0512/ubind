// <copyright file="FeatureSettingServiceTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests
{
    using FluentAssertions;
    using Humanizer;
    using Moq;
    using UBind.Application.Queries.Services;
    using UBind.Domain;
    using UBind.Domain.Aggregates.User;
    using UBind.Domain.Enums;
    using UBind.Domain.Events;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Product;
    using UBind.Domain.Repositories;
    using UBind.Domain.Tests.Fakes;
    using Xunit;

    public class FeatureSettingServiceTests
    {
        private IFeatureSettingService featureSettingService;
        private Mock<IFeatureSettingRepository> mockSettingRepository;
        private Mock<IUserAggregateRepository> mockUserRepository;
        private Mock<ICachingResolver> mockCachingResolver;
        private Mock<ITenantSystemEventEmitter> mockTenantSystemEventEmitter;

        public FeatureSettingServiceTests()
        {
            this.mockSettingRepository = new Mock<IFeatureSettingRepository>();
            this.mockUserRepository = new Mock<IUserAggregateRepository>();
            this.mockCachingResolver = new Mock<ICachingResolver>();
            this.mockTenantSystemEventEmitter = new Mock<ITenantSystemEventEmitter>();

            this.featureSettingService = new FeatureSettingService(this.mockSettingRepository.Object, this.mockTenantSystemEventEmitter.Object, this.mockCachingResolver.Object);
        }

        [Fact]
        public void TenantHasActiveFeature_Should_Return_True_When_it_HasSettings()
        {
            // Arrange
            var now = new TestClock().GetCurrentInstant();
            var settings = new List<Setting>();
            settings.Add(new Setting("customer-mgmt", Feature.CustomerManagement.Humanize(), "contact", 1, now, IconLibrary.IonicV4));
            settings.Add(new Setting("policy-mgmt", Feature.PolicyManagement.Humanize(), "shield", 2, now, IconLibrary.IonicV4));
            settings.Add(new Setting("user-mgmt", Feature.UserManagement.Humanize(), "people", 3, now, IconLibrary.IonicV4));
            this.mockCachingResolver
                .Setup(x => x.GetActiveSettingsOrNull(It.IsAny<Guid>()))
                .Returns(settings);
            var tenantId = Guid.NewGuid();

            // Act
            var result = this.featureSettingService.TenantHasActiveFeature(tenantId, Feature.CustomerManagement);

            // Assert
            result.Should().Be(true);
        }

        [Fact]
        public void TenantHasActiveFeature_Should_Return_False_When_it_HasSettings()
        {
            // Arrange
            this.mockCachingResolver
                .Setup(x => x.GetActiveSettingsOrThrow(It.IsAny<Guid>()))
                .Returns((List<Setting>)null);
            var tenantId = Guid.NewGuid();

            // Act
            var result = this.featureSettingService.TenantHasActiveFeature(tenantId, Feature.CustomerManagement);

            // Assert
            result.Should().Be(false);
        }

        [Fact]
        public void TenantHasActiveFeature_Should_Throw_ErrorException_When_TenantId_Is_Empty()
        {
            // Arrange
            this.featureSettingService = new FeatureSettingService(this.mockSettingRepository.Object, this.mockTenantSystemEventEmitter.Object, this.GetCachingResolver());
            var invalidTenantId = Guid.NewGuid();

            // Act
            var result = () => this.featureSettingService.TenantHasActiveFeature(Guid.Empty, Feature.CustomerManagement);

            // Assert
            result.Should().Throw<ErrorException>();
        }

        private CachingResolver GetCachingResolver()
        {
            var mockMediator = new Mock<ICqrsMediator>();
            var mockTenantRepository = new Mock<ITenantRepository>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockFeatureSettingRepository = new Mock<IFeatureSettingRepository>();
            var mockProductSettingRepository = new Mock<IProductFeatureSettingRepository>();

            return new CachingResolver(
                mockMediator.Object,
                mockTenantRepository.Object,
                mockProductRepository.Object,
                mockFeatureSettingRepository.Object,
                mockProductSettingRepository.Object);
        }
    }
}
