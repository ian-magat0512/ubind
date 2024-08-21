// <copyright file="PermissionModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using Humanizer;
    using UBind.Domain.Permissions;

    /// <summary>
    /// Resource model for permissions.
    /// </summary>
    public class PermissionModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PermissionModel"/> class.
        /// </summary>
        /// <param name="permissionType">The permission of the role.</param>
        public PermissionModel(Permission permissionType)
        {
            this.Type = permissionType;
            this.Concern = this.Type.GetConcern();
            this.Description = this.Type.Humanize();
        }

        /// <summary>
        /// Gets or sets the reference Id for the permission.
        /// </summary>
        public Permission Type { get; set; }

        /// <summary>
        /// Gets or sets the Description for the permission.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the GroupName of the permission.
        /// </summary>
        public Concern Concern { get; set; }
    }
}
