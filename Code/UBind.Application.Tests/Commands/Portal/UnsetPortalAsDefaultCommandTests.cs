// <copyright file="UnsetPortalAsDefaultCommandTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Commands.Portal
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Moq;
    using NodaTime;
    using UBind.Application.Commands.Portal;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Organisation;
    using UBind.Domain.Aggregates.Portal;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadModel.Portal;
    using UBind.Domain.Repositories;
    using UBind.Domain.Tests.Fakes;
    using Xunit;
    using static UBind.Domain.Aggregates.Organisation.Organisation;

    public class UnsetPortalAsDefaultCommandTests
    {
        private IClock clock;

        [Fact]
        public async Task UnsetAgentPortalAsDefaultCommand_RemovesDefaultPortalFromOrganisation_WhenOrganisationIsNonDefault()
        {
            // Arrange
            var testClock = new TestClock();
            this.clock = testClock;
            var now = this.clock.Now();
            var tenantId = Guid.NewGuid();
            var portalId = Guid.NewGuid();
            var organisationId = Guid.NewGuid();
            var tenantRepositoryMock = new Mock<ITenantRepository>();
            var portalAggregateRepositoryMock = new Mock<IPortalAggregateRepository>();
            var portalReadModelRepositoryMock = new Mock<IPortalReadModelRepository>();
            var organisationAggregateRepositoryMock = new Mock<IOrganisationAggregateRepository>();
            var httpContextPropertiesResolverMock = new Mock<IHttpContextPropertiesResolver>();
            var cachingResolverMock = new Mock<ICachingResolver>();
            var tenant = new Tenant(
                TenantFactory.DefaultId,
                TenantFactory.DefaultName,
                TenantFactory.DefaultAlias,
                null,
                Guid.NewGuid(), // some other organisation is the tenant default
                Guid.NewGuid(), // some other portal is the tenant default
                now);
            tenantRepositoryMock.Setup(s => s.GetTenantById(It.IsAny<Guid>())).Returns(tenant);
            var organisation = Organisation.CreateNewOrganisation(
                tenantId, "my-org", "My Org", null, null, now);
            organisation.SetDefaultPortal(portalId, null, now);
            organisationAggregateRepositoryMock.Setup(s => s.GetById(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(organisation);
            var portalAggregate = new PortalAggregate(
                tenantId,
                portalId,
                "Original Agent Portal",
                "original-agent",
                "Original Agent Portal",
                PortalUserType.Agent,
                organisationId,
                null,
                now);
            portalAggregateRepositoryMock.Setup(s => s.GetById(It.IsAny<Guid>(), portalId)).Returns(portalAggregate);
            var command = new UnsetPortalAsDefaultCommand(tenantId, organisationId, portalId);
            testClock.Increment(Duration.FromMilliseconds(1));
            var handler = new UnsetPortalAsDefaultCommandHandler(
                tenantRepositoryMock.Object,
                portalAggregateRepositoryMock.Object,
                portalReadModelRepositoryMock.Object,
                organisationAggregateRepositoryMock.Object,
                httpContextPropertiesResolverMock.Object,
                this.clock,
                cachingResolverMock.Object);

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            organisation.UnsavedEvents.OfType<SetOrganisationDefaultPortalEvent>().Last()
                .PortalId.Should().Be(null);
        }
    }
}
