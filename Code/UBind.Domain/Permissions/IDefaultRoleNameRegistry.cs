// <copyright file="IDefaultRoleNameRegistry.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Permissions
{
    /// <summary>
    /// Provides a way to get a DefaultRole for a given role name.
    /// </summary>
    public interface IDefaultRoleNameRegistry
    {
        /// <summary>
        /// Gets the DefaultRole matching the given role name.
        /// </summary>
        /// <param name="roleName">The name of the role.</param>
        /// <param name="roleType">The RoleType.</param>
        /// <returns>The DefaultRole.</returns>
        DefaultRole GetDefaultRoleForRoleName(string roleName, RoleType roleType);
    }
}
