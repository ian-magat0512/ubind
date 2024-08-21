// <copyright file="IAttributeObjectPropertyMapRegistry.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Attributes
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    /// <summary>
    /// Maps table attributes to object properties.
    /// This is used when parsing a workbook and populating an object with values from a column in the workbook.
    /// The objects are told which column maps to which property by using the WorkbookColumnNameAttribute on the
    /// C# property. This class creates a map on startup and holds it so that setting a property value is fast
    /// during parsing.
    /// </summary>
    /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
    public interface IAttributeObjectPropertyMapRegistry<TAttribute>
    {
        /// <summary>
        /// Gets a map of workbook column name to MethodInfo, so that C# Properties can be
        /// populated with values from the workbook column.
        /// </summary>
        /// <param name="type">The object type that has properties with the WorkbookColumnNameAttribute.</param>
        /// <returns>A dictionary which maps column names to Properties.</returns>
        Dictionary<TAttribute, PropertyInfo> GetAttributeToPropertyMap(Type type);
    }
}
