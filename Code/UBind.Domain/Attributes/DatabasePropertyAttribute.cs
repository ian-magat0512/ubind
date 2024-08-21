// <copyright file="DatabasePropertyAttribute.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Attributes
{
    using System;

    /// <summary>
    /// Attribute for identifying alternative property for use in database queries.
    /// </summary>
    /// <remarks>
    /// When a property exposed by an entity accessible to automations does not map directly
    /// to a database column, use this attribute on the property to indicate which other property
    /// should be used in the query instead.
    /// E.g. Mark a property CreatedDateTime of type <see cref="NodaTime.Instant"/> with an attribute
    /// specifying the name of the coresponding CreatedTimeInUnixTicks of type <see cref="long"/>.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class DatabasePropertyAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DatabasePropertyAttribute"/> class.
        /// </summary>
        /// <param name="propertyName">The name of the property to use in database queries.</param>
        public DatabasePropertyAttribute(string propertyName) =>
            this.PropertyName = propertyName;

        /// <summary>
        /// Gets the name of the property to use instead of the marked property in database queries.
        /// </summary>
        public string PropertyName { get; }
    }
}
