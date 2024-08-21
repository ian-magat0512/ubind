// <copyright file="SetPortalAsDefaultCommandTests.cs" company="uBind">
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

    public class SetPortalAsDefaultCommandTests
    {
        private IClock clock;

        [Fact]
        public async Task SetPortalAsDefaultCommand_SetsDefaultPortalOnOrganisationAndTenant_WhenPortalIsAgentAndOrganisationIsDefault()
        {
            // Arrange
            var testClock = new TestClock();
            this.clock = testClock;
            var now = this.clock.Now();
            var tenantId = Guid.NewGuid();
            var portalId = Guid.NewGuid();
            var defaultOrganisationId = Guid.NewGuid();
            var originalPortalId = Guid.NewGuid();
            var newPortalId = Guid.NewGuid();
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
                defaultOrganisationId,
                originalPortalId,
                now);
            tenantRepositoryMock.Setup(s => s.GetTenantById(It.IsAny<Guid>())).Returns(tenant);
            var organisation = Organisation.CreateNewOrganisation(
                tenantId, "my-org", "My Org", null, null, now);
            organisation.SetDefaultPortal(originalPortalId, null, now);
            organisationAggregateRepositoryMock.Setup(s => s.GetById(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(organisation);
            var originalPortal = new PortalAggregate(
                tenantId,
                originalPortalId,
                "Original Agent Portal",
                "original-agent",
                "Original Agent Portal",
                PortalUserType.Agent,
                defaultOrganisationId,
                null,
                now);
            originalPortal.SetDefault(true, null, this.clock.Now());
            var newPortal = new PortalAggregate(
                tenantId,
                newPortalId,
                "New Agent Portal",
                "new-agent",
                "New Agent Portal",
                PortalUserType.Agent,
                defaultOrganisationId,
                null,
                now);
            portalAggregateRepositoryMock.Setup(s => s.GetById(It.IsAny<Guid>(), originalPortalId)).Returns(originalPortal);
            portalAggregateRepositoryMock.Setup(s => s.GetById(It.IsAny<Guid>(), newPortalId)).Returns(newPortal);
            portalReadModelRepositoryMock
                .Setup(s => s.GetDefaultPortalId(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<PortalUserType>()))
                .Returns(originalPortalId);
            var command = new SetPortalAsDefaultCommand(tenantId, defaultOrganisationId, newPortalId);
            testClock.Increment(Duration.FromMilliseconds(1));
            var handler = new SetPortalAsDefaultCommandHandler(
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
            organisation.UnsavedEvents.OfType<SetOrganisationDefaultPortalEvent>().Should().HaveCount(2);
            organisation.UnsavedEvents.OfType<SetOrganisationDefaultPortalEvent>().Last()
                .PortalId.Should().Be(newPortalId);
            tenant.Details.DefaultPortalId.Should().Be(newPortalId);
        }

        [Fact]
        public async Task SetPortalAsDefaultCommand_DoesNotSetDefaultPortalOnOrganisationAndTenant_WhenPortalIsCustomerAndOrganisationIsDefault()
        {
            // Arrange
            var testClock = new TestClock();
            this.clock = testClock;
            var now = this.clock.Now();
            var tenantId = Guid.NewGuid();
            var defaultOrganisationId = Guid.NewGuid();
            var agentPortalId = Guid.NewGuid();
            var customerPortalId = Guid.NewGuid();
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
                defaultOrganisationId,
                agentPortalId,
                now);
            tenantRepositoryMock.Setup(s => s.GetTenantById(It.IsAny<Guid>())).Returns(tenant);
            var organisation = Organisation.CreateNewOrganisation(
                tenantId, "my-org", "My Org", null, null, now);
            organisation.SetDefaultPortal(agentPortalId, null, now);
            organisationAggregateRepositoryMock.Setup(s => s.GetById(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(organisation);
            var agentPortal = new PortalAggregate(
                tenantId,
                agentPortalId,
                "Original Agent Portal",
                "original-agent",
                "Original Agent Portal",
                PortalUserType.Agent,
                defaultOrganisationId,
                null,
                now);
            agentPortal.SetDefault(true, null, this.clock.Now());
            var newPortal = new PortalAggregate(
                tenantId,
                customerPortalId,
                "New Customer Portal",
                "new-customer",
                "New Customer Portal",
                PortalUserType.Customer,
                defaultOrganisationId,
                null,
                now);
            portalAggregateRepositoryMock.Setup(s => s.GetById(It.IsAny<Guid>(), agentPortalId)).Returns(agentPortal);
            portalAggregateRepositoryMock.Setup(s => s.GetById(It.IsAny<Guid>(), customerPortalId)).Returns(newPortal);
            portalReadModelRepositoryMock
                .Setup(s => s.GetDefaultPortalId(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<PortalUserType>()))
                .Returns((Guid?)null);
            var command = new SetPortalAsDefaultCommand(tenantId, defaultOrganisationId, customerPortalId);
            testClock.Increment(Duration.FromMilliseconds(1));
            var handler = new SetPortalAsDefaultCommandHandler(
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
            organisation.UnsavedEvents.OfType<SetOrganisationDefaultPortalEvent>().Should().HaveCount(1);
            organisation.UnsavedEvents.OfType<SetOrganisationDefaultPortalEvent>().Last()
                .PortalId.Should().Be(agentPortalId);
            tenant.Details.DefaultPortalId.Should().Be(agentPortalId);
        }

        [Fact]
        public async Task SetAgentPortalAsDefaultCommand_RemovesDefaultFlagOnExistingAgentPortal()
        {
            // Arrange
            this.clock = new TestClock();
            var now = this.clock.Now();
            var tenantId = Guid.NewGuid();
            var portalId = Guid.NewGuid();
            var defaultOrganisationId = Guid.NewGuid();
            var originalPortalId = Guid.NewGuid();
            var newPortalId = Guid.NewGuid();
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
                defaultOrganisationId,
                originalPortalId,
                now);
            tenantRepositoryMock.Setup(s => s.GetTenantById(It.IsAny<Guid>())).Returns(tenant);
            var organisation = Organisation.CreateNewOrganisation(
                tenantId, "my-org", "My Org", null, null, now);
            organisation.SetDefaultPortal(originalPortalId, null, now);
            organisationAggregateRepositoryMock.Setup(s => s.GetById(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(organisation);
            var originalPortal = new PortalAggregate(
                tenantId,
                originalPortalId,
                "Original Agent Portal",
                "original-agent",
                "Original Agent Portal",
                PortalUserType.Agent,
                defaultOrganisationId,
                null,
                now);
            originalPortal.SetDefault(true, null, this.clock.Now());
            var newPortal = new PortalAggregate(
                tenantId,
                newPortalId,
                "New Agent Portal",
                "new-agent",
                "New Agent Portal",
                PortalUserType.Agent,
                defaultOrganisationId,
                null,
                now);
            portalAggregateRepositoryMock.Setup(s => s.GetById(It.IsAny<Guid>(), originalPortalId)).Returns(originalPortal);
            portalAggregateRepositoryMock.Setup(s => s.GetById(It.IsAny<Guid>(), newPortalId)).Returns(newPortal);
            portalReadModelRepositoryMock
                .Setup(s => s.GetDefaultPortalId(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<PortalUserType>()))
                .Returns(originalPortalId);
            var command = new SetPortalAsDefaultCommand(tenantId, defaultOrganisationId, newPortalId);
            var handler = new SetPortalAsDefaultCommandHandler(
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
            originalPortal.IsDefault.Should().BeFalse();
            newPortal.IsDefault.Should().BeTrue();
        }

        [Fact]
        public async Task SetCustomerPortalAsDefaultCommand_DoesNotRemoveDefaultFlagOnExistingAgentPortal()
        {
            // Arrange
            var testClock = new TestClock();
            this.clock = testClock;
            var now = this.clock.Now();
            var tenantId = Guid.NewGuid();
            var defaultOrganisationId = Guid.NewGuid();
            var agentPortalId = Guid.NewGuid();
            var customerPortalId = Guid.NewGuid();
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
                defaultOrganisationId,
                agentPortalId,
                now);
            tenantRepositoryMock.Setup(s => s.GetTenantById(It.IsAny<Guid>())).Returns(tenant);
            var organisation = Organisation.CreateNewOrganisation(
                tenantId, "my-org", "My Org", null, null, now);
            organisation.SetDefaultPortal(agentPortalId, null, now);
            organisationAggregateRepositoryMock.Setup(s => s.GetById(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(organisation);
            var agentPortal = new PortalAggregate(
                tenantId,
                agentPortalId,
                "Original Agent Portal",
                "original-agent",
                "Original Agent Portal",
                PortalUserType.Agent,
                defaultOrganisationId,
                null,
                now);
            agentPortal.SetDefault(true, null, this.clock.Now());
            var customerPortal = new PortalAggregate(
                tenantId,
                customerPortalId,
                "New Customer Portal",
                "new-customer",
                "New Customer Portal",
                PortalUserType.Customer,
                defaultOrganisationId,
                null,
                now);
            portalAggregateRepositoryMock.Setup(s => s.GetById(It.IsAny<Guid>(), agentPortalId)).Returns(agentPortal);
            portalAggregateRepositoryMock.Setup(s => s.GetById(It.IsAny<Guid>(), customerPortalId)).Returns(customerPortal);
            portalReadModelRepositoryMock
                .Setup(s => s.GetDefaultPortalId(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<PortalUserType>()))
                .Returns((Guid?)null);
            var command = new SetPortalAsDefaultCommand(tenantId, defaultOrganisationId, customerPortalId);
            var handler = new SetPortalAsDefaultCommandHandler(
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
            agentPortal.IsDefault.Should().BeTrue();
            customerPortal.IsDefault.Should().BeTrue();
        }
    }
}
