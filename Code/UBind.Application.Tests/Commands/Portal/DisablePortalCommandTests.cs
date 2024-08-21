// <copyright file="DisablePortalCommandTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Tests.Commands.Portal
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Moq;
    using NodaTime;
    using UBind.Application.Commands.Portal;
    using UBind.Application.Services;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Organisation;
    using UBind.Domain.Aggregates.Portal;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Repositories;
    using UBind.Domain.Tests.Fakes;
    using Xunit;

    public class DisablePortalCommandTests
    {
        private readonly Mock<IUserSessionDeletionService> mockUserSessionDeletionService = new Mock<IUserSessionDeletionService>();
        private IClock? clock;

        [Fact]
        public async Task DisablePortalCommand_Throws_Exception_When_Portal_Is_Tenant_DefaultAsync()
        {
            // Arrange
            this.clock = new TestClock();
            var now = this.clock.Now();
            var tenantId = Guid.NewGuid();
            var portalId = Guid.NewGuid();
            var defaultOrganisationId = Guid.NewGuid();
            var defaultPortalId = Guid.NewGuid();
            var tenantRepositoryMock = new Mock<ITenantRepository>();
            var portalAggregateRepositoryMock = new Mock<IPortalAggregateRepository>();
            var organisationReadModelRepositoryMock = new Mock<IOrganisationReadModelRepository>();
            var organisationAggregateRepositoryMock = new Mock<IOrganisationAggregateRepository>();
            var httpContextPropertiesResolverMock = new Mock<IHttpContextPropertiesResolver>();
            var cachingResolverMock = new Mock<ICachingResolver>();
            var tenant = new Tenant(
                TenantFactory.DefaultId,
                TenantFactory.DefaultName,
                TenantFactory.DefaultAlias,
                null,
                defaultOrganisationId,
                defaultPortalId,
                now);
            tenantRepositoryMock.Setup(s => s.GetTenantById(It.IsAny<Guid>())).Returns(tenant);
            var organisationAggregate = Organisation.CreateNewOrganisation(
                tenantId, "my-org", "My Org", null, null, now);
            organisationAggregate.SetDefaultPortal(portalId, null, now);
            var organisationReadModel = new OrganisationReadModel(
                tenantId, defaultOrganisationId, "my-org", "My Org", null, true, false, now);
            organisationReadModel.DefaultPortalId = portalId;
            organisationAggregateRepositoryMock.Setup(s => s.GetById(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(organisationAggregate);
            organisationReadModelRepositoryMock.Setup(s => s.Get(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(organisationReadModel);
            var portal = new PortalAggregate(
                tenantId,
                defaultPortalId,
                "Default Agent Portal",
                "agent",
                "Default Agent Portal",
                PortalUserType.Agent,
                defaultOrganisationId,
                null,
                now);
            portalAggregateRepositoryMock.Setup(s => s.GetById(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(portal);
            var command = new DisablePortalCommand(tenantId, defaultPortalId);
            var handler = new DisablePortalCommandHandler(
                tenantRepositoryMock.Object,
                portalAggregateRepositoryMock.Object,
                organisationReadModelRepositoryMock.Object,
                organisationAggregateRepositoryMock.Object,
                this.clock,
                httpContextPropertiesResolverMock.Object,
                cachingResolverMock.Object,
                this.mockUserSessionDeletionService.Object);

            // Act
            Func<Task> func = async () => await handler.Handle(command, CancellationToken.None);

            // Assert
            (await func.Should().ThrowAsync<ErrorException>())
                .Which.Error.Code.Should().Be("portal.cannot.disable.default.portal.for.tenant");
        }

        [Fact]
        public async Task DisablePortalCommand_RemovesDefaultPortalFromOrganisation_WhenPortalIsDefault()
        {
            // Arrange
            this.clock = new TestClock();
            var now = this.clock.Now();
            var tenantId = Guid.NewGuid();
            var organisationId = Guid.NewGuid();
            var portalId = Guid.NewGuid();
            var tenantRepositoryMock = new Mock<ITenantRepository>();
            var portalAggregateRepositoryMock = new Mock<IPortalAggregateRepository>();
            var organisationReadModelRepositoryMock = new Mock<IOrganisationReadModelRepository>();
            var organisationAggregateRepositoryMock = new Mock<IOrganisationAggregateRepository>();
            var httpContextPropertiesResolverMock = new Mock<IHttpContextPropertiesResolver>();
            var cachingResolverMock = new Mock<ICachingResolver>();
            var tenant = new Tenant(
                TenantFactory.DefaultId,
                TenantFactory.DefaultName,
                TenantFactory.DefaultAlias,
                null,
                Guid.NewGuid(), // some other org is the default for the tenant
                Guid.NewGuid(), // some other portal is the default for the tenant
                now);
            tenantRepositoryMock.Setup(s => s.GetTenantById(It.IsAny<Guid>())).Returns(tenant);
            var organisationAggregate = Organisation.CreateNewOrganisation(
                tenantId, "my-org", "My Org", null, null, now);
            organisationAggregate.SetDefaultPortal(portalId, null, now);
            var organisationReadModel = new OrganisationReadModel(
                tenantId, organisationId, "my-org", "My Org", null, true, false, now);
            organisationReadModel.DefaultPortalId = portalId;
            organisationAggregateRepositoryMock.Setup(s => s.GetById(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(organisationAggregate);
            organisationReadModelRepositoryMock.Setup(s => s.Get(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(organisationReadModel);
            var portalAggregate = new PortalAggregate(
                tenantId,
                portalId,
                "Default Agent Portal",
                "agent",
                "Default Agent Portal",
                PortalUserType.Agent,
                organisationId,
                null,
                now);
            portalAggregate.SetDefault(true, null, now);
            portalAggregateRepositoryMock.Setup(s => s.GetById(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(portalAggregate);
            var command = new DisablePortalCommand(tenantId, portalId);
            var handler = new DisablePortalCommandHandler(
                tenantRepositoryMock.Object,
                portalAggregateRepositoryMock.Object,
                organisationReadModelRepositoryMock.Object,
                organisationAggregateRepositoryMock.Object,
                this.clock,
                httpContextPropertiesResolverMock.Object,
                cachingResolverMock.Object,
                this.mockUserSessionDeletionService.Object);

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            organisationAggregate.DefaultPortalId.Should().Be(null);
        }
    }
}
