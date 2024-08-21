// <copyright file="WorkbookTableSectionPropertyNameAttribute.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Product.Component
{
    using System;

    /// <summary>
    /// Allows us to specify a workbook property name in a table, to fulfil the the class property with
    /// when parsing a uBind workbook.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class WorkbookTableSectionPropertyNameAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WorkbookTableSectionPropertyNameAttribute"/> class.
        /// </summary>
        /// <param name="majorHeader">The major heading that the minor heading is found under.</param>
        /// <param name="minorHeader">The minor heading that the property is found under (optional).</param>
        /// <param name="propertyName">The property name.</param>
        public WorkbookTableSectionPropertyNameAttribute(string majorHeader, string minorHeader, string propertyName)
        {
            this.MajorHeader = majorHeader;
            this.MinorHeader = minorHeader;
            this.PropertyName = propertyName;
        }

        /// <summary>
        /// Gets the major heading that the minor heading is found under.
        /// </summary>
        public string MajorHeader { get; private set; }

        /// <summary>
        /// Gets the minor heading that the property is found under (optional).
        /// </summary>
        public string MinorHeader { get; private set; }

        /// <summary>
        /// Gets the property name.
        /// </summary>
        public string PropertyName { get; private set; }
    }
}
