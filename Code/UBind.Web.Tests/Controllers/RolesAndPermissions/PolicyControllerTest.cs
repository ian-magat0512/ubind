// <copyright file="PolicyControllerTest.cs" company="uBind">
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
    using Moq;
    using UBind.Application.Services;
    using UBind.Domain;
    using UBind.Domain.Permissions;
    using UBind.Web.Controllers;
    using UBind.Web.ResourceModels;
    using Xunit;

    public class PolicyControllerTest
    {
        private readonly PolicyController policyController;

        public PolicyControllerTest()
        {
            var formDataPrettifier = new Mock<IFormDataPrettifier>();
            var mockRenewalInvitationService = new Mock<IRenewalInvitationService>();
            this.policyController = new PolicyController(
                null,
                null,
                null,
                null,
                formDataPrettifier.Object,
                null,
                null,
                null,
                null,
                null,
                mockRenewalInvitationService.Object);
        }

        [Fact]
        public void Cancel_Policy_Success()
        {
            string[] userPermissions = new string[]
            {
                Permission.ManagePolicies.ToString(),
            };

            // Assert
            Assert.True(AuthorizationTest.IsAuthorized(
                this.policyController,
                "Cancel",
                new Type[] { typeof(Guid) },
                userPermissions,
                null));
        }

        [Fact]
        public void Cancel_Policy_Forbidden()
        {
            string[] userPermissions = new string[]
            {
                Permission.ReviewQuotes.ToString(),
            };

            // Assert
            Assert.False(AuthorizationTest.IsAuthorized(
                this.policyController,
                "Cancel",
                new Type[] { typeof(Guid) },
                userPermissions,
                null));
        }

        [Fact]
        public void Get_Policy_Success()
        {
            string[] userPermissions = new string[]
            {
                Permission.ViewPolicies.ToString(),
            };

            // Assert
            Assert.True(AuthorizationTest.IsAuthorized(
                this.policyController,
                "GetPolicies",
                new Type[] { typeof(PolicyQueryOptionsModel) },
                userPermissions,
                null));
        }

        [Fact]
        public void Get_Policy_Forbidden()
        {
            string[] userPermissions = new string[]
            {
                Permission.ViewPortals.ToString(),
            };

            // Assert
            Assert.False(AuthorizationTest.IsAuthorized(
                this.policyController,
                "GetPolicies",
                new Type[] { typeof(PolicyQueryOptionsModel) },
                userPermissions,
                null));
        }

        [Fact]
        public void Get_PolicyDetails_Success()
        {
            string[] userPermissions = new string[]
            {
                Permission.ViewPolicies.ToString(),
            };

            // Assert
            Assert.True(AuthorizationTest.IsAuthorized(
                this.policyController,
                "GetPolicy",
                new Type[] { typeof(Guid), typeof(string) },
                userPermissions,
                null));
        }

        [Fact]
        public void Get_PolicyDetails_Forbidden()
        {
            string[] userPermissions = new string[]
            {
                Permission.ViewProducts.ToString(),
            };

            // Assert
            Assert.False(AuthorizationTest.IsAuthorized(
                this.policyController,
                "GetPolicy",
                new Type[] { typeof(Guid), typeof(string) },
                userPermissions,
                null));
        }

        [Fact]
        public void AssociateWithCustomer_Success()
        {
            string[] userPermissions = new string[]
            {
                Permission.ManagePolicies.ToString(),
                Permission.ManageCustomers.ToString(),
            };

            // Assert
            Assert.True(AuthorizationTest.IsAuthorized(
                this.policyController,
                "AssociateWithCustomer",
                new Type[] { typeof(Guid), typeof(Guid) },
                userPermissions,
                null));
        }

        [Fact]
        public void AssociateWithCustomer_Unauthorised()
        {
            string[] userPermissions = new string[]
            {
                Permission.ViewPolicies.ToString(),
                Permission.ManageCustomers.ToString(),
            };

            // Assert
            Assert.False(AuthorizationTest.IsAuthorized(
                this.policyController,
                "AssociateWithCustomer",
                new Type[] { typeof(Guid), typeof(Guid) },
                userPermissions,
                null));
        }
    }
}
