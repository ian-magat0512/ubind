// <copyright file="PopulateWhenEmptyAttribute.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Product.Component
{
    using System;

    /// <summary>
    /// Allows us to specify that the property should or should not be poplulated with an empty string.
    /// When objects are populated from a uBind workbook, sometimes we don't want the properties to be set
    /// with an empty string. We'd rather they were just left null. This attribute allows us to specify
    /// that.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class PopulateWhenEmptyAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PopulateWhenEmptyAttribute"/> class.
        /// </summary>
        /// <param name="populate">Whether to populate the property if the string is empty.</param>
        public PopulateWhenEmptyAttribute(bool populate)
        {
            this.Populate = populate;
        }

        /// <summary>
        /// Gets a value indicating whether the populate the property if the string is empty.
        /// </summary>
        public bool Populate { get; private set; }
    }
}
