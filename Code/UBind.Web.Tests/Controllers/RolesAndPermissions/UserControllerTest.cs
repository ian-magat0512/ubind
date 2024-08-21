// <copyright file="UserControllerTest.cs" company="uBind">
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
    using UBind.Web.ResourceModels;
    using Xunit;

    /// <summary>
    /// Defines the <see cref="UserControllerTest" />.
    /// </summary>
    public class UserControllerTest
    {
        /// <summary>
        /// Defines the userController.
        /// </summary>
        private UserController userController;

        public UserControllerTest()
        {
            this.userController = new UserController(null, null, null, null, null, null, null, null, null);
        }

        /// <summary>
        /// The Get_Users_Success.
        /// </summary>
        [Fact]
        public void Get_Users_Success()
        {
            string[] userPermissions = new string[]
            {
                Permission.ViewUsers.ToString(),
            };

            // Assert
            Assert.True(AuthorizationTest.IsAuthorized(
                this.userController,
                "GetUsers",
                new Type[] { typeof(UserQueryOptionsModel) },
                userPermissions,
                null));
        }

        /// <summary>
        /// The Get_Users_Forbidden.
        /// </summary>
        [Fact]
        public void Get_Users_Forbidden()
        {
            string[] userPermissions = new string[]
            {
                Permission.ViewReports.ToString(),
            };

            // Assert
            Assert.False(AuthorizationTest.IsAuthorized(
                this.userController,
                "GetUsers",
                new Type[] { typeof(UserQueryOptionsModel) },
                userPermissions,
                null));
        }

        /// <summary>
        /// The Get_UserDetails_Success.
        /// </summary>
        [Fact]
        public void Get_UserDetails_Success()
        {
            string[] userPermissions = new string[]
            {
                Permission.ViewUsers.ToString(),
            };

            // Assert
            Assert.True(AuthorizationTest.IsAuthorized(
                this.userController,
                "GetUser",
                new Type[] { typeof(Guid), typeof(string), typeof(string) },
                userPermissions,
                null));
        }

        /// <summary>
        /// The DisableUser_Success.
        /// </summary>
        [Fact]
        public void DisableUser_Success()
        {
            string[] userPermissions = new string[]
            {
                Permission.ManageUsers.ToString(),
            };

            // Assert
            Assert.True(AuthorizationTest.IsAuthorized(
                this.userController,
                "DisableUser",
                new Type[] { typeof(ToggleUserModel) },
                userPermissions,
                null));
        }

        /// <summary>
        /// The DisableUser_Forbidden.
        /// </summary>
        [Fact]
        public void DisableUser_Forbidden()
        {
            string[] userPermissions = new string[]
            {
                Permission.ViewUsers.ToString(),
            };

            // Assert
            Assert.False(AuthorizationTest.IsAuthorized(
                this.userController,
                "DisableUser",
                new Type[] { typeof(ToggleUserModel) },
                userPermissions,
                null));
        }

        /// <summary>
        /// The EnableUser_Success.
        /// </summary>
        [Fact]
        public void EnableUser_Success()
        {
            string[] userPermissions = new string[]
            {
                Permission.ManageUsers.ToString(),
            };

            // Assert
            Assert.True(AuthorizationTest.IsAuthorized(
                this.userController,
                "EnableUser",
                new Type[] { typeof(ToggleUserModel) },
                userPermissions,
                null));
        }

        /// <summary>
        /// The EnableUser_Forbidden.
        /// </summary>
        [Fact]
        public void EnableUser_Forbidden()
        {
            string[] userPermissions = new string[]
            {
                Permission.ViewUsers.ToString(),
            };

            // Assert
            Assert.False(AuthorizationTest.IsAuthorized(
                this.userController,
                "EnableUser",
                new Type[] { typeof(ToggleUserModel) },
                userPermissions,
                null));
        }

        /// <summary>
        /// The Get_UserDetails_Forbidden.
        /// </summary>
        [Fact]
        public void Get_UserDetails_Forbidden()
        {
            string[] userPermissions = new string[]
            {
                Permission.ViewQuoteVersions.ToString(),
            };

            // Assert
            Assert.False(AuthorizationTest.IsAuthorized(
                this.userController,
                "GetUser",
                new Type[] { typeof(Guid), typeof(string), typeof(string) },
                userPermissions,
                null));
        }

        /// <summary>
        /// The Post_User_Success.
        /// </summary>
        [Fact]
        public void Post_User_Success()
        {
            string[] userPermissions = new string[]
            {
                Permission.ManageUsers.ToString(),
            };

            // Assert
            Assert.True(AuthorizationTest.IsAuthorized(
                this.userController,
                "CreateUser",
                new Type[] { typeof(SignupModel) },
                userPermissions,
                null));
        }

        /// <summary>
        /// The Post_User_Forbidden.
        /// </summary>
        [Fact]
        public void Post_User_Forbidden()
        {
            string[] userPermissions = new string[]
            {
                Permission.ManageReports.ToString(),
            };

            // Assert
            Assert.False(AuthorizationTest.IsAuthorized(
                this.userController,
                "CreateUser",
                new Type[] { typeof(SignupModel) },
                userPermissions,
                null));
        }

        /// <summary>
        /// The Put_User_Success.
        /// </summary>
        [Fact]
        public void Put_User_Success()
        {
            string[] userPermissions = new string[]
            {
                Permission.ManageUsers.ToString(),
            };

            // Assert
            Assert.True(AuthorizationTest.IsAuthorized(
                this.userController,
                "UpdateUser",
                new Type[] { typeof(Guid), typeof(Web.ResourceModels.UserUpdateModel) },
                userPermissions,
                null));
        }

        /// <summary>
        /// The Put_User_Forbidden.
        /// </summary>
        [Fact]
        public void Put_User_Forbidden()
        {
            string[] userPermissions = new string[]
            {
                Permission.ManageReports.ToString(),
            };

            // Assert
            Assert.False(AuthorizationTest.IsAuthorized(
                this.userController,
                "UpdateUser",
                new Type[] { typeof(Guid), typeof(Web.ResourceModels.UserUpdateModel) },
                userPermissions,
                null));
        }

        /// <summary>
        /// The Post_UpdateUserPic_Success.
        /// </summary>
        [Fact]
        public void Post_UpdateUserPic_Success()
        {
            string[] userPermissions = new string[]
            {
                Permission.ManageUsers.ToString(),
            };

            // Assert
            Assert.True(AuthorizationTest.IsAuthorized(
                this.userController,
                "UpdateUserPicture",
                new Type[] { typeof(Guid), typeof(string) },
                userPermissions,
                null));
        }

        /// <summary>
        /// The Get_UpdateUserPic_Forbidden.
        /// </summary>
        [Fact]
        public void Get_UpdateUserPic_Forbidden()
        {
            string[] userPermissions = new string[]
            {
                Permission.ViewReports.ToString(),
            };

            // Assert
            Assert.False(AuthorizationTest.IsAuthorized(
                this.userController,
                "UpdateUserPicture",
                new Type[] { typeof(Guid), typeof(string) },
                userPermissions,
                null));
        }
    }
}
