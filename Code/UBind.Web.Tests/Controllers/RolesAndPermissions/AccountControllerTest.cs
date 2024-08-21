// <copyright file="AccountControllerTest.cs" company="uBind">
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
    using FluentAssertions;
    using UBind.Domain.Permissions;
    using UBind.Web.Controllers;
    using UBind.Web.ResourceModels;
    using Xunit;

    /// <summary>
    /// Defines the <see cref="AccountControllerTest" />.
    /// </summary>
    public class AccountControllerTest
    {
        /// <summary>
        /// Defines the accountController.
        /// </summary>
        private AccountController accountController;

        public AccountControllerTest()
        {
            this.accountController = new AccountController(null, null, null, null, null, null, null, null);
        }

        /// <summary>
        /// The Get_Account_Success.
        /// </summary>
        [Fact]
        public void Get_Account_Success()
        {
            string[] userPermissions = new string[]
            {
                Permission.ViewMyAccount.ToString(),
            };

            // Assert
            Assert.True(AuthorizationTest.IsAuthorized(
                this.accountController,
                "GetAccount",
                null,
                userPermissions,
                null));
        }

        /// <summary>
        /// The Get_Account_Forbidden.
        /// </summary>
        [Fact]
        public void Get_Account_Forbidden()
        {
            string[] userPermissions = new string[]
            {
                Permission.ViewPolicies.ToString(),
            };

            // Assert
            Assert.False(AuthorizationTest.IsAuthorized(
                this.accountController,
                "GetAccount",
                null,
                userPermissions,
                null));
        }

        /// <summary>
        /// The Update_Account_Success.
        /// </summary>
        [Fact]
        public void Update_Account_Success()
        {
            string[] userPermissions = new string[]
              {
                Permission.EditMyAccount.ToString(),
              };

            // Assert
            Assert.True(AuthorizationTest.IsAuthorized(
                this.accountController,
                "UpdateAccount",
                new Type[] { typeof(AccountUpdateViewModel) },
                userPermissions,
                null));
        }

        /// <summary>
        /// The Update_Account_Forbidden.
        /// </summary>
        [Fact]
        public void Update_Account_Forbidden()
        {
            string[] userPermissions = new string[]
              {
                Permission.ManageProducts.ToString(),
              };

            // Assert
            Assert.False(AuthorizationTest.IsAuthorized(
                this.accountController,
                "UpdateAccount",
                new Type[] { typeof(AccountUpdateViewModel) },
                userPermissions,
                null));
        }

        /// <summary>
        /// The Action_ShouldBe_Authorized.
        /// </summary>
        [Theory]
        [InlineData("GetPermissionsOfTheLoggedInUser")]
        [InlineData("GetRolesOfTheLoggedInUser")]
        public void Action_ShouldBe_Authorized(string actionName)
        {
            // Arrange/Act
            var isAuthorized = AuthorizationTest.IsAuthorized(
                this.accountController,
                actionName,
                null);

            // Assert
            isAuthorized.Should().BeTrue();
        }
    }
}
