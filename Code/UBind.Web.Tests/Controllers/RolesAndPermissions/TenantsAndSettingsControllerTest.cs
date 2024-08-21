// <copyright file="TenantsAndSettingsControllerTest.cs" company="uBind">
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
    using UBind.Domain.Permissions;
    using UBind.Web.Controllers;
    using UBind.Web.ResourceModels;
    using Xunit;

    /// <summary>
    /// Defines the <see cref="TenantsAndSettingsControllerTest" />.
    /// </summary>
    public class TenantsAndSettingsControllerTest
    {
        /// <summary>
        /// Defines the tenantController.
        /// </summary>
        private TenantController tenantController;

        /// <summary>
        /// Defines the settingController.
        /// </summary>
        private FeatureSettingController settingController;

        public TenantsAndSettingsControllerTest()
        {
            this.tenantController = new TenantController(null, null, null, null);
            this.settingController = new FeatureSettingController(null, null, null, null, null);
        }

        /// <summary>
        /// The Get_Tenants_Success.
        /// </summary>
        [Fact]
        public void Get_Tenants_Success()
        {
            List<string> userPermissions = new List<string>
            {
                Permission.ViewTenants.ToString(),
            };

            // Assert
            Assert.True(AuthorizationTest.IsAuthorized(
                this.tenantController,
                "GetAllTenants",
                new Type[] { typeof(TenantQueryOptionsModel) },
                userPermissions.ToArray(),
                null));
        }

        /// <summary>
        /// The Get_Tenants_Forbidden.
        /// </summary>
        [Fact]
        public void Get_Tenants_Forbidden()
        {
            List<string> userPermissions = new List<string>
            {
                Permission.ManageCustomers.ToString(),
            };

            // Act

            // Assert
            Assert.True(!AuthorizationTest.IsAuthorized(
                this.tenantController,
                "GetAllTenants",
                new Type[] { typeof(TenantQueryOptionsModel) },
                userPermissions.ToArray(),
                null));
        }

        /// <summary>
        /// The Get_TenantsDetails_Forbidden.
        /// </summary>
        [Fact]
        public void Get_TenantsDetails_Forbidden()
        {
            List<string> userPermissions = new List<string>
            {
                Permission.ManageTenants.ToString(),
            };

            // Assert
            Assert.False(AuthorizationTest.IsAuthorized(
                this.tenantController,
                "GetTenant",
                new Type[] { typeof(string) },
                userPermissions.ToArray(),
                null));
        }

        /// <summary>
        /// The Create_Tenant_Success.
        /// </summary>
        [Fact]
        public void Create_Tenant_Success()
        {
            List<string> userPermissions = new List<string>
            {
                Permission.ManageTenants.ToString(),
            };

            // Assert
            Assert.True(AuthorizationTest.IsAuthorized(
                this.tenantController,
                "CreateTenant",
                new Type[] { typeof(TenantModel) },
                userPermissions.ToArray(),
                null));
        }

        /// <summary>
        /// The Create_Tenant_Forbidden.
        /// </summary>
        [Fact]
        public void Create_Tenant_Forbidden()
        {
            List<string> userPermissions = new List<string>
            {
                Permission.ManageRoles.ToString(),
                Permission.ManageProducts.ToString(),
            };

            // Assert
            Assert.False(AuthorizationTest.IsAuthorized(
                this.tenantController,
                "CreateTenant",
                new Type[] { typeof(TenantModel) },
                userPermissions.ToArray(),
                null));
        }

        /// <summary>
        /// The Update_Tenant_Success.
        /// </summary>
        [Fact]
        public void Update_Tenant_Success()
        {
            List<string> userPermissions = new List<string>
            {
                Permission.ManageTenants.ToString(),
            };

            // Assert
            Assert.True(AuthorizationTest.IsAuthorized(
                this.tenantController,
                "UpdateTenant",
                new Type[] { typeof(string), typeof(TenantModel) },
                userPermissions.ToArray(),
                null));
        }

        /// <summary>
        /// The Update_Tenant_Forbidden.
        /// </summary>
        [Fact]
        public void Update_Tenant_Forbidden()
        {
            List<string> userPermissions = new List<string>
            {
                Permission.ManageRoles.ToString(),
                Permission.ManageProducts.ToString(),
            };

            // Assert
            Assert.False(AuthorizationTest.IsAuthorized(
                this.tenantController,
                "UpdateTenant",
                new Type[] { typeof(string), typeof(TenantModel) },
                userPermissions.ToArray(),
                null));
        }

        /// <summary>
        /// The GetTenantName_Anonymous.
        /// </summary>
        [Fact]
        public void GetTenantName_Anonymous()
        {
            // Assert
            Assert.True(AuthorizationTest.IsAnonymous(
                this.tenantController,
                "GetTenantName",
                new Type[] { typeof(string) }));
        }

        /// <summary>
        /// The Put_TenantSettings_Success.
        /// </summary>
        [Fact]
        public void Put_TenantSettings_Success()
        {
            List<string> userPermissions = new List<string>
            {
                Permission.ManageTenants.ToString(),
            };

            // Assert
            Assert.True(AuthorizationTest.IsAuthorized(
                this.settingController,
                "UpdateTenantSetting",
                new Type[] { typeof(string), typeof(string), typeof(SettingModel) },
                userPermissions.ToArray(),
                null));
        }

        /// <summary>
        /// The Put_TenantSettings_Forbidden.
        /// </summary>
        [Fact]
        public void Put_TenantSettings_Forbidden()
        {
            List<string> userPermissions = new List<string>
            {
                Permission.AccessProductionData.ToString(),
            };

            // Assert
            Assert.False(AuthorizationTest.IsAuthorized(
                this.settingController,
                "UpdateSetting",
                new Type[] { typeof(string), typeof(string), typeof(string), typeof(SettingModel) },
                userPermissions.ToArray(),
                null));
        }
    }
}
