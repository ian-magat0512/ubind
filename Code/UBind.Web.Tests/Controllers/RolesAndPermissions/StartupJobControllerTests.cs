// <copyright file="StartupJobControllerTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

// CS1591 Missing XML comment for publicly visible type or member
// Suppress CS1591. The unit test method must be named correctly instead of adding a comment in it.
#pragma warning disable CS1591

namespace UBind.Web.Tests.Controllers.RolesAndPermissions
{
    using FluentAssertions;
    using UBind.Domain.Entities;
    using UBind.Domain.Permissions;
    using UBind.Web.Controllers;
    using Xunit;

    public class StartupJobControllerTests
    {
        private StartupJobController startupJobController;

        public StartupJobControllerTests()
        {
            this.startupJobController = new StartupJobController(null, null);

            var defaultRolePermissionRegistry = new DefaultRolePermissionsRegistry();
            var defaultRoleNameRegistry = new DefaultRoleNameRegistry();
            var roleTypePermissionRegistry = new RoleTypePermissionsRegistry();
            Role.SetDefaultRolePermissionsRegistry(defaultRolePermissionRegistry);
            Role.SetDefaultRoleNameRegistry(defaultRoleNameRegistry);
            PermissionExtensions.SetDefaultRolePermissionsRegistry(defaultRolePermissionRegistry);
            PermissionExtensions.SetRoleTypePermissionsRegistry(roleTypePermissionRegistry);
        }

        [Fact]
        public void Post_StartupJobs_Sucess()
        {
            string[] userPermissions = new string[]
            {
                Permission.ManageStartupJobs.ToString(),
            };

            // Act + Assert
            this.IsAuthorisedToRunJob(userPermissions).Should().BeTrue();
        }

        [Fact]
        public void Post_StartupJobs_Forbidden()
        {
            string[] userPermissions = new string[]
            {
                Permission.ManageBackgroundJobs.ToString(),
            };

            // Act + Assert
            this.IsAuthorisedToRunJob(userPermissions).Should().BeFalse();
        }

        private bool IsAuthorisedToRunJob(string[] userPermissions)
        {
            return AuthorizationTest.IsAuthorized(
                this.startupJobController,
                "RunJob",
                new System.Type[] { typeof(string), typeof(bool) },
                userPermissions,
                null);
        }
    }
}
