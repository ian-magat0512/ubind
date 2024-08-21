// <copyright file="DefaultRoleNameRegistry.cs" company="uBind">
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
    using Humanizer;
    using UBind.Domain.Entities;

    /// <summary>
    /// Creates a map of default role names to the DefaultRole enum.
    /// This is done on lazily and cached since reflection is slow.
    /// </summary>
    public class DefaultRoleNameRegistry : IDefaultRoleNameRegistry
    {
        private Dictionary<Tuple<string, RoleType>, DefaultRole> defaultRoleNames;
        private object mapLock = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultRoleNameRegistry"/> class.
        /// </summary>
        public DefaultRoleNameRegistry()
        {
            Role.SetDefaultRoleNameRegistry(this);
        }

        /// <summary>
        /// Gets the DefaultRole matching the given role name.
        /// </summary>
        /// <param name="roleName">The name of the role.</param>
        /// <param name="roleType">The RoleType.</param>
        /// <returns>The DefaultRole.</returns>
        public DefaultRole GetDefaultRoleForRoleName(string roleName, RoleType roleType)
        {
            lock (this.mapLock)
            {
                if (this.defaultRoleNames == null)
                {
                    this.PopulateDefaultRoleNames();
                }
            }

            var key = new Tuple<string, RoleType>(roleName, roleType);
            if (!this.defaultRoleNames.TryGetValue(key, out var defaultRole))
            {
                throw new ArgumentException(
                    $"When trying to get the DefaultRole for a role with the name {roleName} and "
                    + $"RoleType {roleType.Humanize()}, no such DefaultRole was found. "
                    + "Only \"Fixed\" roles have a matching DefaultRole, and they should be named "
                    + "according to the RoleInformation attribute on that DefaultRole enum field. "
                    + "If a fixed role was somehow renamed to something that no longer matched the "
                    + "RoleInformation attribute, that could be the cause of this.");
            }

            return defaultRole;
        }

        private void PopulateDefaultRoleNames()
        {
            this.defaultRoleNames = new Dictionary<Tuple<string, RoleType>, DefaultRole>();

            var defaultRoleEnumType = typeof(DefaultRole);
            IEnumerable<FieldInfo> defaultRoleFields = defaultRoleEnumType.GetFields()
                .Where(m => m.GetCustomAttributes(typeof(RoleInformationAttribute), false).Length > 0);
            foreach (var defaultRoleField in defaultRoleFields)
            {
                var roleInfoAttrs = defaultRoleField.GetCustomAttributes(typeof(RoleInformationAttribute), false);
                RoleInformationAttribute first = roleInfoAttrs.FirstOrDefault() as RoleInformationAttribute;
                DefaultRole defaultRole = (DefaultRole)Enum.Parse(typeof(DefaultRole), defaultRoleField.Name, true);
                var key = new Tuple<string, RoleType>(first.Name, first.RoleType);
                this.defaultRoleNames.Add(key, defaultRole);
            }
        }
    }
}
