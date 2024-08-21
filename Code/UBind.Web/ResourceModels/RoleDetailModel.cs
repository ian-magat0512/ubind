// <copyright file="RoleDetailModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UBind.Domain.Entities;
    using UBind.Domain.Helpers;
    using UBind.Domain.Permissions;

    /// <summary>
    /// Resource model for Roles.
    /// </summary>
    public class RoleDetailModel : RoleSummaryModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RoleDetailModel"/> class.
        /// </summary>
        /// <param name="role">The product.</param>
        public RoleDetailModel(Role role)
            : base(role)
        {
            this.Permissions = this.SortPermissions(role.Permissions);
        }

        /// <summary>
        /// Gets the role's assigned permissions.
        /// </summary>
        public IEnumerable<PermissionModel> Permissions { get; private set; }

        private IReadOnlyList<PermissionModel> SortPermissions(IEnumerable<Permission> permissions)
        {
            List<PermissionModel> sortedPermissions = new List<PermissionModel>();
            var orderedEnumValues = Enum<Permission>.GetValues();
            foreach (var value in orderedEnumValues)
            {
                if (permissions.Contains(value))
                {
                    sortedPermissions.Add(new PermissionModel(value));
                }
            }

            return sortedPermissions;
        }
    }
}
