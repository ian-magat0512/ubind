// <copyright file="GetPortalUrlQueryTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Queries.Portal
{
    using FluentAssertions;
    using Moq;
    using NodaTime;
    using UBind.Application.Queries.Portal;
    using UBind.Domain;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Portal;
    using UBind.Domain.Services;
    using Xunit;

    public class GetPortalUrlQueryTests
    {
        const string BaseUrl = "https://app.ubind.io";
        private readonly IClock clock = SystemClock.Instance;
        private readonly Mock<IBaseUrlResolver> baseUrlResolverMock = new Mock<IBaseUrlResolver>();
        private readonly Mock<IPortalReadModelRepository> portalReadModelRepositoryMock = new Mock<IPortalReadModelRepository>();

        public GetPortalUrlQueryTests()
        {
            this.baseUrlResolverMock.Setup(s => s.GetBaseUrl(It.IsAny<Tenant>())).Returns(BaseUrl);
        }

        [Fact]
        public async Task Handle_ReturnsCustomUrl_WhenOneIsSet()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var organisationId = Guid.NewGuid();
            var portalId = Guid.NewGuid();
            var portal = new PortalReadModel
            {
                Id = portalId,
                Name = "Test Portal",
                Alias = "test-portal",
                Title = "Test Portal",
                UserType = PortalUserType.Agent,
                IsDefault = true,
                OrganisationId = organisationId,
                TenantId = tenantId,
                ProductionUrl = "https://custom.production.domain/one.html",
            };
            var cachingResolverMock = new Mock<ICachingResolver>();
            cachingResolverMock.Setup(s => s.GetPortalOrThrow(It.IsAny<Guid>(), It.IsAny<GuidOrAlias>()))
                .Returns(Task.FromResult(portal));
            var portalService = new PortalService(cachingResolverMock.Object, this.baseUrlResolverMock.Object, this.portalReadModelRepositoryMock.Object);
            var sut = new GetPortalUrlQueryHandler(cachingResolverMock.Object, portalService);

            // Act
            var result = await sut.Handle(new GetPortalUrlQuery(
                tenantId, organisationId, portalId, DeploymentEnvironment.Production),
                CancellationToken.None);

            // Assert
            result.Should().Be(portal.ProductionUrl);
        }

        [Fact]
        public async Task Handle_ReturnsFullDefaultUrl_WhenCustomOneIsNotSetAndPortalIsNotDefault()
        {
            // Arrange
            var now = this.clock.Now();
            var tenantId = Guid.NewGuid();
            var organisationId = Guid.NewGuid();
            var portalId = Guid.NewGuid();
            var portal = new PortalReadModel
            {
                Id = portalId,
                Name = "Test Portal",
                Alias = "test-portal",
                Title = "Test Portal",
                UserType = PortalUserType.Agent,
                IsDefault = false,
                OrganisationId = organisationId,
                TenantId = tenantId,
                ProductionUrl = "https://custom.production.domain/one.html",
            };
            var organisation = new OrganisationReadModel(tenantId, organisationId, "test-org", "Test Org", null, false, false, now);
            var tenant = new Tenant(tenantId, "Test Tenant", "test-tenant", null, default, default, now);
            var cachingResolverMock = new Mock<ICachingResolver>();
            cachingResolverMock.Setup(s => s.GetPortalOrThrow(It.IsAny<Guid>(), It.IsAny<GuidOrAlias>()))
                .Returns(Task.FromResult(portal));
            cachingResolverMock.Setup(s => s.GetTenantOrThrow(It.IsAny<Guid>()))
                .Returns(Task.FromResult(tenant));
            cachingResolverMock.Setup(s => s.GetOrganisationOrThrow(It.IsAny<Guid>(), It.IsAny<GuidOrAlias>()))
                .Returns(Task.FromResult(organisation));
            cachingResolverMock.Setup(s => s.GetOrganisationOrThrow(It.IsAny<Tenant>(), It.IsAny<GuidOrAlias>()))
                .Returns(Task.FromResult(organisation));
            var portalService = new PortalService(cachingResolverMock.Object, this.baseUrlResolverMock.Object, this.portalReadModelRepositoryMock.Object);
            var sut = new GetPortalUrlQueryHandler(cachingResolverMock.Object, portalService);

            // Act
            var result = await sut.Handle(new GetPortalUrlQuery(
                tenantId, organisationId, portalId, DeploymentEnvironment.Staging), CancellationToken.None);

            // Assert
            result.Should().Be($"{BaseUrl}/portal/{tenant.Details.Alias}/{organisation.Alias}/{portal.Alias}?environment=staging");
        }

        [Fact]
        public async Task Handle_ReturnsShortenedDefaultUrl_WhenCustomOneIsNotSetAndPortalIsDefaultForOrganisation()
        {
            // Arrange
            var now = this.clock.Now();
            var tenantId = Guid.NewGuid();
            var organisationId = Guid.NewGuid();
            var portalId = Guid.NewGuid();
            var portal = new PortalReadModel
            {
                Id = portalId,
                Name = "Test Portal",
                Alias = "test-portal",
                Title = "Test Portal",
                UserType = PortalUserType.Agent,
                IsDefault = true,
                OrganisationId = organisationId,
                TenantId = tenantId,
                ProductionUrl = "https://custom.production.domain/one.html",
            };
            var organisation = new OrganisationReadModel(tenantId, organisationId, "test-org", "Test Org", null, false, false, now);
            organisation.DefaultPortalId = portalId;
            var tenant = new Tenant(tenantId, "Test Tenant", "test-tenant", null, default, default, now);
            var cachingResolverMock = new Mock<ICachingResolver>();
            cachingResolverMock.Setup(s => s.GetPortalOrThrow(It.IsAny<Guid>(), It.IsAny<GuidOrAlias>()))
                .Returns(Task.FromResult(portal));
            cachingResolverMock.Setup(s => s.GetTenantOrThrow(It.IsAny<Guid>()))
                .Returns(Task.FromResult(tenant));
            cachingResolverMock.Setup(s => s.GetOrganisationOrThrow(It.IsAny<Guid>(), It.IsAny<GuidOrAlias>()))
                .Returns(Task.FromResult(organisation));
            cachingResolverMock.Setup(s => s.GetOrganisationOrThrow(It.IsAny<Tenant>(), It.IsAny<GuidOrAlias>()))
                .Returns(Task.FromResult(organisation));
            var portalService = new PortalService(cachingResolverMock.Object, this.baseUrlResolverMock.Object, this.portalReadModelRepositoryMock.Object);
            var sut = new GetPortalUrlQueryHandler(cachingResolverMock.Object, portalService);

            // Act
            var result = await sut.Handle(new GetPortalUrlQuery(
                tenantId, organisationId, portalId, DeploymentEnvironment.Staging), CancellationToken.None);

            // Assert
            result.Should().Be($"{BaseUrl}/portal/{tenant.Details.Alias}/{organisation.Alias}?environment=staging");
        }

        [Fact]
        public async Task Handle_ReturnsShortenedDefaultUrl_WhenCustomOneIsNotSetAndPortalIsDefaultForDefaultOrganisation()
        {
            // Arrange
            var now = this.clock.Now();
            var tenantId = Guid.NewGuid();
            var organisationId = Guid.NewGuid();
            var portalId = Guid.NewGuid();
            var portal = new PortalReadModel
            {
                Id = portalId,
                Name = "Test Portal",
                Alias = "test-portal",
                Title = "Test Portal",
                UserType = PortalUserType.Agent,
                IsDefault = true,
                OrganisationId = organisationId,
                TenantId = tenantId,
                ProductionUrl = "https://custom.production.domain/one.html",
            };
            var organisation = new OrganisationReadModel(tenantId, organisationId, "test-org", "Test Org", null, false, false, now);
            organisation.DefaultPortalId = portalId;
            var tenant = new Tenant(tenantId, "Test Tenant", "test-tenant", null, organisationId, default, now);
            var cachingResolverMock = new Mock<ICachingResolver>();
            cachingResolverMock.Setup(s => s.GetPortalOrThrow(It.IsAny<Guid>(), It.IsAny<GuidOrAlias>()))
                .Returns(Task.FromResult(portal));
            cachingResolverMock.Setup(s => s.GetTenantOrThrow(It.IsAny<Guid>()))
                .Returns(Task.FromResult(tenant));
            cachingResolverMock.Setup(s => s.GetOrganisationOrThrow(It.IsAny<Guid>(), It.IsAny<GuidOrAlias>()))
                .Returns(Task.FromResult(organisation));
            cachingResolverMock.Setup(s => s.GetOrganisationOrThrow(It.IsAny<Tenant>(), It.IsAny<GuidOrAlias>()))
                .Returns(Task.FromResult(organisation));
            var portalService = new PortalService(cachingResolverMock.Object, this.baseUrlResolverMock.Object, this.portalReadModelRepositoryMock.Object);
            var sut = new GetPortalUrlQueryHandler(cachingResolverMock.Object, portalService);

            // Act
            var result = await sut.Handle(new GetPortalUrlQuery(
                tenantId, organisationId, portalId, DeploymentEnvironment.Staging), CancellationToken.None);

            // Assert
            result.Should().Be($"{BaseUrl}/portal/{tenant.Details.Alias}?environment=staging");
        }
    }
}
