// <copyright file="ProductFeatureSettingControllerTest.cs" company="uBind">
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
    using UBind.Domain;
    using UBind.Domain.Permissions;
    using UBind.Domain.Product;
    using UBind.Web.Controllers;
    using Xunit;

    public class ProductFeatureSettingControllerTest
    {
        private ProductFeatureSettingController productFeatureController;

        public ProductFeatureSettingControllerTest()
        {
            this.productFeatureController = new ProductFeatureSettingController(null, null, null);
        }

        [Fact]
        public void GetTenantProductFeature_Forbidden()
        {
            string[] userPermissions = Array.Empty<string>();

            // Assert
            Assert.False(AuthorizationTest.IsAuthorized(
                this.productFeatureController,
                "GetProductFeaturesByDeployedEnvironment",
                new Type[] { typeof(string), typeof(DeploymentEnvironment) },
                userPermissions,
                null));
        }

        [Fact]
        public void EnableProductFeature_Success()
        {
            string[] userPermissions = new string[]
            {
                Permission.ManageProducts.ToString(),
            };

            // Assert
            Assert.True(AuthorizationTest.IsAuthorized(
                this.productFeatureController,
                "EnableProductFeature",
                new Type[] { typeof(string), typeof(ProductFeatureSettingItem), typeof(string) },
                userPermissions,
                null));
        }

        [Fact]
        public void EnableProductFeature_Forbidden()
        {
            string[] userPermissions = Array.Empty<string>();

            // Assert
            Assert.False(AuthorizationTest.IsAuthorized(
                this.productFeatureController,
                "EnableProductFeature",
                new Type[] { typeof(string), typeof(ProductFeatureSettingItem), typeof(string) },
                userPermissions,
                null));
        }

        [Fact]
        public void DisableProductFeature_Success()
        {
            string[] userPermissions = new string[]
            {
                Permission.ManageProducts.ToString(),
            };

            // Assert
            Assert.True(AuthorizationTest.IsAuthorized(
                this.productFeatureController,
                "DisableProductFeature",
                new Type[] { typeof(string), typeof(ProductFeatureSettingItem), typeof(string) },
                userPermissions,
                null));
        }

        [Fact]
        public void DisableProductFeature_Forbidden()
        {
            string[] userPermissions = Array.Empty<string>();

            // Assert
            Assert.False(AuthorizationTest.IsAuthorized(
                this.productFeatureController,
                "DisableProductFeature",
                new Type[] { typeof(string), typeof(ProductFeatureSettingItem), typeof(string) },
                userPermissions,
                null));
        }
    }
}
