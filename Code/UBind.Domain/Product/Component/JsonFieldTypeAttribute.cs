// <copyright file="JsonFieldTypeAttribute.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Product.Component
{
    using System;

    /// <summary>
    /// Allows us to specify the type name to be used during deserialization
    /// to determine the class to deserialize to.
    /// </summary>
    internal class JsonFieldTypeAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JsonFieldTypeAttribute"/> class.
        /// </summary>
        /// <param name="fieldType">The field type string to match when deserializing the json.</param>
        public JsonFieldTypeAttribute(string fieldType)
        {
            this.FieldType = fieldType;
        }

        /// <summary>
        /// Gets the value of the field type specified in the attribute.
        /// </summary>
        public string FieldType { get; private set; }
    }
}
