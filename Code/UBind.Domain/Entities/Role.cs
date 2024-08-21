// <copyright file="Role.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Entities
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Linq.Expressions;
    using Humanizer;
    using NodaTime;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Permissions;
    using UBind.Domain.ReadModel.User;

    /// <summary>
    /// A uBind role.
    /// </summary>
    public class Role : MutableEntity<Guid>
    {
        /// <summary>
        /// Gets an expression mapping private property <see cref="Role.SerializedPermissions"/> requiring persistence for EF.
        /// </summary>
        public static readonly Expression<Func<Role, string>> PropertyListExpression =
            role => role.SerializedPermissions;

#pragma warning disable IDE0052 // Remove unread private members. This property is in use.
        private static IRoleTypePermissionsRegistry roleTypePermissionsRegistry;
#pragma warning restore IDE0052 // Remove unread private members
        private static IDefaultRolePermissionsRegistry defaultRolePermissionsRegistry;
        private static IDefaultRoleNameRegistry defaultRoleNameRegistry;
        private List<Permission> permissionsList = new List<Permission>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Role"/> class.
        /// </summary>
        /// <param name="tenantId">The tenant Id of the role.</param>
        /// <param name="organisationId">The organisation Id of the role.</param>
        /// <param name="role">The default role (e.g. Master, Client, Product Developer, Under writer, etc.)</param>
        /// <param name="createdTimestamp">The date and time the role is created.</param>
        public Role(Guid tenantId, Guid organisationId, DefaultRole role, Instant createdTimestamp)
            : base(Guid.NewGuid(), createdTimestamp)
        {
            var roleInfo = role.GetAttributeOfType<RoleInformationAttribute>();
            this.VerifyRoleInformation(tenantId, roleInfo.RoleType, roleInfo.IsFixed, roleInfo.Name);

            this.IsAdmin = roleInfo.IsFixed;
            this.Type = roleInfo.RoleType;
            this.TenantId = tenantId;
            this.OrganisationId = organisationId;
            this.Name = roleInfo.Name;
            this.Description = roleInfo.Description;
            this.permissionsList = defaultRolePermissionsRegistry.GetPermissionsForDefaultRole(
                defaultRoleNameRegistry.GetDefaultRoleForRoleName(this.Name, this.Type)).ToList();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Role"/> class.
        /// </summary>
        /// <param name="tenantId">The tenant id.</param>
        /// <param name="organisationId">The organisation Id of the role.</param>
        /// <param name="type">The role type.</param>
        /// <param name="name">The name of the role.</param>
        /// <param name="description">The description of the role.</param>
        /// <param name="createdTimestamp">The date and time the role created.</param>
        /// <param name="isFixed">Whether the role is fixed and cannot be edited.</param>
        public Role(
            Guid tenantId,
            Guid organisationId,
            RoleType type,
            string name,
            string description,
            Instant createdTimestamp,
            bool isFixed)
            : base(Guid.NewGuid(), createdTimestamp)
        {
            this.VerifyRoleInformation(tenantId, type, isFixed, name);

            this.IsAdmin = isFixed;
            this.Type = type;
            this.TenantId = tenantId;
            this.OrganisationId = organisationId;
            this.Name = name;
            this.Description = description;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Role"/> class.
        /// </summary>
        /// <remarks>
        /// A protected, parameterless constructor for EF, allowing proxy generation for lazy loading.
        /// .</remarks>
        protected Role()
            : base(default, default)
        {
        }

        /// <summary>
        /// Gets a value indicating whether the role is a fixed role where the permissions cannot be changed.
        /// </summary>
        /// <remarks>Admin roles automatically have all permissions available for their role type.</remarks>
        public bool IsAdmin { get; private set; }

        /// <summary>
        /// Gets the ID of the tenant of the Role.
        /// </summary>
        public Guid TenantId { get; private set; }

        /// <summary>
        /// Gets or sets the ID of the organisation of the Role.
        /// </summary>
        public Guid OrganisationId { get; set; }

        /// <summary>
        /// Gets the type of the role.
        /// </summary>
        public RoleType Type { get; private set; }

        /// <summary>
        /// Gets the name of the role.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the description of the role.
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the role's permissions can be modified.
        /// </summary>
        public bool IsEditable => !this.IsAdmin;

        /// <summary>
        /// Gets a value indicating whether the role cannot be modified or deleted.
        /// </summary>
        public bool IsFixed => this.IsAdmin;

        /// <summary>
        ///  Gets a value indicating whether the role can be deleted.
        /// </summary>
        public bool IsDeletable => !this.IsPermanent();

        /// <summary>
        ///  Gets a value indicating whether the role can be renamed (including editing description).
        /// </summary>
        public bool IsRenamable => this.IsDeletable;

        /// <summary>
        /// Gets a value indicating whether this is the "Tenant Admin" role.
        /// </summary>
        public bool IsTenantAdmin => this.Name == "Tenant Admin";

        /// <summary>
        /// Gets a value indicating whether this is the "Organisation Admin" role.
        /// </summary>
        public bool IsOrganisationAdmin => this.Name == "Organisation Admin";

        /// <summary>
        /// Gets the users whom this role has been assigned to.
        /// </summary>
        /// <remarks>
        /// Only here for EF to generate the join table for many-to-many relationship.
        /// .</remarks>
        public virtual ICollection<UserReadModel> Users { get; private set; } = new Collection<UserReadModel>();

        /// <summary>
        /// Gets the permissions of the role.
        /// </summary>
        public IEnumerable<Permission> Permissions =>
            this.IsFixed
                ? defaultRolePermissionsRegistry.GetPermissionsForDefaultRole(
                    defaultRoleNameRegistry.GetDefaultRoleForRoleName(this.Name, this.Type))
                : this.permissionsList;

        // For persisting permissions in single column in database.
        private string SerializedPermissions
        {
            get
            {
                return string.Join(",", this.permissionsList);
            }

            set
            {
                // all of these will need to be removed after the UB-4685 released.
                this.permissionsList = value
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Where(s => s != "BindQuotes") // we've removed the BindQuotes permission
                    .Select(s => s == "Imports" ? "ImportData" : s) // we've renamed the Imports permission to "ImportData"
                    .Select(s => s == "ManageUBindAdminUsers" ? "ManageMasterAdminUsers" : s) // we've renamed this
                    .Select(s => s == "UpdatePolicies" ? "ManagePolicies" : s) // we've renamed this
                    .Select(s => s == "AdjustPolicies" ? "ManagePolicies" : s) // Added for UB-4685
                    .Select(s => s == "RenewPolicies" ? "ManagePolicies" : s) // Added for UB-4685
                    .Select(s => s == "CancelPolicies" ? "ManagePolicies" : s) // Added for UB-4685
                    .Select(s => s == "AssignClaimNumbers" ? "ManageClaims" : s) // Added for UB-4685
                    .Select(s => s == "AssociateClaims" ? "ManageClaims" : s) // Added for UB-4685
                    .Select(s => s == "ManageClientAdminUsers" ? "ManageTenantAdminUsers" : s) // Added for UB-4685
                    .Select(s => s == "ViewEmails" ? "ViewMessages" : s)
                    .Select(s => s == "ManageEmails" ? "ManageMessages" : s)
                    .Select(s => s == "ViewAllEmails" ? "ViewAllMessages" : s)
                    .Select(s => s == "ManageAllEmails" ? "ManageAllMessages" : s)
                    .Select(s => s.ToEnumOrThrow<Permission>())
                    .ToList();
            }
        }

        /// <summary>
        /// Stores a reference to the IRoleTypePermissionsRegistry dependency for use within.
        /// this class.
        /// </summary>
        /// <param name="roleTypePermissionsRegistry">The role type permissions registry.</param>
        public static void SetRoleTypePermissionsRegistry(IRoleTypePermissionsRegistry roleTypePermissionsRegistry)
        {
            Role.roleTypePermissionsRegistry = roleTypePermissionsRegistry;
        }

        /// <summary>
        /// Stores a reference to the IDefaultRolePermissionsRegistry dependency for use within.
        /// this class.
        /// </summary>
        /// <param name="defaultRolePermissionsRegistry">The default role permissions registry.</param>
        public static void SetDefaultRolePermissionsRegistry(
            IDefaultRolePermissionsRegistry defaultRolePermissionsRegistry)
        {
            Role.defaultRolePermissionsRegistry = defaultRolePermissionsRegistry;
        }

        /// <summary>
        /// Stores a reference to the IDefaultRoleNameRegistry dependency for use within.
        /// this class.
        /// </summary>
        /// <param name="defaultRoleNameRegistry">The default role permissions registry.</param>
        public static void SetDefaultRoleNameRegistry(
            IDefaultRoleNameRegistry defaultRoleNameRegistry)
        {
            Role.defaultRoleNameRegistry = defaultRoleNameRegistry;
        }

        /// <summary>
        /// Updates the role.
        /// </summary>
        /// <param name="name">A descriptive name for the role.</param>
        /// <param name="description"> A descrtion of the role.</param>
        /// <param name="lastModifiedTimestamp">The date time the role was modified.</param>
        public void Update(string name, string description, Instant lastModifiedTimestamp)
        {
            if (!this.IsRenamable)
            {
                throw new ErrorException(Errors.Role.CannotUpdateDefaultRole(this.Name));
            }

            this.Name = name;
            this.Description = description;
            this.LastModifiedTimestamp = lastModifiedTimestamp;
        }

        /// <summary>
        /// Create a permission for a role.
        /// </summary>
        /// <param name="permission">Permission type.</param>
        /// <param name="lastModifiedTimestamp">The date time the role was modified.</param>
        public void AddPermission(Permission permission, Instant lastModifiedTimestamp)
        {
            if (permission == Permission.ManageTenantAdminUsers)
            {
                throw new ErrorException(Errors.Role.PermissionCannotBeAdded(
                    permission.Humanize(),
                    this.Type.Humanize(),
                    this.Name,
                    "Please assign the \"Tenant Admin\" role to the user instead. "));
            }

            if (permission == Permission.ManageOrganisationAdminUsers)
            {
                throw new ErrorException(Errors.Role.PermissionCannotBeAdded(
                    permission.Humanize(),
                    this.Type.Humanize(),
                    this.Name,
                    "Please assign the \"Organisation Admin\" role to the user instead. "));
            }

            if (!this.IsEditable)
            {
                throw new ErrorException(Errors.Role.CannotUpdateDefaultRole(this.Name));
            }

            if (!permission.IsAssignableToRoleType(this.Type))
            {
                throw new ErrorException(Errors.Role.CannotAssignPermissionToRoleOfType(permission.Humanize(), this.Type.Humanize(), this.Name));
            }

            if (this.permissionsList.Contains(permission))
            {
                throw new ErrorException(Errors.Role.AlreadyContainsPermission(this.Name, permission.Humanize()));
            }

            this.permissionsList.Add(permission);
            this.LastModifiedTimestamp = lastModifiedTimestamp;
        }

        /// <summary>
        /// Remove a permission for a role.
        /// </summary>
        /// <param name="permission">Permission type.</param>
        /// <param name="lastModifiedTimestamp">The date time the role was modified.</param>
        public void RemovePermission(Permission permission, Instant lastModifiedTimestamp)
        {
            if (!this.IsEditable)
            {
                throw new ErrorException(Errors.Role.CannotUpdateDefaultRole(this.Name));
            }

            if (!this.permissionsList.Contains(permission))
            {
                throw new ErrorException(Errors.Role.RemovePermissionNotFound(this.Name, permission.Humanize()));
            }

            this.permissionsList.Remove(permission);
            this.LastModifiedTimestamp = lastModifiedTimestamp;
        }

        /// <summary>
        /// Checks if role is one of the permanent roles.
        /// </summary>
        /// <returns>true if the name is of the permanent roles otherwise false.</returns>
        public bool IsPermanent()
        {
            var clock = SystemClock.Instance;
            var defaultRoles = ((DefaultRole[])Enum.GetValues(typeof(DefaultRole))).Select(c => c.GetAttributeOfType<RoleInformationAttribute>()).Where(c => c.IsFixed || c.RoleType == RoleType.Customer);

            return defaultRoles.Any(a =>
                a.RoleType == this.Type
                && a.Name.EqualsIgnoreCase(this.Name)
                && a.Description.EqualsIgnoreCase(a.Description)
                && a.IsFixed == this.IsFixed);
        }

        /// <summary>
        /// Removes duplicate permissions just in case there are permissions that where inputted twice in the list.
        /// </summary>
        public void RemoveDuplicatePermissions()
        {
            this.permissionsList = this.permissionsList.Distinct().ToList();
        }

        private void VerifyRoleInformation(Guid tenantId, RoleType type, bool isFixed, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ErrorException(Errors.Role.NameIsBlank());
            }

            if (type == RoleType.Customer && tenantId == Tenant.MasterTenantId)
            {
                throw new ErrorException(Errors.Role.NoCustomerRolesInMasterTenant());
            }

            if (isFixed && type == RoleType.Customer)
            {
                throw new ErrorException(Errors.Role.CustomersCannotBeAdmins());
            }
        }
    }
}
