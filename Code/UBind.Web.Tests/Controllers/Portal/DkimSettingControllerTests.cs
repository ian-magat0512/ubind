// <copyright file="DkimSettingControllerTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Tests.Controllers.Portal
{
    using System;
    using FluentAssertions;
    using UBind.Domain.Permissions;
    using UBind.Web.Controllers;
    using UBind.Web.Tests.Controllers.RolesAndPermissions;
    using Xunit;

    public class DkimSettingControllerTests
    {
        /// <summary>
        /// Defines the dkimSettingController.
        /// </summary>
        private readonly DkimSettingController dkimSettingController;

        public DkimSettingControllerTests()
        {
            this.dkimSettingController = new DkimSettingController(null, null, null);
        }

        [Fact]
        public void GetDkimSettingsByOrganisationId_ShouldThrowUnauthorized_WhenUserDoesNotHavePermission()
        {
            // Arrange
            string[] userPermissions = new string[]
            {
                Permission.ViewOrganisations.ToString(),
                Permission.ViewAllQuotesFromAllOrganisations.ToString(),
            };

            // Act & Assert
            AuthorizationTest
                .IsAuthorized(
                    this.dkimSettingController,
                    "GetDkimSettingsByOrganisationId",
                    new Type[] { typeof(string), typeof(string) },
                    userPermissions,
                    null)
                .Should()
                .BeFalse();
        }

        [Fact]
        public void GetDkimSettingsByOrganisationId_ShouldSucceed_WhenUseHasPermission()
        {
            // Arrange
            string[] userPermissions = new string[]
            {
                Permission.ManageOrganisations.ToString(),
            };

            // Act & Assert
            AuthorizationTest
                .IsAuthorized(
                    this.dkimSettingController,
                    "GetDkimSettingsByOrganisationId",
                    new Type[] { typeof(string), typeof(string) },
                    userPermissions,
                    null)
                .Should()
                .BeTrue();
        }
    }
}
