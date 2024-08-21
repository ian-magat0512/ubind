// <copyright file="Authorisation.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using System.Net;
    using Humanizer;
    using UBind.Domain.Permissions;

    public static partial class Errors
    {
        public static class Authorisation
        {
            public static Error PermissionRequiredToViewResource(
                Permission[] oneOfThePermissions,
                string resourceTypeName,
                string resourceIdentifier = null)
            {
                string permissionList = string.Join(", ", oneOfThePermissions.Select(p => p.Humanize()));
                return new Error(
                    $"authorisation.permission.required.to.view.{resourceTypeName}",
                    $"You don't have permission to view that {resourceTypeName}",
                    $"In order to view the {resourceTypeName}"
                    + $"{(string.IsNullOrEmpty(resourceIdentifier) ? string.Empty : $" \"{resourceIdentifier}\"")}"
                    + $", you need {(oneOfThePermissions.Length > 1 ? "one of the permissions" : "the permission")}"
                    + $"\"{permissionList}\""
                    + "If you feel you should have access, please get in touch with an administrator to request a "
                    + "higher level of access.",
                    HttpStatusCode.Forbidden);
            }

            public static Error CannotViewResourceFromThatOrganisation(
                string resourceTypeName) =>
                new Error(
                    $"authorisation.cannot.view.{resourceTypeName}.from.that.organisation",
                    $"You can't view that {resourceTypeName} from that organisation",
                    $"When attempting to view the {resourceTypeName}, it was found to be from another organisation "
                    + $"which you don't manage.",
                    HttpStatusCode.Forbidden);

            public static Error PermissionRequiredToCreateResource(
                Permission[] oneOfThePermissions,
                string resourceTypeName)
            {
                string permissionList = string.Join(", ", oneOfThePermissions.Select(p => p.Humanize()));
                return new Error(
                    $"authorisation.permission.required.to.create.{resourceTypeName}",
                    $"You don't have permission to create a {resourceTypeName}",
                    $"In order to create a {resourceTypeName}"
                    + $", you need {(oneOfThePermissions.Length > 1 ? "one of the permissions" : "the permission")}"
                    + $"\"{permissionList}\""
                    + "If you feel you should have access, please get in touch with an administrator to request a "
                    + "higher level of access.",
                    HttpStatusCode.Forbidden);
            }

            public static Error CannotCreateResourceInThatOrganisation(
                string resourceTypeName) =>
                new Error(
                    $"authorisation.cannot.create.{resourceTypeName}.in.that.organisation",
                    $"You can't create a {resourceTypeName} in that organisation",
                    $"You're not allowed to create a {resourceTypeName} in that organisation, "
                    + $"because you don't manage that organisation.",
                    HttpStatusCode.Forbidden);

            public static Error PermissionRequiredToModifyResource(
                Permission[] oneOfThePermissions,
                string resourceTypeName)
            {
                string permissionList = string.Join(", ", oneOfThePermissions.Select(p => p.Humanize()));
                return new Error(
                    $"authorisation.permission.required.to.modify.{resourceTypeName}",
                    $"You don't have permission to modify that {resourceTypeName}",
                    $"In order to create a {resourceTypeName}"
                    + $", you need {(oneOfThePermissions.Length > 1 ? "one of the permissions" : "the permission")}"
                    + $"\"{permissionList}\""
                    + "If you feel you should have access, please get in touch with an administrator to request a "
                    + "higher level of access.",
                    HttpStatusCode.Forbidden);
            }

            public static Error CannotModifyResourceFromThatOrganisation(
                string resourceTypeName) =>
                new Error(
                    $"authorisation.cannot.modify.{resourceTypeName}.in.that.organisation",
                    $"You can't modify that {resourceTypeName} in that organisation",
                    $"You're not allowed to modify that {resourceTypeName} in that organisation, "
                    + $"because you don't manage that organisation.",
                    HttpStatusCode.Forbidden);

            public static Error PermissionRequiredToDeleteResource(
                Permission[] oneOfThePermissions,
                string resourceTypeName)
            {
                string permissionList = string.Join(", ", oneOfThePermissions.Select(p => p.Humanize()));
                return new Error(
                    $"authorisation.permission.required.to.delete.{resourceTypeName}",
                    $"You don't have permission to delete that {resourceTypeName}",
                    $"In order to delete a {resourceTypeName}"
                    + $", you need {(oneOfThePermissions.Length > 1 ? "one of the permissions" : "the permission")}"
                    + $"\"{permissionList}\""
                    + "If you feel you should have access, please get in touch with an administrator to request a "
                    + "higher level of access.",
                    HttpStatusCode.Forbidden);
            }

            public static Error CannotDeleteResourceFromThatOrganisation(
                string resourceTypeName) =>
                new Error(
                    $"authorisation.cannot.delete.{resourceTypeName}.in.that.organisation",
                    $"You can't delete that {resourceTypeName} in that organisation",
                    $"You're not allowed to delete that {resourceTypeName} in that organisation, "
                    + $"because you don't manage that organisation.",
                    HttpStatusCode.Forbidden);

            public static Error CannotDeleteOwnOrganisation() =>
                new Error(
                    $"authorisation.cannot.delete.own.organisation",
                    $"You can't delete your own organisation",
                    $"You're not allowed to delete your own organisation, "
                    + $"because that would really mess things up.",
                    HttpStatusCode.Forbidden);

            public static Error CannotDeleteOwnUserAccount() =>
                new Error(
                    $"authorisation.cannot.delete.own.user.account",
                    $"You can't delete your own user account",
                    $"You're not allowed to delete your own user account. "
                    + "If you wish to close your account please have an administrator do that for you, "
                    + "or get in touch with customer support.",
                    HttpStatusCode.Forbidden);
        }
    }
}
