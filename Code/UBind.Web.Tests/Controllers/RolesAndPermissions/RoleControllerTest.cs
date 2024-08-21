// <copyright file="RoleControllerTest.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

// CS1591 Missing XML comment for publicly visible type or member
// Suppress CS1591. The unit test method must be named correctly instead of adding a comment in it.
#pragma warning disable CS1591

namespace UBind.Web.Tests.Controllers.RolesAndPermissions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.AspNetCore.Routing;
    using Moq;
    using UBind.Application;
    using UBind.Application.Authorisation;
    using UBind.Application.Infrastructure;
    using UBind.Application.Queries.FeatureSettings;
    using UBind.Application.User;
    using UBind.Domain;
    using UBind.Domain.Entities;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Permissions;
    using UBind.Web.Controllers.Portal;
    using UBind.Web.ResourceModels;
    using Xunit;

    /// <summary>
    /// Defines the <see cref="RoleControllerTest" />.
    /// </summary>
    public class RoleControllerTest
    {
        private readonly Mock<ICqrsMediator> mockMediator;
        private ClaimsPrincipal testUser;
        private RoleController rolesController;
        private Mock<ICachingResolver> mockcachingResolver = new Mock<ICachingResolver>();

        public RoleControllerTest()
        {
            this.mockMediator = new Mock<ICqrsMediator>();
            this.testUser = new ClaimsPrincipal();

            this.rolesController = new RoleController(null, null, null, null, null, null, null, null);

            var defaultRolePermissionsRegistry = new DefaultRolePermissionsRegistry();
            var defaultRoleNameRegistry = new DefaultRoleNameRegistry();
            var roleTypePermissionsRegistry = new RoleTypePermissionsRegistry();
            Role.SetDefaultRolePermissionsRegistry(defaultRolePermissionsRegistry);
            Role.SetDefaultRoleNameRegistry(defaultRoleNameRegistry);
            PermissionExtensions.SetDefaultRolePermissionsRegistry(defaultRolePermissionsRegistry);
            PermissionExtensions.SetRoleTypePermissionsRegistry(roleTypePermissionsRegistry);
        }

        /// <summary>
        /// The Get_Roles_Success.
        /// </summary>
        [Fact]
        public void Get_Roles_Success()
        {
            string[] userPermissions = new string[]
            {
                Permission.ManageUsers.ToString(),
            };

            // Assert
            Assert.True(AuthorizationTest.IsAuthorized(
                this.rolesController,
                "Get",
                new Type[] { typeof(RoleQueryOptionsModel) },
                userPermissions,
                null));
        }

        /// <summary>
        /// The Get_Roles_Forbidden.
        /// </summary>
        [Fact]
        public void Get_Roles_Forbidden()
        {
            string[] userPermissions = new string[]
            {
                Permission.ViewUsers.ToString(),
            };

            // Assert
            Assert.False(AuthorizationTest.IsAuthorized(
                this.rolesController,
                "Get",
                new Type[] { typeof(RoleQueryOptionsModel) },
                userPermissions,
                null));
        }

        /// <summary>
        /// The Get_RoleById_Success.
        /// </summary>
        [Fact]
        public void Get_RoleById_Success()
        {
            string[] userPermissions = new string[]
            {
                Permission.ViewRoles.ToString(),
            };

            // Assert
            Assert.True(AuthorizationTest.IsAuthorized(
                this.rolesController,
                "GetRoleById",
                new Type[] { typeof(Guid), typeof(string) },
                userPermissions,
                null));
        }

        /// <summary>
        /// The Get_RoleById_Forbidden.
        /// </summary>
        [Fact]
        public void Get_RoleById_Forbidden()
        {
            string[] userPermissions = new string[]
            {
                Permission.ViewUsers.ToString(),
            };

            // Assert
            Assert.False(AuthorizationTest.IsAuthorized(
                this.rolesController,
                "GetRoleById",
                new Type[] { typeof(Guid), typeof(string) },
                userPermissions,
                null));
        }

        /// <summary>
        /// The Post_Roles_Success.
        /// </summary>
        [Fact]
        public void Post_Roles_Success()
        {
            string[] userPermissions = new string[]
            {
                Permission.ManageRoles.ToString(),
            };

            // Assert
            Assert.True(AuthorizationTest.IsAuthorized(
                this.rolesController,
                "Post",
                new Type[] { typeof(RoleUpdateModel) },
                userPermissions,
                null));
        }

        /// <summary>
        /// The Post_Roles_Forbidden.
        /// </summary>
        [Fact]
        public void Post_Roles_Forbidden()
        {
            string[] userPermissions = new string[]
            {
                Permission.ManageReleases.ToString(),
            };

            // Assert
            Assert.False(AuthorizationTest.IsAuthorized(
                this.rolesController,
                "Post",
                new Type[] { typeof(RoleUpdateModel) },
                userPermissions,
                null));
        }

        /// <summary>
        /// The Put_Roles_Success.
        /// </summary>
        [Fact]
        public void Put_Roles_Success()
        {
            string[] userPermissions = new string[]
            {
                Permission.ManageRoles.ToString(),
            };

            // Assert
            Assert.True(AuthorizationTest.IsAuthorized(
                this.rolesController,
                "Put",
                new Type[] { typeof(Guid), typeof(RoleUpdateModel) },
                userPermissions,
                null));
        }

        /// <summary>
        /// The Put_Roles_Forbidden.
        /// </summary>
        [Fact]
        public void Put_Roles_Forbidden()
        {
            string[] userPermissions = new string[]
            {
                Permission.ManageReleases.ToString(),
            };

            // Assert
            Assert.False(AuthorizationTest.IsAuthorized(
                this.rolesController,
                "Put",
                new Type[] { typeof(Guid), typeof(RoleUpdateModel) },
                userPermissions,
                null));
        }

        /// <summary>
        /// The Delete_Roles_Success.
        /// </summary>
        [Fact]
        public void Delete_Roles_Success()
        {
            string[] userPermissions = new string[]
            {
                Permission.ManageRoles.ToString(),
            };

            // Assert
            Assert.True(AuthorizationTest.IsAuthorized(
                this.rolesController,
                "Delete",
                new Type[] { typeof(Guid) },
                userPermissions,
                null));
        }

        /// <summary>
        /// The Delete_Roles_Forbidden.
        /// </summary>
        [Fact]
        public void Delete_Roles_Forbidden()
        {
            string[] userPermissions = new string[]
            {
                Permission.ManageReleases.ToString(),
            };

            // Assert
            Assert.False(AuthorizationTest.IsAuthorized(
                this.rolesController,
                "Delete",
                new Type[] { typeof(Guid) },
                userPermissions,
                null));
        }

        /// <summary>
        /// The Post_AssignPermissionToRole_Success.
        /// </summary>
        [Fact]
        public void Post_AssignPermissionToRole_Success()
        {
            string[] userPermissions = new string[]
            {
                Permission.ManageRoles.ToString(),
            };

            // Assert
            Assert.True(AuthorizationTest.IsAuthorized(
                this.rolesController,
                "AssignPermission",
                new Type[] { typeof(Guid), typeof(Permission) },
                userPermissions,
                null));
        }

        /// <summary>
        /// The Post_AssignPermissionToRole_Forbidden.
        /// </summary>
        [Fact]
        public void Post_AssignPermissionToRole_Forbidden()
        {
            string[] userPermissions = new string[]
            {
                Permission.ManageReports.ToString(),
            };

            // Assert
            Assert.False(AuthorizationTest.IsAuthorized(
                this.rolesController,
                "AssignPermission",
                new Type[] { typeof(Guid), typeof(Permission) },
                userPermissions,
                null));
        }

        /// <summary>
        /// The Delete_RetractPermissionToRole_Success.
        /// </summary>
        [Fact]
        public void Delete_RetractPermissionToRole_Success()
        {
            string[] userPermissions = new string[]
            {
                Permission.ManageRoles.ToString(),
            };

            // Assert
            Assert.True(AuthorizationTest.IsAuthorized(
                this.rolesController,
                "AssignPermission",
                new Type[] { typeof(Guid), typeof(Permission) },
                userPermissions,
                null));
        }

        /// <summary>
        /// The Delete_RetractPermissionToRole_Forbidden.
        /// </summary>
        [Fact]
        public void Delete_RetractPermissionToRole_Forbidden()
        {
            string[] userPermissions = new string[]
            {
                Permission.ManageReports.ToString(),
            };

            // Assert
            Assert.False(AuthorizationTest.IsAuthorized(
                this.rolesController,
                "AssignPermission",
                new Type[] { typeof(Guid), typeof(Permission) },
                userPermissions,
                null));
        }

        /// <summary>
        /// The Get_UserRoles_Forbidden.
        /// </summary>
        [Fact]
        public void Get_UserRoles_Forbidden()
        {
            string[] userPermissions = new string[]
            {
                Permission.ManageQuoteVersions.ToString(),
            };

            // Assert
            Assert.False(AuthorizationTest.IsAuthorized(
                this.rolesController,
                "GetUserRoles",
                new Type[] { typeof(Guid), typeof(string) },
                userPermissions,
                null));
        }

        /// <summary>
        /// The Put_AssignRoleToUser_Success.
        /// </summary>
        [Fact]
        public void Put_AssignRoleToUser_Success()
        {
            string[] userPermissions = new string[]
            {
                Permission.ManageUsers.ToString(),
            };

            // Assert
            Assert.True(AuthorizationTest.IsAuthorized(
                this.rolesController,
                "AssignRoleToUser",
                new Type[] { typeof(Guid), typeof(Guid), typeof(string) },
                userPermissions,
                null));
        }

        /// <summary>
        /// The Put_AssignRoleToUser_Forbidden.
        /// </summary>
        [Fact]
        public void Put_AssignRoleToUser_Forbidden()
        {
            string[] userPermissions = new string[]
            {
                Permission.ManageReports.ToString(),
            };

            // Assert
            Assert.False(AuthorizationTest.IsAuthorized(
                this.rolesController,
                "AssignRoleToUser",
                new Type[] { typeof(Guid), typeof(Guid), typeof(string) },
                userPermissions,
                null));
        }

        /// <summary>
        /// The Delete_RemoveRoleFromUser_Success.
        /// </summary>
        [Fact]
        public void Delete_RemoveRoleFromUser_Success()
        {
            string[] userPermissions = new string[]
            {
                Permission.ManageUsers.ToString(),
            };

            // Assert
            Assert.True(AuthorizationTest.IsAuthorized(
                this.rolesController,
                "RemoveRoleFromUser",
                new Type[] { typeof(Guid), typeof(Guid), typeof(string) },
                userPermissions,
                null));
        }

        /// <summary>
        /// The Delete_RemoveRoleFromUser_Forbidden.
        /// </summary>
        [Fact]
        public void Delete_RemoveRoleFromUser_Forbidden()
        {
            string[] userPermissions = new string[]
            {
                Permission.ManageReports.ToString(),
            };

            // Assert
            Assert.False(AuthorizationTest.IsAuthorized(
                this.rolesController,
                "RemoveRoleFromUser",
                new Type[] { typeof(Guid), typeof(Guid), typeof(string) },
                userPermissions,
                null));
        }

        [Fact]
        public void GetPermissions_Should_Return_CorrectPermission_When_RoleType_Is_Client()
        {
            // Arrange
            var mockRoleService = new Mock<IRoleService>();
            var mockUserService = new Mock<IUserService>();
            var mockSettingService = new Mock<IFeatureSettingService>();
            var mockAuthorisationService = new Mock<IAuthorisationService>();
            var mockUserAuthorisationService = new Mock<IUserAuthorisationService>();
            var mockRoleAuthorisationService = new Mock<IRoleAuthorisationService>();
            var mockMediator = new Mock<ICqrsMediator>();
            var httpContext = new Mock<Microsoft.AspNetCore.Http.HttpContext>();
            var userTenantId = Guid.NewGuid();
            httpContext.Setup(m => m.User.FindFirst(ClaimNames.TenantId)).Returns(new System.Security.Claims.Claim(ClaimNames.TenantId, Tenant.MasterTenantId.ToString()));
            httpContext.Setup(m => m.User.FindFirst(ClaimNames.TenantId)).Returns(new System.Security.Claims.Claim(ClaimNames.TenantId, userTenantId.ToString()));

            var context = new ControllerContext(new ActionContext(httpContext.Object, new RouteData(), new ControllerActionDescriptor()));
            var roleController = new RoleController(
                mockRoleService.Object,
                mockUserService.Object,
                new RoleTypePermissionsRegistry(),
                mockAuthorisationService.Object,
                mockMediator.Object,
                this.mockcachingResolver.Object,
                mockUserAuthorisationService.Object,
                mockRoleAuthorisationService.Object)
            {
                ControllerContext = context,
            };

            mockMediator.Setup(m => m.Send(It.IsAny<UserHasActiveFeatureSettingQuery>(), CancellationToken.None))
                .ReturnsAsync(true);

            // Act
            var response = roleController.GetPermissions(RoleType.Client);

            // Assert
            var result = response as OkObjectResult;
            var permissions = (result.Value as IEnumerable<PermissionModel>).Select(c => c.Type);
            permissions.Should().NotBeNull();
            permissions.Should().NotBeEmpty();

            Assert.Contains(Permission.ViewMyAccount, permissions);
            Assert.Contains(Permission.EditMyAccount, permissions);

            // users related permissions
            Assert.Contains(Permission.ViewUsers, permissions);
            Assert.Contains(Permission.ManageUsers, permissions);
            Assert.Contains(Permission.ManageTenantAdminUsers, permissions);
            Assert.DoesNotContain(Permission.ManageMasterAdminUsers, permissions);

            // roles related permissions
            Assert.Contains(Permission.ViewRoles, permissions);
            Assert.Contains(Permission.ManageRoles, permissions);

            // tenants related permissions
            Assert.DoesNotContain(Permission.ViewTenants, permissions);
            Assert.DoesNotContain(Permission.ManageTenants, permissions);

            // products related permissions
            Assert.Contains(Permission.ViewProducts, permissions);
            Assert.Contains(Permission.ManageProducts, permissions);

            // releases related permissions
            Assert.Contains(Permission.ViewReleases, permissions);
            Assert.Contains(Permission.ManageReleases, permissions);
            Assert.Contains(Permission.PromoteReleasesToStaging, permissions);
            Assert.Contains(Permission.PromoteReleasesToProduction, permissions);

            // portals related permissions
            ////Assert.DoesNotContain(Permission.ViewPortals, permissions);
            ////Assert.DoesNotContain(Permission.ManagePortals, permissions);

            // background jobs related permissions
            Assert.Contains(Permission.ViewBackgroundJobs, permissions);
            Assert.Contains(Permission.ManageBackgroundJobs, permissions);

            // integration related permissions
            ////Assert.DoesNotContain(Permission.ReplayIntegrationEvents, permissions);

            // quote related pemissions
            Assert.Contains(Permission.ViewQuotes, permissions);
            Assert.Contains(Permission.ManageQuotes, permissions);
            Assert.Contains(Permission.EndorseQuotes, permissions);
            Assert.Contains(Permission.ReviewQuotes, permissions);
            Assert.Contains(Permission.ExportQuotes, permissions);
            Assert.Contains(Permission.ManageQuoteVersions, permissions);
            Assert.Contains(Permission.ViewQuoteVersions, permissions);

            // policies related permissions
            Assert.Contains(Permission.ViewPolicies, permissions);
            Assert.Contains(Permission.ManagePolicies, permissions);
            Assert.Contains(Permission.ExportPolicies, permissions);
            Assert.Contains(Permission.ImportPolicies, permissions);

            // claims related permissions
            Assert.Contains(Permission.ViewClaims, permissions);
            Assert.Contains(Permission.ManageClaims, permissions);
            Assert.Contains(Permission.AcknowledgeClaimNotifications, permissions);
            Assert.Contains(Permission.ReviewClaims, permissions);
            Assert.Contains(Permission.AssessClaims, permissions);
            Assert.Contains(Permission.SettleClaims, permissions);
            Assert.Contains(Permission.ExportClaims, permissions);
            Assert.Contains(Permission.ImportClaims, permissions);

            // customer related permissions
            Assert.Contains(Permission.ViewCustomers, permissions);
            Assert.Contains(Permission.ManageCustomers, permissions);
            Assert.Contains(Permission.ImportCustomers, permissions);

            // emails related permissions
            Assert.Contains(Permission.ViewMessages, permissions);
            Assert.Contains(Permission.ManageMessages, permissions);

            // no reports related permissions
            Assert.Contains(Permission.ViewReports, permissions);
            Assert.Contains(Permission.ManageReports, permissions);
            Assert.Contains(Permission.GenerateReports, permissions);

            // environment related permissions
            Assert.Contains(Permission.AccessDevelopmentData, permissions);
            Assert.Contains(Permission.AccessStagingData, permissions);
            Assert.Contains(Permission.AccessProductionData, permissions);
        }

        [Fact]
        public void GetPermissions_Should_Return_CorrectPermission_When_RoleType_Is_Master()
        {
            // Arrange
            var mockRoleService = new Mock<IRoleService>();
            var mockUserService = new Mock<IUserService>();
            var mockSettingService = new Mock<IFeatureSettingService>();
            var mockAuthorisationService = new Mock<IAuthorisationService>();
            var mockUserAuthorisationService = new Mock<IUserAuthorisationService>();
            var mockRoleAuthorisationService = new Mock<IRoleAuthorisationService>();
            var mockMediator = new Mock<ICqrsMediator>();
            var httpContext = new Mock<Microsoft.AspNetCore.Http.HttpContext>();
            var userTenantId = Tenant.MasterTenantAlias;
            httpContext.Setup(m => m.User.FindFirst(ClaimNames.TenantId)).Returns(new System.Security.Claims.Claim(ClaimNames.TenantId, Tenant.MasterTenantId.ToString()));
            httpContext.Setup(m => m.User.FindFirst(ClaimNames.TenantId)).Returns(new System.Security.Claims.Claim(ClaimNames.TenantId, userTenantId));

            var context = new ControllerContext(new ActionContext(httpContext.Object, new RouteData(), new ControllerActionDescriptor()));
            var roleController = new RoleController(
                mockRoleService.Object,
                mockUserService.Object,
                new RoleTypePermissionsRegistry(),
                mockAuthorisationService.Object,
                mockMediator.Object,
                this.mockcachingResolver.Object,
                mockUserAuthorisationService.Object,
                mockRoleAuthorisationService.Object)
            {
                ControllerContext = context,
            };

            this.mockMediator.Setup(m => m.Send(It.IsAny<UserHasActiveFeatureSettingQuery>(), CancellationToken.None))
                .ReturnsAsync(true);

            // Act
            var response = roleController.GetPermissions(RoleType.Master);

            // Assert
            var result = response as OkObjectResult;
            var permissions = (result.Value as IEnumerable<PermissionModel>).Select(c => c.Type);
            permissions.Should().NotBeNull();
            permissions.Should().NotBeEmpty();

            Assert.Contains(Permission.ViewMyAccount, permissions);
            Assert.Contains(Permission.EditMyAccount, permissions);

            // users related permissions
            Assert.Contains(Permission.ViewUsers, permissions);
            Assert.Contains(Permission.ManageUsers, permissions);
            ////Assert.DoesNotContain(Permission.ManageClientAdminUsers, permissions);
            Assert.Contains(Permission.ManageMasterAdminUsers, permissions);

            // roles related permissions
            Assert.Contains(Permission.ViewRoles, permissions);
            Assert.Contains(Permission.ManageRoles, permissions);

            // tenants related permissions
            Assert.Contains(Permission.ViewTenants, permissions);
            Assert.Contains(Permission.ManageTenants, permissions);

            // products related permissions
            Assert.Contains(Permission.ViewProducts, permissions);
            Assert.Contains(Permission.ManageProducts, permissions);

            // releases related permissions
            Assert.Contains(Permission.ViewReleases, permissions);
            Assert.Contains(Permission.ManageReleases, permissions);
            Assert.Contains(Permission.PromoteReleasesToStaging, permissions);
            Assert.Contains(Permission.PromoteReleasesToProduction, permissions);

            // portals related permissions
            Assert.Contains(Permission.ViewPortals, permissions);
            Assert.Contains(Permission.ManagePortals, permissions);

            // background jobs related permissions
            Assert.Contains(Permission.ViewBackgroundJobs, permissions);
            Assert.Contains(Permission.ManageBackgroundJobs, permissions);

            // integration related permissions
            Assert.Contains(Permission.ReplayIntegrationEvents, permissions);

            // quote related pemissions
            Assert.DoesNotContain(Permission.ViewQuotes, permissions);
            Assert.DoesNotContain(Permission.ManageQuotes, permissions);
            Assert.DoesNotContain(Permission.EndorseQuotes, permissions);
            Assert.DoesNotContain(Permission.ReviewQuotes, permissions);
            Assert.DoesNotContain(Permission.ExportQuotes, permissions);
            Assert.DoesNotContain(Permission.ManageQuoteVersions, permissions);
            Assert.DoesNotContain(Permission.ViewQuoteVersions, permissions);

            // policies related permissions
            Assert.DoesNotContain(Permission.ViewPolicies, permissions);
            Assert.DoesNotContain(Permission.ManagePolicies, permissions);
            Assert.DoesNotContain(Permission.ExportPolicies, permissions);
            Assert.DoesNotContain(Permission.ImportPolicies, permissions);

            // claims related permissions
            Assert.DoesNotContain(Permission.ViewClaims, permissions);
            Assert.DoesNotContain(Permission.ManageClaims, permissions);
            Assert.DoesNotContain(Permission.AcknowledgeClaimNotifications, permissions);
            Assert.DoesNotContain(Permission.ReviewClaims, permissions);
            Assert.DoesNotContain(Permission.AssessClaims, permissions);
            Assert.DoesNotContain(Permission.SettleClaims, permissions);
            Assert.DoesNotContain(Permission.ExportClaims, permissions);
            Assert.DoesNotContain(Permission.ImportClaims, permissions);

            // customer related permissions
            Assert.DoesNotContain(Permission.ViewCustomers, permissions);
            Assert.DoesNotContain(Permission.ManageCustomers, permissions);
            Assert.DoesNotContain(Permission.ImportCustomers, permissions);

            // emails related permissions
            Assert.Contains(Permission.ViewAllMessages, permissions);
            Assert.Contains(Permission.ManageAllMessages, permissions);

            // no reports related permissions
            ////Assert.DoesNotContain(Permission.ViewReports, permissions);
            ////Assert.DoesNotContain(Permission.ManageReports, permissions);
            ////Assert.DoesNotContain(Permission.GenerateReports, permissions);

            // environment related permissions
            Assert.DoesNotContain(Permission.AccessDevelopmentData, permissions);
            Assert.DoesNotContain(Permission.AccessStagingData, permissions);
            Assert.DoesNotContain(Permission.AccessProductionData, permissions);
        }

        [Fact]
        public void GetPermissions_Should_Return_CorrectPermission_When_RoleType_Is_Customer()
        {
            // Arrange
            var mockRoleService = new Mock<IRoleService>();
            var mockUserService = new Mock<IUserService>();
            var mockSettingService = new Mock<IFeatureSettingService>();
            var mockAuthorisationService = new Mock<IAuthorisationService>();
            var mockUserAuthorisationService = new Mock<IUserAuthorisationService>();
            var mockRoleAuthorisationService = new Mock<IRoleAuthorisationService>();
            var mockMediator = new Mock<ICqrsMediator>();
            var httpContext = new Mock<Microsoft.AspNetCore.Http.HttpContext>();
            var userTenantId = Tenant.MasterTenantAlias;
            httpContext.Setup(m => m.User.FindFirst(ClaimNames.TenantId)).Returns(new System.Security.Claims.Claim(ClaimNames.TenantId, Tenant.MasterTenantId.ToString()));
            httpContext.Setup(m => m.User.FindFirst(ClaimNames.TenantId)).Returns(new System.Security.Claims.Claim(ClaimNames.TenantId, userTenantId));

            var context = new ControllerContext(new ActionContext(httpContext.Object, new RouteData(), new ControllerActionDescriptor()));
            var roleController = new RoleController(
                mockRoleService.Object,
                mockUserService.Object,
                new RoleTypePermissionsRegistry(),
                mockAuthorisationService.Object,
                mockMediator.Object,
                this.mockcachingResolver.Object,
                mockUserAuthorisationService.Object,
                mockRoleAuthorisationService.Object)
            {
                ControllerContext = context,
            };

            this.mockMediator.Setup(m => m.Send(It.IsAny<UserHasActiveFeatureSettingQuery>(), CancellationToken.None))
                .ReturnsAsync(true);

            // Act
            var response = roleController.GetPermissions(RoleType.Customer);

            // Assert
            var result = response as OkObjectResult;
            var permissions = (result.Value as IEnumerable<PermissionModel>).Select(c => c.Type);
            permissions.Should().NotBeNull();
            permissions.Should().NotBeEmpty();

            Assert.Contains(Permission.ViewMyAccount, permissions);
            Assert.Contains(Permission.EditMyAccount, permissions);

            // users related permissions
            Assert.DoesNotContain(Permission.ViewUsers, permissions);
            Assert.DoesNotContain(Permission.ManageUsers, permissions);
            Assert.DoesNotContain(Permission.ManageTenantAdminUsers, permissions);
            Assert.DoesNotContain(Permission.ManageMasterAdminUsers, permissions);

            // roles related permissions
            Assert.DoesNotContain(Permission.ViewRoles, permissions);
            Assert.DoesNotContain(Permission.ManageRoles, permissions);

            // tenants related permissions
            Assert.DoesNotContain(Permission.ViewTenants, permissions);
            Assert.DoesNotContain(Permission.ManageTenants, permissions);

            // products related permissions
            Assert.DoesNotContain(Permission.ViewProducts, permissions);
            Assert.DoesNotContain(Permission.ManageProducts, permissions);

            // releases related permissions
            Assert.DoesNotContain(Permission.ViewReleases, permissions);
            Assert.DoesNotContain(Permission.ManageReleases, permissions);
            Assert.DoesNotContain(Permission.PromoteReleasesToStaging, permissions);
            Assert.DoesNotContain(Permission.PromoteReleasesToProduction, permissions);

            // portals related permissions
            Assert.DoesNotContain(Permission.ViewPortals, permissions);
            Assert.DoesNotContain(Permission.ManagePortals, permissions);

            // background jobs related permissions
            Assert.DoesNotContain(Permission.ViewBackgroundJobs, permissions);
            Assert.DoesNotContain(Permission.ManageBackgroundJobs, permissions);

            // integration related permissions
            Assert.DoesNotContain(Permission.ReplayIntegrationEvents, permissions);

            // quote related pemissions
            Assert.Contains(Permission.ViewQuotes, permissions);
            Assert.Contains(Permission.ManageQuotes, permissions);
            Assert.DoesNotContain(Permission.EndorseQuotes, permissions);
            Assert.DoesNotContain(Permission.ReviewQuotes, permissions);
            Assert.DoesNotContain(Permission.ExportQuotes, permissions);
            Assert.Contains(Permission.ManageQuoteVersions, permissions);
            Assert.Contains(Permission.ViewQuoteVersions, permissions);

            // policies related permissions
            Assert.Contains(Permission.ViewPolicies, permissions);
            Assert.Contains(Permission.ManagePolicies, permissions);
            Assert.DoesNotContain(Permission.ExportPolicies, permissions);
            Assert.DoesNotContain(Permission.ImportPolicies, permissions);

            // claims related permissions
            Assert.Contains(Permission.ViewClaims, permissions);
            Assert.Contains(Permission.ManageClaims, permissions);
            Assert.DoesNotContain(Permission.AcknowledgeClaimNotifications, permissions);
            Assert.DoesNotContain(Permission.ReviewClaims, permissions);
            Assert.DoesNotContain(Permission.AssessClaims, permissions);
            Assert.DoesNotContain(Permission.SettleClaims, permissions);
            Assert.DoesNotContain(Permission.ExportClaims, permissions);
            Assert.DoesNotContain(Permission.ImportClaims, permissions);

            // customer related permissions
            Assert.DoesNotContain(Permission.ViewCustomers, permissions);
            Assert.DoesNotContain(Permission.ManageCustomers, permissions);
            Assert.DoesNotContain(Permission.ImportCustomers, permissions);

            // emails related permissions
            Assert.Contains(Permission.ViewMessages, permissions);
            Assert.Contains(Permission.ManageMessages, permissions);

            // no reports related permissions
            Assert.DoesNotContain(Permission.ViewReports, permissions);
            Assert.DoesNotContain(Permission.ManageReports, permissions);
            Assert.DoesNotContain(Permission.GenerateReports, permissions);

            // environment related permissions
            ////Assert.DoesNotContain(Permission.AccessDevelopmentData, permissions);
            ////Assert.DoesNotContain(Permission.AccessStagingData, permissions);
            ////Assert.DoesNotContain(Permission.AccessProductionData, permissions);
        }

        [Fact]
        public void GetPermissions_Should_Return_All_Permissions_When_RoleType_Is_NULL()
        {
            // Arrange
            var mockRoleService = new Mock<IRoleService>();
            var mockUserService = new Mock<IUserService>();
            var mockSettingService = new Mock<IFeatureSettingService>();
            var mockAuthorisationService = new Mock<IAuthorisationService>();
            var mockUserAuthorisationService = new Mock<IUserAuthorisationService>();
            var mockRoleAuthorisationService = new Mock<IRoleAuthorisationService>();
            var mockMediator = new Mock<ICqrsMediator>();
            var httpContext = new Mock<Microsoft.AspNetCore.Http.HttpContext>();
            var userTenantId = Tenant.MasterTenantAlias;
            httpContext.Setup(m => m.User.FindFirst(ClaimNames.TenantId)).Returns(new System.Security.Claims.Claim(ClaimNames.TenantId, Tenant.MasterTenantId.ToString()));
            httpContext.Setup(m => m.User.FindFirst(ClaimNames.TenantId)).Returns(new System.Security.Claims.Claim(ClaimNames.TenantId, userTenantId));

            var context = new ControllerContext(new ActionContext(httpContext.Object, new RouteData(), new ControllerActionDescriptor()));
            var roleController = new RoleController(
                mockRoleService.Object,
                mockUserService.Object,
                new RoleTypePermissionsRegistry(),
                mockAuthorisationService.Object,
                mockMediator.Object,
                this.mockcachingResolver.Object,
                mockUserAuthorisationService.Object,
                mockRoleAuthorisationService.Object)
            {
                ControllerContext = context,
            };

            this.mockMediator.Setup(m => m.Send(It.IsAny<UserHasActiveFeatureSettingQuery>(), CancellationToken.None))
                .ReturnsAsync(true);

            // Act
            var response = roleController.GetPermissions(null);

            // Assert
            var result = response as OkObjectResult;
            var permissions = (result.Value as IEnumerable<PermissionModel>).Select(c => c.Type);
            permissions.Should().NotBeNull();
            permissions.Should().NotBeEmpty();

            Assert.Contains(Permission.ViewMyAccount, permissions);
            Assert.Contains(Permission.EditMyAccount, permissions);

            // users related permissions
            Assert.Contains(Permission.ViewUsers, permissions);
            Assert.Contains(Permission.ManageUsers, permissions);
            Assert.Contains(Permission.ManageTenantAdminUsers, permissions);
            Assert.Contains(Permission.ManageMasterAdminUsers, permissions);

            // roles related permissions
            Assert.Contains(Permission.ViewRoles, permissions);
            Assert.Contains(Permission.ManageRoles, permissions);

            // tenants related permissions
            Assert.Contains(Permission.ViewTenants, permissions);
            Assert.Contains(Permission.ManageTenants, permissions);

            // products related permissions
            Assert.Contains(Permission.ViewProducts, permissions);
            Assert.Contains(Permission.ManageProducts, permissions);

            // releases related permissions
            Assert.Contains(Permission.ViewReleases, permissions);
            Assert.Contains(Permission.ManageReleases, permissions);
            Assert.Contains(Permission.PromoteReleasesToStaging, permissions);
            Assert.Contains(Permission.PromoteReleasesToProduction, permissions);

            // portals related permissions
            Assert.Contains(Permission.ViewPortals, permissions);
            Assert.Contains(Permission.ManagePortals, permissions);

            // background jobs related permissions
            Assert.Contains(Permission.ViewBackgroundJobs, permissions);
            Assert.Contains(Permission.ManageBackgroundJobs, permissions);

            // integration related permissions
            Assert.Contains(Permission.ReplayIntegrationEvents, permissions);

            // quote related pemissions
            Assert.Contains(Permission.ViewQuotes, permissions);
            Assert.Contains(Permission.ManageQuotes, permissions);
            Assert.Contains(Permission.EndorseQuotes, permissions);
            Assert.Contains(Permission.ReviewQuotes, permissions);
            Assert.Contains(Permission.ExportQuotes, permissions);
            Assert.Contains(Permission.ManageQuoteVersions, permissions);
            Assert.Contains(Permission.ViewQuoteVersions, permissions);

            // policies related permissions
            Assert.Contains(Permission.ViewPolicies, permissions);
            Assert.Contains(Permission.ManagePolicies, permissions);
            Assert.Contains(Permission.ManagePolicies, permissions);
            Assert.Contains(Permission.ManagePolicies, permissions);
            Assert.Contains(Permission.ExportPolicies, permissions);
            Assert.Contains(Permission.ImportPolicies, permissions);

            // claims related permissions
            Assert.Contains(Permission.ViewClaims, permissions);
            Assert.Contains(Permission.ManageClaims, permissions);
            Assert.Contains(Permission.AcknowledgeClaimNotifications, permissions);
            Assert.Contains(Permission.ReviewClaims, permissions);
            Assert.Contains(Permission.AssessClaims, permissions);
            Assert.Contains(Permission.SettleClaims, permissions);
            Assert.Contains(Permission.ExportClaims, permissions);
            Assert.Contains(Permission.ImportClaims, permissions);

            // customer related permissions
            Assert.Contains(Permission.ViewCustomers, permissions);
            Assert.Contains(Permission.ManageCustomers, permissions);
            Assert.Contains(Permission.ImportCustomers, permissions);

            // emails related permissions
            Assert.Contains(Permission.ViewMessages, permissions);
            Assert.Contains(Permission.ManageMessages, permissions);

            // no reports related permissions
            Assert.Contains(Permission.ViewReports, permissions);
            Assert.Contains(Permission.ManageReports, permissions);
            Assert.Contains(Permission.GenerateReports, permissions);

            // environment related permissions
            Assert.Contains(Permission.AccessDevelopmentData, permissions);
            Assert.Contains(Permission.AccessStagingData, permissions);
            Assert.Contains(Permission.AccessProductionData, permissions);
        }
    }
}
