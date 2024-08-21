// <copyright file="RoleInformationAttribute.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Permissions
{
    using System;

    /// <summary>
    /// Role Information class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class RoleInformationAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RoleInformationAttribute"/> class.
        /// </summary>
        /// <param name="name">The name of the role. </param>
        /// <param name="description">The description of the role. </param>
        /// <param name="roleType">The role type. (e.g Master, Client, Customer).</param>
        /// <param name="isFixed">Whether the role is fixed and cannot be edited.</param>
        public RoleInformationAttribute(string name, string description, RoleType roleType, bool isFixed = false)
        {
            this.Name = name;
            this.Description = description;
            this.RoleType = roleType;
            this.IsFixed = isFixed;
        }

        /// <summary>
        /// Gets a value for role name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets a value for role description.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Gets a value for role type (e.g. Master, Client or Customer).
        /// </summary>
        public RoleType RoleType { get; }

        /// <summary>
        /// Gets a value indicating whether role is fixed and cannot be edited.
        /// </summary>
        public bool IsFixed { get; }
    }
}
