// <copyright file="ProductControllerTest.cs" company="uBind">
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
    using System.Collections.Generic;
    using UBind.Domain;
    using UBind.Domain.Permissions;
    using UBind.Web.Controllers;
    using UBind.Web.ResourceModels;
    using Xunit;

    /// <summary>
    /// Defines the <see cref="ProductControllerTest" />.
    /// </summary>
    public class ProductControllerTest
    {
        /// <summary>
        /// Defines the productsController.
        /// </summary>
        private ProductController productController;

        public ProductControllerTest()
        {
            this.productController = new ProductController(null, null, null, null, null, null, null, null);
        }

        /// <summary>
        /// The Get_Products_Success.
        /// </summary>
        [Fact]
        public void Get_Products_Success()
        {
            // Assert
            Assert.True(AuthorizationTest.IsAuthorized(
                this.productController,
                "GetProducts",
                new Type[] { typeof(ProductQueryOptionsModel) },
                null,
                null));
        }

        /// <summary>
        /// The Get_Products_Forbidden.
        /// </summary>
        [Fact]
        public void Get_Products_Forbidden()
        {
            List<string> userPermissions = new List<string>
            {
                Permission.ManageCustomers.ToString(),
            };

            // Assert
            Assert.True(!AuthorizationTest.IsAuthorized(
                this.productController,
                "GetProducts",
                new Type[] { typeof(ProductQueryOptionsModel) },
                userPermissions.ToArray(),
                null));
        }

        /// <summary>
        /// The Get_ProductsDevelopmentRelease_Success.
        /// </summary>
        [Fact]
        public void Get_ProductsDevelopmentRelease_Success()
        {
            List<string> userPermissions = new List<string>
            {
                Permission.ViewProducts.ToString(),
            };

            // Assert
            Assert.True(AuthorizationTest.IsAuthorized(
                this.productController,
                "GetSyncResult",
                new Type[] { typeof(string), typeof(string) },
                userPermissions.ToArray(),
                null));
        }

        /// <summary>
        /// The Get_ProductsDevelopmentRelease_Forbidden.
        /// </summary>
        [Fact]
        public void Get_ProductsDevelopmentRelease_Forbidden()
        {
            List<string> userPermissions = new List<string>
            {
                Permission.ViewQuotes.ToString(),
            };

            // Assert
            Assert.False(AuthorizationTest.IsAuthorized(
                this.productController,
                "GetSyncResult",
                new Type[] { typeof(string), typeof(string) },
                userPermissions.ToArray(),
                null));
        }

        /// <summary>
        /// The Create_ProductRelease_Success.
        /// </summary>
        [Fact]
        public void Create_ProductRelease_Success()
        {
            List<string> userPermissions = new List<string>
            {
                Permission.ManageProducts.ToString(),
            };

            // Assert
            Assert.True(AuthorizationTest.IsAuthorized(
                this.productController,
                "Synchronise",
                new Type[] { typeof(string), typeof(WebFormAppType), typeof(string) },
                userPermissions.ToArray(),
                null));
        }

        /// <summary>
        /// The Create_productRelease_Forbidden.
        /// </summary>
        [Fact]
        public void Create_productRelease_Forbidden()
        {
            List<string> userPermissions = new List<string>
            {
                Permission.ManageRoles.ToString(),
                Permission.ManageReleases.ToString(),
            };

            // Assert
            Assert.False(AuthorizationTest.IsAuthorized(
                this.productController,
                "Synchronise",
                new Type[] { typeof(string), typeof(WebFormAppType), typeof(string) },
                userPermissions.ToArray(),
                null));
        }

        /// <summary>
        /// The Get_ProductById_Success.
        /// </summary>
        [Fact]
        public void Get_ProductById_Success()
        {
            List<string> userPermissions = new List<string>
            {
                Permission.ManageRoles.ToString(),
                Permission.ViewProducts.ToString(),
            };

            // Assert
            Assert.True(AuthorizationTest.IsAuthorized(
                this.productController,
                "GetProductById",
                new Type[] { typeof(string), typeof(string), typeof(DeploymentEnvironment) },
                userPermissions.ToArray(),
                null));
        }

        /// <summary>
        /// The Get_ProductById_Forbidden.
        /// </summary>
        [Fact]
        public void Get_ProductById_Forbidden()
        {
            List<string> userPermissions = new List<string>
            {
                Permission.ManageRoles.ToString(),
                Permission.ViewPolicies.ToString(),
            };

            // Assert
            Assert.False(AuthorizationTest.IsAuthorized(
                this.productController,
                "GetProductById",
                new Type[] { typeof(string), typeof(string), typeof(DeploymentEnvironment) },
                userPermissions.ToArray(),
                null));
        }

        /// <summary>
        /// The Post_Product_Success.
        /// </summary>
        [Fact]
        public void Post_Product_Success()
        {
            List<string> userPermissions = new List<string>
            {
                Permission.ManageProducts.ToString(),
            };

            // Assert
            Assert.True(AuthorizationTest.IsAuthorized(
                this.productController,
                "CreateProduct",
                new Type[] { typeof(ProductCreateRequestModel) },
                userPermissions.ToArray(),
                null));
        }

        /// <summary>
        /// The Post_product_Forbidden.
        /// </summary>
        [Fact]
        public void Post_product_Forbidden()
        {
            List<string> userPermissions = new List<string>
            {
                Permission.ManageRoles.ToString(),
                Permission.ManageReleases.ToString(),
            };

            // Assert
            Assert.False(AuthorizationTest.IsAuthorized(
                this.productController,
                "CreateProduct",
                new Type[] { typeof(ProductCreateRequestModel) },
                userPermissions.ToArray(),
                null));
        }

        /// <summary>
        /// The Put_Product_Success.
        /// </summary>
        [Fact]
        public void Put_Product_Success()
        {
            List<string> userPermissions = new List<string>
            {
                Permission.ManageProducts.ToString(),
            };

            // Assert
            Assert.True(AuthorizationTest.IsAuthorized(
                this.productController,
                "UpdateProduct",
                new Type[] { typeof(string), typeof(ProductUpdateRequestModel) },
                userPermissions.ToArray(),
                null));
        }

        /// <summary>
        /// The Put_product_Forbidden.
        /// </summary>
        [Fact]
        public void Put_product_Forbidden()
        {
            List<string> userPermissions = new List<string>
            {
                Permission.ManageRoles.ToString(),
                Permission.ManageReleases.ToString(),
            };

            // Assert
            Assert.False(AuthorizationTest.IsAuthorized(
                this.productController,
                "UpdateProduct",
                new Type[] { typeof(string), typeof(ProductUpdateRequestModel) },
                userPermissions.ToArray(),
                null));
        }

        /// <summary>
        /// The Get_ReleasesForProduct_Success.
        /// </summary>
        [Fact]
        public void Get_ReleasesForProduct_Success()
        {
            string[] userPermissions = new string[]
            {
                Permission.ViewReleases.ToString(),
            };

            // Assert
            Assert.True(AuthorizationTest.IsAuthorized(
                this.productController,
                "GetReleasesForProduct",
                new Type[] { typeof(string), typeof(QueryOptionsModel) },
                userPermissions,
                null));
        }

        /// <summary>
        /// The Get_ReleasesForProduct_Forbidden.
        /// </summary>
        [Fact]
        public void Get_ReleasesForProduct_Forbidden()
        {
            string[] userPermissions = new string[]
            {
                Permission.ViewMyAccount.ToString(),
            };

            // Assert
            Assert.False(AuthorizationTest.IsAuthorized(
                this.productController,
                "GetReleasesForProduct",
                new Type[] { typeof(string), typeof(QueryOptionsModel) },
                userPermissions,
                null));
        }
    }
}
