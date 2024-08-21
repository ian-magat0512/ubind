// <copyright file="DefaultRolePermissionsRegistry.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Permissions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using UBind.Domain.Entities;

    /// <summary>
    /// Contains the permissions for a default role, for fast lookup.
    /// This is gathered using Reflection and is done on first use and kept in memory
    /// so that subsequent lookups are fast.
    /// </summary>
    public class DefaultRolePermissionsRegistry : IDefaultRolePermissionsRegistry
    {
        private Dictionary<DefaultRole, List<Permission>> defaultRolePermissions;
        private object mapLock = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultRolePermissionsRegistry"/> class.
        /// </summary>
        public DefaultRolePermissionsRegistry()
        {
            Role.SetDefaultRolePermissionsRegistry(this);
            PermissionExtensions.SetDefaultRolePermissionsRegistry(this);
        }

        /// <summary>
        /// Gets the permissions for the given DefaultRole.
        /// </summary>
        /// <param name="defaultRole">The DefaultRole.</param>
        /// <returns>The Permissions.</returns>
        public IReadOnlyList<Permission> GetPermissionsForDefaultRole(DefaultRole defaultRole)
        {
            lock (this.mapLock)
            {
                if (this.defaultRolePermissions == null)
                {
                    this.PopulateDefaultRolePermissions();
                }
            }

            this.defaultRolePermissions.TryGetValue(defaultRole, out var permissions);
            return permissions;
        }

        private void PopulateDefaultRolePermissions()
        {
            this.defaultRolePermissions = new Dictionary<DefaultRole, List<Permission>>();
            IEnumerable<FieldInfo> fieldInfos = typeof(Permission).GetFields()
                .Where(f => f.GetCustomAttributes(typeof(PermittedForDefaultRoleAttribute), false).Length > 0);
            foreach (var fieldInfo in fieldInfos)
            {
                string permissionName = fieldInfo.Name;
                Permission permission = (Permission)Enum.Parse(typeof(Permission), permissionName, true);
                var permittedRoleAttrs = fieldInfo.GetCustomAttributes(typeof(PermittedForDefaultRoleAttribute), false);
                foreach (PermittedForDefaultRoleAttribute permittedRoleAttr in permittedRoleAttrs)
                {
                    var roleName = permittedRoleAttr.Role.ToString();
                    DefaultRole defaultRole = (DefaultRole)Enum.Parse(typeof(DefaultRole), roleName, true);
                    this.AddPermissionToDefaultRole(permission, defaultRole);
                }

                var optionalPermittedRoleAttrs =
                    fieldInfo.GetCustomAttributes(typeof(OptionallyPermittedForDefaultRoleAttribute), false);
                foreach (OptionallyPermittedForDefaultRoleAttribute optionalPermittedRoleAttr in optionalPermittedRoleAttrs)
                {
                    var roleName = optionalPermittedRoleAttr.Role.ToString();
                    DefaultRole defaultRole = (DefaultRole)Enum.Parse(typeof(DefaultRole), roleName, true);
                    this.AddPermissionToDefaultRole(permission, defaultRole);
                }
            }
        }

        private void AddPermissionToDefaultRole(Permission permission, DefaultRole defaultRole)
        {
            List<Permission> rolePermissions;
            if (!this.defaultRolePermissions.ContainsKey(defaultRole))
            {
                rolePermissions = new List<Permission>();
                this.defaultRolePermissions.Add(defaultRole, rolePermissions);
            }
            else
            {
                this.defaultRolePermissions.TryGetValue(defaultRole, out rolePermissions);
            }

            rolePermissions.Add(permission);
        }
    }
}
