// <copyright file="WorkbookFieldTypeAttribute.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Product.Component
{
    using System;

    /// <summary>
    /// Allows us to specify a workbook field type for a given field type class in our C# configuration model, so that
    /// the FieldFactory can know which C# class to insantiate when parsing a uBind workbook.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    internal class WorkbookFieldTypeAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WorkbookFieldTypeAttribute"/> class.
        /// </summary>
        /// <param name="fieldType">The value of the field type specified in the workbook.</param>
        public WorkbookFieldTypeAttribute(string fieldType)
        {
            this.FieldType = fieldType;
        }

        /// <summary>
        /// Gets the value of the field type specified in the workbook for a given field.
        /// </summary>
        public string FieldType { get; private set; }
    }
}
