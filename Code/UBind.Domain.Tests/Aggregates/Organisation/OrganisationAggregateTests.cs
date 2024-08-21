// <copyright file="OrganisationAggregateTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

#pragma warning disable SA1600

namespace UBind.Domain.Tests.Aggregates.Organisation
{
    using System;
    using FluentAssertions;
    using NodaTime;
    using UBind.Domain.Aggregates.Organisation;
    using UBind.Domain.Tests.Fakes;
    using Xunit;

    public class OrganisationAggregateTests
    {
        private readonly IClock clock = SystemClock.Instance;
        private readonly Guid tenantId = TenantFactory.DefaultId;
        private readonly string tenantAlias = "tenant-alias";
        private readonly string tenantName = "tenant-name";
        private readonly Guid updatedTenantId = Guid.NewGuid();
        private readonly string updatedTenantAlias = "updated-tenant-alias";
        private readonly string updatedTenantName = "updated-tenant-name";
        private readonly Guid? performingUserId = Guid.NewGuid();

        [Fact]
        public void Initialisation_ShouldUpdatePropertiesCorrectly()
        {
            // Arrange
            var organisationAggregate = Organisation.CreateNewOrganisation(
                this.tenantId, this.tenantAlias, this.tenantName, null, this.performingUserId, this.clock.GetCurrentInstant());

            var updatedOrganisationAggregate = Organisation
                .CreateNewOrganisation(
                    this.updatedTenantId,
                    this.updatedTenantAlias,
                    this.updatedTenantName,
                    null, this.performingUserId,
                    this.clock.GetCurrentInstant());

            // Act
            organisationAggregate.Update(
                updatedOrganisationAggregate.Alias,
                updatedOrganisationAggregate.Name,
                this.performingUserId,
                this.clock.GetCurrentInstant());

            // Assert
            organisationAggregate.Alias.Should().Be(this.updatedTenantAlias);
            organisationAggregate.Name.Should().Be(this.updatedTenantName);
        }

        [Fact]
        public void Activate_ShouldActivateOrganisation()
        {
            // Arrange
            var nonDefaultOrganisationAggregate = Organisation.CreateNewOrganisation(
                this.tenantId, this.tenantAlias, this.tenantName, null, this.performingUserId, this.clock.GetCurrentInstant());

            // Act
            nonDefaultOrganisationAggregate.Disable(this.performingUserId, this.clock.GetCurrentInstant());

            // Assert
            nonDefaultOrganisationAggregate.IsActive.Should().BeFalse();

            // Act
            nonDefaultOrganisationAggregate.Activate(this.performingUserId, this.clock.GetCurrentInstant());

            // Assert
            nonDefaultOrganisationAggregate.IsActive.Should().BeTrue();
        }

        [Fact]
        public void Disable_ShouldDisable_ForNonDefaultOrganisations()
        {
            // Arrange
            var organisationAggregate = Organisation.CreateNewOrganisation(
                this.tenantId, this.tenantAlias, this.tenantName, null, this.performingUserId, this.clock.GetCurrentInstant());

            // Act
            organisationAggregate.Disable(this.performingUserId, this.clock.GetCurrentInstant());

            // Assert
            organisationAggregate.IsActive.Should().BeFalse();
        }

        [Fact]
        public void MarkAsDeleted_ShouldUpdateDeletedProperty_ForNonDefaultOrganisation()
        {
            // Arrange
            var organisationAggregate = Organisation.CreateNewOrganisation(
                this.tenantId, this.tenantAlias, this.tenantName, null, this.performingUserId, this.clock.GetCurrentInstant());

            // Act
            organisationAggregate.MarkAsDeleted(this.performingUserId, this.clock.GetCurrentInstant());

            // Assert
            organisationAggregate.IsDeleted.Should().BeTrue();
        }

        [Fact]
        public void UpdateTenant_ShouldChangeOrganisationTenant()
        {
            // Arrange
            var organisationAggregate = Organisation.CreateNewOrganisation(
                this.tenantId, this.tenantAlias, this.tenantName, null, this.performingUserId, this.clock.GetCurrentInstant());

            // Act
            organisationAggregate.UpdateTenant(this.updatedTenantId, this.performingUserId, this.clock.GetCurrentInstant());

            // Assert
            organisationAggregate.TenantId.Should().Be(this.updatedTenantId);
        }

        [Fact]
        public void UpdateName_ShouldChangeOrganisationName()
        {
            // Arrange
            var organisationAggregate = Organisation.CreateNewOrganisation(
                this.tenantId, this.tenantAlias, this.tenantName, null, this.performingUserId, this.clock.GetCurrentInstant());

            // Act
            organisationAggregate.UpdateName(this.updatedTenantName, this.performingUserId, this.clock.GetCurrentInstant());

            // Assert
            organisationAggregate.Name.Should().Be(this.updatedTenantName);
        }

        [Fact]
        public void UpdateAlias_ShouldChangeOrganisationAlias()
        {
            // Arrange
            var organisationAggregate = Organisation.CreateNewOrganisation(
                this.tenantId, this.tenantAlias, this.tenantName, null, this.performingUserId, this.clock.GetCurrentInstant());

            // Act
            organisationAggregate.UpdateAlias(this.updatedTenantAlias, this.performingUserId, this.clock.GetCurrentInstant());

            // Assert
            organisationAggregate.Alias.Should().Be(this.updatedTenantAlias);
        }

        [Fact]
        public void TenantSetDefaultOrgansation_ShouldMarkTheOrganisationAsDefault()
        {
            // Arrange
            var tenant = new Tenant(
                this.tenantId,
                this.tenantName,
                this.tenantAlias,
                null,
                default,
                default,
                SystemClock.Instance.GetCurrentInstant());

            var organisationAggregate = Organisation
                .CreateNewOrganisation(
                    this.tenantId,
                    this.tenantAlias,
                    this.tenantName,
                    null, this.performingUserId,
                    this.clock.GetCurrentInstant());

            // Act
            tenant.SetDefaultOrganisation(
                organisationAggregate.Id, this.clock.GetCurrentInstant().Plus(Duration.FromMinutes(1)));

            // Assert
            tenant.Details.DefaultOrganisationId.Should().Be(organisationAggregate.Id);
            organisationAggregate.IsActive.Should().BeTrue();
            organisationAggregate.IsDeleted.Should().BeFalse();
        }
    }
}
