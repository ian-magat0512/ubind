// <copyright file="RoleSummaryModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using System;
    using UBind.Domain.Entities;
    using UBind.Domain.Extensions;
    using UBind.Domain.Permissions;

    /// <summary>
    /// Resource model for role summary (for use in role list).
    /// </summary>
    public class RoleSummaryModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RoleSummaryModel"/> class.
        /// </summary>
        /// <param name="role">The role.</param>
        public RoleSummaryModel(Role role)
        {
            this.Id = role.Id;
            this.Name = role.Name;
            this.OrganisationId = role.OrganisationId;
            this.Description = role.Description;
            this.IsFixed = role.IsAdmin;
            this.ArePermissionsEditable = !role.IsAdmin;
            this.IsDeletable = role.IsDeletable;
            this.IsRenamable = role.IsRenamable;
            this.IsPermanentRole = role.IsPermanent();
            this.Type = role.Type;
            this.CreatedDateTime = role.CreatedTimestamp.ToExtendedIso8601String();
            this.LastModifiedDateTime = role.LastModifiedTimestamp.ToExtendedIso8601String();
        }

        /// <summary>
        /// Gets or sets the Id of the Role.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the Role.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the Role.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets a value indicating whether the role is an admin role
        /// (and thus not deletable, renamable, and permissions cannot be modified).
        /// </summary>
        public bool IsFixed { get; private set; }

        /// <summary>
        ///  Gets a value indicating whether the role can be deleted.
        /// </summary>
        public bool IsDeletable { get; private set; }

        /// <summary>
        ///  Gets a value indicating whether the role can be renamed (including editing description).
        /// </summary>
        public bool IsRenamable { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the role is the default or not.
        /// </summary>
        public bool IsPermanentRole { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the permissions for this role are editable.
        /// If true, permissions can be added to this role, deleted from the role, and edited.
        /// </summary>
        public bool ArePermissionsEditable { get; private set; }

        /// <summary>
        /// Gets the type of the role.
        /// </summary>
        public RoleType Type { get; private set; }

        /// <summary>
        /// Gets a timestamp which represents when the role was created.
        /// </summary>
        public string CreatedDateTime { get; private set; }

        /// <summary>
        /// Gets a timestamp when the role was modified.
        /// </summary>
        public string LastModifiedDateTime { get; private set; }

        /// <summary>
        /// Gets the organisation id of the role.
        /// </summary>
        public Guid? OrganisationId { get; private set; }
    }
}
