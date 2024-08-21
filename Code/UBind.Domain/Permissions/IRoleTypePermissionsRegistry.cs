// <copyright file="IRoleTypePermissionsRegistry.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Permissions
{
    using System.Collections.Generic;

    /// <summary>
    /// Contains the permissions available per user type role, for fast lookup.
    /// This is gathered using Reflection and is done on first use and kept in memory
    /// so that subsequent lookups are fast.
    /// </summary>
    public interface IRoleTypePermissionsRegistry
    {
        /// <summary>
        /// Gets the permissions for the given UserType.
        /// </summary>
        /// <param name="roleType">The UserType.</param>
        /// <returns>The Permissions.</returns>
        IReadOnlyList<Permission> GetPermissionsForRoleType(RoleType roleType);
    }
}
