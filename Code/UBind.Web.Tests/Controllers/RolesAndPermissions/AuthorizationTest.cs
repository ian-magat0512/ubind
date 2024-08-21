// <copyright file="AuthorizationTest.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

// CS1591 Missing XML comment for publicly visible type or member
// Suppress CS1591. The unit test method must be named correctly instead of adding a comment in it.
#pragma warning disable CS1591

namespace UBind.Web.Tests.Controllers.RolesAndPermissions
{
    using System;
    using System.Linq;
    using System.Reflection;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using UBind.Web.Filters;
    using MustBeLoggedInAttribute = UBind.Web.Filters.MustBeLoggedInAttribute;

    public static class AuthorizationTest
    {
        public static bool IsAnonymous(Controller controller, string methodName, Type[] methodTypes)
        {
            return GetMethodAttribute<AllowAnonymousAttribute>(controller, methodName, methodTypes) != null ||
                (GetControllerAttribute<MustBeLoggedInAttribute>(controller) == null &&
                    GetMethodAttribute<MustBeLoggedInAttribute>(controller, methodName, methodTypes) == null);
        }

        public static bool IsAuthorized(Controller controller, string methodName, Type[] methodTypes)
        {
            return GetMethodAttribute<MustHaveOneOfPermissionsAttribute>(controller, methodName, methodTypes) != null ||
                GetMethodAttribute<MustBeLoggedInAttribute>(controller, methodName, methodTypes) != null ||
                (GetControllerAttribute<MustBeLoggedInAttribute>(controller) != null &&
                    GetMethodAttribute<AllowAnonymousAttribute>(controller, methodName, methodTypes) == null);
        }

        public static bool IsAuthorized(Controller controller, string methodName, Type[] parameterTypes, string[] permissions, string[] users)
        {
            bool isAuthorize = false;
            if (permissions == null && users == null)
            {
                return IsAuthorized(controller, methodName, parameterTypes);
            }

            if (!IsAuthorized(controller, methodName, parameterTypes))
            {
                return false;
            }

            MustBeLoggedInAttribute controllerAttribute = GetControllerAttribute<MustBeLoggedInAttribute>(controller);
            MustBeLoggedInAttribute methodAttribute = GetMethodAttribute<MustBeLoggedInAttribute>(controller, methodName, parameterTypes);
            MustHavePermissionAttribute permissionMethodAttribute = GetMethodAttribute<MustHavePermissionAttribute>(controller, methodName, parameterTypes);
            MustHaveOneOfPermissionsAttribute multiPermissionMethodAttribute = GetMethodAttribute<MustHaveOneOfPermissionsAttribute>(controller, methodName, parameterTypes);

            // there is authorization but no permission attribute and your expecting no permission
            if ((controllerAttribute != null || methodAttribute != null)
                && permissions == null
                && permissionMethodAttribute == null)
            {
                return true;
            }

            if (permissions != null)
            {
                foreach (string permission in permissions)
                {
                    bool permissionTypeIsAuthorized
                        = permissionMethodAttribute != null && permissionMethodAttribute.Permission.ToString() == permission;

                    if (permissionTypeIsAuthorized)
                    {
                        isAuthorize = true;
                    }

                    var multiPermissionTypeIsAuthorized = multiPermissionMethodAttribute != null
                        && multiPermissionMethodAttribute.Permissions.Any(x => x.ToString() == permission.ToString());

                    if (multiPermissionTypeIsAuthorized)
                    {
                        isAuthorize = true;
                    }
                }
            }

            return isAuthorize;
        }

        public static bool IsAuthorizedOneOfPermissions(Controller controller, string methodName, Type[] parameterTypes, string[] permissions, string[] users)
        {
            bool isAuthorize = false;
            if (permissions == null && users == null)
            {
                return IsAuthorized(controller, methodName, parameterTypes);
            }

            if (!IsAuthorized(controller, methodName, parameterTypes))
            {
                return false;
            }

            MustBeLoggedInAttribute controllerAttribute = GetControllerAttribute<MustBeLoggedInAttribute>(controller);
            MustBeLoggedInAttribute methodAttribute = GetMethodAttribute<MustBeLoggedInAttribute>(controller, methodName, parameterTypes);
            MustHaveOneOfPermissionsAttribute permissionMethodAttribute = GetMethodAttribute<MustHaveOneOfPermissionsAttribute>(controller, methodName, parameterTypes);

            // there is authorization but no permission attribute and your expecting no permission
            if ((controllerAttribute != null || methodAttribute != null)
                && permissions == null
                && permissionMethodAttribute == null)
            {
                return true;
            }

            if (permissions != null)
            {
                foreach (string permission in permissions)
                {
                    bool permissionTypeIsAuthorized =
                        permissionMethodAttribute != null ?
                            permissionMethodAttribute.Permissions.Any(r => r.ToString() == permission) : false;

                    if (permissionTypeIsAuthorized)
                    {
                        isAuthorize = true;
                    }
                }
            }

            return isAuthorize;
        }

        private static T GetControllerAttribute<T>(Controller controller)
            where T : Attribute
        {
            Type type = controller.GetType();
            object[] attributes = type.GetCustomAttributes(typeof(T), true);
            T attribute = attributes.Length == 0 ? null : (T)attributes[0];
            return attribute;
        }

        private static T GetMethodAttribute<T>(Controller controller, string methodName, Type[] methodTypes)
            where T : Attribute
        {
            Type type = controller.GetType();
            if (methodTypes == null)
            {
                methodTypes = Array.Empty<Type>();
            }

            MethodInfo method = type.GetMethod(methodName, methodTypes);
            object[] attributes = method.GetCustomAttributes(typeof(T), true);
            T attribute = attributes.Length == 0 ? null : (T)attributes[0];
            return attribute;
        }
    }
}
