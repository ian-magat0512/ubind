// <copyright file="QueryOrderMethodAttribute.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Attributes
{
    using System;

    /// <summary>
    /// This class is needed because we need to attach query order information to properties that require sort options.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class QueryOrderMethodAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QueryOrderMethodAttribute"/> class.
        /// </summary>
        /// <param name="orderMethod">The order in query order method.</param>
        public QueryOrderMethodAttribute(string orderMethod)
        {
            this.OrderMethod = orderMethod;
        }

        /// <summary>
        /// Gets a value of the order in query method.
        /// </summary>
        public string OrderMethod { get; }
    }
}
