// <copyright file="RoleTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

#pragma warning disable SA1600 // Elements should be documented
#pragma warning disable SA1649 // Elements should be documented
#pragma warning disable SA1402 // Elements should be documented
#pragma warning disable SA1516 // Elements should be documented

namespace UBind.Domain.Tests
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using FluentAssertions;
    using NodaTime;
    using UBind.Domain.Entities;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Permissions;
    using UBind.Domain.Tests.Fakes;
    using UBind.Domain.Tests.Helpers;
    using Xunit;

    public class RoleTests
    {
        private readonly Tenant clientTenant = TenantFactory.Create();
        private IClock clock = SystemClock.Instance;

        public RoleTests()
        {
            var defaultRolePermissionsRegistry = new DefaultRolePermissionsRegistry();
            var defaultRoleNameRegistry = new DefaultRoleNameRegistry();
            var roleTypePermissionsRegistry = new RoleTypePermissionsRegistry();
            Role.SetDefaultRolePermissionsRegistry(defaultRolePermissionsRegistry);
            Role.SetDefaultRoleNameRegistry(defaultRoleNameRegistry);
            PermissionExtensions.SetDefaultRolePermissionsRegistry(defaultRolePermissionsRegistry);
            PermissionExtensions.SetRoleTypePermissionsRegistry(roleTypePermissionsRegistry);
        }

        [Fact]
        public void CreateCustomerRole_Throws_WhenTenantIdIsMasterTenant()
        {
            // Act
            Action act = () => RoleHelper.CreateCustomerRole(Tenant.MasterTenantId, Guid.NewGuid(), this.clock.Now());

            // Assert
            act.Should().Throw<ErrorException>().And.Error.Code.Should().Be("master.tenant.cannot.create.customer.role");
        }

        [Fact]
        public void CreateCustomerRole_DoesNotThrow_WhenTenantIdIsNotMasterTenant()
        {
            // Act
            var role = RoleHelper.CreateCustomerRole(
                this.clientTenant.Id, this.clientTenant.Details.DefaultOrganisationId, this.clock.Now());

            // Assert
            Assert.Equal(this.clientTenant.Id, role.TenantId);
            Assert.Equal(this.clientTenant.Details.DefaultOrganisationId, role.OrganisationId);
        }

        [Fact]
        public void AddPermission_ThrowsException_WhenRoleIsUBindAdmin()
        {
            // Arrange
            var role = RoleHelper.CreateMasterAdminRole(this.clock.Now());

            // Act
            Action act = () => role.AddPermission(Permission.ViewMyAccount, this.clock.Now());

            // Assert
            act.Should().Throw<ErrorException>().And.Error.Code.Should().Be("cannot.update.default.role");
        }

        [Fact]
        public void AddPermission_ThrowsException_WhenRoleIsClientAdmin()
        {
            // Arrange
            var tenant = TenantFactory.Create();
            var role = RoleHelper.CreateTenantAdminRole(tenant.Id, tenant.Details.DefaultOrganisationId, this.clock.Now());

            // Act
            Action act = () => role.AddPermission(Permission.ViewMyAccount, this.clock.Now());

            // Assert
            act.Should().Throw<ErrorException>().And.Error.Code.Should().Be("cannot.update.default.role");
        }

        [Fact]
        public void AddPermission_ThrowsException_WhenPermissionIsManageClientAdminUsers()
        {
            // Arrange
            var tenant = TenantFactory.Create();
            var role = RoleHelper.CreateTenantAdminRole(tenant.Id, tenant.Details.DefaultOrganisationId, this.clock.Now());

            // Act
            Action act = () => role.AddPermission(Permission.ManageTenantAdminUsers, this.clock.Now());

            // Assert
            act.Should().Throw<ErrorException>().And.Error.Code.Should().Be("role.permission.cannot.be.added");
        }

        [Fact]
        public void AddPermission_ThrowsException_WhenPermissionIsManageOrganisationAdminUsers()
        {
            // Arrange
            var tenant = TenantFactory.Create();
            var role = RoleHelper.CreateTenantAdminRole(tenant.Id, tenant.Details.DefaultOrganisationId, this.clock.Now());

            // Act
            Action act = () => role.AddPermission(Permission.ManageOrganisationAdminUsers, this.clock.Now());

            // Assert
            act.Should().Throw<ErrorException>().And.Error.Code.Should().Be("role.permission.cannot.be.added");
        }

        [Fact]
        public void AddPermission_Succeeds_ForCustomerRole()
        {
            // Arrange
            var role = RoleHelper.CreateCustomerRole(
                this.clientTenant.Id, this.clientTenant.Details.DefaultOrganisationId, this.clock.Now());
            role.RemovePermission(Permission.ViewMyAccount, this.clock.Now());

            // Act
            role.AddPermission(Permission.ViewMyAccount, this.clock.Now());

            // Assert
            Assert.Contains(Permission.ViewMyAccount, role.Permissions);
        }

        [Fact]
        public void AddPermission_Succeeds_ForBrokerRole()
        {
            // Arrange
            var role = RoleHelper.CreateClientBrokerRole(
                this.clientTenant.Id, this.clientTenant.Details.DefaultOrganisationId, this.clock.Now());
            role.RemovePermission(Permission.ViewMyAccount, this.clock.Now());

            // Act
            role.AddPermission(Permission.ViewMyAccount, this.clock.Now());

            // Assert
            Assert.Contains(Permission.ViewMyAccount, role.Permissions);
        }

        [Fact]
        public void AddPermission_Succeeds_ForClaimsAgentRole()
        {
            // Arrange
            var role = RoleHelper.CreateClientClaimsAgentRole(
                this.clientTenant.Id, this.clientTenant.Details.DefaultOrganisationId, this.clock.Now());
            role.RemovePermission(Permission.ViewMyAccount, this.clock.Now());

            // Act
            role.AddPermission(Permission.ViewMyAccount, this.clock.Now());

            // Assert
            Assert.Contains(Permission.ViewMyAccount, role.Permissions);
        }

        [Fact]
        public void AddPermission_Succeeds_ForUnderwriterRole()
        {
            // Arrange
            var role = RoleHelper.CreateClientUnderwriterRole(
                this.clientTenant.Id, this.clientTenant.Details.DefaultOrganisationId, this.clock.Now());
            role.RemovePermission(Permission.ViewMyAccount, this.clock.Now());

            // Act
            role.AddPermission(Permission.ViewMyAccount, this.clock.Now());

            // Assert
            Assert.Contains(Permission.ViewMyAccount, role.Permissions);
        }

        [Fact]
        public void AddPermission_Succeeds_ForClientProductDeveloperRole()
        {
            // Arrange
            var role = RoleHelper.CreateClientProductDeveloperRole(
                this.clientTenant.Id, this.clientTenant.Details.DefaultOrganisationId, this.clock.Now());
            role.RemovePermission(Permission.ViewMyAccount, this.clock.Now());

            // Act
            role.AddPermission(Permission.ViewMyAccount, this.clock.Now());

            // Assert
            Assert.Contains(Permission.ViewMyAccount, role.Permissions);
        }

        [Fact]
        public void AddPermission_Succeeds_ForValidPermissionForRegularUBindRole()
        {
            // Arrange
            var role = RoleHelper.CreateRole(Tenant.MasterTenantId, Guid.NewGuid(), "My Role", "My role description", this.clock.Now());

            // Act
            role.AddPermission(Permission.ViewMyAccount, this.clock.Now());

            // Assert
            Assert.Contains(Permission.ViewMyAccount, role.Permissions);
        }

        [Fact]
        public void AddPermission_Succeeds_ForValidPermissionForRegularClientRole()
        {
            // Arrange
            var role = RoleHelper.CreateRole(
                this.clientTenant.Id,
                this.clientTenant.Details.DefaultOrganisationId,
                "My Role",
                "My role description",
                this.clock.Now());

            // Act
            role.AddPermission(Permission.ViewMyAccount, this.clock.Now());

            // Assert
            Assert.Contains(Permission.ViewMyAccount, role.Permissions);
        }

        [Fact]
        public void AddPermission_Throws_ForInvalidPermissionForRegularUBindRole()
        {
            // Arrange
            var role = RoleHelper.CreateRole(Tenant.MasterTenantId, Guid.NewGuid(), "My Role", "My role description", this.clock.Now());

            // Act
            Action act = () => role.AddPermission(Permission.ExportQuotes, this.clock.Now());

            // Assert
            act.Should().Throw<ErrorException>().And.Error.Code.Should().Be("cannot.assign.permission.to.role.of.type");
        }

        [Fact]
        public void AddPermission_Throws_ForInvalidPermissionForRegularClientRole()
        {
            // Arrange
            var role = RoleHelper.CreateRole(
                this.clientTenant.Id,
                this.clientTenant.Details.DefaultOrganisationId,
                "My Role",
                "My role description",
                this.clock.Now());

            // Act
            Action act = () => role.AddPermission(Permission.ViewTenants, this.clock.Now());

            // Assert
            act.Should().Throw<ErrorException>().And.Error.Code.Should().Be("cannot.assign.permission.to.role.of.type");
        }

        [Fact]
        public void RemovePermission_ThrowsException_WhenRoleIsUBindAdmin()
        {
            // Arrange
            var role = RoleHelper.CreateMasterAdminRole(this.clock.Now());

            // Act
            Action act = () => role.RemovePermission(Permission.ViewMyAccount, this.clock.Now());

            // Assert
            act.Should().Throw<ErrorException>().And.Error.Code.Should().Be("cannot.update.default.role");
        }

        [Fact]
        public void RemovePermission_ThrowsException_WhenRoleIsClientAdmin()
        {
            // Arrange
            var role = RoleHelper.CreateTenantAdminRole(TenantFactory.DefaultId, Guid.NewGuid(), this.clock.Now());

            // Act
            Action act = () => role.RemovePermission(Permission.ViewMyAccount, this.clock.Now());

            // Assert
            act.Should().Throw<ErrorException>().And.Error.Code.Should().Be("cannot.update.default.role");
        }

        [Fact]
        public void RemovePermission_ThrowsException_WhenCustomerRoleDoesNotHavePermission()
        {
            // Arrange
            var role = RoleHelper.CreateCustomerRole(
                this.clientTenant.Id, this.clientTenant.Details.DefaultOrganisationId, this.clock.Now());
            role.RemovePermission(Permission.ViewMyAccount, this.clock.Now());

            // Act
            Action act = () => role.RemovePermission(Permission.ViewMyAccount, this.clock.Now());

            // Assert
            act.Should().Throw<ErrorException>().And.Error.Code.Should().Be("role.removal.permission.not.found");
        }

        [Fact]
        public void RemovePermission_ThrowsException_WhenRegularUBinddRoleDoesNotHavePermission()
        {
            // Arrange
            var role = RoleHelper.CreateRole(Tenant.MasterTenantId, Guid.NewGuid(), "foo", "bar", this.clock.Now());

            // Act
            Action act = () => role.RemovePermission(Permission.ViewMyAccount, this.clock.Now());

            // Assert
            act.Should().Throw<ErrorException>().And.Error.Code.Should().Be("role.removal.permission.not.found");
        }

        [Fact]
        public void RemovePermission_ThrowsException_WhenRegularClientRoleDoesNotHavePermission()
        {
            // Arrange
            var role = RoleHelper.CreateRole(
                this.clientTenant.Id, this.clientTenant.Details.DefaultOrganisationId, "foo", "bar", this.clock.Now());

            // Act
            Action act = () => role.RemovePermission(Permission.ViewMyAccount, this.clock.Now());

            // Assert
            act.Should().Throw<ErrorException>().And.Error.Code.Should().Be("role.removal.permission.not.found");
        }

        [Fact]
        public void RemovePermission_Succeeds_WhenRegularUBindRoleHasPermission()
        {
            // Arrange
            var role = RoleHelper.CreateRole(Tenant.MasterTenantId, Guid.NewGuid(), "foo", "bar", this.clock.Now());
            role.AddPermission(Permission.ViewMyAccount, this.clock.Now());

            // Act
            role.RemovePermission(Permission.ViewMyAccount, this.clock.Now());

            // Assert
            Assert.DoesNotContain(Permission.ViewMyAccount, role.Permissions);
        }

        [Fact]
        public void RemovePermission_Succeeds_WhenRegularClientRoleHasPermission()
        {
            // Arrange
            var role = RoleHelper.CreateRole(
                this.clientTenant.Id, this.clientTenant.Details.DefaultOrganisationId, "foo", "bar", this.clock.Now());
            role.AddPermission(Permission.ViewMyAccount, this.clock.Now());

            // Act
            role.RemovePermission(Permission.ViewMyAccount, this.clock.Now());

            // Assert
            Assert.DoesNotContain(Permission.ViewMyAccount, role.Permissions);
        }

        [Fact]
        public void RemovePermission_Succeeds_ForCustomerRoleWhenRoleHasPermission()
        {
            // Arrange
            var role = RoleHelper.CreateCustomerRole(
                this.clientTenant.Id, this.clientTenant.Details.DefaultOrganisationId, this.clock.Now());

            // Act
            role.RemovePermission(Permission.ViewMyAccount, this.clock.Now());

            // Assert
            Assert.DoesNotContain(Permission.ViewMyAccount, role.Permissions);
        }

        [Fact]
        public void Update_ThrowsException_WhenRoleIsUBindAdmin()
        {
            // Arrange
            var role = RoleHelper.CreateMasterAdminRole(this.clock.Now());

            // Act
            Action act = () => role.Update("foo", "bar", this.clock.Now());

            // Assert
            act.Should().Throw<ErrorException>().And.Error.Code.Should().Be("cannot.update.default.role");
        }

        [Fact]
        public void Update_ThrowsException_WhenRoleIsClientAdmin()
        {
            // Arrange
            var role = RoleHelper.CreateTenantAdminRole(
                this.clientTenant.Id, this.clientTenant.Details.DefaultOrganisationId, this.clock.Now());

            // Act
            Action act = () => role.Update("foo", "bar", this.clock.Now());

            // Assert
            act.Should().Throw<ErrorException>().And.Error.Code.Should().Be("cannot.update.default.role");
        }

        [Fact]
        public void Update_ThrowsException_WhenRoleIsCustomer()
        {
            // Arrange
            var role = RoleHelper.CreateCustomerRole(
                this.clientTenant.Id, this.clientTenant.Details.DefaultOrganisationId, this.clock.Now());

            // Act
            Action act = () => role.Update("foo", "bar", this.clock.Now());

            // Assert
            act.Should().Throw<ErrorException>().And.Error.Code.Should().Be("cannot.update.default.role");
        }

        [Fact]
        public void Update_Succeeds_WhenRoleIsRegularUBindRole()
        {
            // Arrange
            var role = RoleHelper.CreateRole(
                this.clientTenant.Id, this.clientTenant.Details.DefaultOrganisationId, "foo", "bar", this.clock.Now());
            const string newName = "newName";
            const string newDescription = "newDescription";

            // Act
            role.Update(newName, newDescription, this.clock.Now());

            // Assert
            Assert.Equal(newName, role.Name);
            Assert.Equal(newDescription, role.Description);
        }

        private void Invoke(bool condition, Action @true, Action @false)
        {
            var action = condition ? @true : @false;
            action();
        }

        public class DefaultRoleTestData : IEnumerable<object[]>
        {
            public IEnumerator<object[]> GetEnumerator()
            {
                yield return new object[] { RoleHelper.CreateMasterAdminRole(SystemClock.Instance.Now()) };
                yield return new object[] { RoleHelper.CreateUBindProductDeveloperRole(SystemClock.Instance.Now()) };
                yield return new object[] { RoleHelper.CreateTenantAdminRole(TenantFactory.DefaultId, Guid.NewGuid(), SystemClock.Instance.Now()) };
                yield return new object[] { RoleHelper.CreateClientBrokerRole(TenantFactory.DefaultId, Guid.NewGuid(), SystemClock.Instance.Now()) };
                yield return new object[] { RoleHelper.CreateClientClaimsAgentRole(TenantFactory.DefaultId, Guid.NewGuid(), SystemClock.Instance.Now()) };
                yield return new object[] { RoleHelper.CreateClientProductDeveloperRole(TenantFactory.DefaultId, Guid.NewGuid(), SystemClock.Instance.Now()) };
                yield return new object[] { RoleHelper.CreateCustomerRole(TenantFactory.DefaultId, Guid.NewGuid(), SystemClock.Instance.Now()) };
                yield return new object[] { RoleHelper.CreateClientUnderwriterRole(TenantFactory.DefaultId, Guid.NewGuid(), SystemClock.Instance.Now()) };
            }

            IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
        }

        public class DefaultPermission
        {
            public DefaultPermission(
                                     RoleInformationAttribute roleInfo,
                                     bool viewMyAccount,
                                     bool editMyAccount,
                                     bool viewUsers,
                                     bool manageUsers,
                                     bool manageClientAdminUsers,
                                     bool manageMasterAdminUsers,
                                     bool viewRoles,
                                     bool manageRoles,
                                     bool viewQuotes,
                                     bool manageQuotes,
                                     bool endorseQuotes,
                                     bool reviewQuotes,
                                     bool exportQuotes,
                                     bool viewQuoteVersions,
                                     bool manageQuoteVersions,
                                     bool viewPolicies,
                                     bool adjustPolicies,
                                     bool renewPolicies,
                                     bool cancelPolicies,
                                     bool exportPolicies,
                                     bool importPolicies,
                                     bool viewClaims,
                                     bool manageClaims,
                                     bool acknowledgeClaimNotifications,
                                     bool assignClaimNumbers,
                                     bool reviewClaims,
                                     bool assessClaims,
                                     bool settleClaims,
                                     bool exportClaims,
                                     bool importClaims,
                                     bool viewCustomers,
                                     bool manageCustomers,
                                     bool importCustomers,
                                     bool viewEmails,
                                     bool manageEmails,
                                     bool viewReports,
                                     bool manageReports,
                                     bool generateReports,
                                     bool accessDevelopmentData,
                                     bool accessStagingData,
                                     bool accessProductionData,
                                     bool viewTenants,
                                     bool manageTenants,
                                     bool viewProducts,
                                     bool manageProducts,
                                     bool viewReleases,
                                     bool manageReleases,
                                     bool promoteReleasestoStaging,
                                     bool promoteReleasestoProduction,
                                     bool viewPortals,
                                     bool managePortals,
                                     bool viewBackgroundJobs,
                                     bool manageBackgroundJobs,
                                     bool replayIntegrationEvents)
            {
                this.RoleInfo = roleInfo;
                this.ViewMyAccount = viewMyAccount;
                this.EditMyAccount = editMyAccount;
                this.ViewUsers = viewUsers;
                this.ManageUsers = manageUsers;
                this.ManageClientAdminUsers = manageClientAdminUsers;
                this.ManageMasterAdminUsers = manageMasterAdminUsers;
                this.ViewRoles = viewRoles;
                this.ManageRoles = manageRoles;
                this.ViewQuotes = viewQuotes;
                this.ManageQuotes = manageQuotes;
                this.EndorseQuotes = endorseQuotes;
                this.ReviewQuotes = reviewQuotes;
                this.ExportQuotes = exportQuotes;
                this.ViewQuoteVersions = viewQuoteVersions;
                this.ManageQuoteVersions = manageQuoteVersions;
                this.ViewPolicies = viewPolicies;
                this.AdjustPolicies = adjustPolicies;
                this.RenewPolicies = renewPolicies;
                this.CancelPolicies = cancelPolicies;
                this.ExportPolicies = exportPolicies;
                this.ImportPolicies = importPolicies;
                this.ViewClaims = viewClaims;
                this.ManageClaims = manageClaims;
                this.AcknowledgeClaimNotifications = acknowledgeClaimNotifications;
                this.AssignClaimNumbers = assignClaimNumbers;
                this.ReviewClaims = reviewClaims;
                this.AssessClaims = assessClaims;
                this.SettleClaims = settleClaims;
                this.ExportClaims = exportClaims;
                this.ImportClaims = importClaims;
                this.ViewCustomers = viewCustomers;
                this.ManageCustomers = manageCustomers;
                this.ImportCustomers = importCustomers;
                this.ViewEmails = viewEmails;
                this.ManageEmails = manageEmails;
                this.ViewReports = viewReports;
                this.ManageReports = manageReports;
                this.GenerateReports = generateReports;
                this.AccessDevelopmentData = accessDevelopmentData;
                this.AccessStagingData = accessStagingData;
                this.AccessProductionData = accessProductionData;
                this.ViewTenants = viewTenants;
                this.ManageTenants = manageTenants;
                this.ViewProducts = viewProducts;
                this.ManageProducts = manageProducts;
                this.ViewReleases = viewReleases;
                this.ManageReleases = manageReleases;
                this.PromoteReleasesToStaging = promoteReleasestoStaging;
                this.PromoteReleasesToProduction = promoteReleasestoProduction;
                this.ViewPortals = viewPortals;
                this.ManagePortals = managePortals;
                this.ViewBackgroundJobs = viewBackgroundJobs;
                this.ManageBackgroundJobs = manageBackgroundJobs;
                this.ReplayIntegrationEvents = replayIntegrationEvents;
            }

            public RoleInformationAttribute RoleInfo { get; private set; }
            public bool ViewMyAccount { get; private set; }
            public bool EditMyAccount { get; private set; }
            public bool ViewUsers { get; private set; }
            public bool ManageUsers { get; private set; }
            public bool ManageClientAdminUsers { get; private set; }
            public bool ManageMasterAdminUsers { get; private set; }
            public bool ViewRoles { get; private set; }
            public bool ManageRoles { get; private set; }
            public bool ViewQuotes { get; private set; }
            public bool ManageQuotes { get; private set; }
            public bool EndorseQuotes { get; private set; }
            public bool ReviewQuotes { get; private set; }
            public bool ExportQuotes { get; private set; }
            public bool ViewQuoteVersions { get; private set; }
            public bool ManageQuoteVersions { get; private set; }
            public bool ViewPolicies { get; private set; }
            public bool AdjustPolicies { get; private set; }
            public bool RenewPolicies { get; private set; }
            public bool CancelPolicies { get; private set; }
            public bool ExportPolicies { get; private set; }
            public bool ImportPolicies { get; private set; }
            public bool ViewClaims { get; private set; }
            public bool ManageClaims { get; private set; }
            public bool AcknowledgeClaimNotifications { get; private set; }
            public bool AssignClaimNumbers { get; private set; }
            public bool ReviewClaims { get; private set; }
            public bool AssessClaims { get; private set; }
            public bool SettleClaims { get; private set; }
            public bool ExportClaims { get; private set; }
            public bool ImportClaims { get; private set; }
            public bool ViewCustomers { get; private set; }
            public bool ManageCustomers { get; private set; }
            public bool ImportCustomers { get; private set; }
            public bool ViewEmails { get; private set; }
            public bool ManageEmails { get; private set; }
            public bool ViewReports { get; private set; }
            public bool ManageReports { get; private set; }
            public bool GenerateReports { get; private set; }
            public bool AccessDevelopmentData { get; private set; }
            public bool AccessStagingData { get; private set; }
            public bool AccessProductionData { get; private set; }
            public bool ViewTenants { get; private set; }
            public bool ManageTenants { get; private set; }
            public bool ViewProducts { get; private set; }
            public bool ManageProducts { get; private set; }
            public bool ViewReleases { get; private set; }
            public bool ManageReleases { get; private set; }
            public bool PromoteReleasesToStaging { get; private set; }
            public bool PromoteReleasesToProduction { get; private set; }
            public bool ViewPortals { get; private set; }
            public bool ManagePortals { get; private set; }
            public bool ViewBackgroundJobs { get; private set; }
            public bool ManageBackgroundJobs { get; private set; }
            public bool ReplayIntegrationEvents { get; private set; }
        }
    }
}
