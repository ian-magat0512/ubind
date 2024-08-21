// <copyright file="IRoleService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using UBind.Domain.Permissions;
    using UBind.Domain.ReadModel;
    using Entities = UBind.Domain.Entities;

    /// <summary>
    /// Service for managing Role creation and initialization.
    /// </summary>
    public interface IRoleService
    {
        /// <summary>
        /// Retrieves a collection of Roles.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="filters">Role model filter.</param>
        /// <returns>List of roles.</returns>
        IReadOnlyList<Entities.Role> GetRoles(Guid tenantId, RoleReadModelFilters filters);

        /// <summary>
        /// Retrieves a given role.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="roleId">The ID of the role.</param>
        /// <returns>The role, if found otherwise null.</returns>
        Entities.Role GetRole(Guid tenantId, Guid roleId);

        /// <summary>
        /// Creates a role.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="organisationId">The organisation ID.</param>
        /// <param name="name">The name of the role.</param>
        /// <param name="roleType">The role type.</param>
        /// <param name="description">The description of the role.</param>
        /// <returns>a newly created role.</returns>
        Task<Entities.Role> CreateRole(Guid tenantId, Guid organisationId, string name, RoleType roleType, string description);

        /// <summary>
        /// Create default roles for tenant.
        /// </summary>
        /// <param name="tenant">The tenant information.</param>
        void CreateDefaultRolesForTenant(Domain.Tenant tenant);

        /// <summary>
        /// Updates a role.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="roleId">The id of the role.</param>
        /// <param name="name">The name of the role.</param>
        /// <param name="description">The description of the role.</param>
        /// <returns>an updated role.</returns>
        Entities.Role UpdateRole(Guid tenantId, Guid roleId, string name, string description);

        /// <summary>
        /// Deletes a role.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="roleId">The id of the role.</param>
        /// <returns>true.</returns>
        bool DeleteRole(Guid tenantId, Guid roleId);

        /// <summary>
        /// Adds permission to a role.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="roleId">The ID of the role.</param>
        /// <param name="permissionType">The permission.</param>
        /// <returns>A role permission object.</returns>
        Permission AddPermissionToRole(Guid tenantId, Guid roleId, Permission permissionType);

        /// <summary>
        /// Update permission of a role.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="roleId">The ID of the role.</param>
        /// <param name="previousPermissionType">The previous permission to update.</param>
        /// <param name="newPermissionType">The new permission.</param>
        /// <returns>A role permission object.</returns>
        Permission UpdatePermissionOfARole(Guid tenantId, Guid roleId, Permission previousPermissionType, Permission newPermissionType);

        /// <summary>
        /// Removes a permission from a role.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="roleId">The ID of the role.</param>
        /// <param name="permissionType">The permission.</param>
        /// <returns>A role permission object.</returns>
        bool RemovePermissionFromRole(Guid tenantId, Guid roleId, Permission permissionType);
    }
}
