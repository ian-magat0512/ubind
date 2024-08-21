// <copyright file="OrderAttribute.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using System;

    /// <summary>
    /// This class is needed because we need to attach ordering for enumeration.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class OrderAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RelationshipTypeInformationAttribute"/> class.
        /// </summary>
        /// <param name="order">The ordering of the enum.</param>
        public OrderAttribute(int order)
        {
            this.Order = order;
        }

        /// <summary>
        /// Gets a value of the sort order.
        /// </summary>
        public int Order { get; }
    }
}
