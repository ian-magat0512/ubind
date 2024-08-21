// <copyright file="RoleTypePermissionsRegistry.cs" company="uBind">
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
    public class RoleTypePermissionsRegistry : IRoleTypePermissionsRegistry
    {
        private Dictionary<RoleType, List<Permission>> roleTypePermissions;
        private object mapLock = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="RoleTypePermissionsRegistry"/> class.
        /// </summary>
        public RoleTypePermissionsRegistry()
        {
            Role.SetRoleTypePermissionsRegistry(this);
            PermissionExtensions.SetRoleTypePermissionsRegistry(this);
        }

        /// <summary>
        /// Gets the permissions for the given RoleType.
        /// </summary>
        /// <param name="roleType">The RoleType.</param>
        /// <returns>The Permissions.</returns>
        public IReadOnlyList<Permission> GetPermissionsForRoleType(RoleType roleType)
        {
            lock (this.mapLock)
            {
                if (this.roleTypePermissions == null)
                {
                    this.PopulateUserTypePermissions();
                }
            }

            this.roleTypePermissions.TryGetValue(roleType, out var permissions);
            return permissions;
        }

        private void PopulateUserTypePermissions()
        {
            this.roleTypePermissions = new Dictionary<RoleType, List<Permission>>();
            IEnumerable<FieldInfo> fieldInfos = typeof(Permission).GetFields()
                .Where(f => f.GetCustomAttributes(typeof(PermittedForRoleTypeAttribute), false).Length > 0);
            foreach (var fieldInfo in fieldInfos)
            {
                string permissionName = fieldInfo.Name;
                Permission permission = (Permission)Enum.Parse(typeof(Permission), permissionName, true);
                var permittedUserTypeAttrs = fieldInfo.GetCustomAttributes(typeof(PermittedForRoleTypeAttribute), false);
                foreach (PermittedForRoleTypeAttribute permittedUserTypeAttr in permittedUserTypeAttrs)
                {
                    var roleTypeName = permittedUserTypeAttr.RoleType.ToString();
                    RoleType roleType = (RoleType)Enum.Parse(typeof(RoleType), roleTypeName, true);
                    this.AddPermissionToUserType(permission, roleType);
                }
            }
        }

        private void AddPermissionToUserType(Permission permission, RoleType roleType)
        {
            List<Permission> permissions;
            if (!this.roleTypePermissions.ContainsKey(roleType))
            {
                permissions = new List<Permission>();
                this.roleTypePermissions.Add(roleType, permissions);
            }
            else
            {
                this.roleTypePermissions.TryGetValue(roleType, out permissions);
            }

            permissions.Add(permission);
        }
    }
}
