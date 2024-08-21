// <copyright file="DeletePortalCommandTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Commands.Portal
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Hangfire;
    using Moq;
    using NodaTime;
    using UBind.Application.Commands.Portal;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Organisation;
    using UBind.Domain.Aggregates.Portal;
    using UBind.Domain.Aggregates.User;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Repositories;
    using UBind.Domain.Tests.Fakes;
    using Xunit;

    public class DeletePortalCommandTests
    {
        private IClock clock;

        [Fact]
        public async Task DeletePortalCommand_Throws_Exception_When_Portal_Is_Tenant_DefaultAsync()
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
            var userReadModelRepositoryMock = new Mock<IUserReadModelRepository>();
            var userAggregateRepositoryMock = new Mock<IUserAggregateRepository>();
            var backgroundJobClientMock = new Mock<IBackgroundJobClient>();
            var dbContextMock = new Mock<IUBindDbContext>();
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
            var command = new DeletePortalCommand(tenantId, defaultPortalId);
            var handler = new DeletePortalCommandHandler(
                tenantRepositoryMock.Object,
                portalAggregateRepositoryMock.Object,
                organisationReadModelRepositoryMock.Object,
                organisationAggregateRepositoryMock.Object,
                this.clock,
                httpContextPropertiesResolverMock.Object,
                cachingResolverMock.Object,
                userReadModelRepositoryMock.Object,
                userAggregateRepositoryMock.Object,
                backgroundJobClientMock.Object,
                dbContextMock.Object);

            // Act
            Func<Task> func = async () => await handler.Handle(command, CancellationToken.None);

            // Assert
            (await func.Should().ThrowAsync<ErrorException>())
                .Which.Error.Code.Should().Be("portal.cannot.delete.default.portal.for.tenant");
        }
    }
}
