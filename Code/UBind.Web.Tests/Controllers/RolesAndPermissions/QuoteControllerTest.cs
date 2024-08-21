// <copyright file="QuoteControllerTest.cs" company="uBind">
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
    using Xunit;

    public class QuoteControllerTest
    {
        private readonly QuoteController controller;

        public QuoteControllerTest()
        {
            this.controller = new QuoteController(null, null, null, null, null, null, null, null, null, null, null, null);
        }

        [Fact]
        public void QuoteAssociateWithCustomer_Success()
        {
            string[] userPermissions = new string[]
            {
                Permission.ManageQuotes.ToString(),
                Permission.ManageCustomers.ToString(),
            };

            // Assert
            Assert.True(AuthorizationTest.IsAuthorized(
                this.controller,
                "QuoteAssociateWithCustomer",
                new Type[] { typeof(Guid), typeof(Guid), },
                userPermissions,
                null));
        }

        [Fact]
        public void QuoteAssociateWithCustomer_Unauthorised()
        {
            string[] userPermissions = new string[]
            {
                Permission.ManageCustomers.ToString(),
            };

            // Assert
            Assert.False(AuthorizationTest.IsAuthorized(
                this.controller,
                "QuoteAssociateWithCustomer",
                new Type[] { typeof(Guid), typeof(Guid), },
                userPermissions,
                null));
        }
    }
}
