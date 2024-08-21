// <copyright file="Role.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

#pragma warning disable SA1118 // Parameter should not span multiple lines

namespace UBind.Domain
{
    using System;
    using System.Net;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Allows enumeration of all application errors as outlined here: https://enterprisecraftsmanship.com/posts/advanced-error-handling-techniques/.
    /// </summary>
    public static partial class Errors
    {
        /// <summary>
        /// Role errors.
        /// </summary>
        public static class Role
        {
            public static Error NotFound(Guid roleId) =>
                new Error(
                    "role.not.found",
                    $"Role not found",
                    $"When trying to find the role '{roleId}', nothing came up. Please ensure that you are passing the correct ID "
                    + "or contact customer support if you are experiencing this error in the portal.",
                    HttpStatusCode.NotFound,
                    null,
                    new JObject()
                    {
                        { "roleId", roleId },
                    });

            public static Error NameInUse(string name) =>
                new Error(
                    "role.name.in.use",
                    "That role is taken",
                    $"The role name {name} is already used. Please choose another name.",
                    HttpStatusCode.Conflict);

            public static Error RoleAlreadyAssigned(string userName, string roleName) =>
                new Error(
                    "role.already.assigned",
                    "Role already assigned",
                    $"The user {userName} already has the role {roleName} assigned to them.",
                    HttpStatusCode.Conflict);

            public static Error UnassignRoleNotAssigned(string userName, string roleName) =>
                new Error(
                    "role.unassign.not.assigned",
                    "Role isn't assigned",
                    $"The user {userName} does not have the role {roleName} assigned to them, so you can't unassign it.",
                    HttpStatusCode.Conflict);

            public static Error CustomersCannotBeAdmins() =>
                new Error(
                    "role.cannot.create.customer.admins",
                    "Can't create admin role for customers",
                    "You can't create an admin role to be assigned to customers.",
                    HttpStatusCode.NotAcceptable);

            public static Error NoCustomerRolesInMasterTenant() =>
                new Error(
                    "master.tenant.cannot.create.customer.role",
                    "Can't create customer roles here",
                    "You can't create a customer role for the master tenancy as customers cannot access the master tenancy.",
                    HttpStatusCode.NotAcceptable);

            public static Error CannotUpdateDefaultRole(string roleName) =>
                new Error(
                    "cannot.update.default.role",
                    "You can't alter default roles",
                    $"You tried to make a change to the {roleName} role, however since it's a default role, you can't alter it. If you want to customise a default role, "
                    + "instead make a new role with the permissions you desire, and assign that to users.",
                    HttpStatusCode.NotAcceptable);

            public static Error CannotAssignPermissionToRoleOfType(string permissionName, string roleTypeName, string roleName) =>
                new Error(
                    "cannot.assign.permission.to.role.of.type",
                    "You can't assign that permission to that role",
                    $"You tried to assign the permission {permissionName} to the {roleName} role, however since it's a {roleTypeName} type role, you can't do that. "
                    + "You may only assign permissions that are relevant to that role type",
                    HttpStatusCode.Conflict);

            public static Error AlreadyContainsPermission(string roleName, string permissionName) =>
                new Error(
                    "role.already.contains.permission",
                    "That's already in there",
                    $"The role {roleName} already has the permission {permissionName}.",
                    HttpStatusCode.Conflict);

            public static Error RemovePermissionNotFound(string roleName, string permissionName) =>
                new Error(
                    "role.removal.permission.not.found",
                    "That's not in that role",
                    $"You are trying to remove permission {permissionName} from role {roleName}, however it's not in that role.",
                    HttpStatusCode.NotFound);

            public static Error CannotDeletePermanentRole(string roleName) =>
                new Error(
                    "cannot.delete.permanent.role",
                    "You can't delete that role",
                    $"Role {roleName} is a permanent role and it can't be deleted.",
                    HttpStatusCode.Forbidden);

            public static Error CannotDeleteRoleInUse(string roleName) =>
                new Error(
                    "cannot.delete.role.in.use",
                    "That role is being used",
                    $"The role {roleName} cannot be deleted because it's assigned to one or more users. Please un-assign this role from all users and try again.",
                    HttpStatusCode.PreconditionFailed);

            public static Error CannotAssignRoleToUser(string roleName, string userName) =>
                new Error(
                    "cannot.assign.role.to.user",
                    "You can't assign that role to that user.",
                    $"You're not allowed to assign the role \"{roleName}\" to the user \"{userName}\".",
                    HttpStatusCode.Forbidden);

            public static Error RoleIsNotAssignableToUserUnderOrganisation(string roleName, string organisationName) =>
                new Error(
                    "role.is.not.assignable.to.user",
                    "The role is not assignable to user",
                    $"You're not allowed to assign the role \"{roleName}\" for the user under organisation \"{organisationName}\".",
                    HttpStatusCode.Forbidden,
                    null,
                    new JObject
                    {
                        { "role", roleName },
                        { "organisationName", organisationName },
                    });

            public static Error CannotUnassignRole() =>
                new Error(
                    "cannot.unassign.role",
                    "Cannot un-assign role",
                    $"This role must be assigned to at least one active user. Before un-assigning this role from this user, first assign it to another active user.",
                    HttpStatusCode.PreconditionFailed);

            public static Error NameIsBlank() =>
                new Error(
                    "role.name.cannot.be.blank",
                    "You need to enter a role name",
                    $"The role name should not be blank. Please enter role name.",
                    HttpStatusCode.NotAcceptable);

            public static Error PermissionCannotBeAdded(
                string permission,
                string roleType,
                string roleName,
                string reason) => new Error(
                    "role.permission.cannot.be.added",
                    "That permission can't be added",
                    $"The permission {permission} can't be added to the {roleType} role \"{roleName}\". "
                    + (reason != null ? reason : string.Empty),
                    HttpStatusCode.BadRequest,
                    null,
                    new JObject
                    {
                        { "permission", permission },
                        { "roleType", roleType },
                        { "role", roleName },
                    });
        }
    }
}
