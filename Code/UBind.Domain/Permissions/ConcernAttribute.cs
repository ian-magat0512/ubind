// <copyright file="ConcernAttribute.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Permissions
{
    using System;

    /// <summary>
    /// The entity or topic the permission is about.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class ConcernAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConcernAttribute"/> class.
        /// </summary>
        /// <param name="concern">The permission concern.</param>
        public ConcernAttribute(Concern concern)
        {
            this.Concern = concern;
        }

        /// <summary>
        /// Gets the permission concern.
        /// </summary>
        public Concern Concern { get; }
    }
}
