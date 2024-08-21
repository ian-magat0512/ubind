// <copyright file="CustomerControllerTest.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

// CS1591 Missing XML comment for publicly visible type or member
// Suppress CS1591. The unit test method should be named correctly instead of adding a comment in it.
#pragma warning disable CS1591

namespace UBind.Web.Tests.Controllers.RolesAndPermissions
{
    using System;
    using UBind.Domain.Permissions;
    using UBind.Web.Controllers;
    using UBind.Web.ResourceModels;
    using Xunit;

    public class CustomerControllerTest
    {
        private CustomerController customerController;

        public CustomerControllerTest()
        {
            this.customerController
                = new CustomerController(null, null, null, null, null, null, null, null, null, null, null, null);
        }

        [Fact]
        public void Get_Customer_Success()
        {
            string[] userPermissions = new string[]
            {
                Permission.ViewCustomers.ToString(),
            };

            // Assert
            Assert.True(AuthorizationTest.IsAuthorized(
                this.customerController,
                "GetCustomers",
                new Type[] { typeof(QueryOptionsModel) },
                userPermissions,
                null));
        }

        [Fact]
        public void Get_Customer_Forbidden()
        {
            string[] userPermissions = new string[]
            {
                Permission.ManageCustomers.ToString(),
            };

            // Assert
            Assert.False(AuthorizationTest.IsAuthorized(
                this.customerController,
                "GetCustomers",
                new Type[] { typeof(QueryOptionsModel) },
                userPermissions,
                null));
        }

        [Fact]
        public void Post_Customer_Success()
        {
            string[] userPermissions = new string[]
            {
                Permission.ManagePortals.ToString(),
                Permission.ManageCustomers.ToString(),
            };

            // Assert
            Assert.True(AuthorizationTest.IsAuthorized(
                this.customerController,
                "CreateCustomer",
                new Type[] { typeof(string), typeof(CustomerPersonalDetailsModel) },
                userPermissions,
                null));
        }

        [Fact]
        public void Post_Customer_Forbidden()
        {
            string[] userPermissions = new string[]
            {
                Permission.ManagePortals.ToString(),
                Permission.ViewMyAccount.ToString(),
            };

            // Assert
            Assert.False(AuthorizationTest.IsAuthorized(
                this.customerController,
                "CreateCustomer",
                new Type[] { typeof(string), typeof(CustomerPersonalDetailsModel) },
                userPermissions,
                null));
        }

        /// <summary>
        /// The Get_PrimaryPersonDetails_Success.
        /// </summary>
        [Fact]
        public void Get_PrimaryPersonDetails_Success()
        {
            string[] userPermissions = new string[]
            {
                Permission.ViewCustomers.ToString(),
            };

            // Assert
            Assert.True(this.IsAuthorized(
                "GetPrimaryPersonDetails",
                new Type[] { typeof(Guid) },
                userPermissions));
        }

        /// <summary>
        /// The Get_PrimaryPersonDetails_Forbidden.
        /// </summary>
        [Fact]
        public void Get_PrimaryPersonDetails_Forbidden()
        {
            string[] userPermissions = new string[]
            {
                Permission.ViewClaims.ToString(),
            };

            // Assert
            Assert.False(this.IsAuthorized(
                "GetPrimaryPersonDetails",
                new Type[] { typeof(Guid) },
                userPermissions));
        }

        /// <summary>
        /// The Post_SetAsPrimaryRecordForCustomer_Success.
        /// </summary>
        [Fact]
        public void Patch_SetAsPrimaryRecordForCustomer_Success()
        {
            string[] userPermissions = new string[]
            {
                Permission.ManageCustomers.ToString(),
            };

            // Assert
            Assert.True(this.IsAuthorized(
                "SetPersonAsPrimaryRecordForCustomer",
                new Type[] { typeof(Guid), typeof(CustomerPatchModel) },
                userPermissions));
        }

        /// <summary>
        /// The Post_SetAsPrimaryRecordForCustomer_Forbidden.
        /// </summary>
        [Fact]
        public void Patch_SetAsPrimaryRecordForCustomer_Forbidden()
        {
            string[] userPermissions = new string[]
            {
                Permission.ViewClaims.ToString(),
            };

            // Assert
            Assert.False(this.IsAuthorized(
                "SetPersonAsPrimaryRecordForCustomer",
                new Type[] { typeof(Guid), typeof(CustomerPatchModel) },
                userPermissions));
        }

        private bool IsAuthorized(string functionName, Type[] paramterTypes, string[] userPermissions)
        {
            return AuthorizationTest.IsAuthorized(
                this.customerController,
                functionName,
                paramterTypes,
                userPermissions,
                null);
        }
    }
}
