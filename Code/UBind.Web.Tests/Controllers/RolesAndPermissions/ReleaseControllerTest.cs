// <copyright file="ReleaseControllerTest.cs" company="uBind">
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
    using UBind.Web.ResourceModels.ProductRelease;
    using Xunit;

    /// <summary>
    /// Defines the <see cref="ReleaseControllerTest" />.
    /// </summary>
    public class ReleaseControllerTest
    {
        /// <summary>
        /// Defines the releaseController.
        /// </summary>
        private ReleaseController releaseController;

        public ReleaseControllerTest()
        {
            this.releaseController = new ReleaseController(null, null, null, null);
        }

        /// <summary>
        /// The Get_SourceFilesForRelease_Success.
        /// </summary>
        [Fact]
        public void Get_SourceFilesForRelease_Success()
        {
            string[] userPermissions = new string[]
              {
                Permission.ViewReleases.ToString(),
              };

            // Assert
            Assert.True(AuthorizationTest.IsAuthorized(
                this.releaseController,
                "GetSourceFilesForRelease",
                new Type[] { typeof(Guid), typeof(string) },
                userPermissions,
                null));
        }

        /// <summary>
        /// The Get_SourceFilesForRelease_Forbidden.
        /// </summary>
        [Fact]
        public void Get_SourceFilesForRelease_Forbidden()
        {
            string[] userPermissions = new string[]
              {
                Permission.ManageCustomers.ToString(),
              };

            // Assert
            Assert.False(AuthorizationTest.IsAuthorized(
                this.releaseController,
                "GetSourceFilesForRelease",
                new Type[] { typeof(Guid), typeof(string) },
                userPermissions,
                null));
        }

        /// <summary>
        /// The Get_ReleaseById_Success.
        /// </summary>
        [Fact]
        public void Get_ReleaseById_Success()
        {
            string[] userPermissions = new string[]
              {
                Permission.ViewReleases.ToString(),
              };

            // Assert
            Assert.True(AuthorizationTest.IsAuthorized(
                this.releaseController,
                "GetReleaseById",
                new Type[] { typeof(Guid), typeof(string) },
                userPermissions,
                null));
        }

        /// <summary>
        /// The Get_ReleaseById_Forbidden.
        /// </summary>
        [Fact]
        public void Get_ReleaseById_Forbidden()
        {
            string[] userPermissions = new string[]
              {
                Permission.ViewQuoteVersions.ToString(),
              };

            // Assert
            Assert.False(AuthorizationTest.IsAuthorized(
                this.releaseController,
                "GetReleaseById",
                new Type[] { typeof(Guid), typeof(string) },
                userPermissions,
                null));
        }

        /// <summary>
        /// The Post_Release_Success.
        /// </summary>
        [Fact]
        public void Post_Release_Success()
        {
            string[] userPermissions = new string[]
              {
                Permission.ManageReleases.ToString(),
              };

            // Assert
            Assert.True(AuthorizationTest.IsAuthorized(
                this.releaseController,
                "CreateRelease",
                new Type[] { typeof(ReleaseUpsertModel) },
                userPermissions,
                null));
        }

        /// <summary>
        /// The Post_Release_Forbidden.
        /// </summary>
        [Fact]
        public void Post_Release_Forbidden()
        {
            string[] userPermissions = new string[]
              {
                Permission.ManageProducts.ToString(),
              };

            // Assert
            Assert.False(AuthorizationTest.IsAuthorized(
                this.releaseController,
                "CreateRelease",
                new Type[] { typeof(ReleaseUpsertModel) },
                userPermissions,
                null));
        }

        /// <summary>
        /// The Put_Release_Success.
        /// </summary>
        [Fact]
        public void Put_Release_Success()
        {
            string[] userPermissions = new string[]
              {
                Permission.ManageReleases.ToString(),
              };

            // Assert
            Assert.True(AuthorizationTest.IsAuthorized(
                this.releaseController,
                "UpdateRelease",
                new Type[] { typeof(Guid), typeof(ReleaseUpsertModel) },
                userPermissions,
                null));
        }

        /// <summary>
        /// The Put_Release_Forbidden.
        /// </summary>
        [Fact]
        public void Put_Release_Forbidden()
        {
            string[] userPermissions = new string[]
              {
                Permission.ManageProducts.ToString(),
              };

            // Assert
            Assert.False(AuthorizationTest.IsAuthorized(
                this.releaseController,
                "UpdateRelease",
                new Type[] { typeof(Guid), typeof(ReleaseUpsertModel) },
                userPermissions,
                null));
        }

        /// <summary>
        /// The Delete_Release_Success.
        /// </summary>
        [Fact]
        public void Delete_Release_Success()
        {
            string[] userPermissions = new string[]
              {
                Permission.ManageReleases.ToString(),
              };

            // Assert
            Assert.True(AuthorizationTest.IsAuthorized(
                this.releaseController,
                "DeleteRelease",
                new Type[] { typeof(Guid), typeof(string) },
                userPermissions,
                null));
        }

        /// <summary>
        /// The Delete_Release_Forbidden.
        /// </summary>
        [Fact]
        public void Delete_Release_Forbidden()
        {
            string[] userPermissions = new string[]
              {
                Permission.ManageProducts.ToString(),
              };

            // Assert
            Assert.False(AuthorizationTest.IsAuthorized(
                this.releaseController,
                "DeleteRelease",
                new Type[] { typeof(Guid), typeof(string) },
                userPermissions,
                null));
        }

        /// <summary>
        /// The Restore_Release_Success.
        /// </summary>
        [Fact]
        public void Restore_Release_Success()
        {
            string[] userPermissions = new string[]
              {
                Permission.ManageReleases.ToString(),
              };

            // Assert
            Assert.True(AuthorizationTest.IsAuthorized(
                this.releaseController,
                "Restore",
                new Type[] { typeof(Guid), typeof(string) },
                userPermissions,
                null));
        }

        /// <summary>
        /// The Restore_Release_Forbidden.
        /// </summary>
        [Fact]
        public void Restore_Release_Forbidden()
        {
            string[] userPermissions = new string[]
              {
                Permission.ManageProducts.ToString(),
              };

            // Assert
            Assert.False(AuthorizationTest.IsAuthorized(
                this.releaseController,
                "Restore",
                new Type[] { typeof(Guid), typeof(string) },
                userPermissions,
                null));
        }
    }
}
