// <copyright file="EmailControllerTest.cs" company="uBind">
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
    /// Defines the <see cref="EmailControllerTest" />.
    /// </summary>
    public class EmailControllerTest
    {
        /// <summary>
        /// Defines the emailController.
        /// </summary>
        private EmailController emailController;

        public EmailControllerTest()
        {
            this.emailController = new EmailController(null, null, null, null);
        }

        /// <summary>
        /// The Get_Email_Success.
        /// </summary>
        [Fact]
        public void Get_Email_Success()
        {
            string[] userPermissions = new string[]
            {
                Permission.ViewMessages.ToString(),
            };

            // Assert
            Assert.True(AuthorizationTest.IsAuthorized(
                this.emailController,
                "GetEmails",
                new Type[] { typeof(EmailQueryOptionsRequestModel) },
                userPermissions,
                null));
        }

        /// <summary>
        /// The Get_Email_Forbidden.
        /// </summary>
        [Fact]
        public void Get_Email_Forbidden()
        {
            string[] userPermissions = new string[]
            {
                Permission.ViewMyAccount.ToString(),
            };

            // Assert
            Assert.False(AuthorizationTest.IsAuthorized(
                this.emailController,
                "GetEmails",
                new Type[] { typeof(EmailQueryOptionsRequestModel) },
                userPermissions,
                null));
        }

        /// <summary>
        /// The Get_EmailDetails_Success.
        /// </summary>
        [Fact]
        public void Get_EmailDetails_Success()
        {
            string[] userPermissions = new string[]
            {
                Permission.ViewMessages.ToString(),
            };

            // Assert
            Assert.True(AuthorizationTest.IsAuthorized(
                this.emailController,
                "GetEmailDetails",
                new Type[] { typeof(Guid), typeof(string) },
                userPermissions,
                null));
        }

        /// <summary>
        /// The Get_EmailDetails_Forbidden.
        /// </summary>
        [Fact]
        public void Get_EmailDetails_Forbidden()
        {
            string[] userPermissions = new string[]
            {
                Permission.ViewPolicies.ToString(),
            };

            // Assert
            Assert.False(AuthorizationTest.IsAuthorized(
                this.emailController,
                "GetEmailDetails",
                new Type[] { typeof(Guid), typeof(string) },
                userPermissions,
                null));
        }
    }
}
