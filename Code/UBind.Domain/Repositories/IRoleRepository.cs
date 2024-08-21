// <copyright file="IRoleRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Repositories
{
    using System;
    using System.Collections.Generic;
    using CSharpFunctionalExtensions;
    using UBind.Domain.Entities;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// A repository for Roles.
    /// </summary>
    public interface IRoleRepository
    {
        /// <summary>
        /// Insert new role.
        /// </summary>
        /// <param name="role">A role to add.</param>
        void Insert(Role role);

        /// <summary>
        /// Get the roles by tenant and filter.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="filters">Role model filter.</param>
        /// <returns>List of roles per tenant.</returns>
        IReadOnlyList<Role> GetRoles(Guid tenantId, RoleReadModelFilters filters);

        /// <summary>
        /// Insert new role.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="roleId">The role ID.</param>
        /// <returns>A role object.</returns>
        Role GetRoleById(Guid tenantId, Guid roleId);

        /// <summary>
        /// Get role by name and tenant id.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="roleName">The role name.</param>
        /// <returns>A role instance, maybe.</returns>
        Maybe<Role> TryGetRoleByName(Guid tenantId, string roleName);

        /// <summary>
        /// Get role by name and tenant id, or throw if it does not find one.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="roleName">The role name.</param>
        /// <returns>A role object.</returns>
        Role GetRoleByNameOrThrow(Guid tenantId, string roleName);

        /// <summary>
        /// Gets the master admin role.
        /// </summary>
        /// <returns>The master admin role .</returns>
        Role GetMasterAdminRole();

        /// <summary>
        /// Gets the customer role for a given tenant.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant.</param>
        /// <returns>The customer role for the tenant.</returns>
        Role GetCustomerRoleForTenant(Guid tenantId);

        /// <summary>
        /// Gets the admin role for a given tenant.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant.</param>
        /// <returns>The admin role for the tenant.</returns>
        Role GetAdminRoleForTenant(Guid tenantId);

        /// <summary>
        /// Checks to see if a given name is available for use by a role in a given tenant.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant to check in.</param>
        /// <param name="name">The name to check.</param>
        /// <param name="roleId">The ID of the role the name would be for, or default if not specified.</param>
        /// <returns><c>true</c> if the name is already existing for tenant, otherwise <c>false</c>.</returns>
        bool IsNameInUse(Guid tenantId, string name, Guid roleId = default);

        /// <summary>
        /// Deletes a role.
        /// </summary>
        /// <param name="role">The role to delete.</param>
        /// <returns>true.</returns>
        bool Delete(Role role);

        /// <summary>
        /// Saves the current context.
        /// </summary>
        void SaveChanges();
    }
}
