// <copyright file="PortalControllerTest.cs" company="uBind">
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
    using UBind.Domain.Permissions;
    using UBind.Web.Controllers;
    using UBind.Web.ResourceModels.Portal;
    using Xunit;

    /// <summary>
    /// Defines the <see cref="PortalControllerTest" />.
    /// </summary>
    public class PortalControllerTest
    {
        /// <summary>
        /// Defines the portalController.
        /// </summary>
        private PortalController portalController;

        public PortalControllerTest()
        {
            this.portalController = new PortalController(null, null, null, null, null, null);
        }

        /// <summary>
        /// The Get_Portal_Success.
        /// </summary>
        [Fact]
        public void Get_Portals_Success()
        {
            string[] userPermissions = new string[]
            {
                Permission.ViewPortals.ToString(),
            };

            // Assert
            Assert.True(AuthorizationTest.IsAuthorizedOneOfPermissions(
                this.portalController,
                "GetPortals",
                new Type[] { typeof(PortalQueryOptionsModel) },
                userPermissions,
                null));
        }

        /// <summary>
        /// The Get_Portal_Forbidden.
        /// </summary>
        [Fact]
        public void Get_Portal_Forbidden()
        {
            string[] userPermissions = new string[]
            {
                Permission.ViewQuotes.ToString(),
            };

            // Assert
            Assert.False(AuthorizationTest.IsAuthorized(
                this.portalController,
                "GetPortals",
                new Type[] { typeof(PortalQueryOptionsModel) },
                userPermissions,
                null));
        }

        /// <summary>
        /// The Get_PortalDetails_Success.
        /// </summary>
        [Fact]
        public void Get_PortalDetails_Success()
        {
            string[] userPermissions = new string[]
            {
                Permission.ViewPortals.ToString(),
            };

            // Assert
            Assert.True(AuthorizationTest.IsAuthorized(
                this.portalController,
                "GetPortalDetails",
                new Type[] { typeof(string), typeof(string), typeof(bool) },
                userPermissions,
                null));
        }

        /// <summary>
        /// The Get_PortalDetails_Forbidden.
        /// </summary>
        [Fact]
        public void Get_PortalDetails_Forbidden()
        {
            string[] userPermissions = new string[]
            {
                Permission.ViewReports.ToString(),
            };

            // Assert
            Assert.False(AuthorizationTest.IsAuthorized(
                this.portalController,
                "GetPortalDetails",
                new Type[] { typeof(string), typeof(string), typeof(bool) },
                userPermissions,
                null));
        }

        /// <summary>
        /// The Get_PortalActiveFeatures_anonymous.
        /// </summary>
        [Fact]
        public void Get_PortalActiveFeatures_anonymous()
        {
            // Assert
            Assert.True(AuthorizationTest.IsAnonymous(
                this.portalController,
                "GetActiveFeatures",
                new Type[] { typeof(string), typeof(string) }));
        }

        /// <summary>
        /// The Post_Portal_Success.
        /// </summary>
        [Fact]
        public void Post_Portal_Success()
        {
            string[] userPermissions = new string[]
            {
                Permission.ManageQuotes.ToString(),
                Permission.ManagePortals.ToString(),
                Permission.ManageCustomers.ToString(),
            };

            // Assert
            Assert.True(AuthorizationTest.IsAuthorized(
                this.portalController,
                "CreatePortal",
                new Type[] { typeof(PortalRequestModel) },
                userPermissions,
                null));
        }

        /// <summary>
        /// The Post_Portal_Forbidden.
        /// </summary>
        [Fact]
        public void Post_Portal_Forbidden()
        {
            string[] userPermissions = new string[]
            {
                Permission.ManageQuotes.ToString(),
                Permission.ManageCustomers.ToString(),
            };

            // Assert
            Assert.False(AuthorizationTest.IsAuthorized(
                this.portalController,
                "CreatePortal",
                new Type[] { typeof(PortalRequestModel) },
                userPermissions,
                null));
        }

        /// <summary>
        /// The Put_Portal_Success.
        /// </summary>
        [Fact]
        public void Put_Portal_Success()
        {
            string[] userPermissions = new string[]
            {
                Permission.ManageQuotes.ToString(),
                Permission.ManagePortals.ToString(),
                Permission.ManageCustomers.ToString(),
            };

            // Assert
            Assert.True(AuthorizationTest.IsAuthorized(
                this.portalController,
                "UpdatePortal",
                new Type[] { typeof(string), typeof(PortalRequestModel) },
                userPermissions,
                null));
        }

        /// <summary>
        /// The Put_Portal_Forbidden.
        /// </summary>
        [Fact]
        public void Put_Portal_Forbidden()
        {
            string[] userPermissions = new string[]
            {
                Permission.ViewTenants.ToString(),
                Permission.ManageCustomers.ToString(),
            };

            // Assert
            Assert.False(AuthorizationTest.IsAuthorized(
                this.portalController,
                "UpdatePortal",
                new Type[] { typeof(string), typeof(PortalRequestModel) },
                userPermissions,
                null));
        }

        /// <summary>
        /// The Post_PortalSettings_Success.
        /// </summary>
        [Fact]
        public void Put_PortalFatures_Success()
        {
            string[] userPermissions = new string[]
            {
                Permission.ManagePortals.ToString(),
            };

            // Assert
            Assert.True(AuthorizationTest.IsAuthorized(
                this.portalController,
                "UpdatePortalFeatures",
                new Type[] { typeof(string), typeof(FeatureSettingsUpdateResourceModel) },
                userPermissions,
                null));
        }

        /// <summary>
        /// The Post_PortalSettings_Forbidden.
        /// </summary>
        [Fact]
        public void Put_PortalFeatures_Forbidden()
        {
            string[] userPermissions = new string[]
          {
                Permission.ViewTenants.ToString(),
                Permission.ManageCustomers.ToString(),
          };

            // Assert
            Assert.False(AuthorizationTest.IsAuthorized(
                this.portalController,
                "UpdatePortalFeatures",
                new Type[] { typeof(string), typeof(FeatureSettingsUpdateResourceModel) },
                userPermissions,
                null));
        }
    }
}
