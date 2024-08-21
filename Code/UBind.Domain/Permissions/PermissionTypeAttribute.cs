// <copyright file="PermissionTypeAttribute.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Permissions
{
    using System;

    /// <summary>
    /// An attribute to specify the type of permission, e.g. View, Manage.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class PermissionTypeAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PermissionTypeAttribute"/> class.
        /// </summary>
        /// <param name="permissionType">The permission type.</param>
        public PermissionTypeAttribute(PermissionType permissionType)
        {
            this.PermissionType = permissionType;
        }

        /// <summary>
        /// Gets the permission type.
        /// </summary>
        public PermissionType PermissionType { get; }
    }
}
