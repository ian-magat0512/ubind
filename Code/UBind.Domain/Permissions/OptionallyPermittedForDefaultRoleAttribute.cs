// <copyright file="OptionallyPermittedForDefaultRoleAttribute.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Permissions
{
    using System;

    /// <summary>
    /// Optional permitted role attribute for permission,
    /// it will be used for adding permission to a role if you want to.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class OptionallyPermittedForDefaultRoleAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OptionallyPermittedForDefaultRoleAttribute"/> class.
        /// </summary>
        /// <param name="role">The default role. </param>
        public OptionallyPermittedForDefaultRoleAttribute(DefaultRole role)
        {
            this.Role = role;
        }

        /// <summary>
        /// Gets a value for role tenancy.
        /// </summary>
        public DefaultRole Role { get; }
    }
}
