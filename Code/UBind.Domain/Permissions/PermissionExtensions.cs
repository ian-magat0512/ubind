// <copyright file="PermissionExtensions.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Permissions
{
    using System;
    using System.ComponentModel;
    using System.Linq;
    using System.Reflection;
    using UBind.Domain.Entities;

    /// <summary>
    /// Extension methods for <see cref="Permission"/>.
    /// </summary>
    public static class PermissionExtensions
    {
        private static IRoleTypePermissionsRegistry roleTypePermissionsRegistry;
        private static IDefaultRolePermissionsRegistry defaultRolePermissionsRegistry;

        /// <summary>
        /// Stores a reference to the IRoleTypePermissionsRegistry dependency for use within
        /// this static class.
        /// </summary>
        /// <param name="roleTypePermissionsRegistry">The role type permissions registry.</param>
        public static void SetRoleTypePermissionsRegistry(IRoleTypePermissionsRegistry roleTypePermissionsRegistry)
        {
            PermissionExtensions.roleTypePermissionsRegistry = roleTypePermissionsRegistry;
        }

        /// <summary>
        /// Stores a reference to the IDefaultRolePermissionsRegistry dependency for use within
        /// this static class.
        /// </summary>
        /// <param name="defaultRolePermissionsRegistry">The default role permissions registry.</param>
        public static void SetDefaultRolePermissionsRegistry(
            IDefaultRolePermissionsRegistry defaultRolePermissionsRegistry)
        {
            PermissionExtensions.defaultRolePermissionsRegistry = defaultRolePermissionsRegistry;
        }

        /// <summary>
        /// Extension to get the concern a permission relates to.
        /// </summary>
        /// <param name="permission">The permission.</param>
        /// <returns>The concern the permission relates to.</returns>
        /// <exception cref="InvalidEnumArgumentException">Thrown when value is missing required Concern attribute.</exception>
        public static Concern GetConcern(this Permission permission)
        {
            var concernAttribute = (ConcernAttribute)typeof(Permission)
                .GetField(Enum.GetName(typeof(Permission), permission))
                .GetCustomAttributes(typeof(ConcernAttribute), false)
                .SingleOrDefault();

            if (concernAttribute != null)
            {
                return concernAttribute.Concern;
            }

            throw new InvalidEnumArgumentException(
                $"{nameof(Permission)} Enum's value {permission.ToString()} is missing the required Concern attribute");
        }

        /*
        /// <summary>
        /// Extension to check if a permission is assignable to a role type.
        /// </summary>
        /// <param name="permissionType">The permission.</param>
        /// <param name="roleType">The role type.</param>
        /// <returns>true, if the permission is assignable to the role type, otherwise false.</returns>
        public static bool IsAssignableToRoleType(this Permission permissionType, RoleType roleType)
        {
            var role = DefaultRole.ClientAdmin;
            if (roleType == RoleType.Client)
            {
                role = DefaultRole.ClientAdmin;
            }

            if (roleType == RoleType.Master)
            {
                role = DefaultRole.MasterAdmin;
            }

            if (roleType == RoleType.Customer)
            {
                role = DefaultRole.Customer;
            }

            var hasPermittedRole = typeof(Permission)
                .GetField(Enum.GetName(typeof(Permission), permissionType))
                .GetCustomAttributes(typeof(PermittedRoleAttribute), false)
                .Any(attribute => ((PermittedRoleAttribute)attribute).Role == role);

            var hasOptionalPermittedRole = typeof(Permission)
                .GetField(Enum.GetName(typeof(Permission), permissionType))
                .GetCustomAttributes(typeof(OptionalPermittedRoleAttribute), false)
                .Any(attribute => ((OptionalPermittedRoleAttribute)attribute).Role == role);

            return hasPermittedRole || hasOptionalPermittedRole;
        }
        */

        /// <summary>
        /// Extension to check if a permission is assignable to a role type.
        /// </summary>
        /// <param name="permissionType">The permission.</param>
        /// <param name="role">The role.</param>
        /// <returns>true, if the permission is assignable to the role type, otherwise false.</returns>
        public static bool IsAssignableToRole(this Permission permissionType, Role role)
        {
            var defaultRoleEnumType = typeof(DefaultRole);
            MemberInfo defaultRoleMember = defaultRoleEnumType.GetMembers()
                .Where(m =>
                {
                    var roleInfoAttrs = m.GetCustomAttributes(typeof(RoleInformationAttribute), false);
                    if (roleInfoAttrs.Length == 0)
                    {
                        return false;
                    }
                    else
                    {
                        RoleInformationAttribute first = roleInfoAttrs.FirstOrDefault() as RoleInformationAttribute;
                        return first.Name == role.Name;
                    }
                }).FirstOrDefault();
            string defaultRoleName = defaultRoleMember.Name;

            var hasPermittedRole = typeof(Permission)
                .GetField(Enum.GetName(typeof(Permission), permissionType))
                .GetCustomAttributes(typeof(PermittedForDefaultRoleAttribute), false)
                .Any(attribute =>
                    ((PermittedForDefaultRoleAttribute)attribute).Role.ToString() == defaultRoleName);

            var hasOptionalPermittedRole = typeof(Permission)
                .GetField(Enum.GetName(typeof(Permission), permissionType))
                .GetCustomAttributes(typeof(OptionallyPermittedForDefaultRoleAttribute), false)
                .Any(attribute =>
                    ((OptionallyPermittedForDefaultRoleAttribute)attribute).Role.ToString() == defaultRoleName);

            return hasPermittedRole || hasOptionalPermittedRole;
        }

        /// <summary>
        /// Extension to check if a permission is assignable to a role type.
        /// </summary>
        /// <param name="permission">The permission.</param>
        /// <param name="roleType">The user type.</param>
        /// <returns>true, if the permission is assignable to the role type, otherwise false.</returns>
        public static bool IsAssignableToRoleType(this Permission permission, RoleType roleType)
        {
            return roleTypePermissionsRegistry.GetPermissionsForRoleType(roleType).Contains(permission);
        }

        /// <summary>
        /// Extension to check if a permission is assignable to a role type.
        /// </summary>
        /// <param name="permissionType">The permission.</param>
        /// <param name="role">The role type.</param>
        /// <returns>true, if the permission is assignable to the role type, otherwise false.</returns>
        public static bool IsAssignableToRole(this Permission permissionType, DefaultRole role) =>
            typeof(Permission)
                .GetField(Enum.GetName(typeof(Permission), permissionType))
                .GetCustomAttributes(typeof(PermittedForDefaultRoleAttribute), false)
                .Any(attribute => ((PermittedForDefaultRoleAttribute)attribute).Role == role);
    }
}
